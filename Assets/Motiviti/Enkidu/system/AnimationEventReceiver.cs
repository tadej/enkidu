using UnityEngine;
using System.Collections;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	public class AnimationEventReceiver : MonoBehaviour 
	{
		public void AnimationEventEndCutScene()
		{
			if (Global.player) Global.player.AnimationEventEndCutScene ();
		}

		public void AnimationActionPoint(string animationName)
		{
			if(Global.player) Global.player.AnimationActionPoint(animationName);
		}

		public void AnimationFinished(string animationName)
		{
			if(Global.player) Global.player.AnimationFinished(animationName);
		}
	}
}
