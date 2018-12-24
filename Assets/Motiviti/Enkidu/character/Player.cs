using UnityEngine;
using System;
using System.Collections;
using PigeonCoopToolkit.Navmesh2D;
using UnityEngine.SceneManagement;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class Player : AnimatedCharacter
    {
        public delegate void MovedEventHandler(object sender, EventArgs e);
        public event MovedEventHandler Moved;
        public CutsceneTools cutsceneTools;
        public Hashtable doors;
        public Inventory inventory;
        public ClickIndicator clickIndicator;

        public bool inCutScene = false;
        public bool inConversation = false;

        float lastTimeChangedDirection = 0;
        int voicesWontWorkIndex = 0;
        float setDestinationTimeStamp = 0;
        public float direction = 1;
        float scale = 1;
        bool overrideAutoPilotScale = false;
        float overridedAutoPilotScale = 1;
        Vector3 autoPilotStartedScale = Vector3.zero;
        float autoPilotStartedTime = 0;
        Vector3 autoPilotUseScaleVector = Vector3.one;
        Vector3 startHoldPosition;
        float timeDelayBeforePanning = 0.2f;
        public bool justReleasedAfterPeekPanning = false;
        int procesingInventory = 0;
        bool isInUninterruptibleAction = false;

        IEnumerator Start()
        {
            base.Initialise();

            scene = Global.scene;
            flipYCoordinate = scene.flipYCoordinate;
            if (scene && scene.startWithStaticCharacter) staticCharacter = true;
            animator = transformAdv.GetComponent<Animator>();
            inventory = Global.inventory;
            audioManager = Global.audioManager;
            advCamera = Global.advCamera;

            if (!this.isOnlyAVoice)
            {
                navmeshWalker = GetComponent<NavmeshWalker>();

                doors = new Hashtable();

                yield return StartCoroutine(InitialisePlayerArrival());
            }

            yield return null;
        }

        void Update()
        {
            if (isOnlyAVoice) return;

            if (staticCharacter)
            {
                navmeshWalker.Stop();
            }

            if (!inCutScene && !Global.inPause)
            {
                if (state != lastState)
                {
                    ChangeState(state);
                }

                ProcessDirectionAndScale();
                StartCoroutine(ProcessInteraction());
            }
            else if (isInAutopilot)
            {
                transform.position += autoPilotDelta * Time.deltaTime * autoPilotSpeed;//Vector3.Lerp (transform.position, autoPilotTarget, Time.deltaTime);
                ProcessScale();
            }
        }

        IEnumerator InitialisePlayerArrival()
        {
            string arrivalDoor = Global.GetArrivalDoor();

            foreach (DoorBetweenLevels door in GameObject.FindObjectsOfType<DoorBetweenLevels>())
            {
                doors[door.levelPath] = door;
            }

            if (!string.IsNullOrEmpty(arrivalDoor) && !staticCharacter)
            {
                yield return StartCoroutine(EnterRoom(arrivalDoor));
                yield return null;
            }
            else
            {
                if (advCamera != null && advCamera.followElroy)
                    advCamera.transform.position = transform.position + Vector3.up * advCamera.yFollowOffset;

                if (!staticCharacter)
                {
                    ChangeState(State.IdleDiagonalFront);
                }
            }

            yield return null;
        }

        float GetSpeedModifierBasedOnYPosition()
        {
            float smax = Global.scene.scaleMax;
            float smin = Global.scene.scaleMin;

            float scale = transform.localScale.y;

            float ret = (scale - smin) / (smax - smin);

            ret += 0.47f;

            if (ret > 1) ret = 1;

            if (ret < 0.7f) ret = 0.7f;

            return ret;
        }

        public float TimeSinceSetDestination()
        {
            return Time.time - setDestinationTimeStamp;
        }

        public void AnimationEventEndCutScene()
        {
            if (scene.initialStateOneTime && !scene.initialStateFinished)
            {
                ChangeState(State.IdleDiagonalFront);
                StartCoroutine(ExitCutscene());
            }
        }

        public float GetXVelocity()
        {
            return navmeshWalker.GetVelocity().x;
        }

        IEnumerator ExitCutscene()
        {
            ChangeState(State.IdleDiagonalFront);
            ProcessScale();

            if (string.IsNullOrEmpty(scene.sentenceAfterInitialState)) SetInCutScene(false);

            if (!string.IsNullOrEmpty(scene.sentenceAfterInitialState) && !scene.initialStateFinished)
            {
                talkDirection = TalkDirection.Diagonal;

                SetInCutScene(true);

                yield return null;//yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(SpeakProcedure(scene.sentenceAfterInitialState));

                ChangeState(State.IdleDiagonalFront);

                if (!scene.stayInCutsceneAfterInitialState)
                    SetInCutScene(false);
            }

            scene.initialStateFinished = true;

            scene.SaveState();
            SaveState();

            yield return null;
        }

        public void TeleportToPosition(Vector3 pos)
        {
            if (!IsIdleOrWalking())
            {
                ChangeState(State.IdleDiagonalFront);
            }
            if (targetItem != null && !targetItem.IsInInventory() && (advCamera.IsVisible(targetItem.transform) || !IsIdleOrWalking()))
            {
                SetDestination(targetItem.transform.position + targetItem.GetStoppingDistance(heldItem));
            }
            else
            {
                targetItem = null;

            }
        }

        public IEnumerator SetDestinationInterim(Vector3 worldPosition)
        {
            if (!staticCharacter)
            {
                if (navmeshWalker.GetVelocity().sqrMagnitude > 0)  //mr if player is already walking we shouldn't change his destination 
                {
                    // TODO
                }
                else if (IsIdleOrWalking())
                {
                    var ti = targetItem;

                    if (targetItem != null)
                    {
                        targetItem = null;
                    }

                    SetDestination(worldPosition);
                    yield return new WaitForSeconds(0.2f);

                    if (ti != null && !ti.IsInInventory() && advCamera.IsVisible(ti.transform))
                    {
                        SetDestination(ti.transform.position + ti.GetStoppingDistance(heldItem));
                        targetItem = ti;
                    }

                    justReleasedAfterPeekPanning = false;
                }
            }
            yield return null;
        }

        public void HoldItem(InventoryItem item)
        {
            heldItem = item;
        }

        public void SetInCutScene(bool cs, CutsceneTools.Type type = CutsceneTools.Type.BlackBands, Vector3 pos = default(Vector3), bool startOn = false, float minimumuCircleSize = 0.01f, bool hideInventory = true)
        {
            if (Global.inventory && (!cs || hideInventory)) Global.inventory.SetEnabled(!cs);

            inCutScene = cs;
            if (animator != null)
            {
                if (!staticCharacter)
                    if (cutsceneTools)
                        cutsceneTools.SetCutscene(cs ? type : CutsceneTools.Type.None, pos, startOn, minimumuCircleSize);
            }
        }

        bool IsTalking(State st)
        {
            return st == State.TalkDiagonal || state == State.TalkFront || state == State.TalkDiagonalBack;
        }

        public void SayThatWontWork()
        {
            int wontWorkCount = scene.voicesWontWork.Length;

            if (wontWorkCount < 1) return;

            Speak(scene.voicesWontWork[voicesWontWorkIndex], TalkMode.ThinkingHead);

            voicesWontWorkIndex++;
            if (voicesWontWorkIndex > scene.voicesWontWork.Length - 1) voicesWontWorkIndex = 0;
        }

        public void SayItsLocked(string lockedComment)
        {
            Speak(lockedComment, TalkMode.ThinkingChin);
        }

        public void ChangeDirection(int x)
        {
            direction = x < 0 ? -1 : 1;
            ProcessScale();
        }

        public void Speak(string clipName, TalkMode mode = TalkMode.Default)
        {
            StartCoroutine(SpeakProcedure(clipName, mode));
        }

        void SwitchToTalking()
        {
            animator.SetBool("talking", true);

            PlayerBrain.SetEyeGesture(CharacterBrain.EyeGestures.talk);
        }

        void SwitchToIdle()
        {
            PlayerBrain.SetEyeGesture(CharacterBrain.EyeGestures.idle);
        }

        void ChangeStateToIdleBasedOnDirection()
        {
            Direction dir = GetDirection();

            switch (dir)
            {
                case Direction.FRONT:
                    ChangeState(State.IdleFront);
                    break;
                case Direction.BACK:
                    ChangeState(State.IdleBack);
                    break;
                case Direction.SIDE:
                    ChangeState(State.IdleSide);
                    break;
                case Direction.DIAGONAL_FRONT:
                    ChangeState(State.IdleDiagonalFront);
                    break;
            }
        }

        void ChangeStateToIdleBasedOnWalkAnimation(State wi)
        {
            switch (wi)
            {
                case State.WalkFront:
                    ChangeState(State.IdleFront);
                    break;

                case State.WalkBack:
                    ChangeState(State.IdleBack);
                    break;

                case State.WalkSide:
                    ChangeState(State.IdleSide);
                    break;

                default:
                    ChangeState(State.IdleFront);
                    break;
            }
        }

        public void StopTalking()
        {
            conversationPartner = null;
            animator.SetBool("talking", false);
            interruptingSentence = true;
            PlayerBrain.StopTalking();
        }

        public void StopWalking()
        {
            if (navmeshWalker != null) navmeshWalker.Stop();
        }

        public IEnumerator SpeakProcedure(string clipName, TalkMode mode = TalkMode.Default, bool remainInConversation = false)
        {
            StopWalking();
            PlayerBrain.interruptFlag = false;
            interruptingSentence = false;
            if (!staticCharacter)
            {
                SwitchToTalking();
                animator.SetInteger("talkMode", (int)mode);
            }

            animator.SetBool("talking", true);
            yield return StartCoroutine(PlayerBrain.Talk(clipName));

            if (!interruptingSentence)
            {
                if (!remainInConversation)
                {
                    if (!staticCharacter) SwitchToIdle();
                }
            }

            animator.SetBool("talking", false);
        }

        void ProcessInput(ref bool isHit, ref bool holding, ref Vector3 holdPosition, ref Vector3 worldPosition, ref InteractiveItem interactiveItem, ref bool justTouched)
        {
            justTouched = false;

            foreach (Touch touch in Input.touches)
            {
                holding = true;
                if (touch.phase == TouchPhase.Began)
                {
                    justTouched = true;
                    holdPosition = touch.position;

                    startHoldPosition = holdPosition;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    holding = false;
                    holdPosition = touch.position;
                    isHit = true;
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                holding = false;
                holdPosition = Input.mousePosition;
                isHit = true;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                justTouched = true;
                startHoldPosition = Input.mousePosition;
            }

            if (Input.GetButton("Fire1"))
            {
                holding = true;
                holdPosition = Input.mousePosition;
            }

            worldPosition = Global.activeCamera.ScreenToWorldPoint(holdPosition);

            if (isHit)
            {
                Vector2 v = Global.activeCamera.ScreenToWorldPoint(holdPosition);

                Collider2D[] col = Physics2D.OverlapPointAll(v);

                interactiveItem = null;

                if (col.Length > 0)
                {
                    int lastLayer = int.MinValue;

                    foreach (Collider2D c in col)
                    {
                        if (c.gameObject.layer == 10)
                        {
                            var elroyItem1 = c.gameObject.GetComponent<InteractiveItem>();

                            if (elroyItem1 != null && elroyItem1.enabled && elroyItem1.itemLayer > lastLayer)
                            {

                                lastLayer = elroyItem1.itemLayer;

                                interactiveItem = elroyItem1;

                                Vector3 posx = interactiveItem.transform.position + interactiveItem.GetCenterOffset(heldItem);// interactiveItem.collider2D.bounds.center;// + interactiveItem.transform.position;

                                worldPosition = posx + interactiveItem.GetStoppingDistance(heldItem);

                            }
                        }
                    }
                }
            }
        }

        IEnumerator ProcessInteraction()
        {
            // TODO: refactor and clean up this method. Bad variable names, too tightly coupled with AdvCamera and Inventory

            bool justLooking = false;
            bool clicked1 = false;
            bool isHolding1 = false;
            Vector3 holdPosition1 = Vector3.zero;
            Vector3 worldPosition1 = Vector3.zero;
            InventoryItem item1 = null;
            bool inventoryClick = false;
            bool clicked = false;
            bool isHolding = false;
            Vector3 holdPosition = Vector3.zero;
            Vector3 worldPosition = Vector3.zero;
            InteractiveItem interactiveItem = null;
            bool justTouched = false;

            if (!isInUninterruptibleAction)
            {
                if (inventory.ProcessInput(ref clicked1, ref isHolding1, ref holdPosition1, ref worldPosition1, ref item1))
                {
                    procesingInventory = 1;
                    if (clicked1)
                    {
                        procesingInventory = 0;
                        inventoryClick = true;
                    }
                    if (item1 != null || heldItem != null)
                        procesingInventory = 0;
                }
            }

            if (inventory.MouseOverInventory())
            {
                procesingInventory = 1;
            }

            if (!isInUninterruptibleAction)
                ProcessInput(ref clicked, ref isHolding, ref holdPosition, ref worldPosition, ref interactiveItem, ref justTouched);
            else
            {
                if (navmeshWalker.GetVelocity().sqrMagnitude == 0)
                    isInUninterruptibleAction = false;
            }

            bool callElroyMoved = false;

            if (!wasHolding && isHolding)
            {
                holdingSince = Time.realtimeSinceStartup;
            }

            wasHolding = isHolding;

            if (isHolding1)
            {
                inventory.inventoryItemPosition = worldPosition1;

                if (inventory.heldItem == null && !advCamera.IsPeekPanning())
                {
                    if (item1 != null) inventory.HoldItem(item1);
                }

                if (item1 == null && heldItem == null)
                {
                    if (Time.realtimeSinceStartup - holdingSince > timeDelayBeforePanning)
                    {
                        if (!advCamera.IsPeekPanning() && procesingInventory <= 0)
                        {
                            startHoldPosition = holdPosition;
                            advCamera.SetPeekPanning(true);
                        }
                        else if (Input.touches.Length < 2)//added because of two finger jumping screen
                        {
                            Vector3 delta = Global.activeCamera.ScreenToWorldPoint(holdPosition) - Global.activeCamera.ScreenToWorldPoint(startHoldPosition);

                            advCamera.AdjustPeekPanning(-delta);

                        }
                    }
                }
            }
            else
            {
                ReturnItemToInventory();
            }

            if (clicked && !inCutScene && !Global.inPause)
            {
                if (!advCamera.WasPeekingLongEnough())
                {
                    clickIndicator.transform.position = worldPosition;

                    if (interactiveItem != null)
                    {
                        justLooking = ProcessItemInteraction(justLooking, inventoryClick, holdPosition, interactiveItem);

                    }
                    else if (!inventoryClick)
                    {
                        clickIndicator.isFlipped = false;
                        clickIndicator.ChangeState(ClickIndicator.State.Walk);
                    }

                    if (!isHolding)
                    {
                        combiningItem = heldItem;
                        heldItem = null;
                    }

                    if (IsTalking(state))
                    {
                        interruptingSentence = true;
                        StopTalking();
                        SwitchToIdle();
                    }

                    if (IsIdleOrWalking())
                    {
                        if (!justLooking) targetItem = interactiveItem;

                        var iho = Global.inventory.ItemHoveringOver();
                        if (iho != null /* TG && targetItem == null */)
                        {
                            targetItem = iho.GetComponent<InteractiveItem>();
                        }

                        if (targetItem != null && combiningItem != null && targetItem.IsInInventory() && targetItem != combiningItem)
                        {
                            // Already there

                            ArrivedAt(combiningItem);
                        }

                        if (targetItem != null && targetItem.IsInInventory())
                        {
                            navmeshWalker.Stop();
                        }
                        else if (!inventoryClick)
                        {
                            if (!justLooking && !staticCharacter && (procesingInventory <= 0)) worldPosition = SetDestination(worldPosition);

                            callElroyMoved = true;

                            bool x = CheckIfCombiningMatchingItems(targetItem, combiningItem);

                            if (x) isInUninterruptibleAction = x;

                            if (targetItem != null && (navmeshWalker.HasArrived() || staticCharacter)/*0.06f*/)
                            {
                                advCamera.SetPeekPanning(false);
                                yield return null;

                                navmeshWalker.Stop();

                                yield return null;
                                ArrivedAt(combiningItem);
                            }
                        }
                    }
                }

                advCamera.SetPeekPanning(false);
            }
            else if (clicked)
            {
                procesingInventory--;
                advCamera.SetPeekPanning(false);
            }

            if (callElroyMoved && !justLooking) advCamera.ElroyMoved();

            if (!inventory.MouseOverInventory())
            {
                procesingInventory = 0;
            }

            yield return null;
        }

        private bool ProcessItemInteraction(bool justLooking, bool inventoryClick, Vector3 holdPosition, InteractiveItem interactiveItem)
        {
            if (interactiveItem.IsInInventory() == false && !inventoryClick)
            {
                if (interactiveItem.SupportsDoubleClick() && interactiveItem.DoubleClickTime() < 0.75f && !isInAutopilot && !inCutScene)
                {
                    interactiveItem.DoubleClick();
                }

                if (interactiveItem.isClickIndicatorFlipped)
                {
                    clickIndicator.isFlipped = true;
                }
                else
                {
                    clickIndicator.isFlipped = false;
                }

                if (interactiveItem.IsPerson() && heldItem == null)
                {
                    clickIndicator.ChangeState(ClickIndicator.State.Talk);
                }
                else
                if (interactiveItem.IsExit())
                {
                    clickIndicator.ChangeState(ClickIndicator.State.Exit);
                }
                else
                if (interactiveItem.CanBeUsed())
                {
                    clickIndicator.ChangeState(ClickIndicator.State.Use);
                }
                else
                if (interactiveItem.CanBeCombinedWith.Length > 0 && heldItem != null)
                {
                    clickIndicator.ChangeState(ClickIndicator.State.Use);
                }
                else
                if (heldItem != null)
                {
                    clickIndicator.ChangeState(ClickIndicator.State.Use);
                }
                else
                {
                    clickIndicator.ChangeState(ClickIndicator.State.Look);

                    if (!interactiveItem.needToWalkToItem && !isInAutopilot && !inCutScene)
                    {
                        targetItem = interactiveItem;
                        navmeshWalker.Stop();
                        talkDirection = TalkDirection.Front;

                        ProcessDirectionAndScale(false);

                        justLooking = true;

                        if (IsTalking(state))
                        {
                            talkDirection = TalkDirection.Front;
                            interruptingSentence = true;
                            StopTalking();
                        }

                        talkDirection = TalkDirection.Front;
                        ArrivedAt(heldItem);

                        targetItem = null;
                    }

                }
            }

            interactiveItem.Selected();

            clickIndicator.transform.position = advCamera.GetComponent<Camera>().ScreenToWorldPoint(holdPosition);
            return justLooking;
        }

        public void ProcessInventoryInteraction()
        {
            Vector3 holdPosition = Vector3.zero;
            bool justTouched = false;
            bool clicked1 = false;
            bool isHolding1 = false;
            Vector3 holdPosition1 = Vector3.zero;
            Vector3 worldPosition1 = Vector3.zero;
            InventoryItem item1 = null;

            inventory.ProcessInput(ref clicked1, ref isHolding1, ref holdPosition1, ref worldPosition1, ref item1);

            if (isHolding1)
            {
                inventory.inventoryItemPosition = worldPosition1;

                if (inventory.heldItem == null && !advCamera.IsPeekPanning())
                {
                    if (item1 != null) inventory.HoldItem(item1);
                }

                if (item1 == null)
                {
                    if (justTouched)
                        advCamera.SetPeekPanning(true);
                    else if (Input.touches.Length < 2) //added because of two finger jumping screen
                    {
                        Vector3 delta = advCamera.GetComponent<Camera>().ScreenToWorldPoint(holdPosition) - advCamera.GetComponent<Camera>().ScreenToWorldPoint(startHoldPosition);

                        advCamera.AdjustPeekPanning(-delta);
                    }

                }
            }
            else
            {
                ReturnItemToInventory();
            }
        }

        public void ReturnItemToInventory()
        {
            if (inventory.heldItem != null)
            {
                if (Time.realtimeSinceStartup - holdingSince < 0.2f)
                {
                    inventory.heldItem.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                }
                inventory.ReturnItem(inventory.heldItem);
                inventory.HoldItem(null);
                inventory.heldItem = null;
            }
        }


        public void AnimateGiving()
        {
            StartCoroutine(AnimateGivingProc());
        }

        IEnumerator AnimateGivingProc()
        {
            var s = state;
            ChangeState(State.SwitchOnOffSide);
            yield return new WaitForSeconds(0.8f);
            ChangeState(s);
        }

        public bool CheckIfCombiningMatchingItems(InteractiveItem targetItem, InventoryItem combiningItem)
        {
            if (targetItem == null)
                return false;

            bool x = targetItem.CheckIfCombiningMatchingItems(combiningItem);

            return x;
        }

        public Vector3 SetDestination(Vector3 worldPosition)
        {
            if (staticCharacter)
            {

            }
            else
            {
                if (inventory) inventory.SetVisible(false);
                navmeshWalker.SetTarget(worldPosition);
                setDestinationTimeStamp = Time.time;

                if (Moved != null) Moved(this, null);
            }

            return worldPosition;
        }

        public IEnumerator EnterRoom(string levelIndex)
        {
            DoorBetweenLevels door = (DoorBetweenLevels)doors[levelIndex];
            InteractiveItemChangeCamera doorCam = null;

            if (door == null)
            {
                SetInCutScene(true, CutsceneTools.Type.ZoomIn, transform.position + Vector3.up * advCamera.yFollowOffset, false);

                if (!staticCharacter)
                {
                    ChangeStateToIdleBasedOnDirection();
                }

                if (!scene.stayInCutsceneAfterInitialState)
                    SetInCutScene(false);
            }
            else
            {
                autoPilotSpeed = door.walkInOutSpeed;
                transform.position = door.gameObject.transform.position + door.outsideOffset;

                ProcessDirectionAndScale();
                doorCam = door.gameObject.GetComponent<InteractiveItemChangeCamera>();

                if (doorCam)
                    StartCoroutine(doorCam.ProcessArrivedAt());

                heldItem = combiningItem = null;
                targetItem = null;

                Vector3 cameraPos = transform.position + Vector3.up * advCamera.yFollowOffset;

                cameraPos.z = -5;

                advCamera.Move(new Vector3(cameraPos.x, advCamera.transform.position.y, cameraPos.z), true);

                if (door.zoomoutCircleTransformPosition != null)
                    SetInCutScene(true, CutsceneTools.Type.ZoomIn, door.zoomoutCircleTransformPosition.position, false, 0.01f, false);
                else
                    SetInCutScene(true, CutsceneTools.Type.ZoomIn, transform.position + Vector3.up * advCamera.yFollowOffset, false, 0.01f, false);

                if (door.openCloseAnimator)
                {
                    transform.position = door.gameObject.transform.position + door.outsideOffset;
                    door.openCloseAnimator.SetBool("open", true);
                    ChangeStateToIdleBasedOnWalkAnimation(door.animationWalkIn);
                    yield return new WaitForSeconds(0.7f);
                }

                if (!staticCharacter)
                    ChangeState(door.animationWalkIn);   //mr 18.11.2014 to enable walk in front in 

                autoPilotTarget = door.insideOffset + door.gameObject.transform.position;

                autoPilotDelta = (door.insideOffset - door.outsideOffset);

                isInAutopilot = true;

                direction = transform.position.x < (autoPilotTarget.x) ? 1 : -1;

                ProcessDirectionAndScale(false);
                yield return new WaitForSeconds(1 / autoPilotSpeed);

                isInAutopilot = false;
                if (!staticCharacter)
                    ChangeStateToIdleBasedOnDirection();

                InteractiveItem interactiveItemDoor = door.GetComponent<InteractiveItem>();
                if (interactiveItemDoor == null || interactiveItemDoor.realyStayInCutscene != true)
                    SetInCutScene(false);

                advCamera.CenterOnElroy();

                if (door.openCloseAnimator)
                {
                    door.openCloseAnimator.SetBool("open", false);
                }
            }
        }

        public IEnumerator NextLevel(Vector3 offsetCircle = default(Vector3), bool zoomout = true)
        {
            if (cutsceneTools)
                cutsceneTools.SetFadeout(true);

            if (zoomout) SetInCutScene(true, CutsceneTools.Type.ZoomOut, transform.position + (advCamera != null ? (Vector3.up * advCamera.yFollowOffset) : Vector3.zero) + offsetCircle, false, 0.0001f);

            Debug.Log("Next Level");
            audioManager.Fadeout();

            yield return new WaitForSeconds(1.5f);
            LoadLevelInternal(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public IEnumerator LeaveRoom(DoorBetweenLevels door, State walkOption = State.WalkSide)
        {
            heldItem = combiningItem = null;
            targetItem = null;

            if (!staticCharacter)
            {
                if (door.zoomoutCircleTransformPosition != null)
                {
                    Debug.Log("LeaveRoom transfrom pos" + door);
                    SetInCutScene(true, CutsceneTools.Type.ZoomOut, door.zoomoutCircleTransformPosition.position, false, 0.0001f, false);
                }
                else
                    SetInCutScene(true, CutsceneTools.Type.ZoomOut, transform.position + door.zoomoutCircleOffset, false, 0.0001f, false);

                autoPilotTarget = door.outsideOffset + door.gameObject.transform.position;
                autoPilotDelta = autoPilotTarget - transform.position;
                isInAutopilot = true;
                autoPilotSpeed = 1 / 1.5f;
                autoPilotSpeed = door.walkInOutSpeed;

                ChangeState(walkOption);
                direction = transform.position.x < (autoPilotTarget.x) ? 1 : -1;
                ProcessDirectionAndScale(false);
            }

            Global.SetArrivalDoor(SceneManager.GetActiveScene().path);

            audioManager.Fadeout();

            if (!staticCharacter)
            {
                float maxTime = 1 / autoPilotSpeed;
                float startTime = Time.time;
                float dist = 100f;
                while (true)
                {
                    dist = Vector3.Distance(transform.position, door.transform.position + door.outsideOffset);
                    if (dist < 0.1f) break;
                    if (Time.time - startTime > maxTime) break;
                    yield return new WaitForSeconds(0.02f);
                }
            }
            StartCoroutine(DisableAutopilot(true));
            yield return StartCoroutine(door.ExitTranslationAnimation());
            scene.arrivalCave = -1;

            if (door.GetLevelIndex() == -1)
            {
                LoadLevelInternal(0);
            }
            else
            {
                LoadLevelInternal(door.GetLevelIndex());
            }
        }

        public IEnumerator LeaveRoomImmediate(DoorBetweenLevels door)
        {
            heldItem = combiningItem = null;
            targetItem = null;
            if (door.zoomoutCircleTransformPosition != null)
            {
                SetInCutScene(true, CutsceneTools.Type.ZoomOut, door.zoomoutCircleTransformPosition.position, false, 0.0001f);
            }
            else
                SetInCutScene(true, CutsceneTools.Type.ZoomOut, transform.position + door.zoomoutCircleOffset, false, 0.0001f);

            Global.SetArrivalDoor(SceneManager.GetActiveScene().path);

            audioManager.Fadeout();

            scene.arrivalCave = -1;

            if (door.GetLevelIndex() == -1)
            {
                LoadLevelInternal(0);
            }
            else
            {
                LoadLevelInternal(door.GetLevelIndex());
            }

            yield return null;
        }

        private void LoadLevelInternal(int level)
        {
            Global.SetState("Global level", level);
            Global.SetState("Global loadingLevel", level, true);

            SceneManager.LoadScene(level);
        }

        IEnumerator DisableAutopilot(bool changeState)
        {
            yield return new WaitForSeconds(1 / autoPilotSpeed);
            isInAutopilot = false;
            if (changeState)
                ChangeState(State.IdleFront);
        }

        void ArrivedAt(InventoryItem item)
        {
            StopTalking();
            StopWalking();
            isInUninterruptibleAction = false;

            if (targetItem && !targetItem.ArrivedAt(item))
            {
                if (targetItem.name != item.name)
                {
                    SayThatWontWork();
                }
            }

            heldItem = null;
            combiningItem = null;
        }

        public void TurnTowards(InteractiveItem obj)
        {
            int d = transform.position.x < (obj.transform.position.x + obj.GetCenterOffset(heldItem).x) ? 1 : -1;
            ChangeDirection(d);
            ProcessDirectionAndScale(false);
        }

        public void TurnTowards(bool left = false)
        {
            int d = left ? 1 : -1;
            ChangeDirection(d);
            Debug.Log("turning toward: " + left + " dir: " + direction);
            ProcessDirectionAndScale(false);
        }

        public void TurnTowards(int d = 1)
        {
            ChangeDirection(d);
            ProcessDirectionAndScale(false);
        }

        bool ShouldChangeDirection()
        {
            if (Time.time - lastTimeChangedDirection > 0.2f) return true;
            return false;
        }

        public void SetNavmeshWalkerEnabled(bool b)
        {
            if (navmeshWalker != null)
                navmeshWalker.enabled = b;
        }

        public void SetTargetItem(InteractiveItem newTargetItem, bool useNull = false)
        {
            if (newTargetItem != null || useNull)
            {
                targetItem = newTargetItem;
            }
        }

        protected void ProcessDirectionAndScale(bool changeState = true)
        {
            if (staticCharacter) return;
            ProcessDirection(changeState);
            ProcessScale();
        }

        protected void ProcessDirection(bool changeState)
        {
            Vector3 vel = navmeshWalker.GetVelocity();

            if (IsIdleOrWalking())
            {
                if (!ignoreNavMeshAgent && navmeshWalker && vel.sqrMagnitude > 0 && !staticCharacter)
                {
                    if (Mathf.Abs(vel.x * 0.2f) > Mathf.Abs(vel.y))
                    {
                        if (changeState)
                        {
                            if (ShouldChangeDirection())
                            {
                                ChangeState(State.WalkSide);
                                lastTimeChangedDirection = Time.time;
                            }
                        }

                        navmeshWalker.speed = walkSpeedMultiplier * Global.scene.speedHorizontal * GetSpeedModifierBasedOnYPosition();

                        if (vel.sqrMagnitude > 0)
                        {
                            if (!isInAutopilot)
                            {
                                if (vel.x > 0) ChangeDirection(1);
                                else
                                    ChangeDirection(-1);
                            }
                        }

                        if (vel.y <= 0)
                        {
                            talkDirection = TalkDirection.Diagonal;
                        }
                        else
                        {
                            talkDirection = TalkDirection.DiagonalBack;
                        }

                    }
                    else
                    {
                        navmeshWalker.speed = walkSpeedMultiplier * Global.scene.speedVertical * GetSpeedModifierBasedOnYPosition();

                        if ((vel.y <= 0 && !flipYCoordinate) || (vel.y > 0 && flipYCoordinate))
                        {
                            if (changeState)
                            {
                                if (ShouldChangeDirection())
                                {
                                    ChangeState(State.WalkFront);
                                    lastTimeChangedDirection = Time.time;
                                }
                            }
                            talkDirection = TalkDirection.Front;
                        }
                        else
                        {
                            if (ShouldChangeDirection())
                            {
                                ChangeState(State.WalkBack);
                                lastTimeChangedDirection = Time.time;
                            }
                            talkDirection = TalkDirection.DiagonalBack;
                        }
                    }
                }
                else
                {
                    if (!IsIdle() || staticCharacter)
                    {
                        switch (state)
                        {
                            case State.WalkBack:
                                if (changeState) ChangeState(State.IdleBack);
                                talkDirection = TalkDirection.DiagonalBack;
                                break;

                            case State.WalkFront:
                                if (changeState) ChangeState(State.IdleFront);
                                talkDirection = TalkDirection.Front;
                                break;

                            case State.WalkSide:
                                if (changeState) ChangeState(State.IdleDiagonalFront);
                                talkDirection = TalkDirection.Diagonal;
                                break;

                            default:
                                if (changeState) ChangeState(State.IdleDiagonalFront);
                                talkDirection = TalkDirection.Diagonal;
                                break;
                        }

                        if (targetItem)
                        {
                            ArrivedAt(combiningItem);
                        }
                    }
                }
            }
        }

        protected void ProcessScale()
        {
            float direction = this.direction;
            if (doProcessScale)
            {
                if (overrideAutoPilotScale)
                {
                    transform.localScale = Vector3.Lerp(autoPilotStartedScale, autoPilotUseScaleVector * overridedAutoPilotScale, (Time.time - autoPilotStartedTime) * autoPilotSpeed);
                }
                else
                {
                    scale = 1;
                    if (scene)
                    {
                        scale = scene.GetCharacterScale();
                    }
                    transform.localScale = direction < 1 ? scaleUnitVectorLeft * scale : scaleUnitVectorRight * scale;
                }
            }
            else
            {  // just change direction
                transform.localScale = direction < 1 ? scaleUnitVectorLeft * scale : scaleUnitVectorRight * scale;
            }
        }
    }
}
