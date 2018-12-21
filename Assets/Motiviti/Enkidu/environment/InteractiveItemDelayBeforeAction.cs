using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class InteractiveItemDelayBeforeAction : InteractiveItemAction {

        public float delay;
        public int useHowManyTimes;

        [SaveState]
        int used;
        // Use this for initialization
        void Start () {
        
        }
        
        // Update is called once per frame
        new void Update () {
        
        }

        public override IEnumerator ProcessArrivedAt()
        {
            if (useHowManyTimes == 0 || used < useHowManyTimes)
            {
                Debug.Log("starting to wait");
                yield return new WaitForSeconds(delay);
                Debug.Log("end to wait");
                used++;
            }
        }
    }
}