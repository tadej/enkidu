using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using RogoDigital.Lipsync;
using UnityEditor;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class AudioManager : MonoBehaviour {

		string [] phonemes = {"AI", "E", "U", "O", "CDGKNRSThYZ", "FV", "L", "MBP", "WQ", "Rest"};
		public class VoiceClip
		{
			public string name;
			public string text;
			public PhonemePositions phonemes;
			public AudioClip audioClip;
			public CharacterHead.Moods mood;
			public string mode = "idle";
			public AudioSource audioSource;
			public float volume = 1;
			public bool interruptFlag = false;
			public float duration = 0;
		}
		
		public class PhonemePosition
		{
			public float position = 0;
			public string phoneme;
			
			public PhonemePosition(float pos, string pho)
			{
				position = pos;
				phoneme = pho;
			}

			public override string ToString()
			{
				return String.Format("[{0}] {1}", position.ToString(), phoneme);

			}
		}

		public class MoodPosition
		{
			public float position = 0;
			public CharacterHead.Moods mood;
			
			public MoodPosition(float pos, CharacterHead.Moods m)
			{
				position = pos;
				mood = m;
			}
		}

		public class ModePosition
		{
			public float position = 0;
			public string mode;
			
			public ModePosition(float pos, string m)
			{
				position = pos;
				mode = m;
			}
		}

		LipSyncProject lipSyncSettings;
		
		
		public class PhonemePositions
		{
			public LipSyncData lipSyncData;
			ArrayList positions = new ArrayList();

			ArrayList moodPositions = new ArrayList();

			ArrayList modePositions = new ArrayList();
			
			public void Add( PhonemePosition pos ) // must be ordered
			{
				positions.Add ( pos );
			}

			public void AddMood( MoodPosition pos ) // must be ordered
			{
				moodPositions.Add ( pos );
			}

			public void AddMode( ModePosition pos ) // must be ordered
			{
				modePositions.Add ( pos );
			}
			
			public string GetPhoneme( float position )
			{
				float currentPosition = -1;
				string currentPhoneme = "MBP";
				
				for(int i=0; i<positions.Count; i++)
				{
					currentPhoneme = ((PhonemePosition)positions[i]).phoneme;

					if(i < positions.Count-1)
					{
						currentPosition = ((PhonemePosition)positions[i+1]).position;
					}
					else
						break;

					if(currentPosition >= position) break;
				}
				
				int maxI = positions.Count-1;
				
				var cp = ((PhonemePosition)positions[ maxI ]).position;
				
				if(position > cp)
				{
					//Debug.Log (position + " " + cp);
					
					if(position - cp > 0.1f) currentPhoneme = "MBP";
				}

				return currentPhoneme;
			}

			public void GetMood( float position, ref CharacterHead.Moods currentMood, ref bool change  )
			{
				change = false;
				float currentPosition = -1;

				for(int i=0; i<moodPositions.Count; i++)
				{
					currentPosition = ((MoodPosition)moodPositions[i]).position;
					
					if(position >= currentPosition && (i >= moodPositions.Count-1 || position < ((MoodPosition)moodPositions[i+1]).position)) 
					{
						if(currentMood != ((MoodPosition)moodPositions[i]).mood)
						{
							currentMood = ((MoodPosition)moodPositions[i]).mood;
							change = true;
						}
						return;
					}
				}

				currentMood = CharacterHead.Moods.Neutral; // will not get picked up unless change==true
			}

			public void GetMode( float position, ref string currentMode, ref bool change )
			{
				change = false;
				float currentPosition = -1;

				for(int i=0; i<modePositions.Count; i++)
				{
					currentPosition = ((ModePosition)modePositions[i]).position;
					
					if(position >= currentPosition && (i >= modePositions.Count-1 || position < ((ModePosition)modePositions[i+1]).position)) 
					{
						if(currentMode != ((ModePosition)modePositions[i]).mode)
						{
							currentMode = ((ModePosition)modePositions[i]).mode;
							change = true;
						}
						return;
					}
				}

				currentMode = "idle"; // will not get picked up unless change==true
			}
		}

		Hashtable originalVolumes;

		public string[] AudioClipFolders;
		
		protected AudioClip [] audioClips;
		
		protected Hashtable voiceClips = new Hashtable();
		
		AudioSource currentVoicePlaying = null;

		public Transform audioSourceTemplate;
		public Transform audioSourceFolder;

		ArrayList musicClips = new ArrayList();

		public ArrayList audioSourcesToFadeOut = new ArrayList ();

		public GameObject music;

		ArrayList pausedList;
		bool isPaused = false;
		Hashtable volumeList = new Hashtable();
		bool isMutedSound = false;
		bool isMutedMusic = false;
		bool fadingOutMusic = false;
		bool fadingOutSounds = false;

		public AudioSource GetCurrentVoicePlaying()
		{
			return currentVoicePlaying;
		}

		public static IEnumerable<string> StringSplit(string str, int maxChunkSize)
		{
			int i = 0;

			maxChunkSize = Mathf.Min(maxChunkSize, str.Length);
			
			for(int j=maxChunkSize-1; j>0; j--)
			{
				if( str[i+j] == ' ' || str[i+j] == ',' || str[i+j] == '.' || str[i+j] == '!' || str[i+j] == '?') 
				{
					maxChunkSize = j+1;
					break;
				}
			}
			
			for (i = 0; i < str.Length; i += maxChunkSize) 
			{
				maxChunkSize = Mathf.Min(maxChunkSize, str.Length-i);
				
				int newChunkSize = maxChunkSize;
				
				for(int j=maxChunkSize-1; j>0; j--)
				{
					int cc = i+j;
					if(str[cc] == ' ' ||  str[cc] == ',' || str[cc] == '.' || str[cc] == '!' || str[cc] == '?') 
					{
						newChunkSize = j+1;
						break;
					}
				}
				
				maxChunkSize = newChunkSize;
				if(maxChunkSize < 0) break;
				
				yield return str.Substring(i, maxChunkSize);
			}
		
		}
		
		protected void ShowText(string line, CharacterHead head, int textCharactersInLine = 32)
		{
			string[] lines = StringSplit(line, textCharactersInLine).ToArray();
			
			if(lines.Length < 1) 
			{
				//Global.characterDialogText.SetText("");
				return;
			}

			// string line1 = lines[0];
			string line2 = lines.Length > 1 ? lines[1] : "";
			
			if(lines.Length > 2)
			{
				for(int i=2; i<lines.Length; i++)
				{			
					line2 += lines[i];
				}
			}
		}

		public VoiceClip GetVoiceClip(string name)
		{
			VoiceClip clip = voiceClips[ name ] as VoiceClip;

			if(clip == null)
			{
				foreach(DictionaryEntry tmp in voiceClips)
				{
					VoiceClip tmpClp = tmp.Value as VoiceClip;

					if(tmpClp.text == name || (name.Length > 2 && name.Substring(2) == tmpClp.text)) return tmpClp;

					try
					{
						if(tmpClp.text.Replace ('\"', '\'') == name.Replace ('\"', '\'') || (name.Length > 2 && name.Substring(2) == tmpClp.text)) return tmpClp;
					}
					catch{}
				}

			}

			return clip;
		}

		public delegate void ShowPhoneme(string ph);

		public delegate void ChangeMood(CharacterHead.Moods mood);

		public delegate void ChangeMode(string mode);

		public IEnumerator PlayVoice(VoiceClip clip, ShowPhoneme showPhonemeFunction, ChangeMood changeMoodFunction, ChangeMode changeModeFunction, CharacterHead head, int textCharactersInLine = 32, float volume = 1, bool playAudioSource = true)
		{
			string text = clip.text;
			
			float pause = 1f/240f;

	//		Debug.Log ("Talk: " + name);
			
			string[] textLines = new string[1];
			
			if(string.IsNullOrEmpty(text))
			{
				textLines[0]="";
			}
			else
			textLines = StringSplit(text, textCharactersInLine*2).ToArray();

			CharacterHead.Moods currentMood;
			string currentMode;

			if(textLines.Length > 0)
			{

				float textLineCount = textLines.Length;
				
				int currentTextLine = 0;
				
				ShowText( textLines[0], head, textCharactersInLine);
				currentMood = clip.mood;
				currentMode = clip.mode;

				if (playAudioSource)
				{
					
					clip.audioSource.Play();
				}

				currentVoicePlaying = clip.audioSource;

				//clip.audioSource.volume = !(isMutedSound || Global.muteSpeech) ? volume : 0;

				while(clip.audioSource.isPlaying || isPaused)
				{
					float time = clip.audioSource.time ;
					
					float textLine = textLineCount * (time / clip.audioSource.clip.length);
					
					int c = (int)Mathf.Floor(textLine);
					
					if(c != currentTextLine && c < textLines.Length)
					{
						currentTextLine = c;
						ShowText(textLines[currentTextLine], head, textCharactersInLine);
					}

					bool change = false;

					clip.phonemes.GetMode( time /*+ pause*/, ref currentMode, ref change);

					if(change)
					{

						changeModeFunction( currentMode );
					}

					change = false;

					clip.phonemes.GetMood( time /*+ pause*/, ref currentMood, ref change);
					
					if(change)
					{
						changeMoodFunction( currentMood );
					}

					string ph = clip.phonemes.GetPhoneme( time /*+ pause*/ ).Trim ();

					showPhonemeFunction( ph );

					/*
					if(ph == "O" || ph == "U") // HACK: FOR O and U, hold the phoneme longer
					{
						yield return new WaitForSeconds(pause*4);
					}
					else
					*/
					if(clip.interruptFlag)
					{
						Debug.Log("interrupt flag " + clip.interruptFlag);
						clip.audioSource.Stop();

						break;
					}

					yield return new WaitForSeconds(pause);

				}

				showPhonemeFunction("MBP");
				changeMoodFunction( CharacterHead.Moods.Neutral );

				currentVoicePlaying = null;
				//Global.characterDialogText.SetText("");
			}
		
			//Global.player.ChangeMood(CharacterHead.Moods.Neutral);
			//Global.player.ChangeTalkMode("idle");
			yield return null;
		}

		public IEnumerator PlayVoiceFake(VoiceClip clip, ShowPhoneme showPhonemeFunction, ChangeMood changeMoodFunction, ChangeMode changeModeFunction, CharacterHead head, int textCharactersInLine = 32, float volume = 1)
		{
			//		Debug.Log ("PlayVoice " + textCharactersInLine);

			string text = clip.text;

			float pause = 1f / 24f;

			//		Debug.Log ("Talk: " + name);

			string[] textLines = StringSplit(text, textCharactersInLine * 2).ToArray();

			CharacterHead.Moods currentMood;
			string currentMode;

			currentMood = clip.mood;
			if(changeMoodFunction != null) {
				changeMoodFunction(currentMood);
				Debug.Log("PlayVoiceFake Mood Change: " + currentMood);
			}
			float startTime = Time.time;

			if (textLines.Length > 0)
			{
				float textLineCount = textLines.Length;

				int currentTextLine = 0;

				ShowText(textLines[0], head, textCharactersInLine);

				currentMood = clip.mood;
				currentMode = clip.mode;

				clip.audioSource.Play();

				currentVoicePlaying = clip.audioSource;

				//clip.audioSource.volume = !(isMutedSound || Global.muteSpeech) ? volume : 0;

				while ((Time.time - startTime) < clip.duration)
				{
					float time = (Time.time - startTime);// clip.audioSource.time;

					float textLine = textLineCount * (time / clip.duration );

					int c = (int)Mathf.Floor(textLine);

					if (c != currentTextLine && c < textLines.Length)
					{
						currentTextLine = c;
						ShowText(textLines[currentTextLine], head, textCharactersInLine);
					}

					bool change = false;

					clip.phonemes.GetMode(time + pause, ref currentMode, ref change);

					if (change)
					{
						changeModeFunction(currentMode);
					}

					change = false;

					clip.phonemes.GetMood(time + pause, ref currentMood, ref change);

					if (change)
					{
						changeMoodFunction(currentMood);
					}

					string ph = clip.phonemes.GetPhoneme(time + pause).Trim();
					showPhonemeFunction(ph);

					/*
					if(ph == "O" || ph == "U") // HACK: FOR O and U, hold the phoneme longer
					{
						yield return new WaitForSeconds(pause*4);
					}
					else
					*/
					if (clip.interruptFlag)
					{
						Debug.Log("interrupt flag " + clip.interruptFlag);
						clip.audioSource.Stop();

						break;
					}

					yield return new WaitForSeconds(pause);

				}

				showPhonemeFunction("MBP");
				changeMoodFunction( CharacterHead.Moods.Neutral );

				currentVoicePlaying = null;
				//Global.characterDialogText.SetText("");
			}

			//Global.player.ChangeMood(CharacterHead.Moods.Neutral);

			yield return null;
		}

		CharacterHead.Moods GetMoodFromString(string name, out string restOfText)
		{
		//	CharacterHead.Moods mood = CharacterHead.Moods.Neutral;

			if(string.IsNullOrEmpty(name)) 
			{
				restOfText = name;
				return CharacterHead.Moods.Neutral;
			}

			restOfText = name;

			int moodPos = name.IndexOf (">");
			
			int moodId = 0;
			
			if(moodPos != -1)
			{
				string moodStr = name.Substring(0, moodPos);
				moodId = int.Parse(moodStr);
				name = name.Substring(moodPos+1);
				restOfText = name;
			}

			return (CharacterHead.Moods)moodId;
		}

		public VoiceClip  GetDefaultVoiceClip(string name)
		{
			VoiceClip cliptmp = GetVoiceClip("default_Neutral");

			VoiceClip clip = new VoiceClip();
			clip.audioClip = cliptmp.audioClip;
			clip.audioSource = cliptmp.audioSource;
			clip.interruptFlag = cliptmp.interruptFlag;
			clip.mood = cliptmp.mood;
			clip.name = cliptmp.name;
			clip.phonemes = cliptmp.phonemes;
			//OBSOLETE-TOMIclip.text = LocalizationManager.GetTranslation(name);

			clip.audioSource.volume = 0;
			
			clip.mood = GetMoodFromString(clip.text, out clip.text);

			return clip;
		}
		
		public void LoadVoiceovers(string AudioClipFolder)
		{
			audioClips = Resources.LoadAll<AudioClip>("audio/voices-01/" + AudioClipFolder);
			
			foreach(AudioClip audioClip in audioClips)
			{
				string prefix = "audio/voices-01/" + AudioClipFolder + "/";

				TextAsset text = Resources.Load (prefix + audioClip.name + "_txt") as TextAsset;
				LipSyncData lipSyncData = Resources.Load<LipSyncData>(prefix + audioClip.name);

				
				string txt = "";
				
				if(text!=null) txt = text.text;
				TextAsset phonemes = Resources.Load ("audio/voices-01/" + AudioClipFolder + "/" + audioClip.name + "_phonemes") as TextAsset; 
				
				VoiceClip voiceClip = new VoiceClip();
				voiceClip.name = audioClip.name;

				CharacterHead.Moods mood = CharacterHead.Moods.Neutral;

				if(lipSyncData != null)
				{
					voiceClip.phonemes = GetPhonemesFromLipSyncData(lipSyncData, audioClip.length);

					Debug.Log("Lipsync Data " + voiceClip.phonemes);
				}
				else
				{
					voiceClip.phonemes = GetPhonemesFromMohoText(phonemes.text);
				}

				voiceClip.audioClip = audioClip;
				voiceClip.mood = mood;

				GameObject audioSource = (GameObject.Instantiate(audioSourceTemplate) as Transform).gameObject;

				audioSource.name = voiceClip.name;
				audioSource.GetComponent<AudioSource>().clip = voiceClip.audioClip;
				audioSource.transform.parent = audioSourceFolder;

				voiceClip.audioSource = audioSource.GetComponent<AudioSource>();
				voiceClip.audioSource.playOnAwake = false;

				voiceClip.volume = 1;
				
				voiceClips.Add (voiceClip.name, voiceClip);
			}
		}

		PhonemePositions GetPhonemesFromMohoText( string text )
		{
			PhonemePositions pos = new PhonemePositions();
			
			string [] lines = text.Split ('\n');
			
			int lineCount = 0;

			foreach(string line in lines)
			{
				if(lineCount != 0) // first line contains descriptive text
				{
					string[] lineParts = line.Split(' ');
					
					if(string.IsNullOrEmpty(lineParts[0])) break;
					
					float ms = int.Parse(lineParts[0]);
					
					ms /= 24f; // 24 FPS
					
					string phoneme = lineParts[1];
					
					pos.Add (new PhonemePosition(ms, phoneme));

					try
					{
					if(lineParts.Length > 2 && !string.IsNullOrEmpty(lineParts[2])) pos.AddMood( new MoodPosition(ms, (CharacterHead.Moods)int.Parse( lineParts[2].Trim ())) );
					}
					catch{}

					try
					{
					//if(lineParts.Length > 3 && !string.IsNullOrEmpty(lineParts[3])) pos.AddMode( new ModePosition(ms, (ElroyAdv.ArmGestures)int.Parse( lineParts[3].Trim ())) );
					}
					catch{}
				}
				
				lineCount++;
			}
			
			return pos;
		}

		CharacterHead.Moods GetMoodFromString(string moodString)
		{
			CharacterHead.Moods mood = CharacterHead.Moods.Neutral;

			switch(moodString)
			{
				case "Neutral":
				mood = CharacterHead.Moods.Neutral;
				break;

				case "Angry":
				mood = CharacterHead.Moods.Angry;
				break;

				case "Sad":
				mood = CharacterHead.Moods.Sad;
				break;

				case "Happy":
				mood = CharacterHead.Moods.Happy;
				break;

				case "Scared":
				mood = CharacterHead.Moods.Scared;
				break;

				case "Determined":
				mood = CharacterHead.Moods.Determined;
				break;

				
			}

			return mood;
		}

		PhonemePositions GetPhonemesFromLipSyncData( LipSyncData data, float audioClipLength )
		{
			PhonemePositions pos = new PhonemePositions();
			
			foreach(var phonemeMarker in data.phonemeData)
			{
				pos.Add( new PhonemePosition( phonemeMarker.time * audioClipLength, phonemes[ phonemeMarker.phonemeNumber] ));
			}

			foreach(var emotionMarker in data.emotionData)
			{
				pos.AddMood( new MoodPosition( emotionMarker.startTime * audioClipLength, GetMoodFromString(emotionMarker.emotion) ) );
				pos.AddMood( new MoodPosition( emotionMarker.endTime * audioClipLength, CharacterHead.Moods.Neutral ) );
			}

			foreach(var gestureMarker in data.gestureData)
			{
				pos.AddMode( new ModePosition( gestureMarker.time * audioClipLength, gestureMarker.gesture ) );
			}
		
			return pos;
		}


		public void Initialise() // called from Global.Awake
		{
		
			foreach(string audioFolder in AudioClipFolders){
				LoadVoiceovers(audioFolder);
			}

			originalVolumes = new Hashtable();
			
			music = GameObject.Find ("Music");

			pausedList = new ArrayList ();

			EnvironmentMusic environmentMusic = music.GetComponent<EnvironmentMusic>();

			if (environmentMusic != null && environmentMusic.playOnAwake)
			{
				foreach(AudioSource asource in environmentMusic.playAudiosOnAwake){
					asource.Play();
				}
			}
			
			AddOriginalVolumeRecursive(music.transform);
		}

		void AddOriginalVolumeRecursive(Transform tr)
		{
			if(tr && tr.GetComponent<AudioSource>())
			{
				originalVolumes[tr.gameObject.GetInstanceID()] = tr.GetComponent<AudioSource>().volume;

				musicClips.Add (tr);
			}

			foreach(Transform child in tr) AddOriginalVolumeRecursive(child);
		}

		void DuckMusic(bool duck)
		{
			float duckVolume = 0.25f;

			foreach(Transform tr in musicClips)
			{
				if(originalVolumes[tr.gameObject.GetInstanceID()] != null)
				{
					{
						if(duck)
						{
							if(tr.GetComponent<AudioSource>().volume > duckVolume)
							tr.GetComponent<AudioSource>().volume = Decrement(tr.GetComponent<AudioSource>().volume, 2, duckVolume);
						}
						else
						{
							tr.GetComponent<AudioSource>().volume = Increment(tr.GetComponent<AudioSource>().volume, 2, (float)originalVolumes[tr.gameObject.GetInstanceID()]);
						}
					}
				}
			}
		}

		public void PlayMusic()
		{
			if(music && music.GetComponent<AudioSource>()) music.GetComponent<AudioSource>().Play();
		}

		public void FadeoutMusic()
		{
			fadingOutMusic = true;
		}

		public void FadeinMusic()
		{
			fadingOutMusic = false;
		}

		public void FadeoutClip(AudioSource clip)
		{
			if (!audioSourcesToFadeOut.Contains (audioSourcesToFadeOut)) 
			{
				audioSourcesToFadeOut.Add (clip);
			}
		}

		public void Fadeout()
		{
			fadingOutMusic = true;
			fadingOutSounds = true;
		}

		public void PauseResume(bool pause)
		{
			if (!isPaused && pause)	pausedList = new ArrayList ();

			if(music)PauseResumeClips (music.transform, pause);
			if(audioSourceFolder)PauseResumeClips (audioSourceFolder, pause);
		
			isPaused = pause;
		}


		void PauseResumeClips(Transform tr, bool pause)
		{
			if (tr.GetComponent<AudioSource>()) 
			{
				if(pause)
				{
					if(tr.GetComponent<AudioSource>().isPlaying && !isPaused) pausedList.Add (tr.GetComponent<AudioSource>());
						tr.GetComponent<AudioSource>().Pause ();
				}
				else
				{
					if(pausedList.Contains(tr.GetComponent<AudioSource>()))
						tr.GetComponent<AudioSource>().Play ();
				}
			}

			foreach (Transform child in tr) 
			{
				PauseResumeClips (child, pause);
			}
		}

		public void ToggleMusic(bool on)
		{
			ToggleMute (music.transform, !on, isMutedMusic);
			isMutedMusic = !on;
			Debug.Log("toggle music"+on);
		}

		public void ToggleSound(bool on)
		{
			ToggleMute (audioSourceFolder, !on, isMutedSound);
			isMutedSound = !on;
		}

		void ToggleMute(Transform tr, bool mute, bool isMuted)
		{
			if (tr != null && tr.GetComponent<AudioSource>()) 
			{
				if(mute)
				{
					int id = tr.GetComponent<AudioSource>().GetInstanceID();
					if(!isMuted)volumeList[id] = (tr.GetComponent<AudioSource>().volume);
					tr.GetComponent<AudioSource>().volume = 0;
				}
				else
				{
					int id = tr.GetComponent<AudioSource>().GetInstanceID();
					var a = volumeList[ id ];

					if(a != null) 
					{
						tr.GetComponent<AudioSource>().volume = (float)a;
					}
				}
			}
			
			foreach (Transform child in tr) 
			{
				ToggleMute (child, mute, isMuted);
			}
		}

		void FadeoutRecursive(Transform tr)
		{
			if (tr != null && tr.GetComponent<AudioSource>()) 
			{
				tr.GetComponent<AudioSource>().volume = Decrement (tr.GetComponent<AudioSource>().volume);
			}
			
			foreach (Transform child in tr) 
			{
				FadeoutRecursive (child);
			}
		}


		float Decrement(float volume, float factor = 1, float limit = 0)
		{
			volume -= Time.deltaTime * factor;
			if (volume < limit)	volume = limit;
			return volume;
		}

		float Increment(float volume, float factor = 1, float limit = 1)
		{
			volume += Time.deltaTime * factor;
			if (volume > limit)	volume = limit;
			return volume;
		}
		
		// Update is called once per frame
		void Update () {
			if(fadingOutMusic)
			{
				FadeoutRecursive (music.transform);
			}
			else if(!isMutedMusic)
			{
		//		DuckMusic( currentVoicePlaying != null);
			}

			if (fadingOutSounds) 
			{
				FadeoutRecursive (audioSourceFolder);
			}

			ArrayList asToRemove = new ArrayList ();

			foreach (AudioSource clip in audioSourcesToFadeOut)
			{
				clip.GetComponent<AudioSource>().volume = Decrement (clip.GetComponent<AudioSource>().volume);

				if(clip.GetComponent<AudioSource>().volume <= 0) asToRemove.Add (clip);
			}

			foreach (AudioSource clip in asToRemove)
			{
				audioSourcesToFadeOut.Remove (clip);
			}

			//if (!isPaused && Global.inPause == true)
			//{
			//    PauseResume(true);
			//}

		}
	}
}