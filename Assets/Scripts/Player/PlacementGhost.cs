using UnityEngine;

namespace Player
{
    public class PlacementGhost : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Layer to move the ghost object to (usually Ignore Raycast).")]
        [SerializeField] private int ghostLayer = 2; // Ignore Raycast

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

            // Strip unnecessary components to make it purely visual

            // 1. Remove Scripts
            var scripts = currentGhost.GetComponentsInChildren<MonoBehaviour>();
            foreach (var script in scripts) DestroyImmediate(script);

            // 2. Remove Rigidbody
            if (currentGhost.TryGetComponent(out Rigidbody rb)) DestroyImmediate(rb);

            // 3. Remove Colliders
            var colliders = currentGhost.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) DestroyImmediate(col);

            // 4. Cache Renderers
            ghostRenderers = currentGhost.GetComponentsInChildren<Renderer>();

            // 5. Set Layer
            SetLayerRecursively(currentGhost, ghostLayer);

            currentGhost.SetActive(false);
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
        }

        public void UpdateGhost(Vector3 position, Quaternion rotation, bool isValid, bool isVisible)
        {
            if (currentGhost == null) return;

            if (currentGhost.activeSelf != isVisible)
                currentGhost.SetActive(isVisible);

            if (!isVisible) return;

            // Use SetPositionAndRotation for atomicity
            currentGhost.transform.SetPositionAndRotation(position, rotation);

            Material targetMat = isValid ? validMat : invalidMat;

            // Only update material if changed (optimization)
            if (ghostRenderers != null && ghostRenderers.Length > 0 && ghostRenderers[0].sharedMaterial != targetMat)
            {
                for (int i = 0; i < ghostRenderers.Length; i++)
                {
                    ghostRenderers[i].sharedMaterial = targetMat;
                }
            }
        }

        public void Clear()
        {
            if (currentGhost != null) Destroy(currentGhost);
            currentGhost = null;
            ghostRenderers = null;
        }
    }
}