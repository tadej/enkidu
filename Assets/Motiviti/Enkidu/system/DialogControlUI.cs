using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class DialogControlUI : MonoBehaviour
    {
        public int selectedLine = -1;
        public static bool hoveringOverLine = false;
        public static DialogControlUI instance;
        public DialogTextLine[] lines;

        bool showing = false;
        CanvasGroup canvasGroup;

        int lineDelta = 0;

        public Color colorText = Color.white, colorShadow = Color.black, colorBackgroundSelected = Color.red, colorBackgroundNeutral = new Color(0, 0, 0, 0);

        // Use this for initialization
        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            DialogControlUI.instance = this;
            foreach (var line in lines) line.SetColor(colorText, colorShadow, colorBackgroundNeutral);
            SetText("", "", "", "", "");
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            Show(false);
        }

        void TestData()
        {
            SetText("First line", "Second line", "Third line blabla", "Fourth line", "Fifth line");
        }

        void Show(bool b)
        {
            showing = b;
            canvasGroup.blocksRaycasts = b;
            canvasGroup.interactable = b;
            Color invisible = new Color(0, 0, 0, 0);
            foreach (var line in lines) line.SetColor(b ? colorText : invisible, b ? colorShadow : invisible, b ? colorBackgroundNeutral : invisible);

        }

        void SetText(string line1, string line2, string line3, string line4, string line5)
        {
            lines[0].SetText(line1);
            lines[1].SetText(line2);
            lines[2].SetText(line3);
            lines[3].SetText(line4);
            lines[4].SetText(line5);
        }

        void SetText(string[] lineText)
        {
            for (int i = 0; i < lines.Length; i++) lines[i].SetText(lineText[i]);
        }

        void InitStringArray(ref string[] strArray)
        {
            for (int i = 0; i < strArray.Length; i++)
            {
                strArray[i] = "";
            }
        }

        public IEnumerator ShowDialogProc(string line1, string line2, string line3, string line4, string line5)
        {
            Show(true);
            selectedLine = -1;

            string[] lns = { line1, line2, line3, line4, line5 };
            string[] lns1 = new string[lines.Length];

            InitStringArray(ref lns1);

            int j = lines.Length - 1;
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(lns[i]))
                {
                    lns1[j--] = lns[i];
                }
            }

            lineDelta = j + 1;

            SetText(lns1);

            while (selectedLine == -1)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Show(false);
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void LineHoverIn(int lineId)
        {
            if (!showing) return;
            DialogControlUI.hoveringOverLine = true;
            //Debug.Log("Line in " + lineId + " " + Time.time);
            if (lines[lineId].CanBeSelected()) lines[lineId].SetColor(colorText, colorShadow, colorBackgroundSelected);
        }

        public void LineHoverOut(int lineId)
        {
            if (!showing) return;
            DialogControlUI.hoveringOverLine = false;
            //Debug.Log("Line out " + lineId + " " + Time.time);
            if (lines[lineId].CanBeSelected()) lines[lineId].SetColor(colorText, colorShadow, colorBackgroundNeutral);
        }

        public void LineSelected(int lineId)
        {
            if (!showing) return;
            //Debug.Log("Line selected " + lineId + " " + Time.time) ;
            if (lines[lineId].CanBeSelected()) selectedLine = lineId - lineDelta;
        }
    }
}