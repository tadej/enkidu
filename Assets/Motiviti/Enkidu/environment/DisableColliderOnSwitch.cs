using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class DisableColliderOnSwitch : MonoBehaviour
    {

        public InteractiveItemSwitch.State defaultState;

        public InteractiveItemSwitch iSwitch;

        public Collider2D coll;

        public bool useAnotherItem = false;

        public Collider2D otherCollider;

        void Update()
        {
            if (!enabled)
                return;
            if (iSwitch.state != defaultState)
            {
                enabled = false;
                coll.enabled = false;
                if (useAnotherItem)
                    otherCollider.enabled = true;
            }
        }
    }
}