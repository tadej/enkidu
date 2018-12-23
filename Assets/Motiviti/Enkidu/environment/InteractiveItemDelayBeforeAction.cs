using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class InteractiveItemDelayBeforeAction : InteractiveItemAction
    {

        public float delay;
        public int useHowManyTimes;

        [SaveState]
        int used;

        public override IEnumerator ProcessArrivedAt()
        {
            if (useHowManyTimes == 0 || used < useHowManyTimes)
            {
                yield return new WaitForSeconds(delay);
                used++;
            }
        }
    }
}