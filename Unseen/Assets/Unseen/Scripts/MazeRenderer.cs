using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
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
    [Header("Player Reference")]
    [SerializeField] Transform playerTransform;
    [Header("Equipment Spawning")]
    [SerializeField] GameObject flashlightPrefab;
    [SerializeField] Vector3 flashlightSpawnOffset = new Vector3(0f, 0f, 0.8f);
    [SerializeField] GameObject tablePrefab;
    [Tooltip("Forward distance from the player to spawn the table, along their facing direction.")]
    [SerializeField] float tableForwardDistance = 1.2f;
    [Tooltip("Vertical offset when placing the table.")]
    [SerializeField] float tableHeightOffset = 0f;

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
        [Tooltip("Minimum spacing between spawned instances of this prefab.")]
        [Min(0f)] public float minSpacing = 0f;
        [Tooltip("Minimum distance away from the player this entity can spawn.")]
        [Min(0f)] public float minDistanceFromPlayer = 0f;
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
        if (playerTransform == null)
        {
            playerTransform = ResolvePlayerTransform();
        }

        SpawnEquipment();

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
            List<Vector3> placedPositions = new List<Vector3>();

            while (placedCount < definition.amount)
            {
                if (!TrySpawnEntity(definition, availableCells, placedPositions, out Vector3 placedPosition))
                {
                    Debug.LogWarning($"MazeRenderer: Unable to place all instances of {definition.prefab.name}. Placed {placedCount}/{definition.amount}.");
                    break;
                }

                placedPositions.Add(placedPosition);
                placedCount++;
            }
        }
    }

    bool TrySpawnEntity(EntitySpawnDefinition definition, List<Vector3> availableCells, List<Vector3> placedPositions, out Vector3 placedPosition)
    {
        placedPosition = Vector3.zero;

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

            if (!HasRequiredSpacing(spawnPos, placedPositions, definition.minSpacing))
            {
                attempts++;
                continue;
            }

            if (!MeetsPlayerDistance(spawnPos, definition.minDistanceFromPlayer))
            {
                attempts++;
                continue;
            }

            Quaternion rotation = definition.randomizeYRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : definition.prefab.transform.rotation;

            Instantiate(definition.prefab, spawnPos, rotation, entityParent);
            placedPosition = spawnPos;
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

    bool HasRequiredSpacing(Vector3 candidate, List<Vector3> placedPositions, float minSpacing)
    {
        if (minSpacing <= 0f || placedPositions == null || placedPositions.Count == 0)
            return true;

        float minSpacingSqr = minSpacing * minSpacing;
        foreach (var pos in placedPositions)
        {
            if ((candidate - pos).sqrMagnitude < minSpacingSqr)
                return false;
        }

        return true;
    }

    bool MeetsPlayerDistance(Vector3 spawnPos, float minDistance)
    {
        if (minDistance <= 0f || playerTransform == null)
            return true;

        Vector3 playerPos = playerTransform.position;
        Vector3 spawnFlat = new Vector3(spawnPos.x, 0f, spawnPos.z);
        Vector3 playerFlat = new Vector3(playerPos.x, 0f, playerPos.z);
        return Vector3.SqrMagnitude(spawnFlat - playerFlat) >= minDistance * minDistance;
    }

    Transform ResolvePlayerTransform()
    {
        XROrigin xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null)
            return xrOrigin.transform;

        GameObject taggedPlayer = GameObject.FindWithTag("Player");
        if (taggedPlayer != null)
            return taggedPlayer.transform;

        Camera cam = Camera.main;
        return cam != null ? cam.transform : null;
    }

    void SpawnEquipment()
    {
        if (playerTransform == null) return;

        Transform tableTransform = null;

        if (tablePrefab != null)
        {
            Vector3 forwardSpawn = playerTransform.position + playerTransform.forward * tableForwardDistance;
            tableTransform = SpawnTableAgainstWall(forwardSpawn);
        }

        if (flashlightPrefab != null)
        {
            if (tableTransform != null)
            {
                Vector3 top = tableTransform.position;
                Collider tableCollider = tableTransform.GetComponent<Collider>();
                if (tableCollider != null)
                {
                    top.y = tableCollider.bounds.max.y;
                }
                Vector3 spawnPos = top + tableTransform.right * flashlightSpawnOffset.x
                    + tableTransform.up * (flashlightSpawnOffset.y + 0.02f)
                    + tableTransform.forward * flashlightSpawnOffset.z;

                Quaternion spawnRot = Quaternion.LookRotation(tableTransform.forward, Vector3.up);
                Instantiate(flashlightPrefab, spawnPos, spawnRot);
            }
            else
            {
                SpawnPrefabNearPlayer(flashlightPrefab, flashlightSpawnOffset, 0.05f);
            }
        }
    }

    Transform SpawnPrefabNearPlayer(GameObject prefab, Vector3 offset, float liftHeight)
    {
        if (prefab == null || playerTransform == null) return null;

        Vector3 spawnPos = playerTransform.position
            + playerTransform.right * offset.x
            + Vector3.up * offset.y
            + playerTransform.forward * offset.z;

        if (Physics.Raycast(spawnPos + Vector3.up, Vector3.down, out RaycastHit hit, 5f))
        {
            spawnPos = hit.point + Vector3.up * liftHeight;
        }

        return SpawnPrefabAtPosition(prefab, spawnPos, liftHeight, playerTransform.forward, Vector3.zero);
    }

    Transform SpawnTableAgainstWall(Vector3 fallbackPosition)
    {
        if (tablePrefab == null) return null;

        Vector3 direction = playerTransform.forward.normalized;
        Vector3 origin = playerTransform.position + Vector3.up * 0.5f;
        float rayDistance = tableForwardDistance + 2f;
        Vector3 spawnPos = fallbackPosition;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, rayDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            float tableDepth = 0.5f;
            Collider prefabCollider = tablePrefab.GetComponentInChildren<Collider>();
            if (prefabCollider != null)
            {
                tableDepth = Mathf.Max(prefabCollider.bounds.extents.x, prefabCollider.bounds.extents.z);
            }

            spawnPos = hit.point - direction * (tableDepth + 0.05f);
        }

        return SpawnPrefabAtPosition(tablePrefab, spawnPos, tableHeightOffset, direction, new Vector3(-90f, 0f, 0f));
    }

    Transform SpawnPrefabAtPosition(GameObject prefab, Vector3 position, float liftHeight, Vector3 forward, Vector3 extraEuler)
    {
        if (prefab == null) return null;

        Vector3 spawnPos = position;
        if (Physics.Raycast(spawnPos + Vector3.up, Vector3.down, out RaycastHit hit, 5f))
        {
            spawnPos = hit.point + Vector3.up * liftHeight;
        }

        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z).normalized;
        Quaternion spawnRot = Quaternion.LookRotation(flatForward, Vector3.up) * Quaternion.Euler(extraEuler);
        Transform instance = Instantiate(prefab, spawnPos, spawnRot).transform;
        instance.localScale = prefab.transform.localScale;
        return instance;
    }
}
