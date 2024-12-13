using UnityEngine;

public class ChunkCenterMarker : MonoBehaviour
{
    private GameObject centerMarker;

    void Start()
    {
        centerMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centerMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        centerMarker.GetComponent<Renderer>().material.color = Color.red;
    }
}
