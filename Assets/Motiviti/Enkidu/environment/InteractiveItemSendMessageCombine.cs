using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class InteractiveItemSendMessageCombine : InteractiveItemCombine
    {
        public GameObject actionReceiver;
        public bool isCancelable = true;

        public InteractiveItemSwitch.State allowOnlyForSwitchState;
        public string commentIfNotAllowed = "";

        InteractiveItemSwitch sw;

        public InventoryItem[] supportedItems;

        void Start()
        {
            base.Initialise();
            sw = GetComponent<InteractiveItemSwitch>();
        }

        public override IEnumerator ProcessArrivedAt()
        {
            interactiveItem.currentCombine = this;

            SendMessage(actionOnArrival, SendMessageOptions.DontRequireReceiver);

            progressCounter = 0;

            Global.player.TurnTowards(interactiveItem);

            Combine();

            yield return null;
        }

        public bool SupportsItem(InventoryItem item)
        {
            bool found = false;
            foreach (InventoryItem i in supportedItems)
            {
                if (i == item) found = true;
            }
            return found;
        }

        protected override void CombineWithItem(InventoryItem item)
        {
            if (!SupportsItem(item)) return;

            string action = "Combine" + item.gameObject.name;
            string actionParameter = null;

            if (sw != null && allowOnlyForSwitchState != InteractiveItemSwitch.State.ANY)
            {
                if (sw.state != allowOnlyForSwitchState)
                {
                    StartCoroutine(Global.player.SpeakProcedure(commentIfNotAllowed));
                    return;
                }
            }

            if (!actionReceiver)
                actionReceiver = gameObject;

            if (actionParameter != null && actionParameter.Length > 0)
                actionReceiver.SendMessage(action, actionParameter, SendMessageOptions.RequireReceiver);
            else
                actionReceiver.SendMessage(action, SendMessageOptions.RequireReceiver);

            if (removeInventoryItemAfterUse) item.Remove();

        }
    }
}

