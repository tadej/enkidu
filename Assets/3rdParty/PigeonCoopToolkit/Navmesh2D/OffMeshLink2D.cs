using PigeonCoopToolkit.Navmesh2D;
using PigeonCoopToolkit.Utillities;
using UnityEngine;


[ExecuteInEditMode]
[AddComponentMenu("Pigeon Coop Toolkit/Navigation/OffMesh Link 2D")]
public class OffMeshLink2D : MonoBehaviour
{
    public int PointA = -1, PointB = -1;
    [HideInInspector]
    public Vector2 PointAPos = Vector2.right, PointBPos = -Vector2.right;
    public bool LinkActive = true;
    public bool Bidirectional = true;

    private Vector2 _lastPointAPos, _lastPointBPos;
    private bool _lastBidirectional;
    private bool LinkEstablished
    {
        get { return PointA >= 0 && PointB >= 0 && NavMesh2D.GetNavMeshObject().GetNode(PointA) != null && NavMesh2D.GetNavMeshObject().GetNode(PointB) != null; }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        if (LinkEstablished == false || NavMesh2D.SceneHasNavmesh() == false)
        {
            return;
        }

        Gizmos.color = Color.white;
        GizmosExtra.GizmosDrawArrow(NavMesh2D.GetNavMeshObject().GetNode(PointA).position + Vector3.back * 0.1f, NavMesh2D.GetNavMeshObject().GetNode(PointB).position + Vector3.back * 0.1f, 0.2f);
        if(Bidirectional)
            GizmosExtra.GizmosDrawArrow(NavMesh2D.GetNavMeshObject().GetNode(PointB).position + Vector3.back * 0.1f, NavMesh2D.GetNavMeshObject().GetNode(PointA).position + Vector3.back * 0.1f, 0.2f);

           
    }

    void OnDrawGizmosSelected()
    {
        if (LinkEstablished == false)
        {
            return;
        }

        GizmosExtra.GizmosDrawCircle(transform.TransformPoint(PointAPos), Vector3.back, 0.1f, 10);
        GizmosExtra.GizmosDrawCircle(transform.TransformPoint(PointBPos), Vector3.back, 0.1f, 10);
    }

    void OnDestroy()
    {
        if (NavMesh2D.SceneHasNavmesh())
            BreakLink();
    }

    void Update()
    {
        if(NavMesh2D.SceneHasNavmesh() == false)
        {
            PointA = -1;
            PointB = -1;
            return;
        }

        EnforceConnection();

        if(LinkEstablished && !LinkActive)
        {
            BreakLink();
        }
        else if(LinkEstablished == false && LinkActive)
        {
            EstablishLink();
        }
        else if (((Vector3)_lastPointAPos != transform.TransformPoint(PointAPos) || (Vector3)_lastPointBPos != transform.TransformPoint(PointBPos)) || (_lastBidirectional != Bidirectional))
        {
            BreakLink();
            EstablishLink();
        }
    }

    private void EnforceConnection()
    {
        if(LinkEstablished && LinkActive)
        {
            NavMesh2D.GetNavMeshObject().GetNode(PointA).ConnectTo(PointB, NavMesh2DConnection.ConnectionType.Standard);
            if (Bidirectional)
            {
                NavMesh2D.GetNavMeshObject().GetNode(PointB).ConnectTo(PointA, NavMesh2DConnection.ConnectionType.Standard);
            }
        }
    }

    private void BreakLink()
    {
        if(LinkEstablished)
        {
            NavMesh2D.GetNavMeshObject().GetNode(PointA).DisconnectFrom(PointB);
            NavMesh2D.GetNavMeshObject().GetNode(PointB).DisconnectFrom(PointA);
        }

        PointA = -1;
        PointB = -1;
    }

    void EstablishLink()
    {
        _lastPointAPos = transform.TransformPoint(PointAPos);
        _lastPointBPos = transform.TransformPoint(PointBPos);
        _lastBidirectional = Bidirectional;

        PointA = NavMesh2D.GetNavMeshObject().ClosestNodeIndexTo(transform.TransformPoint(PointAPos));
        PointB = NavMesh2D.GetNavMeshObject().ClosestNodeIndexTo(transform.TransformPoint(PointBPos));

        if (PointA == -1 || PointB == -1)
        {
            PointA = NavMesh2D.GetNavMeshObject().ActualClosestNodeIndexTo(transform.TransformPoint(PointAPos));
            PointB = NavMesh2D.GetNavMeshObject().ActualClosestNodeIndexTo(transform.TransformPoint(PointBPos));
            if (PointA == -1 || PointB == -1)
            {
                return;
            }
        }

        if (Bidirectional && NavMesh2D.GetNavMeshObject().GetNode(PointA).ConnectedTo(PointB) && NavMesh2D.GetNavMeshObject().GetNode(PointB).ConnectedTo(PointA))
        {
            PointA = -1;
            PointB = -1;
            return;
        }

        if (!Bidirectional && NavMesh2D.GetNavMeshObject().GetNode(PointA).ConnectedTo(PointB))
        {
            PointA = -1;
            PointB = -1;
            return;
        }

        NavMesh2D.GetNavMeshObject().GetNode(PointA).ConnectTo(PointB, NavMesh2DConnection.ConnectionType.Standard);
        if(Bidirectional)
            NavMesh2D.GetNavMeshObject().GetNode(PointB).ConnectTo(PointA, NavMesh2DConnection.ConnectionType.Standard);
    }

    public void ForceReset()
    {
        BreakLink();
    }
}