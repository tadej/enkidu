using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InteractiveItemNextLevel : InteractiveItemAction
    {

        int progressCounter = 0;
      
        public override void AnimationActionPoint(string animationName)
        {
            Debug.Log("animation action point");
            progressCounter = 1;
        }

        public override void AnimationFinished(string animationName)
        {
            Debug.Log("animation finish point");
            progressCounter = 2;
        }

        public override IEnumerator ProcessArrivedAt()
        {
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
            StartCoroutine(Global.player.NextLevel());


            yield return null;
        }
    }
}