using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	
        public class InteractiveItemGive : InteractiveItemCombine {

        public string action = "JumpToConversationEntry";
        public string actionParameter = "0";
        public GameObject actionReceiver;
        public bool isCancelable = true;

        public InteractiveItemSwitch.State allowOnlyForSwitchState;
        public string commentIfNotAllowed = "";

        bool returnToEndState = false;

        InteractiveItemSwitch sw;

        void Start()
        {
            base.Initialise();
            sw = GetComponent<InteractiveItemSwitch>();
        }

        protected override void CombineWithItem(InventoryItem item)
        {
            if(sw != null && allowOnlyForSwitchState != InteractiveItemSwitch.State.ANY)
            {
                if(sw.state != allowOnlyForSwitchState) 
                {
                    StartCoroutine (Global.player.SpeakProcedure(commentIfNotAllowed));
                    return;
                }
            }

            if(item == inventoryItem)
            {
                if (!actionReceiver)
                    actionReceiver = gameObject;

                //Debug.Log("Sending message " + action + " with parameter " + actionParameter + " to " + actionReceiver.name);

                if(actionParameter.Length > 0)
                    actionReceiver.SendMessage(action, actionParameter, SendMessageOptions.RequireReceiver);
                else
                    actionReceiver.SendMessage(action, SendMessageOptions.RequireReceiver);

                if (removeInventoryItemAfterUse) item.Remove();
            }
        }

        public override IEnumerator ProcessArrivedAt()
        {
            interactiveItem.currentCombine = this;

            if (interactiveItem.heldItem == inventoryItem)
            {
                SendMessage(actionOnArrival, SendMessageOptions.DontRequireReceiver);

                progressCounter = 0;

                Global.player.TurnTowards(interactiveItem);

                float time0 = Time.time;

                Global.player.ChangeState(actionAnimation);

                while (progressCounter == 0)
                {
                    yield return new WaitForSeconds(0.05f);

                    if (Time.time - time0 > Global.maxCharacterAnimationLength)
                    {
                        Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                        break;
                    }

                }

                Combine();
                while (progressCounter == 1)
                {
                    yield return new WaitForSeconds(0.05f);

                    if (Time.time - time0 > Global.maxCharacterAnimationLength)
                    {
                        Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                        break;
                    }
                }

                if (returnToEndState)
                {
                    if (Global.player && Global.player.gameObject.activeInHierarchy)
                        Global.player.ChangeState(endState);
                
                    yield return new WaitForSeconds(0.4f);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

}
