using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElement : MonoBehaviour
{
    private const float RayLength = 3;

    [SerializeField] private Transform pivot;
    [SerializeField] private LayerMask gridLayerMask;

    private MeshRenderer gridRenderer;
    private MeshRenderer mesh;
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

        if (!Physics.Raycast(ray, out hit, RayLength, gridLayerMask)) return;

        // Calculate pivot desired location within the grid
        Vector3 pivotRelativePositionToGrid = pivot.position - Grid.Instance.Pivot.position;
        Vector3 tileKey = new Vector3(Mathf.Round(pivotRelativePositionToGrid.x / Grid.Instance.tileSize), 0, Mathf.Round(pivotRelativePositionToGrid.z / Grid.Instance.tileSize));
        Vector3 pivotTargetPosition = tileKey * Grid.Instance.tileSize + pivot.position - pivotRelativePositionToGrid;

        // Calculate and set element's new position
        Vector3 finalPosition = transform.position + pivotTargetPosition - pivot.position;
        transform.position = finalPosition;

        Debug.DrawRay(transform.position, transform.forward * RayLength, Color.red);

        print(tileKey);
    }
}
