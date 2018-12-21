using PigeonCoopToolkit.Navmesh2D;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PathFollower : MonoBehaviour
{
	public Transform pathingTarget;
    private List<Vector2> path;




	// LateUpdate is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.E))
	    {
	        path = NavMesh2D.GetSmoothedPath(transform.position, pathingTarget.position);
	    }

        if(path != null && path.Count != 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, path[0], 5*Time.deltaTime);
            if(Vector2.Distance(transform.position,path[0]) < 0.01f)
            {
                path.RemoveAt(0);
            }
        }
	}
}
