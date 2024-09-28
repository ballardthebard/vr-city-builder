using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private float squareSize;

    private Dictionary<Vector2, Object> spots;

    void Start()
    {
        spots = new Dictionary<Vector2, Object>();
    }

    public void OccupySpots() { }
    public void DisoccupySpots() { }
    public void CheckSpotAvailability() { }
}
