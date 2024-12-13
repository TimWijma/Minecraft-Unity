using UnityEngine;

public class ChunkMarker : MonoBehaviour
{
    private bool isCreated = false;

    public void CreateMarker()
    {
        if (isCreated) return;

        GameObject centerMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);

        centerMarker.transform.parent = transform;

        centerMarker.transform.localPosition = Vector3.zero;
        centerMarker.transform.localScale = new Vector3(1, 10, 1);

        Destroy(centerMarker.GetComponent<BoxCollider>());
        centerMarker.GetComponent<MeshRenderer>().material.color = Color.red;

        isCreated = true;
    }
}
