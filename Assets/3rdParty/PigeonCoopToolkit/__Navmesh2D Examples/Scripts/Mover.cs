using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour {

	
	
	// LateUpdate is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.W))
            transform.position = transform.position + Vector3.up * 5 * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            transform.position = transform.position - Vector3.up * 5 * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            transform.position = transform.position + Vector3.right * 5 * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
            transform.position = transform.position - Vector3.right * 5 * Time.deltaTime;
	}
}
