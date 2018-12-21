using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class InteractiveItemPuzzleInInventory : InteractiveItemAction
	{

			public FullscreenPuzzle puzzle;
			[SaveState]
			bool
					usedFlag = false;
			public Player.State actionAnimationOpen = Player.State.PickUpSide;
			[SaveState]
			int
					progressCounter = 0;
			public SpriteRenderer finishedSprite;
			public SpriteRenderer finishedSpriteOverlay;
			InventoryItem inventoryItem;

			// Use this for initialization
			void Start ()
			{
					base.Initialise ();

					inventoryItem = GetComponent<InventoryItem> ();

					puzzle.gameObject.SetActive (true);
			}
		
			// Update is called once per frame
			new void Update ()
			{
					base.Update ();
			}

			public void showPuzzle ()
			{
					Global.FlurryLog ("InteractiveItemPuzzle", gameObject.name, "Show", "in-inventory");

					if (!usedFlag) {
							usedFlag = true;
							Global.player.SetInCutScene (true, CutsceneTools.Type.Puzzle);
							puzzle.ToggleShow (true);
							SaveState ();
					}
			}

			public void PickedUp ()
			{
					showPuzzle ();
			}

			public void PuzzleClosed ()
			{
					Global.FlurryLog ("InteractiveItemPuzzle", gameObject.name, "Closed", "in-inventory");
					Debug.Log ("puzzle closed");
					puzzle.ToggleShow (false);

					Global.player.SetInCutScene (false);
					StartCoroutine (close ());
					//	gameMenu.setPauseButtonVisible(true);
					Global.player.StopTalking ();
			}

			public void PuzzleFinished ()
			{
					Global.FlurryLog ("InteractiveItemPuzzle", gameObject.name, "Finished", "in-inventory");

					puzzle.ToggleShow (false);
					Global.player.SetInCutScene (false, CutsceneTools.Type.None);

					//transform.collider2D.enabled=false;

					SpriteRenderer tmp = inventoryItem.inventorySprite;

					inventoryItem.inventorySprite.enabled = false;

					inventoryItem.inventorySprite = finishedSprite;

					inventoryItem.highlightBorderSprite = finishedSpriteOverlay;
					//	gameMenu.setPauseButtonVisible(true);
					tmp.enabled = false;
					finishedSprite.GetComponent<Renderer>().enabled = true;

					StartCoroutine (finish ());

					SaveState ();
					//gameObject.SetActive(false);
			}

			public void FinishedInStart ()
			{
					inventoryItem.inventorySprite.enabled = false;

					inventoryItem.inventorySprite = finishedSprite;

					inventoryItem.highlightBorderSprite = finishedSpriteOverlay;

					finishedSprite.GetComponent<Renderer>().enabled = true;
			}

			public override void AnimationActionPoint (string animationName)
			{
					Debug.Log ("animation action point");
					progressCounter = 1;
			}
		
			public override void AnimationFinished (string animationName)
			{
					Debug.Log ("animation finish point");
					progressCounter = 2;
			}

			IEnumerator finish ()
			{
					/*	progressCounter = 0;

			Global.elroy.ChangeState( actionAnimationOpen );
			float time0 = Time.time;
			while(progressCounter == 0)
			{
				yield return new WaitForSeconds(0.05f);
				if(Time.time - time0 > Global.maxCharacterAnimationLength) 
				{
					Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
					break;
				}
			}


			while(progressCounter == 1)
			{
				yield return new WaitForSeconds(0.05f);
				if(Time.time - time0 > Global.maxCharacterAnimationLength) 
				{
					Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
					break;
				}
			}

			yield return new WaitForSeconds(0.4f);*/
					yield return new WaitForSeconds (0.05f);
					Global.player.ChangeState (Player.State.IdleFront);

					SaveState ();
			}

			IEnumerator close ()
			{
					progressCounter = 0;
					Global.player.ChangeState (actionAnimationOpen);
					Global.player.SetTargetItem (base.interactiveItem);
					float time0 = Time.time;
					float currentTime = Time.time;
					while (progressCounter == 0) {
							yield return new WaitForSeconds (0.05f);
							if (Time.time - currentTime > 1) {
									progressCounter = 2;
							}

							if (Time.time - time0 > Global.maxCharacterAnimationLength) {
									Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
									break;
							}
					}
			
					while (progressCounter == 1) {
							yield return new WaitForSeconds (0.05f);

							if (Time.time - time0 > Global.maxCharacterAnimationLength) {
									Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
									break;
							}
					}
					yield return new WaitForSeconds (0.4f);
					yield return null;
					Global.player.ChangeState (endState);
			
			}
	}
}