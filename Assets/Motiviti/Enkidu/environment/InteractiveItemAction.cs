using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using System;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
        
    public class InteractiveItemAction : StatefulItem 
    {
        public bool overrideStoppingDistance = false;
        public Vector3 stoppingDistance = Vector3.zero;
        public Vector3 centerOffset = Vector3.zero;

        [SaveState]
        public int sequenceNumber = 0;

        public Player.State actionAnimation = Player.State.PickUpSide;
        
        protected AudioManager audioManager;

        public InteractiveItem interactiveItem;

        public bool highlightWhenSelected = false;
        public SpriteRenderer highlightOverlay = null;
        protected float highlightAlphaValue = 0;
        protected float highlightAmp = 0;
        protected bool highlighting = false;

        public Player.State endState = Player.State.None;

        void OnDrawGizmosSelected()
        {
            if (overrideStoppingDistance)
            {
                Gizmos.DrawIcon(transform.position + stoppingDistance, "gizmo-stopping.psd", true);
                Gizmos.DrawIcon(transform.position + centerOffset, "gizmo-center.psd", true);
                Gizmos.DrawIcon(transform.position, "gizmo-pos.psd", true);
            }
        }

        // Use this for initialization
        void Start () {
            Initialise();
        }

        public virtual bool SupportsDoubleClick()
        {
            return false;
        }

        public virtual void DoubleClicked()
        {
            Debug.Log("Not implemented");
        }

        public virtual bool CanDeactivate()
        {
            return true;
        }

        public virtual bool ShouldSkip()
        {
            return false;
        }

        protected override void Initialise()
        {
            base.Initialise();

            interactiveItem = GetComponent<InteractiveItem>();
            
            audioManager = Global.audioManager;
        }

        protected override void InitialiseGlobal()
        {
            base.InitialiseGlobal();
            
            interactiveItem = GetComponent<InteractiveItem>();
            
            audioManager = Global.audioManager;
        }
        
        // Update is called once per frame
        protected virtual void Update () 
        {
            HightlightFrameUpdate();
        }

        protected virtual void HightlightFrameUpdate()
        {
            if (highlightOverlay)
            {
                highlightAmp = Mathf.Lerp(highlightAmp, 0, Time.deltaTime * 1.5f);

                if (highlighting)
                {
                    highlightOverlay.color = new Color(1, 1, 1, (Mathf.Sin(Time.time * 20) + 1) * 0.5f * highlightAmp);
                }
                else
                {
                    highlightOverlay.color = new Color(1, 1, 1, 0);
                }
            }
        }

        IEnumerator HighlightProcedure()
        {
            highlightAmp = 0.2f;
            highlighting = true;
            yield return new WaitForSeconds(1.5f);
            highlighting = false;
            yield return null;
        }

        public void Selected()
        {
            if (highlightWhenSelected)
            {
                StartCoroutine(HighlightProcedure());
            }
        }

        public void StopHighlighting()
        {
            highlighting = false;
        }
        
        public virtual IEnumerator ProcessArrivedAt()
        {
            yield return null;
        }

        public virtual void AnimationActionPoint(string animationName)
        {
            
        }
        
        public virtual void AnimationFinished(string animationName)
        {
            
        }

        
    }
}