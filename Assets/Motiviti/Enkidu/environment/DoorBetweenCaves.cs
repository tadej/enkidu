using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class DoorBetweenCaves : InteractiveItemAction {
		public string levelName = "";
		
		InteractiveItemSwitch connectedSwitch;
		public InteractiveItemSwitch.State enableOnlyForSwitchState = InteractiveItemSwitch.State.ON;
		
		public Vector3 insideOffset = Vector3.zero, outsideOffset = Vector3.zero;

		public float autoPilotSpeed = 1/ 1.5f;

		public float outsideScale = 1;

		public float insideScale = 1;

		public float caveScaleMax = 1.1f, caveScaleMin = 0.9f, caveScaleMaxPos = -2, CaveScaleMinPos = -1;

		public bool overrideAutoPilotScale = false;

		public bool noWaitTime = false;
		
		public Vector3 zoomoutCircleOffset = Vector3.zero;

		public Transform zoomoutCircleTransformPosition;
		
		public bool allowDirectAccess = false;
		
		int index = 0;

		public int caveNumber = 0;
		
		public AudioSource exitAudio;

		public Player.State animationWalkOut = Player.State.WalkSide;

		public Player.State animationWalkIn = Player.State.WalkSide;

		public DoorBetweenCaves enterDoor;

		public Animator elevatorDoor;

		public int toWhichLevel = 0;

		public float addedWaitFor = 0;

		public float waitForDoorToClose = 0.5f;

		public float waitBeforeDisableAutoPilot = 0.5f;

		public float minCameraX;

		public float maxCameraX;

		public bool cameraYEnforced = false;
		public float minCameraY;
		public float maxCameraY;

		public bool isFront = false;

		public bool disableAutopilot = false;

		public bool isElevator = false;

		public bool isLastElevator = false;

		public bool loadNextLevelOnExit = false;

		public bool UseEnterCaveProcedure = true;

		public AudioSource sfxOpenDoor, sfxCloseDoor, sfxOpenElevator, sfxCloseElevator, sfxTravelElevator;


		// Use this for initialization
		void Start () 
		{
			base.Initialise();
			
			connectedSwitch = gameObject.GetComponent<InteractiveItemSwitch>();
			if(connectedSwitch != null)
				connectedSwitch.ProcessOnlyOnce(true);
		}

		void OnDrawGizmosSelected() {
			if(!interactiveItem) interactiveItem = GetComponent<InteractiveItem>();
			Gizmos.DrawIcon(interactiveItem.gameObject.transform.position + outsideOffset, "gizmo-caveoutside.psd", true);
			Gizmos.DrawIcon(interactiveItem.gameObject.transform.position + insideOffset, "gizmo-caveinside.psd", true);
		}
		
		public InteractiveItem GetInteractiveItem()
		{
			return interactiveItem;
		}
		
		// Update is called once per frame
		new void Update () {
			
		}
		
		public override IEnumerator ProcessArrivedAt()
		{
			if(connectedSwitch != null && !connectedSwitch.ValidateSwitchState(enableOnlyForSwitchState))
			{
				yield return null;
			}
			else
			{
				if(index >= 1 || allowDirectAccess) // make sure we've been at the open door and are clicking it again (we don't want the hero to exit immediately upon opening the door)
				{
					Debug.Log ("Leaving room ");
					
					if(exitAudio) exitAudio.Play ();
					
					StartCoroutine(Global.player.LeaveCave(this, enterDoor, animationWalkOut, addedWaitFor, autoPilotSpeed));

					if(elevatorDoor!=null && isElevator){
						yield return new WaitForSeconds(waitForDoorToClose);
						CloseDoor();
						elevatorDoor.SetInteger("toLevel", toWhichLevel);
					}
				}
				
				index++;
			}
		}

		public void setToLevel(int level){
			if(elevatorDoor != null) elevatorDoor.SetInteger("toLevel", level);
		}

		public void OpenDoor(bool playSound = true){
			if (elevatorDoor == null) {
				Debug.Log ("no elevator door");	
				return;
			}

			Debug.Log("opening elevator                                               " + elevatorDoor.gameObject.name);

			elevatorDoor.SetTrigger(isElevator ? "openElevator" : "openDoor");

			elevatorDoor.ResetTrigger(isElevator ? "closeElevator" : "doorClose");

			if(isElevator)
			{
				elevatorDoor.SetInteger("state", 1);
				if(sfxOpenElevator && playSound) sfxOpenElevator.Play ();
			}
			else
			{
				if(sfxOpenDoor && playSound) sfxOpenDoor.Play ();
			}
		}
		
		public void CloseDoor(){
			if (elevatorDoor == null) {
				Debug.Log ("no elevator door");	
				return;
			}
			Debug.Log("closing elevator                                               " + elevatorDoor.gameObject.name);
			elevatorDoor.SetTrigger(isElevator ? "closeElevator" : "doorClose");
			elevatorDoor.ResetTrigger(isElevator ? "openElevator" : "openDoor");
			if(isElevator)
			{
				elevatorDoor.SetInteger("state", 2);
				if(sfxCloseElevator) sfxCloseElevator.Play ();
			}
			else
			{
				if(sfxCloseDoor) sfxCloseDoor.Play ();
			}
		}
	}
}