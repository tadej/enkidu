using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class SendMessageWhenPickedUp : MonoBehaviour
    {

        public GameObject target;
        public string message;

        public void PickedUp()
        {
            if (target != null) target.SendMessage(message, SendMessageOptions.DontRequireReceiver);
        }
    }
}