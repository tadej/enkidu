using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InteractiveItemCustomCombine : InteractiveItemCombine
    {

        public string action;
        public string actionParameter;
        public GameObject actionReceiver;
        public bool isCancelable = true;

        public InteractiveItemSwitch.State allowOnlyForSwitchState;
        public string commentIfNotAllowed = "";

        InteractiveItemSwitch sw;

        void Start()
        {
            base.Initialise();
            sw = GetComponent<InteractiveItemSwitch>();
        }

        protected override void CombineWithItem(InventoryItem item)
        {
            if (sw != null && allowOnlyForSwitchState != InteractiveItemSwitch.State.ANY)
            {
                if (sw.state != allowOnlyForSwitchState)
                {
                    StartCoroutine(Global.player.SpeakProcedure(commentIfNotAllowed));
                    return;
                }
            }

            if (item == inventoryItem)
            {
                if (actionReceiver == null)
                    actionReceiver = gameObject;

                if (actionParameter != null && actionParameter.Length > 0)
                    actionReceiver.SendMessage(action, actionParameter, SendMessageOptions.RequireReceiver);
                else
                    actionReceiver.SendMessage(action, SendMessageOptions.RequireReceiver);

                if (removeInventoryItemAfterUse) item.Remove();
            }
        }
    }

}
