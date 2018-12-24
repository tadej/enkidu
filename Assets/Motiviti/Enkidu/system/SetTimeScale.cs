using UnityEngine;

namespace Motiviti.Enkidu
{
    public class SetTimeScale : MonoBehaviour
    {
        public float timeScale = 1;

        void Update()
        {
            if (!PersistentEngine.inPause)
            {
                if (timeScale < 0)
                    timeScale = 0;

                Time.timeScale = timeScale;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
            }
        }
    }
}