using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class SetLoadingLevel : MonoBehaviour {


		public DoorBetweenLevels loadingDoor;
		// Use this for initialization
		void Start () {
			int? level = Global.GetState("Global loadingLevel");
			if(level<10)
				level = 15;

			level++;
		//	loadingDoor.levelNumber = level;
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}