using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    // Constants
    private const int RotationStep = 90;

    // Static variables
    public static Grid Instance;

    // Public variables
    public float tileSize;
    public int xSize;
    public int ySize;

    // Properties
    public Transform Pivot { get => pivot; }

    // Private variables
    [SerializeField] private Transform initialLayout;
    [SerializeField] private Transform pivot;
    private GridElement[,] tiles;

    private void Awake()
    {
        // Initialize singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        // Initialize tiles
        tiles = new GridElement[xSize, ySize];

        // Fill tiles with objects already on the layout
        SetInitialLayout();
        DontDestroyOnLoad(this);
    }

    public void UpdateTile(bool isPlacing, GridElement gridElement)
    {
        Action<int, int> updateAction = (xGrid, yGrid) =>
        {
            if (isPlacing)
                tiles[xGrid, yGrid] = gridElement;
            else
                tiles[xGrid, yGrid] = null;
        };

        IterateGridTiles(gridElement, updateAction);
    }

    public bool CheckTilesAvailability(GridElement gridElement)
    {
        bool isAvailable = true;

        Action<int, int> checkAction = (xGrid, yGrid) =>
        {
            if (xGrid < 0 || yGrid < 0 || xGrid >= xSize || yGrid >= ySize)
            {
                isAvailable = false;
            }
            else if (tiles[xGrid, yGrid] != null)
            {
                isAvailable = false;
            }
        };

        IterateGridTiles(gridElement, checkAction);

        return isAvailable;
    }

    public Vector3 GetPositionInsideGrid(GridElement gridElement)
    {
        // Calculate pivot desired location within the grid
        Vector3 pivotRelativePositionToGrid = gridElement.Pivot.position - pivot.position;
        Vector3 pivotGridPosition = new Vector3(Mathf.Round(pivotRelativePositionToGrid.x / tileSize), pivot.position.y, Mathf.Round(pivotRelativePositionToGrid.z / tileSize));
        Vector3 pivotTargetPosition = pivotGridPosition * tileSize + gridElement.Pivot.position - pivotRelativePositionToGrid;

        // Return final world position 
        return gridElement.transform.position + pivotTargetPosition - gridElement.Pivot.position;
    }

    private void IterateGridTiles(GridElement gridElement, Action<int, int> tileAction)
    {
        // Precompute rotationOffset and index values before the loop
        Vector2 index = GetElementPivotGridPosition(gridElement);
        int baseX = (int)index.x;
        int baseY = (int)index.y;

        int rotationOffset = GetRotationOffset(gridElement);
        bool isEvenRotation = (rotationOffset % 2 == 0);

        int xGrid;
        int yGrid;

        for (int i = 0; i < gridElement.GridSize.x; i++)
        {
            for (int j = 0; j < gridElement.GridSize.y; j++)
            {
                if (isEvenRotation)
                {
                    xGrid = baseX + i;
                    yGrid = baseY + j;
                }
                else
                {
                    xGrid = baseX + j;
                    yGrid = baseY + i;
                }

                tileAction(xGrid, yGrid);
            }
        }
    }

    private Vector2 GetElementPivotGridPosition(GridElement gridElement)
    {
        // Calculate the element's pivot's position on grid
        Vector3 elementPivotRelativePosition = gridElement.Pivot.position - pivot.position;
        Vector2 elementPivotGridPosition = new Vector3(Mathf.Round(elementPivotRelativePosition.x / tileSize), Mathf.Round(elementPivotRelativePosition.z / tileSize));

        // Adjust offset caused by rotation
        int rotationOffset = GetRotationOffset(gridElement);
        switch (rotationOffset)
        {
            case 1:
                return new Vector2(elementPivotGridPosition.x, elementPivotGridPosition.y - gridElement.GridSize.x);

            case 2:
                return new Vector2(elementPivotGridPosition.x - gridElement.GridSize.x, elementPivotGridPosition.y - gridElement.GridSize.y);

            case 3:
                return new Vector2(elementPivotGridPosition.x - gridElement.GridSize.y, elementPivotGridPosition.y);

            case -1:
                return new Vector2(elementPivotGridPosition.x, elementPivotGridPosition.y + gridElement.GridSize.x);

            case -2:
                return new Vector2(elementPivotGridPosition.x + gridElement.GridSize.x, elementPivotGridPosition.y + gridElement.GridSize.y);

            case -3:
                return new Vector2(elementPivotGridPosition.x + gridElement.GridSize.y, elementPivotGridPosition.y);
        }

        return elementPivotGridPosition;
    }

    private void SetInitialLayout()
    {
        GridElement[] gridElements = initialLayout.GetComponentsInChildren<GridElement>();

        foreach (GridElement gridElement in gridElements)
        {
            Vector3 finalPosition = GetPositionInsideGrid(gridElement);
            finalPosition.y = pivot.position.y;

            gridElement.transform.parent = null;
            gridElement.transform.position = finalPosition;
            gridElement.ToggleChildren();
            gridElement.Animator.SetTrigger("StartOnScene");
            gridElement.isPlaced = true;
            UpdateTile(true, gridElement);
        }
    }

    private int GetRotationOffset(GridElement gridElement) => Mathf.RoundToInt(gridElement.transform.rotation.eulerAngles.y / RotationStep);
}
