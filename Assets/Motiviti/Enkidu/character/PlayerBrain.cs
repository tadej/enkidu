using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using RogoDigital.Lipsync;
using System;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class PlayerBrain : CharacterBrain
    {
        public enum State
        {
            None = -1,
            IdleFront = 0,
            IdleDiagonalFront = 1,
            IdleDiagonalBack = 2,
            IdleSide = 3,
            IdleBack = 4,
            WalkFront = 5,
            WalkDiagonalFront = 6,			// NOT YET SUPPORTED
            WalkSide = 7,
            WalkBack = 8,
            TalkFront = 9,
            TalkDiagonal = 10,
            IdleDiagonalFrontSmile = 11,
            WalkDiagonalBack = 12,			// NOT YET SUPPORTED
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

        public enum TalkDirection
        {
            Front = 0,
            Diagonal = 1,
            DiagonalBack = 2,
            TurnBack = 3
        }

        State[] idleOrWalkingStates = {State.IdleFront,
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

        State[] idleStates = {State.IdleFront,
            State.IdleDiagonalFront ,
            State.IdleDiagonalBack ,
            State.IdleSide ,
            State.IdleBack ,
            State.IdleDiagonalFrontAction,
            State.IdleDiagonalFrontSmile,
            State.IdleDiagonalFrontTurnBack,
            State.IdleBackAction};

        State[] talkingStates = {State.TalkDiagonal,
            State.TalkDiagonalBack ,
            State.TalkFront,
            State.TalkTurnBack};

        public Moods defaultIdleMood = Moods.Happy;
        public bool allowLongCustomAnimations = false;
        public State state = State.None;

        private bool reversePerspective = false;
        bool canWalk = true;

        State lastIdleState = State.IdleDiagonalFront;

        protected override void SwitchToIdleIfNotAlready()
        {
            if (!idleStates.Contains(state))
            {
                ChangeState(lastIdleState);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            state = State.None;
        }

        public override void ChangeState(int newState)
        {
            State s = State.IdleBack;

            try
            {
                s = (State)newState;
                ChangeState(s);
            }
            catch
            {
                Debug.LogWarning("Could not parse state: " + newState.ToString());
            }
        }

        protected override void SetArmAnimationEnabled(bool b)
        {
            animator.SetBool("disable", !b);
        }

        public void SetHoldingRemote(bool hr)
        {
            holdingRemote = hr;

            ChangeState(lastIdleState);

            ResetArmAnimationStatus();

            if (!hr)
            {
                ChangeGestureArms(ArmGestures.idle);
            }
        }

        public void SetHoldingRemote(int hr)
        {
            bool hrb = hr == 1 ? true : false;
            holdingRemote = hrb;

            ChangeState(lastIdleState);

            ResetArmAnimationStatus();

            if (!hrb)
            {
                ChangeGestureArms(ArmGestures.idle);
            }
        }

        //Adventure Creator Integration
        public override void PlayStandardAnimation(string clip, string dir)
        {
            if (!isPlayingCustomAnimation)
            {
                switch (clip)
                {
                    case "idle":
                        {
                            switch (dir)
                            {
                                case "U":
                                    if (reversePerspective)
                                        ChangeState(PlayerBrain.State.IdleFront);
                                    else
                                        ChangeState(PlayerBrain.State.IdleBack);
                                    break;
                                case "D":
                                    if (reversePerspective)
                                        ChangeState(PlayerBrain.State.IdleBack);
                                    else
                                        ChangeState(PlayerBrain.State.IdleFront);
                                    break;
                                case "L":
                                    ChangeState(PlayerBrain.State.IdleDiagonalFront);
                                    break;
                                case "R":
                                    ChangeState(PlayerBrain.State.IdleDiagonalFront);
                                    break;
                                default:
                                    ChangeState(PlayerBrain.State.IdleDiagonalFront);
                                    break;

                            }
                        }
                        break;

                    case "walk":
                        {
                            if (!canWalk) break;

                            switch (dir)
                            {
                                case "U":
                                    if (reversePerspective)
                                        ChangeState(PlayerBrain.State.WalkFront);
                                    else
                                        ChangeState(PlayerBrain.State.WalkBack);
                                    break;
                                case "D":
                                    if (reversePerspective)
                                        ChangeState(PlayerBrain.State.WalkBack);
                                    else
                                        ChangeState(PlayerBrain.State.WalkFront);
                                    break;
                                case "L":
                                    ChangeState(PlayerBrain.State.WalkSide);
                                    break;
                                case "R":
                                    ChangeState(PlayerBrain.State.WalkSide);
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public override bool PlayCustomAnimation(string name, AnimationWaitingMethod waitingMethod)
        {
            base.PlayCustomAnimation(name, waitingMethod);

            Type type = typeof(PlayerBrain.State);

            PlayerBrain.State st;

            try
            {
                st = (PlayerBrain.State)Enum.Parse(type, name);
            }
            catch
            {
                return false;
            }

            ChangeState(st);

            isPlayingCustomAnimation = true;
            return true;
        }

        public void ChangeState(State newState)
        { 
            if (newState == state && lastFrameIsTalking == IsTalking)
            {
                return;
            }

            state = newState;
            if (idleStates.Contains(newState)) lastIdleState = newState;
         
            if (animator)
            {
                ResetArmAnimationStatus();

                animator.SetInteger("state", (int)newState);

                animator.SetInteger("idleMode", 0);

                if (idleOrWalkingStates.Contains(newState))
                {
                    SetEyeGesture(EyeGestures.idle);

                    SetMood((int)defaultIdleMood);
                }
                isAnimationPlaying = true;
            }

            lastFrameIsTalking = IsTalking;
        }

        bool IsAbleToCarryRemote()
        {
            return (idleStates.Contains(state) || talkingStates.Contains(state));
        }

        protected override void ResetArmAnimationStatus()
        {
            if (IsTalking && !holdingRemote)
            {
                SetArmAnimationEnabled(true);
            }
            else
            if (IsAbleToCarryRemote())
            {
                if (holdingRemote)
                {
                    SetArmAnimationEnabled(true);
                    ChangeGestureArms(ArmGestures.holdingRemote);
                }
            }
            else
            {
                SetArmAnimationEnabled(false);
                ChangeGestureArms(ArmGestures.idle);
            }
        }

        public override void ChangeTalkMode(string mode)
        {
            //state == State.TalkDiagonal
            if (true && !string.IsNullOrEmpty(mode))
            {
                string[] parts = mode.Split('/');
                GestureConfigurations conf = GestureConfigurations.idle;
                if (parts.Length == 1)
                {
                    try
                    {
                        conf = (GestureConfigurations)Enum.Parse(typeof(GestureConfigurations), parts[0]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Couldn't parse gesture configuration " + mode + " " + e.Message);
                    }

                    ChangeGestureConfiguration(conf);
                }
                else if (parts.Length >= 2)
                {
                    switch (parts[0])
                    {
                        case "arms":
                            try
                            {
                                ArmGestures gest = (ArmGestures)Enum.Parse(typeof(ArmGestures), parts[1]);
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
                                EyeGestures gest = (EyeGestures)Enum.Parse(typeof(EyeGestures), parts[1]);
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
                                HeadGestures gest = (HeadGestures)Enum.Parse(typeof(HeadGestures), parts[1]);
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
                                NeckGestures gest = (NeckGestures)Enum.Parse(typeof(NeckGestures), parts[1]);
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
                                BodyGestures gest = (BodyGestures)Enum.Parse(typeof(BodyGestures), parts[1]);
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

        void ChangeGestureArms(ArmGestures ag)
        {
            if (!holdingRemote || (ag == ArmGestures.holdingRemote || (!IsAbleToCarryRemote() && ag == ArmGestures.idle)))
            {
                animator.SetInteger("arms", (int)ag);

                currentArmGesture = ag;
            }
        }

        void ChangeGestureEyes(EyeGestures eg)
        {
            currentEyeGesture = eg;
            SetEyeGesture(eg);
        }

        void ChangeGestureHead(HeadGestures hg)
        {
            animator.SetInteger("head", (int)hg);
            currentHeadGesture = hg;
        }

        void ChangeGestureNeck(NeckGestures ng)
        {
            animator.SetInteger("neck", (int)ng);
            currentNeckGesture = ng;
        }

        void ChangeGestureBody(BodyGestures bg)
        {
            animator.SetInteger("body_lean", (int)bg);
            currentBodyGesture = bg;
        }

        void ChangeGestureConfiguration(GestureConfigurations conf)
        {
            switch (conf)
            {
                case GestureConfigurations.idle:
                    ChangeGestureArms(ArmGestures.idle);
                    ChangeGestureBody(BodyGestures.idle);
                    ChangeGestureNeck(NeckGestures.idle);
                    ChangeGestureHead(HeadGestures.idle);
                    ChangeGestureEyes(EyeGestures.talk);
                    break;

                case GestureConfigurations.explaining:
                    ChangeGestureArms(ArmGestures.explain5); // or 6 or 7
                    ChangeGestureBody(BodyGestures.backLeanSlight);
                    ChangeGestureNeck(NeckGestures.idle);
                    ChangeGestureHead(HeadGestures.talk1);
                    ChangeGestureEyes(EyeGestures.talk);
                    break;

                case GestureConfigurations.explainingExactly:
                    ChangeGestureArms(ArmGestures.explain4);
                    ChangeGestureBody(BodyGestures.frontLeanSlight);
                    ChangeGestureNeck(NeckGestures.idle);
                    ChangeGestureHead(HeadGestures.talk1);
                    ChangeGestureEyes(EyeGestures.talk);
                    break;

                case GestureConfigurations.extremelyExcited:
                    ChangeGestureArms(ArmGestures.armsUp);
                    ChangeGestureBody(BodyGestures.backLean);
                    ChangeGestureNeck(NeckGestures.idle);
                    ChangeGestureHead(HeadGestures.talk1);
                    ChangeGestureEyes(EyeGestures.talk);
                    break;

                case GestureConfigurations.proud:
                    ChangeGestureArms(ArmGestures.proud);
                    ChangeGestureBody(BodyGestures.backLeanSlight);
                    ChangeGestureNeck(NeckGestures.idle);
                    ChangeGestureHead(HeadGestures.talk1);
                    ChangeGestureEyes(EyeGestures.talk);
                    break;

                case GestureConfigurations.embarrassed:
                    ChangeGestureArms(ArmGestures.crossedArms);
                    ChangeGestureBody(BodyGestures.frontLeanSlight);
                    ChangeGestureNeck(NeckGestures.idle);
                    ChangeGestureHead(HeadGestures.embarrassed);
                    ChangeGestureEyes(EyeGestures.down);
                    break;
            }
        }

        public void SetEyeGesture(CharacterBrain.EyeGestures eyeGesture)
        {
            foreach (PlayerHead h in heads)
            {
                if (h != null)
                    h.SetEyeGesture(eyeGesture);
            }

            this.eyeGesture = eyeGesture;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void RunLipSync(string charName, AudioSource src, AudioClip audioClip, int lineID, string message)
        {
            animator.SetBool("talking", true);

            SwitchToIdleIfNotAlready();

            base.RunLipSync(charName, src, audioClip, lineID, message);
        }

        public override void AnimationEventEndCutScene()
        {

        }

        public override void AnimationActionPoint(string animationName)
        {
            if (animationWaitingMethod == AnimationWaitingMethod.UntilActionPoint)
                isAnimationPlaying = false;
        }

        public override void AnimationFinished(string animationName)
        {
            isAnimationPlaying = false;
        }

        public void DisableWalk()
        {
            canWalk = false;
        }
        public void EnableWalk()
        {
            canWalk = true;
        }

        public void OnTeleport()
        {
            var sg = GetComponent<UnityEngine.Rendering.SortingGroup>();
            if (sg != null) sg.sortingOrder = -(int)(transform.position.y * 10);
        }
    }
}