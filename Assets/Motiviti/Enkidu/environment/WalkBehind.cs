using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class WalkBehind : MonoBehaviour
    {

        public float walkBehindY = 0;

        public int sortingLayerInFrontOfElroy = 15;

        public string sortingLayerNameInFrontOfElroy = "";
        public int sortingIndexInFrontOfElroy = 0;

        SpriteRenderer sprite;

        int originalSortingIndex = 0;

        string originalSortingLayerName = "";

        bool flipYCoordinate = false;

        void Awake()
        {
        }

        IEnumerator Start()
        {
            yield return null;
            flipYCoordinate = PersistentEngine.scene.flipYCoordinate;
            sprite = GetComponent<SpriteRenderer>();

            originalSortingLayerName = sprite.sortingLayerName;
            originalSortingIndex = sprite.sortingOrder;
            if (sortingLayerNameInFrontOfElroy == "")
                sortingLayerNameInFrontOfElroy = WalkBehind.GetSortingLayerNameById(sortingLayerInFrontOfElroy);
        }

        void FixedUpdate()
        {


            if (PersistentEngine.player && ((!flipYCoordinate && PersistentEngine.player.transform.position.y > walkBehindY)
                                || (flipYCoordinate && PersistentEngine.player.transform.position.y < walkBehindY)))


            {
                if (sprite)
                {
                    sprite.sortingLayerName = sortingLayerNameInFrontOfElroy;
                    sprite.sortingOrder = sortingIndexInFrontOfElroy;
                }
            }
            else
            {
                if (sprite)
                {
                    sprite.sortingLayerName = originalSortingLayerName;
                    sprite.sortingOrder = originalSortingIndex;
                }


            }
        }

        public static string GetSortingLayerNameById(int id)
        {
            string name = "";
            switch (id)
            {
                case 0: name = "Negative2"; break;
                case 1: name = "Negative1"; break;
                case 2: name = "Behind background"; break;
                case 3: name = "Background"; break;
                case 4: name = "Default"; break;
                case 5: name = "Items"; break;
                case 6: name = "Body"; break;
                case 7: name = "Chest"; break;
                case 8: name = "Head"; break;
                case 9: name = "Face"; break;
                case 10: name = "Pupils"; break;
                case 11: name = "Eyes"; break;
                case 12: name = "Eyebrows"; break;
                case 13: name = "Mouth"; break;
                case 14: name = "AnimatedBody"; break;
                case 15: name = "Items-InFrontOfElroy"; break;
                case 16: name = "Paralax1"; break;
                case 17: name = "Puzzle"; break;
                case 18: name = "Full Screen"; break;
                case 19: name = "CutSceneBand"; break;
                case 20: name = "Inventory"; break;
                case 21: name = "PeggyPhone-Body"; break;
                case 22: name = "PeggyPhone-Chest"; break;
                case 23: name = "PeggyPhone-Body"; break;
                case 24: name = "PeggyPhone-Head"; break;
                case 25: name = "PeggyPhone-Face"; break;
                case 26: name = "PeggyPhone-Pupils"; break;
                case 27: name = "PeggyPhone-Eyes"; break;
                case 28: name = "PeggyPhone-Eyebrows"; break;
                case 29: name = "PeggyPhone-Mouth"; break;
                case 30: name = "cutscene"; break;
                case 31: name = "DialogText"; break;
                case 32: name = "Puppet2D - Bones"; break;
                case 33: name = "Puppet2D - Controls"; break;
            }
            return name;
        }
    }
}