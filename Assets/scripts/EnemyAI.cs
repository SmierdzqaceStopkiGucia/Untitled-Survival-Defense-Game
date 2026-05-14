using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public string targetTag = "PlacedObject";
    public float detectionRadius = 0.7f; 

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackRate = 1f; 
    
    private GameObject currentTarget;
    private float attackTimer;
    private Rigidbody2D rb;
    private DayNightCycle timeManager;

    void Start()
    {
        timeManager = Object.FindObjectOfType<DayNightCycle>();
        
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.mass = 5000; 
        }
    }

    void Update()
    {
        if (timeManager != null)
        {
            float current = timeManager.currentHour;
            // Despawn if the time is between 8:00 AM and 10:00 PM
            if (current >= 8f && current < 22f)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (currentTarget == null)
        {
            FindClosestTarget();
        }
    }

    void FixedUpdate()
    {
        if (currentTarget == null) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius);
        
        if (hit != null)
        {
            if (hit.gameObject == currentTarget || hit.CompareTag(targetTag))
            {
                rb.velocity = Vector2.zero;
                AttackTarget(hit.gameObject);
            }
            else 
            {
                MoveTowardsTarget();
            }
        }
        else
        {
            MoveTowardsTarget();
        }
    }

    void MoveTowardsTarget()
    {
        Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    void AttackTarget(GameObject target)
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0;
            
            ObjectHealth oh = target.GetComponent<ObjectHealth>();
            if (oh == null) oh = target.GetComponentInParent<ObjectHealth>();
            if (oh == null) oh = target.GetComponentInChildren<ObjectHealth>();

            if (oh != null)
            {
                oh.TakeDamage(attackDamage);
            }
        }
    }

    void FindClosestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        float closestDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (GameObject t in targets)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = t;
            }
        }
        currentTarget = closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}