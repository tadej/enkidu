using UnityEngine;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    [ExecuteInEditMode]
    public class LineRendererLayer : MonoBehaviour
    {
        public string sortingLayer;
        public int sortingOrder;
        Renderer meshRenderer = null;

        void Start()
        {
            meshRenderer = getMeshRenderer();
        }

        private Renderer getMeshRenderer()
        {
            if (meshRenderer == null) meshRenderer = gameObject.GetComponent<Renderer>();
            return meshRenderer;
        }

        void Update()
        {
            if (getMeshRenderer() && getMeshRenderer().sortingLayerName != sortingLayer && sortingLayer != "")
            {
                getMeshRenderer().sortingLayerName = sortingLayer;
                getMeshRenderer().sortingOrder = sortingOrder;
            }
        }
    }
}