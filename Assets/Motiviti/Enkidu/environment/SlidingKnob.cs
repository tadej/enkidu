using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
	
		public class SlidingKnob : MonoBehaviour {

		public bool snap = true, disableWhenTargetReached = true;
		public Orientation orientation = Orientation.Horizontal;

		public bool isSnappedAndFinished = false;

		public delegate void TargetReached();
		public  event TargetReached OnTargetReached;

		public delegate void Moved( float d );
		public  event Moved OnMoved;

		public delegate void Snapped( float d );
		public  event Snapped OnSnapped;

		public float initialPosition = 0;
		public float[] snapPositions;
		public float snapThreshold=30;


		public enum Orientation
		{
			Horizontal = -1,
			Vertical = 1
		}

		float startPosition=0, targetPosition=100;

		Vector3 startPos = Vector3.zero;

		public bool isEnabled = true;

		public Transform gameObjectToSlide;

		new	Collider2D collider;

		bool sliding = false;
		bool movingFrontButton = false;
		float startD = 0;

		// Use this for initialization
		void Start () 
		{
			collider = GetComponent<Collider2D>();
			if(gameObjectToSlide == null) gameObjectToSlide = transform;

			if(snapPositions.Length >= 2)
			{
				startPosition = snapPositions[0];
				targetPosition = snapPositions[ snapPositions.Length-1 ];
			}

			SnapToPosition(initialPosition);
		
		}

		void SnapToPosition(float pos)
		{
			Vector3 c = transform.localPosition;
			Vector3 newPos = new Vector3( orientation == Orientation.Horizontal ? pos : c.x, orientation == Orientation.Vertical ? pos : c.y, c.z);
			gameObjectToSlide.transform.localPosition = newPos;
			if(OnSnapped != null) OnSnapped(pos);
		}

		void EnforceLimit()
		{
			Vector3 c = gameObjectToSlide.transform.localPosition;

			if(orientation == Orientation.Horizontal)
			{
				if(c.x < startPosition) SnapToPosition(startPosition);
				if(c.x > targetPosition) SnapToPosition(targetPosition);
			}
			else
			if(orientation == Orientation.Vertical)
			{
				if(c.y < startPosition) SnapToPosition(startPosition);
				if(c.y > targetPosition) SnapToPosition(targetPosition);
			}
		}

		void MoveFrontButton(Vector3 start, Vector3 current)
		{
			Vector3 c = gameObjectToSlide.transform.localPosition;

			Vector3 gc = gameObjectToSlide.transform.position;

			switch(orientation)
			{
			case Orientation.Horizontal:
				gameObjectToSlide.transform.position = new Vector3(current.x-startD, gc.y, gc.z);
				break;

			case Orientation.Vertical:
				gameObjectToSlide.transform.position = new Vector3(gc.x, current.y-startD, gc.z);
				break;
			}

			EnforceLimit();
			AnalyzePositionAndSendEvents();

			// Vector3 da = gameObjectToSlide.transform.localPosition - c;

			if(OnMoved != null)
				OnMoved(orientation == Orientation.Horizontal ? c.x : c.y);
		}

		void AnalyzePositionAndSendEvents()
		{
			Vector3 c = gameObjectToSlide.transform.localPosition;

			float curr = orientation == Orientation.Horizontal ? c.x : c.y;

			if(Mathf.Abs ( curr - targetPosition ) < 0.5f)
			{
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

				switch(orientation)
				{
				case Orientation.Horizontal:
				
					startD = holdPosition.x - gameObjectToSlide.transform.position.x;
					break;

				case Orientation.Vertical:
					startD = holdPosition.y - gameObjectToSlide.transform.position.y;

					break;
				}



				Collider2D[] col = Physics2D.OverlapPointAll(holdPosition);
				foreach (Collider2D c in col)
				{
					if (c == collider)
					{
						movingFrontButton = true;
						sliding = true;
						
					/*
						currentRot = gameObjectToSlide.transform.localEulerAngles;
						currentRot.x=0;
						currentRot.y=0;
						
						Vector3 worldHold = holdPosition;//Camera.main.ScreenToWorldPoint(holdPosition);
						worldHold.z = gameObjectToSlide.transform.position.z;
						
						gameObjectToSlide.transform.LookAt(worldHold, Vector3.forward);
						gameObjectToSlide.transform.Rotate(90+angleOffset, 0 ,0);
						
						startedRotation = new Vector3(0,0,gameObjectToSlide.transform.localEulerAngles.z-currentRot.z);
						
						gameObjectToSlide.transform.localEulerAngles -= new Vector3(0,0,gameObjectToSlide.transform.localEulerAngles.z);// + new Vector3(0,0,rotatingKey.gameObjectToSlide.transform.localEulerAngles.z);
						
						gameObjectToSlide.transform.localEulerAngles += currentRot;
						
						if(limit) EnforceLimit();
						AnalyzePositionAndSendEvents();
					*/
					}
				
				}
			}
			else if (sliding)
			{
				holdPosition = Camera.main.ScreenToWorldPoint(holdPosition);
				if(movingFrontButton) MoveFrontButton(startPos, holdPosition);
				
			}
			else if (stopMoving)
			{
				stopMoving = false;

				if(snap)
				{
					/*
					float cA = gameObjectToSlide.localEulerAngles.z;
					float dS = Mathf.Abs (Mathf.DeltaAngle(cA, startAngle));
					float dT = Mathf.Abs (Mathf.DeltaAngle(cA, targetAngle));

					//Debug.Log (cA + " [" + startAngle + ", " + targetAngle + "] dS=" + dS + ", dT=" + dT); 

					if(dS < dT) SnapToAngle(startAngle);
					else SnapToAngle (targetAngle);
					*/
					ProcessSnapPositions();
					AnalyzePositionAndSendEvents();
				}


				//startAngle = gameObjectToSlide.transform.eulerAngles.z;
			}
		}

		void ProcessSnapPositions()
		{
			float _currentDiff = float.MaxValue;
			float _currentPos = 0;
			bool _doSnap = false;
			Vector3 c = gameObjectToSlide.transform.localPosition;

			float _cA = orientation == Orientation.Horizontal ? c.x : c.y;

			foreach(float _pos in snapPositions)
			{
				float _diff = Mathf.Abs ( _cA - _pos );

				if(_diff < _currentDiff && _diff < snapThreshold)
				{
					_currentDiff = _diff;
					_currentPos = _pos;
					_doSnap = true;
				}
			}

			if(_doSnap)
			{
				SnapToPosition(_currentPos);
			}
		}

	}
}