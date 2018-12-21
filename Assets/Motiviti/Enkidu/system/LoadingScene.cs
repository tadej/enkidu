using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class LoadingScene : StatefulItem {
		
		AsyncOperation async;

		// Use this for initialization
		IEnumerator Start () {
			Application.backgroundLoadingPriority = ThreadPriority.High;

			int? level = Global.GetState("Global loadingLevel");

			Debug.Log("LoadingScene: level " + level.ToString());

			SceneManager.LoadScene((int)level);

			yield return null;
		}
		
		// Update is called once per frame
		void Update () {

		}


	}
}
