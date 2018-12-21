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
        
    public class NPCBrain : CharacterBrain
    {
        public enum State
        {
            None = -1,
            Idle = 0,
            Talk = 1,
            Action = 2,
            FacePlayer = 3,
            Give = 4,
            Take = 5,

            CustomAction1 = 6,
            CustomAction2 = 7,
            CustomAction3 = 8,

        }
    
        public State state;
        
        public override void ChangeState(int newState)
        {
            State s = State.Idle;

            try
            {
                s = (State)newState;
                ChangeState(s);
            }
            catch
            {}
        }

        public override void PlayStandardAnimation(string clip, string dir)
        {
            if(!isPlayingCustomAnimation)
            {
                switch(clip)
                {
                    case "idle":
                    {
                        ChangeState(State.Idle);

                        switch(dir)
                        {
                            case "U":
                                //ChangeState(PlayerBrain.State.IdleBack);
                                break;
                            case "D":
                                //ChangeState(PlayerBrain.State.IdleFront);
                                break;
                            case "L":
                                //ChangeState(PlayerBrain.State.IdleDiagonalFront);
                                break;
                            case "R":
                                //ChangeState(PlayerBrain.State.IdleDiagonalFront);
                                break;
                        }
                    }
                    break;

                    case "walk":
                    {
                        switch(dir)
                        {
                            case "U":
                                //ChangeState(PlayerBrain.State.WalkBack);
                                break;
                            case "D":
                                //.ChangeState(PlayerBrain.State.WalkFront);
                                break;
                            case "L":
                                //ChangeState(PlayerBrain.State.WalkSide);
                                break;
                            case "R":
                                //ChangeState(PlayerBrain.State.WalkSide);
                                break;
                        }
                    }
                    break; 
                }
                
                //PlayCharAnim (clip, 0);
            }
        }
        public override bool PlayCustomAnimation(string name, AnimationWaitingMethod waitingMethod)
        {
            base.PlayCustomAnimation(name, waitingMethod);
            
            Type type = typeof(State);
            State st;
            
            try
            {
                st = (State)Enum.Parse( type, name );
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
            if(newState == state) return;

            Debug.Log("Changing state to " + newState);

            if(animator) 
            {	
                animator.SetInteger("idleMode", 0);//.RoundToInt(6 * UnityEngine.Random.value));
                animator.SetInteger( "state", (int)newState );

                animator.SetBool( "disable", true );

                isAnimationPlaying = true;
                
            }
            state = newState;
            
        }

        public override void ChangeTalkMode(string mode)
        {
            //state == State.TalkDiagonal
            if(true && !string.IsNullOrEmpty(mode))
            {
                string [] parts = mode.Split('/');
                GestureConfigurations conf = GestureConfigurations.idle;
                if(parts.Length == 1)
                {
                    try
                    {
                        conf = (GestureConfigurations)Enum.Parse(typeof(GestureConfigurations), parts[0]);
                    }
                    catch(Exception e)
                    {
                        Debug.LogWarning("Couldn't parse gesture configuration " + mode + " " + e.Message);
                    }

                    ChangeGestureConfiguration(conf);
                }
                else if(parts.Length >= 2)
                {
                    switch(parts[0])
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
            animator.SetInteger("arms", (int)ag);
            currentArmGesture = ag;
        }
    
        void ChangeGestureEyes(EyeGestures eg)
        {
            //Debug.LogWarning("TODO: Eye gestures not implemented yet");
            currentEyeGesture = eg;
            //animator.SetInteger("arms", (int)ag);
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
            switch(conf)
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
            foreach(PlayerHead h in heads)
            {
                if(h!=null)
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
            ChangeState(State.Talk);

            base.RunLipSync(charName, src, audioClip, lineID, message);
        }

        protected override void ResetToIdle()
        {
            ChangeState(State.Idle);
        }

        public override void AnimationEventEndCutScene()
        {
            
        }

        public override void AnimationActionPoint(string animationName)
        {
            if(animationWaitingMethod == AnimationWaitingMethod.UntilActionPoint)
                isAnimationPlaying = false;
        }

        public override void AnimationFinished(string animationName)
        {
            Debug.Log("AnimationFinished " + animationName);
            isAnimationPlaying = false;
        }

    }
}
