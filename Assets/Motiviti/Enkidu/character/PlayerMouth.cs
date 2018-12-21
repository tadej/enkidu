using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class PlayerMouth : MonoBehaviour {

		public string MoodName = "neutral";

		public SpriteRenderer phono_AI, phono_E, phono_U, phono_O, phono_WQ, phono_etc, phono_rest, phono_MBP, phono_L, phono_FV;

		SpriteRenderer [] listSprites;

		string currentPhoneme = "AI";
		SpriteRenderer currentSprite;

		Color colorVisible=new Color(1,1,1,1), colorHidden=new Color(1,1,1,0);

		bool mouthEnabled = false;
		
		// Use this for initialization
		void Start () {

			currentSprite = phono_MBP;

			listSprites = new SpriteRenderer[10];
			listSprites[0] = phono_AI;
			listSprites[1] = phono_E;
			listSprites[2] = phono_U;
			listSprites[3] = phono_O;
			listSprites[4] = phono_WQ;
			listSprites[5] = phono_etc;
			listSprites[6] = phono_rest;
			listSprites[7] = phono_MBP;
			listSprites[8] = phono_L;
			listSprites[9] = phono_FV;

			StartCoroutine(Initialise());

			//StartCoroutine( TestSequence() );
		}



		IEnumerator TestSequence()
		{
			float pause = 0.2f;

			while(true)
			{
				ShowPhoneme ("AI");

				yield return new WaitForSeconds(pause);

				ShowPhoneme ("E");
				
				yield return new WaitForSeconds(pause);

				ShowPhoneme ("U");
				
				yield return new WaitForSeconds(pause);

				ShowPhoneme ("O");
				
				yield return new WaitForSeconds(pause);

				ShowPhoneme ("WQ");
				
				yield return new WaitForSeconds(pause);

				ShowPhoneme ("etc");
				
				yield return new WaitForSeconds(pause);

				ShowPhoneme ("rest");
				
				yield return new WaitForSeconds(pause);

				ShowPhoneme ("MBP");
				
				yield return new WaitForSeconds(pause);

				ShowPhoneme ("L");
				
				yield return new WaitForSeconds(pause);

				ShowPhoneme ("FV");
				
				yield return new WaitForSeconds(pause);

			}
		}

		public void SetMouthEnabled(bool b)
		{
			if(b)
			{

				//if(mouthEnabled == false && b)
				//TG 17.8.2018 ShowPhoneme("MBP");
			}
			else
			{

				currentSprite = null;
			}
			mouthEnabled = b;
		}

		public void ShowPhoneme(string phoneme, bool showAlways = false)
		{
	//		Debug.Log(Time.time + " Head ShowPhoneme " + phoneme);
			if(phoneme != currentPhoneme || showAlways)
			{
				switch(phoneme.Trim ())
				{
				case "AI":
					currentSprite = phono_AI;
					break;

				case "E":
					currentSprite = phono_E;
					break;

				case "U":
					currentSprite = phono_U;
					break;

				case "O":
					currentSprite = phono_O;
					break;

				case "WQ":
					currentSprite = phono_WQ;
					break;

				case "etc":
				case "CDGKNRSThYZ":
					currentSprite = phono_etc;
					break;

				case "rest":
					currentSprite = phono_rest;
					break;

				case "MBP":
					currentSprite = phono_MBP;
					break;

				case "L":
					currentSprite = phono_L;
					break;

				case "FV":
					currentSprite = phono_FV;
					break;

				default:
					currentSprite = phono_MBP;
					break;
				}

				//currentSprite.color = colorVisible;
				currentPhoneme = phoneme;


			}

			//Debug.Log (Time.time + " " + phoneme + " " + currentSprite.transform.parent.name);
		}
		
		// Update is called once per frame
		void Update () 
		{
			foreach(SpriteRenderer s in listSprites)
			{
				if(!mouthEnabled || s != currentSprite)	
				s.color = colorHidden;//Color.Lerp(s.color, colorHidden, Time.deltaTime * 100);

				//if(!mouthEnabled) s.color = colorHidden;
			}		

			if(currentSprite != null && mouthEnabled) 
			{
				//currentSprite.color = colorVisible;
				currentSprite.color = colorVisible;//Color.Lerp(currentSprite.color, colorVisible, Time.deltaTime * 100);
			}
		}

		IEnumerator Initialise()
		{
			yield return null;

			foreach(SpriteRenderer s in listSprites)
			{
				s.enabled = true;
				s.color = colorHidden;
			}
		
			yield return null;
		}

	}
}