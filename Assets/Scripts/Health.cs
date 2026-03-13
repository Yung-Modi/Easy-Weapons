/// <summary>
/// Health.cs
/// Author: MutantGopher
/// This is a sample health script.  If you use a different script for health,
/// make sure that it is called "Health".  If it is not, you may need to edit code
/// referencing the Health component from other scripts
/// </summary>

using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    public bool canDie = true;                  // Whether or not this health can die

    public float startingHealth = 100.0f;       // The amount of health to start with
    public float maxHealth = 100.0f;            // The maximum amount of health
    private float currentHealth;                // The current ammount of health

    public bool replaceWhenDead = false;        // Whether or not a dead replacement should be instantiated.  (Useful for breaking/shattering/exploding effects)
    public GameObject deadReplacement;          // The prefab to instantiate when this GameObject dies
    public bool makeExplosion = false;          // Whether or not an explosion prefab should be instantiated
    public GameObject explosion;                // The explosion prefab to be instantiated

    public bool isPlayer = false;               // Whether or not this health is the player
    public GameObject deathCam;                 // The camera to activate when the player dies

    private bool dead = false;                  // Used to make sure the Die() function isn't called twice

    // Event: (currentHealth, maxHealth)
    public Action<float, float> OnHealthChanged;

    // Static event fired when any Nuke-tagged object dies
    public static Action OnNukeDestroyed;

    // Expose current health read-only
    public float CurrentHealth { get { return currentHealth; } }

    private PlayerRespawn playerRespawn;

    [Header("Nuke behaviour")]
    [Tooltip("If this GameObject has the tag 'Nuke', it will automatically Die after this many seconds.")]
    public float nukeLifetime = 5f;
    // internal coroutine handle so we can stop/restart the countdown if needed
    private Coroutine nukeCoroutine;

    // Use this for initialization
    void Start()
    {
        // Initialize the currentHealth variable to the value specified by the user in startingHealth
        currentHealth = startingHealth;

        if (isPlayer)
        {
            playerRespawn = GetComponent<PlayerRespawn>();
        }

        // Notify listeners of initial value
        if (OnHealthChanged != null)
            OnHealthChanged(currentHealth, maxHealth);

        // If this object is a "Nuke" start the timed death countdown
        if (CompareTag("Nuke") && canDie)
        {
            // ensure any existing coroutine is stopped (defensive)
            if (nukeCoroutine != null) StopCoroutine(nukeCoroutine);
            nukeCoroutine = StartCoroutine(NukeCountdown());
        }
    }

    public void ChangeHealth(float amount)
    {
        // Change the health by the amount specified in the amount variable
        currentHealth += amount;

        // Clamp to [0, maxHealth]
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        if (currentHealth < 0)
            currentHealth = 0;

        // Notify listeners
        if (OnHealthChanged != null)
            OnHealthChanged(currentHealth, maxHealth);

        // If the health runs out, then Die.
        if (currentHealth <= 0 && !dead && canDie)
            Die();
    }

    public void Die()
    {
        // Prevent multiple calls
        if (dead) return;
        // This GameObject is officially dead.  This is used to make sure the Die() function isn't called again
        dead = true;

        // Stop any pending nuke countdown (defensive)
        if (nukeCoroutine != null)
        {
            StopCoroutine(nukeCoroutine);
            nukeCoroutine = null;
        }

        // If this was a Nuke, notify listeners before destruction
        if (CompareTag("Nuke"))
        {
            OnNukeDestroyed?.Invoke();
            Debug.Log("Nuke destroyed, OnNukeDestroyed event invoked.");
        }

        // Make death effects
        if (replaceWhenDead)
            Instantiate(deadReplacement, transform.position, transform.rotation);
        if (makeExplosion)
            Instantiate(explosion, transform.position, transform.rotation);
        Debug.Log(gameObject.name + " has died.");

        // ensure listeners see zero before destroy
        if (OnHealthChanged != null)
            OnHealthChanged(0f, maxHealth);

        // Remove this GameObject from the scene
        if (isPlayer)
        {
            // If a PlayerRespawn component exists, try to respawn.
            // TryRespawn() returns true when a respawn is scheduled; false when no respawns left.
            bool respawned = true;
            if (playerRespawn != null)
            {
                respawned = playerRespawn.TryRespawn();
                Debug.Log("Player died. Respawn attempted: " + respawned);
            }
            else
            {
                // No respawn system present -> treat as no respawns left
                respawned = false;
                Debug.LogWarning("Player died but no PlayerRespawn component found. Game over.");
            }

            // If we could not respawn the player, notify GameManager of game over.
            if (!respawned)
            {
                GameManager.Instance?.GameOver();
                Debug.Log("Game over. Notifying GameManager.");
            }
        }
        else
        {
            GameManager.Instance.EnemyKilled();
            Destroy(gameObject);
            Debug.Log(gameObject.name + " destroyed and GameManager notified of enemy kill.");
        }
    }

    public void Revive()
    {
        dead = false;
        currentHealth = startingHealth;
        // Notify listeners
        if (OnHealthChanged != null)
            OnHealthChanged(currentHealth, maxHealth);

        // If this object is a Nuke, restart the countdown when revived
        if (CompareTag("Nuke") && canDie)
        {
            if (nukeCoroutine != null) StopCoroutine(nukeCoroutine);
            nukeCoroutine = StartCoroutine(NukeCountdown());
        }
    }

    // Countdown coroutine for objects tagged "Nuke"
    private IEnumerator NukeCountdown()
    {
        // Wait for the configured lifetime
        yield return new WaitForSeconds(nukeLifetime);

        // Double-check conditions are still valid before killing
        if (!dead && canDie)
        {
            Die();
        }
    }

}