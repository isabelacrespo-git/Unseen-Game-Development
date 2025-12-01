using UnityEngine;

public class PartyGoerSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // player root / camera rig
    public GameObject partygoerPrefab;       // prefab with PartygoerEncounter

    [Header("Spawn placement")]
    public LayerMask floorMask;              // your floor layer(s)
    public LayerMask wallMask;               // your wall layer(s)
    public float minRadius = 8f;
    public float maxRadius = 18f;
    public float eyeHeight = 1.7f;           // approx player eye height
    public int maxSpawnTries = 20;

    [Header("Timing")]
    public float minDelayBetweenEncounters = 15f;
    public float maxDelayBetweenEncounters = 35f;

    [Header("Debugging")]
    public bool enableDebugLogs = true;
    [Tooltip("If true, falls back to any collider when floorMask misses so the spawner can still work while you debug layers.")]
    public bool fallbackToAnyLayer = true;

    PartygoerEncounter _activeEncounter;
    float _nextSpawnTime;
    bool _loggedSpawnWindow;
    bool _spawningDisabled;

    void Start()
    {
        LogDebug("PartyGoerSpawner initializing.");
        LogDebug($"Floor mask: {DescribeMask(floorMask)} (value {floorMask.value}).");
        LogDebug($"Wall mask: {DescribeMask(wallMask)} (value {wallMask.value}).");
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (_spawningDisabled)
        {
            return;
        }

        if (_activeEncounter != null)
        {
            _loggedSpawnWindow = false;
            return;
        }

        if (Time.time < _nextSpawnTime)
        {
            _loggedSpawnWindow = false;
            return;
        }

        if (!_loggedSpawnWindow)
        {
            LogDebug("Spawn window open. Trying to find a spawn point.");
            _loggedSpawnWindow = true;
        }

        if (TryGetSpawnPosition(out Vector3 spawnPos))
        {
            LogDebug($"Spawn position acquired at {spawnPos}.");
            _loggedSpawnWindow = false;
            SpawnEncounter(spawnPos);
        }
        else
        {
            // couldn't find a spot this frame, try again soon
            LogDebug("Unable to find a spawn spot this frame, retrying shortly.");
            _nextSpawnTime = Time.time + 5f;
            _loggedSpawnWindow = false;
        }
    }

    void SpawnEncounter(Vector3 pos)
    {
        Quaternion lookRot = Quaternion.LookRotation(
            (player.position - pos).normalized,
            Vector3.up
        );

        GameObject go = Instantiate(partygoerPrefab, pos, lookRot);
        _activeEncounter = go.GetComponent<PartygoerEncounter>();

        if (_activeEncounter != null)
        {
            LogDebug("Partygoer encounter component found. Beginning encounter.");
            _activeEncounter.BeginEncounter(player, OnEncounterFinished);
        }
        else
        {
            Debug.LogWarning("PartyGoerSpawner: prefab missing PartyGoerEncounter.");
        }
    }

    void ScheduleNextSpawn()
    {
        float delay = Random.Range(minDelayBetweenEncounters, maxDelayBetweenEncounters);
        _nextSpawnTime = Time.time + delay;
        _loggedSpawnWindow = false;
        LogDebug($"Next spawn scheduled in {delay:F1}s (target time {_nextSpawnTime:F1}).");
    }

    void OnEncounterFinished(bool success)
    {
        // success = player "cleansed" it with flashlight
        // false  = balloon popped (you can hook damage / scare here too)

        _activeEncounter = null;

        if (!success)
        {
            _spawningDisabled = true;
            enabled = false;
            LogDebug("Encounter ended in failure. Spawner disabled to prevent additional spawns.");
            return;
        }

        LogDebug($"Encounter finished. Success = {success}. Scheduling next spawn.");
        ScheduleNextSpawn();
    }

    bool TryGetSpawnPosition(out Vector3 result)
    {
        LogDebug("Attempting to locate a spawn position...");
        result = Vector3.zero;
        if (player == null)
        {
            LogDebug("Cannot spawn because player reference is missing.");
            return false;
        }
        if (floorMask.value == 0)
        {
            LogDebug("Floor mask is currently zero. No layers will be considered for spawning.");
        }

        LayerMask maskToUse = EffectiveFloorMask;

        for (int i = 0; i < maxSpawnTries; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minRadius, maxRadius);

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 basePos = player.position + offset + Vector3.up * 10f; // cast down from above

            Debug.DrawLine(basePos, basePos + Vector3.down * 30f, Color.yellow, 1f);

            if (Physics.Raycast(basePos, Vector3.down, out RaycastHit hit, 30f, maskToUse, QueryTriggerInteraction.Ignore))
            {
                if (ValidateSpawnPoint(hit.point, i, out result))
                {
                    LogDebug($"Attempt {i + 1}: spawn point accepted.");
                    return true;
                }
            }
            else
            {
                LogDebug($"Attempt {i + 1}: failed, floor raycast from {basePos} did not hit using mask {maskToUse.value} ({DescribeMask(maskToUse)}).");
                if (i == 0)
                {
                    // First failure: also check without mask so we can report what layer was hit.
                    if (Physics.Raycast(basePos, Vector3.down, out RaycastHit anyHit, 30f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    {
                        LogDebug($"Attempt {i + 1}: without mask we would hit {anyHit.collider.name} on layer {LayerMask.LayerToName(anyHit.collider.gameObject.layer)}.");
                    }
                    else
                    {
                        LogDebug($"Attempt {i + 1}: even without mask no collider was detected. Is the floor within 30 units below {basePos}?");
                    }
                }

                if (fallbackToAnyLayer && Physics.Raycast(basePos, Vector3.down, out RaycastHit fallbackHit, 30f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    string fallbackLayer = LayerMask.LayerToName(fallbackHit.collider.gameObject.layer);
                    LogDebug($"Attempt {i + 1}: fallback hit {fallbackHit.collider.name} on layer {fallbackLayer}. Using fallback result.");
                    if (ValidateSpawnPoint(fallbackHit.point, i, out result))
                    {
                        LogDebug($"Attempt {i + 1}: spawn point accepted via fallback.");
                        return true;
                    }
                }
            }
        }

        LogDebug("Failed to find a spawn position after all attempts.");
        return false;
    }

    bool ValidateSpawnPoint(Vector3 spawnPos, int attempt, out Vector3 result)
    {
        result = spawnPos;

        Vector3 toSpawn = (spawnPos - player.position).normalized;
        float dot = Vector3.Dot(player.forward, toSpawn);
        if (dot > 0.7f)
        {
            LogDebug($"Attempt {attempt + 1}: rejected because spawn is in front of player (dot={dot:F2}).");
            return false;
        }

        Vector3 fromPlayerEye = player.position + Vector3.up * eyeHeight;
        Vector3 dir = (spawnPos + Vector3.up * 1.5f) - fromPlayerEye;
        float dist = dir.magnitude;

        LayerMask wallMaskToUse = wallMask.value == 0 ? Physics.DefaultRaycastLayers : wallMask;
        if (Physics.Raycast(fromPlayerEye, dir.normalized, dist, wallMaskToUse))
        {
            LogDebug($"Attempt {attempt + 1}: rejected because vision is blocked (distance {dist:F1}m).");
            return false;
        }

        return true;
    }

    LayerMask EffectiveFloorMask => floorMask.value == 0 ? Physics.DefaultRaycastLayers : floorMask;

    string DescribeMask(LayerMask mask)
    {
        if (mask.value == 0) return "(None)";

        string names = "";
        for (int bit = 0; bit < 32; bit++)
        {
            if ((mask.value & (1 << bit)) == 0) continue;
            string layerName = LayerMask.LayerToName(bit);
            if (string.IsNullOrEmpty(layerName))
                layerName = $"Layer {bit}";

            if (names.Length > 0) names += ", ";
            names += layerName;
        }

        return string.IsNullOrEmpty(names) ? $"(Custom bits {mask.value})" : names;
    }

    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PartyGoerSpawner] {message}", this);
        }
    }
}
