using UnityEngine;
using System.Collections;
using TMPro;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	
    public class DialogControl : MonoBehaviour 
    {
        public TextMeshPro [] textLines;

        public TextMeshPro [] textLinesShadows;

        public TextMeshPro [] textLinesShadows1;

        public TextMeshPro [] textLinesShadows2;

        string [] textStrings;

        public SpriteRenderer background, backgroundLine;

        Transform bg;

        Color colorSelected, colorIdle;
        Color colorShadowSelected, colorShadowIdle;

        public int lineSelected = -1;
        public string lineSelectedText = "";

        public float shadowOffsetLeft, shadowOffsetDown;

        public float shadowOffsetLeft1, shadowOffsetDown1;

        public float shadowOffsetLeft2, shadowOffsetDown2;

        public float textPosPlus, textPosKrat;

        float defTextPosKrat, defTextPosPlus;

        AdvCamera advCamera;

        int numberOfOptions;

        public float offsetBackground;

        Vector3 defBgPos;

        float defScreenHeight = 630;

        public bool calcFunctions = false;

        float defCamSizeForCalc = 5.5f;

        public float multiplierSmall = 0.25f;

        public float multiplierLarge = 0.25f;

        public float secondMultiplier = 0.15f;

        float addedOffest = 0.8f;

        public int test = 0;

        public bool isEnabled = false;

        // Use this for initialization
        void Start () 
        {
            advCamera = Global.advCamera;
            float cameraSize = 4;
            if (advCamera)
                cameraSize = advCamera.cameraSize;
            else
                cameraSize = Global.activeCamera.fieldOfView;

            textStrings = new string[5];
            colorSelected = new Color32(255,168,48,255);
            colorIdle = new Color32(255,243,197,255);
            colorShadowSelected = new Color32(32,53,63,255);
            colorShadowIdle = new Color32(13,63,81,255);
            bg = background.transform.parent.transform;
            defBgPos = bg.transform.localPosition;
            if (calcFunctions)
            {
                if(Global.IsSmallScreen())
                    defBgPos.y = (-cameraSize  + defCamSizeForCalc) * multiplierSmall;
                else
                    defBgPos.y = (-cameraSize + defCamSizeForCalc) * multiplierLarge;
                offsetBackground = (cameraSize - defCamSizeForCalc) * secondMultiplier + addedOffest;
            }

            defTextPosKrat = textPosKrat;
            defTextPosPlus = textPosPlus;
            SetEnabled(false);

            shadowOffsetLeft *= cameraSize / 0.93f;
            shadowOffsetLeft1 *= cameraSize / 0.93f;
            shadowOffsetLeft2 *= cameraSize / 0.93f;
            shadowOffsetDown *= cameraSize / 0.93f;
            shadowOffsetDown1 *= cameraSize / 0.93f;
            shadowOffsetDown2 *= cameraSize / 0.93f;
        }

        public IEnumerator ShowDialog(string line1, string line2, string line3, string line4, string line5)
        {
            yield return null;

            if (calcFunctions)
            {
                if (Global.IsSmallScreen())
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierSmall;
                else
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierLarge;
                offsetBackground = (advCamera.cameraSize - defCamSizeForCalc) * secondMultiplier + addedOffest;
            }

            SetEnabled(true);

            SetLines (line1, line2, line3, line4, line5);
            lineSelected = -1;

            while(lineSelected == -1)
            {
                yield return new WaitForSeconds(0.5f);
            }

            SetEnabled (false);
        }

        void tryLines()
        {
            if (calcFunctions)
            {
                if (Global.IsSmallScreen())
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierSmall;
                else
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierLarge;
                offsetBackground = (advCamera.cameraSize - defCamSizeForCalc) * secondMultiplier + addedOffest;
            }
            StartCoroutine(ShowDialog("test1", "test2", "test3", "test4", ""));
        }

        void tryLines5()
        {
            if (calcFunctions)
            {
                if (Global.IsSmallScreen())
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierSmall;
                else
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierLarge;
                offsetBackground = (advCamera.cameraSize - defCamSizeForCalc) * secondMultiplier + addedOffest;
            }
            StartCoroutine(ShowDialog("test1", "test2", "test3", "test4", "test5"));
        }

        void tryLines3()
        {
            if (calcFunctions)
            {
                if (Global.IsSmallScreen())
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierSmall;
                else
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierLarge;
                offsetBackground = (advCamera.cameraSize - defCamSizeForCalc) * secondMultiplier + addedOffest;
            }
            StartCoroutine(ShowDialog("test1", "test2", "test3", "", ""));
        }

        void tryLines2()
        {
            if (calcFunctions)
            {
                if (Global.IsSmallScreen())
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierSmall;
                else
                    defBgPos.y = (-advCamera.cameraSize + defCamSizeForCalc) * multiplierLarge;
                offsetBackground = (advCamera.cameraSize - defCamSizeForCalc) * secondMultiplier + addedOffest;
            }
            StartCoroutine(ShowDialog("test1", "test2", "", "", ""));
        }

        void SetLines(string line1, string line2, string line3, string line4, string line5)
        {
            textStrings[0] = line1;
            textStrings[1] = line2;
            textStrings[2] = line3;
            textStrings[3] = line4;
            textStrings[4] = line5;
        
            textLines[0].text = (line1);
            textLines[1].text = (line2);
            textLines[2].text = (line3);
            textLines[3].text = (line4);
            textLines[4].text = (line5);

            textLinesShadows[0].text = line1;
            textLinesShadows[1].text = line2;
            textLinesShadows[2].text = line3;
            textLinesShadows[3].text = line4;
            textLinesShadows[4].text = line5;

            textLinesShadows1[0].text = line1;
            textLinesShadows1[1].text = line2;
            textLinesShadows1[2].text = line3;
            textLinesShadows1[3].text = line4;
            textLinesShadows1[4].text = line5;

            textLinesShadows2[0].text = line1;
            textLinesShadows2[1].text = line2;
            textLinesShadows2[2].text = line3;
            textLinesShadows2[3].text = line4;
            textLinesShadows2[4].text = line4;

            for(int i = 0; i<5; i++){
                textLines[i].color = colorIdle;
                if(advCamera)textLines[i].fontSize = advCamera.cameraSize * 0.9f;
                if(advCamera)textLinesShadows[i].fontSize = advCamera.cameraSize * 0.9f;
                textLinesShadows[i].color = colorShadowIdle;
                textLinesShadows[i].outlineColor = colorShadowIdle;
                if (advCamera) textLinesShadows1[i].fontSize = advCamera.cameraSize * 0.9f;
                textLinesShadows1[i].color = colorShadowIdle;
                textLinesShadows1[i].outlineColor = colorShadowIdle;
                if (advCamera) textLinesShadows2[i].fontSize = advCamera.cameraSize * 0.9f;
                textLinesShadows2[i].color = colorShadowIdle;
                textLinesShadows2[i].outlineColor = colorShadowIdle;
            }

            bg.transform.localPosition = defBgPos;

            numberOfOptions = 5; 
            if (string.IsNullOrEmpty(line5))
            {
                textLines[4].GetComponent<Renderer>().enabled = false;
                textLinesShadows[4].GetComponent<Renderer>().enabled = false;
                textLinesShadows1[4].GetComponent<Renderer>().enabled = false;
                textLinesShadows2[4].GetComponent<Renderer>().enabled = false;
                numberOfOptions = 4;
                bg.transform.localPosition += Vector3.down * offsetBackground;
            }
            if(string.IsNullOrEmpty(line4)){
                textLines[3].GetComponent<Renderer>().enabled = false;
                textLinesShadows[3].GetComponent<Renderer>().enabled = false;
                textLinesShadows1[3].GetComponent<Renderer>().enabled = false;
                textLinesShadows2[3].GetComponent<Renderer>().enabled = false;
                numberOfOptions = 3;
                bg.transform.localPosition += Vector3.down * offsetBackground;
            }
            if(string.IsNullOrEmpty(line3)){
                textLines[2].GetComponent<Renderer>().enabled = false;
                textLinesShadows[2].GetComponent<Renderer>().enabled = false;
                textLinesShadows1[2].GetComponent<Renderer>().enabled = false;
                textLinesShadows2[2].GetComponent<Renderer>().enabled = false;
                numberOfOptions = 2;
                bg.transform.localPosition += Vector3.down * offsetBackground;
            }
        }

        void SetEnabled(bool b)
        {
            isEnabled = b;

            background.enabled = b;

            backgroundLine.enabled = b;

            foreach(var g in textLines) g.GetComponent<Renderer>().enabled = b;

            foreach(var g in textLinesShadows) g.GetComponent<Renderer>().enabled = b;

            foreach(var g in textLinesShadows1) g.GetComponent<Renderer>().enabled = b;

            foreach(var g in textLinesShadows2) g.GetComponent<Renderer>().enabled = b;
        }

        void Position3DLines(){
            for(int i = 0; i < numberOfOptions; i++){
                Vector3 poss = Global.activeCamera.ScreenToWorldPoint( new Vector3(20, textPosPlus + textPosKrat * (numberOfOptions - 1 - i), 0.02f) );
                Vector3 defPos = poss;
                textLines[i].transform.position = poss;

                poss = defPos + new Vector3(shadowOffsetLeft, shadowOffsetDown, 0); 

                textLinesShadows[i].transform.position = poss;

                poss = defPos + new Vector3(shadowOffsetLeft1, shadowOffsetDown1, 0); 

                textLinesShadows1[i].transform.position = poss;

                poss = defPos + new Vector3(shadowOffsetLeft2, shadowOffsetDown2, 0); 

                textLinesShadows2[i].transform.position = poss;
            }
        }

        void SelectLine(int i)
        {
            if(!string.IsNullOrEmpty( textLines[i].text ))
            {
                lineSelectedText = textLines[i].text ;
                lineSelected = i;
            }
        }
        
        // Update is called once per frame
        void Update () 
        {
            if (isEnabled)
            {
                float screenHeight = Screen.height;

                float k = screenHeight / defScreenHeight;

                textPosKrat = defTextPosKrat * k;

                textPosPlus = defTextPosPlus * k;
                Position3DLines();
                if (!Global.inPause)
                {
                    bool isHit = false;
                    bool holding = false;
                    Vector3 holdPosition = Vector3.zero;
                    Vector3 worldPosition = Vector3.zero;

                    ProcessInput(ref isHit, ref holding, ref holdPosition, ref worldPosition);

                    Vector3 screenPos = Global.activeCamera.WorldToScreenPoint(worldPosition);
                    screenPos.z = 0;
                    //	Debug.Log("position: " + screenPos);
                    if (holding)
                    {
                        for (int i = 0; i < numberOfOptions; i++)
                        {
                            TextMeshPro textLine = textLines[i];
                            TextMeshPro textLineShadow = textLinesShadows[i];

                            int x = (int)((screenPos.y - 20) / textPosKrat);

                            if (x == (numberOfOptions - 1 - i))
                            {
                                textLine.color = colorSelected;
                                textLineShadow.color = colorShadowSelected;
                                textLineShadow.outlineColor = colorShadowSelected;
                                if (isHit)
                                {
                                    SelectLine(i);
                                }
                            }
                            else
                            {
                                textLine.color = colorIdle;
                                textLineShadow.color = colorShadowIdle;
                                textLineShadow.outlineColor = colorShadowIdle;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < textLines.Length; i++)
                        {
                            //					TextMesh textLine = textLines[i];
                            //					textLine.color = colorIdle;
                        }
                    }
                }
                if (test == 5)
                {
                    tryLines5();
                    test = 0;
                }
                if (test == 4)
                {
                    tryLines();
                    test = 0;
                }
                if (test == 3)
                {
                    tryLines3();
                    test = 0;
                }
                if (test == 2)
                {
                    tryLines2();
                    test = 0;
                }
            }
        }

        void ProcessInput (ref bool isHit, ref bool holding, ref Vector3 holdPosition, ref Vector3 worldPosition)
        {
            foreach (Touch touch in Input.touches) 
            {			
                holding = true;
                if(touch.phase == TouchPhase.Began)
                {
                    holdPosition = touch.position;
                }
                
                if(touch.phase == TouchPhase.Ended)
                {
                    holding = true;
                    holdPosition = touch.position;
                    isHit = true;
                }
            }

            holdPosition = Input.mousePosition;
            if(Input.GetButtonUp ("Fire1"))
            {
                holding = true;
                holdPosition = Input.mousePosition;
                isHit = true;
            }
            
            if(Input.GetButton ("Fire1"))
            {
                holding = true;
                holdPosition = Input.mousePosition;
            }
            
            worldPosition = Global.activeCamera.ScreenToWorldPoint(holdPosition);
        
        }
    }
}