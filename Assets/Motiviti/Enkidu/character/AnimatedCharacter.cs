using UnityEngine;
using System;
using System.Collections;
using PigeonCoopToolkit.Navmesh2D;
using UnityEngine.SceneManagement;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class AnimatedCharacter : StatefulItem
    {
        public bool staticCharacter = false;
        protected Action _AnimationActionPointCallBack = null;
        protected Action _AnimationFinishedCallBack = null;
        protected Vector3 scaleUnitVectorLeft = new Vector3(-1, 1, 1);
        protected Vector3 scaleUnitVectorRight = new Vector3(1, 1, 1);
        public bool doProcessScale = true;

        public enum State
        {
            None = -1,
            IdleFront = 0,
            IdleDiagonalFront = 1,
            IdleDiagonalBack = 2,
            IdleSide = 3,
            IdleBack = 4,
            WalkFront = 5,
            WalkDiagonalFront = 6,          // NOT YET SUPPORTED
            WalkSide = 7,
            WalkBack = 8,
            TalkFront = 9,
            TalkDiagonal = 10,
            IdleDiagonalFrontSmile = 11,
            WalkDiagonalBack = 12,          // NOT YET SUPPORTED
            SwitchOnOffSide = 14,
            PickUpSide = 15,
            PickUpFront = 18,
            IdleDiagonalFrontAction = 22,
            PushBack = 23,
            FixingBack = 24,
            TalkDiagonalBack = 25,
            IdleDiagonalFrontTurnBack = 26,
            TalkTurnBack = 29,
            PickUpFromFloorDiagonalFront = 30,
            StandUpFromFloor = 31,
            IdleBackAction = 32,
            PickUpFromFloorBack = 33,
            SurprisedDiagonalFront = 40,
            SurprisedJumpDiagonalFront = 41,
            GiveSideDown = 45,
            SideKnock = 46,
            Invisible = 1000000
        }

        public enum GestureConfigurations
        {
            idle = 0,
            explaining = 1,
            explainingExactly = 2,
            extremelyExcited = 3,
            proud = 4,
            embarrassed = 5
        }

        public enum BodyGestures
        {
            idle = 0,
            frontLeanSlight = 1,
            frontLean = 2,
            backLeanSlight = 3,
            backLean = 4
        }

        public enum HeadGestures
        {
            idle = 0,
            talk1 = 1,
            talk2 = 2,
            slowTalk = 3,
            lookAtCamera = 4,
            embarrassed = 5
        }

        public enum NeckGestures
        {
            idle = 0,
            leanFront = 1,
            leanBack = 2,
            lookBack = 3
        }

        public enum EyeGestures
        {
            idle = 0,
            talk = 1,
            up = 2,
            down = 3,
            left = 4,
            right = 5,
            back = 6,
            side = 7,
            follow = 8
        }
        public enum ArmGestures
        {
            idle = 0,
            me1 = 1,
            me2 = 2,
            me3 = 3,
            you1 = 5,
            you2 = 6,
            you3 = 7,
            excitedFists = 8,
            stop = 9,
            explain1 = 10,
            explain2 = 11,
            explain3 = 12,
            explain4 = 13,
            explain5 = 14,
            explain6 = 15,
            explain7 = 16,
            idea = 17,
            why1 = 20,
            why2 = 21,
            oops1 = 25,
            oops2 = 26,
            thinking1 = 30,
            thinking2 = 31,
            proud = 35,
            crossedArms = 36,
            armsUp = 40
        }
        public enum TalkMode
        {
            Default = 0,
            ThinkingChin = 1,
            ThinkingHead = 2,
            Excited = 3,
            OnPhone = 4,
            Proud = 5
        }

        public enum TalkDirection
        {
            Front = 0,
            Diagonal = 1,
            DiagonalBack = 2,
            TurnBack = 3
        }


        public enum EyeMode
        {
            Idle = 0,
            Talk = 1,
            Up = 2,
            Down = 3,
            Back = 6,
            Side = 7
        }

        public TalkDirection talkDirection = TalkDirection.Diagonal;

        protected State[] idleOrWalkingStates = {State.IdleFront,
            State.IdleDiagonalFront ,
            State.IdleDiagonalBack ,
            State.IdleSide ,
            State.IdleBack ,
            State.WalkFront ,
            State.WalkSide ,
            State.WalkBack ,
            State.IdleDiagonalFrontSmile,
            State.WalkDiagonalBack,
            State.WalkDiagonalFront,
            State.IdleDiagonalFrontAction,
            State.IdleBackAction,
            State.Invisible
        };

        protected State[] walkingStates = {
            State.WalkFront ,
            State.WalkSide ,
            State.WalkBack
        };

        protected State[] idleStates = {State.IdleFront,
            State.IdleDiagonalFront ,
            State.IdleDiagonalBack ,
            State.IdleSide ,
            State.IdleBack ,
            State.IdleDiagonalFrontAction,
            State.IdleDiagonalFrontSmile,
            State.IdleDiagonalFrontTurnBack,
            State.IdleBackAction};

        protected enum Direction
        {
            FRONT = 0,
            BACK = 1,
            SIDE = 2,
            DIAGONAL_FRONT = 3
        }

        public State state;
        protected State lastState;
        protected CharacterBrain.ArmGestures currentArmGesture;
        protected CharacterBrain.HeadGestures currentHeadGesture;
        protected CharacterBrain.EyeGestures currentEyeGesture;
        protected CharacterBrain.NeckGestures currentNeckGesture;
        protected CharacterBrain.BodyGestures currentBodyGesture;
        public PlayerBrain PlayerBrain;
        protected PlayerHead PlayerHead;
        protected AdvCamera advCamera;
        public CharacterHead conversationPartner = null;
        protected bool flipYCoordinate = false;
        public bool isOnlyAVoice = false;
        public float walkSpeedMultiplier = 1f;
        public Transform transformTalkFront, transformTalkDiagonal, transformAdv, transformTalkDiagonalBack;
        protected bool ignoreNavMeshAgent = false;
        public Transform transformTalk;
        protected Animator animator;
        protected NavmeshWalker navmeshWalker;
        protected AudioManager audioManager;
        protected Scene scene;
        public InteractiveItem targetItem = null;
        protected InventoryItem heldItem = null;
        protected InventoryItem combiningItem = null;
        protected bool interruptingSentence = false;
        protected bool isInAutopilot = false;
        protected Vector3 autoPilotTarget = Vector3.zero;
        protected Vector3 autoPilotDelta = Vector3.zero;
        protected float autoPilotSpeed = 1 / 1.5f;
        protected float holdingSince = 0;
        protected bool wasHolding = false;

        protected Direction GetDirection()
        {
            Direction dir;

            switch (state)
            {
                case State.WalkFront:
                case State.TalkFront:
                case State.IdleFront:
                    dir = Direction.FRONT;
                    break;
                case State.WalkBack:
                case State.IdleBack:
                case State.TalkTurnBack:
                    dir = Direction.BACK;
                    break;
                case State.WalkSide:
                case State.PickUpSide:
                case State.SideKnock:
                case State.GiveSideDown:
                case State.IdleSide:
                    dir = Direction.SIDE;
                    break;

                default:
                    dir = Direction.DIAGONAL_FRONT;
                    break;

            }

            return dir;
        }


        public void ChangeTalkMode(string mode)
        {
            if (!staticCharacter && state == State.TalkDiagonal && !string.IsNullOrEmpty(mode))
            {
                string[] parts = mode.Split('/');

                if (parts.Length == 1)
                {
                    try
                    {
                        CharacterBrain.GestureConfigurations conf = (CharacterBrain.GestureConfigurations)Enum.Parse(typeof(CharacterBrain.GestureConfigurations), parts[0]);
                        ChangeGestureConfiguration(conf);
                    }
                    catch
                    {
                        Debug.LogWarning("Couldn't parse gesture configuration " + mode + " " + parts[0]);
                    }
                }
                else if (parts.Length >= 2)
                {
                    switch (parts[0])
                    {
                        case "arms":
                            try
                            {
                                CharacterBrain.ArmGestures gest = (CharacterBrain.ArmGestures)Enum.Parse(typeof(CharacterBrain.ArmGestures), parts[1]);
                                ChangeGestureArms(gest);
                            }
                            catch
                            {
                                Debug.LogWarning("Couldn't parse gesture arms " + mode + " " + parts[1]);
                            }
                            break;

                        case "eyes":
                            try
                            {
                                CharacterBrain.EyeGestures gest = (CharacterBrain.EyeGestures)Enum.Parse(typeof(CharacterBrain.EyeGestures), parts[1]);
                                ChangeGestureEyes(gest);
                            }
                            catch
                            {
                                Debug.LogWarning("Couldn't parse gesture eyes " + mode + " " + parts[1]);
                            }
                            break;

                        case "head":
                            try
                            {
                                CharacterBrain.HeadGestures gest = (CharacterBrain.HeadGestures)Enum.Parse(typeof(CharacterBrain.HeadGestures), parts[1]);
                                ChangeGestureHead(gest);
                            }
                            catch
                            {
                                Debug.LogWarning("Couldn't parse gesture head " + mode + " " + parts[1]);
                            }
                            break;

                        case "neck":
                            try
                            {
                                CharacterBrain.NeckGestures gest = (CharacterBrain.NeckGestures)Enum.Parse(typeof(CharacterBrain.NeckGestures), parts[1]);
                                ChangeGestureNeck(gest);
                            }
                            catch
                            {
                                Debug.LogWarning("Couldn't parse gesture neck " + mode + " " + parts[1]);
                            }
                            break;

                        case "body":
                            try
                            {
                                CharacterBrain.BodyGestures gest = (CharacterBrain.BodyGestures)Enum.Parse(typeof(CharacterBrain.BodyGestures), parts[1]);
                                ChangeGestureBody(gest);
                            }
                            catch
                            {
                                Debug.LogWarning("Couldn't parse gesture body " + mode + " " + parts[1]);
                            }
                            break;

                        default:
                            Debug.LogWarning("Unknown gesture type, " + mode);
                            break;
                    }
                }
            }
        }

        void ChangeGestureArms(CharacterBrain.ArmGestures ag)
        {
            animator.SetInteger("arms", (int)ag);
            currentArmGesture = ag;
        }

        public CharacterBrain.EyeGestures GetCurrentEyeGesture()
        {
            return currentEyeGesture;
        }
        void ChangeGestureEyes(CharacterBrain.EyeGestures eg)
        {
            currentEyeGesture = eg;
            if (PlayerBrain != null) PlayerBrain.SetEyeGesture(eg);
        }

        void ChangeGestureHead(CharacterBrain.HeadGestures hg)
        {
            animator.SetInteger("head", (int)hg);
            currentHeadGesture = hg;
        }

        void ChangeGestureNeck(CharacterBrain.NeckGestures ng)
        {
            animator.SetInteger("neck", (int)ng);
            currentNeckGesture = ng;
        }

        void ChangeGestureBody(CharacterBrain.BodyGestures bg)
        {
            animator.SetInteger("body_lean", (int)bg);
            currentBodyGesture = bg;
        }

        void ChangeGestureConfiguration(CharacterBrain.GestureConfigurations conf)
        {
            switch (conf)
            {
                case CharacterBrain.GestureConfigurations.idle:
                    ChangeGestureArms(CharacterBrain.ArmGestures.idle);
                    ChangeGestureBody(CharacterBrain.BodyGestures.idle);
                    ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
                    ChangeGestureHead(CharacterBrain.HeadGestures.idle);
                    ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
                    break;

                case CharacterBrain.GestureConfigurations.explaining:
                    ChangeGestureArms(CharacterBrain.ArmGestures.explain5); // or 6 or 7
                    ChangeGestureBody(CharacterBrain.BodyGestures.backLeanSlight);
                    ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
                    ChangeGestureHead(CharacterBrain.HeadGestures.talk1);
                    ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
                    break;

                case CharacterBrain.GestureConfigurations.explainingExactly:
                    ChangeGestureArms(CharacterBrain.ArmGestures.explain4);
                    ChangeGestureBody(CharacterBrain.BodyGestures.frontLeanSlight);
                    ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
                    ChangeGestureHead(CharacterBrain.HeadGestures.talk1);
                    ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
                    break;

                case CharacterBrain.GestureConfigurations.extremelyExcited:
                    ChangeGestureArms(CharacterBrain.ArmGestures.armsUp);
                    ChangeGestureBody(CharacterBrain.BodyGestures.backLean);
                    ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
                    ChangeGestureHead(CharacterBrain.HeadGestures.talk1);
                    ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
                    break;

                case CharacterBrain.GestureConfigurations.proud:
                    ChangeGestureArms(CharacterBrain.ArmGestures.proud);
                    ChangeGestureBody(CharacterBrain.BodyGestures.backLeanSlight);
                    ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
                    ChangeGestureHead(CharacterBrain.HeadGestures.talk1);
                    ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
                    break;

                case CharacterBrain.GestureConfigurations.embarrassed:
                    ChangeGestureArms(CharacterBrain.ArmGestures.crossedArms);
                    ChangeGestureBody(CharacterBrain.BodyGestures.frontLeanSlight);
                    ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
                    ChangeGestureHead(CharacterBrain.HeadGestures.embarrassed);
                    ChangeGestureEyes(CharacterBrain.EyeGestures.down);
                    break;
            }
        }

        public void AnimateWithCallBack(Player.State animation, Action AnimationActionPointCallBack, Action AnimationFinishedCallBack)
        {
            ChangeState(animation, true, AnimationActionPointCallBack, AnimationFinishedCallBack);
        }

        public void AnimationActionPoint(string animationName)
        {
            if (targetItem) targetItem.AnimationActionPoint(animationName);

            if (_AnimationActionPointCallBack != null)
            {
                _AnimationActionPointCallBack();
            }
        }

        public void AnimationFinished(string animationName)
        {
            if (targetItem)
            {
                targetItem.AnimationFinished(animationName);
                targetItem = null;
            }

            if (_AnimationFinishedCallBack != null)
            {
                _AnimationFinishedCallBack();
            }

            if (animationName == "Idle")
            {
                if (animator) animator.SetInteger("idleMode", 0);
            }

        }

        Vector3 GetClosestPosition(Vector3 inPos)
        {
            Vector3 outPos = Vector3.zero;

            UnityEngine.AI.NavMeshHit hit;

            if (UnityEngine.AI.NavMesh.SamplePosition(inPos, out hit, 500, navmeshWalker.walkableMask))
            {
                outPos = hit.position;
            }

            return outPos;
        }

        public void ChangeState(State newState, bool overrideSameStateCheck = false, Action AnimationActionPointCallBack = null, Action AnimationFinishedCallBack = null)
        {
            _AnimationActionPointCallBack = AnimationActionPointCallBack;
            _AnimationFinishedCallBack = AnimationFinishedCallBack;

            if (newState == State.None)
                return;

            if (state == newState && !overrideSameStateCheck) return;

            Debug.Log(state + " -> " + newState);

            StartCoroutine(ChangeStateProcess(newState, overrideSameStateCheck));
        }

        IEnumerator ChangeStateProcess(State newState, bool overrideSameStateCheck = false)
        {
            if (animator) animator.SetInteger("idleMode", 0);

            switch (newState)
            {
                case State.IdleDiagonalFront:
                    if (PlayerHead)
                        PlayerHead.SetMood((int)PlayerHead.Moods.Neutral);
                    break;
                case State.PushBack:
                    talkDirection = TalkDirection.DiagonalBack;
                    break;
            }

            ApplyTalkDirection();

            state = newState;

            animator.SetInteger("state", (int)state);

            bool dis = true;

            if (state == State.TalkDiagonal)
            {
                dis = false;
            }

            animator.SetBool("disable", dis);

            lastState = newState;

            yield return null;
        }

        protected void ApplyTalkDirection()
        {
            if (talkDirection == TalkDirection.Diagonal)
            {
                transformTalk = transformTalkDiagonal;
            }
            else if (talkDirection == TalkDirection.DiagonalBack)
            {
                transformTalk = transformTalkDiagonalBack;
            }
            else if (talkDirection == TalkDirection.TurnBack)
            {
                transformTalk = transformTalkDiagonal;
            }
            else
            {
                transformTalk = transformTalkFront;
            }
        }

        public void AnimationFinished()
        {
            Debug.Log("animation finished");
        }

        public void ChangeMood(CharacterHead.Moods mood)
        {
            PlayerBrain.ChangeMood(mood);
        }

        public bool IsIdleOrWalking()
        {
            foreach (var st in idleOrWalkingStates)
            {
                if (state == st) return true;

            }
            return false;
        }

        public bool IsWalking()
        {
            foreach (var st in walkingStates)
            {
                if (state == st) return true;

            }
            return false;
        }

        public bool IsTalking()
        {
            if (animator == null) return false;
            return animator.GetBool("talking");
        }

        protected bool IsIdle()
        {
            foreach (var st in idleStates)
            {
                if (state == st) return true;

            }
            return false;
        }

        public void TurnTowardsPlayer()
        {
            talkDirection = TalkDirection.Front;
            ApplyTalkDirection();
        }

        public void ChangeTalkDirection(TalkDirection dir)
        {
            talkDirection = dir;
            ApplyTalkDirection();
        }


    }
}