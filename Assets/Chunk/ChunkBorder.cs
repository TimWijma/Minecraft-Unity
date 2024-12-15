using UnityEngine;

public class ChunkBorder : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Color borderColor = Color.red;
    public float lineWidth = 0.03f;
    private bool isCreated = false;
    private int chunkSize = 16;

    private void Awake()
    {
        InitializeLineRenderer();
    }

    private void Start()
    {
        if (lineRenderer == null)
        {
            InitializeLineRenderer();
        }
    }

    private void InitializeLineRenderer()
    {
        return;

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = borderColor;
        lineRenderer.endColor = borderColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = false;
        lineRenderer.useWorldSpace = false;
    }

    public void CreateBorder(int chunkSize)
    {
        this.chunkSize = chunkSize;

        if (isCreated) return;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = borderColor;
        lineRenderer.endColor = borderColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = false;
        lineRenderer.useWorldSpace = false;

        lineRenderer.positionCount = 16;

        lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        lineRenderer.SetPosition(1, new Vector3(chunkSize, 0, 0));
        lineRenderer.SetPosition(2, new Vector3(chunkSize, 0, chunkSize));
        lineRenderer.SetPosition(3, new Vector3(0, 0, chunkSize));
        lineRenderer.SetPosition(4, new Vector3(0, 0, 0));

        lineRenderer.SetPosition(5, new Vector3(0, chunkSize, 0));
        lineRenderer.SetPosition(6, new Vector3(chunkSize, chunkSize, 0));
        lineRenderer.SetPosition(7, new Vector3(chunkSize, chunkSize, chunkSize));
        lineRenderer.SetPosition(8, new Vector3(0, chunkSize, chunkSize));
        lineRenderer.SetPosition(9, new Vector3(0, chunkSize, 0));

        lineRenderer.SetPosition(10, new Vector3(0, chunkSize, chunkSize));
        lineRenderer.SetPosition(11, new Vector3(0, 0, chunkSize));
        lineRenderer.SetPosition(12, new Vector3(chunkSize, 0, chunkSize));
        lineRenderer.SetPosition(13, new Vector3(chunkSize, chunkSize, chunkSize));
        lineRenderer.SetPosition(14, new Vector3(chunkSize, chunkSize, 0));
        lineRenderer.SetPosition(15, new Vector3(chunkSize, 0, 0));

        isCreated = true;
    }

    public void Highlight()
    {
        borderColor = Color.green;
        isCreated = false;
        CreateBorder(chunkSize);
    }

    public void Unhighlight()
    {
        borderColor = Color.red;
        isCreated = false;
        CreateBorder(chunkSize);
    }
}
