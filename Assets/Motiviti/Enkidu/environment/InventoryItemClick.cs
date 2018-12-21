using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class InventoryItemClick : InteractiveItemPuzzle {

		Inventory inventory;
		public SpriteRenderer spriteInInventory;
		public string messageSendingToPuzzle;
		public string messageSendingToPuzzleOnCombine;
		public string colliderOnWhichToCheck;
		public string heldItemName;

		public bool showInventory = true;

		bool sliding=false;
		
		bool startSliding=false;

		bool stopSliding = false;

		InventoryItem heldItem = null;

		Vector3 holdPosition;

		int used = 0;

		bool isEnabled = false;

		// Use this for initialization
		void Start () {
			inventory = Global.inventory;

			advCam = Global.advCamera;
			advCamCanMove = advCam.canMove;
		}
		
		// Update is called once per frame
		new void Update () {
			if(isEnabled){
			Global.player.ProcessInventoryInteraction();
			foreach (Touch touch in Input.touches) //start sliding
			{			
				if(touch.phase == TouchPhase.Began)
				{
					sliding = true;
					startSliding=true;
					stopSliding = false; 
					holdPosition = Input.mousePosition;
				}
				else if(touch.phase == TouchPhase.Ended)
				{
					sliding = false;
					stopSliding = true; 
				}
				else if(touch.phase == TouchPhase.Moved)
				{
					holdPosition = Input.mousePosition;
				}
			}
			if(Input.GetButtonDown ("Fire1"))
			{
				if(sliding==false)
					startSliding=true;
				sliding = true;
				stopSliding = false; 
				holdPosition = Input.mousePosition;
			}
			
			else if(Input.GetButtonUp ("Fire1"))
			{
				sliding = false;
				stopSliding = true; 
				startSliding = false;
			}
			else if(Input.GetButton ("Fire1"))
			{
				holdPosition = Input.mousePosition;
			}

			if(startSliding){
				startSliding=false;
				if(inventory.heldItem != null){
					heldItem = inventory.heldItem;
				}
			}

				if(sliding){
					startSliding=false;
					if(inventory.heldItem != null){
						heldItem = inventory.heldItem;
					}
				}

			if(stopSliding){
				stopSliding = false;
					if(heldItem != null && heldItem.name == heldItemName){
					RaycastHit hit;
					startSliding=false;
					holdPosition = Input.mousePosition;
					Ray ray = Camera.main.ScreenPointToRay(holdPosition);
					if (Physics.Raycast (ray, out hit, 1000, 1<<13)) 
					{
							Debug.Log("raycast" + hit.collider.name);
							if(hit.collider.name==colliderOnWhichToCheck){
								puzzle.SendMessage(messageSendingToPuzzleOnCombine);
								//TODO inventory.SetVisible(false);
								used++;
						}
					}
				}
				heldItem = null;
			}
			}
		}

		public void closeClicked(){
			isEnabled=false;
			if(spriteInInventory)
				spriteInInventory.enabled=true;
		}

		public void OnClick(){
			Debug.Log("have been clicked");
			//SpriteRenderer spriter=gameObject.GetComponent<SpriteRenderer>();
			if(spriteInInventory)
				spriteInInventory.enabled=false;
			Global.player.SetInCutScene(true, CutsceneTools.Type.Puzzle);
			Global.player.ChangeState(actionAnimation);
			Global.player.SetDestination(Global.player.transform.position);
			if(showInventory)
				inventory.SetVisible(true);
			isEnabled = true;
			if(puzzle!=null){
				puzzle.gameObject.SetActive(true);
				puzzle.ToggleShow(true);
				puzzle.puzzle=this;
				if(messageSendingToPuzzle.Length>0)
					puzzle.SendMessage(messageSendingToPuzzle);

			//	gameMenu.setPauseButtonVisible(false);
			}
			if(used>0){
				puzzle.SendMessage(messageSendingToPuzzleOnCombine);
				//TODO inventory.SetVisible(false);
			}
			StartCoroutine(onClickCoroutine());
		}

		IEnumerator onClickCoroutine(){
			yield return new WaitForSeconds(0.1f);
			if(spriteInInventory)
				spriteInInventory.enabled=false;
		}

	}
}