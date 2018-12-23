using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class Global : MonoBehaviour
	{
		public static bool takingVideo = false;

		public static bool muteSpeech = false;

		public static float maxCharacterAnimationLength = 6.5f;
		public static float volume = 1;

		//public bool inventoryOn = true;

		public static Player narrator;
		public static Hashtable state = new Hashtable();

		static string savePath = ""; 

		static int currentSlot = 1;

		static string defaultSavePath = "";

		static bool initialized = false;

		static ArrayList statefulItemList = new ArrayList();

		static bool soundOn = true;

		static bool musicOn = true;

		static bool demoOn = false;

		public static bool subtitlesOn = true;

		SpriteRenderer blackFade;

		public static AudioManager audioManager;

		bool fading=false;

		public static bool inPause = false;

		public static bool isSmallScreen = false;

		public bool isSmallScreenL = false;

		public static Player player;

		public static AdvCamera advCamera;

		public static UnityEngine.AI.NavMeshAgent navMeshAgent;

		public static Inventory inventory;

		public static Scene scene;

		public static Camera activeCamera;

		public static CustomCursor customCursor;

		public static CharacterDialogText characterDialogText;

		public bool devStatus_show = false;
		public string devStatus_comments = "/";

		public string arrivalDoor = "";

	//    public static ItemCombStorage itemsData;


		void Start()
		{
			// TODO: remove
			devStatus_show = false;

			if (audioManager == null && GameObject.Find ("AudioManager"))
				audioManager = GameObject.Find ("AudioManager").GetComponent<AudioManager> ();

			try{
			if(narrator == null && GameObject.Find("Narrator"))
			narrator = GameObject.Find("Narrator").GetComponent<Player>();
			}catch{
				
			}
				
		}

		void OnGUI()
		{
			if (devStatus_show && !takingVideo) {
				string str = string.Format ("DEV STATUS: {0}", devStatus_comments);

				GUIStyle style = new GUIStyle(GUI.skin.label);
				style.fontStyle = FontStyle.Bold;
				style.fontSize = 17;


				int TextWidth = 1000;
				GUI.Label (new Rect (10, Screen.height - 30, TextWidth, 40), str, style);
			}
		}

		public static void SetSound(bool enableSound)
		{
			soundOn = enableSound;
			SetState("Global sound", soundOn ? 1:0, true);

			audioManager.ToggleSound (enableSound);
		}

		public static void SetDemo(bool demo)
		{
			demoOn = demo;
			SetState("Global demo", soundOn ? 1 : 0, true);
		}

		public static void SetMusic(bool enableMusic)
		{
			musicOn = enableMusic;
		//	Debug.Log("seting music" + enableMusic);

			SetState("Global music", musicOn ? 1:0, true);

			audioManager.ToggleMusic (enableMusic);
		}

		public static void SetSubtitles(bool enableSubtitles)
		{
			subtitlesOn = enableSubtitles;

			SetState("Global subtitles", subtitlesOn ? 1 : 0, false);
		}

		public static void SetSavegame(int saveGameSlot = 1){
		//	Debug.Log("seting Savegame" + saveGameSlot);
			
			SetState("Global Savegame", saveGameSlot, true);
		}

		public static void SetArrivalDoor(string arrivalDoor)
		{
			SetStateStr ("Global ArrivalDoor", arrivalDoor, true);
			SaveState();
		}

		public static string GetArrivalDoor()
		{
			return GetStateStr("Global ArrivalDoor");
		}

		public static void setPause(bool pause){
			Global.inPause = pause;
		//	Global g = GameObject.Find("Global").GetComponent<Global>();
			if (audioManager) 
				audioManager.PauseResume (pause);
			if(pause){
				SetState("Global pause", 1); 
				Vector3 newPos=Global.activeCamera.transform.position;
				newPos.z+=5;
				if(Global.inventory)
				{
					if(Global.inventory.heldItem != null){
						Global.inventory.ReturnItem(Global.inventory.heldItem);
						Global.inventory.HoldItem(null);
						Global.inventory.heldItem = null;
					}
					Global.inventory.FadeOut();
				}
			//	g.blackFade.transform.position=newPos;
			//	g.blackFade.color = new Color(1,1,1,0.5f); //disablano 7.5.2014 po tasku
			}
			else{
				SetState("Global pause", 0);
			//	g.blackFade.color = new Color(1,1,1,0);
				if(Global.inventory)
				{
					Global.inventory.FadeIn();
				}
			}
		}

		public static bool GetSound()
		{
			return soundOn;
		}

		public static bool GetMusic()
		{
			return musicOn;
		}

		public static bool GetDemo()
		{
			return demoOn;
		}

		public static void FadeOut()
		{
			audioManager.Fadeout ();
		}

		IEnumerator RestartGameProcedure()
		{
			audioManager.Fadeout ();

			Time.timeScale = 1;
			fading=true;
			Vector3 newPos=Camera.main.transform.position;
			newPos.z+=5;
			blackFade.transform.position=newPos;

			Global.setPause(false);

			yield return new WaitForSeconds(1.5f);
			if (Global.GetDemo())
			{
				Global.SetState("Global level", 1);
				Global.SetState("Global loadingLevel", 1, true);
			}
			else
			{
				Global.SetState("Global level", 1);
				Global.SetState("Global loadingLevel", 1, true);
			}
			SceneManager.LoadScene("loadingScreen");
		}

		void startSelectedLevelFunction(){
			StartCoroutine(startSelectedLevel());
		}

		IEnumerator startSelectedLevel(){
			audioManager.Fadeout ();
			
			Time.timeScale = 1;
			fading=true;
			Vector3 newPos=Camera.main.transform.position;
			newPos.z+=5;
			blackFade.transform.position=newPos;
			Global.setPause(false);
			yield return new WaitForSeconds(1.5f);
			
			//Application.LoadLevel("loadingScreen");
			SceneManager.LoadScene("loadingScreen");
		}
		
		public static void StartLevel(int levelNumber){
			audioManager.Fadeout ();
			Global g=GameObject.Find("Global").GetComponent<Global>();
			RemoveAllSavedData();
			Global.SetState("Global level", levelNumber);
			Global.SetState("Global loadingLevel", levelNumber, true);
			Global.setPause(false);
			g.startSelectedLevelFunction();
		}

		public static void StartCaveColorLevel(int levelNumber){
			audioManager.Fadeout ();
			Global g=GameObject.Find("Global").GetComponent<Global>();
			RemoveAllSavedData();
			Global.SetState("Global level", levelNumber);
			Global.SetState("Global loadingLevel", levelNumber, true);
			Global.SetState("18-hookmt-caves_Scene[Scene] arrivalCave", 12, true);
			Global.setPause(false);
			g.startSelectedLevelFunction();
		}

		public static void StartCaveVentilatorLevel(int levelNumber){
			audioManager.Fadeout ();
			Global g=GameObject.Find("Global").GetComponent<Global>();
			RemoveAllSavedData();
			Global.SetState("Global level", levelNumber);
			Global.SetState("Global loadingLevel", levelNumber, true);
			Global.SetState("18-hookmt-caves_Scene[Scene] arrivalCave", 15, true);
			Global.SetState("18-hookmt-caves_BucketFinal[InventoryItem] state", 2, true);
			Global.setPause(false);
			g.startSelectedLevelFunction();
		}

		public static bool IsSmallScreen()
		{

	#if UNITY_IPHONE
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				if (iPhone.generation == iPhoneGeneration.iPodTouch1Gen
					|| iPhone.generation == iPhoneGeneration.iPodTouch2Gen
					|| iPhone.generation == iPhoneGeneration.iPodTouch3Gen
					|| iPhone.generation == iPhoneGeneration.iPodTouch4Gen
					|| iPhone.generation == iPhoneGeneration.iPodTouch5Gen
					|| iPhone.generation == iPhoneGeneration.iPodTouchUnknown
					|| iPhone.generation == iPhoneGeneration.iPhone
					|| iPhone.generation == iPhoneGeneration.iPhone3G
					|| iPhone.generation == iPhoneGeneration.iPhone3GS
					|| iPhone.generation == iPhoneGeneration.iPhone4
					|| iPhone.generation == iPhoneGeneration.iPhone4S
					|| iPhone.generation == iPhoneGeneration.iPhone5
					|| iPhone.generation == iPhoneGeneration.iPhone5C
					|| iPhone.generation == iPhoneGeneration.iPhone5S
					|| iPhone.generation == iPhoneGeneration.iPhoneUnknown
					)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
	#endif
			if (Global.isSmallScreen)
				return true;
			else
				return false;
		}

		public void RestartGame()
		{
			StartCoroutine(RestartGameProcedure());
		}

		IEnumerator ContinueGameProcedure()
		{
			Time.timeScale = 1;
			fading=true;
			Vector3 newPos=Camera.main.transform.position;
			newPos.z+=5;
			blackFade.transform.position=newPos;
			state.Clear();

			Initialize();

			yield return new WaitForSeconds(1.5f);

		//   Global.SaveState();

			int? lvl = GetState("Global level");

			if(lvl<1)
				lvl=1;

			if (lvl == 24)  //wormhole psychadelic
				lvl--;

			Global.SetState("Global level", (int)lvl);
			Global.SetState("Global loadingLevel", (int)lvl, true);

			Global.setPause(false);

			Time.timeScale = 1;
			
			//Application.LoadLevel("loadingScreen");
			SceneManager.LoadScene("loadingScreen");

		}
		
		public void ContinueGame()
		{
			StartCoroutine(ContinueGameProcedure());
		}

		public static void StartNewGame(int slot)
		{
			audioManager.Fadeout ();
			Global g=GameObject.Find("Global").GetComponent<Global>();

			savePath = defaultSavePath.Remove(defaultSavePath.Length - 4);

			savePath = savePath + slot + ".txt";

			currentSlot = slot;

			Global.SetState("Global Savegame", slot, true);

			RemoveAllSavedData();
			g.RestartGame();
		}

		public static void ContinueFromSavedGame(int slot)
		{
			audioManager.Fadeout ();

			Global.SetState("Global Savegame", slot, true);

			Global g=GameObject.Find("Global").GetComponent<Global>();
			g.ContinueGame();
		}

		public static void SetState(string index, int? s, bool fileSave = false)
		{
			state[index] = s;

			if(fileSave) SaveState ();
		}

		public static void SetStateStr(string index, string s, bool fileSave = false)
		{
			state[index] = s;

			if(fileSave) SaveState ();
		}
		
		public static void AddStatefulItem(StatefulItem item)
		{
			statefulItemList.Add(item);
		}

		public Player[] characters;

		void Awake ()
		{
			float starttime = Time.realtimeSinceStartup;
			Debug.Log ("Global Awake: Init: " + starttime);

			if(SceneManager.GetActiveScene().buildIndex == 0){
				initialized = false;
				state = new Hashtable();
				Initialize();
			}
			isSmallScreen = isSmallScreenL;

			//elroy = GameObject.Find ("Elroy") ? GameObject.Find ("Elroy").GetComponent<ElroyAdv>() : null;

			//Debug.Log(elroy);

			var men = Resources.Load("prefabs/Menus") as GameObject;

			var meni = Instantiate (men);
			meni.name = "Menus";
			
			var dt = Resources.Load("prefabs/CharacterDialogText") as GameObject;
			var dti = Instantiate (dt);
			dti.name = "CharacterDialogText";

			var dc = Resources.Load("prefabs/DialogControl") as GameObject;
			var dci = Instantiate (dc);
			dci.name = "DialogControl";

			Global.characterDialogText = dti.GetComponent<CharacterDialogText>();

			player = GameObject.FindObjectOfType<Player>() ? GameObject.FindObjectOfType<Player>().GetComponent<Player>() : null;
			
			advCamera = GameObject.Find ("MainCamera") ? GameObject.Find ("MainCamera").GetComponent<AdvCamera>() : null;

			if (advCamera == null) advCamera = GameObject.Find("Main Camera Runner") ? GameObject.Find("Main Camera Runner").GetComponent<AdvCamera>() : null;

			navMeshAgent = GameObject.Find ("NavmeshAgent") ? GameObject.Find ("NavmeshAgent").GetComponent<UnityEngine.AI.NavMeshAgent>() : null;

			//inventory = GameObject.Find ("UI-Inventory") ? GameObject.Find ("UI-Inventory").GetComponent<Inventory>() : null;

			scene = GameObject.Find("Scene") ? GameObject.Find("Scene").GetComponent<Scene>() : null;

			customCursor = GameObject.Find("CustomCursor") ? GameObject.Find("CustomCursor").GetComponent<CustomCursor>() : null;
			customCursor.gameObject.SetActive(true);
			if (advCamera && Global.activeCamera == null)
			{
				Global.activeCamera = advCamera.GetComponent<Camera>();
			}

			if (Global.activeCamera == null && Camera.main)
			{
				Global.activeCamera = Camera.main;
			}

			//Debug.LogError("prvi" + SceneManager.GetActiveScene().buildIndex);
			//Debug.LogError("drugi" + (SceneManager.sceneCountInBuildSettings - 1));

			bool inBuildSettings = true;
			String current = SceneManager.GetActiveScene().path;

			var bf = Resources.Load("prefabs/BlackFade") as GameObject;

			var bfi = Instantiate (bf);
			bfi.name = "BlackFade";
			GameObject Fade = GameObject.Find("BlackFade");
			blackFade = Fade.GetComponent<SpriteRenderer>();
			/*foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
			{
				if (s.path == current)
					inBuildSettings = true;
			}*/

			
			if (!inBuildSettings || SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1 ){
				audioManager = GameObject.Find ("AudioManager").GetComponent<AudioManager> ();

				audioManager.Initialise ();

				SetMusic(GetState("Global music") == 1 || GetState("Global music") == -1);
				SetSound(GetState("Global sound") == 1 || GetState("Global sound") == -1);
				int? savegameSlot = GetState("Global Savegame");
				if (savegameSlot == null)
					savegameSlot = 1;
				SetSavegame((int)savegameSlot);		

			
				if(SceneManager.GetActiveScene().buildIndex > 0 && GetState("Global level") != SceneManager.GetActiveScene().buildIndex)
					SetState("Global level", SceneManager.GetActiveScene().buildIndex, true);

				float endTime = Time.realtimeSinceStartup;

				float timePassed = endTime - starttime;

				Debug.Log ("Global Awake: Init End: " + endTime + " time passed: " + timePassed);
			}

			blackFade.color = new Color(1, 1, 1, 1);

			if(string.IsNullOrEmpty(GetArrivalDoor()) && !string.IsNullOrEmpty( arrivalDoor) ) SetArrivalDoor(arrivalDoor);
		}

		int framesScene = 0;

		void Update(){
			if(fading && blackFade){
				blackFade.color = Color.Lerp (blackFade.color, new Color(1,1,1,1), Time.deltaTime*4);
			}
			else if(blackFade && (blackFade.color.a != 0) && framesScene > 1)
			{
				blackFade.color = Color.Lerp(blackFade.color, new Color(1, 1, 1, 0), Time.deltaTime * 4);
			}
			framesScene++;
			framesScene %= 100;
			// GARBAGE COLLECTION

			if (Time.frameCount % (60 * 30) == 0)
			{
				System.GC.Collect();
			}

			
			if(Input.GetKeyDown(KeyCode.M))
			{
				SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex + 1);


			}

			if(Input.GetKeyDown(KeyCode.N))
			{
				SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex -1);


			}
	/* 
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				SwitchCharacter(1);
			}

			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				SwitchCharacter(2);

			}

			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				SwitchCharacter(3);

			}*/
		}

		void SwitchCharacter(int i)
		{
			for (int j = 0; j < characters.Length; j++)
			{
				var c = characters[j];
				if (j != i) c.gameObject.SetActive(false);
				else
				{
					c.gameObject.SetActive(true);
					player = c;
				}
			}
		}

		IEnumerator OnApplicationPause(bool paused)
		{
			if(paused && SceneManager.GetActiveScene().buildIndex != 0){
				Global.SaveState();
			}
			else{
			}
			yield return null;
		}

		public static bool IsNumericType(FieldInfo o)
		{
			switch (Type.GetTypeCode(o.FieldType))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}

		public static bool IsBooleanType(FieldInfo o)
		{
			switch (Type.GetTypeCode(o.FieldType))
			{
				case TypeCode.Boolean:
				
					return true;
				default:
					return false;
			}
		}

		public static bool IsStringType(FieldInfo o)
		{
			switch (Type.GetTypeCode(o.FieldType))
			{
				case TypeCode.String:

					return true;
				default:
					return false;
			}
		}

		public static int? GetState(string index)
		{
			if (!initialized) Initialize();

			//if(state[index] == null) state[index] = null;

			return (int?)state[index];
		}

		public static string GetStateStr(string index)
		{
			if (!initialized) Initialize();

			string s = null;
			//if(state[index] == null) state[index] = null;
			try{
			s =  (string)state[index];
			}catch{

			}
			return s;
		}

		// Use this for initialization
		static void Initialize () {

	//		Debug.Log("Global initializing");

			bool findSaveGameSlot = false;

			switch(Application.platform)
			{
			case RuntimePlatform.IPhonePlayer:
				{
					savePath = GetiPhoneDocumentsPath() + "/saveGame";
					defaultSavePath = GetiPhoneDocumentsPath() + "/saveGame.txt";	
					break;
				}
			case RuntimePlatform.Android:
			{
				savePath = Application.persistentDataPath + "/saveGame";
				defaultSavePath = Application.persistentDataPath + "/saveGame.txt";
				break;
			}

			case RuntimePlatform.OSXPlayer:
			{
				savePath = Application.persistentDataPath + "/saveGame";
				defaultSavePath = Application.persistentDataPath + "/saveGame.txt";
			}
			break;
			case RuntimePlatform.WindowsPlayer:
			{
				savePath = Application.persistentDataPath + "/saveGame";
				defaultSavePath = Application.persistentDataPath + "/saveGame.txt";
			}
			break;

			default:
				savePath = "saveGame";
				defaultSavePath = "saveGame.txt";
				break;
			}

			try
			{
				using (StreamReader reader = new StreamReader(defaultSavePath))
				{
					while (true)
					{
						string line = reader.ReadLine();
						if (line == null) break;

						try
						{
							string[] splitLine = line.Split(':');
													
							try
							{
								state[splitLine[0]] = int.Parse(splitLine[1]);
								if(splitLine[0] == "Global Savegame"){
									savePath += splitLine[1]+".txt";
									currentSlot = int.Parse(splitLine[1]);
									findSaveGameSlot = true;
								}
							}
							catch
							{
								Debug.Log("loader exception");
								state[splitLine[0]] = -1;
							}
						}
						catch
						{
							Debug.Log("Global file reader: Failed to split line: " + line);
						}
					}
				}

				if(!findSaveGameSlot){
					findSaveGameSlot = true;
					savePath += "1.txt";
					currentSlot = 1;
				}

				using (StreamReader reader = new StreamReader(savePath))
				{
					while (true)
					{
						string line = reader.ReadLine();
						if (line == null) break;
						
						try
						{
							string[] splitLine = line.Split(':');
							
							try
							{
								state[splitLine[0]] = int.Parse(splitLine[1]);
							}
							catch
							{
								Debug.Log("loader exception (DON'T WORRY ABOUT THIS): " + splitLine[1]);
								state[splitLine[0]] = splitLine[1];//-1;
							}
						}
						catch
						{
							Debug.Log("Global file reader: Failed to split line: " + line);
							
						}
					}
				}
			}
			catch
			{
				Debug.Log("Cannot open save file.");
			}

			if(!findSaveGameSlot){
				findSaveGameSlot = true;
				savePath += "1.txt";
				currentSlot = 1;
			}

			initialized = true;
		}

		static void SaveAllSubscribedObjects(bool saveFile = false)
		{
			for (int i = 0; i < statefulItemList.Count; i++)
			{
				StatefulItem item = (StatefulItem)statefulItemList[i];
				if(item)item.SaveState(saveFile);
			}
		}

		public static void RemoveAllSavedData(){
			if (!initialized) 	Initialize ();

			Debug.Log("removing all saved data ");

			int? music = GetState("Global music");
			int? sound = GetState("Global sound");
			int? savegame = GetState("Global Savegame");
			int? slot1 = GetState("Global level 1");
			int? slot2 = GetState("Global level 2");
			int? slot3 = GetState("Global level 3");
			state = new Hashtable();
			SetState("Global music", music);
			SetState("Global sound", sound);
			SetState("Global level 1", slot1);
			SetState("Global level 2", slot2);
			SetState("Global level 3", slot3);
			SetState("Global Savegame", savegame, true);
			File.Delete (savePath);
		}

		public static void SaveState()
		{
			if (!initialized) 	Initialize ();

			SaveAllSubscribedObjects(false);

		//	Debug.LogWarning("save path: " + savePath + " def save path: " + defaultSavePath);

			StreamWriter writer = new StreamWriter (savePath, false);

			StreamWriter defaultWriter = new StreamWriter (defaultSavePath, false);

			foreach (DictionaryEntry entry in state) 
			{
				string key = entry.Key.ToString();
				if(key == "Global Savegame" || key == "Global music" || key == "Global sound" || key == "Global pause")
				{
					defaultWriter.WriteLine(key + ":" + entry.Value);
				}
				else if(key != "Global level" && key != "Global level 1" && key != "Global level 2" && key != "Global level 3"){
					writer.WriteLine(key + ":" + entry.Value);
				}

				if(key == "Global level"){
					defaultWriter.WriteLine(key + " " + currentSlot + ":" + entry.Value);
					writer.WriteLine(key + ":" + entry.Value);
				}
				else if(key == "Global level 1" && currentSlot != 1){
					defaultWriter.WriteLine(key + ":" + entry.Value);
				}
				else if(key == "Global level 2" && currentSlot != 2){
					defaultWriter.WriteLine(key + ":" + entry.Value);
				}
				else if(key == "Global level 3" && currentSlot != 3){
					defaultWriter.WriteLine(key + ":" + entry.Value);
				}
			}

			defaultWriter.Close ();
			writer.Close ();
		}

		public static string GetiPhoneDocumentsPath () 
		{ 
			// Your game has read+write access to /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/Documents 
			// Application.dataPath returns              
			// /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/myappname.app/Data 
			// Strip "/Data" from path 
			string path = Application.persistentDataPath.Substring (0, Application.persistentDataPath.Length - 5); 
			// Strip application name 
			path = path.Substring(0, path.LastIndexOf('/'));  
			return path + "/Documents"; 
		}
	}

	[AttributeUsage(AttributeTargets.All)]
	public class SaveStateAttribute : System.Attribute
	{
		public SaveStateAttribute() 
		{
		
		}
		
	}
}