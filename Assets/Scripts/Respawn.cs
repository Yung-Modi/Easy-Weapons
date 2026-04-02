using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Lives")]
    [Tooltip("Total number of respawns allowed. Set to 0 to disable respawning.")]
    public int maxRespawns = 3;

    [Header("Respawn")]
    [Tooltip("Where the player will be moved when respawning. If null, player will respawn at this object's starting position or the nearest object tagged \"Respawn Point\".")]
    public Transform respawnPoint;
    [Tooltip("Delay before respawn (seconds).")]
    public float respawnDelay = 2f;

    [Header("What to disable on death")]
    [Tooltip("Behaviour components to disable while 'dead' (e.g. PlayerController).")]
    public Behaviour[] componentsToDisable;
    [Tooltip("GameObjects to disable while 'dead' (e.g. visuals).")]
    public GameObject[] gameObjectsToDisable;

    [Header("Behavior on out of lives")]
    [Tooltip("If true the GameObject will be destroyed when out of respawns.")]
    public bool destroyOnNoRespawns = false;

    [Header("Control automatic return to menu")]
    [Tooltip("If true the system will automatically load the configured scene index when out of respawns. Otherwise fire the OnOutOfRespawns event only.")]
    public bool autoLoadSceneOnNoRespawns = false;
    [Tooltip("Scene index to load when autoLoadSceneOnNoRespawns is true.")]
    public int sceneIndexToLoadOnNoRespawns = 2;

    [Header("Events")]
    public UnityEvent OnRespawnScheduled;
    public UnityEvent OnRespawned;
    public UnityEvent OnOutOfRespawns;

    int remainingRespawns;
    Vector3 initialPosition;
    Quaternion initialRotation;
    bool isDead;

    public int RemainingRespawns => remainingRespawns;

    // Tag used to search for respawn points in the scene
    private const string RespawnPointTag = "Respawn Point";

    void Awake()
    {
        remainingRespawns = maxRespawns;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// Call this from your death handler. Returns true if a respawn was scheduled; false if there are no respawns left.
    /// When out of respawns behavior: fire OnOutOfRespawns and, if autoLoadSceneOnNoRespawns is true, load the configured scene index.
    /// </summary>
    public bool TryRespawn()
    {
        if (isDead) return false; // already handling a death

        if (remainingRespawns <= 0)
        {
            // Already out of respawns
            OnOutOfRespawns?.Invoke();
            if (destroyOnNoRespawns)
                Destroy(gameObject);

            // Do not force a scene change by default; only load if explicitly enabled
            if (autoLoadSceneOnNoRespawns)
            {
                // Only load if not already in target scene
                if (SceneManager.GetActiveScene().buildIndex != sceneIndexToLoadOnNoRespawns)
                    SceneManager.LoadScene(sceneIndexToLoadOnNoRespawns);
            }

            return false;
        }

        // consume one respawn
        remainingRespawns--;

        // If this consumption used the last respawn, fire OutOfRespawns and optionally auto-load
        if (remainingRespawns <= 0)
        {
            OnOutOfRespawns?.Invoke();
            if (destroyOnNoRespawns)
                Destroy(gameObject);

            if (autoLoadSceneOnNoRespawns)
            {
                if (SceneManager.GetActiveScene().buildIndex != sceneIndexToLoadOnNoRespawns)
                    SceneManager.LoadScene(sceneIndexToLoadOnNoRespawns);
                return false;
            }

            return false;
        }

        StartCoroutine(DoRespawn());
        OnRespawnScheduled?.Invoke();
        return true;
    }

    IEnumerator DoRespawn()
    {
        isDead = true;
        // disable requested components and objects
        SetDisabledState(true);

        // optional: add death visuals or sound here

        yield return new WaitForSeconds(Mathf.Max(0f, respawnDelay));

        // perform respawn: move and re-enable
        Transform chosen = GetRespawnTransform();
        Vector3 targetPos = (chosen != null) ? chosen.position : initialPosition;
        Quaternion targetRot = (chosen != null) ? chosen.rotation : initialRotation;

        transform.position = targetPos;
        transform.rotation = targetRot;

        SetDisabledState(false);
        isDead = false;
        OnRespawned?.Invoke();

        // If after decrementing there are no respawns left, fire event (useful to lock UI)
        if (remainingRespawns <= 0)
        {
            OnOutOfRespawns?.Invoke();
            if (destroyOnNoRespawns)
                Destroy(gameObject);
        }
    }

    void SetDisabledState(bool disabled)
    {
        if (componentsToDisable != null)
        {
            foreach (var c in componentsToDisable)
            {
                if (c != null)
                    c.enabled = !disabled;
            }
        }

        if (gameObjectsToDisable != null)
        {
            foreach (var g in gameObjectsToDisable)
            {
                if (g != null)
                    g.SetActive(!disabled);
            }
        }
    }

    private Transform GetRespawnTransform()
    {
        if (respawnPoint != null)
            return respawnPoint;

        GameObject[] candidates;
        try
        {
            candidates = GameObject.FindGameObjectsWithTag(RespawnPointTag);
        }
        catch
        {
            candidates = null;
        }

        if (candidates == null || candidates.Length == 0)
            return null;

        Transform nearest = null;
        float bestDistSqr = float.PositiveInfinity;
        Vector3 myPos = transform.position;

        for (int i = 0; i < candidates.Length; i++)
        {
            var go = candidates[i];
            if (go == null || !go.activeInHierarchy) continue;
            float d = (go.transform.position - myPos).sqrMagnitude;
            if (d < bestDistSqr)
            {
                bestDistSqr = d;
                nearest = go.transform;
            }
        }

        return nearest;
    }

    public void ResetLives()
    {
        remainingRespawns = maxRespawns;
    }
}