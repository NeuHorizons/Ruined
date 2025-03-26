using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    private float nextFireTime = 0f;
    public StatManager statManager;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + statManager.FinalAttackSpeed;
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
            projectileScript.damage += statManager.FinalDamage;
    }

    private void OnDrawGizmos()
    {
        if (firePoint == null) return;
        Gizmos.color = Color.green;
        Vector3 direction = firePoint.right;
        Gizmos.DrawLine(firePoint.position, firePoint.position + direction * 3f);
    }
}