using UnityEngine;

public class ChunkData : MonoBehaviour
{
    private int chunkSize;
    public int ChunkSize => chunkSize;

    public void Initialize(int chunkSize)
    {
        this.chunkSize = chunkSize;
    }
}
