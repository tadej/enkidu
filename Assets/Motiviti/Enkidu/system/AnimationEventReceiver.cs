using UnityEngine;
using System.Collections;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class AnimationEventReceiver : MonoBehaviour
    {
        public void AnimationEventEndCutScene()
        {
            if (PersistentEngine.player) PersistentEngine.player.AnimationEventEndCutScene();
        }

        public void AnimationActionPoint(string animationName)
        {
            if (PersistentEngine.player) PersistentEngine.player.AnimationActionPoint(animationName);
        }

        public void AnimationFinished(string animationName)
        {
            if (PersistentEngine.player) PersistentEngine.player.AnimationFinished(animationName);
        }
    }
}
