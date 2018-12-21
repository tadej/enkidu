using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	
	public class SetTimeScale : MonoBehaviour {

		public float timeScale = 1;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			if(!Global.inPause){
			if(timeScale<0)
				timeScale=0;

			Time.timeScale = timeScale;
			Time.fixedDeltaTime = 0.02F * Time.timeScale;
			}
		}
	}
}