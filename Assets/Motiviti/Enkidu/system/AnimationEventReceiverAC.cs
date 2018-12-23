using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class AnimationEventReceiverAC : MonoBehaviour
    {
        public CharacterBrain brain;
        // Use this for initialization
        public void AnimationEventEndCutScene()
        {
            if (brain) brain.AnimationEventEndCutScene();
        }

        public void AnimationActionPoint(string animationName)
        {
            if (brain) brain.AnimationActionPoint(animationName);
        }

        public void AnimationFinished(string animationName)
        {
            if (brain) brain.AnimationFinished(animationName);
        }
    }
}