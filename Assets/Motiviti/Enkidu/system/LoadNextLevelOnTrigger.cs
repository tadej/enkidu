using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class LoadNextLevelOnTrigger : MonoBehaviour
    {

        bool loadingAlready = false;

        Player elroy;

        void Start()
        {
            elroy = Global.player;
        }

        public void Trigger()
        {
            if (!loadingAlready)
                StartCoroutine(NextLevel());

            loadingAlready = true;
        }
        IEnumerator NextLevel()
        {
            if (elroy)
                StartCoroutine(elroy.NextLevel());
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            yield return null;

        }
    }
}