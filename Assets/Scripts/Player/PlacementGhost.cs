using Systems.Items;
using UnityEngine;

public class PlacementGhost : MonoBehaviour
{
    private Material validMat;
    private Material invalidMat;
    private GameObject currentGhost;
    private Renderer[] ghostRenderers;

    public void Initialize(Material valid, Material invalid)
    {
        validMat = valid;
        invalidMat = invalid;
    }

    public void CreateGhost(GameObject prefab)
    {
        Clear();
        currentGhost = Instantiate(prefab);

        DestroyImmediate(currentGhost.GetComponent<PickupableItems>());
        DestroyImmediate(currentGhost.GetComponent<Rigidbody>());
        foreach (var c in currentGhost.GetComponentsInChildren<Collider>()) Destroy(c);

        ghostRenderers = currentGhost.GetComponentsInChildren<Renderer>();
        currentGhost.layer = 2;
        currentGhost.SetActive(false);
    }

    public void UpdateGhost(Vector3 position, Quaternion rotation, bool isValid, bool isVisible)
    {
        if (currentGhost == null) return;

        currentGhost.SetActive(isVisible);
        if (!isVisible) return;

        currentGhost.transform.position = position;
        currentGhost.transform.rotation = rotation;

        Material targetMat = isValid ? validMat : invalidMat;
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostRenderers[i].sharedMaterial = targetMat;
        }
    }

    public void Clear()
    {
        if (currentGhost != null) Destroy(currentGhost);
        currentGhost = null;
        ghostRenderers = null;
    }
}
