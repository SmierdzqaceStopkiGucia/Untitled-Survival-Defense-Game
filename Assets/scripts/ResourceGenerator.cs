using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public enum ResourceType { Wood, Stone, Iron, Health }

    [Header("Pickup Settings")]
    public ResourceType resourceType;
    public float amount = 10f;

    [Header("Audio")]
    public AudioClip collectSound;
    [Range(0, 1)] public float volume = 1f;

    private UIManager ui;
    private bool isCollected = false;

    void Start()
    {
        ui = Object.FindObjectOfType<UIManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            Collect();
        }
    }

    void Collect()
    {
        if (ui == null) return;

        isCollected = true;

        switch (resourceType)
        {
            case ResourceType.Wood:
                ui.wood += amount;
                break;
            case ResourceType.Stone:
                ui.stone += (int)amount;
                break;
            case ResourceType.Iron:
                ui.iron += amount;
                break;
            case ResourceType.Health:
                ui.health += amount;
                break;
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, volume);
        }

        Destroy(gameObject);
    }
}