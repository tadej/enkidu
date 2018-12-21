using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class Shaking : MonoBehaviour {

		public float amplitude = 0.2f;

		public Vector3 originalPosition;

		// Use this for initialization
		void Start () {
			originalPosition = transform.position;
		}
		
		// Update is called once per frame
		void Update () {

			if(!Global.inPause){
				Vector3 newPos = originalPosition + Vector3.one * (Random.value-0.5f)*amplitude;
				newPos.z = 0;
				transform.position = newPos;
			}
			//amplitude = Mathf.Lerp (amplitude, 0, Time.deltaTime);
		}
	}
}