using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class InteractiveItemPuzzleInInventory : InteractiveItemAction
    {
        public FullscreenPuzzle puzzle;
        [SaveState]
        bool usedFlag = false;
        public Player.State actionAnimationOpen = Player.State.PickUpSide;
        [SaveState]
        int progressCounter = 0;
        public SpriteRenderer finishedSprite;
        public SpriteRenderer finishedSpriteOverlay;
        InventoryItem inventoryItem;

        void Start()
        {
            base.Initialise();

            inventoryItem = GetComponent<InventoryItem>();

            puzzle.gameObject.SetActive(true);
        }

        new void Update()
        {
            base.Update();
        }

        public void showPuzzle()
        {
            if (!usedFlag)
            {
                usedFlag = true;
                PersistentEngine.player.SetInCutScene(true, CutsceneTools.Type.Puzzle);
                puzzle.ToggleShow(true);
                SaveState();
            }
        }

        public void PickedUp()
        {
            showPuzzle();
        }

        public void PuzzleClosed()
        {
            puzzle.ToggleShow(false);

            PersistentEngine.player.SetInCutScene(false);
            StartCoroutine(Close());
            PersistentEngine.player.StopTalking();
        }

        public void PuzzleFinished()
        {

            puzzle.ToggleShow(false);
            PersistentEngine.player.SetInCutScene(false, CutsceneTools.Type.None);

            SpriteRenderer tmp = inventoryItem.inventorySprite;

            inventoryItem.inventorySprite.enabled = false;

            inventoryItem.inventorySprite = finishedSprite;

            inventoryItem.highlightBorderSprite = finishedSpriteOverlay;
            tmp.enabled = false;
            finishedSprite.GetComponent<Renderer>().enabled = true;

            StartCoroutine(Finish());

            SaveState();
        }

        public void FinishedInStart()
        {
            inventoryItem.inventorySprite.enabled = false;

            inventoryItem.inventorySprite = finishedSprite;

            inventoryItem.highlightBorderSprite = finishedSpriteOverlay;

            finishedSprite.GetComponent<Renderer>().enabled = true;
        }

        public override void AnimationActionPoint(string animationName)
        {
            progressCounter = 1;
        }

        public override void AnimationFinished(string animationName)
        {
            progressCounter = 2;
        }

        IEnumerator Finish()
        {
            yield return new WaitForSeconds(0.05f);
            PersistentEngine.player.ChangeState(Player.State.IdleFront);

            SaveState();
        }

        IEnumerator Close()
        {
            progressCounter = 0;
            PersistentEngine.player.ChangeState(actionAnimationOpen);
            PersistentEngine.player.SetTargetItem(base.interactiveItem);
            float time0 = Time.time;
            float currentTime = Time.time;
            while (progressCounter == 0)
            {
                yield return new WaitForSeconds(0.05f);
                if (Time.time - currentTime > 1)
                {
                    progressCounter = 2;
                }

                if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                {
                    Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                    break;
                }
            }

            while (progressCounter == 1)
            {
                yield return new WaitForSeconds(0.05f);

                if (Time.time - time0 > PersistentEngine.maxCharacterAnimationLength)
                {
                    Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
                    break;
                }
            }
            yield return new WaitForSeconds(0.4f);
            yield return null;
            PersistentEngine.player.ChangeState(endState);

        }
    }
}