using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections;
using TMPro;
using UnityEngine;

public class GridElement : MonoBehaviour
{
    // Constants
    private const float RayLength = 1;
    private const float GridDistanceTolerance = 0.03f;
    private const float CanvasSpeed = 3f;
    private const float CanvasDistanceThreshold = 0.1f;

    // Properties
    public Transform Pivot { get => pivot; }
    public Vector2 GridSize { get => gridSize; }

    //Private variables
    [Header("Preview")]
    [SerializeField] private Color allowedPlacement;
    [SerializeField] private Color disallowedPlacement;
    [SerializeField] private Transform preview;

    [Header("Grid Verification")]
    [SerializeField] private Transform pivot;
    [SerializeField] private LayerMask gridLayerMask;
    [SerializeField] private Vector2 gridSize;

    [Header("Feedback")]
    [SerializeField] private GameObject element;
    [SerializeField] private Transform previewAnimation;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Vector3 canvasTargetPosition;

    private Animator animator;
    private GrabInteractable grabInteractable;
    private HandGrabInteractable handGrabInteractable;
    private Vector3 lastGridPosition;
    private bool grabbedOneHand;
    private bool grabbedTwoHand;
    private bool isInsideGrid;
    private bool isPlaced;
    private float mainGridHeight;
    private MeshRenderer[] previewMeshRenderers;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        grabInteractable = GetComponent<GrabInteractable>();
        handGrabInteractable = GetComponent<HandGrabInteractable>();
        previewMeshRenderers = preview.GetComponentsInChildren<MeshRenderer>();
    }

    private void OnEnable()
    {
        // Register grab events
        grabInteractable.WhenSelectingInteractorViewAdded += OnGrab;
        grabInteractable.WhenSelectingInteractorViewRemoved += OnRelease;
        handGrabInteractable.WhenSelectingInteractorViewAdded += OnGrab;
        handGrabInteractable.WhenSelectingInteractorViewRemoved += OnRelease;
    }

    private void OnDisable()
    {
        // Unregister grab events
        grabInteractable.WhenSelectingInteractorViewAdded -= OnGrab;
        grabInteractable.WhenSelectingInteractorViewRemoved -= OnRelease;
        handGrabInteractable.WhenSelectingInteractorViewAdded -= OnGrab;
        handGrabInteractable.WhenSelectingInteractorViewRemoved -= OnRelease;
    }

    private void Update()
    {
        if (!grabbedOneHand) return;
        UpdatePreviewRotation();

        if (!IsInsideGrid()) return;
        UpdatePreviewPositionAndVisuals();
    }

    public void ToggleChilds() 
    {
        previewAnimation.gameObject.SetActive(!previewAnimation.gameObject.activeSelf);
        element.SetActive(!element.activeSelf);
        canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
    }

    private void UpdatePreviewRotation()
    {
        // Update preview rotation to fit a 90 degrees interval
        if (grabbedTwoHand)
        {
            Vector3 newRotation = new Vector3(0, Mathf.Round(transform.eulerAngles.y / 90.0f) * 90.0f, 0);
            preview.eulerAngles = newRotation;
        }
    }

    private void UpdatePreviewPositionAndVisuals()
    {
        // Update preview visuals
        if (Grid.Instance.CheckTilesAvailability(this))
        {
            foreach (MeshRenderer meshRenderer in previewMeshRenderers)
            {
                meshRenderer.materials[0].color = allowedPlacement;
            }
        }
        else
        {
            foreach (MeshRenderer meshRenderer in previewMeshRenderers)
            {
                meshRenderer.materials[0].color = disallowedPlacement;
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
    }

    private bool IsInsideGrid()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, -transform.up);

        // Left grid
        if (!Physics.Raycast(ray, out hit, RayLength, gridLayerMask))
        {
            print("Left Grid");
            preview.gameObject.SetActive(false);
            isInsideGrid = false;
            return false;
        }
        // Entered grid
        else if (!isInsideGrid)
        {
            print("Entered Grid");
            mainGridHeight = hit.collider.transform.position.y;
            isInsideGrid = true;
            preview.gameObject.SetActive(true);

        }

        return true;
    }

    private void PlaceOnGrid()
    {
        if (!Grid.Instance.CheckTilesAvailability(this))
        {
            DestroyElement();
            return;
        }

        // Place element on grid
        StartCoroutine(PlacedFeedback());
        Grid.Instance.UpdateTile(true, this);
        isPlaced = true;
    }


    private void DestroyElement()
    {
        transform.gameObject.SetActive(false);
        Destroy(preview.gameObject);
        Destroy(transform.gameObject);

    }

    private void OnGrab(IInteractorView view)
    {
        if (grabbedOneHand)
        {
            grabbedTwoHand = true;
        }
        else
        {
            // Remove grabbable from spawn point
            if (transform.parent != null)
                transform.parent = null;

            // Unparent preview so that it isn't directly affected by grab
            // and reset any rotation caused by moving the UI
            if (preview.parent != null)
            {
                preview.parent = null;
                preview.rotation = Quaternion.identity;
                transform.rotation = Quaternion.identity;
            }

            grabbedOneHand = true;

            if (!isPlaced) return;

            // Remove element from grid
            Grid.Instance.UpdateTile(false, this);
            animator.SetBool("Placed", false);
            isPlaced = false;

            // Reset preview and canvas
            canvas.localPosition = Vector3.zero;
            preview.localScale = Vector3.one;
            ToggleChilds();
        }
    }

    private void OnRelease(IInteractorView view)
    {
        if (!grabbedTwoHand)
        {
            grabbedOneHand = false;

            PlaceOnGrid();
        }
        else
        {
            // Update rotation to fit a 90 degrees interval
            Vector3 newRotation = new Vector3(0, Mathf.Round(transform.eulerAngles.y / 90.0f) * 90.0f, 0);
            transform.eulerAngles = newRotation;
            grabbedTwoHand = false;
        }
    }

    private IEnumerator PlacedFeedback()
    {
        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        yield return waitForEndOfFrame;

        Vector3 canvasOriginalLocalPosition = canvas.position;

        transform.position = preview.position;
        transform.rotation = preview.rotation;
        canvas.position = canvasOriginalLocalPosition;
        preview.parent = previewAnimation;

        animator.SetBool("Repositioning", true);

        yield return waitForEndOfFrame;

        while (Vector3.Distance(canvas.localPosition, canvasTargetPosition) > CanvasDistanceThreshold)
        {
            canvas.localPosition = Vector3.Lerp(canvas.localPosition, canvasTargetPosition, CanvasSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Repositioning", false);
    }
}
