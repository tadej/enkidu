using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class DialogTextLine : MonoBehaviour {

		public TextMeshProUGUI[] textAndShadows;
		bool canBeSelected = false;
		UnityEngine.UI.Image bgImage;

		Color invisible = new Color(0,0,0,0);

		public bool CanBeSelected()
		{
			return canBeSelected;
		}

		public void SetColor(Color c, Color shadowColor, Color? bgColor)
		{
			if(textAndShadows.Length > 0) textAndShadows[0].color = c;
			for(int i=1; i<textAndShadows.Length; i++) textAndShadows[i].color = shadowColor;

			if(bgColor != null)
			{
				bgImage.color = (Color)bgColor;
			}
		}

		// Use this for initialization
		void Awake () {
			bgImage = GetComponent<UnityEngine.UI.Image>();
		}

		public void SetText(string text)
		{
			foreach(var t in textAndShadows) t.text = text;

			if(string.IsNullOrEmpty(text))
			{
				canBeSelected = false;
				SetColor(invisible, invisible, invisible);
			}
			else
			{
				canBeSelected = true;
			}
		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}