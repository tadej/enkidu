using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InteractiveItemControlCamera : InteractiveItemAction
    {

        public Transform cameraFollowObject;
        public bool doFollow;
        AdvCamera advCamera;
        Player elroy;
        public Animator animator;
        InteractiveItem interactiveItemMain;

        bool stayInCutsceneBefore, realyStayInCutsceneBefore;

        public InventoryItem useOnCombineWith;

        public int useHowManyTimes = 0;

        [SaveState]
        int used = 0;

        void Start()
        {
            base.Initialise();
            advCamera = PersistentEngine.advCamera;
            elroy = PersistentEngine.player;
            interactiveItemMain = gameObject.GetComponent<InteractiveItem>();
            if (animator == null)
                animator = gameObject.GetComponent<Animator>();
        }

        new void Update()
        {
            base.Update();
        }

        public override IEnumerator ProcessArrivedAt()
        {
            if (doFollow && (useHowManyTimes == 0 || used < useHowManyTimes) && ((useOnCombineWith != null && useOnCombineWith == interactiveItemMain.heldItem) || useOnCombineWith == null))
            {
                stayInCutsceneBefore = interactiveItemMain.stayInCutscene;
                realyStayInCutsceneBefore = interactiveItemMain.realyStayInCutscene;
                interactiveItemMain.stayInCutscene = true;
                interactiveItemMain.realyStayInCutscene = true;
                Debug.Log("do foollow");
                animator.enabled = true;
                advCamera.SetFollowObjectMode(true, cameraFollowObject, 1000);
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
            advCamera.SetFollowObjectMode(false, cameraFollowObject, 100);
            elroy.SetInCutScene(false);
        }
    }
}