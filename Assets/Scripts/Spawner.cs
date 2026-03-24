using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public GameObject prefabToSpawn;                // The prefab that should be spawned
    public float spawnFrequency = 6.0f;             // The time (in seconds) between spawns
    public bool spawnOnStart = false;               // Whether or not one instance of the prefab should be spawned on Start()
    public bool move = true;                        // Move this spawn spot around
    public float moveAmount = 5.0f;                 // The amount to move
    public float turnAmount = 5.0f;                 // The amount to turn
    public Transform[] spawnPoints;                 // An array of possible spawn points
    public int spawnPerWave = 3;                    // How many prefabs to spawn each time the Spawn() is called

    [Tooltip("Maximum number of spawned instances allowed at the same time. Set to 0 for unlimited.")]
    public int spawnCap = 20;                       // 0 means no cap (unlimited)

    [Header("Nuke gating")]
    [Tooltip("If true, this Spawner will wait until a GameObject tagged 'Nuke' dies before spawning.")]
    public bool requireNukeDeath = true;

    private float spawnTimer = 0.0f;

    // Track spawned instances so we can enforce the cap
    private readonly List<GameObject> spawnedInstances = new List<GameObject>();

    // Internal flag set when a Nuke has died
    private bool nukeDied = false;

    // Use this for initialization
    void Start()
    {
        if (spawnOnStart && (!requireNukeDeath || nukeDied))
        {
            Spawn();
        }

        // Subscribe to Health's Nuke death event if required
        if (requireNukeDeath)
        {
            nukeDied = false;
            Health.OnNukeDestroyed += HandleNukeDestroyed;
        }
        else
        {
            nukeDied = true;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid leaks
        if (requireNukeDeath)
            Health.OnNukeDestroyed -= HandleNukeDestroyed;
    }

    // Update is called once per frame
    void Update()
    {
        // Clean up any destroyed instances so our count stays accurate
        CleanUpSpawnedInstances();

        // Update the spawning timer and spawn only if gating is satisfied
        if (!requireNukeDeath || nukeDied)
        {
            spawnTimer += Time.deltaTime;

            // Spawn a prefab if the timer has reached spawnFrequency
            if (spawnTimer >= spawnFrequency)
            {
                // First reset the spawn timer to 0
                spawnTimer = 0.0f;
                Spawn();
            }
        }

        // Move and turn so that boxes don't keep spawning in the same spots
        transform.Translate(0, 0, moveAmount);
        transform.Rotate(0, turnAmount, 0);
    }

    void Spawn()
    {
        Debug.Log("Spawner: Spawn() called. Current spawned count: " + spawnedInstances.Count);
        // Validate prefab and spawn points
        if (prefabToSpawn == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        // Clean up destroyed references before calculating cap
        CleanUpSpawnedInstances();

        // If a cap is set (> 0), compute how many we are allowed to spawn
        int availableByCap = (spawnCap <= 0) ? int.MaxValue : Mathf.Max(0, spawnCap - spawnedInstances.Count);
        if (availableByCap <= 0)
            return; // reached cap, do not spawn any more

        // Ensure we don't request more spawns than available unique spawn points or cap allows
        int count = Mathf.Clamp(spawnPerWave, 1, Mathf.Min(spawnPoints.Length, availableByCap));

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
            GameObject go = Instantiate(prefabToSpawn, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            // Track the spawned instance for cap enforcement
            spawnedInstances.Add(go);
        }
    }

    // Remove null entries from the spawnedInstances list (destroyed GameObjects become null)
    private void CleanUpSpawnedInstances()
    {
        if (spawnedInstances.Count == 0) return;
        spawnedInstances.RemoveAll(item => item == null);
    }

    // Handler for the global Nuke-death event
    private void HandleNukeDestroyed()
    {
        nukeDied = true;
        Spawn();
    }
}