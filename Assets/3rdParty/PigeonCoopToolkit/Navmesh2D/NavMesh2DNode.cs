using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PigeonCoopToolkit.Navmesh2D
{
    [System.Serializable]
    public class NavMesh2DNode
    {
        public List<NavMesh2DConnection> connections = new List<NavMesh2DConnection>();
        public HashSet<int> connectionsHashed = null;
        public Vector3 position;

        private void Init()
        {
            if (connectionsHashed == null)
            {
                connectionsHashed = new HashSet<int>();
                foreach (NavMesh2DConnection connection in connections)
                {
                    connectionsHashed.Add(connection.connectedNodeIndex);
                }
            }
        }

        public bool ConnectedTo(int n)
        {
            Init();

            return connectionsHashed.Contains(n);
        }

        public virtual bool ConnectTo(int n, NavMesh2DConnection.ConnectionType connectionType)
        {
            Init();

            if (!ConnectedTo(n))
            {
                connectionsHashed.Add(n);
                NavMesh2DConnection newConnection = new NavMesh2DConnection();

                newConnection.connectedNodeIndex = n;
                newConnection.connectionType = connectionType;
                connections.Add(newConnection);
                return true;
            }

            return false;
        }

        public virtual bool DisconnectFrom(int n)
        {
            Init();

            if (ConnectedTo(n))
            {
                connectionsHashed.Remove(n);

                connections.Remove(connections.First(a => a.connectedNodeIndex == n));
                return true;
            }

            return false;
        }


    }
}