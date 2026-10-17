using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject manualLevel;
    [SerializeField] private GameObject[] tilePrefabs = new GameObject[9];

    [SerializeField] private Camera levelCamera;

    [SerializeField] private float cameraPadding = 1.5f;

    private void Start()
    {
        if(manualLevel != null)
        {
            manualLevel.SetActive(false);
        }
            GenerateLevel();
            AdjustCamera();
    }

    private void GenerateLevel()
    {
        int rows = levelMap.GetLength(0);
        int columns = levelMap.GetLength(1);

        GameObject generatedLevel = new GameObject("GeneratedLevel");

        generatedLevel.transform.SetParent(transform,false);

        CreateQuadrant(generatedLevel.transform, "TopLeft",new Vector3(-columns + 0.5f, rows - 1f, 0f), new Vector3(1f, 1f, 1f), true);

        CreateQuadrant(generatedLevel.transform, "TopRight",new Vector3(columns - 0.5f, rows - 1f, 0f), new Vector3(-1f, 1f, 1f), true);

        CreateQuadrant(generatedLevel.transform,"BottomLeft",new Vector3(-columns + 0.5f, -rows + 1f, 0f),new Vector3(1f, -1f, 1f),false);

        CreateQuadrant(generatedLevel.transform,"BottomRight",new Vector3(columns - 0.5f, -rows + 1f, 0f),new Vector3(-1f, -1f, 1f),false);


    }

    private void CreateQuadrant(Transform levelParent, string quadrantName, Vector3 quadrantPosition, Vector3 quadrantScale, bool includeLastRow)
    {
        GameObject quadrant = new GameObject(quadrantName);
        quadrant.transform.SetParent(levelParent, false);
        quadrant.transform.localPosition = quadrantPosition;
        quadrant.transform.localScale = quadrantScale;

        int rows = levelMap.GetLength(0);

        if(!includeLastRow)
        {
            rows = rows - 1;
        }

        int columns = levelMap.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for(int column = 0; column < columns; column++)
            {
                int tileType = levelMap[row, column];

                if(tileType == 0)
                {
                    continue;
                }

                GameObject newTile = Instantiate(tilePrefabs[tileType], quadrant.transform);
                newTile.name = $"Row_{row}_Column_{column}_Type_{tileType}";

                newTile.transform.localPosition = new Vector3(
                    column,
                    -row,
                    0f
                );
                float rotation = GetTileRotation(row, column, tileType);
                newTile.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

            }
        }
    }

    private bool IsWall(int tileType)
    {
        return tileType == 1
            || tileType == 2
            || tileType == 3
            || tileType == 4
            || tileType == 7
            || tileType == 8;
    }

    private bool HasWall(int row, int column)
    {
       int rows = levelMap.GetLength(0);
    int columns = levelMap.GetLength(1);

    if (row < 0 || row >= rows)
    {
        return false;
    }

    if (column < 0 || column >= columns)
    {
        return false;
    }

    int neighbourType = levelMap[row, column];

    return IsWall(neighbourType);
    }

    private float GetTileRotation(int row, int column, int tileType)
    {
        bool up = HasWall(row - 1, column);
        bool down = HasWall(row + 1 , column );
        bool left = HasWall(row, column - 1);
        bool right = HasWall(row, column + 1);

        if(tileType == 2 || tileType == 4 || tileType == 8)
        {
           int verticalWalls = 0;
           int horizontalWalls = 0;

            if (up)
            {
                verticalWalls++;
            }
            if (down)
            {
                verticalWalls++;
            }
            if (left)
            {
                horizontalWalls++;
            }
            if (right)
            {
                horizontalWalls++;
            }
            if( verticalWalls > horizontalWalls)
            {
                return 90f;
            }
                return 0f;

        }

        if (tileType == 1 || tileType == 3)
        {
            if (up && right && down && left)
            {
                if(row +1 < levelMap.GetLength(0) && levelMap[row + 1, column] == 3)
                {
                    return 90f;
                }
                return 0f;

            }

            if(right&& down)
            {
                return 0f;
            }
            if(up && right)
            {
                return 90f;
            }
            if(down && left)
            {
                return 270f;
            }

            if(up && left)
            {
                return 180f;
            }

            if(column ==levelMap.GetLength(1) -1 && up)
            {
                return 90f;
            }
            return 0f;

        }

        if(tileType == 7)
        {
            if(column == levelMap.GetLength(1)-1)
            {
                right = true;

            }

            if(up && right && down && !left)
            {
                return 90f;
            }

            if (up && right && left && !down)
        {
            return 180f;
        }

        if (up && down && left && !right)
        {
            return 270f;
        }

        return 0f;
        }
        return 0f;
    }

    private void AdjustCamera()
    {
        if(levelCamera == null)
        {
            return;
        }

        int rows = levelMap.GetLength(0);
        int columns = levelMap.GetLength(1);

        float verticalSize = rows - 0.5f;
        float horizontalSize = columns/levelCamera.aspect;
        float requiredSize = Mathf.Max(verticalSize,horizontalSize);

    levelCamera.transform.position = new Vector3(0f, 0f, levelCamera.transform.position.z);

        levelCamera.orthographicSize = requiredSize + cameraPadding;
    }

    public bool CanPlayerEnter(Vector2 worldCenter)
    {
        int column = Mathf.RoundToInt(13.5f - Mathf.Abs(worldCenter.x));
        int row = Mathf.RoundToInt(14f - Mathf.Abs(worldCenter.y));

        if (row < 0 || row >= levelMap.GetLength(0) ||
            column < 0 || column >= levelMap.GetLength(1))
        {
            return false;
        }

        int tileType = levelMap[row, column];
        return tileType == 5 || tileType == 6 ||
        (row == 14 && column <= 5);
    }

    public bool IsTunnelExit(Vector2 worldCenter, Vector2Int direction)
    {
        if (Mathf.Abs(worldCenter.y) > 0.01f)
            return false;

        bool leftExit = Mathf.Abs(worldCenter.x + 13.5f) < 0.01f
                        && direction == Vector2Int.left;
        bool rightExit = Mathf.Abs(worldCenter.x - 13.5f) < 0.01f
                        && direction == Vector2Int.right;

        return leftExit || rightExit;
    }

   private readonly int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0}
    };

}
