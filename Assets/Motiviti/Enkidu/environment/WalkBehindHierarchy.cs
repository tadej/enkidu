using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class WalkBehindHierarchy : MonoBehaviour
    {

        public float walkBehindY = 0;

        public int sortingLayerInFrontOfElroy = 10;

        public string sortingLayerNameInFrontOfElroy = "";

        Transform elroy;

        SpriteRenderer[] sprites = new SpriteRenderer[100];
        int spriteCounter = 0;
        int[] originalSortingLayers = new int[100];
        string[] originalSortingLayersNames = new string[100];

        void AddSprite(Transform t)
        {
            SpriteRenderer c = t.GetComponent<SpriteRenderer>();
            if (c != null)
            {
                originalSortingLayers[spriteCounter] = c.sortingLayerID;
                originalSortingLayersNames[spriteCounter] = c.sortingLayerName;
                sprites[spriteCounter] = c;
                spriteCounter++;
            }

            foreach (Transform t1 in t)
            {
                AddSprite(t1);
            }
        }

        // Use this for initialization
        void Start()
        {
            elroy = PersistentEngine.player.transform;

            AddSprite(transform);

            if (sortingLayerNameInFrontOfElroy == "")
                sortingLayerNameInFrontOfElroy = WalkBehind.GetSortingLayerNameById(sortingLayerInFrontOfElroy);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (elroy.position.y > walkBehindY)
            {
                for (int i = 0; i < spriteCounter; i++)
                {
                    var sprite = sprites[i];
                    sprite.sortingLayerName = sortingLayerNameInFrontOfElroy;
                }
            }
            else
            {
                for (int i = 0; i < spriteCounter; i++)
                {
                    var sprite = sprites[i];
                    sprite.sortingLayerName = originalSortingLayersNames[i];

                }
            }
        }
    }
}