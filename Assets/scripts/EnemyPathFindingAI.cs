using UnityEngine;
using UnityEngine.AI;

public class EnemyPathFindingAI : MonoBehaviour
{
    [Header("Targeting")]
    public string targetTag = "PlacedObject";
    public float detectionRadius = 0.8f;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackRate = 1f;

    private float attackTimer;
    private NavMeshAgent agent;
    private DayNightCycle timeManager;

    private void Start()
    {
        timeManager = Object.FindObjectOfType<DayNightCycle>();
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (timeManager != null)
        {
            float current = timeManager.currentHour;
            if (current >= 8f && current < 22f)
            {
                Destroy(gameObject);
                return;
            }
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius);

        if (hit != null && hit.CompareTag(targetTag))
        {
            agent.isStopped = true;
            AttackTarget(hit.gameObject);
        }
        else
        {
            agent.isStopped = false;

            if (ObjectHealth.MainTarget != null)
            {
                agent.SetDestination(ObjectHealth.MainTarget.position);
            }
        }
    }

    void AttackTarget(GameObject target)
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0;

            ObjectHealth oh = target.GetComponent<ObjectHealth>();
            if (oh == null) oh = target.GetComponentInParent<ObjectHealth>();

            if (oh != null)
            {
                oh.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}