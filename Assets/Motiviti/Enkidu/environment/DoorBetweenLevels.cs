using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class DoorBetweenLevels : InteractiveItemAction {

		public string levelPath = "";

		InteractiveItemSwitch connectedSwitch;
		public InteractiveItemSwitch.State enableOnlyForSwitchState = InteractiveItemSwitch.State.ON;

		public Vector3 insideOffset = Vector3.zero, outsideOffset = Vector3.zero;

		public Vector3 zoomoutCircleOffset = Vector3.zero;

		public Transform zoomoutCircleTransformPosition;

		public bool allowDirectAccess = false;

		int index = 0;

		public AudioSource exitAudio;

		public float pauseAfterGoingThrough = 0;

		public string animationBoolTrueAfterGoingThrough = null;

		public Player.State animationWalkOut = Player.State.WalkSide;

		public Player.State animationWalkIn = Player.State.WalkSide;

		public Animator openCloseAnimator;

		public Transform characterParentOnLeaveRoom;

		public bool isGlobal = false;

		public float walkInOutSpeed = 1 / 1.5f;

		// Use this for initialization
		void Awake () 
		{
			
			if(isGlobal)base.InitialiseGlobal(); else base.Initialise();
			
			connectedSwitch = gameObject.GetComponent<InteractiveItemSwitch>();
			if(connectedSwitch != null)
				connectedSwitch.ProcessOnlyOnce(true);
		}

		public void SetTargetPath(string p)
		{
			if(!Application.isEditor) return;

			Debug.Log("Setting target path to [" + p + "]");

			if(!string.IsNullOrEmpty(p))
			{		
				levelPath = p;
				Debug.Log("Saved " + p);
			}
		}


		public InteractiveItem GetInteractiveItem()
		{
			return interactiveItem;
		}

		// Update is called once per frame
		new void Update () {
			
		}

		public override bool SupportsDoubleClick()
		{
			return true;
		}

		public override void DoubleClicked()
		{
			bool advance = connectedSwitch == null || connectedSwitch.ValidateSwitchState(enableOnlyForSwitchState);

			if(advance)
			StartCoroutine(Global.player.LeaveRoomImmediate(this));
		}

		void OnDrawGizmosSelected() {
			if(!interactiveItem) interactiveItem = GetComponent<InteractiveItem>();
			Gizmos.DrawIcon(interactiveItem.gameObject.transform.position + outsideOffset, "gizmo-caveoutside.psd", true);
			Gizmos.DrawIcon(interactiveItem.gameObject.transform.position + insideOffset, "gizmo-caveinside.psd", true);
		}

		public int GetLevelIndex(){

			if(!string.IsNullOrEmpty(levelPath))
			{
				int j = levelPath.LastIndexOf('/');

				string str = levelPath;

				if(j != -1) str = str.Substring(j+1);

				str = str.Replace(".unity", "");

	//			Debug.LogWarning("Level name:" + levelPath);

				int i = SceneUtility.GetBuildIndexByScenePath(levelPath);

	//			Debug.LogWarning("Level build index: " + i);
				return i;
			}

			return -1;
		}
		
		public override IEnumerator ProcessArrivedAt()
		{
			bool advance = connectedSwitch == null || connectedSwitch.ValidateSwitchState(enableOnlyForSwitchState);

			Debug.Log("ProcessArrivedAt " + gameObject.name + " " + advance + " " + index + " " + allowDirectAccess);

			if(!advance)
			{
				yield return null;
			}
			else
			{
				if(index >= 1 || allowDirectAccess) // make sure we've been at the open door and are clicking it again (we don't want the hero to exit immediately upon opening the door)
				{
					Debug.Log ("Leaving room ");

					if(exitAudio) exitAudio.Play ();

					StartCoroutine(ExitProcess());
				}

				index++;
			}
		}

		IEnumerator ExitProcess()
		{
			if(openCloseAnimator != null)
			{
				if(!openCloseAnimator.GetBool("open"))
				{
					openCloseAnimator.SetBool("open", true);
					yield return new WaitForSeconds(0.55f);

					
				}
				
			}
			StartCoroutine(Global.player.LeaveRoom(this, animationWalkOut));

			yield return null;
		}

		public IEnumerator ExitTranslationAnimation()
		{
			if(openCloseAnimator)openCloseAnimator.SetBool("open", false);
			
			if(openCloseAnimator)
			if(!string.IsNullOrEmpty(animationBoolTrueAfterGoingThrough))
			{
				openCloseAnimator.SetBool(animationBoolTrueAfterGoingThrough, true);
				if(characterParentOnLeaveRoom)
				{
					Global.player.transform.parent = characterParentOnLeaveRoom;

					Global.player.transform.localPosition = Vector3.zero;

					Global.player.elroyStaticButOperational = true;
				}
			}
			yield return new WaitForSeconds(pauseAfterGoingThrough);
		}
	}
}