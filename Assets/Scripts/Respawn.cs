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
    [Tooltip("Where the player will be moved when respawning. If null, player will respawn at this object's starting position.")]
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

    [Header("Events")]
    public UnityEvent OnRespawnScheduled;
    public UnityEvent OnRespawned;
    public UnityEvent OnOutOfRespawns;

    int remainingRespawns;
    Vector3 initialPosition;
    Quaternion initialRotation;
    bool isDead;

    public int RemainingRespawns => remainingRespawns;

    void Awake()
    {
        remainingRespawns = maxRespawns;
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (respawnPoint == null)
        {
            // keep null — we'll use initialPosition as fallback
        }
    }

    /// <summary>
    /// Call this from your death handler. Returns true if a respawn was scheduled; false if there are no respawns left.
    /// If the last allowed respawn is consumed (player died maxRespawns times), scene 0 will be loaded.
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

            // Send player back to scene 0
            SceneManager.LoadScene(0);
            return false;
        }

        // consume one respawn
        remainingRespawns--;

        // If this consumption used the last respawn, go to scene 0 instead of scheduling another respawn
        if (remainingRespawns <= 0)
        {
            OnOutOfRespawns?.Invoke();
            if (destroyOnNoRespawns)
                Destroy(gameObject);

            // Send player back to scene 0
            SceneManager.LoadScene(0);
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
        Vector3 targetPos = respawnPoint != null ? respawnPoint.position : initialPosition;
        Quaternion targetRot = respawnPoint != null ? respawnPoint.rotation : initialRotation;

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
    /// Resets remaining respawns back to maxRespawns.
    /// </summary>
    public void ResetLives()
    {
        remainingRespawns = maxRespawns;
    }
}