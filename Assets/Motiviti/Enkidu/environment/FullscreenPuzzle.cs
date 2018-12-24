using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class FullscreenPuzzle : MonoBehaviour
    {

        protected bool isEnabled = false;

        public bool hideSprites = true;

        protected Player elroy;

        public Renderer[] visibleElements;

        public Collider[] colliders;

        public Collider2D[] colliders2D;

        protected SpriteRenderer closeSpriter;

        public InteractiveItemPuzzle puzzle;

        protected bool enableCloseButton = true;

        protected CustomCursor customCursor;

        public int hasBackgroundCollider = -1;

        public virtual void ToggleShow(bool enabled)
        {
            isEnabled = enabled;
            Collider2D[] newColliders2D = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D c in newColliders2D)
            {
                c.enabled = enabled;
            }

            Collider[] newColliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in newColliders)
            {
                c.enabled = enabled;
            }

            if (hideSprites)
            {
                SpriteRenderer[] newSprites = GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer c in newSprites)
                {
                    c.enabled = enabled;
                }
            }

            if (hasBackgroundCollider == -1)
            {
                foreach (Transform child in transform)
                {
                    if (child.gameObject.layer == 15 && child.GetComponent<Collider2D>() != null)
                    {
                        hasBackgroundCollider = 1;
                    }
                }
                if (hasBackgroundCollider == -1)
                    hasBackgroundCollider = 0;
            }

            if (customCursor)
            {
                customCursor.SetInPuzzle(enabled, hasBackgroundCollider);
            }
        }

        public virtual void Initialize()
        {
            ToggleShow(false);
        }

        void Start()
        {
            customCursor = PersistentEngine.customCursor;
            ToggleShow(false);
        }
    }
}