using UnityEngine;

public class ChunkBorder : MonoBehaviour
{
    private ChunkData chunkData;
    private LineRenderer lineRenderer;
    public Color borderColor = Color.red;
    public float lineWidth = 0.03f;
    private bool isCreated = false;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        chunkData = GetComponent<ChunkData>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
    }

    public void CreateBorder()
    {
        if (isCreated) return;

        float halfSize = chunkData.ChunkSize / 2f;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = borderColor;
        lineRenderer.endColor = borderColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = false;
        lineRenderer.useWorldSpace = false;

        lineRenderer.positionCount = 16;

        lineRenderer.SetPosition(0, new Vector3(-halfSize, -halfSize, -halfSize));
        lineRenderer.SetPosition(1, new Vector3(halfSize, -halfSize, -halfSize));
        lineRenderer.SetPosition(2, new Vector3(halfSize, -halfSize, halfSize));
        lineRenderer.SetPosition(3, new Vector3(-halfSize, -halfSize, halfSize));
        lineRenderer.SetPosition(4, new Vector3(-halfSize, -halfSize, -halfSize));

        lineRenderer.SetPosition(5, new Vector3(-halfSize, halfSize, -halfSize));
        lineRenderer.SetPosition(6, new Vector3(halfSize, halfSize, -halfSize));
        lineRenderer.SetPosition(7, new Vector3(halfSize, halfSize, halfSize));
        lineRenderer.SetPosition(8, new Vector3(-halfSize, halfSize, halfSize));
        lineRenderer.SetPosition(9, new Vector3(-halfSize, halfSize, -halfSize));

        lineRenderer.SetPosition(10, new Vector3(-halfSize, halfSize, halfSize));
        lineRenderer.SetPosition(11, new Vector3(-halfSize, -halfSize, halfSize));
        lineRenderer.SetPosition(12, new Vector3(halfSize, -halfSize, halfSize));
        lineRenderer.SetPosition(13, new Vector3(halfSize, halfSize, halfSize));
        lineRenderer.SetPosition(14, new Vector3(halfSize, halfSize, -halfSize));
        lineRenderer.SetPosition(15, new Vector3(halfSize, -halfSize, -halfSize));

        isCreated = true;
    }

    public void Highlight()
    {
        borderColor = Color.green;
        isCreated = false;
        CreateBorder();
    }

    public void Unhighlight()
    {
        borderColor = Color.red;
        isCreated = false;
        CreateBorder();
    }
}
