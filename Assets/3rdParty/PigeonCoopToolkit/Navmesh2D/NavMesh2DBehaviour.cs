using System.Collections.Generic;
using System.Linq;
using PigeonCoopToolkit.Generic;
using PigeonCoopToolkit.Utillities;
using UnityEngine;

namespace PigeonCoopToolkit.Navmesh2D
{
    [AddComponentMenu("")]
    public class NavMesh2DBehaviour : MonoBehaviour
    {
        public enum JoinType { jtSquare, jtRound, jtMiter };

        [System.Serializable]
        public class MeshContour
        {
            public Vector2[] Points;
        }

        [System.Serializable]
        public class NavmeshGenerationInformation
        {
            public string WalkableColliderLayer;
            public string ObstructionColliderLayer;
            public float CircleColliderSubdivisionFactor;
            public float CalculationScaleFactor;
            public float ColliderPadding;
            public bool UseGrid;
            public Vector2 GridSize;
            public NavMesh2DBehaviour.JoinType JoinType;
        }

        public MeshContour[] Contours;
        public Mesh NavMesh;
        public NavMesh2DNode[] NavMesh2DNodes;
        public NodeIndexQuadTree NodeNodeIndexQuadTree;
        public NavmeshGenerationInformation GenerationInformation;
        public VersionInformation Version;

        /// <summary>
        /// Get Node
        /// </summary>
        /// <param name="index">Node at Index</param>
        public NavMesh2DNode GetNode(int index)
        {
            if (index == -1)
                return null;

            return NavMesh2DNodes[index];
        }

        /// <summary>
        /// Gets the estimated closest node index to a given position (Uses the quadtree, faster but may not be accurate)
        /// </summary>
        /// <param name="pos">The position to test against</param>
        public int ClosestNodeIndexTo(Vector2 pos)
        {
            return NodeNodeIndexQuadTree.ClosestIndexTo(pos);
        }

        /// <summary>
        /// Gets the closest node index to a given position (Slower but guaranteed to be accurate)
        /// </summary>
        /// <param name="pos">The position to test against</param>
        public int ActualClosestNodeIndexTo(Vector2 pos, bool checkObstructions = true)
        {
            return NodeNodeIndexQuadTree.ActualClosestIndexTo(pos, checkObstructions);
        }

        /// <summary>
        /// Returns the layer on which the walls for this navmesh reside
        /// </summary>
        public LayerMask GetObstructionLayer()
        {
            return 1 << LayerMask.NameToLayer(GenerationInformation.ObstructionColliderLayer);
        }

        /// <summary>
        /// Returns the layer on which the floors for this navmesh reside
        /// </summary>
        public LayerMask GetWalkableLayer()
        {
            return 1 << LayerMask.NameToLayer(GenerationInformation.WalkableColliderLayer);
        }

        /// <summary>
        /// Gets the estimated closest node to a given position (Uses the quadtree, faster but may not be accurate)
        /// </summary>
        /// <param name="pos">The position to test against</param>
        public NavMesh2DNode ClosestNodeTo(Vector2 pos)
        {
            int index = ClosestNodeIndexTo(pos);

            if (index == -1)
                return null;
            else
                return NavMesh2DNodes[index];
        }

        /// <summary>
        /// Gets the closest node to a given position (Slower but guaranteed to be accurate)
        /// </summary>
        /// <param name="pos">The position to test against</param>
        public NavMesh2DNode ActualClosestNodeTo(Vector2 pos, bool checkObstructions = true)
        {
            int index = ActualClosestNodeIndexTo(pos, checkObstructions);

            if (index == -1)
                return null;
            else
                return NavMesh2DNodes[index];
        }

        #region Path Finding
        internal class AStarData
        {
            public NodeDataPair cameFrom = null;
            public float fScore = 0;
            public float gScore = 0;
            public float hScore = 0;
        }

        internal class NodeDataPair
        {
            public NavMesh2DNode n = null;
            public AStarData d = null;
        }

