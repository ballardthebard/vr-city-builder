using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    // Static variables
    public static Grid Instance;

    // Public variables
    public float tileSize;
    public Vector2 gridSize;

    // Properties
    public Transform Pivot { get => pivot; }

    // Private variables
    [SerializeField] private Transform pivot;
    private Dictionary<Vector2, GridElement> tiles;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }

        tiles = new Dictionary<Vector2, GridElement>();
    }

    public void OccupySpots() { }
    public void DisoccupySpots() { }
    public void CheckSpotAvailability(GridElement gridElement)
    {
    
    }
}
