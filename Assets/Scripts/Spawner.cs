/// <summary>
/// Spawner.cs
/// Author: MutantGopher
/// This is a sample spawning script used to spawn the red cubes in the demo scene.
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
	public GameObject prefabToSpawn;				// The prefab that should be spawned
	public float spawnFrequency = 6.0f;				// The time (in seconds) between spawns
	public bool spawnOnStart = false;				// Whether or not one instance of the prefab should be spawned on Start()
	public bool move = true;						// Move this spawn spot around
	public float moveAmount = 5.0f;					// The amount to move
	public float turnAmount = 5.0f;                 // The amount to turn
	public Transform[] spawnPoints;                 // An array of possible spawn points
	public int spawnPerWave = 3;                    // How many prefabs to spawn each time the Spawn() is called

    private float spawnTimer = 0.0f;

	// Use this for initialization
	void Start()
	{
		if (spawnOnStart)
		{
			Spawn();
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		// Update the spawning timer
		spawnTimer += Time.deltaTime;

		// Spawn a prefab if the timer has reached spawnFrequency
		if (spawnTimer >= spawnFrequency)
		{
			// First reset the spawn timer to 0
			spawnTimer = 0.0f;
			Spawn();
		}

		// Move and turn so that boxes don't keep spawning in the same spots
		transform.Translate(0, 0, moveAmount);
		transform.Rotate(0, turnAmount, 0);
	}

	void Spawn()
	{
		// Validate prefab and spawn points
		if (prefabToSpawn == null || spawnPoints == null || spawnPoints.Length == 0)
			return;

		// Ensure we don't request more spawns than available unique spawn points
		int count = Mathf.Clamp(spawnPerWave, 1, spawnPoints.Length);

		// Build a list of available indices and pick unique random ones
		List<int> available = new List<int>(spawnPoints.Length);
		for (int i = 0; i < spawnPoints.Length; i++)
			available.Add(i);

		for (int i = 0; i < count; i++)
		{
			int pickIndex = Random.Range(0, available.Count);
			int spawnIndex = available[pickIndex];
			available.RemoveAt(pickIndex);

			// Instantiate at the spawn point position and rotation
			Instantiate(prefabToSpawn, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
		}
	}
}