        /// <summary>
        /// Attempts to path from one Node to another. Use ClosestNodeTo and ActualClosestNodeTo to get the nodes. 
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <returns>A list of nodes that form a path from the node start to the end node.</returns>
        public List<NavMesh2DNode> GetPath(NavMesh2DNode start, NavMesh2DNode end)
        {
            return RunPathWorker(start, end);
        }

        private List<NavMesh2DNode> RunPathWorker(NavMesh2DNode start, NavMesh2DNode end)
        {
            AStarData startData = new AStarData
            {
                cameFrom = null,
                fScore = 0,
                gScore = 0,
                hScore = 0
            };

            Dictionary<NavMesh2DNode, NodeDataPair> openList = new Dictionary<NavMesh2DNode, NodeDataPair>();
            Dictionary<NavMesh2DNode, NodeDataPair> closedList = new Dictionary<NavMesh2DNode, NodeDataPair>();

            openList.Add(start, new NodeDataPair
            {
                n = start,
                d = startData
            });

            while (openList.Count != 0)
            {
                NodeDataPair current = null;

                {
                    float lowestFscore = float.MaxValue;
                    foreach (NodeDataPair n in openList.Values)
                    {
                        if (n.d.fScore < lowestFscore)
                        {
                            lowestFscore = n.d.fScore;
                            current = n;
                        }
                    }
                }

                openList.Remove(current.n);
                closedList.Add(current.n, current);

                if (closedList.ContainsKey(end))
                {
                    //found a path
                    List<NavMesh2DNode> pathFound = new List<NavMesh2DNode>();
                    ReconstructPath(current, ref pathFound);
                    pathFound.Reverse();
                    //Path path = new Path(startNode, endNode);
                    //FindGoingTo(endNode, null);
                    //ReconstructPath(startNode, ref path);
                    //path.Smooth();

                    return pathFound;
                }

                foreach (NavMesh2DConnection connection in current.n.connections)
                {
                    NavMesh2DNode connectedMesh2DNode = GetNode(connection.connectedNodeIndex);
                    if (closedList.ContainsKey(connectedMesh2DNode))
                        continue;

                    NodeDataPair pairAlreadyInOpenlist = null;
                    if (openList.ContainsKey(connectedMesh2DNode))
                        pairAlreadyInOpenlist = openList[connectedMesh2DNode];

                    if (pairAlreadyInOpenlist == null)
                    { //if the neighbor doesn't exist
                        AStarData nextNodeData = new AStarData();

                        nextNodeData.gScore = current.d.gScore + Vector3.Distance(current.n.position, connectedMesh2DNode.position);
                        nextNodeData.hScore = CalculateHeuristic(connectedMesh2DNode.position, end.position);//connection.Key == endNode ? Vector3.Distance(current.position,endNode.position) * 10 : Vector3.Distance(connection.Key.position,endNode.position); //heucistics will be used to find
                        //paths that are less dangerous, etc. danger being the heucistic.
                        nextNodeData.fScore = nextNodeData.gScore + nextNodeData.hScore;
                        nextNodeData.cameFrom = current;
                        openList.Add(connectedMesh2DNode, new NodeDataPair
                        {
                            n = connectedMesh2DNode,
                            d = nextNodeData
                        });
                    }
                    else if (current.d.gScore + Vector3.Distance(current.n.position, connectedMesh2DNode.position) < pairAlreadyInOpenlist.d.gScore)
                    { //if the neighbor exists and has bigger g score
                        pairAlreadyInOpenlist.d.gScore = current.d.gScore + Vector3.Distance(current.n.position, connectedMesh2DNode.position);
                        pairAlreadyInOpenlist.d.hScore = CalculateHeuristic(pairAlreadyInOpenlist.n.position, end.position);//connection.Key == endNode ? Vector3.Distance(current.position,endNode.position) * 10 : Vector3.Distance(connection.Key.position,endNode.position);//Vector3.Distance(connection.Key.position,endNode.position);
                        pairAlreadyInOpenlist.d.fScore = pairAlreadyInOpenlist.d.gScore + pairAlreadyInOpenlist.d.hScore;
                        pairAlreadyInOpenlist.d.cameFrom = current;
                    }
                }

            }

            return null;
        }

