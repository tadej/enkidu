using UnityEngine;
using System.Collections.Generic;



using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	[RequireComponent(typeof(CircleCollider2D))]
	public class PointForceComponent : MonoBehaviour 
	{
		public bool NonLinear = true;
		public float Force = 0f;
		public float LinearDrag = 0f;
		public float AngularDrag = 0f;
		public GameObject TrackedObject;
		private CircleCollider2D myCircleCollider;
		private List<Collider2D> objects = new List<Collider2D>();
		private float FLT_EPSILON = 1.19209290E-07F;
		
		// Use this for initialization
		void Start () 
		{
			myCircleCollider = (CircleCollider2D)GetComponent("CircleCollider2D");
		}
		
		// This function is called every fixed framerate frame
		void FixedUpdate() 
		{
			float radiusSqr = myCircleCollider.radius * myCircleCollider.radius;
			
			// Calculate the force squared in-case we need it.
			float forceSqr = Force * Force * (( Force < 0.0f ) ? -1.0f : 1.0f);
			
			// Calculate drag coefficients (time-integrated).
			for(int i = 0; i < objects.Count; i++)
			{
				// Fetch the object rigid body
				Rigidbody2D body = objects[i].attachedRigidbody;
				
				// Ignore if it's the object we're tracking
				if (body.gameObject == TrackedObject)
					continue;
				
				// Calculate the force distance to the controllers current position.
				Vector2 distanceForce = transform.position - body.transform.position;
				
				// Fetch distance squared.
				float distanceSqr = distanceForce.x * distanceForce.x + distanceForce.y * distanceForce.y;
				
				// Skip if the position is outside the radius or is centered on the controller.
				if ( distanceSqr > radiusSqr || distanceSqr < FLT_EPSILON )
					continue;
				
				// Non-Linear force?
				if (NonLinear)
				{
					// Yes, so use an approximation of the inverse-square law.
					distanceForce *= (1.0f / distanceSqr) * forceSqr;
				}
				else
				{
					// No, so normalize to the specified force (linear).
					distanceForce = Vector2.ClampMagnitude(distanceForce, Force);
				}
				
				// Apply the force
				body.AddForce(distanceForce);
				
				// Linear drag?
				if (LinearDrag > 0.0f)
				{
					// Yes, so fetch linear velocity.
					Vector2 linearVelocity = body.velocity;
					
					// Calculate linear velocity change.
					Vector2 linearVelocityDelta = linearVelocity * LinearDrag;
					
					// Set linear velocity.
					body.velocity = new Vector2(linearVelocity.x - linearVelocityDelta.x, linearVelocity.y - linearVelocityDelta.y);
				}
				
				// Angular drag?
				if (AngularDrag > 0.0f)
				{
					// Yes, so fetch angular velocity.
					float angularVelocity = body.angularVelocity;
					
					// Calculate angular velocity change.
					float angularVelocityDelta = angularVelocity * AngularDrag;
					
					// Set angular velocity.
					body.angularVelocity = angularVelocity - angularVelocityDelta;
				}
			}
		}
		
		void OnTriggerEnter2D(Collider2D other)
		{
			objects.Add(other);
		}
		
		void OnTriggerExit2D(Collider2D other)
		{
			objects.Remove(other);
		}
	}
}