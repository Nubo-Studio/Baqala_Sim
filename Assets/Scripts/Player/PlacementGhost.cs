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
        
        var scripts = currentGhost.GetComponentsInChildren<MonoBehaviour>();
        for (int i = 0; i < scripts.Length; i++) Destroy(scripts[i]);
        
        if (currentGhost.TryGetComponent(out Rigidbody rb)) Destroy(rb);
        
        var colliders = currentGhost.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) Destroy(colliders[i]);
        
        ghostRenderers = currentGhost.GetComponentsInChildren<Renderer>();
        currentGhost.layer = 2; 
        currentGhost.SetActive(false);
    }

    public void UpdateGhost(Vector3 position, Quaternion rotation, bool isValid, bool isVisible)
    {
        if (currentGhost == null) return;

        if (currentGhost.activeSelf != isVisible) currentGhost.SetActive(isVisible);
        if (!isVisible) return;

        currentGhost.transform.SetPositionAndRotation(position, rotation);

        Material targetMat = isValid ? validMat : invalidMat;
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            if (ghostRenderers[i].sharedMaterial != targetMat)
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
