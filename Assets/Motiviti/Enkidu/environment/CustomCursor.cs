using UnityEngine;
using System.Collections;
using TMPro;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
        public class CustomCursor : MonoBehaviour {

        Transform myTransform;

        Transform pointer;

        Animator animator;

        TextMeshPro[] lines;

        ArrayList puzzles = new ArrayList();

        Vector3 rot, pos, sca;

        Player elroy;

        public Texture2D idleCursor, activeCursor, doorCursor, puzzleHand, puzzleHandGrab, puzzleExit;

        public float cursorRotation = 0;

        bool puzzleHasBackground = false;

        public bool isHidden = false;

        public enum CursorState
        {
            IDLE = 0,
            ACTIVE = 1,
            DOOR = 2,
            PUZZLE_OVER_ITEM = 3,
            PUZZLE_HOLD_ITEM = 4,
            PUZZLE_EXIT = 5,
            INVISIBLE = 6,
        }

        public CursorState cursorState = CursorState.IDLE;

        bool inPuzzle = false;

        public GameObject linesObject;

        // Use this for initialization
        void Start () {
            if(!Application.isEditor)
                Cursor.visible = false;
            
            elroy = Global.player;
            myTransform = transform;
            animator = GetComponentInChildren<Animator>();
            linesObject.SetActive(true);
            lines = GetComponentsInChildren<TextMeshPro>();
            pointer = myTransform.Find("Pointer");
            rot = pos = sca = new Vector3(0, 0, 0);
            foreach (TextMeshPro line in lines)
            {
                line.text = "";
                line.isOverlay = true;
                
            }
            if (isHidden)
                cursorState = CursorState.INVISIBLE;
        }
        public int cursorSize = 100;
        public int activeCursorSize = 100;
        void OnGUI()
        {
            Rect rect = new Rect(Event.current.mousePosition.x - cursorSize / 2, Event.current.mousePosition.y - cursorSize / 2, cursorSize, cursorSize);
            if (cursorState == CursorState.ACTIVE)
                rect = new Rect(Event.current.mousePosition.x - activeCursorSize / 2, Event.current.mousePosition.y - activeCursorSize / 2, activeCursorSize, activeCursorSize);
            GUI.depth = -10;

            Vector2 pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);

            Matrix4x4 matrixBackup = GUI.matrix;

            GUIUtility.RotateAroundPivot(cursorRotation, pivot);

            switch (cursorState) {
                case CursorState.IDLE:
                    GUI.DrawTexture(rect, idleCursor); break;
                case CursorState.ACTIVE:
                    GUI.DrawTexture(rect, activeCursor); break;
                case CursorState.DOOR:
                    GUI.DrawTexture(rect, doorCursor); break;
                case CursorState.PUZZLE_OVER_ITEM:
                    GUI.DrawTexture(rect, puzzleHand); break;
                case CursorState.PUZZLE_HOLD_ITEM:
                    GUI.DrawTexture(rect, puzzleHandGrab); break;
                case CursorState.PUZZLE_EXIT:
                    GUI.DrawTexture(rect, puzzleExit); break;
            }

            GUI.matrix = matrixBackup;
        }

        // Update is called once per frame
        void LateUpdate () {
            if (isHidden)
                return;
            
            string text = "";
            cursorRotation = 0;
            CursorState newState = CursorState.IDLE;
            Camera cam = Global.activeCamera ? Global.activeCamera : Camera.main;
            int lastLayer = -10000;
            if (!cam || (elroy && elroy.inCutScene && !inPuzzle && !elroy.inConversation))
            {
                newState = CursorState.INVISIBLE;
            }
            else if(elroy && elroy.inConversation)
            {
                newState = CursorState.IDLE;
            }
            else
            {
                pos = cam.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0;
                myTransform.position = pos;                                             //move to mouse pos

                float height = cam.orthographicSize * 2;

                sca = Vector3.one * height / 25f;
                sca.z = 1;
                myTransform.localScale = sca;

                Collider2D[] col = Physics2D.OverlapPointAll(transform.position);       //get all colliders
                                            //default state is idle
                if (inPuzzle)
                {
                    newState = CursorState.PUZZLE_EXIT;                                 //default state in puzzle is exit
                    if (!puzzleHasBackground)
                        newState = CursorState.IDLE;                                    //if puzzle has no background default is idle
                }
                if (inPuzzle && cursorState == CursorState.PUZZLE_HOLD_ITEM
                    && Input.GetButton("Fire1"))                                        //if in puzzle and holding object do nothing
                {
                    return;
                }

                int lastColliderLayer = int.MinValue;

                

                foreach (Collider2D c in col)
                {
    //                Debug.Log(Time.time + " " + c.gameObject.name + " " + LayerMask.LayerToName(c.gameObject.layer));

                    var ii = c.gameObject.GetComponent<InteractiveItem>();

                    if(ii && ii.itemLayer < lastColliderLayer)
                    {
                        continue;
                    }
                    else
                    {
                        if(ii) lastColliderLayer = ii.itemLayer;
                    }

                    if(ii && ii.noClickBackground)
                    {
                        newState = CursorState.IDLE;
                        
                        cursorRotation = 0;
                        text = "";
                        continue;
                    }

                    if (inPuzzle)                                                       //if in puzzle 
                    {
                        //Debug.Log("in puzzle layer name" + c.gameObject.name + " layer " + c.gameObject.layer);
                        if (c.gameObject.layer == 13)                                   //if in puzzle and active object
                        {
                            newState = CursorState.PUZZLE_OVER_ITEM;
                            if (Input.GetButton("Fire1"))
                            {
                                newState = CursorState.PUZZLE_HOLD_ITEM;
                            }
                            text = "";
                            break;
                        }

                        if (c.gameObject.layer == 15)                                   //if in puzzle over background
                        {
                            newState = CursorState.IDLE;
                            cursorRotation = 0;
                            text = "";
                        }
                    }
                    else
                    {
                        if (c.gameObject.layer == 15)                                   //if in puzzle over background
                        {
                            newState = CursorState.IDLE;
                            cursorRotation = 0;
                            text = "";
                        
                        }

                        if (c.gameObject.layer == 10)
                        {
                            var d = c.gameObject.GetComponent<DoorBetweenLevels>();
                            if (d == null || !d.enabled)     //if over item
                            {
                                newState = CursorState.ACTIVE;
                            }
                            else
                            {
                                DoorBetweenLevels door1 = c.gameObject.GetComponent<DoorBetweenLevels>();
                                InteractiveItemSwitch switchItem = c.gameObject.GetComponent<InteractiveItemSwitch>();
                                if (switchItem != null)         //if over door with swithc on
                                {
                                    if (door1.enableOnlyForSwitchState != InteractiveItemSwitch.State.ANY && door1.enableOnlyForSwitchState != switchItem.state)
                                    {
                                        //newState = CursorState.ACTIVE;
                                        newState = CursorState.DOOR;
                                    }
                                    else
                                    {
                                        newState = CursorState.DOOR;
                                    }
                                }
                                else
                                {
                                    newState = CursorState.DOOR;
                                }
                            }
                            InteractiveItem item = c.gameObject.GetComponent<InteractiveItem>();
                            if (item && item.enabled && item.itemLayer > lastLayer)                       //set text from interactive item
                            {
                                lastLayer = item.itemLayer;
                                if (!string.IsNullOrEmpty(item.objectName))
                                    text = item.objectName;
                                else
                                    text = item.name;

                                cursorRotation = item.cursorRotation * -1;
                                rot.z = item.cursorRotation;           //rotate pointer to rotation set on interactive item
                                if (pointer)
                                    pointer.localEulerAngles = rot;
                            }
                        }
                        if (c.gameObject.layer == 11)           //added to disable highlightning over Inventory  
                        {
                            newState = CursorState.IDLE;
                            text = "";
                            //break;
                        }

                    
                    }
                }
            }

            if(/*string.IsNullOrEmpty(text) && */Global.inventory != null)
            {
                var invItem = Global.inventory.ItemHoveringOver();
                if(invItem != null)
                {
                    newState = CursorState.PUZZLE_OVER_ITEM;
                    text = invItem.interactiveItem.GetObjectName();
                }
            }

            cursorState = newState;
            animator.SetInteger("state", (int)cursorState);
            if (text != lines[0].text)
            {
                foreach (TextMeshPro line in lines)
                {
                    line.text = text;
                }
            }
            if (newState == CursorState.PUZZLE_EXIT && Input.GetButtonDown("Fire1"))
            {
                foreach (GameObject g in puzzles)
                {
                    g.SendMessage("CloseClickedGUI", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void SetInPuzzle(bool isInPuzzle, int hasBackground)
        {
            inPuzzle = isInPuzzle;
            if (hasBackground == 1)
                puzzleHasBackground = true;
            else
                puzzleHasBackground = false;
        }

        public void AddToPuzzleList(GameObject g)
        {
            puzzles.Add(g);
        }
    }
}