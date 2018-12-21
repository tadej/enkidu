namespace PigeonCoopToolkit.Navmesh2D
{
    [System.Serializable]
    public class NavMesh2DConnection {

        public enum ConnectionType
        {
            Standard,
        }

        public ConnectionType connectionType; 
        public int connectedNodeIndex;

    }
}

