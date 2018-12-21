using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class InteractiveItemSwitch : InteractiveItemAction {

		public enum State
		{
			OFF,
			ON,
			ANY
		}

		public SpriteRenderer highlightOverlayOn = null;

		public bool contributesToCanBeUsedFlag = true;

		[SaveState]
		public State state = State.OFF;

		[SaveState]
		public bool isLocked = false;

		[SaveState]
		public bool onlyOnce = false;

		[SaveState]
		bool switchedFlag = false;

		public bool ignoreArrivedAt = false;

		public Renderer stateOn, stateOff;

		public InteractiveItemSwitch [] connectedItemSwitch;
		public Transform[] connectedItemEnableDisable;
		public Transform[] connectedItemEnableDisableInverse;

		public Player.State actionAnimationOpen = Player.State.PickUpSide, actionAnimationClose = Player.State.PickUpSide;

		public bool switchOffWhenConnectedEnabled=false;

		public AudioSource sfxUnlock, sfxOpen, sfxClose, sfxTryButlocked;

		public string lockedComment = "";

		public bool isGlobal = false;

		[SaveState]
		int progressCounter = 0;

		public override bool ShouldSkip()
		{
			return (onlyOnce && switchedFlag);
		}

		// Use this for initialization
		IEnumerator Start () {
			if(isGlobal)
				base.InitialiseGlobal();
			else
			base.Initialise();

			if(connectedItemEnableDisable != null)
			foreach(var ci in connectedItemEnableDisable)
			{
				if(ci.GetComponent<InteractiveItem>() != null)
				{
					ci.gameObject.SendMessage ( state == State.ON ? "ItemActivate" : "ItemDeactivate");
				}
				else
				{
					if(ci.gameObject.GetComponent<Renderer>() != null)
					{
						ci.gameObject.GetComponent<Renderer>().enabled = state == State.ON;
					}

					if(ci.gameObject.GetComponent<Collider>() != null)
					{
						ci.gameObject.GetComponent<Collider>().enabled = state == State.ON;
					}
				}
			}
			
			if(connectedItemEnableDisableInverse != null)
			foreach (var ci in connectedItemEnableDisableInverse)
			{
				if (ci.GetComponent<InteractiveItem>() != null)
				{
					ci.gameObject.SendMessage(state == State.OFF ? "ItemActivate" : "ItemDeactivate");
				}
				else
				{
					if (ci.gameObject.GetComponent<Renderer>() != null)
					{
						ci.gameObject.GetComponent<Renderer>().enabled = state == State.OFF;
					}

					if (ci.gameObject.GetComponent<Collider>() != null)
					{
						ci.gameObject.GetComponent<Collider>().enabled = state == State.OFF;
					}
				}
			}

			ProcessState();

			yield return null;
		}

		public void ProcessOnlyOnce(bool b)
		{
			onlyOnce = b;

			if(onlyOnce && state == State.ON)
			{
				switchedFlag = true;
			}

		//  SaveState();
		}

		// Update is called once per frame
		new void Update () {
			base.Update();
		}

		protected override void HightlightFrameUpdate()
		{
			highlightAmp = Mathf.Lerp(highlightAmp, 0, Time.deltaTime * 1.5f);

			if(highlighting)
			{
				if (state == State.OFF)
				{
					if (highlightOverlay)
					{
						highlightOverlay.color = new Color(1, 1, 1, (Mathf.Sin(Time.time * 20) + 1) * 0.5f * highlightAmp);
					}
				}
				else if (state == State.ON)
				{
					if (highlightOverlayOn)
					{
						highlightOverlayOn.color = new Color(1, 1, 1, (Mathf.Sin(Time.time * 20) + 1) * 0.5f * highlightAmp);
					}
				}
			}
			else
			{
				if(highlightOverlay) highlightOverlay.color = new Color(1, 1, 1, 0);
				if (highlightOverlayOn) highlightOverlayOn.color = new Color(1, 1, 1, 0);
			}
		}

		public bool ValidateSwitchState(State rule)
		{
			Debug.Log("validating " + rule + " ?? " + state);
			switch(rule)
			{
			case State.ANY:
				return true;
				
			case State.ON:
				return (state == State.ON);

			case State.OFF:
				return (state == State.OFF);
			}

			return false;
		}

		public void Unlock()
		{
			isLocked = false;

			if (sfxUnlock)	sfxUnlock.Play ();

			SaveState();
		}

		public void SwitchOn()
		{
			if(state == State.OFF) Switch();
		}

		public void SwitchOff()
		{
			if(state == State.ON) Switch ();

		}

		bool Switch()
		{
			switchedFlag = true;

			Debug.Log("switching " + gameObject.name + ", which is currently " +  state + " time: " + Time.time);
			switch(state)
			{
			case State.OFF:
				state = State.ON;
				if(sfxOpen)sfxOpen.Play ();
				SendMessage ("SwitchedOn", SendMessageOptions.DontRequireReceiver);

				break;
				
			case State.ON:
				state = State.OFF;
				if(sfxClose)sfxClose.Play ();
				SendMessage ("SwitchedOff", SendMessageOptions.DontRequireReceiver);

				break;
			}



			foreach(var ci1 in connectedItemSwitch) ci1.Switch();

			if (connectedItemEnableDisable != null)
			foreach(var ci in connectedItemEnableDisable)
			{
				if(ci.GetComponent<InteractiveItem>() != null)
				{
					if(switchOffWhenConnectedEnabled)
						ci.SendMessage("SwitchOff", SendMessageOptions.DontRequireReceiver); // in case this is a switchable

					ci.gameObject.SendMessage ( state == State.ON ? "ItemActivate" : "ItemDeactivate");
				}
				else
				{
					if(ci.gameObject.GetComponent<Renderer>() != null)
					{
						ci.gameObject.GetComponent<Renderer>().enabled = state == State.ON;
					}
					
					if(ci.gameObject.GetComponent<Collider>() != null)
					{
						ci.gameObject.GetComponent<Collider>().enabled = state == State.ON;
					}
				}


			}

			if(connectedItemEnableDisableInverse != null)
			foreach (var ci in connectedItemEnableDisableInverse)
			{
				if (ci.GetComponent<InteractiveItem>() != null)
				{
					ci.gameObject.SendMessage(state == State.OFF ? "ItemActivate" : "ItemDeactivate");
				}
				else
				{
					if (ci.gameObject.GetComponent<Renderer>() != null)
					{
						ci.gameObject.GetComponent<Renderer>().enabled = state == State.OFF;
					}

					if (ci.gameObject.GetComponent<Collider>() != null)
					{
						ci.gameObject.GetComponent<Collider>().enabled = state == State.OFF;
					}
				}
			}

			ProcessState();

			return true;
		}

		void ProcessState()
		{
			switch(state)
			{
			case State.ANY:
			case State.OFF:
				if(stateOn)stateOn.enabled = false;
				if(stateOff)stateOff.enabled = true;

				
				break;
				
			case State.ON:
				if(stateOn)stateOn.enabled = true;
				if(stateOff)stateOff.enabled = false;

				break;
			}

			//SaveState();
		}

		public override void AnimationActionPoint(string animationName)
		{
			progressCounter = 1;
			SaveState();
		}
		
		public override void AnimationFinished(string animationName)
		{
			progressCounter = 2;
			SaveState();
		}

		public override IEnumerator ProcessArrivedAt()
		{
			if(!ignoreArrivedAt)
			{

				progressCounter = 0;


				if (interactiveItem.desiredDirection != 0)
					Global.player.TurnTowards(interactiveItem.desiredDirection);
				else
					Global.player.TurnTowards(interactiveItem);

				float time0 = Time.time;

				if(onlyOnce && switchedFlag) 
				{

				}
				else
				{
					if(state != State.ON)
					{
						Global.player.ChangeState( actionAnimationOpen );
					}
					else
					{
						Global.player.ChangeState( actionAnimationClose );
					}

					while(progressCounter == 0)
					{
						yield return new WaitForSeconds(0.05f);

						if(Time.time - time0 > Global.maxCharacterAnimationLength) 
						{
							Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
							break;
						}
					}

					if(isLocked)
					{
						if(sfxTryButlocked)sfxTryButlocked.Play ();
					}
					else
					{
						Switch ();
					}

					while(progressCounter == 1)
					{
						yield return new WaitForSeconds(0.05f);

						if(Time.time - time0 > Global.maxCharacterAnimationLength) 
						{
							Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
							break;
						}
					}

					//yield return new WaitForSeconds(0.4f);

					Global.player.ChangeState( endState );

					//yield return new WaitForSeconds(0.3f);
					if(isLocked && state != State.ON)
					{
						yield return new WaitForSeconds(0.2f);

						Global.player.SayItsLocked(lockedComment);
					}


				}
				SaveState();
			}
		}
	}
}