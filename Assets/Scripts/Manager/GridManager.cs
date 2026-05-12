using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private List<GameObject> gridObjects = new List<GameObject>();

    private void Awake()
    {
        // Singleton pattern to access it easily from any script
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // Optional: Auto-fill the list if you forgot to drag them in
        if (gridObjects.Count == 0)
        {
            gridObjects.AddRange(GameObject.FindGameObjectsWithTag("Grid"));
        }
    }

    public void ToggleGrid(bool visible)
    {
        foreach (var grid in gridObjects)
        {
            if (grid != null) grid.SetActive(visible);
        }
    }
}