using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AreaSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    public DayNightCycle timeManager; 

    [Header("Spawn Settings")]
    public GameObject[] prefabsToSpawn;
    public float spawnInterval = 5f; 
    [Range(0, 100)] public float spawnChance = 50f;

    [Header("Visuals")]
    public Color debugBoundsColor = new Color(0, 1, 0, 0.3f);

    private BoxCollider2D spawnArea;
    private float timer;

    void Start()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        spawnArea.isTrigger = true;

        if (timeManager == null)
        {
            timeManager = Object.FindObjectOfType<DayNightCycle>();
        }
    }

    void Update()
    {
        if (timeManager == null) return;

        float current = timeManager.currentHour;

        bool isNightTime = (current >= 22f || current < 8f);

        if (!isNightTime)
        {
            timer = 0; 
            return; 
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0;
            AttemptSpawn();
        }
    }

    void AttemptSpawn()
    {
        if (prefabsToSpawn.Length == 0 || Random.Range(0f, 100f) > spawnChance) return;

        Bounds bounds = spawnArea.bounds;
        Vector3 spawnPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            0
        );

        GameObject prefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        if (spawnArea == null) spawnArea = GetComponent<BoxCollider2D>();
        
        Gizmos.color = debugBoundsColor;
        Gizmos.DrawCube(spawnArea.bounds.center, spawnArea.bounds.size);
        
        Gizmos.color = new Color(debugBoundsColor.r, debugBoundsColor.g, debugBoundsColor.b, 1f);
        Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
    }
}