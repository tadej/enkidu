﻿using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class InteractiveItemPuzzle : InteractiveItemAction {

		public FullscreenPuzzle puzzle;
		//public GameObject back_shade;
		
		public string commentOnFinish;
		public Player.State actionAnimationOpen = Player.State.PickUpSide;

		public string customActionOnPuzzleSolved;

		[SaveState]
		public int progressCounter = 0;
		public string [] comments;

		//[SaveState]
		public bool allowsDirect=false;

		public bool disableCollider = true;

		[SaveState]
		bool commenting=true;

		[SaveState]
		bool colliderDisabled = false;

		public bool setProgresToZero = true;

		public InteractiveItemSwitch.State enableOnlyForSwitchState = InteractiveItemSwitch.State.ANY;

		InteractiveItemSwitch connectedSwitch;

		public bool disableOnClose = true;

		public bool disableOnFinish = false;

		public bool removeInventoryItemOnFinish = false;

		public InventoryItem itemToRemove;

		public bool stayInCutscene = false;

		public bool doDelay = true;

		public bool doActionAnimationOpenAtTheEnd = true;

		protected AdvCamera advCam;

		protected bool advCamCanMove = false;

		public bool disableInteractiveItemPuzzleOnFinish = false;

		public bool doCloseup = false;

		public Vector3 closeUpOffset;

		public float closeUpSize;

		// Use this for initialization
		void Start () {
			base.Initialise();
		//	back_shade.renderer.sortingOrder=1;
		//	back_shade.renderer.sortingLayerName="Minesweeper";

			advCam = Global.advCamera;
			advCamCanMove = advCam.canMove;

			connectedSwitch = GetComponent<InteractiveItemSwitch>();
			if(colliderDisabled)
				this.GetComponent<Collider2D>().enabled=false;
		}
		
		// Update is called once per frame
		new void Update () {
			base.Update();
		}

		public IEnumerator AnnounceFinished(string msg)
		{
			yield return StartCoroutine (Global.player.SpeakProcedure (msg, Player.TalkMode.Default, false));
		}

		public override IEnumerator ProcessArrivedAt()
		{
			float time0 = Time.time;

			Debug.Log("PuzzleItem ArrivedAt");

			// yield return new WaitForSeconds(1);
			if (connectedSwitch != null && !connectedSwitch.ValidateSwitchState(enableOnlyForSwitchState))
			{
				Debug.Log("Connected switch OFF, " + connectedSwitch);
				yield return null;
			}
			else
								
			if(allowsDirect){

				Debug.Log("Turning towards puzzle object");
			//	yield return new WaitForSeconds(0.05f);
				if(setProgresToZero)
					progressCounter = 0;
				Global.player.TurnTowards(base.interactiveItem);
				Global.player.ChangeState( actionAnimation );
			

				while(progressCounter == 0)
				{
					//Debug.Log("Progress: " + progressCounter);   
					yield return new WaitForSeconds(0.05f);

					if(Time.time - time0 > Global.maxCharacterAnimationLength) 
					{
						Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
						break;
					}
				}
				showPuzzle();
				while (progressCounter == 1)
				{
					//Debug.Log("Progress: " + progressCounter);   
					yield return new WaitForSeconds(0.05f);

					if (Time.time - time0 > Global.maxCharacterAnimationLength)
					{
						Debug.Log("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
						break;
					}
				}
				Debug.Log("Finish " + gameObject.name);
				Global.player.ChangeState(endState);

			}
			yield return null;
		}

		IEnumerator showComments(){
			foreach(string comment in comments){
				if(commenting)
					yield return StartCoroutine(Global.player.SpeakProcedure(comment));
			}
		}

		public void showPuzzle(){
			Global.player.SetInCutScene(true, CutsceneTools.Type.Puzzle);
			Global.player.ChangeState(actionAnimation);
			Global.player.SetDestination(Global.player.transform.position);
			
			if(doCloseup)
				advCam.CloseUpBegin(closeUpSize, closeUpOffset);

			StartCoroutine(showPuzzleProcedure());
		}

		IEnumerator showPuzzleProcedure()
		{
			if(!puzzle.gameObject.activeInHierarchy)puzzle.gameObject.SetActive(true);
			yield return null;
			if (!puzzle.gameObject.activeInHierarchy) puzzle.gameObject.SetActive(true);
			yield return null;
			puzzle.puzzle = this;
			puzzle.ToggleShow(true);
			if (!advCam)
				advCam = Global.advCamera;
			
			advCam.CanCamMove(false);
			StartCoroutine(showComments());

		}

		public void PuzzleClosed(){
			puzzle.ToggleShow(false);
			if(!advCam)
				advCam = Global.advCamera;
			advCam.CanCamMove(advCamCanMove);

			if(disableOnClose)
				puzzle.gameObject.SetActive(false);
			Global.player.SetInCutScene(false);
			StartCoroutine(close());

			Global.player.StopTalking ();
		}

		public void PuzzleFinished(){

			if (!advCam)
				advCam = Global.advCamera;
			advCam.CanCamMove(advCamCanMove);
			puzzle.ToggleShow(false);
			if(disableOnFinish)
				puzzle.gameObject.SetActive(false);
			if(disableCollider){
				transform.GetComponent<Collider2D>().enabled=false;
				colliderDisabled = true;
			}

			if (disableInteractiveItemPuzzleOnFinish) this.enabled = false;

			StartCoroutine(finish());
			if(removeInventoryItemOnFinish && itemToRemove!=null){
				itemToRemove.Remove();
			}
	
			gameObject.SendMessage("PuzzleCompleted", SendMessageOptions.DontRequireReceiver);
		}

		public override void AnimationActionPoint(string animationName)
		{
			progressCounter = 1;
		}
		
		public override void AnimationFinished(string animationName)
		{
			progressCounter = 2;
		}

		IEnumerator finish(){
			progressCounter = 0;

			Global.player.ChangeState( actionAnimationOpen );

			if (doCloseup)
				advCam.CloseUpEnd();

			Global.player.SetTargetItem(base.interactiveItem);
			commenting=false;
		
			int i=0;
			float time0 = Time.time;

			while(progressCounter == 0)
			{
				yield return new WaitForSeconds(0.05f);
				if(i>20)
					progressCounter=2;

				i++;

				if(Time.time - time0 > Global.maxCharacterAnimationLength) 
				{
					Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
					break;
				}
			}
			yield return new WaitForSeconds(0.1f);


			if (string.IsNullOrEmpty (customActionOnPuzzleSolved) == false) {
				gameObject.SendMessage (customActionOnPuzzleSolved, SendMessageOptions.DontRequireReceiver);
			}

			gameObject.SendMessage ("SwitchOn",  SendMessageOptions.DontRequireReceiver);

			while(progressCounter == 1)
			{
				yield return new WaitForSeconds(0.05f);

				if(Time.time - time0 > Global.maxCharacterAnimationLength) 
				{
					Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
					break;
				}
			}

			if(doDelay)
				yield return new WaitForSeconds(0.2f);
			Global.player.ChangeState(endState);
			yield return new WaitForSeconds(0.2f);

			if (!string.IsNullOrEmpty(commentOnFinish)){
				yield return StartCoroutine(Global.player.SpeakProcedure(commentOnFinish));
			}
			
			StartCoroutine(ElroySmile());

			yield return new WaitForSeconds(0.3f);

			if(!stayInCutscene)
				Global.player.SetInCutScene(false);
		}

		IEnumerator ElroySmile(){
			if(Global.player.state == Player.State.IdleDiagonalFront){
				Global.player.ChangeState(Player.State.IdleDiagonalFrontSmile);
				yield return new WaitForSeconds(1.5f);
				if(Global.player.state == Player.State.IdleDiagonalFrontSmile){
					Debug.Log("elroy state: " + Global.player.state+ " front smile: " + Player.State.IdleDiagonalFrontSmile);  
					Global.player.ChangeState(Player.State.IdleDiagonalFront);
				}
				else{
					Debug.Log("razlicni stati elroy state: " + Global.player.state+ " front smile: " + Player.State.IdleDiagonalFrontSmile);  
				}
			}
		}

		IEnumerator close(){
			Debug.Log ("Closing " + gameObject.name);
			progressCounter = 0;
			commenting=false;

			if(doActionAnimationOpenAtTheEnd)
				Global.player.ChangeState( actionAnimationOpen );

			if (doCloseup)
				advCam.CloseUpEnd();
		
			Global.player.SetTargetItem(base.interactiveItem);
			float currentTime = Time.time;


			while(doActionAnimationOpenAtTheEnd && progressCounter == 0)
			{
				yield return new WaitForSeconds(0.05f);
				if(Time.time-currentTime>1){
					progressCounter = 2;
				}
			}
			
			while(doActionAnimationOpenAtTheEnd && progressCounter == 1)
			{
				yield return new WaitForSeconds(0.05f);
			}

			if(doActionAnimationOpenAtTheEnd)yield return new WaitForSeconds(0.1f);

			Global.player.ChangeState(endState);
			Global.player.SetTargetItem(null, true);
			yield return new WaitForSeconds(0.3f);
			
		}
	}
}