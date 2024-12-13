using UnityEngine;

public class ChunkData : MonoBehaviour
{
    private float chunkSize;
    public float ChunkSize => chunkSize;

    public void Initialize(float chunkSize)
    {
        this.chunkSize = chunkSize;
    }
}
