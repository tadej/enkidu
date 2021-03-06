﻿using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InteractiveItemPuzzle : InteractiveItemAction
    {

        public FullscreenPuzzle puzzle;

        public string commentOnFinish;
        public Player.State actionAnimationOpen = Player.State.PickUpSide;

        public string customActionOnPuzzleSolved;

        [SaveState]
        public int progressCounter = 0;
        public string[] comments;

        public bool allowsDirect = false;

        public bool disableCollider = true;

        [SaveState]
        bool commenting = true;

        [SaveState]
        bool colliderDisabled = false;

        public bool setProgresToZero = true;

        public InteractiveItemSwitch.State enableOnlyForSwitchState = InteractiveItemSwitch.State.ANY;

        InteractiveItemSwitch connectedSwitch;

        public bool disableOnClose = true;

        public bool disableOnFinish = false;

        public bool removeInventoryItemOnFinish = false;

        public InventoryItem itemToRemove;

        public bool stayInCutscene = false;

        public bool doDelay = true;

        public bool doActionAnimationOpenAtTheEnd = true;

        protected AdvCamera advCam;

        protected bool advCamCanMove = false;

        public bool disableInteractiveItemPuzzleOnFinish = false;

        public bool doCloseup = false;

        public Vector3 closeUpOffset;

        public float closeUpSize;

        void Start()
        {
            base.Initialise();
     
            advCam = PersistentEngine.advCamera;
            advCamCanMove = advCam.canMove;

            connectedSwitch = GetComponent<InteractiveItemSwitch>();
            if (colliderDisabled)
                this.GetComponent<Collider2D>().enabled = false;
        }

        new void Update()
        {
            base.Update();
        }

        public IEnumerator AnnounceFinished(string msg)
        {
            yield return StartCoroutine(PersistentEngine.player.SpeakProcedure(msg, Player.TalkMode.Default, false));
        }

        public override IEnumerator ProcessArrivedAt()
        {
            float time0 = Time.time;

            if (connectedSwitch != null && !connectedSwitch.ValidateSwitchState(enableOnlyForSwitchState))
            {
                yield return null;
            }
            else

            if (allowsDirect)
            {
                if (setProgresToZero)
                    progressCounter = 0;
                PersistentEngine.player.TurnTowards(base.interactiveItem);
                PersistentEngine.player.ChangeState(actionAnimation);

                while (progressCounter == 0)
                {
                    yield return new WaitForSeconds(0.05f);

                    if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                    {
                        Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                        break;
                    }
                }
                ShowPuzzle();
                while (progressCounter == 1)
                {
                    yield return new WaitForSeconds(0.05f);

                    if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                    {
                        Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                        break;
                    }
                }
                PersistentEngine.player.ChangeState(endState);

            }
            yield return null;
        }

        IEnumerator ShowComments()
        {
            foreach (string comment in comments)
            {
                if (commenting)
                    yield return StartCoroutine(PersistentEngine.player.SpeakProcedure(comment));
            }
        }

        public void ShowPuzzle()
        {
            PersistentEngine.player.SetInCutScene(true, CutsceneTools.Type.Puzzle);
            PersistentEngine.player.ChangeState(actionAnimation);
            PersistentEngine.player.SetDestination(PersistentEngine.player.transform.position);

            if (doCloseup)
                advCam.CloseUpBegin(closeUpSize, closeUpOffset);

            StartCoroutine(ShowPuzzleProcedure());
        }

        IEnumerator ShowPuzzleProcedure()
        {
            if (!puzzle.gameObject.activeInHierarchy) puzzle.gameObject.SetActive(true);
            yield return null;
            if (!puzzle.gameObject.activeInHierarchy) puzzle.gameObject.SetActive(true);
            yield return null;
            puzzle.puzzle = this;
            puzzle.ToggleShow(true);
            if (!advCam)
                advCam = PersistentEngine.advCamera;

            advCam.CanCamMove(false);
            StartCoroutine(ShowComments());

        }

        public void PuzzleClosed()
        {
            puzzle.ToggleShow(false);
            if (!advCam)
                advCam = PersistentEngine.advCamera;
            advCam.CanCamMove(advCamCanMove);

            if (disableOnClose)
                puzzle.gameObject.SetActive(false);
            PersistentEngine.player.SetInCutScene(false);
            StartCoroutine(CloseProcedure());

            PersistentEngine.player.StopTalking();
        }

        public void PuzzleFinished()
        {

            if (!advCam)
                advCam = PersistentEngine.advCamera;
            advCam.CanCamMove(advCamCanMove);
            puzzle.ToggleShow(false);
            if (disableOnFinish)
                puzzle.gameObject.SetActive(false);
            if (disableCollider)
            {
                transform.GetComponent<Collider2D>().enabled = false;
                colliderDisabled = true;
            }

            if (disableInteractiveItemPuzzleOnFinish) this.enabled = false;

            StartCoroutine(FinishProcedure());
            if (removeInventoryItemOnFinish && itemToRemove != null)
            {
                itemToRemove.Remove();
            }

            gameObject.SendMessage("PuzzleCompleted", SendMessageOptions.DontRequireReceiver);
        }

        public override void AnimationActionPoint(string animationName)
        {
            progressCounter = 1;
        }

        public override void AnimationFinished(string animationName)
        {
            progressCounter = 2;
        }

        IEnumerator FinishProcedure()
        {
            progressCounter = 0;

            PersistentEngine.player.ChangeState(actionAnimationOpen);

            if (doCloseup)
                advCam.CloseUpEnd();

            PersistentEngine.player.SetTargetItem(base.interactiveItem);
            commenting = false;

            int i = 0;
            float time0 = Time.time;

            while (progressCounter == 0)
            {
                yield return new WaitForSeconds(0.05f);
                if (i > 20)
                    progressCounter = 2;

                i++;

                if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                {
                    Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                    break;
                }
            }
            yield return new WaitForSeconds(0.1f);


            if (string.IsNullOrEmpty(customActionOnPuzzleSolved) == false)
            {
                gameObject.SendMessage(customActionOnPuzzleSolved, SendMessageOptions.DontRequireReceiver);
            }

            gameObject.SendMessage("SwitchOn", SendMessageOptions.DontRequireReceiver);

            while (progressCounter == 1)
            {
                yield return new WaitForSeconds(0.05f);

                if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                {
                    Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                    break;
                }
            }

            if (doDelay)
                yield return new WaitForSeconds(0.2f);
            PersistentEngine.player.ChangeState(endState);
            yield return new WaitForSeconds(0.2f);

            if (!string.IsNullOrEmpty(commentOnFinish))
            {
                yield return StartCoroutine(PersistentEngine.player.SpeakProcedure(commentOnFinish));
            }

            StartCoroutine(CharacterSmileSuccess());

            yield return new WaitForSeconds(0.3f);

            if (!stayInCutscene)
                PersistentEngine.player.SetInCutScene(false);
        }

        IEnumerator CharacterSmileSuccess()
        {
            if (PersistentEngine.player.state == Player.State.IdleDiagonalFront)
            {
                PersistentEngine.player.ChangeState(Player.State.IdleDiagonalFrontSmile);
                yield return new WaitForSeconds(1.5f);
                if (PersistentEngine.player.state == Player.State.IdleDiagonalFrontSmile)
                {
                    PersistentEngine.player.ChangeState(Player.State.IdleDiagonalFront);
                }
            }
        }

        IEnumerator CloseProcedure()
        {
            progressCounter = 0;
            commenting = false;

            if (doActionAnimationOpenAtTheEnd)
                PersistentEngine.player.ChangeState(actionAnimationOpen);

            if (doCloseup)
                advCam.CloseUpEnd();

            PersistentEngine.player.SetTargetItem(base.interactiveItem);
            float currentTime = Time.time;


            while (doActionAnimationOpenAtTheEnd && progressCounter == 0)
            {
                yield return new WaitForSeconds(0.05f);
                if (Time.time - currentTime > 1)
                {
                    progressCounter = 2;
                }
            }

            while (doActionAnimationOpenAtTheEnd && progressCounter == 1)
            {
                yield return new WaitForSeconds(0.05f);
            }

            if (doActionAnimationOpenAtTheEnd) yield return new WaitForSeconds(0.1f);

            PersistentEngine.player.ChangeState(endState);
            PersistentEngine.player.SetTargetItem(null, true);
            yield return new WaitForSeconds(0.3f);

        }
    }
}