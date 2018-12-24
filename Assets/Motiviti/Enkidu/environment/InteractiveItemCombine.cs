using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InteractiveItemCombine : InteractiveItemAction
    {
        [SaveState]
        protected int progressCounter = 0;

        public InventoryItem inventoryItem;

        public string actionOnArrival;

        public string[] animationsToIgnoreActionsFrom;

        public bool removeInventoryItemAfterUse = true;

        new void Update()
        {
            base.Update();
        }

        void Start()
        {
            base.Initialise();
        }

        protected void Combine()
        {
            CombineWithItem(interactiveItem.heldItem);
        }

        protected virtual void CombineWithItem(InventoryItem item)
        {
            Debug.Log("TODO: CombineWithItem not implemented.");
        }

        public override void AnimationActionPoint(string animationName)
        {

            if (IsAnimationAllowed(animationName))
            {
                progressCounter = 1;
            }

        }

        bool IsAnimationAllowed(string name)
        {
            if (animationsToIgnoreActionsFrom == null || animationsToIgnoreActionsFrom.Length <= 0)
                return true;

            foreach (string str in animationsToIgnoreActionsFrom)
            {
                if (str == name) return false;
            }

            return true;
        }

        public override void AnimationFinished(string animationName)
        {
            if (IsAnimationAllowed(animationName))
            {
                progressCounter = 2;
                Debug.Log("combine action finish " + animationName);
            }
        }

        public override IEnumerator ProcessArrivedAt()
        {
            interactiveItem.currentCombine = this;

            if (interactiveItem.heldItem == inventoryItem)
            {
                SendMessage(actionOnArrival, SendMessageOptions.DontRequireReceiver);

                progressCounter = 0;

                PersistentEngine.player.TurnTowards(interactiveItem);

                float time0 = Time.time;

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

                Combine();
                while (progressCounter == 1)
                {
                    yield return new WaitForSeconds(0.05f);

                    if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                    {
                        Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                        break;
                    }
                }
                if (PersistentEngine.player && PersistentEngine.player.gameObject.activeInHierarchy && endState != Player.State.None)
                    PersistentEngine.player.ChangeState(endState);

                yield return new WaitForSeconds(0.4f);
            }
            else
            {
                yield return null;
            }
        }
    }
}