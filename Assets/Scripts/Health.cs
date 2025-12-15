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
	public bool canDie = true;					// Whether or not this health can die
	
	public float startingHealth = 100.0f;		// The amount of health to start with
	public float maxHealth = 100.0f;			// The maximum amount of health
	private float currentHealth;				// The current ammount of health

	public bool replaceWhenDead = false;		// Whether or not a dead replacement should be instantiated.  (Useful for breaking/shattering/exploding effects)
	public GameObject deadReplacement;			// The prefab to instantiate when this GameObject dies
	public bool makeExplosion = false;			// Whether or not an explosion prefab should be instantiated
	public GameObject explosion;				// The explosion prefab to be instantiated

	public bool isPlayer = false;				// Whether or not this health is the player
	public GameObject deathCam;					// The camera to activate when the player dies

	private bool dead = false;					// Used to make sure the Die() function isn't called twice

	// Event: (currentHealth, maxHealth)
	public Action<float, float> OnHealthChanged;

	// Expose current health read-only
	public float CurrentHealth { get { return currentHealth; } }

	private PlayerRespawn playerRespawn;

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
		// This GameObject is officially dead.  This is used to make sure the Die() function isn't called again
		dead = true;

		// Make death effects
		if (replaceWhenDead)
			Instantiate(deadReplacement, transform.position, transform.rotation);
		if (makeExplosion)
			Instantiate(explosion, transform.position, transform.rotation);

		// ensure listeners see zero before destroy
		if (OnHealthChanged != null)
			OnHealthChanged(0f, maxHealth);

		if (isPlayer && deathCam != null)
			deathCam.SetActive(true);

		// Remove this GameObject from the scene
		if (isPlayer)
		{
			playerRespawn.TryRespawn();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void Revive()
	{
		dead = false;
		currentHealth = startingHealth;
		// Notify listeners
		if (OnHealthChanged != null)
			OnHealthChanged(currentHealth, maxHealth);
    }
}
