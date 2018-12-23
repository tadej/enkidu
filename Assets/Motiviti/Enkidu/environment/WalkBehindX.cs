using UnityEngine;

namespace Motiviti.Enkidu
{
    public class WalkBehindX : MonoBehaviour
    {
        public float walkBehindX = 0;

        public int sortingLayerInFrontOfElroy = 10;

        public string sortingLayerNameInFrontOfElroy = "";

        SpriteRenderer sprite;

        Transform elroy;

        string originalSortingLayerName = "";

        void Start()
        {
            elroy = Global.player.transform;
            sprite = GetComponent<SpriteRenderer>();

            originalSortingLayerName = sprite.sortingLayerName;

            if (sortingLayerNameInFrontOfElroy == "")
                sortingLayerNameInFrontOfElroy = WalkBehind.GetSortingLayerNameById(sortingLayerInFrontOfElroy);
        }

        void Update()
        {
            if (elroy.position.x < walkBehindX)
            {
                sprite.sortingLayerName = sortingLayerNameInFrontOfElroy;
            }
            else
            {
                sprite.sortingLayerName = originalSortingLayerName;
            }
        }
    }
}