        private float CalculateHeuristic(Vector3 start, Vector3 end)
        {
            //return Vector3.Distance(start,end);
            //return Mathf.Abs(Mathf.Abs(start.x - end.x) - Mathf.Abs(start.y - end.y));
            return Mathf.Abs(Mathf.Abs(start.x - end.x) - Mathf.Abs(start.y - end.y));
        }

        private void ReconstructPath(NodeDataPair currentPair, ref List<NavMesh2DNode> path)
        {
            path.Add(currentPair.n);

            if (currentPair.d.cameFrom != null)
                ReconstructPath(currentPair.d.cameFrom, ref path);
        }

        /// <summary>
        /// Smooths a precalculated path from Start to End.
        /// </summary>
        /// <param name="paddingSize">This should usually be the same value as yourr navmesh padding. It stops objects from cutting corners. You can pass the radius of your character in as this value, too.</param>
        /// <param name="s">The start position</param>
        /// <param name="e">The end position</param>
        /// <param name="path">the precalculated path</param>
        /// <returns>A list of vector2's that form a smoothed path from start to end. The points s and e are inserted into the start and end of the path</returns>
        public List<Vector2> SmoothedVectorPath2D(float paddingSize, Vector2 s, Vector2 e, List<NavMesh2DNode> path)
        {

            List<Vector2> finalPath = new List<Vector2>();
            if (path == null)
                return finalPath;

            finalPath.Add(s);
            finalPath.AddRange(path.Select(a => (Vector2)a.position));
            finalPath.Add(e);

            for (int i = 0; i < finalPath.Count - 1; i++)
            {
                if (finalPath.Count <= 2)
                    break;

                int toRemove = 0;
                for (int lookAhead = i + 2; lookAhead < finalPath.Count; lookAhead++)
                {
                    Vector2 cross = Vector2.zero;
                    bool noHit = false;

                    if (GenerationInformation.ObstructionColliderLayer == "")
                        noHit = true;
                    else if (paddingSize > Mathf.Epsilon)
                    {
                        cross = Vector3.Cross((finalPath[lookAhead] - finalPath[i]).normalized, Vector3.back);
                        noHit =
                            Physics2D.Raycast(finalPath[i] + cross * paddingSize,
                                              (finalPath[lookAhead] - finalPath[i]).normalized,
                                              Vector2.Distance(finalPath[i], finalPath[lookAhead]),
                                              1 << LayerMask.NameToLayer(GenerationInformation.ObstructionColliderLayer)) == false
                            &&
                            Physics2D.Raycast(finalPath[i] - cross * paddingSize,
                                              (finalPath[lookAhead] - finalPath[i]).normalized,
                                              Vector2.Distance(finalPath[i], finalPath[lookAhead]),
                                              1 << LayerMask.NameToLayer(GenerationInformation.ObstructionColliderLayer)) == false;
                    }
                    else
                    {
                        noHit =
                            Physics2D.Raycast(finalPath[i],
                                              (finalPath[lookAhead] - finalPath[i]).normalized,
                                              Vector2.Distance(finalPath[i], finalPath[lookAhead]),
                                              1 << LayerMask.NameToLayer(GenerationInformation.ObstructionColliderLayer)) == false;
                    }

                    if (noHit)
                    {
                        toRemove++;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if (toRemove != 0)
                {
                    finalPath.RemoveRange(i + 1, toRemove);
                    if (i == 0)
                        i--;
                    else
                    {
                        i--;
                        i--;
                    }
                }
            }


            return finalPath;
        }
        #endregion
    }
}


