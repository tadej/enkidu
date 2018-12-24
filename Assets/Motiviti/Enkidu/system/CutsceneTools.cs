using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class CutsceneTools : MonoBehaviour
    {

        ArrayList children;

        Color colorVisible = new Color(1, 1, 1, 1);
        Color colorHidden = new Color(1, 1, 1, 0);

        public SpriteRenderer bandTop, bandBottom, fade;
        public SpriteRenderer zoomoutCircle;
        public SpriteRenderer zoomoutLeft, zoomoutRight, zoomoutTop, zoomoutBottom;

        public Transform zoomOutTransform;

        Vector3 zoomoutLeftDef, zoomoutRightDef, zoomoutTopDef, zoomoutBotDef;

        public Vector3 bandTopPositionShown, bandBottomPositionShown;

        public Vector3 bandTopPositionHidden, bandBottomPositionHidden;

        AdvCamera advCamera;

        float fadeOutSpeed = 4;

        public enum Type
        {
            None = 0,
            BlackBands,
            //Circle,
            Puzzle,
            ZoomIn,
            ZoomOut,
            FadeOut
        }


        public Type type = Type.BlackBands;

        float zoomScaleFactor = 4;

        float zoomFactor = 1f;

        float topBoundMin = 0.95f;

        Vector3 topBandPos;

        float minScale = 0.00001f;

        public enum CircleState
        {
            disabled,
            startedOn,
            scalingUp,
            scalingDown
        }

        public CircleState circleState = CircleState.disabled;

        public bool DoZoomouts = false;

        void Awake()
        {
            advCamera = PersistentEngine.advCamera;
        }

        void Start()
        {
            zoomoutLeftDef = zoomoutLeft.transform.localPosition;
            zoomoutRightDef = zoomoutRight.transform.localPosition;
            zoomoutTopDef = zoomoutTop.transform.localPosition;
            zoomoutBotDef = zoomoutBottom.transform.localPosition;

            bandTop.enabled = bandBottom.enabled = false;
            bandTopPositionShown = bandTop.transform.localPosition;
            bandBottomPositionShown = bandBottom.transform.localPosition;
            topBandPos = Camera.main.ViewportToWorldPoint(new Vector3(0, topBoundMin, 0));

            DisableSprites(transform);

            if (!PersistentEngine.takingVideo) bandTop.enabled = true;
            if (!PersistentEngine.takingVideo) bandBottom.enabled = true;

            if (DoZoomouts)
            {
                EnableSprites(zoomOutTransform);
            }

            CalculateBandPositions();
        }

        void DisableSprites(Transform tr)
        {
            SpriteRenderer spr = tr.gameObject.GetComponent<SpriteRenderer>();

            if (spr != null) spr.enabled = false;

            foreach (Transform child in tr)
            {
                DisableSprites(child);
            }
        }

        void EnableSprites(Transform tr)
        {
            SpriteRenderer spr = tr.gameObject.GetComponent<SpriteRenderer>();

            if (spr != null) spr.enabled = true;

            foreach (Transform child in tr)
            {
                EnableSprites(child);
            }
        }

        void CalculateBandPositions()
        {
            topBandPos = PersistentEngine.activeCamera.ViewportToWorldPoint(new Vector3(0, topBoundMin, 0));
            Vector3 toppos = topBandPos - advCamera.transform.position;
            toppos.y = (bandTop.bounds.max.y - bandTop.bounds.min.y) / 2 + toppos.y;

            bandTopPositionShown.y = toppos.y;
            bandBottomPositionShown.y = toppos.y * (-1);

            CalculateHiddenBandPositions();
        }

        void CalculateHiddenBandPositions()
        {
            bandTopPositionHidden = bandTopPositionShown + Vector3.up * 2;
            bandBottomPositionHidden = bandBottomPositionShown - Vector3.up * 2;
        }

        public void SetFadeout(bool v)
        {
            if (v)
                type = Type.FadeOut;
        }

        IEnumerator SetZoomOutCoroutine(Vector3 position)
        {
            yield return new WaitForSeconds(0.5f);
            circleState = CircleState.scalingDown;
            position.z = 0;
            zoomOutTransform.position = position;
            //TODO inventory.SetVisible (false);
            zoomoutCircle.transform.localScale = Vector3.one * advCamera.cameraSize;
            zoomoutLeft.transform.localPosition = zoomoutLeftDef + Vector3.left * 30;
            zoomoutRight.transform.localPosition = zoomoutRightDef + Vector3.right * 30;
            zoomoutTop.transform.localPosition = zoomoutTopDef + Vector3.up * 30;
            zoomoutBottom.transform.localPosition = zoomoutBotDef + Vector3.down * 30;
        }

        public void SetCutscene(Type myType = Type.BlackBands, Vector3 position = default(Vector3), bool startOn = false, float minimumCircleSize = 0.01f)
        {
            type = myType;

            advCamera.setCutscene(myType == Type.None ? true : false);

            //Debug.Log("SetCutscene " + myType + " " + position + " " + startOn);

            switch (type)
            {
                case Type.BlackBands:
                    //TODO inventory.SetVisible (false);
                    if (!PersistentEngine.takingVideo) bandTop.transform.localPosition = startOn ? bandTopPositionShown : bandTopPositionHidden;
                    if (!PersistentEngine.takingVideo) bandBottom.transform.localPosition = startOn ? bandBottomPositionShown : bandBottomPositionHidden;
                    circleState = CircleState.scalingUp;
                    break;

                case Type.Puzzle:
                    //TODO inventory.SetVisible (false);
                    circleState = CircleState.scalingUp;
                    break;

                case Type.ZoomIn:
                    circleState = CircleState.scalingUp;
                    minScale = 0.01f;
                    //TODO inventory.SetVisible (false);
                    zoomOutTransform.position = position;
                    zoomoutCircle.transform.localScale = Vector3.one * 0.01f;
                    zoomoutLeft.transform.localPosition = zoomoutLeftDef;
                    zoomoutRight.transform.localPosition = zoomoutRightDef;
                    zoomoutTop.transform.localPosition = zoomoutTopDef;
                    zoomoutBottom.transform.localPosition = zoomoutBotDef;
                    break;
                case Type.ZoomOut:
                    StartCoroutine(SetZoomOutCoroutine(position));
                    minScale = minimumCircleSize;
                    break;
                case Type.FadeOut:
                    SetFadeout(true);
                    //TODO inventory.SetVisible (false);
                    fade.enabled = true;
                    circleState = CircleState.scalingUp;
                    break;
                default:
                    circleState = CircleState.scalingUp;
                    break;
            }
        }

        void Process()
        {
            CalculateBandPositions();

            if (type == Type.BlackBands)
            {
                if (PersistentEngine.takingVideo == false)
                {
                    Vector3 currentpos = PersistentEngine.activeCamera.WorldToViewportPoint(new Vector3(0, bandTop.GetComponent<Renderer>().bounds.min.y, 0));
                    if (currentpos.y < topBoundMin)
                    {
                        if (bandTop.enabled) bandTop.transform.localPosition = Vector3.Lerp(bandTop.transform.localPosition, bandTopPositionShown, Time.deltaTime * 6);
                        if (bandBottom.enabled) bandBottom.transform.localPosition = Vector3.Lerp(bandBottom.transform.localPosition, bandBottomPositionShown, Time.deltaTime * 6);
                    }
                    else
                    {
                        if (bandTop.enabled) bandTop.transform.localPosition = Vector3.Lerp(bandTop.transform.localPosition, bandTopPositionShown, Time.deltaTime * 6);
                        if (bandBottom.enabled) bandBottom.transform.localPosition = Vector3.Lerp(bandBottom.transform.localPosition, bandBottomPositionShown, Time.deltaTime * 6);
                    }
                }
            }
            else
            {
                if (bandTop.enabled) bandTop.transform.localPosition = Vector3.Lerp(bandTop.transform.localPosition, bandTopPositionHidden, Time.deltaTime * 6);
                if (bandBottom.enabled) bandBottom.transform.localPosition = Vector3.Lerp(bandBottom.transform.localPosition, bandBottomPositionHidden, Time.deltaTime * 6);

                if (bandTop.enabled && Mathf.Abs(bandTop.transform.localPosition.y - bandTopPositionHidden.y) < 0.1f)
                {
                    bandTop.enabled = bandBottom.enabled = false;
                }
            }

            if (type == Type.FadeOut)
            {
                if (fade.enabled) fade.color = Color.Lerp(fade.color, colorVisible, Time.deltaTime * fadeOutSpeed);
            }
            else
            {
                if (fade.enabled)
                {
                    fade.color = Color.Lerp(fade.color, colorHidden, Time.deltaTime * fadeOutSpeed);
                    if (Mathf.Abs(fade.color.a - colorHidden.a) < 0.01f)
                        fade.enabled = false;
                }
            }


            if (circleState == CircleState.scalingDown)
            {
                float speed = 10f * zoomFactor;
                zoomoutCircle.transform.localScale = Vector3.Lerp(zoomoutCircle.transform.localScale, Vector3.one * minScale, Time.deltaTime * speed);
                float scalex = zoomoutCircle.transform.localScale.x;
                zoomoutLeft.transform.localPosition = zoomoutLeftDef + Vector3.left * scalex * 4;
                zoomoutRight.transform.localPosition = zoomoutRightDef + Vector3.right * scalex * 4;
                zoomoutTop.transform.localPosition = zoomoutTopDef + Vector3.up * scalex * 4;
                zoomoutBottom.transform.localPosition = zoomoutBotDef + Vector3.down * scalex * 4;
            }
            else
            {
                float speed = 1 * zoomFactor / 4;
                zoomoutCircle.transform.localScale = Vector3.Lerp(zoomoutCircle.transform.localScale, Vector3.one * advCamera.cameraSize * zoomScaleFactor, Time.deltaTime * speed);
                float scalex = zoomoutCircle.transform.localScale.x;
                zoomoutLeft.transform.localPosition = zoomoutLeftDef + Vector3.left * scalex * 4;
                zoomoutRight.transform.localPosition = zoomoutRightDef + Vector3.right * scalex * 4;
                zoomoutTop.transform.localPosition = zoomoutTopDef + Vector3.up * scalex * 4;
                zoomoutBottom.transform.localPosition = zoomoutBotDef + Vector3.down * scalex * 4;
            }
        }

        // Update is called once per frame
        void Update()
        {
            Process();
        }
    }
}