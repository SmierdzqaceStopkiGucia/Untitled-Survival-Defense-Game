using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] spawnPrefabs; // Items that can be spawned
    [Range(0f, 100f)] public float spawnChance = 50f; // Percentage chance to spawn
    public int spawnCount = 1; // How many items to try and spawn per click
    public float spawnRadius = 2.0f; // How far away they can spawn

    [Header("Audio")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        // Add an AudioSource if one doesn't exist
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnMouseDown()
    {
        // Play click sound
        if (clickSound != null) audioSource.PlayOneShot(clickSound);

        for (int i = 0; i < spawnCount; i++)
        {
            // Roll the dice!
            float roll = Random.Range(0f, 100f);

            if (roll <= spawnChance)
            {
                SpawnNearby();
            }
        }
    }

    void SpawnNearby()
    {
        if (spawnPrefabs.Length == 0) return;

        // 1. Get a random point inside a circle
        Vector2 randomCirclePoint = Random.insideUnitCircle * spawnRadius;
        
        // 2. Add that point to our current position
        Vector3 spawnPosition = new Vector3(
            transform.position.x + randomCirclePoint.x,
            transform.position.y + randomCirclePoint.y,
            transform.position.z // Keeps it on the same Z plane
        );

        // 3. Pick a random prefab from the list
        GameObject randomPrefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];

        // 4. Spawn it
        Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
    }

    // Helps you see the spawn area in the Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}