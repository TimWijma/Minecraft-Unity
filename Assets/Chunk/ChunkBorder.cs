using UnityEngine;

public class ChunkBorder : MonoBehaviour
{
    private ChunkData chunkData;
    private LineRenderer lineRenderer;
    public float borderHeight = 5f;
    public Color borderColor = Color.red;
    public float lineWidth = 0.1f;
    private bool isCreated = false;

    private void Awake()
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
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;

        lineRenderer.positionCount = 4;

        Debug.Log("Creating border for chunk with size: " + chunkData.ChunkSize);

        lineRenderer.SetPosition(0, new Vector3(-halfSize, borderHeight, -halfSize));
        lineRenderer.SetPosition(1, new Vector3(halfSize, borderHeight, -halfSize));
        lineRenderer.SetPosition(2, new Vector3(halfSize, borderHeight, halfSize));
        lineRenderer.SetPosition(3, new Vector3(-halfSize, borderHeight, halfSize));


        isCreated = true;
    }
}
