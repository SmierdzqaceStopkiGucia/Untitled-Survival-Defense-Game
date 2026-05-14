using UnityEngine;

public class TurretAI : MonoBehaviour
{
    [Header("Targeting")]
    public string enemyTag = "Enemy"; 
    public float range = 5f;
    public float fireRate = 1f;

    [Header("Combat")]
    public float damage = 10f;

    [Header("Visuals (Optional)")]
    public LineRenderer laserLine; 
    public float laserDuration = 0.05f;

    private float fireTimer;
    private Transform currentTarget;

    void Update()
    {
        FindClosestEnemy();

        if (currentTarget != null)
        {
            fireTimer += Time.deltaTime;

            if (fireTimer >= 1f / fireRate)
            {
                Shoot();
                fireTimer = 0f;
            }
        }
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            currentTarget = nearestEnemy.transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    void Shoot()
    {
        if (currentTarget == null) return;

        ObjectHealth enemyHealth = currentTarget.GetComponent<ObjectHealth>();
        if (enemyHealth == null) enemyHealth = currentTarget.GetComponentInParent<ObjectHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        if (laserLine != null)
        {
            StartCoroutine(ShootEffect());
        }
    }

    private System.Collections.IEnumerator ShootEffect()
    {
        laserLine.SetPosition(0, transform.position);
        laserLine.SetPosition(1, currentTarget.position);
        laserLine.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}