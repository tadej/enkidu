using UnityEngine;
using System.Collections;
using TMPro;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	public class DialogText3D : MonoBehaviour 
	{
		public TextMeshPro textLine, textLineShadow;
		public TextMeshPro textLineShadow1, textLineShadow2;

		public float shadowOffsetLeft = -0.004f;
		public float shadowOffsetDown = 0.0015f;

		public float shadowOffsetLeft1 = -0.004f;
		public float shadowOffsetDown1 = -0.001f;

		public float shadowOffsetLeft2 = -0.007f;
		public float shadowOffsetDown2 = -0.0001f;

		float originalShadowOffsetLeft, originalShadowOffsetLeft1, originalShadowOffsetLeft2, originalShadowOffsetDown, originalShadowOffsetDown1, originalShadowOffsetDown2; 

		string text = "";

		public bool isShowing = false;

		public float offset = 1;

		public Color outlineColor;

		public float fontSize = 1;

		Transform myTransform;

		float previousCameraSize;

		public bool minesweeperView = false;

		public bool runner = false;

		Vector3 screenPos;

		void Start()
		{
			originalShadowOffsetDown = shadowOffsetDown;
			originalShadowOffsetDown1 = shadowOffsetDown1;
			originalShadowOffsetDown2 = shadowOffsetDown2;

			originalShadowOffsetLeft = shadowOffsetLeft;
			originalShadowOffsetLeft1 = shadowOffsetLeft1;
			originalShadowOffsetLeft2 = shadowOffsetLeft2;

			myTransform = transform;

			screenPos = new Vector3(0.5f, 0 + offset, 0.02f);

			if(!runner)
				fontSize = fontSize * 0.7f;

			if(Camera.main && Camera.main.orthographic)
				offset = offset * 0.7f;

			if (Global.takingVideo)	offset += 100000;

			Reposition();
		}

		public void Hide()
		{
			textLine.GetComponent<Renderer>().enabled = false;
			textLineShadow.GetComponent<Renderer>().enabled = false;
			textLineShadow1.GetComponent<Renderer>().enabled = false;
			textLineShadow2.GetComponent<Renderer>().enabled = false;
			isShowing = false;
		}

		public void ShowText(string line1, string line2)
		{
			text = line1 + "\r\n" + line2;
			
			if(string.IsNullOrEmpty(line2)) text = "\r\n" + line1;

			textLine.GetComponent<Renderer>().enabled = false;
			textLineShadow.GetComponent<Renderer>().enabled = false;
			textLineShadow1.GetComponent<Renderer>().enabled = false;
			textLineShadow2.GetComponent<Renderer>().enabled = false;
			
			textLine.text = text;
			textLineShadow.text = text;
			textLineShadow1.text = text;
			textLineShadow2.text = text;

			textLineShadow.outlineColor = outlineColor;
			textLineShadow1.outlineColor = outlineColor;
			textLineShadow2.outlineColor = outlineColor;
			
			Reposition();
			
			textLine.GetComponent<Renderer>().enabled = true;
			textLineShadow.GetComponent<Renderer>().enabled = true;
			textLineShadow1.GetComponent<Renderer>().enabled = true;
			textLineShadow2.GetComponent<Renderer>().enabled = true;

			isShowing = true;

		}

		void Reposition()
		{
			screenPos = new Vector3(0.5f, 0 + offset, 0.02f);

			if (Global.activeCamera == null)
				return;

			Vector3 poss = Global.activeCamera.ViewportToWorldPoint( screenPos );
		
			float x = poss.x;
			float y = poss.y;

			myTransform.position = new Vector3(x,y,0);
			
			if(Camera.main.orthographic){

				if(previousCameraSize == Global.activeCamera.orthographicSize)
					return;

				float cameraSize = Camera.main.orthographicSize;
				screenPos = new Vector3(0.3f, 0 + offset, 0.02f);
			
				poss = Camera.main.ViewportToWorldPoint( screenPos );
			
				x = poss.x;
				y = poss.y;
			
				myTransform.position = new Vector3(x,y,0);

				textLine.fontSize = cameraSize * fontSize;
				textLineShadow.fontSize = cameraSize * fontSize;
				textLineShadow1.fontSize = cameraSize * fontSize;
				textLineShadow2.fontSize = cameraSize * fontSize;
			
				shadowOffsetLeft = cameraSize / 0.93f * originalShadowOffsetLeft;
				shadowOffsetLeft1 = cameraSize / 0.93f * originalShadowOffsetLeft1;
				shadowOffsetLeft2 = cameraSize / 0.93f * originalShadowOffsetLeft2;
				shadowOffsetDown = cameraSize / 0.93f * originalShadowOffsetDown;
				shadowOffsetDown1 = cameraSize / 0.93f * originalShadowOffsetDown1;
				shadowOffsetDown2 = cameraSize / 0.93f * originalShadowOffsetDown2;
			
				textLineShadow.transform.localPosition = new Vector3(shadowOffsetLeft, shadowOffsetDown, 0);
				textLineShadow1.transform.localPosition = new Vector3(shadowOffsetLeft1, shadowOffsetDown1, 0);
				textLineShadow2.transform.localPosition = new Vector3(shadowOffsetLeft2, shadowOffsetDown2, 0);
			
				previousCameraSize = cameraSize;
			}

			else if (runner)
			{
				float cameraSize = Camera.main.fieldOfView / 2.5f;


				if (previousCameraSize != cameraSize)
				{
					

					textLine.fontSize = cameraSize * fontSize;
					textLineShadow.fontSize = cameraSize * fontSize;
					textLineShadow1.fontSize = cameraSize * fontSize;
					textLineShadow2.fontSize = cameraSize * fontSize;

					shadowOffsetLeft = cameraSize / 0.93f * originalShadowOffsetLeft;
					shadowOffsetLeft1 = cameraSize / 0.93f * originalShadowOffsetLeft1;
					shadowOffsetLeft2 = cameraSize / 0.93f * originalShadowOffsetLeft2;
					shadowOffsetDown = cameraSize / 0.93f * originalShadowOffsetDown;
					shadowOffsetDown1 = cameraSize / 0.93f * originalShadowOffsetDown1;
					shadowOffsetDown2 = cameraSize / 0.93f * originalShadowOffsetDown2;

					textLineShadow.transform.localPosition = new Vector3(shadowOffsetLeft, shadowOffsetDown, 0);
					textLineShadow1.transform.localPosition = new Vector3(shadowOffsetLeft1, shadowOffsetDown1, 0);
					textLineShadow2.transform.localPosition = new Vector3(shadowOffsetLeft2, shadowOffsetDown2, 0);

					previousCameraSize = cameraSize;
				}
			}

			else
			{
				float cameraSize = Camera.main.fieldOfView / 12.5f;

				if(previousCameraSize != cameraSize)
				{
					
					textLine.fontSize = cameraSize * fontSize;
					textLineShadow.fontSize = cameraSize * fontSize;
					textLineShadow1.fontSize = cameraSize * fontSize;
					textLineShadow2.fontSize = cameraSize * fontSize;

					shadowOffsetLeft = cameraSize / 0.93f * originalShadowOffsetLeft;
					shadowOffsetLeft1 = cameraSize / 0.93f * originalShadowOffsetLeft1;
					shadowOffsetLeft2 = cameraSize / 0.93f * originalShadowOffsetLeft2;
					shadowOffsetDown = cameraSize / 0.93f * originalShadowOffsetDown;
					shadowOffsetDown1 = cameraSize / 0.93f * originalShadowOffsetDown1;
					shadowOffsetDown2 = cameraSize / 0.93f * originalShadowOffsetDown2;

					textLineShadow.transform.localPosition = new Vector3(shadowOffsetLeft, shadowOffsetDown, 0);
					textLineShadow1.transform.localPosition = new Vector3(shadowOffsetLeft1, shadowOffsetDown1, 0);
					textLineShadow2.transform.localPosition = new Vector3(shadowOffsetLeft2, shadowOffsetDown2, 0);

					previousCameraSize = cameraSize;
				}
			}
		}
		void LateUpdate()
		{
			if(isShowing)
				Reposition();
		}

	}
}