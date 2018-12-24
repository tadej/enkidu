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
                customCursor = PersistentEngine.customCursor;
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
                    PersistentEngine.player.inCutScene = true;
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
            cameraCanMove = PersistentEngine.advCamera.canMove;
            PersistentEngine.advCamera.canMove = false;

            if (inCutSceneMode)
            {
                PersistentEngine.player.SetInCutScene(true);
            }
            else
            {
                PersistentEngine.player.staticCharacter = true;
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

            if (showing) PersistentEngine.advCamera.canMove = cameraCanMove;

            showing = false;
            to_show.SetActive(false);


            if (back_shade) back_shade.SetActive(false);

            yield return new WaitForSeconds(0.0f);

            if (inCutSceneMode)
            {
                PersistentEngine.player.SetInCutScene(false);
            }
            else
            {
                PersistentEngine.player.staticCharacter = false;
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
            PersistentEngine.player.SetInCutScene(true);
            PersistentEngine.player.SetTargetItem(null, true);
            yield return StartCoroutine(removePopup());
            if (allowExitCutscene) PersistentEngine.player.SetInCutScene(false);
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