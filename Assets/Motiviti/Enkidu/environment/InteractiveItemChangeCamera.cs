using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
        
    public class InteractiveItemChangeCamera : InteractiveItemAction{

        public bool doFollow = true;
        Player elroy;
        public Animator animator;
        InteractiveItem interactiveItemMain;

        CameraAnimator_eventReceiverCam eventReceiver;

        public InventoryItem useOnCombineWith;

        bool stayInCutsceneBefore, realyStayInCutsceneBefore;

        public int useHowManyTimes = 0;

        [SaveState]
        int used = 0;

        // Use this for initialization
        void Start () {
            base.Initialise();
            elroy = Global.player;
            interactiveItemMain = gameObject.GetComponent<InteractiveItem>();
            if (animator == null)
                animator = gameObject.GetComponent<Animator>();
            eventReceiver = animator.transform.GetComponent<CameraAnimator_eventReceiverCam>();
            eventReceiver.setChangeCam(this);
            
        }

        public int GetUsed()
        {
            return used;
        }
        
        // Update is called once per frame
        new void Update()
        {
            base.Update();
        }

        public override IEnumerator ProcessArrivedAt()
        {
        Debug.Log("control camera arrved at");
            if (doFollow && (useHowManyTimes == 0 || used < useHowManyTimes) && ((useOnCombineWith != null && useOnCombineWith == interactiveItemMain.heldItem) || useOnCombineWith == null))
            {
                stayInCutsceneBefore = interactiveItemMain.stayInCutscene;
                realyStayInCutsceneBefore = interactiveItemMain.realyStayInCutscene;
                interactiveItemMain.stayInCutscene = true;
                interactiveItemMain.realyStayInCutscene = true;
                animator.enabled = true;
                yield return null;
                used++;
                SaveState();
            }
            yield return null;
        }

        public void AnimatingCameraFinished()
        {
            interactiveItemMain.stayInCutscene = stayInCutsceneBefore;
            interactiveItemMain.realyStayInCutscene = realyStayInCutsceneBefore;
            animator.enabled = false;
            if(interactiveItemMain.realyStayInCutscene == false)
            elroy.SetInCutScene(false);
        }
    }
}