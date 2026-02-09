using UnityEngine;
using DG.Tweening;

namespace Player
{
    public class PlacementGhost : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Layer to move the ghost object to (usually Ignore Raycast).")]
        [SerializeField] private int ghostLayer = 2;

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
            for (int i = scripts.Length - 1; i >= 0; i--)
            {
                if (scripts[i] == null) continue;
                DestroyImmediate(scripts[i]);
            }

            if (currentGhost.TryGetComponent(out Rigidbody rb)) 
                DestroyImmediate(rb);

            var colliders = currentGhost.GetComponentsInChildren<Collider>();
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                if (colliders[i] == null) continue;
                DestroyImmediate(colliders[i]);
            }

            ghostRenderers = currentGhost.GetComponentsInChildren<Renderer>();
            SetLayerRecursively(currentGhost, ghostLayer);

            // INSTANT Scale setting for the ghost
            currentGhost.transform.localScale = prefab.transform.lossyScale;

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

            currentGhost.transform.SetPositionAndRotation(position, rotation);

            Material targetMat = isValid ? validMat : invalidMat;
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
            if (currentGhost != null)
            {
                currentGhost.transform.DOKill();
                Destroy(currentGhost);
            }
            currentGhost = null;
            ghostRenderers = null;
        }
    }
}

