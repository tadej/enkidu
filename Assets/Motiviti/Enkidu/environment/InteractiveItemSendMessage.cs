using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InteractiveItemSendMessage : InteractiveItemAction
    {
        public GameObject receiver;
        public string message;

        public bool actOnSwitchOn = false;

        public bool canBeUsed = true;

        public bool immediateActionWithoutAnimation = true;

        [SaveState]
        int progressCounter = 0;

        void Start()
        {
            base.Initialise();
        }

        new void Update()
        {
            base.Update();
        }

        public void sendMessagetoReciver()
        {
            if (receiver) receiver.SendMessage(message);
        }

        public void SwitchedOn()
        {
            if (actOnSwitchOn == true)
                sendMessagetoReciver();
        }

        public override void AnimationActionPoint(string animationName)
        {
            progressCounter = 1;
            SaveState();
        }

        public override void AnimationFinished(string animationName)
        {
            progressCounter = 2;
            SaveState();
        }

        public override IEnumerator ProcessArrivedAt()
        {
            if (!canBeUsed)
            {

            }
            else
            {
                if (immediateActionWithoutAnimation)
                {
                    sendMessagetoReciver();
                }
                else
                {
                    progressCounter = 0;

                    if (interactiveItem.desiredDirection != 0)
                        PersistentEngine.player.TurnTowards(interactiveItem.desiredDirection);
                    else
                        PersistentEngine.player.TurnTowards(interactiveItem);

                    PersistentEngine.player.ChangeState(actionAnimation);

                    float time0 = Time.time;

                    while (progressCounter == 0)
                    {
                        yield return new WaitForSeconds(0.05f);

                        if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                        {
                            Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                            break;
                        }
                    }

                    if (canBeUsed)
                        sendMessagetoReciver();

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

                    SaveState();
                }
            }

        }
    }
}