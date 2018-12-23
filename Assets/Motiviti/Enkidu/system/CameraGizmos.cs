using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class CameraGizmos : MonoBehaviour
    {

        void OnDrawGizmos()
        {
            AspectRatioGizmos(16f / 9f, new Color(0.0f, 0, 0, 0.3f), new Color(0.0f, 0.0f, 0, 0.3f), 0.1f, 4f);
        }

        void AspectRatioGizmos(float ratio, Color color, Color altColor, float bandwidth, float altBandwidth)
        {
            float width = GetComponent<Camera>().orthographicSize * 2 * ratio;
            float height = GetComponent<Camera>().orthographicSize * 2;

            float altRatio = 4f / 3f;

            float altHeight = width / altRatio;

            Gizmos.color = color;
            Gizmos.DrawCube(transform.position + Vector3.up * (height * 0.5f + bandwidth * 0.5f), new Vector3(width + 2 * bandwidth, bandwidth, 10));
            Gizmos.DrawCube(transform.position - Vector3.up * (height * 0.5f + bandwidth * 0.5f), new Vector3(width + 2 * bandwidth, bandwidth, 10));

            bandwidth = altBandwidth;
            Gizmos.color = altColor;
            Gizmos.DrawCube(transform.position + Vector3.up * (altHeight * 0.5f + bandwidth * 0.5f), new Vector3(width + 2 * bandwidth, bandwidth, 10));
            Gizmos.DrawCube(transform.position - Vector3.up * (altHeight * 0.5f + bandwidth * 0.5f), new Vector3(width + 2 * bandwidth, bandwidth, 10));

            Gizmos.DrawCube(transform.position - Vector3.right * (width * 0.5f + bandwidth * 0.5f), new Vector3(bandwidth, altHeight, 10));
            Gizmos.DrawCube(transform.position + Vector3.right * (width * 0.5f + bandwidth * 0.5f), new Vector3(bandwidth, altHeight, 10));

        }
    }
}