using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	
		public class InventoryItem : InteractiveItemAction {

		Inventory inventory;

		public string itemDescription;

		public enum State
		{
			LOCKED,
			WORLD,
			INVENTORY,
			INVENTORY_HOLDING,
			REMOVED
		}

		public AudioSource sfxPickup;

		[SaveState]
		protected int progressCounter = 0;

		[SaveState]
		public int inventoryIndex = 0;

		public int startedInventoryIndex = 0;
		
		[SaveState]
		public State state = State.WORLD;

	/*	[SaveState]
		public int pickedUpIndex = -1;
	*/
		public SpriteRenderer inventorySprite;
		public SpriteRenderer highlightBorderSprite;

		[SaveState]
		public int lifeCycleStatus = -1; // default

		public SpriteRenderer[] lifeCycleStatusInventorySprites;
		public SpriteRenderer[] lifeCycleStatusHighlightBorderSprites;

		public Color highlightColor = Color.green;

		SpriteRenderer spriteRenderer;

		public Player.State actionAnimationPickUp = Player.State.PickUpSide;

		Color colorNormal, colorHolding;

		public Vector3 originalScale;

		Collider2D collider2d;

		//public string [] useInScenes;

		public bool stayInCutscene = false;

		public string wontWorkComment;

		public InteractiveItem wontWorkCommentItem;

		bool isStart = false;

		Color originalColor = Color.white;

		public RectTransform rectTransform;

		RectTransform canvasRect;

		UnityEngine.UI.Image highlightImage, spriteImage;

		bool pointerOver = false;

		CanvasGroup canvasGroup;
		// Use this for initialization

		void Awake(){
			//inventorySprite.transform.localScale /= inventorySprite.transform.parent.localScale.x;

			GameObject obj = new GameObject ();
			UnityEngine.UI.Image img = obj.AddComponent<UnityEngine.UI.Image> ();
			img.sprite = inventorySprite.sprite;
			//img.rectTransform.anchorMin = new Vector2 (, 0.5f);
			obj.name = "UIImage-main";
			obj.transform.SetParent( transform );
			rectTransform = img.rectTransform;
			spriteImage = img;

			if(highlightBorderSprite == null) highlightBorderSprite = inventorySprite;

			if(highlightBorderSprite != null)
			{
				GameObject obj1 = new GameObject ();
				UnityEngine.UI.Image img1 = obj1.AddComponent<UnityEngine.UI.Image> ();
				img1.sprite = highlightBorderSprite.sprite;
				obj1.name = "UIImage-highlight";
				obj1.transform.SetParent( obj.transform );
				img1.color = new Color (0, 0, 0, 0);
				highlightImage = img1;
				var cg1 = obj1.AddComponent<CanvasGroup> ();
				cg1.blocksRaycasts = false;
				cg1.interactable = false;
			}

			canvasGroup = obj.AddComponent<CanvasGroup> ();

			inventorySprite.transform.position += Vector3.up * 100;
			if(highlightBorderSprite!=null)highlightBorderSprite.transform.position += Vector3.up * 100;

			UnityEngine.EventSystems.EventTrigger trigger = obj.AddComponent<UnityEngine.EventSystems.EventTrigger> ();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
			trigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((data) => { OnPointerEnterDelegate((PointerEventData)data); });
			trigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((data) => { OnPointerExitDelegate((PointerEventData)data); });
			trigger.triggers.Add(entry);
			/*
			var arf = obj.AddComponent<UnityEngine.UI.AspectRatioFitter> ();
			arf.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth;
			*/
		}

		public bool PointerOver(bool hasToBeInInventory=true)
		{
			if (!hasToBeInInventory || state == State.INVENTORY) {
				return pointerOver;
			}	
			return false;
		}

		void HighlightBorder(bool b)
		{
			if (highlightBorderSprite == null)
				return;
			highlightBorderSprite.enabled = true;
			highlightImage.color = b ? Color.white : new Color (0, 0, 0, 0);
		}

		public void OnPointerDownDelegate(PointerEventData data)
		{
			inventory.HoldItem (this);
		}

		public void OnPointerEnterDelegate(PointerEventData data)
		{
			pointerOver = true;
			//Debug.Log (Time.time + gameObject.name + " pointer over");
		}

		public void OnPointerExitDelegate(PointerEventData data)
		{
			pointerOver = false;
			//Debug.Log (Time.time + gameObject.name + " pointer out");
		}

		void  Start () {

			base.InitialiseGlobal();
			
			originalColor = inventorySprite.color;

			//Debug.Log (gameObject.name + " " + state);

			inventory = Global.inventory;
			canvasRect = inventory.gameObject.GetComponent<RectTransform> ();
			collider2d = GetComponent<Collider2D>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			if(spriteRenderer)colorNormal = spriteRenderer.color;
			colorHolding = new Color( colorNormal.r, colorNormal.g, colorNormal.b, 0.5f);
			originalScale = inventorySprite.transform.localScale; 
			isStart = true;
			if(state == State.INVENTORY)
			{
				//ManuallyAddToInventoryPickup(1.0f, false);

				AddMe(false);
				//inventory.Add(this, false, false);
				//ChangeState(State.INVENTORY);
			}
			else
			if (state == State.REMOVED)
			{
				ChangeState(State.REMOVED);
				ProcessState();
			}
			else
			if (state == State.INVENTORY_HOLDING) // just can't start the scene already holding an item
			{
				ChangeState(State.INVENTORY);
				ProcessState();
			}

			if(state != State.INVENTORY)
				isStart = false;

			startedInventoryIndex = inventoryIndex;

			ChangeLifeCycleStatus(lifeCycleStatus);
		}

		public void ChangeLifeCycleStatus(string s)
		{
			try
			{
				int a = Int32.Parse(s);
				ChangeLifeCycleStatus(a);
			}
			catch
			{

			}
		}
		public void ChangeLifeCycleStatus(int s)
		{
			bool en1 = inventorySprite.enabled;
			bool en2 = highlightBorderSprite != null ? highlightBorderSprite.enabled : false;

			for(int i=0; i<lifeCycleStatusInventorySprites.Length; i++)
			{
				if(s == i)
				{
					inventorySprite = lifeCycleStatusInventorySprites[i];
				}

				lifeCycleStatusInventorySprites[i].enabled = false;
			}

			for (int i = 0; i < lifeCycleStatusHighlightBorderSprites.Length; i++)
			{
				if (s == i)  
				{
					highlightBorderSprite = lifeCycleStatusHighlightBorderSprites[i];
				}

				lifeCycleStatusHighlightBorderSprites[i].enabled = false;
			}

			inventorySprite.enabled = en1;
			HighlightBorder (en2);

			lifeCycleStatus = s;

			spriteImage.sprite = inventorySprite.sprite;
			highlightImage.sprite = highlightBorderSprite.sprite;

			SaveState();
		}

		public void ManuallyAddToInventoryPickup(float delay = 0.2f, bool setVisible = true, bool updateIndex = false)
		{
			//

			StartCoroutine(ManuallyAddItem(null, delay, setVisible, updateIndex));
		}

		IEnumerator ManuallyAddItem(bool? iss = null, float delay = 0.2f, bool setVisible = true, bool updateIndex= false)
		{
			yield return new WaitForSeconds(delay);
			AddMe(setVisible, updateIndex);
			yield return null;
		}

		void AddMe(bool setVisible = true, bool updateIndex = false)
		{
			inventory.Add(this, setVisible, updateIndex);
			ChangeState(State.INVENTORY);
			ProcessState();
			//SaveState();
			isStart = false;
		}

		public void Remove()
		{
			ChangeState(State.REMOVED);
			ProcessState();
		}

		public override bool CanDeactivate()
		{
			return (state == State.WORLD || state == State.LOCKED);
		}

		public void ChangeState( State newState )
		{
			Global.FlurryLog ("InventoryItemState", gameObject.name, newState.ToString());

			var oldState = state;

			switch(newState)
			{
			case State.LOCKED:
			case State.WORLD:
				GetComponent<Renderer>().enabled = true;
				if(inventorySprite)inventorySprite.enabled = false;
				if(collider2d)collider2d.enabled = true;
				inventorySprite.transform.localScale = originalScale;
				if (inventorySprite) inventorySprite.sortingOrder = 1;
				rectTransform.gameObject.SetActive (false);
				break;
				
			case State.INVENTORY_HOLDING:
				if (inventorySprite)
					inventorySprite.color = colorHolding;
				GetComponent<Renderer> ().enabled = false;
				if (inventorySprite)
					inventorySprite.enabled = true;
				if (collider2d)
					collider2d.enabled = false;
				spriteRenderer.color = colorNormal;
				inventorySprite.transform.localScale = originalScale * 1.1f;

				if (inventorySprite)
					inventorySprite.sortingOrder = 100;

				transform.SetAsLastSibling ();
			//	rectTransform.gameObject.SetActive (false);

				break;

			case State.INVENTORY:
				
				if(inventorySprite)inventorySprite.color = colorNormal;
				GetComponent<Renderer>().enabled = false;
				if(inventorySprite)inventorySprite.enabled = true;
				if(collider2d)collider2d.enabled = true;
				spriteRenderer.color = colorNormal;
				inventorySprite.transform.localScale = originalScale;
				if (inventorySprite) inventorySprite.sortingOrder = 1;
				if(!isStart && oldState == State.WORLD)
					SendMessage ("PickedUp", SendMessageOptions.DontRequireReceiver);

			//	rectTransform.gameObject.SetActive (true);

				break;

			case State.REMOVED:
				inventory.Remove (this);
				GetComponent<Renderer> ().enabled = false;
				if (inventorySprite)
					inventorySprite.enabled = false;
				if (collider2d)
					collider2d.enabled = false;
				SetHighlight (false);

				if (spriteImage)
					spriteImage.enabled = false;

				if (highlightImage)
					highlightImage.enabled = false;
			//	rectTransform.gameObject.SetActive (false);
				break;
			}

			state = newState;


			SaveState();
			/*
			if(useInScenes.Length>0)
				saveForScenes();
			*/

		}



		void SetHighlight(bool b)
		{
			if (highlightBorderSprite != null)
				HighlightBorder (b);
			else 
			{
				if(b)
				{
					inventorySprite.color = highlightColor;
				}
				else
				{
					inventorySprite.color = originalColor;
				}
			}
		}
	/*
		public void saveForScenes(){
			if(useInScenes.Length>0){
			foreach(string scene in useInScenes){
				
				SaveInventoryForMultiple(scene, "state", (int)state);
				SaveInventoryForMultiple(scene, "progressCounter", progressCounter);
				SaveInventoryForMultiple(scene, "inventoryIndex", inventoryIndex);
				SaveInventoryForMultiple(scene, "sequenceNumber", sequenceNumber);
			}
			}
		}
	*/
		void ProcessState()
		{
			switch(state)
			{
			case State.LOCKED:
			case State.WORLD:
				SetHighlight(false);
				break;
				
			case State.INVENTORY_HOLDING:
	//			Debug.Log(inventory.inventoryItemPosition);
				Vector3 pos = inventory.inventoryItemPosition;
				transform.position = pos;
				Vector2 viewportPos = Camera.main.WorldToViewportPoint (pos);

				Vector2 pos2 = new Vector2 ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
								(viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));

				rectTransform.anchoredPosition = pos2 * inventory.canvas.scaleFactor + canvasRect.anchoredPosition /*- 41.5f*Vector2.right*inventory.canvas.scaleFactor*/;// + inventory.itemHolder.GetComponent<RectTransform>().anchoredPosition;
	//			Debug.Log (canvasRect.sizeDelta + " " + inventory.canvas.scaleFactor + " " + pos + " " + pos2);
				HighlightIfOverInteractiveItem ();
				if(canvasGroup)canvasGroup.blocksRaycasts = false;
				break;
			case State.INVENTORY:
				SetHighlight(false);
				if(canvasGroup)canvasGroup.blocksRaycasts = true;
				break;
			case State.REMOVED:
				transform.position = Vector3.one*100;
				canvasGroup.blocksRaycasts = false;
				break;
			}

		
		}

		public void PublicHighlightIfOverInteractiveItem(){
			HighlightIfOverInteractiveItem();
		}

		public void JustHighlight(){
			HighlightBorder (true);
		}

		void HighlightIfOverInteractiveItem()
		{
			if (true)
			{
				bool enable = false;

				Collider2D[] col = Physics2D.OverlapPointAll(transform.position);

				var hoveredItem = inventory.ItemHoveringOver ();
				//if(hoveredItem != null) Debug.Log (Time.time + " hovering over " + hoveredItem.gameObject.name);
				if (hoveredItem != null && hoveredItem != this) {
					enable = true;
					SetHighlight(enable);

					/*

					InteractiveItem intItem = hoveredItem.GetComponent<InteractiveItem>();
					foreach (InventoryItem invItem1 in intItem.CanBeCombinedWith)
					{
						if (invItem1 == this)
						{
							enable = true;
							SetHighlight(enable);

							return;
						}
					}*/
				}

				if(col.Length > 0)
				{
					foreach(Collider2D c in col)
					{
						if(c.gameObject.layer == 10)
						{
	//                        Debug.Log("names: " + c.name);
							if(c.gameObject.GetComponent<DoorBetweenLevels>() == null){
								enable = true;
							//	break;
							}
							else{
								DoorBetweenLevels door = c.gameObject.GetComponent<DoorBetweenLevels>();
								InteractiveItemSwitch switchItem = c.gameObject.GetComponent<InteractiveItemSwitch>();
								if (switchItem != null)
								{
									if (door.enableOnlyForSwitchState != InteractiveItemSwitch.State.ANY && door.enableOnlyForSwitchState != switchItem.state)
									{
										enable = true;
										//break;
									}
								}
							}
							if (c.gameObject.GetComponent<InventoryItem>() != null)
							{
								InteractiveItem intItem = c.gameObject.GetComponent<InteractiveItem>();
								bool doBreak = false;
								foreach (InventoryItem invItem1 in intItem.CanBeCombinedWith)
								{
									Debug.Log("to se zgodi foreach: " + c.name);
									if (invItem1 == this)
									{
										enable = true;
										doBreak = true;
										break;
									}
								}
								if(doBreak)
									break;
							}
						}
						if(c.gameObject.layer == 11){  //added to disable highlightning over Inventory
							enable = false;
							break;
						}
					}
				}
			
				SetHighlight(enable);
			}
		}

		// Update is called once per frame
		protected override void Update () 
		{
			base.Update();
			ProcessState();
		}

		IEnumerator JustPickedUpFlash()
		{
			bool b = false;

			float time = 1.5f;
			float frequency = 10f;
			float delay = 1f / frequency;

			for(float f=0; f<time*frequency; f+=2.0f)
			{
				yield return new WaitForSeconds(delay);

				for (int i = 0; i < 2; i++)
				{
					HighlightBorder (b);
	/*
					if (b)
					{
						inventorySprite.color = highlightColor;
					}
					else
					{
						inventorySprite.color = originalColor;
					}
	*/
					b = !b;
					yield return new WaitForSeconds(delay);
				}

			
			}
			
			SetHighlight(false);
		}

		public bool PickUp()
		{
			Debug.Log ("PickUp " + gameObject.name);

			switch(state)
			{
			case State.LOCKED:
				return false;
				
			case State.WORLD:
				inventory.Add (this);
				ChangeState(State.INVENTORY);

				if(sfxPickup)sfxPickup.Play ();

				SendMessage("PickedUp", SendMessageOptions.DontRequireReceiver);

				if(string.IsNullOrEmpty(itemDescription) == false)
				{
					if(Global.player != null)
					{
						Global.player.Speak(itemDescription);
					}
				}

			// StartCoroutine(JustPickedUpFlash());
				break;
			}

			return true;
		}

		public override void AnimationActionPoint(string animationName)
		{
			progressCounter = 1;
		}
		
		public override void AnimationFinished(string animationName)
		{
			progressCounter = 2;
		}

		public override IEnumerator ProcessArrivedAt()
		{
			progressCounter = 0;

			Global.player.TurnTowards(interactiveItem);

			if(!Global.player.inCutScene)
				Global.player.SetInCutScene(true, CutsceneTools.Type.BlackBands);

			if(state != State.INVENTORY && state != State.LOCKED)Global.player.ChangeState( actionAnimationPickUp );
			float time0 = Time.time;

			Global.player.StopTalking();

			while(progressCounter == 0)
			{
				yield return new WaitForSeconds(0.01f);
				if(Time.time - time0 > Global.maxCharacterAnimationLength) 
				{
					Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
					break;
				}
			}

			PickUp ();

			while(progressCounter == 1)
			{
				yield return new WaitForSeconds(0.05f);
				if(Time.time - time0 > Global.maxCharacterAnimationLength) 
				{
					Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
					break;
				}
			}

		//	Global.elroy.ChangeState( ElroyAdv.State.IdleDiagonalFront );
			Global.player.ChangeState(endState);
			if(Global.player.inCutScene && !stayInCutscene)
				Global.player.SetInCutScene(false);
			yield return new WaitForSeconds(0.4f);

		}
	}
}