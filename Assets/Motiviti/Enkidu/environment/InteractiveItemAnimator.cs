using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class InteractiveItemAnimator : InteractiveItemAction {

		public bool useThisGameobjectAnimator = true;

		public Animator animator;

		public int animatorState;

		int progressCounter = 0;

		public GameObject sendMessageTo;

		public string message;

		// Use this for initialization
		void Start () {
			base.Initialise();
			//	back_shade.renderer.sortingOrder=1;
			//	back_shade.renderer.sortingLayerName="Minesweeper";
			
			if(useThisGameobjectAnimator)
				animator = gameObject.GetComponent<Animator>();
		}
		
		// Update is called once per frame
		new void Update () {
			base.Update();
		}

		public override void AnimationActionPoint(string animationName)
		{
			Debug.Log("animation action point");
			progressCounter = 1;
		}
		
		public override void AnimationFinished(string animationName)
		{
			Debug.Log("animation finish point");
			progressCounter = 2;
		}

		public override IEnumerator ProcessArrivedAt()
		{
			progressCounter = 0;

			Global.player.TurnTowards(interactiveItem);

			Global.player.ChangeState( actionAnimation );
			
			while(progressCounter == 0)
			{
				yield return new WaitForSeconds(0.05f);
			}

			Debug.Log("animation set to 1 point");
			animator.SetInteger("state", animatorState);
			
			while(progressCounter == 1)
			{
				yield return new WaitForSeconds(0.05f);
			}

			//yield return new WaitForSeconds(0.4f);

			Global.player.ChangeState( endState );
		}

		public void AnimatorActionPoint(){
			animator.SetInteger("state", 0);
			sendMessageTo.SendMessage(message);
		}
	}
}