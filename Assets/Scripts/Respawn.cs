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
    public float respawnDelay = 0f;

    [Header("What to disable on death")]
    [Tooltip("Behaviour components to disable while 'dead' (e.g. PlayerController).")]
    public Behaviour[] componentsToDisable;
    [Tooltip("GameObjects to disable while 'dead' (e.g. visuals).")]
    public GameObject[] gameObjectsToDisable;

    [Header("Behavior on out of lives")]
    [Tooltip("If true the GameObject will be destroyed when out of respawns.")]
    public bool destroyOnNoRespawns = false;

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

        // Note: we do not overwrite a user-assigned respawnPoint here.
        // If respawnPoint is null, DoRespawn() will attempt to find the nearest object tagged "Respawn Point".
    }

    /// <summary>
    /// Call this from your death handler. Returns true if a respawn was scheduled; false if there are no respawns left.
    /// If the last allowed respawn is consumed (player died maxRespawns times), scene 0 will be loaded.
    /// </summary>
    public bool TryRespawn()
    {
        if (isDead) return false; // already handling a death

        // consume one respawn
        remainingRespawns--;

        // If this consumption used the last respawn, go to the set scene (e.g. Game Over) instead of scheduling another respawn
        if (remainingRespawns <= 0)
        {
            OnOutOfRespawns?.Invoke();
            if (destroyOnNoRespawns)
                Destroy(gameObject);

            // Send player back to the scene # set
            SceneManager.LoadScene(2);
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

    /// <summary>
    /// Returns a Transform to use for respawn.
    /// Priority:
    /// 1) The explicit public respawnPoint (if set in Inspector)
    /// 2) The nearest active GameObject with tag "Respawn Point" (if any)
    /// 3) Fallback to the initial position recorded on Awake.
    /// </summary>
    private Transform GetRespawnTransform()
    {
        if (respawnPoint != null)
            return respawnPoint;

        // Find all objects with the configured tag and pick the nearest one
        GameObject[] candidates;
        try
        {
            candidates = GameObject.FindGameObjectsWithTag(RespawnPointTag);
        }
        catch
        {
            // If the tag does not exist in Tag Manager, FindGameObjectsWithTag will throw.
            // Fall back to no candidates.
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

    /// <summary>
    /// Resets remaining respawns back to maxRespawns.
    /// </summary>
    public void ResetLives()
    {
        remainingRespawns = maxRespawns;
    }
}