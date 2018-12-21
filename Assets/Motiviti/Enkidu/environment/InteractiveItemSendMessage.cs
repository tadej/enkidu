using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
        
    public class InteractiveItemSendMessage : InteractiveItemAction {

        public GameObject reciever;
        public string message;

        public bool actOnSwitchOn = false;

        public bool canBeUsed = true;

        public bool immediateActionWithoutAnimation = true;

        [SaveState]
        int progressCounter = 0;

        // Use this for initialization
        void Start () {
            base.Initialise();
        }
        
        // Update is called once per frame
        new void Update () {
            base.Update();
        }

        public void sendMessagetoReciver(){
            if(reciever)reciever.SendMessage(message);
        }

        public void SwitchedOn()
        {
            if(actOnSwitchOn == true)
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
            Debug.Log("arrived at send message");
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
                        Global.player.TurnTowards(interactiveItem.desiredDirection);
                    else
                        Global.player.TurnTowards(interactiveItem);

                    Global.player.ChangeState(actionAnimation);

                    float time0 = Time.time;

                    while (progressCounter == 0)
                    {
                        yield return new WaitForSeconds(0.05f);

                        if (Time.time - time0 > Global.maxCharacterAnimationLength)
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

                        if (Time.time - time0 > Global.maxCharacterAnimationLength)
                        {
                            Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                            break;
                        }
                    }

                    //yield return new WaitForSeconds(0.4f);

                    Global.player.ChangeState(endState);

                    SaveState();
                }
            }

        }
    }
}