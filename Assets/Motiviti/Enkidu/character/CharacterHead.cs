using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class CharacterHead : MonoBehaviour {

		public enum Moods
		{
			Neutral = 0,
			Happy = 1,
			Sad = 2,
			Determined = 3,
			Angry = 4,
			Scared = 5
		}

		public Color textColor=Color.white, textShadowColor=Color.black;

		protected int currentMood = -1;

		protected bool isEnabled = true;
		
		protected int moodCount = 6;
		
		protected const int textCharactersInLine = 55;
	
		public virtual void Initialise()
		{
		}

		public virtual void Start () 
		{
			
			Initialise ();
			if(isEnabled)SetMood(0);
		}

		public IEnumerator Talk(string name, bool playAudioSource = true)
		{
			Debug.Log("Talk " + name);
			yield return null;
		}

		public virtual void StopTalking()
		{
		}

		public virtual void ChangeMood(CharacterHead.Moods mood)
		{
			Debug.Log ("NOT IMPLEMENTED: public virtual void ChangeMood(...)");
		}
		
		public virtual void ChangeMode(string mode)
		{
			Debug.Log ("NOT IMPLEMENTED: public virtual void ChangeMood(...)");
		}

		public virtual void ShowPhoneme(string phoneme)
		{
			Debug.Log ("NOT IMPLEMENTED: public virtual void ShowPhoneme(string phoneme)");
		}

		public virtual void SetEnabled(bool b)
		{
			isEnabled = b;
			if(!isEnabled)SetMood (0);
		}

		public virtual void SetMood(int moodIndex)
		{
			Debug.Log ("NOT IMPLEMENTED: public virtual void SetMood(int moodIndex)");
		}
	}
}