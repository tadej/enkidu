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
    public class CharacterBrain : CharacterHead
    {
        public float talkAngle = 180;
        public bool isPlayingCustomAnimation = false;
        public CharacterHead conversationPartner = null;
        public EyeGestures eyeGesture = EyeGestures.idle;
        public CharacterHead.Moods mood;
        public PlayerHead[] heads;
        public bool interruptFlag = false;
        public bool isAnimationPlaying = false;
        public AnimationWaitingMethod animationWaitingMethod = AnimationWaitingMethod.UntilAnimationFinish;
        public bool alwaysTurnToFacePlayer = false;

        protected Transform mainTransform = null;
        protected string currentMode = "";
        protected ArmGestures currentArmGesture;
        protected HeadGestures currentHeadGesture;
        protected EyeGestures currentEyeGesture;
        protected NeckGestures currentNeckGesture;
        protected BodyGestures currentBodyGesture;
        [SerializeField]
        protected Animator animator;
        protected bool isTalking = false;
        protected bool lastFrameIsTalking = false;
        protected bool holdingRemote = false;

        string [] phonemes = {"AI", "E", "U", "O", "CDGKNRSThYZ", "FV", "L", "MBP", "WQ", "Rest"};

        public enum AnimationWaitingMethod
        {
            UntilActionPoint=0,
            UntilAnimationFinish=1
        }
        
        public class VoiceClip
        {
            public string name;
            public string text;
            public PhonemePositions phonemes;
            public AudioClip audioClip;
            public CharacterHead.Moods mood;
            public string mode = "idle";
            public AudioSource audioSource;
            public float volume = 1;
            public bool interruptFlag = false;
            public float duration = 0;
        }

        public class PhonemePosition
        {
            public float position = 0;
            public string phoneme;
            
            public PhonemePosition(float pos, string pho)
            {
                position = pos;
                phoneme = pho;
            }

            public override string ToString()
            {
                return String.Format("[{0}] {1}", position.ToString(), phoneme);

            }
        }

        public class MoodPosition
        {
            public float position = 0;
            public CharacterHead.Moods mood;
            
            public MoodPosition(float pos, CharacterHead.Moods m)
            {
                position = pos;
                mood = m;
            }
        }

        public class ModePosition
        {
            public float position = 0;
            public string mode;
            
            public ModePosition(float pos, string m)
            {
                position = pos;
                mode = m;
            }
        }

        public class PhonemePositions
        {
            public LipSyncData lipSyncData;
            ArrayList positions = new ArrayList();

            ArrayList moodPositions = new ArrayList();

            ArrayList modePositions = new ArrayList();
            
            public void Add( PhonemePosition pos ) // must be ordered
            {
                positions.Add ( pos );
            }

            public void AddMood( MoodPosition pos ) // must be ordered
            {
                moodPositions.Add ( pos );
            }

            public void AddMode( ModePosition pos ) // must be ordered
            {
                modePositions.Add ( pos );
            }
            
            public string GetPhoneme( float position )
            {
                float currentPosition = -1;
                string currentPhoneme = "MBP";
                
                for(int i=0; i<positions.Count; i++)
                {
                    currentPhoneme = ((PhonemePosition)positions[i]).phoneme;

                    if(i < positions.Count-1)
                    {
                        currentPosition = ((PhonemePosition)positions[i+1]).position;
                    }
                    else
                        break;

                    if(currentPosition >= position) break;
                }
                
                int maxI = positions.Count-1;
                
                var cp = ((PhonemePosition)positions[ maxI ]).position;
                
                if(position > cp)
                {
                    if(position - cp > 0.1f) currentPhoneme = "MBP";
                }

                return currentPhoneme;
            }

            public void GetMood( float position, ref CharacterHead.Moods currentMood, ref bool change  )
            {
                change = false;
                float currentPosition = -1;

                for(int i=0; i<moodPositions.Count; i++)
                {
                    currentPosition = ((MoodPosition)moodPositions[i]).position;
                    
                    if(position >= currentPosition && (i >= moodPositions.Count-1 || position < ((MoodPosition)moodPositions[i+1]).position)) 
                    {
                        if(currentMood != ((MoodPosition)moodPositions[i]).mood)
                        {
                            currentMood = ((MoodPosition)moodPositions[i]).mood;
                            change = true;
                        }
                        return;
                    }
                }

                currentMood = CharacterHead.Moods.Neutral; // will not get picked up unless change==true
            }

            public void GetMode( float position, ref string currentMode, ref bool change )
            {
                change = false;
                float currentPosition = -1;

                for(int i=0; i<modePositions.Count; i++)
                {
                    currentPosition = ((ModePosition)modePositions[i]).position;
                    
                    if(position >= currentPosition && (i >= modePositions.Count-1 || position < ((ModePosition)modePositions[i+1]).position)) 
                    {
                        if(currentMode != ((ModePosition)modePositions[i]).mode)
                        {
                            currentMode = ((ModePosition)modePositions[i]).mode;
                            change = true;
                        }
                        return;
                    }
                }

                currentMode = "idle"; // will not get picked up unless change==true
            }
        }
    
        public enum GestureConfigurations
        {
            idle=0,
            explaining=1,
            explainingExactly=2,
            extremelyExcited=3,
            proud=4,
            embarrassed=5
        }

        public enum BodyGestures
        {
            idle=0,
            frontLeanSlight=1,
            frontLean=2,
            backLeanSlight=3,
            backLean=4
        }

        public enum HeadGestures
        {
            idle=0,
            talk1=1,
            talk2=2,
            slowTalk=3,
            lookAtCamera=4,
            embarrassed=5
        }

        public enum NeckGestures
        {
            idle=0,
            leanFront=1,
            leanBack=2,
            lookBack=3
        }

        public enum EyeGestures
        {
            idle=0,
            talk=1,
            up=2,
            down=3,
            left=4,
            right=5,
            back=6,
            side=7,
            follow=8
        }

        public enum ArmGestures
        {
            idle = 0,
            me1 = 1,
            me2 = 2,
            me3 = 3,
            you1 = 5,
            you2 = 6,
            you3=7,
            excitedFists=8,
            stop=9,
            explain1=10,
            explain2=11,
            explain3=12,
            explain4=13,
            explain5=14,
            explain6=15,
            explain7=16,
            idea=17,
            why1=20,
            why2=21,
            oops1=25,
            oops2=26,
            thinking1=30,
            thinking2=31,
            proud=35,
            crossedArms=36,
            armsUp=40,
            holdingRemote=50
        }

        public virtual void ChangeState(int newState)
        {
            Debug.LogWarning("TODO: ChangeState not implemented.");
        }

        public virtual void ChangeTalkMode(string mode)
        {
            Debug.LogWarning("TODO: Implement ChangeTalkMode.");   
        }

        public bool IsTalking
        {
            get
            {
                return isTalking;
            }

            set
            {
                isTalking = value;
            }
        }
        
        public virtual bool PlayCustomAnimation(string name, AnimationWaitingMethod method)
        {
            this.animationWaitingMethod = method;
            return false;
        }

        public virtual void PlayStandardAnimation(string clip, string dir)
        {

        }

        protected virtual void Awake()
        {
            foreach (PlayerHead h in heads)
            {
                if(h!=null)
                h.elroyBrain = this;
            }

            if(mainTransform==null)
            {
                if(animator != null)
                mainTransform = animator.transform.parent;
                else 
                mainTransform = transform;
            }
        }

        public override void Start()
        {
            base.Start();
        }

        void OnEnable()
        {
            StartCoroutine (BlinkProc ());
        }

        public EyeGestures GetCurrentEyeGesture()
        {
            return currentEyeGesture;
        }
        protected virtual IEnumerator BlinkProc()
        {
            while (true) 
            {
                if(IsTalking)
                {
                    yield return new WaitForSeconds (0.5f + UnityEngine.Random.value * 1); 
                }
                else
                {
                    yield return new WaitForSeconds (1.5f + UnityEngine.Random.value * 3); 
                }

                foreach(PlayerHead h in heads)
                {
                    if(h != null) h.Blink ();
                }
            }
        }

        public override void SetMood(int moodIndex)
        {
            mood = (CharacterHead.Moods)moodIndex;

            foreach (PlayerHead head in heads)
            {
                if(head && head.isActiveAndEnabled)head.SetMood(moodIndex);
            }
        }

        public override void ChangeMood(CharacterHead.Moods mood)
        {
            this.mood = mood;
            foreach (PlayerHead head in heads)
            {
                if (head /*&& head.isActiveAndEnabled*/) head.SetMood((int)mood);
            }
        }

        public override void StopTalking()
        {
            ChangeTalkMode("idle");
            ShowPhoneme("MBP");
            SetTalkMode(false);
            base.StopTalking();
            interruptFlag = true;

            foreach (PlayerHead head in heads)
            {
                if (head /*&& head.isActiveAndEnabled*/) head.StopTalking();
            }
            ChangeMood(Moods.Neutral);

            IsTalking = false;
        }

        public override void ShowPhoneme(string phoneme)
        {
            foreach(PlayerHead head in heads)
            {
                if (head && head.isActiveAndEnabled) head.ShowPhoneme(phoneme);
            }
        }

        public virtual void RunLipSync(string charName, AudioSource src, AudioClip audioClip, int lineID, string message)
        {
            StartCoroutine( RunLipSyncProc(charName, src, audioClip, lineID, message));
        }

        CharacterHead.Moods GetMoodFromString(string name, out string restOfText)
        {
            if(string.IsNullOrEmpty(name)) 
            {
                restOfText = name;
                return CharacterHead.Moods.Neutral;
            }

            restOfText = name;

            int moodPos = name.IndexOf (">");
            
            int moodId = 0;
            
            if(moodPos != -1)
            {
                string moodStr = name.Substring(0, moodPos);
                moodId = int.Parse(moodStr);
                name = name.Substring(moodPos+1);
                restOfText = name;
            }

            return (CharacterHead.Moods)moodId;
        }

        CharacterHead.Moods GetMoodFromString(string moodString)
        {
            CharacterHead.Moods mood = CharacterHead.Moods.Neutral;

            switch(moodString)
            {
                case "Neutral":
                mood = CharacterHead.Moods.Neutral;
                break;

                case "Angry":
                mood = CharacterHead.Moods.Angry;
                break;

                case "Sad":
                mood = CharacterHead.Moods.Sad;
                break;

                case "Happy":
                mood = CharacterHead.Moods.Happy;
                break;

                case "Scared":
                mood = CharacterHead.Moods.Scared;
                break;

                case "Determined":
                mood = CharacterHead.Moods.Determined;
                break;
            }

            return mood;
        }

        protected virtual void SwitchToIdleIfNotAlready()
        {
            // TODO
            Debug.LogWarning("TODO: Implement SwitchToIdleIfNotAlready");
        }
            
        public void SetTalkMode(bool tm)
        {
            if(tm) SwitchToIdleIfNotAlready();
            animator.SetBool("talking", tm);
            
            IsTalking = tm;

            ResetArmAnimationStatus();
        }

        protected virtual void ResetToIdle()
        {
        }
        
        protected virtual IEnumerator RunLipSyncProc(string charName, AudioSource src, AudioClip audioClip, int lineID, string message)
        {
            this.interruptFlag = false;
            IsTalking = true;
            
            string currentMode = "idle";
            LipSyncData lipSyncData = Resources.Load<LipSyncData>("Lipsync/" + charName + "/" + charName + lineID.ToString());

            if(audioClip == null || lipSyncData == null)
            {
                float fakeClipLength = 3f;
                float startTime = Time.time;

                fakeClipLength = message.Length * 0.1f;
                
                // Simulate a talk cycle
                string [] talkCycle = {"AI","AI","O","AI","E", "AI", "O", "U"};
                int i = 0;
                while(Time.time - startTime < fakeClipLength && !interruptFlag)
                {
                    //if(character != null && !character.isTalking) interruptFlag = true;
                    string ph = talkCycle[i % talkCycle.Length];
                    i++;    
                    ShowPhoneme(ph);
                    yield return new WaitForSeconds(.09f);
                }

                ShowPhoneme("MBP");
            }
            else
            {
                VoiceClip voiceClip = new VoiceClip();
                voiceClip.name = audioClip.name;
                voiceClip.text = "text not needed here";
                CharacterHead.Moods mood = CharacterHead.Moods.Neutral;

                SetArmAnimationEnabled(true);

                if(lipSyncData != null)
                {
                    voiceClip.phonemes = GetPhonemesFromLipSyncData(lipSyncData, audioClip.length);
                    voiceClip.audioClip = audioClip;
                    voiceClip.mood = mood;

                    CharacterHead.Moods currentMood = CharacterHead.Moods.Neutral;
                
                    while(src.isPlaying && !interruptFlag)
                    {
                        //if(character != null && !character.isTalking) interruptFlag = true;
                        float time = src.time ;
                    
                        bool change = false;

                        voiceClip.phonemes.GetMode( time /*+ pause*/, ref currentMode, ref change);

                        if(change)
                        {
                            ChangeTalkMode(currentMode);
                        }

                        change = false;

                        voiceClip.phonemes.GetMood( time /*+ pause*/, ref currentMood, ref change);
                        
                        if(change)
                        {
                            ChangeMood( currentMood );
                        }

                        string ph = voiceClip.phonemes.GetPhoneme( time /*+ pause*/ ).Trim ();

                        ShowPhoneme( ph );

                        yield return new WaitForSeconds(1/60f);

                    }
                }
            }

            ChangeTalkMode("idle");

            ShowPhoneme("MBP");

            ChangeMood( CharacterHead.Moods.Neutral );

            IsTalking = false;

            animator.SetBool("talking", false);

            ResetArmAnimationStatus();
           
            yield return null;
        }

        protected virtual void ResetArmAnimationStatus()
        {
            Debug.LogWarning("not implemented");
        }

        protected virtual void SetArmAnimationEnabled(bool b)
        {
            Debug.LogWarning("not implemented");
        }
        
        protected PhonemePositions GetPhonemesFromLipSyncData( LipSyncData data, float audioClipLength )
        {
            PhonemePositions pos = new PhonemePositions();
            
            foreach(var phonemeMarker in data.phonemeData)
            {
                pos.Add( new PhonemePosition( phonemeMarker.time * audioClipLength, phonemes[phonemeMarker.phonemeNumber] ));
            }

            foreach(var emotionMarker in data.emotionData)
            {
                pos.AddMood( new MoodPosition( emotionMarker.startTime * audioClipLength, GetMoodFromString(emotionMarker.emotion) ) );
                pos.AddMood( new MoodPosition( emotionMarker.endTime * audioClipLength, CharacterHead.Moods.Neutral ) );
            }

            foreach(var gestureMarker in data.gestureData)
            {
                pos.AddMode( new ModePosition( gestureMarker.time * audioClipLength, gestureMarker.gesture ) );
            }
        
            return pos;
        }

        public virtual void AnimationEventEndCutScene()
        {
            Debug.LogWarning("TODO: Implement AnimationEventEndCutScene");
        }

        public virtual void AnimationActionPoint(string animationName)
        {
            Debug.LogWarning("TODO: Implement AnimationActionPoint");
        }

        public virtual void AnimationFinished(string animationName)
        {
            Debug.Log("AnimationFinished " + animationName);
            isAnimationPlaying = false;
        }
    }
}