using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    // Static variables
    public static Grid Instance;

    // Public variables
    public float tileSize;
    public int xSize;
    public int ySize;

    // Properties
    public Transform Pivot { get => pivot; }

    // Private variables
    [SerializeField] private Transform pivot;
    private GridElement[][] tiles;

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

        tiles = new GridElement[xSize][];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new GridElement[ySize];
        }
    }

    public void UpdateTile(bool isPlacing, GridElement gridElement)
    {
        Vector2 index = GetElementPivotGridPosition(gridElement);
        int rotationOffset = Mathf.RoundToInt(gridElement.transform.rotation.eulerAngles.y / 90);

        int xGrid;
        int yGrid;

        // Only update tiles the element is hoovering
        for (int i = 0; i < gridElement.GridSize.x; i++)
        {
            for (int j = 0; j < gridElement.GridSize.y; j++)
            {
                // Swap axis depending on element rotation
                if (rotationOffset % 2 == 0)
                {
                    xGrid = (int)index.x + i;
                    yGrid = (int)index.y + j;
                }
                else
                {
                    xGrid = (int)index.x + j;
                    yGrid = (int)index.y + i;
                }

               if(isPlacing)
                    tiles[xGrid][yGrid] = gridElement;
               else
                    tiles[xGrid][yGrid] = null;
            }
        }
    }

    public bool CheckTilesAvailability(GridElement gridElement)
    {
        Vector2 index = GetElementPivotGridPosition(gridElement);
        int rotationOffset = Mathf.RoundToInt(gridElement.transform.rotation.eulerAngles.y / 90);

        if (index.x < 0 || index.y < 0) return false;

        int xGrid;
        int yGrid;

        // Only check tiles the element is hoovering
        for (int i = 0; i < gridElement.GridSize.x; i++)
        {
            for (int j = 0; j < gridElement.GridSize.y; j++)
            {
                // Swap axis depending on element rotation
                if (rotationOffset % 2 == 0)
                {
                    xGrid = (int)index.x + i;
                    yGrid = (int)index.y + j;
                }
                else
                {
                    xGrid = (int)index.x + j;
                    yGrid = (int)index.y + i;
                }

                if (xGrid >= xSize || yGrid >= ySize) return false;
                if (tiles[xGrid][yGrid] != null) return false;
            }
        }

        return true;
    }

    public Vector2 GetElementPivotGridPosition(GridElement gridElement)
    {
        // Calculate the element's pivot's position on grid
        Vector3 elementPivotRelativePosition = gridElement.Pivot.position - pivot.position;
        Vector2 elementPivotGridPosition = new Vector3(Mathf.Round(elementPivotRelativePosition.x / tileSize), Mathf.Round(elementPivotRelativePosition.z / tileSize));

        // Adjust offset caused by rotation
        int rotationOffset = Mathf.RoundToInt(gridElement.transform.rotation.eulerAngles.y / 90);
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
}
