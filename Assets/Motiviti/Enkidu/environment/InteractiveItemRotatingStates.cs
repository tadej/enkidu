using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	
		public class InteractiveItemRotatingStates: InteractiveItemAction
	{

			[SaveState]
			public int
					state = 0;
			[SaveState]
			public bool
					isLocked = false;
			[SaveState]
			public bool
					onlyOnce = false;
			[SaveState]
			bool
					switchedFlag = false;
			public Renderer[] states;
			[SaveState]
			int
					progressCounter = 0;
			public string lockedComment;

			// Use this for initialization
			void Start ()
			{
					base.Initialise ();

					ProcessState ();
			}

			public void ProcessOnlyOnce (bool b)
			{
					onlyOnce = b;

					if (onlyOnce && state != 0) {
							switchedFlag = true;
					}
					SaveState ();
			}

			// Update is called once per frame
			new void Update ()
			{
					base.Update ();
			}

			public void Unlock ()
			{
					Debug.Log ("unlock");
					isLocked = false;
					SaveState ();
			}

			bool Switch ()
			{
					state++;

					if (state > states.Length - 1)
							state = 0;

					switchedFlag = true;

					Global.SetState (gameObject.name + "_RotatingState", state);

					ProcessState ();

					return true;
			}

			void ProcessState ()
			{
					for (int i=0; i<states.Length; i++) {
							if (i == state)
									states [i].GetComponent<Renderer>().enabled = true;
							else
									states [i].GetComponent<Renderer>().enabled = false;
					}

					SaveState ();
			}

			public override void AnimationActionPoint (string animationName)
			{
					progressCounter = 1;
			}
		
			public override void AnimationFinished (string animationName)
			{
					progressCounter = 2;
			}

			public override IEnumerator ProcessArrivedAt ()
			{
					progressCounter = 0;

					Global.player.TurnTowards (interactiveItem);

					float time0 = Time.time;

					if (onlyOnce && switchedFlag) {

					} else {
							Global.player.ChangeState (actionAnimation);

							while (progressCounter == 0) {
									yield return new WaitForSeconds (0.05f);

									if (Time.time - time0 > Global.maxCharacterAnimationLength) {
											Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
											break;
									}
							}

							if (isLocked) {

							} else {
									Switch ();
							}

							while (progressCounter == 1) {
									yield return new WaitForSeconds (0.05f);

									if (Time.time - time0 > Global.maxCharacterAnimationLength) {
											Debug.Log ("Warning: ProcessArrivedAt interrupted, Time.time-time0 > maxCharacterAnimationLength");
											break;
									}
							}

							//yield return new WaitForSeconds(0.4f);

							Global.player.ChangeState (Player.State.IdleDiagonalFront);

							//yield return new WaitForSeconds(0.3f);
							if (isLocked) {
									yield return new WaitForSeconds (0.2f);

									Global.player.SayItsLocked (lockedComment);
							}


					}
			}
	}
}