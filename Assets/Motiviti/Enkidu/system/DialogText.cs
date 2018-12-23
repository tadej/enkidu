using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class DialogText : MonoBehaviour
    {
        public string fontName;
        public float fontScale = 0.63f;

        Vector3 minPos;
        Vector3 maxPos;

        UIText fnt;
        UITextInstance text;

        public bool repositionTextWithCamera = false;
        Bounds textBounds;
        bool isShowing = false;

        public bool isStatic = false;

        public float heightOffset = 0.8f;

        UIToolkit uiToolkit;

        public bool subtitlePosition = true;

        // Use this for initialization
        void Start()
        {
            uiToolkit = GetComponent<UIToolkit>();

            fnt = new UIText(uiToolkit, fontName, fontName + ".png");
            text = fnt.addTextInstance("", 0, 0, fontScale, 3);
            text.alignMode = UITextAlignMode.Center;
            text.verticalAlignMode = UITextVerticalAlignMode.Bottom;
            Hide();
        }

        public void ShowText(string line1, string line2)
        {
            text.xPos = 5000;
            text.yPos = 5000;


            text.text = line1 + "\r\n" + line2;

            if (string.IsNullOrEmpty(line2)) text.text = "\r\n" + line1;

            isShowing = true;

            GetComponent<Renderer>().enabled = false;

            fnt.updateText(text);

            Reposition();

            GetComponent<Renderer>().enabled = true;
        }

        void Reposition()
        {
            if (!isShowing) return;

            Vector3 pos1 = Vector3.zero;

            if (subtitlePosition)
            {
                Vector3 poss = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 20, 0.02f));

                pos1 = Camera.main.WorldToScreenPoint(poss);

            }
            else
            {
                //pos1 = Camera.main.WorldToScreenPoint (target.GetPosition());
            }

            if (isShowing)
            {
                GetComponent<Renderer>().enabled = true;
            }
        }

        public void Hide()
        {
            GetComponent<Renderer>().enabled = false;
            //ShowText("","");
            isShowing = false;
        }

        void LateUpdate()
        {

            if (repositionTextWithCamera) Reposition();

        }
    }
}