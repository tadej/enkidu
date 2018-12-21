using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class CaveOpenElevator : InteractiveItemAction {


		[SaveState]
		int state=0;

		public Animator animator;
		public BoxCollider2D boxCollider;
		public DoorBetweenCaves door;

		// Use this for initialization
		void Start () {
			base.Initialise();
			if(state==1){
				StartCoroutine(onStart());
			}
		}

		IEnumerator onStart(){
			yield return new WaitForSeconds(0.5f);
			this.GetComponent<Collider2D>().enabled = false;
			boxCollider.enabled = true;
			door.OpenDoor(false);
		}
		
		// Update is called once per frame
		new void Update () {
		
		}
		
		public void SwitchOn(){
			Debug.Log("switch on in cave elevator door panel");
			if(state != 1){
				state = 1;
				door.OpenDoor();
				StartCoroutine(enableCollider());
			}
		}

		public void SetState(){
			state = 1;
		}

		IEnumerator enableCollider(){
			if(door.isFront)
				yield return new WaitForSeconds(1f);
			else
				yield return new WaitForSeconds(1f);
			boxCollider.enabled = true;
		}
	}
}