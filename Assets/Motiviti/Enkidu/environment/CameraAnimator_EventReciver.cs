using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class CameraAnimator_EventReciver : MonoBehaviour {

		public InteractiveItemControlCamera CameraControler;
		// Use this for initialization

		void Start () {
		
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void AnimationFinished(){
			CameraControler.AnimatingCameraFinished ();
		}
	}

}