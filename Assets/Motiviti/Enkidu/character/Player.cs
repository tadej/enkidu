
using UnityEngine;
using System;
using System.Collections;
using PigeonCoopToolkit.Navmesh2D;
using UnityEngine.SceneManagement;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	public class Player : StatefulItem 
	{
		int frameCounter = 0;
		bool flipYCoordinate = false;

		Action _AnimationActionPointCallBack = null;
		Action _AnimationFinishedCallBack = null;
		public enum State
		{
			None = -1,
			IdleFront = 0,
			IdleDiagonalFront = 1,
			IdleDiagonalBack = 2,
			IdleSide = 3,
			IdleBack = 4,
			WalkFront = 5,
			WalkDiagonalFront = 6,			// NOT YET SUPPORTED
			WalkSide = 7,
			WalkBack = 8,
			TalkFront = 9,
			TalkDiagonal = 10, 				
			IdleDiagonalFrontSmile = 11,
			WalkDiagonalBack = 12,			// NOT YET SUPPORTED
			SwitchOnOffSide = 14,
			PickUpSide = 15,
			PickUpFront = 18,
			IdleDiagonalFrontAction = 22,
			PushBack = 23,
			FixingBack = 24,
			TalkDiagonalBack = 25,
			IdleDiagonalFrontTurnBack = 26,
			TalkTurnBack = 29,
			PickUpFromFloorDiagonalFront = 30,
			StandUpFromFloor = 31,
			IdleBackAction = 32,
			PickUpFromFloorBack = 33,
			SurprisedDiagonalFront = 40,
			SurprisedJumpDiagonalFront = 41,
			GiveSideDown = 45,

			SideKnock = 46,
			Invisible = 1000000
		}

		public enum GestureConfigurations
		{
			idle=0,
			explaining=1,
			explainingExactly=2,
			extremelyExcited=3,
			proud=4,
			embarrassed=5
		}

		public enum BodyGestures
		{
			idle=0,
			frontLeanSlight=1,
			frontLean=2,
			backLeanSlight=3,
			backLean=4
		}

		public enum HeadGestures
		{
			idle=0,
			talk1=1,
			talk2=2,
			slowTalk=3,
			lookAtCamera=4,
			embarrassed=5
		}

		public enum NeckGestures
		{
			idle=0,
			leanFront=1,
			leanBack=2,
			lookBack=3
		}

		public enum EyeGestures
		{
			idle=0,
			talk=1,
			up=2,
			down=3,
			left=4,
			right=5,
			back=6,
			side=7,
			follow=8
		}
		public enum ArmGestures
		{
			idle = 0,
			me1 = 1,
			me2 = 2,
			me3 = 3,
			you1 = 5,
			you2 = 6,
			you3=7,
			excitedFists=8,
			stop=9,
			explain1=10,
			explain2=11,
			explain3=12,
			explain4=13,
			explain5=14,
			explain6=15,
			explain7=16,
			idea=17,
			why1=20,
			why2=21,
			oops1=25,
			oops2=26,
			thinking1=30,
			thinking2=31,
			proud=35,
			crossedArms=36,
			armsUp=40
		}
		public enum TalkMode
		{
			Default = 0,
			ThinkingChin = 1,
			ThinkingHead = 2,
			Excited = 3,
			OnPhone = 4,
			Proud = 5
		}

		public enum TalkDirection
		{
			Front = 0,
			Diagonal = 1,
			DiagonalBack = 2,
			TurnBack = 3
		}


		public enum EyeMode
		{
			Idle = 0,
			Talk = 1,
			Up = 2,
			Down = 3,
			Back = 6,
			Side = 7
		}

		public TalkDirection talkDirection = TalkDirection.Diagonal;

		State [] idleOrWalkingStates = {State.IdleFront,
			State.IdleDiagonalFront ,
			State.IdleDiagonalBack ,
			State.IdleSide ,
			State.IdleBack ,
			State.WalkFront ,
			State.WalkSide ,
			State.WalkBack ,
			State.IdleDiagonalFrontSmile,
			State.WalkDiagonalBack,
			State.WalkDiagonalFront,
			State.IdleDiagonalFrontAction,
			State.IdleBackAction,
			State.Invisible
			/*State.TalkDiagonal,
			State.TalkDiagonalBack,
			State.TalkFront,
			State.TalkTurnBack
			*/
			//State.PressSide,
			//State.PushBack
		};

		State [] walkingStates = {
			State.WalkFront ,
			State.WalkSide ,
			State.WalkBack
		};

		State [] idleStates = {State.IdleFront,
			State.IdleDiagonalFront ,
			State.IdleDiagonalBack ,
			State.IdleSide ,
			State.IdleBack ,
			State.IdleDiagonalFrontAction,
			State.IdleDiagonalFrontSmile,
			State.IdleDiagonalFrontTurnBack,
			State.IdleBackAction};

		public delegate void MovedEventHandler(object sender, EventArgs e);
		public event MovedEventHandler Moved;
		public bool isOnlyAVoice = false;
		public float speedMultiplier = 1f;

		bool useLoadingScreen = false;

		float lastTimeChangedDirection = 0;

		int voicesWontWorkIndex = 0;

		public State state;

		CharacterBrain.ArmGestures currentArmGesture;
		CharacterBrain.HeadGestures currentHeadGesture;
		CharacterBrain.EyeGestures currentEyeGesture;
		CharacterBrain.NeckGestures currentNeckGesture;
		CharacterBrain.BodyGestures currentBodyGesture;

		public bool talking = false;
		
		State lastState;

		float setDestinationTimeStamp = 0;

		public Transform elroyTalkFront, elroyTalkDiagonal, elroyAdv, elroyTalkDiagonalBack;
		PlayerHead PlayerHead;
		AdvCamera advCamera;

		bool ignoreNavMeshAgent = false;

		public PlayerBrain PlayerBrain;

		public CharacterHead conversationPartner = null;

		public Transform elroyTalk;

		ClickIndicator clickIndicator;

		Animator animator;

		NavmeshWalker navmeshWalker;

		public float direction = 1;
		float scale = 1;

		AudioManager audioManager;

		Vector3 scaleUnitVectorLeft = new Vector3(-1,1,1);
		Vector3 scaleUnitVectorRight = new Vector3(1,1,1);

		public bool inCutScene = false;
		public bool inConversation = false;

		CutsceneTools cutsceneTools;

		public InteractiveItem targetItem = null;
		InventoryItem heldItem = null;
		InventoryItem combiningItem = null;

		bool interruptingSentence = false;

		bool isInAutopilot = false;
		Vector3 autoPilotTarget = Vector3.zero;
		Vector3 autoPilotDelta = Vector3.zero;
		float autoPilotSpeed = 1 / 1.5f;
		
		float holdingSince = 0;
		bool wasHolding = false;

		Hashtable doors;

		Scene scene;

		bool overrideAutoPilotScale = false;

		float overridedAutoPilotScale = 1;

		Vector3 autoPilotStartedScale = Vector3.zero;

		float autoPilotStartedTime = 0;

		Vector3 autoPilotUseScaleVector = Vector3.one;

		Inventory inventory;

		Vector3 startHoldPosition;

		float timeDelayBeforePanning = 0.2f;

		public bool justReleasedAfterPeekPanning = false;

		int procesingInventory = 0;

		public bool isInUninterruptibleAction = false;

		bool processScale = true;
		// Time.time << class

		public bool elroyStaticButOperational = false;

		public bool loadingFinished = false;

		public string dialogTextObjectStr = "3DText-Elroy";

		float GetSpeedModifierBasedOnYPosition()
		{
			float smax = Global.scene.scaleMax;
			float smin = Global.scene.scaleMin;

			float scale = transform.localScale.y;

			float ret = (scale - smin) / (smax - smin);

			ret += 0.47f;

			if(ret > 1) ret = 1;

			if(ret < 0.7f) ret = 0.7f;

			return ret;
		}

		IEnumerator Start() 
		{
			scene = Global.scene;
			flipYCoordinate = scene.flipYCoordinate;
			if(scene && scene.startWithStaticCharacter) elroyStaticButOperational = true;

			animator = elroyAdv.GetComponent<Animator>();

			string arrivalDoor = Global.GetArrivalDoor();

			Global.FlurryLog ("Scene " + SceneManager.GetActiveScene().buildIndex);

			inventory = Global.inventory;

			base.Initialise();

			audioManager = Global.audioManager;

			advCamera = Global.advCamera;

			if(GameObject.Find ("CutsceneTools") != null)
			cutsceneTools = GameObject.Find ("CutsceneTools").GetComponent<CutsceneTools>();
			

			if(GameObject.Find ("ClickIndicator") != null)
			clickIndicator = GameObject.Find ("ClickIndicator").GetComponent<ClickIndicator> ();
		
			if(!this.isOnlyAVoice)
			{
				navmeshWalker = GetComponent<NavmeshWalker> ();

				doors = new Hashtable();

				foreach(DoorBetweenLevels door in GameObject.FindObjectsOfType<DoorBetweenLevels>())
				{
					doors[door.levelPath] = door;
				}

				if(scene.initialStateFinished == false)
				{
					if (scene.initialStateOneTime) animator.SetInteger("state", (int)scene.initialState);
					
					if(!elroyStaticButOperational)
						ChangeState(scene.initialState, true); // initialstate

				}

				if(Application.platform == RuntimePlatform.IPhonePlayer)
				{
					timeDelayBeforePanning = 0.05f;
				}
				else
				{
					timeDelayBeforePanning = 0.05f;
				}
	
				if(scene.arrivalCave != -1 && !elroyStaticButOperational)
				{
					Debug.Log(scene.arrivalCave + " =arrivalCave");
					DoorBetweenCaves []caveDoors = GameObject.FindObjectsOfType<DoorBetweenCaves>();
					DoorBetweenCaves enterDoor = null;
					for(int i=0; i<caveDoors.Length; i++){
						DoorBetweenCaves doorC = caveDoors[i]; 
						if(doorC.caveNumber == scene.arrivalCave){
							enterDoor = doorC;
							break;
						}
					}
					if(enterDoor.UseEnterCaveProcedure)
						yield return StartCoroutine(EnterCave(enterDoor));

				}
				else if(!string.IsNullOrEmpty(arrivalDoor) && !elroyStaticButOperational)
				{
					yield return StartCoroutine(EnterRoom(arrivalDoor));
					yield return null;
				}   
				else
				{
				
					if (advCamera != null && advCamera.followElroy)
					advCamera.transform.position = transform.position + Vector3.up * advCamera.yFollowOffset;
		
					if (!elroyStaticButOperational)
					{
						ChangeState(State.IdleDiagonalFront);
		
					}
					
				}

				bool skipRest = false;

				if (!skipRest)
				{

					if (!string.IsNullOrEmpty(scene.sentenceAfterInitialState) && !scene.initialStateFinished)
					{
						talkDirection = TalkDirection.Diagonal;
					
						ChangeState(State.TalkFront);

						if (scene.StartCam != null)
						{
							scene.StartCam.enabled = true;
						}

						yield return StartCoroutine(SpeakProcedure(scene.sentenceAfterInitialState));

						ChangeState(State.IdleDiagonalFront);
					}
					else
					{
						if(!scene.stayInCutsceneAfterInitialState)
						{
							if (cutsceneTools)
								cutsceneTools.SetFadeout(false);
						}
					}
	
					scene.SaveState();
					SaveState();

				}
				else if(scene.initialStateFinished)
				{
					SetInCutScene(false);
				}

				if(scene.initialStateFinished){
					SetInCutScene (false);

					ChangeState(State.IdleDiagonalFront);
				}

				Debug.Log(currentArmGesture + " " + currentHeadGesture + " " + currentNeckGesture + " " + currentBodyGesture);

			}

			if(scene.stayInCutsceneAfterInitialState)
			{
				SetInCutScene(true);
			}

			if(scene.startWithStaticCharacter)
			{
				ChangeState(State.Invisible);
				elroyStaticButOperational = true;
			}

			yield return null;
			loadingFinished = true; // important
			
		}

		public float TimeSinceSetDestination()
		{
			return Time.time - setDestinationTimeStamp;
		}

		public void AnimationEventEndCutScene()
		{
			if(scene.initialStateOneTime && !scene.initialStateFinished)
			{
				ChangeState (State.IdleDiagonalFront);
				StartCoroutine (ExitCutscene ());
			}
		}

		public float GetXVelocity()
		{
			return navmeshWalker.GetVelocity ().x;
		}

		IEnumerator ExitCutscene()
		{
			ChangeState(State.IdleDiagonalFront);
			ProcessScale();

			if(string.IsNullOrEmpty(scene.sentenceAfterInitialState))SetInCutScene(false);
			
			if (!string.IsNullOrEmpty (scene.sentenceAfterInitialState) && !scene.initialStateFinished) {
				talkDirection = TalkDirection.Diagonal;


				SetInCutScene (true);

				yield return null;//yield return new WaitForSeconds(0.5f);

				yield return StartCoroutine (SpeakProcedure (scene.sentenceAfterInitialState));
				
				ChangeState (State.IdleDiagonalFront);
				
				if(!scene.stayInCutsceneAfterInitialState)
					SetInCutScene (false);
			}

			scene.initialStateFinished = true;

			scene.SaveState();
			SaveState();

			yield return null;
		}

		public void TeleportElroyToPosition(Vector3 pos)
		{
			if(!IsIdleOrWalking()){
				ChangeState(State.IdleDiagonalFront);
			}
			if (targetItem != null && !targetItem.IsInInventory() && (advCamera.IsVisible (targetItem.transform) || !IsIdleOrWalking()) ) {
				SetDestination (targetItem.transform.position + targetItem.GetStoppingDistance(heldItem));
			} else {
				targetItem = null;

			}
		}

		public IEnumerator SetDestinationInterim(Vector3 worldPosition)
		{
			if (!elroyStaticButOperational)
			{

				if (navmeshWalker.GetVelocity().sqrMagnitude > 0)  //mr if elroy is already walking we shouldn't change his destination 
				{
				}
				else if (IsIdleOrWalking())
				{
					var ti = targetItem;

					if (targetItem != null)
					{
						targetItem = null;
					}

					SetDestination(worldPosition);
					yield return new WaitForSeconds(0.2f);

					if (ti != null && !ti.IsInInventory() && advCamera.IsVisible(ti.transform))
					{
						SetDestination(ti.transform.position + ti.GetStoppingDistance(heldItem));
						targetItem = ti;
					}

					justReleasedAfterPeekPanning = false;
				}
			}
			yield return null;
		}


		public PlayerHead GetActiveHead()
		{
			return PlayerHead;
		}

		public void HoldItem(InventoryItem item)
		{
			Debug.Log ("holding item " + item);
			heldItem = item;

		}

		void HideElroyTalk()
		{
			elroyTalkDiagonal.GetComponent<Renderer>().enabled = false;
			elroyTalkDiagonalBack.GetComponent<Renderer>().enabled = false;
			elroyTalkFront.GetComponent<Renderer>().enabled = false;
		}

		public Bounds GetBounds()
		{
			return Global.player.GetComponent<Renderer>().bounds;
		}

		public void SetInCutScene(bool cs, CutsceneTools.Type type = CutsceneTools.Type.BlackBands, Vector3 pos = default(Vector3), bool startOn = false, float minimumuCircleSize = 0.01f, bool hideInventory=true)
		{
			if(Global.inventory && (!cs || hideInventory)) Global.inventory.SetEnabled(!cs);
			
			inCutScene = cs;
			if(animator!=null)
			{
				if(!elroyStaticButOperational)
				//animator.SetBool("InCutScene", cs);
				if(cutsceneTools)
					cutsceneTools.SetCutscene(cs ?type : CutsceneTools.Type.None, pos, startOn, minimumuCircleSize);
			}
		}

		bool IsTalking(State st)
		{
			return st == State.TalkDiagonal || state == State.TalkFront || state == State.TalkDiagonalBack;
		}

		public void SayThatWontWork()
		{
			int wontWorkCount = scene.voicesWontWork.Length;

			if(wontWorkCount < 1) return;

			Speak ( scene.voicesWontWork[ voicesWontWorkIndex], TalkMode.ThinkingHead );

			voicesWontWorkIndex++;
			if(voicesWontWorkIndex > scene.voicesWontWork.Length-1) voicesWontWorkIndex = 0;
		}

		public void SayItsLocked(string lockedComment)
		{
			Speak ( lockedComment, TalkMode.ThinkingChin );
		}

		public void TurnTowardsPlayer()
		{
			talkDirection = TalkDirection.Front;
			ApplyTalkDirection();
		}

		public void ChangeTalkDirection(TalkDirection dir)
		{
			talkDirection = dir;
			ApplyTalkDirection ();
		}

		public void ChangeDirection(int x){
			if(x<0)
				direction = -1;
			else
				direction = 1;
			ProcessScale();
		}

		void ApplyTalkDirection()
		{
			if(talkDirection == TalkDirection.Diagonal)
			{
				elroyTalk = elroyTalkDiagonal;
				//PlayerHead = PlayerHeadDiagonal;
			}
			else if(talkDirection == TalkDirection.DiagonalBack)
			{
				elroyTalk = elroyTalkDiagonalBack;
				//PlayerHead = PlayerHeadDiagonalBack;
			}
			else if(talkDirection == TalkDirection.TurnBack)
			{
				elroyTalk = elroyTalkDiagonal;
				//PlayerHead = PlayerHeadDiagonal;
			}
			else
			{
				elroyTalk = elroyTalkFront;
				//PlayerHead = PlayerHeadFront;
			}
		}

		public void ChangeState( State newState, bool overrideSameStateCheck=false, Action AnimationActionPointCallBack = null, Action AnimationFinishedCallBack = null)
		{
			_AnimationActionPointCallBack = AnimationActionPointCallBack;
			_AnimationFinishedCallBack = AnimationFinishedCallBack;

			if (newState == State.None)
				return;

			if(state == newState && !overrideSameStateCheck) return;

			Debug.Log (state + " -> " + newState);

			StartCoroutine(ChangeStateProcess(newState, overrideSameStateCheck));
		}

		public void AnimationFinished()
		{
			Debug.Log ("animation finished");
		}

		void OnEnable()
		{
			StartCoroutine(RandomizeIdleStatesProc());
		}
		IEnumerator RandomizeIdleStatesProc()
		{
			while(true)
			{
				if(animator && IsIdle() )animator.SetInteger("idleMode", 0);
				yield return new WaitForSeconds(0.8f + UnityEngine.Random.value * 6f);
				if(animator && IsIdle() )animator.SetInteger("idleMode", Mathf.RoundToInt(6 * UnityEngine.Random.value));
				yield return new WaitForSeconds(1.2f + UnityEngine.Random.value * 6f);
			}
		}

		public void ChangeMood(CharacterHead.Moods mood)
		{
			PlayerBrain.ChangeMood(mood);
		}
		IEnumerator ChangeStateProcess(State newState, bool overrideSameStateCheck=false)
		{
			if(animator) animator.SetInteger("idleMode", 0);//.RoundToInt(6 * UnityEngine.Random.value));

			switch (newState) 
			{

			case State.IdleDiagonalFront:
				if(PlayerHead)
					PlayerHead.SetMood ((int)PlayerHead.Moods.Neutral);
				break;
			case State.PushBack:
				talkDirection = TalkDirection.DiagonalBack;
				break;
			}

			ApplyTalkDirection();

			Debug.Log("ChangeStateProcess " + state + "->" + newState);

			state = newState;

			animator.SetInteger( "state", (int)state );

			bool dis = true;
			if(state == State.TalkDiagonal)
			{
				dis = false;
				//ProcessDirectionAndScale(false);
				
			}
			
			animator.SetBool("disable", dis);
			
			lastState = newState;

			yield return null;
		}

		public void Speak(string clipName, TalkMode mode = TalkMode.Default)
		{
			StartCoroutine(SpeakProcedure(clipName, mode));
		}

		public IEnumerator StandWithMoodProcedure( float duration, CharacterHead.Moods mood, TalkMode mode = TalkMode.Default, bool switchBack=true )
		{
			SwitchToTalking ();

			if(!elroyStaticButOperational)
			animator.SetInteger("talkMode", (int)mode);
			PlayerHead.SetMood((int)mood);

			yield return new WaitForSeconds(duration);

			if(switchBack) SwitchToIdle ();
			yield return null;
		}

		void SwitchToTalking()
		{
			//Debug.Log ("SwitchToTalking::talkdirection=" + talkDirection);
			talking = true;
			animator.SetBool("talking", true);

			PlayerBrain.SetEyeGesture (CharacterBrain.EyeGestures.talk);
		}

		void SwitchToIdle()
		{
			PlayerBrain.SetEyeGesture(CharacterBrain.EyeGestures.idle);
		}

		void ChangeStateToIdleBasedOnDirection()
		{
			Direction dir = GetDirection();

			switch(dir)
			{
				case Direction.FRONT:
					ChangeState(State.IdleFront);
					break;
				case Direction.BACK:
					ChangeState(State.IdleBack);
					break;
				case Direction.SIDE:
					ChangeState(State.IdleSide);
					break;
				case Direction.DIAGONAL_FRONT:
					ChangeState(State.IdleDiagonalFront);
					break;

			}
		}

		void ChangeStateToIdleBasedOnWalkAnimation(State wi)
		{
			switch(wi)
			{
				case State.WalkFront:
					ChangeState(State.IdleFront);
					break;

				case State.WalkBack:
					ChangeState(State.IdleBack);
					break;

				case State.WalkSide:
					ChangeState(State.IdleSide);
					break;

				default:
					ChangeState(State.IdleFront);
					break;
			}
		}

		public void StopTalking()
		{
			conversationPartner = null;
			animator.SetBool("talking", false);
			interruptingSentence = true;
			PlayerBrain.StopTalking ();
		}

		public void StopWalking(){
			if(navmeshWalker != null) navmeshWalker.Stop();
		}

		public IEnumerator SpeakProcedure( string clipName, TalkMode mode = TalkMode.Default, bool remainInConversation = false )
		{
			StopWalking (); 
			PlayerBrain.interruptFlag = false;
			Debug.Log("mode=" + mode);

			interruptingSentence = false;

			if(!elroyStaticButOperational)
			SwitchToTalking ();

			if(!elroyStaticButOperational)
			animator.SetInteger("talkMode", (int)mode);
			animator.SetBool("talking", true);
			yield return StartCoroutine(PlayerBrain.Talk(clipName));
		
			if (!interruptingSentence)
			{
				if(!remainInConversation)
				{
					if(!elroyStaticButOperational) SwitchToIdle();
				}
			}
		
			animator.SetBool("talking", false);
		}

		public void ChangeTalkMode(string mode)
		{
			if(!elroyStaticButOperational && state == State.TalkDiagonal && !string.IsNullOrEmpty(mode))
			{
				string [] parts = mode.Split('/');

				if(parts.Length == 1)
				{
					try
					{
						CharacterBrain.GestureConfigurations conf = (CharacterBrain.GestureConfigurations)Enum.Parse(typeof(CharacterBrain.GestureConfigurations), parts[0]);
						ChangeGestureConfiguration(conf);
					}
					catch
					{
						Debug.LogWarning("Couldn't parse gesture configuration " + mode + " " + parts[0]);
					}
				}
				else if(parts.Length >= 2)
				{
					switch(parts[0])
					{
						case "arms":
							try
							{
								CharacterBrain.ArmGestures gest = (CharacterBrain.ArmGestures)Enum.Parse(typeof(CharacterBrain.ArmGestures), parts[1]);
								ChangeGestureArms(gest);
							}
							catch
							{
								Debug.LogWarning("Couldn't parse gesture arms " + mode + " " + parts[1]);
							}
							break;
						
						case "eyes":
							try
							{
								CharacterBrain.EyeGestures gest = (CharacterBrain.EyeGestures)Enum.Parse(typeof(CharacterBrain.EyeGestures), parts[1]);
								ChangeGestureEyes(gest);
							}
							catch
							{
								Debug.LogWarning("Couldn't parse gesture eyes " + mode + " " + parts[1]);
							}
							break;

						case "head":
							try
							{
								CharacterBrain.HeadGestures gest = (CharacterBrain.HeadGestures)Enum.Parse(typeof(CharacterBrain.HeadGestures), parts[1]);
								ChangeGestureHead(gest);
							}
							catch
							{
								Debug.LogWarning("Couldn't parse gesture head " + mode + " " + parts[1]);
							}
							break;

						case "neck":
							try
							{
								CharacterBrain.NeckGestures gest = (CharacterBrain.NeckGestures)Enum.Parse(typeof(CharacterBrain.NeckGestures), parts[1]);
								ChangeGestureNeck(gest);
							}
							catch
							{
								Debug.LogWarning("Couldn't parse gesture neck " + mode + " " + parts[1]);
							}
							break;

						case "body":
							try
							{
								CharacterBrain.BodyGestures gest = (CharacterBrain.BodyGestures)Enum.Parse(typeof(CharacterBrain.BodyGestures), parts[1]);
								ChangeGestureBody(gest);
							}
							catch
							{
								Debug.LogWarning("Couldn't parse gesture body " + mode + " " + parts[1]);
							}
							break;	

						default:
							Debug.LogWarning("Unknown gesture type, " + mode);
							break;
					}
				}
			}
		}

		void ChangeGestureArms(CharacterBrain.ArmGestures ag)
		{
			animator.SetInteger("arms", (int)ag);
			currentArmGesture = ag;
		}

		public CharacterBrain.EyeGestures GetCurrentEyeGesture()
		{
			return currentEyeGesture;
		}
		void ChangeGestureEyes(CharacterBrain.EyeGestures eg)
		{
			currentEyeGesture = eg;
			if(PlayerBrain != null) PlayerBrain.SetEyeGesture(eg);
		}

		void ChangeGestureHead(CharacterBrain.HeadGestures hg)
		{
			animator.SetInteger("head", (int)hg);	
			currentHeadGesture = hg;	
		}

		void ChangeGestureNeck(CharacterBrain.NeckGestures ng)
		{
			animator.SetInteger("neck", (int)ng);	
			currentNeckGesture = ng;	
		}

		void ChangeGestureBody(CharacterBrain.BodyGestures bg)
		{
			animator.SetInteger("body_lean", (int)bg);	
			currentBodyGesture = bg;	
		}

		void ChangeGestureConfiguration(CharacterBrain.GestureConfigurations conf)
		{
			switch(conf)
			{
				case CharacterBrain.GestureConfigurations.idle:
					ChangeGestureArms(CharacterBrain.ArmGestures.idle);
					ChangeGestureBody(CharacterBrain.BodyGestures.idle);
					ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
					ChangeGestureHead(CharacterBrain.HeadGestures.idle);
					ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
					break;

				case CharacterBrain.GestureConfigurations.explaining:
					ChangeGestureArms(CharacterBrain.ArmGestures.explain5); // or 6 or 7
					ChangeGestureBody(CharacterBrain.BodyGestures.backLeanSlight);
					ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
					ChangeGestureHead(CharacterBrain.HeadGestures.talk1);
					ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
					break;

				case CharacterBrain.GestureConfigurations.explainingExactly:
					ChangeGestureArms(CharacterBrain.ArmGestures.explain4); 
					ChangeGestureBody(CharacterBrain.BodyGestures.frontLeanSlight);
					ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
					ChangeGestureHead(CharacterBrain.HeadGestures.talk1);
					ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
					break;

				case CharacterBrain.GestureConfigurations.extremelyExcited:
					ChangeGestureArms(CharacterBrain.ArmGestures.armsUp); 
					ChangeGestureBody(CharacterBrain.BodyGestures.backLean);
					ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
					ChangeGestureHead(CharacterBrain.HeadGestures.talk1);
					ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
					break;

				case CharacterBrain.GestureConfigurations.proud:
					ChangeGestureArms(CharacterBrain.ArmGestures.proud); 
					ChangeGestureBody(CharacterBrain.BodyGestures.backLeanSlight);
					ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
					ChangeGestureHead(CharacterBrain.HeadGestures.talk1);
					ChangeGestureEyes(CharacterBrain.EyeGestures.talk);
					break;

				case CharacterBrain.GestureConfigurations.embarrassed:
					ChangeGestureArms(CharacterBrain.ArmGestures.crossedArms); 
					ChangeGestureBody(CharacterBrain.BodyGestures.frontLeanSlight);
					ChangeGestureNeck(CharacterBrain.NeckGestures.idle);
					ChangeGestureHead(CharacterBrain.HeadGestures.embarrassed);
					ChangeGestureEyes(CharacterBrain.EyeGestures.down);
					break;
			}
		}

		public void AnimateWithCallBack(Player.State animation, Action AnimationActionPointCallBack, Action AnimationFinishedCallBack)
		{
			ChangeState(animation, true, AnimationActionPointCallBack, AnimationFinishedCallBack);
		}

		public void AnimationActionPoint(string animationName)
		{
			if(targetItem) targetItem.AnimationActionPoint(animationName);

			if(_AnimationActionPointCallBack != null)
			{
				_AnimationActionPointCallBack();
			}
		}
		
		public void AnimationFinished(string animationName)
		{
			if (targetItem) 
			{
				targetItem.AnimationFinished (animationName);
				targetItem = null;
			}
			
			if (_AnimationFinishedCallBack != null)
			{
				_AnimationFinishedCallBack();
			}

			if(animationName == "Idle")
			{
				if(animator)animator.SetInteger("idleMode", 0);
			}

		}

		Vector3 GetClosestPosition(Vector3 inPos)
		{
			Vector3 outPos = Vector3.zero;

			UnityEngine.AI.NavMeshHit hit;

			if( UnityEngine.AI.NavMesh.SamplePosition(  inPos, out hit, 500, navmeshWalker.walkableMask) )
			{
				outPos = hit.position;
			}

			Debug.Log ("closest pos: " + outPos);

			return outPos;
		}

		void ProcessInput (ref bool isHit, ref bool holding, ref Vector3 holdPosition, ref Vector3 worldPosition, ref InteractiveItem interactiveItem, ref bool justTouched)
		{
			justTouched = false;

			foreach (Touch touch in Input.touches) 
			{			
				holding = true;
				if(touch.phase == TouchPhase.Began)
				{
					justTouched = true;
					holdPosition = touch.position;
				
					startHoldPosition = holdPosition;
				}
				
				if(touch.phase == TouchPhase.Ended)
				{
					holding = false;
					holdPosition = touch.position;
					isHit = true;
				}
			}
			
			if(Input.GetButtonUp ("Fire1"))
			{
				holding = false;
				holdPosition = Input.mousePosition;
				isHit = true;
			}

			if(Input.GetButtonDown ("Fire1"))
			{
				justTouched = true;
				startHoldPosition = Input.mousePosition;
			}
			
			if(Input.GetButton ("Fire1"))
			{
				holding = true;
				holdPosition = Input.mousePosition;
			}

			worldPosition = Global.activeCamera.ScreenToWorldPoint(holdPosition);

			if(isHit)
			{
				Vector2 v = Global.activeCamera.ScreenToWorldPoint(holdPosition);
				
				Collider2D[] col = Physics2D.OverlapPointAll(v);

				interactiveItem = null;

				if(col.Length > 0)
				{
					int lastLayer = int.MinValue;

					foreach(Collider2D c in col)
					{
						if(c.gameObject.layer == 10)
						{
							var elroyItem1 = c.gameObject.GetComponent<InteractiveItem>();

							if(elroyItem1 != null && elroyItem1.enabled && elroyItem1.itemLayer > lastLayer)
							{

								lastLayer = elroyItem1.itemLayer;

								interactiveItem = elroyItem1;

								Vector3 posx = interactiveItem.transform.position + interactiveItem.GetCenterOffset(heldItem);// interactiveItem.collider2D.bounds.center;// + interactiveItem.transform.position;

								worldPosition = posx + interactiveItem.GetStoppingDistance(heldItem);

							}
						}
					}
				}
			}
		}

		Vector3 GetNavmeshPosFromWorldPos( Vector3 n )
		{
			Vector3 navMeshPosition = transform.position;

			//navMeshPosition = RotatePointAroundPivot(n, Vector3.zero, Vector3.right * 90);

			//navMeshPosition = new Vector3(navMeshPosition.x, navMeshAgent.transform.position.y, navMeshPosition.y);

			return navMeshPosition;
		}

		Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) 
		{
			Vector3 dir = point - pivot; // get point direction relative to pivot
			dir = Quaternion.Euler(angles) * dir; // rotate it
			point = dir + pivot; // calculate rotated point
			return point; // return it
		}

		public void AnimateGiving()
		{
			StartCoroutine(AnimateGivingProc());
		}

		IEnumerator AnimateGivingProc()
		{
			var s = state;
			ChangeState(State.SwitchOnOffSide);
			yield return new WaitForSeconds(0.8f);
			ChangeState(s);
		}

		public void ProcessInventoryInteraction()
		{
			Vector3 holdPosition = Vector3.zero;
			bool justTouched = false;
			bool clicked1 = false;
			bool isHolding1 = false;
			Vector3 holdPosition1 = Vector3.zero;
			Vector3 worldPosition1 = Vector3.zero;
			InventoryItem item1 = null;
			
			inventory.ProcessInput(ref clicked1, ref isHolding1, ref holdPosition1, ref worldPosition1, ref item1);
			
			if(isHolding1)
			{
				inventory.inventoryItemPosition = worldPosition1;
				
				if(inventory.heldItem == null && !advCamera.IsPeekPanning())
				{
					if(item1 != null)inventory.HoldItem(item1);
				}
				
				if(item1 == null)
				{
					if(justTouched)
						advCamera.SetPeekPanning(true);
					else if (Input.touches.Length < 2) //added because of two finger jumping screen
					{
						Vector3 delta = advCamera.GetComponent<Camera>().ScreenToWorldPoint(holdPosition) - advCamera.GetComponent<Camera>().ScreenToWorldPoint(startHoldPosition);
						
						advCamera.AdjustPeekPanning( -delta );
					}
					
				}
				
				
			}
			else
			{
				ReturnItemToInventory();
			}
		}

		public void PublicReturnItemToInventory()
		{
			if(inventory.heldItem != null){
				inventory.ReturnItem(inventory.heldItem);
				inventory.HoldItem(null);
				inventory.heldItem = null;
			}
		}

		void ReturnItemToInventory()
		{
			if(inventory.heldItem != null)
			{
				if(Time.realtimeSinceStartup - holdingSince < 0.2f)
				{
					inventory.heldItem.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
				}
				inventory.ReturnItem(inventory.heldItem);
				inventory.HoldItem(null);
				inventory.heldItem = null;
			}
		}

		bool IsWalkingNecessary(InteractiveItem item)
		{
			if(Mathf.Abs (transform.position.x - item.transform.position.x) < 9.5f) 
				return false;
			return true;
		}

		IEnumerator ProcessInteraction()
		{
			bool justLooking = false;
		//	bool walkingNecessary = true;
			bool clicked1 = false;
			bool isHolding1 = false;
			Vector3 holdPosition1 = Vector3.zero;
			Vector3 worldPosition1 = Vector3.zero;
			InventoryItem item1 = null;
			bool inventoryClick = false;
			if(!isInUninterruptibleAction)
			if(inventory.ProcessInput(ref clicked1, ref isHolding1, ref holdPosition1, ref worldPosition1, ref item1)){
				Debug.Log("procesing inventory" + clicked1);
				procesingInventory = 1;
				if(clicked1){
					procesingInventory = 0;
					inventoryClick = true;
				//	return;
				}
				if(item1 != null || heldItem != null)
					procesingInventory = 0;
			//	return;
			}

			if (inventory.MouseOverInventory ()) {
				procesingInventory = 1;
			}

			bool clicked = false;
			bool isHolding = false;
			Vector3 holdPosition = Vector3.zero;
			Vector3 worldPosition = Vector3.zero;
			InteractiveItem interactiveItem = null;
			bool justTouched = false;

			if(!isInUninterruptibleAction)
				ProcessInput(ref clicked, ref isHolding, ref holdPosition, ref worldPosition, ref interactiveItem, ref justTouched);
			else{
				if(navmeshWalker.GetVelocity ().sqrMagnitude == 0)
					isInUninterruptibleAction = false;
			}

			bool callElroyMoved = false;

			if (!wasHolding && isHolding) 
			{
				holdingSince = Time.realtimeSinceStartup;
			}


			wasHolding = isHolding;

			if(isHolding1)
			{
				inventory.inventoryItemPosition = worldPosition1;

				if(inventory.heldItem == null && !advCamera.IsPeekPanning())
				{
					if(item1 != null)inventory.HoldItem(item1);
				}
				
				if(item1 == null && heldItem == null)
				{
					if(Time.realtimeSinceStartup - holdingSince > timeDelayBeforePanning)
					{
						if(!advCamera.IsPeekPanning() && procesingInventory <= 0)
						{
							startHoldPosition = holdPosition;
							advCamera.SetPeekPanning(true);
						}
						else if (Input.touches.Length < 2)//added because of two finger jumping screen
						{   
							Vector3 delta = Global.activeCamera.ScreenToWorldPoint(holdPosition) - Global.activeCamera.ScreenToWorldPoint(startHoldPosition);
							
							advCamera.AdjustPeekPanning( -delta );
						
						}
					}	
				}
				else
				{

				}
				
			}
			else
			{
				ReturnItemToInventory();
			}


			if(clicked && !inCutScene && !Global.inPause /* && procesingInventory <= 0*/)
			{
			

				if(!advCamera.WasPeekingLongEnough())
				{
					clickIndicator.transform.position = worldPosition;

					//sem pridemo 

					if(interactiveItem != null)
					{
						if(interactiveItem.IsInInventory() == false && !inventoryClick)
						{
							//Debug.Log("TT: " + interactiveItem.DoubleClickTime());

							if (interactiveItem.SupportsDoubleClick() && interactiveItem.DoubleClickTime() < 0.75f && !isInAutopilot && !inCutScene)
							{
								interactiveItem.DoubleClick();
							}

							if (interactiveItem.isClickIndicatorFlipped){
								clickIndicator.isFlipped = true;
							}
							else{
								clickIndicator.isFlipped = false;
							}

							if (interactiveItem.IsPerson() && heldItem == null)
							{
								clickIndicator.ChangeState(ClickIndicator.State.Talk);
							}
							else
							if (interactiveItem.IsExit())
							{
								clickIndicator.ChangeState(ClickIndicator.State.Exit);
							}
							else
							if (interactiveItem.CanBeUsed())
							{
								clickIndicator.ChangeState(ClickIndicator.State.Use);
							}
							else
							if (interactiveItem.CanBeCombinedWith.Length > 0 && heldItem != null)
							{
								clickIndicator.ChangeState(ClickIndicator.State.Use);
							}
							else
							if (heldItem != null)
							{
								clickIndicator.ChangeState(ClickIndicator.State.Use);
							}
							else
							{
								clickIndicator.ChangeState(ClickIndicator.State.Look);

								if (!interactiveItem.needToWalkToItem && !isInAutopilot && !inCutScene)
								{
									targetItem = interactiveItem;
									navmeshWalker.Stop();
									talkDirection = TalkDirection.Front;

									ProcessDirectionAndScale(false);

									justLooking = true;

									if (IsTalking(state))
									{
										talkDirection = TalkDirection.Front;
										interruptingSentence = true;
										StopTalking();
									}

									talkDirection = TalkDirection.Front;
									ArrivedAt(heldItem);

									targetItem = null;
								}
							
							}
						}

						interactiveItem.Selected();

						clickIndicator.transform.position = advCamera.GetComponent<Camera>().ScreenToWorldPoint(holdPosition);

					
					}
					else if(!inventoryClick)
					{
						clickIndicator.isFlipped = false;
						clickIndicator.ChangeState(ClickIndicator.State.Walk);
					}

					if(!isHolding)
					{
						combiningItem = heldItem;
						heldItem = null;
					}

					if(IsTalking(state))
					{
						interruptingSentence = true;
						StopTalking();
						SwitchToIdle();
					}

					if(IsIdleOrWalking())
					{
						if(!justLooking) targetItem = interactiveItem;

						var iho = Global.inventory.ItemHoveringOver ();
						if (iho != null /* TG && targetItem == null */) {
							targetItem = iho.GetComponent<InteractiveItem>();
						}

						if (targetItem != null && combiningItem != null && targetItem.IsInInventory() && targetItem != combiningItem)
						{
							// Already there

							ArrivedAt(combiningItem);
						}

						if (targetItem != null && targetItem.IsInInventory())
						{
							navmeshWalker.Stop ();
						}
						else if(!inventoryClick)
						{
							if(!justLooking && !elroyStaticButOperational && (procesingInventory <= 0)) worldPosition = SetDestination(worldPosition);
				

							callElroyMoved = true;

							bool x = CheckIfCombiningMatchingItems(targetItem, combiningItem);

							if(x)
								isInUninterruptibleAction = x;

							if(targetItem != null && (navmeshWalker.HasArrived() || elroyStaticButOperational)/*0.06f*/)
							{
								advCamera.SetPeekPanning(false);
								yield return null;
						
								navmeshWalker.Stop();

								yield return null;
								ArrivedAt (combiningItem);
							}
						}
					}
				}

				advCamera.SetPeekPanning(false);
			}
			else if(clicked){
				procesingInventory--;
				advCamera.SetPeekPanning(false);
			}
			
			if(callElroyMoved && !justLooking)advCamera.ElroyMoved();

			if (!inventory.MouseOverInventory ()) {
				procesingInventory = 0;
			}

			yield return null;
		}
	
		public bool CheckIfCombiningMatchingItems(InteractiveItem targetItem, InventoryItem combiningItem){

			if(targetItem == null)
				return false;

			bool x = targetItem.CheckIfCombiningMatchingItems(combiningItem);

			return x;
		}

		public Vector3 SetDestination(Vector3 worldPosition)
		{
			if (elroyStaticButOperational)
			{
				
			}
			else
			{
				if(inventory)inventory.SetVisible(false);
				navmeshWalker.SetTarget(worldPosition);
				setDestinationTimeStamp = Time.time;

				if(Moved != null) Moved(this, null);
			}
		
			return worldPosition;
		}

		enum Direction
		{
			FRONT=0,
			BACK=1,
			SIDE=2,
			DIAGONAL_FRONT=3
		}

		Direction GetDirection()
		{
			Direction dir;

			switch(state)
			{
				case State.WalkFront:
				case State.TalkFront:
				case State.IdleFront:
					dir = Direction.FRONT;
					break;
				case State.WalkBack:
				case State.IdleBack:
				case State.TalkTurnBack:
					dir = Direction.BACK;
					break;
				case State.WalkSide:
				case State.PickUpSide:
				case State.SideKnock:
				case State.GiveSideDown:
				case State.IdleSide:
					dir = Direction.SIDE;
					break;

				default:
					dir = Direction.DIAGONAL_FRONT;
					break;
				
			}

			return dir;
		}

		public IEnumerator EnterRoom(string levelIndex)
		{
			Debug.Log ("EnterRoom " + levelIndex);

			DoorBetweenLevels door = (DoorBetweenLevels)doors[levelIndex];
			InteractiveItemChangeCamera doorCam = null;

			if(door == null)
			{
				Debug.Log ("ERROR: door " + levelIndex + " == null");
				SetInCutScene(true, CutsceneTools.Type.ZoomIn, transform.position + Vector3.up*advCamera.yFollowOffset, false);

				if(!elroyStaticButOperational) 
				{
					//ChangeState(State.IdleDiagonalFront);
					ChangeStateToIdleBasedOnDirection();
				}

				if(!scene.stayInCutsceneAfterInitialState)
					SetInCutScene(false);
			}
			else
			{
				autoPilotSpeed = door.walkInOutSpeed;
				transform.position = door.gameObject.transform.position + door.outsideOffset;

				ProcessDirectionAndScale();
				doorCam = door.gameObject.GetComponent<InteractiveItemChangeCamera>();
			
				if (doorCam)
					StartCoroutine(doorCam.ProcessArrivedAt());
				
				heldItem = combiningItem = null;
				targetItem = null;

				
				Vector3 cameraPos = transform.position + Vector3.up*advCamera.yFollowOffset;

				cameraPos.z = -5;

				advCamera.Move(new Vector3(cameraPos.x, advCamera.transform.position.y, cameraPos.z), true);

			//tg yield return null;

				if(door.zoomoutCircleTransformPosition != null)
					SetInCutScene(true, CutsceneTools.Type.ZoomIn, door.zoomoutCircleTransformPosition.position, false, 0.01f, false);
				else
					SetInCutScene(true, CutsceneTools.Type.ZoomIn, transform.position + Vector3.up*advCamera.yFollowOffset, false, 0.01f, false);
				

				if(door.openCloseAnimator)
				{
					transform.position = door.gameObject.transform.position + door.outsideOffset;
					door.openCloseAnimator.SetBool("open", true);
					ChangeStateToIdleBasedOnWalkAnimation(door.animationWalkIn);
					//ChangeStateToIdleBasedOnDirection();

					yield return new WaitForSeconds(0.7f);
				}

			
			//	yield return new WaitForSeconds(0.5f);
				
			//	ChangeState(State.WalkSide);
				if(!elroyStaticButOperational) 
				ChangeState(door.animationWalkIn);   //mr 18.11.2014 to enable walk in front in 
				
				autoPilotTarget = door.insideOffset + door.gameObject.transform.position;

				autoPilotDelta = (door.insideOffset - door.outsideOffset);
				
				isInAutopilot = true;
				
				direction =  transform.position.x < (autoPilotTarget.x) ? 1 : -1;
				
				ProcessDirectionAndScale(false);
				yield return new WaitForSeconds(1 / autoPilotSpeed);

				isInAutopilot = false;
				if(!elroyStaticButOperational) 
				ChangeStateToIdleBasedOnDirection(); //ChangeState(State.IdleDiagonalFront);

				//yield return new WaitForSeconds(1);

				InteractiveItem interactiveItemDoor = door.GetComponent<InteractiveItem> ();
				if (interactiveItemDoor == null || interactiveItemDoor.realyStayInCutscene != true)
					SetInCutScene(false);

				advCamera.CenterOnElroy();

				if(door.openCloseAnimator)
				{
					door.openCloseAnimator.SetBool("open", false);
				}
			}
		}

		public IEnumerator AutoPilotProcess(Vector3 dest, float speed = 1 / 1.5f, State walkstate = State.WalkSide, bool setNavmeshDestination = false, bool setInCutScene=false)
		{
			heldItem = combiningItem = null;
			targetItem = null;

			autoPilotTarget = dest;

			autoPilotDelta = (autoPilotTarget - transform.position);

			ChangeState(walkstate);

			if(setInCutScene)SetInCutScene(true);
			isInAutopilot = true;

			direction = transform.position.x < (autoPilotTarget.x) ? 1 : -1;

			ProcessDirectionAndScale(false);
			autoPilotSpeed = speed;
			yield return new WaitForSeconds(1 / autoPilotSpeed);

			isInAutopilot = false;
			if(setInCutScene)SetInCutScene(false);
			Vector3 closestPos = transform.position;
			if (setNavmeshDestination)
			{
				Vector3 destinationNavmesh = closestPos;

				SetDestination(destinationNavmesh);
			}
			ChangeState(State.IdleDiagonalFront);
			yield return null;
		}

		public IEnumerator NextLevel(Vector3 offsetCircle = default(Vector3), bool zoomout = true)
		{
			if (cutsceneTools)
				cutsceneTools.SetFadeout(true);

			if(zoomout)SetInCutScene(true, CutsceneTools.Type.ZoomOut, transform.position + (advCamera != null ? (Vector3.up * advCamera.yFollowOffset) : Vector3.zero) + offsetCircle, false, 0.0001f);

			Debug.Log("Next Level");
			audioManager.Fadeout ();

			yield return new WaitForSeconds(1.5f);
			LoadLevelInternal(SceneManager.GetActiveScene().buildIndex + 1);
		}

		public IEnumerator LoadLevel(int levelId, Vector3 offsetCircle = default(Vector3), bool zoomout = true)
		{
			if (cutsceneTools)
				cutsceneTools.SetFadeout(true);

			if (zoomout) SetInCutScene(true, CutsceneTools.Type.ZoomOut, transform.position + Vector3.up * advCamera.yFollowOffset + offsetCircle, false, 0.0001f);
		
			audioManager.Fadeout();

			yield return new WaitForSeconds(1.5f);

			LoadLevelInternal(levelId);
		}

		public IEnumerator ReloadLevel(Vector3 offsetCircle = default(Vector3))
		{
			if (cutsceneTools)
				cutsceneTools.SetFadeout(true);
			
			SetInCutScene(true, CutsceneTools.Type.ZoomOut, transform.position + Vector3.up * advCamera.yFollowOffset + offsetCircle, false, 0.0001f);
			audioManager.Fadeout ();
			
			yield return new WaitForSeconds(1.5f);

			LoadLevelInternal(SceneManager.GetActiveScene().buildIndex);
		}

		public IEnumerator LeaveRoom(DoorBetweenLevels door, State walkOption = State.WalkSide)
		{
			heldItem = combiningItem = null;
			targetItem = null;

			if(!elroyStaticButOperational)
			{
				if (door.zoomoutCircleTransformPosition != null)
				{
					Debug.Log("LeaveRoom transfrom pos" + door);
					SetInCutScene(true, CutsceneTools.Type.ZoomOut, door.zoomoutCircleTransformPosition.position, false, 0.0001f, false);
				}
				else
					SetInCutScene(true, CutsceneTools.Type.ZoomOut, transform.position + door.zoomoutCircleOffset, false, 0.0001f, false);

				autoPilotTarget = door.outsideOffset + door.gameObject.transform.position;
				autoPilotDelta = autoPilotTarget - transform.position;
				isInAutopilot = true;
				autoPilotSpeed = 1 / 1.5f;
				autoPilotSpeed = door.walkInOutSpeed;

				ChangeState(walkOption);
				direction =  transform.position.x < (autoPilotTarget.x) ? 1 : -1;
				ProcessDirectionAndScale(false);
			}
		
			Global.SetArrivalDoor(SceneManager.GetActiveScene().path);

			audioManager.Fadeout ();
			
			if(!elroyStaticButOperational)
			{
				float maxTime = 1 / autoPilotSpeed;
				float startTime = Time.time;
				float dist = 100f;
				while(true)
				{
					dist = Vector3.Distance(transform.position, door.transform.position+door.outsideOffset);
					if(dist < 0.1f) break;
					if(Time.time - startTime > maxTime) break;
					yield return new WaitForSeconds(0.02f);
				}


			}
			StartCoroutine(DisableAutopilot(true));
			yield return StartCoroutine( door.ExitTranslationAnimation() );
			scene.arrivalCave = -1;

			if(door.GetLevelIndex() == -1)
			{            
				LoadLevelInternal(0);
			}
			else
			{
				LoadLevelInternal(door.GetLevelIndex());
			}		
		}

		public IEnumerator LeaveRoomImmediate(DoorBetweenLevels door)
		{
			heldItem = combiningItem = null;
			targetItem = null;
			if (door.zoomoutCircleTransformPosition != null)
			{
				SetInCutScene(true, CutsceneTools.Type.ZoomOut, door.zoomoutCircleTransformPosition.position, false, 0.0001f);
			}
			else
				SetInCutScene(true, CutsceneTools.Type.ZoomOut, transform.position + door.zoomoutCircleOffset, false, 0.0001f);
			
			Global.SetArrivalDoor(SceneManager.GetActiveScene().path);

			audioManager.Fadeout();
			
			scene.arrivalCave = -1;

			if (door.GetLevelIndex() == -1)
			{
				LoadLevelInternal(0);
			}
			else
			{
				LoadLevelInternal(door.GetLevelIndex());
			}

			yield return null;
		}

		private void LoadLevelInternal(int level)
		{
			if (useLoadingScreen)
			{
				Global.SetState("Global level", level);
				Global.SetState("Global loadingLevel", level, true);

				SceneManager.LoadScene("loadingScreen");
			}
			else
			{
				Global.SetState("Global level", level);
				Global.SetState("Global loadingLevel", level, true);

				SceneManager.LoadScene(level);
			}
		}

		public IEnumerator LeaveCave(DoorBetweenCaves door, DoorBetweenCaves enterDoor, State walkOption = State.WalkSide, float waitFor = 0, float customAutoPilotSpeed = 1 / 1.5f)
		{
			heldItem = combiningItem = null;
			targetItem = null;

			if(!door.noWaitTime)
				SetInCutScene(true, CutsceneTools.Type.None);

			autoPilotTarget = door.outsideOffset + door.gameObject.transform.position ;
			autoPilotDelta = autoPilotTarget - transform.position;

			scene.setArivalCave(enterDoor.caveNumber);

			isInAutopilot = true;
			overrideAutoPilotScale = door.overrideAutoPilotScale;
			overridedAutoPilotScale = door.outsideScale;
			autoPilotStartedTime = Time.time;
			autoPilotStartedScale = transform.localScale;
			autoPilotSpeed = customAutoPilotSpeed;

			ChangeState(walkOption);
			direction =  transform.position.x < (autoPilotTarget.x) ? 1 : -1;
			ProcessDirectionAndScale(false);

			if (!door.isElevator)
			{

			}
			else
				StartCoroutine(DisableAutopilot(door.isElevator));

			autoPilotUseScaleVector = transform.localScale.x < 0 ? scaleUnitVectorLeft : scaleUnitVectorRight;

			if(enterDoor.isElevator) {
				enterDoor.CloseDoor();
				enterDoor.setToLevel(-1);
			}

			if(!door.noWaitTime)
			yield return new WaitForSeconds(door.isElevator ? (door.isLastElevator ? 2f : 4f) : 0.5f);
				
			navmeshWalker.enabled = false;

			if(door.isElevator && door.sfxTravelElevator)	
			{
				door.sfxTravelElevator.Play ();
				yield return new WaitForSeconds(2);
			}

			Vector3 zoomOutPos = transform.position + door.zoomoutCircleOffset;

			if (door.zoomoutCircleTransformPosition)
				zoomOutPos = door.zoomoutCircleTransformPosition.position;

			yield return new WaitForSeconds(waitFor);

			if(door.loadNextLevelOnExit)
				audioManager.Fadeout ();

			if(!door.noWaitTime)SetInCutScene(true, CutsceneTools.Type.ZoomOut, zoomOutPos, false, 0.0001f);
			if(!door.noWaitTime)yield return new WaitForSeconds(1f);

			Global.SaveState();  //end leaving

			if(enterDoor)
			enterDoor.SendMessage("Entered", SendMessageOptions.DontRequireReceiver);

			if(!door.noWaitTime)yield return new WaitForSeconds(1f);

			if(door.loadNextLevelOnExit)
			{
				if(SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCount-1)
				{
					SceneManager.LoadScene( 0 );
				}
				else
				{
					LoadLevelInternal(SceneManager.GetActiveScene().buildIndex + 1);
				}
			}
			else
			StartCoroutine(EnterCave(enterDoor));
		}

		IEnumerator DisableAutopilot(bool changeState)
		{
			Debug.Log("disableAutoPilot " + changeState);
			yield return new WaitForSeconds(1/autoPilotSpeed);
			isInAutopilot = false;
			if(changeState)
				ChangeState(State.IdleFront);
		}

		
		public IEnumerator EnterCave(DoorBetweenCaves enterDoor)
		{

			float? newMinY = null;
			float? newMaxY = null;

			if(enterDoor.cameraYEnforced)
			{
				newMinY = enterDoor.minCameraY;
				newMaxY = enterDoor.maxCameraY;
			}

			advCamera.setNewForcedCameraBoundaries(enterDoor.minCameraX, enterDoor.maxCameraX, newMinY, newMaxY);

			scene.SetNewScales(enterDoor.caveScaleMax, enterDoor.caveScaleMin, enterDoor.caveScaleMaxPos, enterDoor.CaveScaleMinPos);

			autoPilotSpeed = 1/1.5f;

			autoPilotSpeed = enterDoor.autoPilotSpeed;

			navmeshWalker.enabled = false;

			Debug.Log ("EnterCave " + enterDoor);
			
			heldItem = combiningItem = null;
			targetItem = null;

			transform.position = enterDoor.gameObject.transform.position + enterDoor.outsideOffset;

			
			enterDoor.gameObject.SendMessage("SetState", SendMessageOptions.DontRequireReceiver);
			
			autoPilotTarget = transform.position;
			
			isInAutopilot = false;

			Vector3 cameraPos = transform.position + Vector3.up*advCamera.yFollowOffset;
			
			cameraPos.z = -5;

			cameraPos = new Vector3(cameraPos.x, cameraPos.y/*advCamera.transform.position.y*/, cameraPos.z);

			advCamera.transform.position = advCamera.EnforceCameraBoundaries(cameraPos);

			transform.localScale = Vector3.one * enterDoor.outsideScale;

			if(!enterDoor.noWaitTime)
			yield return new WaitForSeconds(0.1f);

			if (enterDoor.zoomoutCircleTransformPosition != null)
				SetInCutScene (true, CutsceneTools.Type.ZoomIn, enterDoor.zoomoutCircleTransformPosition.position);
			else 
			{
				SetInCutScene (true, CutsceneTools.Type.ZoomIn, transform.position + Vector3.up * advCamera.yFollowOffset);
			}

			if(enterDoor.elevatorDoor != null && enterDoor.isElevator){
				if(enterDoor.isFront)
					transform.position += Vector3.down*0.1f;
				ChangeState(State.IdleFront);
				yield return new WaitForSeconds(2.5f);

				enterDoor.OpenDoor();

				if(enterDoor.isFront)
					yield return new WaitForSeconds(enterDoor.isElevator ? 1.5f : 0.5f);
				else
					yield return new WaitForSeconds(enterDoor.isElevator ? 1.5f : 0.8f);
			}

			ChangeState(enterDoor.animationWalkIn);
			
			autoPilotTarget = enterDoor.insideOffset + enterDoor.gameObject.transform.position ;
			
			autoPilotDelta = (autoPilotTarget-transform.position);
			
			isInAutopilot = true;
			overrideAutoPilotScale = enterDoor.overrideAutoPilotScale;
			if(overrideAutoPilotScale)
			{
				transform.localScale = Vector3.one * enterDoor.outsideScale;
				overridedAutoPilotScale = enterDoor.insideScale;
				autoPilotStartedTime = Time.time;
				autoPilotStartedScale = transform.localScale;
			}
			int d =  transform.position.x < (autoPilotTarget.x) ? 1 : -1;
			ChangeDirection(d);
			ProcessDirectionAndScale(false);
			autoPilotUseScaleVector = transform.localScale.x < 0 ? scaleUnitVectorLeft : scaleUnitVectorRight;

			float waitTime = 1 / autoPilotSpeed;
			Debug.Log ("Waiting for " + waitTime + " s, autoPilotSpeed=" + autoPilotSpeed);
			yield return new WaitForSeconds(waitTime);
			navmeshWalker.enabled = true;

			Debug.Log("disabling autopilot (entercave)");
			isInAutopilot = false;
			
			overrideAutoPilotScale = false;
			ChangeState(State.IdleDiagonalFront);
			//yield return new WaitForSeconds(1);
			if(enterDoor.GetComponent<Collider2D>())
				enterDoor.GetComponent<Collider2D>().enabled = true;

			SetInCutScene(false);

			
		}

		void ArrivedAt(InventoryItem item)
		{
			StopTalking();
			StopWalking();
			isInUninterruptibleAction = false;
			
			if(targetItem && !targetItem.ArrivedAt(item))
			{
				Debug.Log("arrived at "+item.name);
				if(targetItem.name != item.name){
					string id_name = null;//Global.ItemCombinationComment(targetItem.name, item.name);
					if (!string.IsNullOrEmpty(id_name))
						Speak(id_name);
					else
						SayThatWontWork();

				}
			}

			heldItem = null;
			combiningItem = null;

		}

		public bool IsIdleOrWalking()
		{
			foreach(var st in idleOrWalkingStates)
			{
				if(state == st) return true;

			}
			return false;
		}

		public bool IsWalking()
		{
			foreach(var st in walkingStates)
			{
				if(state == st) return true;
				
			}
			return false;
		}

		public bool IsTalking()
		{
			if(animator == null) return false;
			return animator.GetBool("talking");
		}

		bool IsIdle()
		{
			foreach(var st in idleStates)
			{
				if(state == st) return true;
				
			}
			return false;
		}

		public void TurnTowards(InteractiveItem obj)
		{
			int d =  transform.position.x < (obj.transform.position.x + obj.GetCenterOffset(heldItem).x) ? 1 : -1;
			ChangeDirection(d);
			ProcessDirectionAndScale(false);
		}

		public void TurnTowards(Vector3 v)
		{
			int d = transform.position.x < (v.x) ? 1 : -1;
			ChangeDirection(d);
			ProcessDirectionAndScale(false);
		}

		public void TurnTowards(bool left = false)
		{
			int d = left ? 1 : -1;
			ChangeDirection(d);
			Debug.Log("turning toward: " + left + " dir: " + direction);
			ProcessDirectionAndScale(false);
		}

		public void TurnTowards(int d = 1)
		{
			ChangeDirection(d);

			ProcessDirectionAndScale(false);
		}

		bool ShouldChangeDirection()
		{
			if(Time.time - lastTimeChangedDirection > 0.2f) return true;
			return false;
		}

		public void SetNavmeshWalkerEnabled(bool b)
		{
			if(navmeshWalker != null)
			navmeshWalker.enabled = b;
		}

		void ProcessDirectionAndScale(bool changeState=true)
		{
			if (elroyStaticButOperational) return;

			/*if(Time.time-setDestinationTimeStamp < 0.05f || frameCounter % 2 == 0)*/ 
			ProcessDirection(changeState);

			ProcessScale();
		}

		void ProcessDirection(bool changeState)
		{
			Vector3 vel = navmeshWalker.GetVelocity ();

			if(IsIdleOrWalking())
			{
				if(!ignoreNavMeshAgent && navmeshWalker && vel.sqrMagnitude > 0 && !elroyStaticButOperational)
				{
					if(Mathf.Abs (vel.x * 0.2f) > Mathf.Abs (vel.y)) 
					{
						if(changeState)
						{
							if(ShouldChangeDirection())
							{
								ChangeState(State.WalkSide);
								lastTimeChangedDirection = Time.time;
							}
						}

						navmeshWalker.speed = speedMultiplier * Global.scene.speedHorizontal * GetSpeedModifierBasedOnYPosition();

						if(vel.sqrMagnitude > 0)
						{
							if(!isInAutopilot)
							{
								if( vel.x > 0) ChangeDirection(1);
								else
									ChangeDirection(-1);
							}
						}

						if(vel.y <= 0)
						{
							talkDirection = TalkDirection.Diagonal;
						}
						else
						{
							talkDirection = TalkDirection.DiagonalBack;
						}

					}
					else
					{
						navmeshWalker.speed = speedMultiplier * Global.scene.speedVertical * GetSpeedModifierBasedOnYPosition();
						
						if((vel.y <= 0 && !flipYCoordinate) || (vel.y > 0 && flipYCoordinate))
						{
							if(changeState)
							{
								if(ShouldChangeDirection())
								{
									ChangeState(State.WalkFront);
									lastTimeChangedDirection = Time.time;
								}
							}
							talkDirection = TalkDirection.Front;
						}
						else
						{
							if(ShouldChangeDirection())
							{
								ChangeState(State.WalkBack);
								lastTimeChangedDirection = Time.time;
							}
							talkDirection = TalkDirection.DiagonalBack;
						}


					}
				}
				else
				{
					if(!IsIdle() || elroyStaticButOperational)
					{
						switch(state)
						{
						case State.WalkBack:
							if(changeState)ChangeState (State.IdleBack);
							talkDirection = TalkDirection.DiagonalBack;
							break;

						case State.WalkFront:
							// if (changeState) ChangeState(State.IdleDiagonalFront); //
								if(changeState)ChangeState (State.IdleFront);
								talkDirection = TalkDirection.Front;
							break;

						case State.WalkSide:
							if(changeState)ChangeState (State.IdleDiagonalFront);
							talkDirection = TalkDirection.Diagonal;
							break;

						default:
							if(changeState)ChangeState(State.IdleDiagonalFront);
							talkDirection = TalkDirection.Diagonal;
							break;
						}

						if(targetItem)
						{
							ArrivedAt(combiningItem);

							//targetItem = null;
						}

					}
				}
			}
		}

		public void setProcessScale(bool process, float defScale = -1){
			processScale = process;
			if (defScale != -1)
			{
				scale = defScale;
			}
		}

		void ProcessScale(){
		
			float direction = this.direction;
			if(processScale)
			{  //can we scale
				if(overrideAutoPilotScale){
					transform.localScale = Vector3.Lerp(autoPilotStartedScale, autoPilotUseScaleVector * overridedAutoPilotScale, (Time.time-autoPilotStartedTime) * autoPilotSpeed);
				}
				else{
					scale = 1;
					if(scene)
					{
						
					
						scale = scene.GetCharacterScale();
					}
					transform.localScale = direction < 1 ? scaleUnitVectorLeft * scale : scaleUnitVectorRight * scale;
				}
			}
			else{  //change direction
				transform.localScale = direction < 1 ? scaleUnitVectorLeft * scale : scaleUnitVectorRight * scale;
			}
		}

		public void SetTargetItem(InteractiveItem newTargetItem, bool useNull = false)
		{
			if(newTargetItem != null || useNull){
				targetItem = newTargetItem;
			//	Debug.Log("1setting target item to " + newTargetItem);
			}
		}

		void Update () 
		{
			frameCounter++;
			if(isOnlyAVoice) return;
		
			if (elroyStaticButOperational)
			{
				navmeshWalker.Stop();
			}
			
			if(!inCutScene && !Global.inPause)
			{
				if(state != lastState)
				{
					ChangeState(state);
				}

				ProcessDirectionAndScale();
				StartCoroutine(ProcessInteraction());
			}
			else if(isInAutopilot)
			{
				transform.position += autoPilotDelta * Time.deltaTime * autoPilotSpeed;//Vector3.Lerp (transform.position, autoPilotTarget, Time.deltaTime);
				ProcessScale();
			}
		}
	}
}
