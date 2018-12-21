using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	
	public class CaveSaveDoorState : StatefulItem {


		[SaveState]
		int state=0;

		Animator animator;
		public BoxCollider2D colliderTo;
		// Use this for initialization
		void Start () {
			base.Initialise();
			animator = gameObject.GetComponent<Animator>();
			if(state==1){
				if(colliderTo!=null){
					colliderTo.enabled=true;
				}
				animator.SetTrigger("openDoor");
				StartCoroutine(enableCol());
			}
			else{
		//		animator.SetTrigger("doorClose");
			}
		}

		IEnumerator enableCol(){
			yield return new WaitForSeconds(1);
			if(colliderTo!=null)
				colliderTo.enabled=true;
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void DoorClose(){
			Debug.Log("closing door");
			state = 0;
			SaveState();
		}
		public void DoorOpens(){
			state = 1;
			SaveState();
		}
	}
}