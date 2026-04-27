using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public GameObject prefabToSpawn;                // The prefab that should be spawned
    public GameObject[] prefabsToSpawn;             // Optional: multiple prefabs to choose from (if set, used instead of prefabToSpawn)
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

    // Occasional special enemy spawns (replaces single-spider fields)
    [Header("Occasional Special Enemy Spawns")]
    [Tooltip("Prefabs to use for occasional special spawns (assign one or more prefabs).")]
    public GameObject[] specialEnemyPrefabs;
    [Tooltip("Interval (seconds) between special spawn roll checks.")]
    public float specialSpawnInterval = 20f;
    [Tooltip("Chance (0..1) that a special enemy will spawn when the special timer elapses.")]
    [Range(0f, 1f)]
    public float specialSpawnChance = 0.25f;

    private float spawnTimer = 0.0f;
    private float specialTimer = 0.0f;

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

        // initialize special timer so first check happens after interval
        specialTimer = specialSpawnInterval;
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

        // Special occasional spawn handling (independent of regular waves)
        if (specialEnemyPrefabs != null && specialEnemyPrefabs.Length > 0 && spawnPoints != null && spawnPoints.Length > 0)
        {
            specialTimer += Time.deltaTime;
            if (specialTimer >= specialSpawnInterval)
            {
                specialTimer = 0f;
                TrySpawnSpecialEnemy();
            }
        }

        // Move and turn so that boxes don't keep spawning in the same spots
        transform.Translate(0, 0, moveAmount);
        transform.Rotate(0, turnAmount, 0);
    }

    void Spawn()
    {
        Debug.Log("Spawner: Spawn() called. Current spawned count: " + spawnedInstances.Count);
        // Validate prefab(s) and spawn points
        bool hasAnyPrefab = prefabToSpawn != null || (prefabsToSpawn != null && prefabsToSpawn.Length > 0);
        if (!hasAnyPrefab || spawnPoints == null || spawnPoints.Length == 0)
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

            // Choose which prefab to instantiate
            GameObject chosenPrefab = prefabToSpawn;
            if (prefabsToSpawn != null && prefabsToSpawn.Length > 0)
            {
                chosenPrefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
            }

            if (chosenPrefab == null) continue;

            // Instantiate at the spawn point position and rotation
            GameObject go = Instantiate(chosenPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            // Track the spawned instance for cap enforcement
            spawnedInstances.Add(go);
        }
    }

    // Try to spawn a special enemy occasionally according to chance and cap
    private void TrySpawnSpecialEnemy()
    {
        // Clean up destroyed references first
        CleanUpSpawnedInstances();

        // Check cap
        int availableByCap = (spawnCap <= 0) ? int.MaxValue : Mathf.Max(0, spawnCap - spawnedInstances.Count);
        if (availableByCap <= 0)
        {
            Debug.Log("Spawner: special spawn skipped due to cap.");
            return;
        }

        // Roll chance
        if (Random.value <= specialSpawnChance)
        {
            // pick a random spawn point
            int idx = Random.Range(0, spawnPoints.Length);
            Vector3 pos = spawnPoints[idx].position;
            Quaternion rot = spawnPoints[idx].rotation;

            // pick a random special prefab
            GameObject chosen = specialEnemyPrefabs[Random.Range(0, specialEnemyPrefabs.Length)];
            if (chosen == null) return;

            GameObject go = Instantiate(chosen, pos, rot);
            // do not override prefab tag; prefab's own tag will be preserved
            spawnedInstances.Add(go);
            Debug.Log("Spawner: Special enemy spawned (" + chosen.name + ") at index " + idx + ". Total spawned: " + spawnedInstances.Count);
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