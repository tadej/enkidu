using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
        
    public class CameraAnimator_eventReceiverCam : MonoBehaviour {

        // Use this for initialization
        InteractiveItemChangeCamera changeCam;
        AdvCamera advCamera;

        bool animating = false;

        public Camera cam;

        Animator animator;

        Player elroy;

        public bool animateAdvCam = true;

        public bool disableAnimatorOnStart = true;

        public bool disableAnimatorOnFinish = true;

        float advCamStartSize = 4;

        void Start () {
            advCamera = Global.advCamera;
            if(!cam)
                cam = GetComponentInChildren<Camera>();
            elroy = Global.player;

            animator = GetComponent<Animator>();
            if(disableAnimatorOnStart)
                animator.enabled = false;
            cam.enabled = false;

        }
        
        // Update is called once per frame
        void Update () {
            if (animating && animateAdvCam)
            {
                advCamera.transform.position = cam.transform.position;
                if(cam)
                    advCamera.SetNewCameraSize(cam.orthographicSize);
                if (elroy && elroy.IsWalking())
                {
                    Debug.Log("elroy walking");
                    animating = false;
                    Global.activeCamera = advCamera.GetComponent<Camera>();
                    advCamera.transform.position = cam.transform.position;
                    advCamera.GetComponent<Camera>().enabled = true;
                    advCamera.forcedCameraBoundaries = true;
                    advCamera.inCloseUp = false;
                    advCamera.followCameraObject = false;
                    advCamera.canZoom = true;
                    advCamera.SetNewCameraSize(advCamStartSize);
                    cam.enabled = false;
                }
            }
        
        }

        public void setChangeCam(InteractiveItemChangeCamera change)
        {
            changeCam = change;
        }

        public void AnimFinished()
        {
            AnimationFinished();
        }



        public void AnimationFinished()
        {
            Debug.Log (advCamera.GetComponent<Camera>());
            if (animating)
            {
                if (advCamera)
                {
                    Global.activeCamera = advCamera.GetComponent<Camera>();
                    if (animateAdvCam)
                        advCamera.transform.position = cam.transform.position;
                    advCamera.GetComponent<Camera>().enabled = true;
                    advCamera.forcedCameraBoundaries = true;
                    advCamera.inCloseUp = false;
                    advCamera.followCameraObject = false;
                    advCamera.canZoom = true;
                    advCamera.SetNewCameraSize(advCamStartSize);
                }
                cam.enabled = false;
            }
            if (changeCam)
                changeCam.AnimatingCameraFinished ();
            else if(disableAnimatorOnFinish)
                animator.enabled = false;
            animating = false;
        }

        public void AnimationStarted()
        {
            Global.activeCamera = cam;
            if (advCamera)
            {
                advCamStartSize = advCamera.cameraSize;
                advCamera.GetComponent<Camera>().enabled = false;
                advCamera.forcedCameraBoundaries = false;
                advCamera.followCameraObject = true;
                advCamera.inCloseUp = false;
                advCamera.canZoom = false;
            }
            cam.enabled = true;
            animating = true;
        }
    }
}