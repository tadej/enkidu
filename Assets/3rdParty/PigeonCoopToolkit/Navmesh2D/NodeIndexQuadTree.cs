using UnityEngine;
using System.Linq;

using System.Collections.Generic;

namespace PigeonCoopToolkit.Navmesh2D
{
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class NodeIndexQuadTree : MonoBehaviour
    {
        [System.Serializable]
        public class VectorIndexPair
        {
            public Vector2 Position;
            public int Obj;
        }

        public VectorIndexPair[] AllIndiciesInTree;
        [SerializeField]
        public NodeIndexQuadTreeNode NodeIndexQuadTreeNode;

        public string ObstructionLayer;

        void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
        }

        public void GenerateQuadTree(string obstructionLayer, int numObjectsPerNode)
        {
            ObstructionLayer = obstructionLayer;
            numObjectsPerNode = Mathf.Clamp(numObjectsPerNode,1, int.MaxValue);
            DestroyTree();
            if(AllIndiciesInTree.Length <= 1)
            {
                Debug.LogError("QuadTree: Only one object in quad tree. Need more.");
                return;
            }

            float minX = AllIndiciesInTree.Min(a => a.Position.x) - 0.5f;
            float minY = AllIndiciesInTree.Min(a => a.Position.y) - 0.5f;
            float maxX = AllIndiciesInTree.Max(a => a.Position.x) + 0.5f;
            float maxY = AllIndiciesInTree.Max(a => a.Position.y) + 0.5f;

            NodeIndexQuadTreeNode = ScriptableObject.CreateInstance<NodeIndexQuadTreeNode>();
            NodeIndexQuadTreeNode.Init(new Rect(minX, minY, Mathf.Abs(minX - maxX), Mathf.Abs(minY - maxY)), AllIndiciesInTree, obstructionLayer,numObjectsPerNode);
        
        }

        public void DestroyTree()
        {
            if (NodeIndexQuadTreeNode != null)
                DestroyImmediate(NodeIndexQuadTreeNode);
        }

        public void SetIndicies(VectorIndexPair[] transforms)
        {
            AllIndiciesInTree = transforms;
        }
        
        public VectorIndexPair ClosestTo(Vector2 pos)
        {
            if (NodeIndexQuadTreeNode == null || Physics2D.OverlapPoint(pos, 1 << LayerMask.NameToLayer(ObstructionLayer)) != null)
                return null;

            return NodeIndexQuadTreeNode.ClosestTo(pos);
        }

        public int ClosestIndexTo(Vector2 pos)
        {
            if (NodeIndexQuadTreeNode == null || Physics2D.OverlapPoint(pos, 1 << LayerMask.NameToLayer(ObstructionLayer)) != null)
                return -1;

            VectorIndexPair pair = NodeIndexQuadTreeNode.ClosestTo(pos);
            if (pair != null) return pair.Obj;
            else return -1;
        }

        public Vector2 ClosestPointTo(Vector2 pos)
        {
            if (NodeIndexQuadTreeNode == null || Physics2D.OverlapPoint(pos, 1 << LayerMask.NameToLayer(ObstructionLayer)) != null)
                return pos;

            return NodeIndexQuadTreeNode.ClosestPointTo(pos);
        }



        public int ActualClosestIndexTo(Vector2 pos, bool checkObstructions = true)
        {
			if (NodeIndexQuadTreeNode == null || (checkObstructions && Physics2D.OverlapPoint(pos, 1 << LayerMask.NameToLayer(ObstructionLayer)) != null))
                return -1;
            
            VectorIndexPair pair = NodeIndexQuadTreeNode.ActualClosestTo(pos);
            if (pair != null)
				return pair.Obj;
			else {

				return -1;
			}
        }

        public Vector2 ActualClosestPointTo(Vector2 pos)
        {
            return NodeIndexQuadTreeNode.ActualClosestPointTo(pos);
        }

        void OnDestroy()
        {
            DestroyTree();
        }

    }

    
    
}
