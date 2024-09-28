using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElement : MonoBehaviour
{
    // Constants
    private const float RayLength = 1;

    // Properties
    public Transform Pivot { get => pivot; }
    public Vector2 GridSize { get => gridSize; }

    //Private variables
    [SerializeField] private MeshRenderer gridMeshRenderer;
    [SerializeField] private Transform pivot;
    [SerializeField] private LayerMask gridLayerMask;
    [SerializeField] private Vector2 gridSize;

    private bool isGrabbed;

    private void Start()
    {
        // FOR DEBUG PURPOSES
        isGrabbed = true;
    }

    private void Update()
    {
        if (!isGrabbed) return;

        SnapToGrid();

    }

    private void SnapToGrid()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, -transform.up);

        if (!Physics.Raycast(ray, out hit, RayLength, gridLayerMask))
        {
            gridMeshRenderer.enabled = false;
            return;
        }

        gridMeshRenderer.enabled = true;
        gridMeshRenderer.transform.position = new Vector3(transform.position.x, hit.transform.position.y, transform.position.z);

        // Calculate pivot desired location within the grid
        Vector3 pivotRelativePositionToGrid = pivot.position - Grid.Instance.Pivot.position;
        Vector3 pivotGridPosition = new Vector3(Mathf.Round(pivotRelativePositionToGrid.x / Grid.Instance.tileSize), 0, Mathf.Round(pivotRelativePositionToGrid.z / Grid.Instance.tileSize));
        Vector3 pivotTargetPosition = pivotGridPosition * Grid.Instance.tileSize + pivot.position - pivotRelativePositionToGrid;

        // Calculate and set element's new position
        Vector3 finalPosition = transform.position + pivotTargetPosition - pivot.position;
        transform.position = new Vector3(finalPosition.x, transform.position.y, finalPosition.z);

        if ((Grid.Instance.CheckTilesAvailability(this)))
        {
            gridMeshRenderer.materials[0].color = Color.blue;
        }
        else
        {
            gridMeshRenderer.materials[0].color = Color.red;
        }
    }
}
