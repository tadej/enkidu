using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class SendMessageToTarget : MonoBehaviour
    {

        public GameObject target;

        public void Send(string message)
        {
            if (target) target.SendMessage(message, SendMessageOptions.DontRequireReceiver);
        }

    }
}