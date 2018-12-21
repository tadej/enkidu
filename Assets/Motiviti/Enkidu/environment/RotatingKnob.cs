using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
		public class RotatingKnob : MonoBehaviour {

		public bool snap = true, limit = true, disableWhenTargetReached = true;
		public Orientation orientation;

		public bool preventNegativeDegrees = false;
		public float upperLimit = 0;

		public bool isSnappedAndFinished = false;

		public delegate void TargetReached();
		public  event TargetReached OnTargetReached;

		public delegate void Rotated( float d );
		public  event Rotated OnRotated;

		public delegate void Snapped( float d );
		public  event Snapped OnSnapped;

		public delegate void FullRotation (int d);
		public event FullRotation OnFullRotation;

		public float[] snapAngles;
		public float snapThreshold=30;

		public int fullRotations=0;

		public int lastSnapIndex = -1;

		public enum Orientation
		{
			ClockWise = -1,
			CounterClockWise = 1
		}

		float startAngle=355, targetAngle=120;

		public float angleOffset = -180;

		public bool isEnabled = true;

		public Transform gameObjectToRotate;

		new Collider2D collider;
		Vector3 startPos = Vector3.zero, startedRotation = Vector3.zero, currentRot = Vector3.zero;
		float lastAngle = 0;
		bool sliding = false;
		bool movingFrontButton = false;

		float frLastAngle=0;
		float frAngle = 0;
		public float totalDegreesTurned = 0;

		// Use this for initialization
		void Start () {
			if (gameObjectToRotate == null)
				gameObjectToRotate = transform;
			frLastAngle = gameObjectToRotate.localEulerAngles.z;

			collider = GetComponent<Collider2D>();
			if(gameObjectToRotate == null) gameObjectToRotate = transform;

			if(snapAngles.Length >= 2)
			{
				startAngle = snapAngles[0];
				targetAngle = snapAngles[ snapAngles.Length-1 ];
			}

			SnapToAngle(startAngle);

		}

		void SnapToAngle(float angle)
		{
			gameObjectToRotate.transform.localEulerAngles = new Vector3(gameObjectToRotate.transform.localEulerAngles.x, gameObjectToRotate.transform.localEulerAngles.y, angle);
			if(OnSnapped != null) OnSnapped(angle);
		}

		void EnforceLimit()
		{
			float c = gameObjectToRotate.transform.localEulerAngles.z;

			float ds = Mathf.DeltaAngle(c, startAngle);

			float dt = Mathf.DeltaAngle (c, targetAngle);

			if(orientation == Orientation.CounterClockWise)
			{
				if(ds > 0) c = startAngle;
				if(dt < 0) c = targetAngle;
			}
			else
			if(orientation == Orientation.ClockWise)
			{
				if(ds < 0) c = startAngle;
				if(dt > 0) c = targetAngle;
			}

			SnapToAngle (c);
		}

		void MoveFrontButton(Vector3 start, Vector3 current)
		{
			Vector3 ea = gameObjectToRotate.transform.localEulerAngles;

			gameObjectToRotate.transform.LookAt(current, Vector3.forward);
			gameObjectToRotate.transform.Rotate(90+angleOffset, 0, 0);
			gameObjectToRotate.transform.localEulerAngles -= startedRotation;

			if(limit) EnforceLimit();
			AnalyzePositionAndSendEvents();
			
			float da = -Mathf.DeltaAngle( gameObjectToRotate.transform.localEulerAngles.z,  lastAngle );

			frAngle = transform.localEulerAngles.z;//Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;

			float oldTotalDegreesTurned = totalDegreesTurned;

			if(orientation == Orientation.CounterClockWise)
				totalDegreesTurned += Mathf.DeltaAngle(frLastAngle, frAngle);
			else
				totalDegreesTurned -= Mathf.DeltaAngle(frLastAngle, frAngle);

			if((totalDegreesTurned < 0 && preventNegativeDegrees) || (upperLimit>0 && totalDegreesTurned>upperLimit))
			{
				gameObjectToRotate.transform.localEulerAngles = ea;
				totalDegreesTurned = oldTotalDegreesTurned;
			}
			else
			{
				if(OnRotated != null)
					OnRotated(da);

				float thresholdRotation1 = (float)(fullRotations+1) * 360f;
				float thresholdRotation2 = (float)(fullRotations-1) * 360f;

				if(totalDegreesTurned >= thresholdRotation1) 
				{
					fullRotations++;
					if(OnFullRotation != null)OnFullRotation(fullRotations);
				}
				if(totalDegreesTurned <= thresholdRotation2) 
				{
					fullRotations--;
					if(OnFullRotation != null)OnFullRotation(fullRotations);
				}

				frLastAngle = frAngle;
			}
			da *= Mathf.Deg2Rad;

			/*
			if(!MoveMap(da * 0.16f, 0))
			{
				gameObjectToRotate.transform.localEulerAngles = ea;
			}
			else
			{
				lastAngle = gameObjectToRotate.transform.localEulerAngles.z;
			}
			*/

		}

		void AnalyzePositionAndSendEvents()
		{
			float c = gameObjectToRotate.transform.localEulerAngles.z;

			if(Mathf.Abs ( Mathf.DeltaAngle(c, targetAngle)) < 0.5f)
			{
	//			Debug.Log ("Target Reached");
				if(disableWhenTargetReached) 
				{
					isEnabled = false;
					isSnappedAndFinished = true;
				}

				if(OnTargetReached != null) OnTargetReached();
			}


		}
		
		// Update is called once per frame
		void Update()
		{
			if (!isEnabled)
				return;
			
			bool firstClick = false;
			bool stopMoving = false;
			Vector3 holdPosition = Vector3.zero;
			
			foreach (Touch touch in Input.touches)
			{
				if (touch.phase == TouchPhase.Began)
				{
					firstClick = true;
					holdPosition = touch.position;
				}
				else if (touch.phase == TouchPhase.Moved)
				{
					holdPosition = touch.position;
				}
				else if (touch.phase == TouchPhase.Ended)
				{
					if (sliding)
					{
						sliding = false;
						
						stopMoving = true;
					}
				}
			}
			
			if (Input.GetMouseButtonDown(0))
			{
				firstClick = true;
				holdPosition = Input.mousePosition;
			}
			else if (Input.GetMouseButton(0))
			{
				holdPosition = Input.mousePosition;
			}
			else if (Input.GetMouseButtonUp(0) || (sliding && !stopMoving && !Input.GetMouseButton(0)))
			{
				if (sliding)
				{
					sliding = false;
					stopMoving = true;
				}
			}
			
			if (firstClick)
			{
				holdPosition = Camera.main.ScreenToWorldPoint(holdPosition);
				startPos = holdPosition;
				Collider2D[] col = Physics2D.OverlapPointAll(holdPosition);
				foreach (Collider2D c in col)
				{
					if (c == collider)
					{
						movingFrontButton = true;
						sliding = true;
						
						currentRot = gameObjectToRotate.transform.localEulerAngles;
						currentRot.x=0;
						currentRot.y=0;
						
						Vector3 worldHold = holdPosition;//Camera.main.ScreenToWorldPoint(holdPosition);
						worldHold.z = gameObjectToRotate.transform.position.z;
						
						gameObjectToRotate.transform.LookAt(worldHold, Vector3.forward);
						gameObjectToRotate.transform.Rotate(90+angleOffset, 0 ,0);
						
						startedRotation = new Vector3(0,0,gameObjectToRotate.transform.localEulerAngles.z-currentRot.z);
						
						gameObjectToRotate.transform.localEulerAngles -= new Vector3(0,0,gameObjectToRotate.transform.localEulerAngles.z);// + new Vector3(0,0,rotatingKey.gameObjectToRotate.transform.localEulerAngles.z);
						
						gameObjectToRotate.transform.localEulerAngles += currentRot;
						
						if(limit) EnforceLimit();
						AnalyzePositionAndSendEvents();
					}
				
				}
			}
			else if (sliding)
			{
				holdPosition = Camera.main.ScreenToWorldPoint(holdPosition);
				if(movingFrontButton) MoveFrontButton(startPos, holdPosition);
				AnalyzePositionAndSendEvents();
				
			}
			else if (stopMoving)
			{
				stopMoving = false;

				if(snap)
				{
					/*
					float cA = gameObjectToRotate.localEulerAngles.z;
					float dS = Mathf.Abs (Mathf.DeltaAngle(cA, startAngle));
					float dT = Mathf.Abs (Mathf.DeltaAngle(cA, targetAngle));

					//Debug.Log (cA + " [" + startAngle + ", " + targetAngle + "] dS=" + dS + ", dT=" + dT); 

					if(dS < dT) SnapToAngle(startAngle);
					else SnapToAngle (targetAngle);
					*/
					ProcessSnapAngles();
					AnalyzePositionAndSendEvents();
				}


				//startAngle = gameObjectToRotate.transform.eulerAngles.z;
			}
		}

		void ProcessSnapAngles()
		{
			float _currentDiff = float.MaxValue;
			float _currentAngle = 0;
			bool _doSnap = false;

			float _cA = gameObjectToRotate.localEulerAngles.z;

			int _angleIndex = -1;

			for(int i=0; i<snapAngles.Length; i++)
			{
				float _angle = snapAngles[i];

				float _diff = Mathf.Abs ( Mathf.DeltaAngle(_cA, _angle) );

				if(_diff < _currentDiff && _diff < snapThreshold)
				{
					_currentDiff = _diff;
					_currentAngle = _angle;
					_doSnap = true;
					_angleIndex = i;
				}
			}

			if(_doSnap)
			{
				lastSnapIndex = _angleIndex;
				SnapToAngle(_currentAngle);
			}
		}

	}
}