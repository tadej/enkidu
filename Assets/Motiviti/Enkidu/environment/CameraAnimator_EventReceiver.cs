using UnityEngine;
using System.Collections;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	public class CameraAnimator_EventReceiver : MonoBehaviour 
    {
        public InteractiveItemControlCamera CameraControler;
		
        public void AnimationFinished(){
			CameraControler.AnimatingCameraFinished ();
		}
	}

}