using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class Inventory : StatefulItem
    {
        [SaveState]
        bool isVisible = false;

        public string itemDescription;

        public CanvasGroup buttonOpen, buttonClose;

        public Transform itemHolder;

        public Vector3 inventoryItemPosition = Vector3.zero;

        public InventoryItem heldItem = null;

        ArrayList items;

        float yOffset = 70f;

        AdvCamera advCamera;

        public float scale = 1;

        public float itemScale = 1;

        public Vector3 originalPosition;

        public Vector3 positionOffset = Vector3.zero;

        bool isHidden = false;

        public float hiddenOffset;

        public SpriteRenderer arrow;

        public SpriteRenderer arrowLight;

        public Sprite arrowClickedSprite;

        bool justClicked = false;

        bool positionOk = false;

        SpriteRenderer[] visibleElements;

        Color hiddenColor = new Color(1, 1, 1, 0);

        Color visibleColor = new Color(1, 1, 1, 1);

        Vector3 topHitpoint;

        public bool autoHide = false;

        [SaveState]
        int scrollPos;

        int itemsVisible = 9;

        public SpriteRenderer arrowLeftOn, arrowLeftOff, arrowRightOn, arrowRightOff;

        Transform arrowRightTransform;

        public InventoryButtonBehavior scrollLeftButton, scrollRightButton;

        public RectTransform parentObject;

        public RectTransform sampleInventoryItem;

        public Canvas canvas;

        ArrayList uiElements;

        int savedScrollIndex;

        enum Fade
        {
            FadeIn = 0,
            Visible = 1,
            FadeOut = 2,
            Invisible = 3
        }

        Fade fading = Fade.Visible;

        int guiGeneralHoverCount = 0;

        public Transform itemDropZone;
        public Animator itemDropZoneAnimator;

        public float hiddenYDelta = 79f;

        public float horizontalItemOffset = 85f;

        float lastTimeCloseClicked = 0;

        public Animator animator;

        public void EnableItemUsage(bool b)
        {
        }

        public void Close()
        {
            SetVisible(false);
            lastTimeCloseClicked = Time.time;
        }

        public void Open()
        {
            SetVisible(true);

        }

        void AddUIElements(Transform t)
        {
            uiElements.Add(t);

            foreach (Transform child in t)
                AddUIElements(child);
        }

        IEnumerator Start()
        {
            base.InitialiseGlobal();

            savedScrollIndex = scrollPos;

            uiElements = new ArrayList();
            AddUIElements(transform);
            //SetVisible (true);
            yOffset = canvas.scaleFactor * horizontalItemOffset;
            UpdateItemPosition_Scroll();

            UnityEngine.EventSystems.EventTrigger trigger = itemDropZone.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { OnItemDropzonePointerEnterDelegate((PointerEventData)data); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((data) => { OnItemDropzonePointerExitDelegate((PointerEventData)data); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drop;
            entry.callback.AddListener((data) => { OnItemDropzonePointerUpDelegate((PointerEventData)data); });
            trigger.triggers.Add(entry);

            foreach (Transform uielement in uiElements)
            {
                trigger = uielement.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) => { OnGUIGeneralPointerEnterDelegate((PointerEventData)data); });
                trigger.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerExit;
                entry.callback.AddListener((data) => { OnGUIGeneralPointerExitDelegate((PointerEventData)data); });
                trigger.triggers.Add(entry);
            }

            SetVisible(isVisible);

            yield return null;
            SetScrollPosition(savedScrollIndex);

            UpdateItemPosition_Scroll();
        }

        public void OnItemDropzonePointerExitDelegate(PointerEventData data)
        {
            itemDropZoneAnimator.SetInteger("state", 0);
        }

        public void OnItemDropzonePointerEnterDelegate(PointerEventData data)
        {
            if (heldItem != null)
                itemDropZoneAnimator.SetInteger("state", 1);
        }

        public void OnItemDropzonePointerUpDelegate(PointerEventData data)
        {
            itemDropZoneAnimator.SetInteger("state", 0);
            if (heldItem != null)
            {
                //string text = heldItem.itemDescription;
                string id = null;//Global.GetItemsData().GetID(heldItem.name);
                if (id.Length == 0)
                    id = "Hm.";
                Global.player.Speak(id);
            }
        }

        public void OnGUIGeneralPointerEnterDelegate(PointerEventData data)
        {
            guiGeneralHoverCount++;
        }

        public void OnGUIGeneralPointerExitDelegate(PointerEventData data)
        {
            guiGeneralHoverCount--;
        }

        public void PointerEnter()
        {
            if (buttonOpen.interactable && (Time.time - lastTimeCloseClicked > 0.5f)) Open();
        }

        void InventoryClosed()
        {
            bool b = false;
            isVisible = b;
            buttonOpen.interactable = !b;
            buttonOpen.alpha = (b ? 0 : 1);
            buttonOpen.blocksRaycasts = !b;

            buttonClose.interactable = b;
            buttonClose.alpha = (!b ? 0 : 1);
            buttonClose.blocksRaycasts = b;
        }
        void InventoryOpen()
        {
            bool b = true;
            isVisible = b;
            buttonOpen.interactable = !b;
            buttonOpen.alpha = (b ? 0 : 1);
            buttonOpen.blocksRaycasts = !b;

            buttonClose.interactable = b;
            buttonClose.alpha = (!b ? 0 : 1);
            buttonClose.blocksRaycasts = b;
        }

        public void SetEnabled(bool e)
        {
            animator.SetBool("enabled", e);
        }
        public void SetVisible(bool b)
        {
            //parentObject.localPosition = originalPosition - Vector3.up *(!b ? hiddenYDelta : 0);
            if (animator)
            {
                animator.SetInteger("state", b ? 1 : 0);
            }

            if (b)
            {
                InventoryOpen();
            }
            else
            {
                InventoryClosed();
            }

            if (!b && heldItem != null)
            {
                ReturnItem(heldItem);
                HoldItem(null);
                heldItem = null;
            }



            UpdateItemPosition_Scroll();
        }

        void Update()
        {
        }

        public bool IsVisible()
        {
            return isVisible;
        }

        public int ItemsCount()
        {
            return items.Count;
        }

        void AdjustItemPosition(InventoryItem item)
        {
            Vector3 pos = new Vector3((item.inventoryIndex + 1 - scrollPos) * yOffset - 30, 8.5f, 0.0f);
            item.transform.localPosition = Vector3.zero;//new Vector3((item.inventoryIndex+1+scrollIndex)*yOffset -30, 8.5f, 0.0f);
            item.rectTransform.localPosition = pos;
        }

        public void Add(InventoryItem item, bool setVisible = true, bool updateIndex = true)
        {
            if (updateIndex)
                item.inventoryIndex = items.Count;

            item.interactiveItem.itemLayer = 100; // TODO ... added so that inv items have priority over Popup Background

            items.Add(item);

            item.rectTransform.localScale = Vector2.one * canvas.scaleFactor * 0.7f;

            item.transform.parent = itemHolder;

            AdjustItemPosition(item);

            if (setVisible)
                SetVisible(true);

            if (isHidden && autoHide)
                StartCoroutine(arrowClickedCoroutine(arrow.gameObject, false));

            SortItems();

            if (updateIndex)
            {
                if (items.Count > itemsVisible)
                {
                    SetScrollPosition(items.Count - itemsVisible);

                }
            }

            UpdateItemPosition_Scroll();
        }

        void SortItems()
        {
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    InventoryItem item1 = (InventoryItem)items[i];
                    InventoryItem item2 = (InventoryItem)items[j];
                    if (item1.inventoryIndex > item2.inventoryIndex)
                    {
                        items[i] = item2;
                        items[j] = item1;
                    }
                }
            }
            /*
			int lastIndex = 0;
			for (int i = 0; i < items.Count; i++) {
				InventoryItem item = (InventoryItem)items [i];
				item.inventoryIndex = lastIndex++;
			}*/
        }

        public void ReturnItem(InventoryItem item)
        {
            if (item != null)
            {
                AdjustItemPosition(item);
            }

            UpdateItemPosition_Scroll();
        }

        public void Remove(InventoryItem item)
        {
            int index = item.inventoryIndex;
            if (items.Contains(item))
            {

                items.Remove(item);

                if (items.Count <= 0)
                {
                    positionOffset = new Vector3(positionOffset.x, 100, 0);
                }
                else
                {
                    foreach (InventoryItem i in items)
                    {  //reposition all lower then removed
                        if (i.inventoryIndex > index)
                        {
                            i.inventoryIndex--;
                            //i.saveForScenes();
                            AdjustItemPosition(i);
                        }
                    }
                }
            }

            UpdateItemPosition_Scroll();
            ScrollLeft();
        }

        void UpdateItemPosition_Scroll()
        {
            if (items == null) return;

            foreach (InventoryItem i in items)
            {
                AdjustItemPosition(i);

                int d = i.inventoryIndex - scrollPos;
                if (d < 0 || d >= itemsVisible)
                {
                    i.transform.localPosition += Vector3.up * 1400;
                }
            }

            bool leftOn = true;
            bool rightOn = true;

            leftOn = scrollPos > 0;

            rightOn = scrollPos < (items.Count - itemsVisible);

            scrollLeftButton.SetEnabled(leftOn);
            scrollRightButton.SetEnabled(rightOn);

            //SaveState();
        }

        // Use this for initialization
        void Awake()
        {
            gameObject.name = "Inventory";

            canvas = GetComponent<Canvas>();
            items = new ArrayList();
            advCamera = Global.advCamera;
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));
            originalPosition = parentObject.localPosition;

            if (autoHide)
                StartCoroutine(arrowClickedCoroutine(arrow.gameObject, false));

            UpdateItemPosition_Scroll();
        }

        void SetScrollPosition(int newPos)
        {
            //		Debug.Log(Time.time + " setScrollPosition " + newPos);
            scrollPos = newPos;
        }

        public void ScrollLeft()
        {
            if (scrollPos > 0)
                SetScrollPosition(scrollPos - 1);
            UpdateItemPosition_Scroll();
        }

        public void ScrollRight()
        {
            Debug.Log("arrow right");
            if (scrollPos < (items.Count - itemsVisible))
                SetScrollPosition(scrollPos + 1);
            UpdateItemPosition_Scroll();
        }

        public InventoryItem ItemHoveringOver()
        {
            foreach (InventoryItem it in items)
            {
                if (it.PointerOver())
                    return it;
            }
            return null;
        }

        public bool ProcessInput(ref bool isHit, ref bool holding, ref Vector3 holdPosition, ref Vector3 worldPosition, ref InventoryItem item)
        {
            foreach (Touch touch in Input.touches)
            {
                holding = true;
                if (touch.phase == TouchPhase.Began)
                {
                    holdPosition = touch.position;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    holding = false;
                    holdPosition = touch.position;
                    isHit = true;
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                holding = false;
                holdPosition = Input.mousePosition;
                isHit = true;
            }

            if (Input.GetButton("Fire1"))
            {
                holding = true;
                holdPosition = Input.mousePosition;
            }
            if (Input.GetButtonDown("Fire1"))
            {
                if (!justClicked)
                {
                    justClicked = true;
                    StartCoroutine(justClickFalse());
                }
            }

            worldPosition = Global.activeCamera.ScreenToWorldPoint(holdPosition);
            worldPosition.z = 0;

            if (holding)
            {
                Vector2 v = Global.activeCamera.ScreenToWorldPoint(holdPosition);

                Collider2D[] col = Physics2D.OverlapPointAll(v);

                item = null;

                if (col.Length > 0)
                {
                    foreach (Collider2D c in col)
                    {

                        if (c.gameObject.layer == 11)
                        {
                            return true;
                        }

                    }
                }
            }
            else if (isHit)
            {

                Vector2 v = Global.activeCamera.ScreenToWorldPoint(holdPosition);

                Collider2D[] col = Physics2D.OverlapPointAll(v);

                if (col.Length > 0)
                {
                    foreach (Collider2D c in col)
                    {

                        if (c.gameObject.layer == 11)
                        {
                            return true;
                        }

                    }
                }
                if (!isHidden && autoHide && justClicked)
                    StartCoroutine(arrowClickedCoroutine(arrow.gameObject, false));
            }


            return false;
        }

        public bool MouseOverInventory()
        {
            if (guiGeneralHoverCount > 0)
                return true;
            return false;
        }

        IEnumerator justClickFalse()
        {
            yield return new WaitForSeconds(0.1f);
            justClicked = false;
        }

        IEnumerator arrowClickedCoroutine(GameObject c, bool enableArrowLight = true)
        {
            yield return null;
        }

        public void HoldItem(InventoryItem item)
        {
            if (items == null) return;
            foreach (InventoryItem i in items)
            {
                if (i == item)
                {
                    if (i.state != InventoryItem.State.INVENTORY_HOLDING)
                    {
                        i.ChangeState(InventoryItem.State.INVENTORY_HOLDING);
                        heldItem = i;
                        Global.player.HoldItem(heldItem);
                    }
                }
                else
                {
                    if (i.state != InventoryItem.State.INVENTORY) i.ChangeState(InventoryItem.State.INVENTORY);
                }
            }
        }

        public void recalculateHiddenOffset()
        {

            if (advCamera)
            {

                hiddenOffset = 0.35f * advCamera.cameraSize / advCamera.transform.localScale.x;
                if (Global.IsSmallScreen())
                {
                    hiddenOffset = hiddenOffset * 1.5f;
                }
                if (positionOk && isHidden)
                {
                    positionOffset = new Vector3(-hiddenOffset, positionOffset.y, 0);
                }

            }
        }

        public void FadeOut()
        {
            visibleElements = transform.GetComponentsInChildren<SpriteRenderer>();
            fading = Fade.FadeOut;
            /*foreach(SpriteRenderer visible in visibleElements){
				visible.color = new Color(1,1,1,0);
			}*/
        }

        public void FadeIn()
        {
            //foreach(SpriteRenderer visible in visibleElements){
            //	visible.color = new Color(1,1,1,1);
            //}
            if (visibleElements == null)
                visibleElements = transform.GetComponentsInChildren<SpriteRenderer>();
            fading = Fade.FadeIn;
        }

        public bool HasItem(string itemName)
        {
            foreach (InventoryItem item in items)
            {
                if (item.gameObject.name == itemName)
                {
                    if (item.state == InventoryItem.State.INVENTORY || item.state == InventoryItem.State.INVENTORY_HOLDING)
                        return true;
                }
            }

            return false;
        }

        // Update is called once per frame
        void LateUpdate()
        {

            if (isHidden)
            {
                if (positionOk)
                {
                    positionOffset = new Vector3(-hiddenOffset, positionOffset.y, 0);
                }
                else
                {
                    if (positionOffset.x - 0.01f < -hiddenOffset)
                    {
                        positionOffset = new Vector3(-hiddenOffset, positionOffset.y, 0);
                        positionOk = true;
                    }
                    else
                        positionOffset = Vector3.Lerp(positionOffset, new Vector3(-hiddenOffset, positionOffset.y, 0), Time.deltaTime * 8);
                }
            }
            else
            {
                if (positionOk)
                {
                }
                else
                {
                    if (positionOffset.x + 0.01f > 0)
                    {
                        positionOffset = new Vector3(0, positionOffset.y, 0);
                        positionOk = true;
                    }
                    else
                        positionOffset = Vector3.Lerp(positionOffset, new Vector3(0, positionOffset.y, 0), Time.deltaTime * 8);
                }
            }

            if (fading == Fade.FadeOut)
            {
                foreach (SpriteRenderer visible in visibleElements)
                {
                    visible.color = Color.Lerp(visible.color, hiddenColor, 0.1f);
                    if (visible.color == hiddenColor)
                        fading = Fade.Invisible;
                }
            }
            else if (fading == Fade.FadeIn)
            {
                foreach (SpriteRenderer visible in visibleElements)
                {
                    visible.color = Color.Lerp(visible.color, visibleColor, 0.1f);
                    if (visible.color == visibleColor)
                        fading = Fade.Visible;
                }
            }
            return;
        }

        public class InventoryItemComparer : IComparer
        {
            public int Compare(object x, object y)
            {

                InventoryItem i1 = (InventoryItem)x;
                InventoryItem i2 = (InventoryItem)y;

                if ((i1.inventoryIndex) < (i2.inventoryIndex)) return -1;

                if ((i1.inventoryIndex) == (i2.inventoryIndex)) return 0;

                return 1;


            }
        }
    }
}