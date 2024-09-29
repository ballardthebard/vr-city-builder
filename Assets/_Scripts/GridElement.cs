using Meta.WitAi;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System;
using UnityEngine;

public class GridElement : MonoBehaviour
{
    // Constants
    private const float RayLength = 1;
    private const float GridDistanceTolerance = 0.03f;

    // Properties
    public Transform Pivot { get => pivot; }
    public Vector2 GridSize { get => gridSize; }

    //Private variables
    [SerializeField] private Transform preview;
    [SerializeField] private Transform pivot;
    [SerializeField] private LayerMask gridLayerMask;
    [SerializeField] private Vector2 gridSize;

    private GrabInteractable grabInteractable;
    private HandGrabInteractable handGrabInteractable;
    private Vector3 lastGridPosition;
    private bool grabbedOneHand;
    private bool grabbedTwoHand;
    private bool isInsideGrid;
    private bool isPlaced;
    private float mainGridHeight;
    private MeshRenderer[] previewMeshRenderers;

    private void Start()
    {
        grabInteractable = GetComponentInParent<GrabInteractable>();
        handGrabInteractable = GetComponentInParent<HandGrabInteractable>();
        previewMeshRenderers = preview.GetComponentsInChildren<MeshRenderer>();

        // Register grab events
        grabInteractable.WhenSelectingInteractorViewAdded += AddInteractor;
        grabInteractable.WhenSelectingInteractorViewRemoved += RemoveInteractor;
        handGrabInteractable.WhenSelectingInteractorViewAdded += AddInteractor;
        handGrabInteractable.WhenSelectingInteractorViewRemoved += RemoveInteractor;
        
        preview.parent = null;
    }

    private void Update()
    {
        if (!grabbedOneHand) return;
        if (!IsInsideGrid()) return;

        UpdatePreview();
    }

    private void AddInteractor(IInteractorView view)
    {
        OnGrab();
    }
    private void RemoveInteractor(IInteractorView view)
    {
        OnRelease();
    }

    private void OnGrab()
    {
        if (grabbedOneHand)
        {
            grabbedTwoHand = true;
        }
        else
        {
            grabbedOneHand = true;

            if (!isPlaced) return;

            Grid.Instance.UpdateTile(false, this);
            isPlaced = false;
        }
    }

    private void OnRelease()
    {
        if (!grabbedTwoHand)
        {
            grabbedOneHand = false;

            if (!IsInsideGrid())
            {
                gameObject.SetActive(false);
                return; 
            }

            if (!Grid.Instance.CheckTilesAvailability(this)) 
            {
                preview.gameObject.SetActive(false);
                return;
            }

            Grid.Instance.UpdateTile(true, this);
            isPlaced = true;
        }
        else
        {
            // Update rotation to fit a 90 degrees interval
            Vector3 newRotation = new Vector3(0, Mathf.Round(transform.parent.eulerAngles.y / 90.0f) * 90.0f, 0);
            transform.parent.eulerAngles = newRotation;
            grabbedTwoHand = false;
        }
    }

    private void UpdatePreview()
    {           
        // Update preview visuals
        if (Grid.Instance.CheckTilesAvailability(this))
        {
            foreach (MeshRenderer meshRenderer in previewMeshRenderers)
            {
                meshRenderer.materials[0].color = Color.blue;
            }
        }
        else
        {
            foreach (MeshRenderer meshRenderer in previewMeshRenderers)
            {
                meshRenderer.materials[0].color = Color.red;
            }
        }

        // Calculate pivot desired location within the grid
        Vector3 pivotRelativePositionToGrid = pivot.position - Grid.Instance.Pivot.position;
        Vector3 pivotGridPosition = new Vector3(Mathf.Round(pivotRelativePositionToGrid.x / Grid.Instance.tileSize), 0, Mathf.Round(pivotRelativePositionToGrid.z / Grid.Instance.tileSize));
        Vector3 pivotTargetPosition = pivotGridPosition * Grid.Instance.tileSize + pivot.position - pivotRelativePositionToGrid;

        // Calculate grid new position
        Vector3 finalPosition = transform.position + pivotTargetPosition - pivot.position;
        if (Vector3.Distance(lastGridPosition, finalPosition) >= GridDistanceTolerance)
            lastGridPosition = finalPosition;

        // Update preview position
        preview.position = new Vector3(lastGridPosition.x, mainGridHeight, lastGridPosition.z);

        // Update preview rotation to fit a 90 degrees interval
        if (grabbedTwoHand)
        {
            Vector3 newRotation = new Vector3(0, Mathf.Round(transform.parent.eulerAngles.y / 90.0f) * 90.0f, 0);
            preview.eulerAngles = newRotation;
        }
    }

    private bool IsInsideGrid()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, -transform.up);

        // Left grid
        if (!Physics.Raycast(ray, out hit, RayLength, gridLayerMask))
        {
            preview.gameObject.SetActive(false);
            isInsideGrid = false;
            return false;
        }
        // Entered grid
        else if (!isInsideGrid)
        {
            mainGridHeight = hit.collider.transform.position.y;
            isInsideGrid = true;
            preview.gameObject.SetActive(true);
        }

        return true;
    }
}
