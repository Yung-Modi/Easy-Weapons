using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;

    // Sight / scanning settings
    public float viewDistance = 12f;
    [Range(10f, 180f)] public float viewAngle = 60f;
    public float scanDuration = 3f;
    public float scanAngle = 60f; // how far left/right to sweep while scanning

    [Tooltip("If the player is closer than this distance the enemy will detect them from any direction (occlusion still applies).")]
    public float proximityRange = 2.5f;

    [Tooltip("Distance inside which enemies will steer away from each other while patrolling.")]
    public float separationRadius = 1.5f;

    [Tooltip("How strongly enemies separate from nearby allies (higher = stronger steering).")]
    public float separationWeight = 1.0f;

    public LayerMask obstructionMask; // set to layers that block sight (walls, environment)

    // Spider lunge configuration
    [Header("Spider Lunge")]
    [Tooltip("Enable to allow this enemy to perform a lunge attack like a spider.")]
    public bool isSpider = false;
    [Tooltip("Maximum distance at which the spider will attempt a lunge.")]
    public float lungeRange = 6f;
    [Tooltip("Speed multiplier used during the lunge movement (fallback if NavMesh not used).")]
    public float lungeSpeed = 12f;
    [Tooltip("Cooldown between lunges.")]
    public float lungeCooldown = 2.0f;
    [Tooltip("Vertical arc height (world units) applied at the apex of the lunge. Set to 0 for a flat lunge.")]
    public float lungeHeight = 1.0f;
    [Tooltip("Maximum time (seconds) allowed for a lunge to avoid infinite lunges if ground isn't detected.")]
    public float lungeMaxTime = 3f;

    private NavMeshAgent agent;
    private Transform player;
    private int currentPatrolIndex;
    private float lastAttackTime;

    private bool isScanning;
    private Coroutine scanRoutine;

    // Lunge state
    private float lastLungeTime = -Mathf.Infinity;
    private Coroutine lungeRoutine;

    // Active enemies registry for fast neighbor checks
    private static readonly List<EnemyAI> allEnemies = new List<EnemyAI>();

    void OnEnable()
    {
        if (!allEnemies.Contains(this)) allEnemies.Add(this);
    }

    void OnDisable()
    {
        allEnemies.Remove(this);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Randomize avoidance priority so agents don't get stuck fighting for the same space
        if (agent != null)
            agent.avoidancePriority = Random.Range(0, 100);

        var found = GameObject.FindGameObjectWithTag("Player");
        player = found ? found.transform : null;

        // If inspector didn't supply patrol points, find all GameObjects tagged "Patrol point"
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            var tagged = GameObject.FindGameObjectsWithTag("Patrol point");
            if (tagged != null && tagged.Length > 0)
            {
                // Order by name for deterministic ordering, then take transforms
                patrolPoints = tagged
                    .OrderBy(go => go.name)
                    .Select(go => go.transform)
                    .ToArray();
            }
        }

        currentPatrolIndex = 0;
        lastAttackTime = -attackCooldown; // Allow immediate attack
        Patrol();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If player is in attack range -> attack
        if (distanceToPlayer <= attackRange && CanSeePlayer())
        {
            // stop any scanning and any lunge in progress
            StopScanning();
            if (lungeRoutine != null)
            {
                StopCoroutine(lungeRoutine);
                lungeRoutine = null;
            }
            Attack();
        }
        // If player visible and within chase range -> chase or lunge (spider)
        else if (distanceToPlayer <= chaseRange && CanSeePlayer())
        {
            StopScanning();

            // Spider lunge: only if configured, off cooldown, and within lungeRange (but not already in melee)
            if (isSpider && Time.time - lastLungeTime >= lungeCooldown && distanceToPlayer <= lungeRange && distanceToPlayer > attackRange)
            {
                if (lungeRoutine == null)
                    lungeRoutine = StartCoroutine(LungeTowardsPlayer());
            }
            else
            {
                // If currently lunging, don't override; otherwise normal chase
                if (lungeRoutine == null)
                    Chase();
            }
        }
        else
        {
            // resume patrolling and scanning behavior
            Patrol();
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Vector3 baseTarget = patrolPoints[currentPatrolIndex].position;
        Vector3 desiredTarget = baseTarget;

        // Apply simple separation steering to avoid nearby allies while patrolling
        if (!isScanning)
        {
            Vector3 separation = ComputeSeparationVector();
            if (separation != Vector3.zero)
            {
                Vector3 candidate = baseTarget + separation;
                // ensure the candidate target lies on the NavMesh
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, separationRadius, NavMesh.AllAreas))
                    desiredTarget = hit.position;
                else
                    desiredTarget = baseTarget; // fallback
            }
        }

        // Only update destination if it's meaningfully different to avoid spamming SetDestination
        if (!agent.hasPath || Vector3.Distance(agent.destination, desiredTarget) > 0.5f)
        {
            agent.isStopped = false;
            agent.SetDestination(desiredTarget);
        }

        // If close to base patrol point (not the separated target), start scanning
        if (!isScanning && Vector3.Distance(transform.position, baseTarget) < 1f)
        {
            // begin a scanning sweep; once finished the coroutine moves to next patrol point
            scanRoutine = StartCoroutine(ScanForPlayerThenAdvance());
        }
    }

    // Compute a simple averaged repulsion vector from nearby allies
    private Vector3 ComputeSeparationVector()
    {
        Vector3 separation = Vector3.zero;
        int count = 0;

        for (int i = 0; i < allEnemies.Count; i++)
        {
            var other = allEnemies[i];
            if (other == null || other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < separationRadius && dist > 0.0001f)
            {
                Vector3 away = (transform.position - other.transform.position).normalized;
                float strength = (separationRadius - dist) / separationRadius; // stronger when closer
                separation += away * strength;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        separation /= count;
        return separation.normalized * separationWeight;
    }

    void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        agent.isStopped = true;
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // Log for debugging
            Debug.Log("Attacking player for " + attackDamage + " damage.");

            // Try to apply damage to the player using the Health component if present.
            if (player != null)
            {
                // Prefer direct component call for type-safety and event firing
                var healthComp = player.GetComponent<Health>();
                if (healthComp != null)
                {
                    healthComp.ChangeHealth(-attackDamage);
                }
                else
                {
                    // Fallback: use SendMessageUpwards for compatibility with other health implementations
                    player.SendMessageUpwards("ChangeHealth", -attackDamage, SendMessageOptions.DontRequireReceiver);
                }
            }

            lastAttackTime = Time.time;
        }
    }

    // Lunge coroutine used by spider enemies
    private IEnumerator LungeTowardsPlayer()
    {
        lastLungeTime = Time.time;

        // Stop the NavMeshAgent while lunging
        if (agent != null)
            agent.isStopped = true;

        Vector3 lungeStart = transform.position;
        Vector3 lungeTarget = player != null ? player.position : lungeStart;
        bool appliedDamage = false;

        // horizontal-only target for consistent travel (use player's xz at start)
        Vector3 targetXZ = new Vector3(lungeTarget.x, 0f, lungeTarget.z);
        Vector3 startXZ = new Vector3(lungeStart.x, 0f, lungeStart.z);
        float totalHorizontalDist = Vector3.Distance(startXZ, targetXZ);
        float traveled = 0f;

        float elapsed = 0f;
        float timeout = lungeMaxTime;
        while (elapsed < timeout)
        {
            if (player == null) break;

            // compute horizontal step
            Vector3 currentXZ = new Vector3(transform.position.x, 0f, transform.position.z);
            float step = lungeSpeed * Time.deltaTime;
            Vector3 nextXZ = (totalHorizontalDist > 0f)
                ? Vector3.MoveTowards(currentXZ, targetXZ, step)
                : currentXZ;

            traveled = Vector3.Distance(startXZ, nextXZ);
            float progress = (totalHorizontalDist > 0f) ? Mathf.Clamp01(traveled / totalHorizontalDist) : 1f;

            // base horizontal position along line
            Vector3 basePos = Vector3.Lerp(lungeStart, new Vector3(lungeTarget.x, lungeStart.y, lungeTarget.z), progress);

            // Parabolic height offset (peak at progress=0.5)
            float heightOffset = 4f * lungeHeight * progress * (1f - progress);

            // Next desired position including vertical arc
            Vector3 nextPos = new Vector3(nextXZ.x, basePos.y + heightOffset, nextXZ.z);

            transform.position = nextPos;

            // Face movement direction (horizontal only)
            Vector3 lookDir = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position);
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir.normalized), 0.5f);

            // Apply damage once if we enter attack range during the lunge (horizontal distance)
            Vector2 horizPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 horizPlayer = new Vector2(player.position.x, player.position.z);
            if (!appliedDamage && Vector2.Distance(horizPos, horizPlayer) <= attackRange)
            {
                var healthComp = player.GetComponent<Health>();
                if (healthComp != null)
                    healthComp.ChangeHealth(-attackDamage);
                else
                    player.SendMessageUpwards("ChangeHealth", -attackDamage, SendMessageOptions.DontRequireReceiver);

                appliedDamage = true;
                lastAttackTime = Time.time;
            }

            // Ground check: cast a short ray downward to see if we are touching ground
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.25f))
            {
                // Consider we've landed if the ground is below and we have progressed a small amount
                if (progress > 0.05f)
                {
                    break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Resume NavMeshAgent behavior
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player != null ? player.position : transform.position);
        }

        lungeRoutine = null;
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position);
        float distance = dirToPlayer.magnitude;

        // If player is very close, allow omnidirectional detection (still respect occlusion)
        if (distance <= proximityRange)
        {
            return HasLineOfSightToPlayer(distance);
        }

        if (distance > viewDistance) return false;

        Vector3 dirNormalized = dirToPlayer.normalized;
        float angle = Vector3.Angle(transform.forward, dirNormalized);
        if (angle > viewAngle * 0.5f) return false;

        return HasLineOfSightToPlayer(distance);
    }

    // Performs occlusion check: returns true if there is a clear line (no blocking objects from obstructionMask)
    private bool HasLineOfSightToPlayer(float distanceToPlayer)
    {
        Vector3 origin = transform.position + Vector3.up * 0.6f;
        Vector3 target = player.position + Vector3.up * 0.6f;
        Vector3 dir = target - origin;
        float maxDistance = Mathf.Min(distanceToPlayer, dir.magnitude);

        var hits = Physics.RaycastAll(origin, dir.normalized, maxDistance);
        if (hits == null || hits.Length == 0)
        {
            // No hits toward the player — cannot confirm visibility
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            var hitTransform = hit.collider.transform;

            // If it's the player (or part of the player) -> visible
            if (hitTransform == player || hitTransform.IsChildOf(player))
                return true;

            // If obstructionMask == 0 treat any collider as blocking
            if (obstructionMask.value == 0)
                return false;

            // If this hit is in an obstruction layer -> blocked
            if ((obstructionMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                return false;

            // Otherwise ignore this hit (non-blocking object) and continue checking
        }

        return false;
    }

    IEnumerator ScanForPlayerThenAdvance()
    {
        isScanning = true;
        agent.isStopped = true;

        float elapsed = 0f;
        float startYaw = transform.eulerAngles.y;
        while (elapsed < scanDuration)
        {
            // simple left-right sweep using sine
            float t = elapsed / scanDuration;
            float yawOffset = Mathf.Sin(t * Mathf.PI * 2f) * (scanAngle * 0.5f);
            float targetYaw = startYaw + yawOffset;
            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);

            // check for player each frame
            if (player != null && CanSeePlayer())
            {
                isScanning = false;
                agent.isStopped = false;
                yield break; // found player -> stop scanning and let Update handle chasing
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // scanning finished, advance to next patrol point
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        isScanning = false;
    }

    void StopScanning()
    {
        if (isScanning && scanRoutine != null)
        {
            StopCoroutine(scanRoutine);
            scanRoutine = null;
            isScanning = false;
            agent.isStopped = false;
        }
    }
}