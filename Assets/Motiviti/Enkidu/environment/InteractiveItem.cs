using UnityEngine;
using System.Collections;
using System.Linq;
using System.Reflection;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class InteractiveItem : MonoBehaviour {

		InteractiveItemAction[] actionChain;

		public bool noClickBackground = false;
		AdvCamera advCamera;
		bool deactivatedBefore = false;

		public bool isCancelable = true;

		public string objectName;

		public float cursorRotation = 0;

		public Vector3 stoppingDistance = Vector3.zero;
		public Vector3 centerOffset = Vector3.zero;

		public bool allowInterruptions = true;

		public CutsceneTools.Type cutsceneTool = CutsceneTools.Type.BlackBands;

		public bool doCloseup = false;
		public float closeUpCameraSize = 5;
		public Vector3 closeUpOffset = Vector3.zero;

		public int itemLayer = 0;

		public InventoryItem heldItem = null;

		public InventoryItem[] CanBeCombinedWith;

		public bool needToWalkToItem = false;

		bool canBeUsed = false;

		bool isExit = false;

		bool isPerson = false;

		public InteractiveItemCombine currentCombine;

		public bool stayInCutscene = false;

		public bool realyStayInCutscene = false;

		public bool isClickIndicatorFlipped = false;

		float lastArrivedAt = 0;

		public int desiredDirection = 0;

		bool highlighting = false;

		GameObject highlightObject;

		float lastClickOnItemTime = float.MinValue;

		void Awake()
		{
			actionChain = GetComponents<InteractiveItemAction>().OrderBy( x => x.sequenceNumber ).ToArray() ;        
		}

		void OnDrawGizmosSelected() {
			Gizmos.DrawIcon(transform.position + stoppingDistance, "gizmo-stopping.psd", true);
			Gizmos.DrawIcon(transform.position + centerOffset, "gizmo-center.psd", true);
			Gizmos.DrawIcon(transform.position, "gizmo-pos.psd", true);
		}

		public string GetObjectName()
		{
			string text = gameObject.name;
			if (!string.IsNullOrEmpty(objectName))
				text = objectName;
			return text;
		}
		// Use this for initialization
		void Start () {
			
			advCamera = Global.advCamera;

			highlightObject = Instantiate ( Resources.Load ("INTERACTIVEOBJECT_HIGHLIGHT") as GameObject ) as GameObject;
			highlightObject.transform.position = transform.position + centerOffset;
			highlightObject.transform.parent = transform;
			highlightObject.GetComponent<Renderer>().enabled = false;

			if (GetComponent<InteractiveItemSendMessage>() != null)
				canBeUsed = GetComponent<InteractiveItemSendMessage>().canBeUsed;

			if (GetComponent<InteractiveItemConversation> () != null)
				canBeUsed = true;

			if (GetComponent<InteractiveItemPuzzle> () != null)
				canBeUsed = true;

			if (GetComponent<InteractiveItemRotatingStates> () != null)
				canBeUsed = true;

			if (GetComponent<InteractiveItemSwitch> () != null)
			{
				if(GetComponent<InteractiveItemSwitch> ().contributesToCanBeUsedFlag)
				canBeUsed = true;
			}

			if (GetComponent<InventoryItem> () != null)
				canBeUsed = true;

			if (GetComponent<InteractiveItemAnimator> () != null)
				canBeUsed = true;

			if (GetComponent<DoorBetweenLevels> () != null)
				isExit = true;

			if (GetComponent<DoorBetweenCaves> () != null)
				isExit = true;

			if (GetComponent<PersonNPC> () != null)
				isPerson = true;
		}
		
		public bool IsInInventory()
		{
			var cmp = GetComponent<InventoryItem>();

			if(cmp)
			{
				if (cmp.state == InventoryItem.State.INVENTORY)
					return true;
			}

			return false;
		}

		// Update is called once per frame
		void Update () {
		
			
		}

		public void Highlight(bool b)
		{
			if(b != highlighting)
			{
				if(b && (!gameObject.activeInHierarchy || !this.enabled || (!GetComponent<Collider2D>() || !GetComponent<Collider2D>().enabled))) b = false;

				if(highlightObject != null) highlightObject.GetComponent<Renderer>().enabled = b;
				highlighting = b;
			}
		}

		public float DoubleClickTime()
		{
			return (Time.time - lastClickOnItemTime);
		}

		public void Selected()
		{
			lastClickOnItemTime = Time.time;
			for (int i = 0; i < actionChain.Length; i++)
			{
				actionChain[i].Selected();
			}
		}

		public bool SupportsDoubleClick()
		{
			for (int i = 0; i < actionChain.Length; i++)
			{
				if (actionChain[i].SupportsDoubleClick()) return true;
			}

			return false;
		}

		public void DoubleClick()
		{
			for (int i = 0; i < actionChain.Length; i++)
			{
				if (actionChain[i].enabled && actionChain[i].SupportsDoubleClick()) actionChain[i].DoubleClicked();
			}
		}

		public bool CanBeUsed()
		{
			return canBeUsed;
		}

		public void SetCanBeUsed(bool flag)
		{
			canBeUsed = flag;
		}

		public bool IsPerson()
		{
			return isPerson;
		}

		public bool IsExit()
		{
			return isExit;
		}

		public void ItemActivate()
		{
			bool allowed = true;
			
			for(int i=0; i<actionChain.Length; i++)
			{
				allowed = allowed && actionChain[i].CanDeactivate();
			}

			if(allowed)
			{
				SetObjectShown(transform, true);
			}
		}

		void SetObjectShown(Transform tr, bool shown)
		{
			if(!shown)tr.position += Vector3.up * 100;
			else if(deactivatedBefore)
				tr.position -= Vector3.up * 100;

			if (!shown) deactivatedBefore = true;
		}

		public void ItemDeactivate()
		{
			bool allowed = true;

			for(int i=0; i<actionChain.Length; i++)
			{
				allowed = allowed && actionChain[i].CanDeactivate();
			}

			if(allowed)
			{
				SetObjectShown(transform, false);
			}
		}

		public void AnimationActionPoint(string animationName)
		{
			for(int i=0; i<actionChain.Length; i++)
			{
				actionChain[i].AnimationActionPoint(animationName);
			}
		}
		
		public void AnimationFinished(string animationName)
		{
			for(int i=0; i<actionChain.Length; i++)
			{
				actionChain[i].AnimationFinished(animationName);
			}
		}

		public bool ArrivedAt(InventoryItem item)
		{
			Debug.Log("Arrived at " + gameObject.name);
			if (Time.time - lastArrivedAt > 0.5f) {

				lastArrivedAt = Time.time;

				heldItem = item;

				if (heldItem != null && !CanCombineItem(heldItem))
				{
					Debug.Log("Can't combine this.");

					return false;
				}
				StartCoroutine (ProcessArrivedAt ());

				Global.player.PublicReturnItemToInventory();

			} else
				return true; // otherwise Elroy says "that won't work" 
		
			return true;
		}

		public bool CheckIfCombiningMatchingItems(InventoryItem item){
			if(item != null){
				if(!CanCombineItem(item)) return false;
				else{
					InteractiveItemCustomCombine[] cc = gameObject.GetComponents<InteractiveItemCustomCombine>();
					foreach(InteractiveItemCustomCombine cc1 in cc){
						if(cc1.inventoryItem == item && !cc1.isCancelable)
							return true;
					}
				}
			}

			if(isCancelable)
				return false;
			else
				return true;
		}

		bool CanCombineItem(InventoryItem item)
		{
			foreach(var ii in CanBeCombinedWith)
			{
				if(ii == item) return true;
			}

			return false;
		}

		public Vector3 GetStoppingDistance(InventoryItem _heldItem = null)
		{
			InteractiveItemAction act = OverridingAction(_heldItem);

			if(act != null)
			{
				Debug.Log("Getting stopping distance: OVERRIDEN by " + act.GetType().ToString());
				return act.stoppingDistance;
			}
			Debug.Log("Getting stopping distance: original");
			return stoppingDistance;           
		}

		public Vector3 GetCenterOffset(InventoryItem _heldItem = null)
		{
			InteractiveItemAction act = OverridingAction(_heldItem);

			if (act != null)
			{
				return act.centerOffset;
			}

			return centerOffset;
		}

		InteractiveItemAction OverridingAction(InventoryItem heldItem)
		{
			Debug.Log("Determining if there's a position overriding action (heldItem=" + heldItem + ")");

			for (int i = 0; i < actionChain.Length; i++)
			{
				// var action = actionChain[i];
				// bool actionShouldSkip = action.ShouldSkip();

				if (!actionChain[i].enabled || actionChain[i].ShouldSkip()) continue;
				
				if (heldItem != null)
				{
					if (actionChain[i] is InteractiveItemCombine || actionChain[i] is InteractiveItemComment || actionChain[i] is InteractiveItemControlCamera || actionChain[i] is InteractiveItemChangeCamera)
					{
						if (actionChain[i].overrideStoppingDistance && ((actionChain[i] as InteractiveItemCombine).inventoryItem == heldItem || (actionChain[i] is InteractiveItemSendMessageCombine && (actionChain[i] as InteractiveItemSendMessageCombine).SupportsItem(heldItem))) )
						{
							return actionChain[i];
						}
					}
				}
				else
				{
					if (!(actionChain[i] is InteractiveItemCombine))
					{
						if (actionChain[i].overrideStoppingDistance) return actionChain[i];
					}
				}
			}
			
			return null;
		}

		IEnumerator ProcessArrivedAt()
		{
			if(desiredDirection == 1)
				Global.player.TurnTowards(true);
			else if(desiredDirection == -1)
				Global.player.TurnTowards(false);

			bool wasInCutScene = false;
			wasInCutScene = Global.player.inCutScene;

			if(!allowInterruptions)Global.player.SetInCutScene(true, cutsceneTool, Global.player.transform.position);
			if(doCloseup)advCamera.CloseUpBegin(closeUpCameraSize, closeUpOffset);

			bool hasSomethingMeaningfulOccurred = false;
			
			for(int i=0; i<actionChain.Length; i++)
			{
				// var action = actionChain[i];
				// bool actionShouldSkip = action.ShouldSkip();

				if (!actionChain[i].enabled || actionChain[i].ShouldSkip()) continue;

				actionChain[i].StopHighlighting();
				if(actionChain[i] is InteractiveItemComment)
					yield return new WaitForSeconds(0.1f);

				if(heldItem != null)
				{
					if (actionChain[i] is InteractiveItemCombine || actionChain[i] is InteractiveItemComment || actionChain[i] is InteractiveItemControlCamera || actionChain[i] is InteractiveItemChangeCamera)
					{
						Global.FlurryLog ("ArrivedAt", gameObject.name, actionChain[i].GetType().ToString(), heldItem.gameObject.ToString());

						yield return StartCoroutine(actionChain[i].ProcessArrivedAt());
					
						hasSomethingMeaningfulOccurred = true;
					}
				}
				else
				{
					if(!(actionChain[i] is InteractiveItemCombine))
					{
						Global.FlurryLog ("ArrivedAt", gameObject.name, actionChain[i].GetType().ToString());
						yield return StartCoroutine(actionChain[i].ProcessArrivedAt());
					}
				}
			}
	//		Debug.Log("pride do konca");
			if(doCloseup)advCamera.CloseUpEnd();

			if(!realyStayInCutscene){
	//			Debug.Log("realyStayInCutscene");
				if (!stayInCutscene || !hasSomethingMeaningfulOccurred) {
	//				Debug.Log("stayInCutscene");
					if (!allowInterruptions && !wasInCutScene){
	//					Debug.Log("allowInterruptions");
						Global.player.SetInCutScene (false);
					}
				}
			}
		}
	}
}