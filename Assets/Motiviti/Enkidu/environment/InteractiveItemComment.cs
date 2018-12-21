using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class InteractiveItemComment : InteractiveItemAction {

		public string comment = "COMMENT MISSING";

		public string commentWhenCombine = "";

		public InventoryItem combiningItem;

		InteractiveItemSwitch connectedSwitch;
		public InteractiveItemSwitch.State enableOnlyForSwitchState = InteractiveItemSwitch.State.ANY;

		bool isInventoryItem = false;

		public string [] multipleComments;

		[SaveState]
		protected bool usedFlag = false;

		[SaveState]
		public bool onlyOnce = false;

		int currentComment = 0;

		public string overrideAction = "";
		public GameObject overrideActionObject;

	public enum TalkDirection{
			None = 0,
			TalkDiagonalFrontLeft = 1,
			TalkDiagonalFrontRight = 2,
			TalkDiagonalBackLeft = 3,
			TalkDiagonalBackRight = 4,
			TalkFront = 5
		}

		public TalkDirection talkDirection = TalkDirection.None;

		// Use this for initialization
		void Start () 
		{
			base.Initialise();

			connectedSwitch = gameObject.GetComponent<InteractiveItemSwitch>();

			isInventoryItem = gameObject.GetComponent<InventoryItem> () != null;
		}
		
		// Update is called once per frame
		new void Update () {
			base.Update();
		}

		public override IEnumerator ProcessArrivedAt()
		{
			if(connectedSwitch != null && !connectedSwitch.ValidateSwitchState(enableOnlyForSwitchState))
			{
				yield return null;
			}
			else
			if((interactiveItem.heldItem == null || combiningItem != null || interactiveItem.heldItem != combiningItem) && string.IsNullOrEmpty(comment))
			{
				yield return null;
			}
			else
			if(interactiveItem.heldItem != null)
			{
				yield return null;

			}
			else
			{
				if(onlyOnce && usedFlag)
				{
					
				}
				else
				{
					
					usedFlag = true;
					SaveState();

					if(!isInventoryItem || (isInventoryItem && !Global.player.IsWalking()))
					{
						if(overrideActionObject != null && !string.IsNullOrEmpty(overrideAction))
						{
							overrideActionObject.SendMessage(overrideAction, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							switch(talkDirection){
							case TalkDirection.TalkDiagonalFrontLeft:
								Global.player.ChangeTalkDirection(Player.TalkDirection.Diagonal);
								Global.player.ChangeDirection(-1);
								break;
							
							case TalkDirection.TalkDiagonalFrontRight:
								Global.player.ChangeTalkDirection(Player.TalkDirection.Diagonal);
								Global.player.ChangeDirection(1);
								break;
							
							case TalkDirection.TalkDiagonalBackLeft:
								Global.player.ChangeTalkDirection(Player.TalkDirection.DiagonalBack);
								Global.player.ChangeDirection(-1);
								break;
							
							case TalkDirection.TalkDiagonalBackRight:
								Global.player.ChangeTalkDirection(Player.TalkDirection.DiagonalBack);
								Global.player.ChangeDirection(1);
								break;
							case TalkDirection.TalkFront:
								Global.player.ChangeTalkDirection(Player.TalkDirection.Front);
								break;
							case TalkDirection.None:
								/*
								if(Global.elroy.talkDirection == ElroyAdv.TalkDirection.DiagonalBack && (transform.position.y-2 > Global.elroy.transform.position.y))
									Global.elroy.ChangeTalkDirection(ElroyAdv.TalkDirection.Diagonal);
								*/


								break;
								}
					//     Debug.Log("interactiveItem.heldItem: " + interactiveItem.heldItem + " combiningItem: " + combiningItem);
							if (interactiveItem.heldItem != null && combiningItem != null && interactiveItem.heldItem == combiningItem)
							{
								if (commentWhenCombine.Length > 1)
									yield return StartCoroutine(Global.player.SpeakProcedure(commentWhenCombine));
							}
							else
							{
								yield return StartCoroutine(Global.player.SpeakProcedure(comment));
								if (multipleComments.Length > 0)
								{
									comment = multipleComments[currentComment];
									yield return null;
									currentComment++;       
									currentComment = currentComment % multipleComments.Length;
								}
							}
						}
					}
				}
			}
		}
	}
}