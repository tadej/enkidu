using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InteractiveItemPopup : InteractiveItemAction
    {

        public GameObject back_shade;
        public GameObject to_show;

        public bool onlyOnce = false;

        public bool inCutSceneMode = true;

        bool cameraCanMove = false;

        public GameObject messageTarget;
        public string messageOnClose;
        public string messageOnOpen;

        CustomCursor customCursor;

        [SaveState]
        bool usedFlag = false;

        public bool showOnArrivedAt = true;

        bool showing = false;

        public bool IsShowing()
        {
            return showing;
        }

        void Start()
        {
            base.Initialise();
            back_shade.GetComponent<Renderer>().sortingOrder = 0;
            back_shade.GetComponent<Renderer>().sortingLayerName = "Puzzle";
            if (customCursor == null)
            {
                customCursor = Global.customCursor;
                if (customCursor)
                    customCursor.AddToPuzzleList(this.gameObject);
            }
        }

        new void Update()
        {
            base.Update();

            if (showing)
            {
                if (inCutSceneMode)
                {
                    Global.player.inCutScene = true;
                }
            }
        }

        public override IEnumerator ProcessArrivedAt()
        {
            if (onlyOnce && usedFlag)
            {

            }
            else
            {
                if (showOnArrivedAt) ShowPopup();
                yield return null;
            }
        }

        public void ShowPopup()
        {
            cameraCanMove = Global.advCamera.canMove;
            Global.advCamera.canMove = false;

            if (inCutSceneMode)
            {
                Global.player.SetInCutScene(true);
            }
            else
            {
                Global.player.staticCharacter = true;
            }

            to_show.SetActive(true);
            showing = true;
            SaveState();

            if (messageTarget != null && string.IsNullOrEmpty(messageOnOpen) == false)
            {
                messageTarget.SendMessage(messageOnOpen);
            }

            if (back_shade) back_shade.SetActive(true);
        }

        public IEnumerator removePopup()
        {

            if (showing) Global.advCamera.canMove = cameraCanMove;

            showing = false;
            to_show.SetActive(false);


            if (back_shade) back_shade.SetActive(false);

            yield return new WaitForSeconds(0.0f);

            if (inCutSceneMode)
            {
                Global.player.SetInCutScene(false);
            }
            else
            {
                Global.player.staticCharacter = false;
            }

            SaveState();

            if (messageTarget != null && string.IsNullOrEmpty(messageOnClose) == false)
            {
                messageTarget.SendMessage(messageOnClose);
            }
        }

        IEnumerator CloseClickedProc(bool allowExitCutscene = true)
        {
            yield return new WaitForSeconds(0.1f);
            Global.player.SetInCutScene(true);
            Global.player.SetTargetItem(null, true);
            yield return StartCoroutine(removePopup());
            if (allowExitCutscene) Global.player.SetInCutScene(false);
        }

        public void CloseClickedGUI()
        {
            CloseClickedGUI(true);
        }
        public void CloseClickedGUI(bool allowExitCutscene = true)
        {
            StartCoroutine(CloseClickedProc(allowExitCutscene));
        }
    }
}