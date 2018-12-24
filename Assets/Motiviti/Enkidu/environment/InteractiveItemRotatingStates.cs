using UnityEngine;
using System.Collections;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class InteractiveItemRotatingStates : InteractiveItemAction
    {
        [SaveState]
        public int
        state = 0;
        [SaveState]
        public bool
        isLocked = false;
        [SaveState]
        public bool
        onlyOnce = false;
        [SaveState]
        bool
        switchedFlag = false;
        public Renderer[] states;
        [SaveState]
        int progressCounter = 0;
        public string lockedComment;

        void Start()
        {
            base.Initialise();

            ProcessState();
        }

        public void ProcessOnlyOnce(bool b)
        {
            onlyOnce = b;

            if (onlyOnce && state != 0)
            {
                switchedFlag = true;
            }
            SaveState();
        }

        new void Update()
        {
            base.Update();
        }

        public void Unlock()
        {
            isLocked = false;
            SaveState();
        }

        bool Switch()
        {
            state++;

            if (state > states.Length - 1)
                state = 0;

            switchedFlag = true;

            PersistentEngine.SetState(gameObject.name + "_RotatingState", state);

            ProcessState();

            return true;
        }

        void ProcessState()
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (i == state)
                    states[i].GetComponent<Renderer>().enabled = true;
                else
                    states[i].GetComponent<Renderer>().enabled = false;
            }

            SaveState();
        }

        public override void AnimationActionPoint(string animationName)
        {
            progressCounter = 1;
        }

        public override void AnimationFinished(string animationName)
        {
            progressCounter = 2;
        }

        public override IEnumerator ProcessArrivedAt()
        {
            progressCounter = 0;

            PersistentEngine.player.TurnTowards(interactiveItem);

            float time0 = Time.time;

            if (onlyOnce && switchedFlag)
            {

            }
            else
            {
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

                if (isLocked)
                {

                }
                else
                {
                    Switch();
                }

                while (progressCounter == 1)
                {
                    yield return new WaitForSeconds(0.05f);

                    if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                    {
                        Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                        break;
                    }
                }

                PersistentEngine.player.ChangeState(Player.State.IdleDiagonalFront);

                if (isLocked)
                {
                    yield return new WaitForSeconds(0.2f); // TODO: fix

                    PersistentEngine.player.SayItsLocked(lockedComment);
                }


            }
        }
    }
}