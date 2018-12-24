using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Motiviti.Enkidu
{
    public class LoadingScene : StatefulItem
    {
        AsyncOperation async;

        IEnumerator Start()
        {
            Application.backgroundLoadingPriority = ThreadPriority.High;

            int? level = PersistentEngine.GetState("Global loadingLevel");

            Debug.Log("LoadingScene: level " + level.ToString());

            SceneManager.LoadScene((int)level);

            yield return null;
        }
    }
}
