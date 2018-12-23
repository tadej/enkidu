using PigeonCoopToolkit.Navmesh2D;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		
	public class NavmeshWalker : MonoBehaviour
	{
		private Vector3 pathingTarget;
		private List<Vector2> path;
		Vector3 velocity;
		Vector3 lastVelBeforeZero = Vector3.zero;
		Vector3 lastPosition;

		public float speed = 1;

		public LayerMask walkableMask;

		public new bool enabled = true;

		public Vector3 GetDestination()
		{
			return pathingTarget;
		}

		public bool SetTarget(Vector3 t)
		{
			if (enabled && NavMesh2D.GetNavMeshObject()) 
			{
				pathingTarget = NearestPoint(t, NavMesh2D.GetNavMeshObject().NavMesh);
				path = NavMesh2D.GetSmoothedPath (transform.position, pathingTarget);

				if (path.Count <= 0)
				{
					Vector3? cn = ClosestNode(t);

					if(cn == null) return false;

					pathingTarget = (Vector3)cn;

					path = NavMesh2D.GetSmoothedPath (transform.position, pathingTarget);
				}

				Vector2 vk = Vector2.zero;
				for(int i=0; i<path.Count; i++)  vk = path[i];
	
				if(path != null && path.Count > 0) velocity = lastVelBeforeZero;

			}

			return true;
		}

		Vector3? ClosestNode(Vector3 point)
		{
			var navmeshObject = NavMesh2D.GetNavMeshObject();
			if(navmeshObject == null) return null;

			var node = navmeshObject.ActualClosestNodeTo (new Vector2 (point.x, point.y), false);

			if (node == null) return null;

			Vector3 n = node.position;	
			
			return n;
		}

		Vector3 NearestPoint(Vector3 point, Mesh mesh)
		{
			float minDistanceSqr = Mathf.Infinity;
			Vector3 nearestVertex = Vector3.zero;
			
			// scan all vertices to find nearest
			foreach (Vector3 vertex in mesh.vertices)
			{
				Vector3 diff = point-vertex;
				float distSqr = diff.sqrMagnitude;
				
				if (distSqr < minDistanceSqr)
				{
					minDistanceSqr = distSqr;
					nearestVertex = vertex;
				}
			}
			
			// convert nearest vertex back to world space
			return nearestVertex;//transform.TransformPoint(nearestVertex);*/
		}
		
		public bool HasArrived()
		{

			if(Vector3.Distance (pathingTarget, transform.position) < 0.06f) 
			{
				return true;
			}

			return false;
		}

		public void Stop()
		{
			path = null;
			velocity = Vector3.zero;
		}

		public bool IsMoving()
		{
			if (path != null && path.Count != 0) return true;
			return false;
		}

		void Start()
		{
			walkableMask = LayerMask.GetMask (new string[] {"Walkarea"} );
			lastPosition = transform.position;
			velocity = Vector3.zero;
		}

		void OnEnable()
		{
			StartCoroutine(VelocityTimer());
		}
		
		public Vector3 GetVelocity()
		{
			return velocity;
		}
		
		IEnumerator VelocityTimer()
		{
			while(true)
			{
				velocity = transform.position - lastPosition;
				lastPosition = transform.position;

				if(velocity.sqrMagnitude <= 0)
				{
					if(path == null || path.Count > 0) velocity = lastVelBeforeZero;
				}
				else
				{
					lastVelBeforeZero = velocity;
				}

				yield return new WaitForSeconds(0.01f);
			}
		}
			
		// LateUpdate is called once per frame
		void Update () 
		{
			if(path != null && path.Count != 0)
			{
				transform.position = Vector2.MoveTowards(transform.position, path[0], 3*speed*Time.deltaTime);
				if(Vector2.Distance(transform.position, path[0]) < 0.01f)
				{
					path.RemoveAt(0);
				}
			}
			else
			{
				lastVelBeforeZero = Vector3.zero;
			}
		}
	}
}