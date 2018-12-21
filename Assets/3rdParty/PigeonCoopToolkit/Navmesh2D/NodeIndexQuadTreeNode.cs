using System.Linq;
using UnityEngine;

namespace PigeonCoopToolkit.Navmesh2D 
{
    [ExecuteInEditMode]
    public class NodeIndexQuadTreeNode : ScriptableObject
    {
        public Rect NodeBounds;
        public string ObstructionLayer;
        public NodeIndexQuadTree.VectorIndexPair[] ObjectsWithinNode;
        public NodeIndexQuadTreeNode[] ChildNodes;

        private void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }

        private void OnDestroy()
        {
            if(Application.isPlaying)
            {
                for (int i = 0; i < ChildNodes.Length; i++)
                {
                    Destroy(ChildNodes[i]);
                }
            }
            else
            {
                for (int i = 0; i < ChildNodes.Length; i++)
                {
                    DestroyImmediate(ChildNodes[i]);
                }
            }
        }


        public void Init(Rect b, NodeIndexQuadTree.VectorIndexPair[] objectsWithinNode, string _ObstructionLayer, int numObjectsPerNode)
        {
            NodeBounds = b;
            ObstructionLayer = _ObstructionLayer;
            ObjectsWithinNode = objectsWithinNode;
            if (ObjectsWithinNode.Length <= numObjectsPerNode)
            {
                ChildNodes = new NodeIndexQuadTreeNode[0];
                return;
            }

            ChildNodes = new NodeIndexQuadTreeNode[4];
            Rect[] childNodeRects = new Rect[4];
            childNodeRects[0] = new Rect(b.xMin, b.yMin, b.width/2f, b.height/2);
            childNodeRects[1] = new Rect(b.xMin + b.width/2f, b.yMin, b.width/2f, b.height/2);
            childNodeRects[2] = new Rect(b.xMin, b.yMin + b.height/2, b.width/2f, b.height/2);
            childNodeRects[3] = new Rect(b.xMin + b.width/2f, b.yMin + b.height/2, b.width/2f, b.height/2);

            ChildNodes[0] = CreateInstance<NodeIndexQuadTreeNode>();
            ChildNodes[0].Init(childNodeRects[0],
                               objectsWithinNode.Where(a => childNodeRects[0].Contains(a.Position)).ToArray(), ObstructionLayer,numObjectsPerNode);
            ChildNodes[1] = CreateInstance<NodeIndexQuadTreeNode>();
            ChildNodes[1].Init(childNodeRects[1],
                               objectsWithinNode.Where(a => childNodeRects[1].Contains(a.Position)).ToArray(), ObstructionLayer, numObjectsPerNode);
            ChildNodes[2] = CreateInstance<NodeIndexQuadTreeNode>();
            ChildNodes[2].Init(childNodeRects[2],
                               objectsWithinNode.Where(a => childNodeRects[2].Contains(a.Position)).ToArray(), ObstructionLayer, numObjectsPerNode);
            ChildNodes[3] = CreateInstance<NodeIndexQuadTreeNode>();
            ChildNodes[3].Init(childNodeRects[3],
                               objectsWithinNode.Where(a => childNodeRects[3].Contains(a.Position)).ToArray(), ObstructionLayer, numObjectsPerNode);
            
        }

        public NodeIndexQuadTree.VectorIndexPair ClosestTo(Vector2 pos)
        {
            return ObjectClosestToRecursive(pos);
        }

        public Vector2 ClosestPointTo(Vector2 pos)
        {
            NodeIndexQuadTree.VectorIndexPair foundPos = ObjectClosestToRecursive(pos);
            return foundPos != null ? foundPos.Position : pos;
        }

        public NodeIndexQuadTree.VectorIndexPair ActualClosestTo(Vector2 pos)
        {
            NodeIndexQuadTree.VectorIndexPair opp =
               ObjectsWithinNode.OrderBy(a => Vector2.Distance(pos, a.Position)).FirstOrDefault();

            return opp ?? null;
        }


        public Vector2 ActualClosestPointTo(Vector2 pos)
        {
            NodeIndexQuadTree.VectorIndexPair opp =
               ObjectsWithinNode.OrderBy(a => Vector2.Distance(pos, a.Position)).FirstOrDefault();

            return opp == null ? Vector2.zero : opp.Position;
        }

        private NodeIndexQuadTree.VectorIndexPair ObjectClosestToRecursive(Vector2 pos)
        {
            if (ObjectsWithinNode.Length == 0 || ObjectsWithinNode.Any() == false) 
                return null;

            NodeIndexQuadTree.VectorIndexPair found = null;

            if (NodeBounds.Contains(pos))
            {
                foreach (NodeIndexQuadTreeNode child in ChildNodes)
                {
                    found = child.ObjectClosestToRecursive(pos);
                    if (found != null)
                        break;
                }

                if (found == null)
                {
                    foreach (var t in
                    ObjectsWithinNode.OrderBy(a => Vector2.Distance(a.Position, pos)))
                    {
                        if (ObstructionLayer == "" || Physics2D.Raycast(pos, ((Vector2)t.Position - pos).normalized, Vector2.Distance(pos, t.Position), 1 << LayerMask.NameToLayer(ObstructionLayer)) == false)
                        {
                            found = t;
                            break;
                        }
                    }
                }
            }

            return found;
        }

       
    }
}