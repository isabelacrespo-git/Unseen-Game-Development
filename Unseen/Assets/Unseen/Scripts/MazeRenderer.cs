using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] GameObject MazeCellPrefab;

    //this is the physical size of our maze cells, getting this wrong will result in overlapping
    //or visible gaps between each cell
    public float CellSize = 1f;

    [Header("Entity Spawn Settings")]
    [SerializeField] List<EntitySpawnDefinition> entitySpawnDefinitions = new List<EntitySpawnDefinition>();
    [SerializeField] Transform entityParent;
    [SerializeField] LayerMask blockedSpawnLayers;
    [SerializeField, Min(0f)] float spawnCollisionCheckRadius = 0.35f;
    [SerializeField, Min(1)] int maxSpawnAttemptsPerEntity = 10;

    private bool hasGenerated = false;
    private readonly List<Vector3> walkableCells = new List<Vector3>();

    [System.Serializable]
    public class EntitySpawnDefinition
    {
        public GameObject prefab;
        [Min(0)] public int amount = 1;
        [Tooltip("Raised placement from the maze floor (useful for floating or tall prefabs).")]
        public float yOffset = 0f;
        [Tooltip("If true, apply a random Y rotation so monsters don't all face the same way.")]
        public bool randomizeYRotation = true;
    }

    private void Start()
    {
        if (hasGenerated)
        {
            Debug.LogWarning("MazeRenderer: Maze already generated! Skipping duplicate generation.");
            return;
        }
        walkableCells.Clear();
        hasGenerated = true;
        Debug.Log($"MazeRenderer: Starting maze generation for {mazeGenerator.mazeWidth}x{mazeGenerator.mazeHeight}");
        
        MazeCell[,] maze = mazeGenerator.GetMaze();
        for(int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for(int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                Vector3 cellPosition = new Vector3((float)x * CellSize, 0f, (float)y * CellSize);

                //instantiate a new maze cell prefab as a child of the MazeRenderer object
                GameObject newCell = Instantiate(MazeCellPrefab, cellPosition, Quaternion.identity, transform);

                //get a reference to the cell's MazeCellPrefab script
                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                //determine which walls need to be active
                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;

                //bottom and right walls are deactivated by default unless we are at the bottom or right
                bool right = false;
                bool bottom = false;
                if (x == mazeGenerator.mazeWidth - 1) right = true;
                if (y == 0) bottom = true;

                mazeCell.Init(top, bottom, right, left);
                walkableCells.Add(cellPosition);
            }
        }
        mazeGenerator.OnMazeGenerationComplete();
        SpawnEntities();
    }

    void SpawnEntities()
    {
        if (entitySpawnDefinitions == null || entitySpawnDefinitions.Count == 0)
        {
            return;
        }

        if (walkableCells.Count == 0)
        {
            Debug.LogWarning("MazeRenderer: No recorded walkable cells to place entities on.");
            return;
        }

        List<Vector3> availableCells = new List<Vector3>(walkableCells);

        foreach (EntitySpawnDefinition definition in entitySpawnDefinitions)
        {
            if (definition == null || definition.prefab == null || definition.amount <= 0)
            {
                continue;
            }

            int placedCount = 0;

            while (placedCount < definition.amount)
            {
                if (!TrySpawnEntity(definition, availableCells))
                {
                    Debug.LogWarning($"MazeRenderer: Unable to place all instances of {definition.prefab.name}. Placed {placedCount}/{definition.amount}.");
                    break;
                }

                placedCount++;
            }
        }
    }

    bool TrySpawnEntity(EntitySpawnDefinition definition, List<Vector3> availableCells)
    {
        if (availableCells.Count == 0)
        {
            return false;
        }

        int attempts = 0;
        while (availableCells.Count > 0 && attempts < maxSpawnAttemptsPerEntity)
        {
            int index = Random.Range(0, availableCells.Count);
            Vector3 candidate = availableCells[index];
            availableCells.RemoveAt(index);

            Vector3 spawnPos = candidate + Vector3.up * definition.yOffset;
            if (IsPositionBlocked(spawnPos))
            {
                attempts++;
                continue;
            }

            Quaternion rotation = definition.randomizeYRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : definition.prefab.transform.rotation;

            Instantiate(definition.prefab, spawnPos, rotation, entityParent);
            return true;
        }

        return false;
    }

    bool IsPositionBlocked(Vector3 position)
    {
        if (spawnCollisionCheckRadius <= 0f || blockedSpawnLayers == 0)
        {
            return false;
        }

        return Physics.CheckSphere(position, spawnCollisionCheckRadius, blockedSpawnLayers, QueryTriggerInteraction.Ignore);
    }
}
