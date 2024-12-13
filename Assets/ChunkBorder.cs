using UnityEngine;

public class ChunkBorder : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public float borderHeight = 5f;
    public Color borderColor = Color.red;
    public float lineWidth = 0.1f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
    }

    public void CreateBorder()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.RecalculateBounds();
        float size = mesh.bounds.size.x;
        float halfSize = size / 2f;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = borderColor;
        lineRenderer.endColor = borderColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = false;
        lineRenderer.useWorldSpace = true;

        lineRenderer.positionCount = 5;

        Vector3 pos = transform.position;
        lineRenderer.SetPosition(0, new Vector3(pos.x - halfSize, borderHeight, pos.z - halfSize));
        lineRenderer.SetPosition(1, new Vector3(pos.x + halfSize, borderHeight, pos.z - halfSize));
        lineRenderer.SetPosition(2, new Vector3(pos.x + halfSize, borderHeight, pos.z + halfSize));
        lineRenderer.SetPosition(3, new Vector3(pos.x - halfSize, borderHeight, pos.z + halfSize));
        lineRenderer.SetPosition(4, new Vector3(pos.x - halfSize, borderHeight, pos.z - halfSize));
    }
}
