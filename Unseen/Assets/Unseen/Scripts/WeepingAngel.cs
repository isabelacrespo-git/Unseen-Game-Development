using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class WeepingAngel : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent ai;
    public Transform player;
    public Animator aiAnim;
    public Camera jumpscareCam;
    
    [Header("Settings")]
    public float aiSpeed = 3.5f;
    public float catchDistance = 2f;
    public float jumpscareTime = 3f;
    public string sceneAfterDeath = "GameOver";

    [Header("Chase Settings")]
    [Tooltip("Maximum distance the angel will chase the player")]
    public float maxChaseDistance = 20f;
    [Tooltip("If the player gets this far away, angel will stop and idle")]
    public float stopChaseDistance = 25f;
    [Tooltip("Maximum path distance (actual walking distance through maze)")]
    public float maxPathDistance = 30f;
    [Tooltip("Check for obstacles between angel and player")]
    public bool requireLineOfSight = true;
    [Tooltip("Layers that block the angel's line of sight (walls, objects, etc.)")]
    public LayerMask obstacleLayers;
    [Tooltip("Maximum ratio of path distance to straight-line distance (prevents chasing around corners)")]
    public float maxPathDistanceRatio = 1.5f;

    [Header("VR Settings")]
    [Tooltip("Leave empty to auto-find the main camera (works for VR)")]
    public Camera playerCam;

    [Header("Animation Settings")] 
    [Tooltip("Name of the idle animation state in the Animator")]
    public string idleAnimationState = "Idle";
    [Tooltip("Name of the running state in the Animator")]
    public string runAnimationState = "Run";

    [Header("Audio Settings")]
    [Tooltip("AudioSource for footstep sounds")]
    public AudioSource footstepAudioSource;
    [Tooltip("Array of footstep sound clips (will be randomized)")]
    public AudioClip[] footstepSounds;
    [Tooltip("Base volume of footstep sounds")]
    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;
    [Tooltip("Randomize volume by +/- this amount")]
    [Range(0f, 0.3f)]
    public float volumeVariation = 0.1f;
    [Tooltip("Randomize pitch by +/- this amount")]
    [Range(0f, 0.3f)]
    public float pitchVariation = 0.15f;
    [Tooltip("Use animation events instead of timer")]
    public bool useAnimationEvents = true;
    [Tooltip("Time between footstep sounds (seconds) - only used if not using animation events")]
    [Range(0.1f, 1f)]
    public float footstepInterval = 0.4f;
    [Tooltip("Use 3D spatial audio for footsteps")]
    public bool use3DAudio = true;
    [Tooltip("Maximum distance to hear footsteps")]
    public float maxFootstepDistance = 30f;

    [Header("Jumpscare Audio")]
    [Tooltip("AudioSource for jumpscare sound (optional - will create if null)")]
    public AudioSource jumpscareAudioSource;
    [Tooltip("Sound effect to play when catching the player")]
    public AudioClip jumpscareSound;
    [Tooltip("Volume of the jumpscare sound")]
    [Range(0f, 1f)]
    public float jumpscareVolume = 1f;
    private Vector3 dest;
    private bool isBeingWatched = false;
    private bool hasCaughtPlayer = false;
    private bool isChasing = false;
    private bool hasLineOfSight = false;
    private bool hasBeenActivated = false;
    private bool isMoving = false;
    private float footstepTimer = 0f;
    private static bool playerGloballyCaptured = false;

    void Start()
    {
        ResolvePlayerReferences();

        if (ai == null) ai = GetComponent<NavMeshAgent>();

        // Make sure jumpscare camera is disabled at start
        if (jumpscareCam != null)
        {
            jumpscareCam.gameObject.SetActive(false);
        }
        
        // Start in idle animation state
        if (aiAnim != null)
        {
            aiAnim.Play(idleAnimationState);
            aiAnim.speed = 0; // Freeze the idle animation
        }

        // Setup footstep audio
        SetupFootstepAudio();
        
        // Validation
        if (ai == null || player == null)
        {
            Debug.LogError("WeepingAngel: Missing required references!");
            enabled = false;
        }

        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            Debug.LogWarning("WeepingAngel: No footstep sounds assigned!");
        }
    }

    void ResolvePlayerReferences()
    {
        // Auto-find main camera if not assigned (works for VR headset camera)
        if (playerCam == null)
        {
            playerCam = Camera.main;
            if (playerCam == null)
            {
                Debug.LogError("WeepingAngel: No camera found! Make sure your XR camera has the 'MainCamera' tag.");
            }
        }

        // Auto-assign player transform if empty by trying common XR rig references
        if (player == null)
        {
            XROrigin xrOrigin = null;
#if UNITY_2023_1_OR_NEWER
            xrOrigin = UnityEngine.Object.FindFirstObjectByType<XROrigin>();
#else
            xrOrigin = UnityEngine.Object.FindObjectOfType<XROrigin>();
#endif
            if (xrOrigin != null)
            {
                player = xrOrigin.transform;
                if (playerCam == null && xrOrigin.Camera != null)
                {
                    playerCam = xrOrigin.Camera;
                }
            }
        }

        if (player == null)
        {
            // Fallback to classic Player tag or object name
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                playerObj = GameObject.Find("XR Rig");
            }

            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("WeepingAngel: No player transform assigned. Please drag the XR Rig transform into the 'player' field.");
        }
    }

    void SetupFootstepAudio()
    {
        // Create AudioSource if not assigned
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource for one-shot playback
        footstepAudioSource.loop = false;
        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.pitch = 1f;

        if (use3DAudio)
        {
            footstepAudioSource.spatialBlend = 1f; // Full 3D
            footstepAudioSource.maxDistance = maxFootstepDistance;
            footstepAudioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            footstepAudioSource.spatialBlend = 0f; // 2D audio
        }

        // Setup jumpscare audio source
        if (jumpscareAudioSource == null)
        {
            jumpscareAudioSource = gameObject.AddComponent<AudioSource>();
        }

        jumpscareAudioSource.loop = false;
        jumpscareAudioSource.playOnAwake = false;
        jumpscareAudioSource.spatialBlend = 0f; // 2D (global sound)
        jumpscareAudioSource.volume = jumpscareVolume;

    }

   void Update()
    {
        if (playerCam == null || hasCaughtPlayer) return;

        // If another angel already triggered the jumpscare, ignore the player.
        if (playerGloballyCaptured)
        {
            if (ai != null)
            {
                ai.speed = 0;
                ai.isStopped = true;
                ai.SetDestination(transform.position);
            }
            SetMoving(false);
            return;
        }

        // Check if angel is in player's view
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCam);
        isBeingWatched = GeometryUtility.TestPlanesAABB(planes, GetComponent<Collider>().bounds);

        float distance = Vector3.Distance(transform.position, player.position);

        // Check line of sight to player FIRST (before any other checks)
        hasLineOfSight = CheckLineOfSight();

        // Check if angel should activate for the first time
        if (!hasBeenActivated)
        {
            if (distance <= maxChaseDistance && !isBeingWatched && (!requireLineOfSight || hasLineOfSight))
            {
                hasBeenActivated = true;
                Debug.Log("WeepingAngel: Activated and started chasing the player!");
            }
            else
            {
                return; // Not activated yet
            }
        }

        // Determine if the angel should be chasing
        if (distance <= maxChaseDistance && (!requireLineOfSight || hasLineOfSight))
        {
            isChasing = true;
        }
        else if (distance >= stopChaseDistance || (requireLineOfSight && !hasLineOfSight))
        {
            isChasing = false;
        }

       if (isBeingWatched)
        {
            // Player is looking at the angel - FREEZE
            ai.speed = 0;
            if (aiAnim != null) aiAnim.speed = 0;
            ai.SetDestination(transform.position);
            SetMoving(false); // Stop footsteps
        }
        else if (isChasing)
        {
            // Player is NOT looking and angel is chasing - MOVE
            ai.speed = aiSpeed;
            if (aiAnim != null)
            {
                aiAnim.speed = 1;

                // Ensure running animation is playing
                if (!aiAnim.GetCurrentAnimatorStateInfo(0).IsName(runAnimationState))
                {
                    aiAnim.Play(runAnimationState);
                }
            }
            dest = player.position;
            ai.SetDestination(dest);
            SetMoving(true); // Play footsteps

            // NEW: Use NavMesh path distance instead of straight-line distance for catch check
            if (ai.hasPath && ai.remainingDistance <= catchDistance && hasLineOfSight)
            {
                CatchPlayer();
            }
        }
        else
        {
            // Not being watched but not chasing - IDLE
            ai.speed = 0;
            if (aiAnim != null) aiAnim.speed = 0;
            ai.SetDestination(transform.position);
            SetMoving(false); // Stop footsteps
        }

        // Handle footstep timer (only if not using animation events)
        if (!useAnimationEvents && isMoving)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                PlayRandomFootstep();
                footstepTimer = 0f;
            }
        }
        else if (!isMoving)
        {
            footstepTimer = 0f;
        }
    }

    void SetMoving(bool moving)
    {
        if (isMoving == moving) return; // No change
        
        isMoving = moving;

        if (!moving)
        {
            footstepTimer = 0f; // Reset timer when stopping
        }
    }

    // Called by Animation Events (add this event to your run animation)
    public void OnFootstep()
    {
        // Only play if actually moving (prevents sounds when frozen)
        if (isMoving)
        {
            PlayRandomFootstep();
        }
    }

    void PlayRandomFootstep()
    {
        if (footstepAudioSource == null || footstepSounds == null || footstepSounds.Length == 0)
            return;

        // Pick random footstep sound
        AudioClip randomClip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        
        if (randomClip == null) return;

        // Randomize volume
        float randomVolume = footstepVolume + Random.Range(-volumeVariation, volumeVariation);
        randomVolume = Mathf.Clamp01(randomVolume);

        // Randomize pitch
        float randomPitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        footstepAudioSource.pitch = randomPitch;

        // Play the sound
        footstepAudioSource.PlayOneShot(randomClip, randomVolume);
    }

    bool CheckLineOfSight()
    {
        if (!requireLineOfSight) return true;

        Vector3 angelEye = transform.position + Vector3.up * 1.5f;
        Vector3 playerEye = player.position + Vector3.up * 1.4f;
        float distanceToPlayer = Vector3.Distance(angelEye, playerEye);

        // CRITICAL: Raycast from angel to player to check for walls
        // This must be checked FIRST before any distance calculations
        if (Physics.Raycast(angelEye, (playerEye - angelEye).normalized, out RaycastHit hit, distanceToPlayer, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"WeepingAngel: (angel->player) BLOCKED by wall: {hit.collider.name} at distance {hit.distance:F2}");
            return false; // Wall detected - no line of sight
        }

        // Raycast from player's camera to angel to ensure the player actually has a clear view
        if (playerCam != null)
        {
            Vector3 camPos = playerCam.transform.position;
            float camDist = Vector3.Distance(camPos, angelEye);
            if (Physics.Raycast(camPos, (angelEye - camPos).normalized, out RaycastHit camHit, camDist, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.Log($"WeepingAngel: (cam->angel) blocked by wall: {camHit.collider.name}");
                return false; // Wall blocks camera view
            }

            // Additional check: ensure first hit from camera is the angel or nothing
            if (Physics.Raycast(camPos, (angelEye - camPos).normalized, out RaycastHit anyHit, camDist, ~0, QueryTriggerInteraction.Ignore))
            {
                GameObject hitObj = anyHit.collider.gameObject;
                if (hitObj != gameObject && !hitObj.transform.IsChildOf(transform) && hitObj != player.gameObject)
                {
                    Debug.Log($"WeepingAngel: (cam->angel) occluded by {hitObj.name}");
                    return false;
                }
            }
        }

        // NavMesh path validation (ensure agent can actually walk to the player)
        NavMeshPath path = new NavMeshPath();
        if (!ai.CalculatePath(player.position, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.Log($"WeepingAngel: NavMesh path invalid or incomplete (status={path.status})");
            return false;
        }

        // Compute actual path distance to avoid chasing through long detours
        float pathDistance = 0f;
        for (int i = 0; i < path.corners.Length - 1; i++)
            pathDistance += Vector3.Distance(path.corners[i], path.corners[i + 1]);

        // Check if path is too convoluted (indicates corners/obstacles)
        float pathRatio = pathDistance / distanceToPlayer;
        Debug.Log($"WeepingAngel: pathDistance={pathDistance:F2}, straightLine={distanceToPlayer:F2}, ratio={pathRatio:F2}, maxRatio={maxPathDistanceRatio}");
        
        if (pathDistance > maxPathDistance) 
        {
            Debug.Log("WeepingAngel: Path too long - not chasing");
            return false;
        }

        if (pathRatio > maxPathDistanceRatio)
        {
            Debug.Log($"WeepingAngel: Path too convoluted (ratio {pathRatio:F2} > {maxPathDistanceRatio}) - player likely behind corner");
            return false;
        }

        return true;
    }

    void CatchPlayer()
    {
        if (playerGloballyCaptured)
        {
            return;
        }

        hasCaughtPlayer = true;
        playerGloballyCaptured = true;
        
        // Stop the angel
        ai.speed = 0;
        ai.isStopped = true;
        SetMoving(false); // Stop footsteps
        
        // Play jumpscare sound
        if (jumpscareAudioSource != null && jumpscareSound != null)
        {
            jumpscareAudioSource.PlayOneShot(jumpscareSound);
            Debug.Log("Jumpscare sound played!");
        }
        
        // Disable player camera/controls
        if (playerCam != null)
        {
            playerCam.gameObject.SetActive(false);
        }
        
        // Disable player GameObject (XR Rig)
        if (player != null)
        {
            player.gameObject.SetActive(false);
        }
        
        // Activate jumpscare camera FIRST
        if (jumpscareCam != null)
        {
            jumpscareCam.gameObject.SetActive(true);
            Debug.Log("Jumpscare camera activated!");
        }
        
        // THEN trigger animation
        if (aiAnim != null)
        {
            aiAnim.SetTrigger("jumpscare");
            Debug.Log("Jumpscare animation triggered!");
        }
        
        // Start death sequence
        StartCoroutine(KillPlayer());
    }

    IEnumerator KillPlayer()
    {
        yield return new WaitForSeconds(jumpscareTime);
        
        Debug.Log($"Loading scene: {sceneAfterDeath}");
        SceneManager.LoadScene(sceneAfterDeath);
    }

    // Debug visualization

    void OnDrawGizmos()
    {
        if (Application.isPlaying && player != null)
        {
            // Draw line color based on state
            if (isBeingWatched)
                Gizmos.color = Color.red; // Being watched
            else if (isChasing)
                Gizmos.color = Color.yellow; // Chasing
            else
                Gizmos.color = Color.green; // Idle

            Gizmos.DrawLine(transform.position, player.position);
            
            // Draw catch distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, catchDistance);
            
            // Draw chase distances
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxChaseDistance);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, stopChaseDistance);

            // Draw line of sight raycast
            if (requireLineOfSight)
            {
                Gizmos.color = hasLineOfSight ? Color.cyan : Color.magenta;
                Gizmos.DrawRay(transform.position + Vector3.up * 1f, (player.position - transform.position).normalized * Vector3.Distance(transform.position, player.position));
            }

            // Draw actual NavMesh path
            if (ai != null && ai.hasPath)
            {
                Gizmos.color = Color.blue;
                Vector3[] corners = ai.path.corners;
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(corners[i], corners[i + 1]);
                }
            }

            // Draw footstep audio range
            if (use3DAudio && footstepAudioSource != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, maxFootstepDistance);
            }
        }
    }
}
