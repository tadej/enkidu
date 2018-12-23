using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
        
    public class AdvCamera : MonoBehaviour
    {

        public Transform autoParallaxInitialisationParent;

        public string[] autoParallaxInitialisationBG0;
        public string[] autoParallaxInitialisationBG1;
        public string[] autoParallaxInitialisationBG2;
        public string[] autoParallaxInitialisationBG3;

        public string[] autoParallaxInitialisationFG0;
        public string[] autoParallaxInitialisationFG1;
        public string[] autoParallaxInitialisationFG2;

        Vector3  XYCentralizationDiff = new Vector3(-0.5f,-0.5f,0);

        public float screenShakeMagnitude = 0.05f;

        public bool screenShake = false;
        Player elroyAdv;
        Transform scene;
        Inventory inventory;

        float minx = 999999999;
        float maxx = -999999999;
        float miny = 999999999;
        float maxy = -999999999;

        float aspectRatio = 4 / 3;
        public float cameraWidth = 1, cameraHeight = 1;

        public float cameraSize = 6.5f;
        Vector3 originalPosition = Vector3.zero;
        float originalCameraSize = 6.5f;

        public float yFollowOffset = 3.2f;

        public bool inCloseUp = false;

        public bool followElroy = true;

        public bool forcedCameraBoundaries = false;

        public float maxCameraX = 0;

        public float minCameraX = 0;

        public float maxCameraY = 0;

        public float minCameraY = 0;

        public float smallCameraSize = 0;

        bool isPeekPanning = false;

        public float cameraSegments = 4;
        float dnx = 0;

        public Transform[] parallaxForeground;
        public Transform[] parallaxBackground;
        public Transform[] parallaxBackground1;
        public Transform[] parallaxBackground2;
        public Transform[] parallaxBackground3;

        public float parallaxFactorForeground = 0.18f;

        public float parallaxFactorBackground = -0.12f;

        public float parallaxFactorBackground1 = -0.2f;

        public float parallaxFactorBackground2 = -0.3f;

        public float parallaxFactorBackground3 = -0.5f;

        Hashtable originalParallaxPositions = new Hashtable();

        public bool followCameraObject = false;

        public Transform followingGameObject;

        float targetSmoothCloseUpSize = 4;
        Vector3 targetSmoothCloseUpPosition;

        float cameraCloseUpSpeed = 1;

        public bool canZoom = true;

        float lastChangeTime;

        public bool canMove = true;

        float parallaxMinx, parallaxMaxx;

        Vector3 peekPanningStartPosition = Vector3.zero;
        Vector3 peekPanningTargetPosition = Vector3.zero;
        float peekPanningStartTime = 0;
        bool elroyFollowShouldBeTrue = false;
        float moveElroyToScreenEdgeTimeStamp = 0;

        float followObjectSpeed = 1;

        public float zoomSpeed = 5.0f;
        public float minZoomAmount = 0;
        public float maxZoomAmount = 2;
        public float SmoothSpeed = 4.0f;

        float defaultFov = 0;
        float defaultOrthoSize = 0;
        public float idealZoomAmount = 0;
        float zoomAmount = 0;

        float defaultSmoothSpeed;

        float defaultMinZoomAmount;

        float defaultMaxZoomAmount;

        public bool closeFollow = false;
        public Vector3 closeFollowOffset = Vector3.zero;
        public bool closeFollowXOnly = false;

        Transform cameraObject;

        new Camera camera;

        // Use this for initialization
        void Awake()
        {
            if (autoParallaxInitialisationParent != null)
            {
                AddAutoParallaxLayers(autoParallaxInitialisationParent);
            }

            if (isSmallScreen())
            {
                cameraSize = smallCameraSize;
            }

            // end awake
            cameraObject = new GameObject().transform;
            cameraObject.transform.parent = transform.parent;
            cameraObject.transform.position = transform.position;

            camera = GetComponent<Camera>();

            originalPosition = cameraObject.transform.position;
            if (isSmallScreen())
            {
                cameraSize = smallCameraSize;
            }
            scene = Global.scene.transform;
            inventory = Global.inventory;
            elroyAdv = Global.player;
            Global.activeCamera = this.camera;
            aspectRatio = camera.aspect;
            originalCameraSize = cameraSize;
            GetTransformBounds(scene, ref  minx, ref  maxx, ref  miny, ref  maxy);

            UpdateCameraSize(cameraSize);

            foreach (Transform tr in parallaxBackground)
            {
                if (tr == null)
                    continue;
                originalParallaxPositions[tr.gameObject.GetInstanceID()] = tr.localPosition;
            }
            foreach (Transform tr in parallaxBackground1)
            {
                if (tr == null)
                    continue;
                originalParallaxPositions[tr.gameObject.GetInstanceID()] = tr.localPosition;
            }
            foreach (Transform tr in parallaxBackground2)
            {
                if (tr == null)
                    continue;
                originalParallaxPositions[tr.gameObject.GetInstanceID()] = tr.localPosition;
            }

            foreach (Transform tr in parallaxBackground3)
            {
                if (tr == null)
                    continue;
                originalParallaxPositions[tr.gameObject.GetInstanceID()] = tr.localPosition;
            }

            foreach (Transform tr in parallaxForeground)
            {
                if (tr == null)
                    continue;
                originalParallaxPositions[tr.gameObject.GetInstanceID()] = tr.localPosition;
            }

            minx += 0.1f;
            maxx -= 0.1f;
            miny += 0.1f;
            maxy -= 0.1f;

            if (followElroy) elroyFollowShouldBeTrue = true;

            if (forcedCameraBoundaries)
            {
                minx = minCameraX;
                maxx = maxCameraX;
                miny = minCameraY;
                maxy = maxCameraY;
            }

            SetDnx();

            SetDefaults();

            parallaxMaxx = maxx;
            parallaxMinx = minx;

            if (followElroy)
                cameraObject.transform.position = transform.position = GetCameraPosFromElroy();

            ApplyParallax();
        }

        IEnumerator Start()
        {
            yield return null;
            inventory = Global.inventory;

            if (followElroy)
                cameraObject.transform.position = transform.position = GetCameraPosFromElroy();
        }

        public void Move(Vector3 newPos, bool onlyIfFollowElroy = true)
        {
            if(followElroy || !onlyIfFollowElroy)
            {
                transform.position = newPos;
            }
        }

        bool IsStringIncluded(string str, string[] strings)
        {
            foreach (string s in strings)
            {
                if (str == s) return true;
            }

            return false;
        }

        void AddParallaxLayer(ref Transform[] parallaxBackground, Transform tr)
        {
            if (parallaxBackground == null) parallaxBackground = new Transform[1];
            else
            {
                Transform[] pb = new Transform[parallaxBackground.Length + 1];
                for (int i = 0; i < parallaxBackground.Length; i++)
                    pb[i] = parallaxBackground[i];

                parallaxBackground = pb;
            }

            parallaxBackground[parallaxBackground.Length - 1] = tr;
        }

        public float GetOriginalSize()
        {
            return originalCameraSize;
        }

        void AddAutoParallaxLayers(Transform tr)
        {
            if (IsStringIncluded(tr.gameObject.name, autoParallaxInitialisationBG0))
            {
                AddParallaxLayer(ref parallaxBackground, tr);
            }
            else
            if (IsStringIncluded(tr.gameObject.name, autoParallaxInitialisationBG1))
            {
                AddParallaxLayer(ref parallaxBackground1, tr);
            }
            else
            if (IsStringIncluded(tr.gameObject.name, autoParallaxInitialisationBG2))
            {
                AddParallaxLayer(ref parallaxBackground2, tr);
            }
            else
            if (IsStringIncluded(tr.gameObject.name, autoParallaxInitialisationBG3))
            {
                AddParallaxLayer(ref parallaxBackground3, tr);
            }
            else
            if (IsStringIncluded(tr.gameObject.name, autoParallaxInitialisationBG3))
            {
                AddParallaxLayer(ref parallaxBackground3, tr);
            }
            else
            if (IsStringIncluded(tr.gameObject.name, autoParallaxInitialisationFG0))
            {
                AddParallaxLayer(ref parallaxForeground, tr);
            }

            foreach (Transform child in tr)
            {
                AddAutoParallaxLayers(child);
            }

        }

        public bool isSmallScreen()
        {
            if (smallCameraSize < 0.1f)
            {
                smallCameraSize = cameraSize * 0.8f;
            }
            if (Application.isEditor)
            {
                if (Global.isSmallScreen)
                {
                    Debug.Log("small screen");
                    return true;
                }
            }

            return true;
        }

        void SetDnx()
        {
            cameraSegments = (maxx - minx) / cameraWidth;
            dnx = (maxx - minx) / cameraSegments;
        }

        public void setNewForcedCameraBoundaries(float newMinx, float newMaxX, float? newMinY=null, float? newMaxY=null, bool useOldForParallax = false)
        {
            if (useOldForParallax)
            {
                minx = minCameraX = newMinx;
                maxx = maxCameraX = newMaxX;

                if(newMinY != null)
                {
                    miny = minCameraY = (float)newMinY;
                    maxy = maxCameraY = (float)newMaxY;
                }
            }
            else
            {
                parallaxMinx = minx = minCameraX = newMinx;
                parallaxMaxx = maxx = maxCameraX = newMaxX;
                            
                if(newMinY != null)
                {
                    miny = minCameraY = (float)newMinY;
                    maxy = maxCameraY = (float)newMaxY;
                }
            }
            UpdateCameraSize(cameraSize);
            SetDnx();
        }

        public void SetForcedCameraBoundariesInCurrentPos()
        {
            Debug.Log("set forced camera boundaries in current pos");
            Vector3 curPos = transform.position;
            parallaxMinx = minx = minCameraX = curPos.x - cameraWidth;
            parallaxMaxx = maxx = maxCameraX = curPos.x + cameraWidth;
            miny = minCameraY = curPos.y - cameraHeight;
            maxy = maxCameraY = curPos.y + cameraHeight;
        }

        public void CloseUpBegin(float size, Vector3 pos, float speed = 1)
        {
            cameraCloseUpSpeed = speed;
            Debug.Log("beginning closeup " + size);
            targetSmoothCloseUpSize = size;
            targetSmoothCloseUpPosition = pos;
            //	camera.transform.position = pos;
            isPeekPanning = false;
            inCloseUp = true;

            canZoom = false;
            IdealZoomAmount = 0;
        }

        public void CloseUpEnd()
        {
            if (inCloseUp)
            {
                Debug.Log("ending closeup " + originalCameraSize);
                //	UpdateCameraSize(originalCameraSize);

                targetSmoothCloseUpSize = originalCameraSize;

                inCloseUp = false;

                StartCoroutine(CloseUpEndCoroutine());
            }
        }


        IEnumerator CloseUpEndCoroutine()
        {
            defaultMinZoomAmount = minZoomAmount;

            defaultMaxZoomAmount = maxZoomAmount;

            defaultSmoothSpeed = SmoothSpeed;

            SmoothSpeed = cameraCloseUpSpeed;

            idealZoomAmount = 0f;

            zoomAmount = defaultOrthoSize - camera.orthographicSize;

            if (zoomAmount < minZoomAmount)
            {
                minZoomAmount = zoomAmount;
            }
            else if (zoomAmount > maxZoomAmount)
            {
                maxZoomAmount = zoomAmount;
            }

            float defaultMiny = miny;

            float defaultMaxy = maxy;

            while (Mathf.Abs(idealZoomAmount - zoomAmount) > 0.01f)
            {
                Vector3 cameraPos = transform.localPosition;
                miny = cameraPos.y - cameraHeight;
                maxy = cameraPos.y + cameraHeight;
                if (miny > defaultMiny)
                    miny = defaultMiny;
                if (maxy < defaultMaxy)
                    maxy = defaultMaxy;
                yield return new WaitForSeconds(0.01f);
                if (zoomAmount < defaultMinZoomAmount)
                    minZoomAmount = zoomAmount;
                else if (zoomAmount > defaultMaxZoomAmount)
                    maxZoomAmount = zoomAmount;
            }

            canZoom = true;
            maxy = defaultMaxy;
            miny = defaultMiny;
            SmoothSpeed = defaultSmoothSpeed;
            minZoomAmount = defaultMinZoomAmount;
            maxZoomAmount = defaultMaxZoomAmount;
        }

        void UpdateCameraSize(float newSize)
        {
    //        Debug.Log("updating camera size from " + camera.orthographicSize + " to " + newSize);
            cameraSize = newSize;
            camera.orthographicSize = newSize;


            cameraHeight = newSize;
            cameraWidth = newSize * aspectRatio;
            //	DefaultFov = newSize;
            //	DefaultOrthoSize = newSize;
    
            
            if (inventory != null)
            {
                /*
                Vector3 pos = inventory.transform.localPosition;

                pos.x = -cameraWidth;
                pos.y = cameraHeight + 0.05f;

                inventory.transform.localPosition = pos + inventory.positionOffset;
                */
                inventory.recalculateHiddenOffset();
            }
            
        }

        void GetTransformBounds(Transform tr, ref float minx, ref float maxx, ref float miny, ref float maxy)
        {
            if (tr.gameObject.name == "Characters" || tr.gameObject.name == "DynamicItems" || tr.gameObject.name == "Clouds")
            {
                return;
            }

            if (tr && tr.GetComponent<Renderer>())
            {
                Bounds b = tr.GetComponent<Renderer>().bounds;

                if (b.min.x < minx) minx = b.min.x;
                if (b.max.x > maxx) { maxx = b.max.x; }

                if (b.min.y < miny) miny = b.min.y;
                if (b.max.y > maxy) maxy = b.max.y;
            }
            else
                if (!tr.GetComponent<Renderer>() && (tr.gameObject.name == "limiterLeft" || tr.gameObject.name == "limiterRight"))
                {
                    if (tr.position.x < minx) minx = tr.position.x;
                    if (tr.position.x > maxx) { maxx = tr.position.x; }

                    if (tr.position.y < miny) miny = tr.position.y;
                    if (tr.position.y > maxy) maxy = tr.position.y;

                }

            if (tr)
            {
                foreach (Transform child in tr)
                {
                    GetTransformBounds(child, ref minx, ref maxx, ref miny, ref maxy);
                }
            }
        }

        public void CenterOnElroy()
        {
            transform.position = GetCameraPosFromElroy();
        }
        public Vector3 GetCameraPosFromElroy(bool floor = true)
        {
            if(closeFollow)
            {
                return new Vector3(Global.player.transform.position.x, closeFollowXOnly ? originalPosition.y : Global.player.transform.position.y, -5) + closeFollowOffset;	
            }

            if (elroyAdv)
            {
                Vector3 cameraPos = cameraObject.transform.position;

                if (Global.player.inCutScene)
                {
                    cameraPos.x = Global.player.transform.position.x + Global.player.direction * 1f;
                    cameraPos.y = Global.player.transform.position.y + yFollowOffset;

                    return cameraPos;
                }

                cameraPos.x = Global.player.transform.position.x;// + Global.elroy.direction * 0.6f;

                cameraPos.y = Global.player.transform.position.y + yFollowOffset;

                float m = floor ? Mathf.Floor(cameraPos.x / dnx) : Mathf.Ceil(cameraPos.x / dnx);

                cameraPos.x = dnx * m + dnx * 0.5f;

                cameraPos = EnforceCameraBoundaries(cameraPos);

                if( (Global.player.direction > 0 && cameraPos.x < cameraObject.transform.position.x) || (Global.player.direction < 0 && cameraPos.x > cameraObject.transform.position.x))
                {
                    if(true)
                    {
                        cameraPos = cameraObject.transform.position;
                    }
                }

                cameraPos = EnforceCameraBoundaries(cameraPos);

                cameraPos.z = -5;

                return cameraPos;
            }
            else
                return elroyAdv.transform.position;
        }

        Vector3 GetCameraPosNextSlot(bool floor = true)
        {
            Vector3 cameraPos = cameraObject.transform.position;
            cameraPos.x = cameraObject.transform.position.x + Global.player.direction * 0.6f;
            cameraPos.y = Global.player.transform.position.y + yFollowOffset;

            float m = floor ? Mathf.Floor(cameraPos.x / dnx) : Mathf.Ceil(cameraPos.x / dnx);

            cameraPos.x = dnx * m + dnx * 0.5f;
            //if(Global.elroy.transform.position.x < cameraPos.x) cameraPos.x = Global.elroy.tra
            cameraPos = EnforceCameraBoundaries(cameraPos);

            return cameraPos;
        }

        public Vector3 EnforceCameraBoundaries(Vector3 cameraPos)
        {
            if (followCameraObject)
                return cameraPos;
            
            if (cameraPos.x < minx + cameraWidth)
                cameraPos.x = minx + cameraWidth;
            //if(Global.elroy.transform.position.x > cameraPos.x) cameraPos.x += Time.deltaTime;
            if (cameraPos.x > maxx - cameraWidth)
                cameraPos.x = maxx - cameraWidth;
            //if(Global.elroy.transform.position.y < cameraPos.y) cameraPos.y -= Time.deltaTime;
            if (cameraPos.y < miny + cameraHeight)
                cameraPos.y = miny + cameraHeight;
            //if(Global.elroy.transform.position.y > cameraPos.y) cameraPos.y += Time.deltaTime;
            if (cameraPos.y > maxy - cameraHeight)
                cameraPos.y = maxy - cameraHeight;

            return cameraPos;
        }


        void ForceCameraIntoBoundaries(Vector3 cameraPos)
        {
            if (cameraPos.x < minx + cameraWidth)
                cameraPos.x = minx + cameraWidth;
            //if(Global.elroy.transform.position.x > cameraPos.x) cameraPos.x += Time.deltaTime;
            if (cameraPos.x > maxx - cameraWidth)
                cameraPos.x = maxx - cameraWidth;
            //if(Global.elroy.transform.position.y < cameraPos.y) cameraPos.y -= Time.deltaTime;
            if (cameraPos.y < miny + cameraHeight)
                cameraPos.y = miny + cameraHeight;
            //if(Global.elroy.transform.position.y > cameraPos.y) cameraPos.y += Time.deltaTime;
            if (cameraPos.y > maxy - cameraHeight)
                cameraPos.y = maxy - cameraHeight;

            cameraObject.transform.position = cameraPos;
        }

        void FollowCharacter()
        {
            if (!canMove)
                return;

            if (followCameraObject)
            {
                if (followingGameObject)
                {
                    Vector3 cameraPos = cameraObject.transform.position;
                    cameraPos.x = followingGameObject.transform.position.x;

                    cameraPos.y = followingGameObject.transform.position.y + yFollowOffset;

                    //			float m = floor ? Mathf.Floor (cameraPos.x / dnx) : Mathf.Ceil (cameraPos.x / dnx);

                    //			cameraPos.x = dnx * m + dnx * 0.5f;
                    //if(Global.elroy.transform.position.x < cameraPos.x) cameraPos.x = Global.elroy.tra

                    cameraPos = EnforceCameraBoundaries(cameraPos);

                    if (followObjectSpeed > 99)
                    {
                        cameraObject.transform.position = cameraPos;
                    }
                    else
                        cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position, cameraPos, Time.deltaTime * followObjectSpeed);

                    camera.orthographicSize = followingGameObject.transform.localScale.x;

                    UpdateCameraSize(followingGameObject.transform.localScale.x);
                }
            }
            else if (followElroy)
            {
                if(closeFollow)
                {
                    cameraObject.transform.position = Vector3.Lerp (GetComponent<Camera>().transform.position, GetCameraPosFromElroy(), Time.deltaTime);
                    
                    
                }
                else
                if (elroyAdv != null && (Time.time < 0.5f || Global.player.TimeSinceSetDestination() > 0.5f))
                {

                    {

                        var cameraPos = GetCameraPosFromElroy( /*Global.elroy.direction < 0 */);

                        float step = Time.deltaTime * 0.1f;

                        if (cameraObject.transform.position.x > Global.player.transform.position.x)
                        {
                            if (Global.player.GetXVelocity() > 0)
                            {
                                step *= 0.1f;
                            }
                            else
                            {
                                step *= 1.8f;
                            }
                        }

                        if (cameraObject.transform.position.x < Global.player.transform.position.x)
                        {
                            if (Global.player.GetXVelocity() < 0)
                            {
                                step *= 0.1f;
                            }
                            else
                            {
                                step *= 1.8f;
                            }
                        }
                        Vector3 currentVelocity = Vector3.zero;

                        cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position, cameraPos, Time.deltaTime);//Vector3.SmoothDamp(cameraObject.transform.position, cameraPos, ref currentVelocity, 0.10f, 20);
                    }

                    ForceCameraIntoBoundaries(cameraObject.transform.position);
    //                Debug.Log ("miny: " + (miny + cameraHeight/2) + " y: " + camera.transform.position.y);
                }
                else
                {
                    elroyAdv = Global.player;
                }
            }
            else
            {

                if (elroyAdv != null)
                {
                    
                    var cameraPos = GetCameraPosFromElroy();
                    cameraPos.x = cameraObject.transform.position.x;
                    cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position, cameraPos, Time.deltaTime * 0.04f);
                    
                    //Debug.Log ("miny: " + (miny + cameraHeight/2) + " y: " + camera.transform.position.y);
                }
                else
                {
                    elroyAdv = Global.player;
                }

            }
        }

        void MoveElroyToScreenEdge(bool leftEdge)
        {
            Debug.Log("MoveElroyToScreenEdge");

            if (Time.time - moveElroyToScreenEdgeTimeStamp > /* 1 MR  2.6. */0.1f && Global.player.IsIdleOrWalking())
            {
                //	Debug.Log ("MoveElroyToScreenEdge 1");
                Vector3 pos = Global.player.transform.position;

                //	Debug.Log ("MoveElroyToScreenEdge " + leftEdge);

                float camDelta = 0.2f * cameraSize;

                if (leftEdge)
                {
                    pos.x = cameraObject.transform.position.x - cameraWidth - camDelta;
                    if (pos.x > Global.player.transform.position.x) Global.player.TeleportToPosition(pos);
                }
                else
                {
                    pos.x = cameraObject.transform.position.x + cameraWidth + camDelta;
                    if (pos.x < Global.player.transform.position.x) Global.player.TeleportToPosition(pos);
                }

                Vector3 newPos = leftEdge ? (pos + Vector3.right * cameraWidth * 0.5f) + Vector3.right * InventoryMovementAddition() : (pos - Vector3.right * cameraWidth * 0.5f);

                StartCoroutine(SetDestinationDelayed(newPos, 0.5f));
                //StartCoroutine (DisableAndReenableFollow (3));

                moveElroyToScreenEdgeTimeStamp = Time.time;
            }
        }

        IEnumerator SetDestinationDelayed(Vector3 dest, float delay)
        {
            Global.player.justReleasedAfterPeekPanning = true;
            //Global.elroy.inCutScene = true;

            yield return new WaitForSeconds(delay);

            //Global.elroy.inCutScene = false;
            StartCoroutine(Global.player.SetDestinationInterim(dest));

            yield return null;
        }

        public void SetPeekPanning(bool b)
        {
    //		Debug.Log ("SetPeekPanning " + b);

            if (inCloseUp && b)
                b = false;

            isPeekPanning = b;

            if (b)
            {
                StopCoroutine("ZoomBackToDefault");
                peekPanningStartTime = Time.time;
                if(inventory)inventory.EnableItemUsage(false);
                peekPanningStartPosition = cameraObject.transform.position;
                peekPanningTargetPosition = cameraObject.transform.position;
            }
            else
            {
                if(inventory)inventory.EnableItemUsage(true);
                followElroy = false;
                //camera.transform.position = EnforceCameraBoundaries(peekPanningStartPosition);

                if (IsElroyBeyondLeftScreenEdge())
                {
                    MoveElroyToScreenEdge(true);
                    //Global.elroy.SetDestination( pos );
                }
                else
                    if (IsElroyBeyondRightScreenEdge())
                    {
                        MoveElroyToScreenEdge(false);
                        //Global.elroy.SetDestination( pos );
                    }
                    else
                    {
                        //StartCoroutine( DisableAndReenableFollow(4, true) );
                    }

                lastChangeTime = Time.time;
                StartCoroutine(ZoomBackToDefault());
            }
        }

        float InventoryMovementAddition()
        {
            return (inventory.IsVisible() ? cameraWidth * 0.3f : 0);
        }

        bool IsElroyBeyondLeftScreenEdge()
        {
            return (Global.player.transform.position.x < cameraObject.transform.position.x - cameraWidth + cameraWidth * 0.1f + InventoryMovementAddition());
        }

        bool IsElroyBeyondRightScreenEdge()
        {
            return (Global.player.transform.position.x > cameraObject.transform.position.x + cameraWidth - cameraWidth * 0.1f);
        }

        public void SetFollowObjectMode(bool b, Transform following, float speed = 1)
        {
            Debug.Log ("set folow object mode: " + b);
            followCameraObject = b;
            followingGameObject = following;
            followElroy = !b;
            followObjectSpeed = speed;
            canZoom = false;
            if (b)
                isPeekPanning = false;
        }

        public bool IsVisible(Transform tr)
        {
            if (tr.transform.position.x > cameraObject.transform.position.x + cameraWidth * 1.1f) return false;

            if (tr.transform.position.x < cameraObject.transform.position.x - cameraWidth * 1.1f) return false;

            if (tr.transform.position.y > cameraObject.transform.position.y + cameraHeight * 1.1f) return false;

            if (tr.transform.position.y < cameraObject.transform.position.y - cameraHeight * 1.1f) return false;

            return true;
        }

        public void ElroyMoved()
        {
            if (elroyFollowShouldBeTrue)
                followElroy = true;
        }

        public IEnumerator DisableAndReenableFollow(float sec, bool reenable = false)
        {
            followElroy = false;
            yield return new WaitForSeconds(sec);
            if (reenable) {
                followElroy = true;
                elroyFollowShouldBeTrue = true;
            }
        }

        public void AdjustPeekPanning(Vector3 delta)
        {
            lastChangeTime = Time.time;
            peekPanningTargetPosition = peekPanningStartPosition + delta;
        }

        public bool WasPeekingLongEnough()
        {
            return isPeekPanning && ((Time.time - peekPanningStartTime) > 0.2f);
        }

        public bool IsPeekPanning()
        {
            return isPeekPanning;// && ((Time.time - peekPanningStartTime) > 0.05f);
        }

        // Update is called once per frame
        void Update()
        {
            
            if (canMove && !inCloseUp)
            {
                if (Mathf.Abs(camera.orthographicSize - originalCameraSize) > 0.0001f && idealZoomAmount == 0)
                {
                    UpdateCameraSize(Mathf.Lerp(camera.orthographicSize, originalCameraSize, Time.deltaTime * cameraCloseUpSpeed));
                }
            }
        }
        int paralax_counter = 20;
        void ApplyParallax()
        {
            if (paralax_counter < 20)
            {
                paralax_counter++;
                if(paralax_counter != 1)
                    return;
            }
            float cameraDX = cameraObject.transform.position.x - (parallaxMinx + (parallaxMaxx - parallaxMinx) * 0.5f);
            float cameraDY = cameraObject.transform.position.y - (miny + (maxy - miny) * 0.5f);

            Vector3 dv = new Vector3(cameraDX, cameraDY, 0);
            
            foreach (Transform tr in parallaxBackground)
            {
                if (tr == null) continue;

                if (tr && tr.gameObject)
                {

                    Vector3 originalPosition = (Vector3)originalParallaxPositions[tr.gameObject.GetInstanceID()];

                    tr.localPosition = originalPosition - dv * parallaxFactorBackground;
                }
            }

            foreach (Transform tr in parallaxBackground1)
            {
                if (tr == null) continue;

                if (tr && tr.gameObject)
                {

                    Vector3 originalPosition = (Vector3)originalParallaxPositions[tr.gameObject.GetInstanceID()];

                    tr.localPosition = originalPosition - dv * parallaxFactorBackground1;
                }
            }

            foreach (Transform tr in parallaxBackground2)
            {
                if (tr == null) continue;

                if (tr && tr.gameObject)
                {

                    Vector3 originalPosition = (Vector3)originalParallaxPositions[tr.gameObject.GetInstanceID()];

                    tr.localPosition = originalPosition - dv * parallaxFactorBackground2;
                }
            }

            foreach (Transform tr in parallaxBackground3)
            {
                if (tr == null) continue;

                if (tr && tr.gameObject)
                {

                    Vector3 originalPosition = (Vector3)originalParallaxPositions[tr.gameObject.GetInstanceID()];

                    tr.localPosition = originalPosition - dv * parallaxFactorBackground3;
                }
            }

            foreach (Transform tr in parallaxForeground)
            {
                if (tr == null) continue;

                if (tr && tr.gameObject)
                {

                    Vector3 originalPosition = (Vector3)originalParallaxPositions[tr.gameObject.GetInstanceID()];

                    tr.localPosition = originalPosition - dv * parallaxFactorForeground;
                }
            }
        }

        void LateUpdate()
        {
            if (inCloseUp)
            {
                UpdateCameraSize(Mathf.Lerp(camera.orthographicSize, targetSmoothCloseUpSize, Time.deltaTime * cameraCloseUpSpeed));
                cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position, targetSmoothCloseUpPosition, Time.deltaTime * cameraCloseUpSpeed);
                ApplyParallax();
                return;
            }

            if (IsPeekPanning() && !Global.inPause)
            {
                if (canMove)
                    cameraObject.transform.position = EnforceCameraBoundaries(peekPanningTargetPosition);
            }

            if (!inCloseUp)
            {
                if (!Global.takingVideo)
                    FollowCharacter();

            }

            ApplyParallax();

            
            
            if (canZoom || idealZoomAmount == 0)
                ZoomAmount = Mathf.Lerp(ZoomAmount, IdealZoomAmount, Time.deltaTime * SmoothSpeed);


            cameraSize = this.camera.orthographicSize;
            //Camera.main.orthographicSize;

            if (Mathf.Abs(camera.orthographicSize - originalCameraSize) > 0.0001f && idealZoomAmount == 0)
            {
                UpdateCameraSize(cameraSize);
            }

            if (canMove)
                cameraObject.transform.position = EnforceCameraBoundaries(cameraObject.transform.position);

            camera.transform.position = cameraObject.transform.position;

            if(screenShake) 
            {
                camera.transform.position += (new Vector3(Random.value , Random.value , 0) + XYCentralizationDiff) * screenShakeMagnitude;
            }

            if(inventory)inventory.recalculateHiddenOffset();

            
        }

        public void SetNewCameraSize(float size)
        {
            
            //Debug.Log("setting camera size from " + camera.orthographicSize + " to " + size);
            cameraSize = size;
            camera.orthographicSize = size;
            //Camera.main.orthographicSize = size;
            cameraHeight = size;
            cameraWidth = size * aspectRatio;
            UpdateCameraSize(size);

            DefaultFov = size;
            DefaultOrthoSize = size;
        }

    


        public float DefaultFov
        {
            get { return defaultFov; }
            set { defaultFov = value; }
        }

        public float DefaultOrthoSize
        {
            get { return defaultOrthoSize; }
            set { defaultOrthoSize = value; }
        }

        public float IdealZoomAmount
        {
            get { return idealZoomAmount; }
            set { idealZoomAmount = Mathf.Clamp(value, minZoomAmount, maxZoomAmount); }
        }

        public float ZoomAmount
        {
            get { return zoomAmount; }
            set
            {
                zoomAmount = Mathf.Clamp(value, minZoomAmount, maxZoomAmount);

                if (camera.orthographic && canZoom)
                {
                //    Debug.Log("zoom amount " + camera.orthographicSize + " to " + zoomAmount);
                    camera.orthographicSize = Mathf.Max(defaultOrthoSize - zoomAmount, 0.1f);
                }
                else
                {
                    CameraFov = Mathf.Max(defaultFov - zoomAmount, 0.1f);
                }
            }
        }

        float CameraFov
        {
            get { return camera.fieldOfView; }
            set { camera.fieldOfView = value; }
        }

        public float ZoomPercent
        {
            get { return (ZoomAmount - minZoomAmount) / (maxZoomAmount - minZoomAmount); }
        }

        public void SetDefaults()
        {
            DefaultFov = CameraFov;
            DefaultOrthoSize = camera.orthographicSize;
        }

        // Handle the pinch event
        public void setCutscene(bool b)
        {
            //Debug.Log("camera cutscene: " + b);
            if (elroyAdv && !Global.player.inCutScene)
                b = true;
            canZoom = b;
            if (!b)
            {
                IdealZoomAmount = 0;
            }
        }

        // void OnPinch(PinchGesture gesture)
        // {
        //     if (canZoom)
        //     {
        //         lastChangeTime = Time.time;
        //         IdealZoomAmount += zoomSpeed * gesture.Delta.Centimeters();
        //         StartCoroutine(ZoomBackToDefault());
        //     }
        // }

        IEnumerator ZoomBackToDefault()
        {
            yield return new WaitForSeconds(3.3f);
            if (Time.time - lastChangeTime > 3.3f)
            {
                //			Debug.Log("setting IdealZoomAmount zoom to: " + 0);
                IdealZoomAmount = 0;
            }
        }

        public void CanCamMove(bool can)
        {
            Debug.LogWarning("Can camera move: " + can);
            canMove = can;
        }

    }
}