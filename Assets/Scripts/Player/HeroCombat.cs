using UnityEngine;

public class HeroCombat : MonoBehaviour
{
    public enum HeroAttackType { Melee, Ranged }
    public HeroAttackType heroAttackType;

    public GameObject targetedEnemy;
    public float attackRange;
    public float rotateSpeedForAttack;

    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    private float attackCooldown = 0f;
    private float attackTimer = 0f;

    private HeroStats heroStats;
    private Movement moveScript;

    public bool isHeroAlive = true;

    void Start()
    {
        moveScript = GetComponent<Movement>();
        heroStats = GetComponent<HeroStats>();
        attackCooldown = 1f / heroStats.attackSpeed;
    }

    void Update()
    {
        if (!isHeroAlive) return;

        attackTimer += Time.deltaTime;

        if (targetedEnemy != null)
        {
            float distance = Vector3.Distance(transform.position, targetedEnemy.transform.position);

            if (distance > attackRange)
            {
                moveScript.agent.SetDestination(targetedEnemy.transform.position);
                moveScript.agent.stoppingDistance = attackRange;
                RotateTowardsTarget();
            }
            else
            {
                moveScript.agent.SetDestination(transform.position);
                RotateTowardsTarget();

                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    if (heroAttackType == HeroAttackType.Ranged)
                    {
                        FireProjectile();
                    }
                }
            }
        }
    }

    void RotateTowardsTarget()
    {
        Quaternion rotationToLookAt = Quaternion.LookRotation(targetedEnemy.transform.position - transform.position);
        float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationToLookAt.eulerAngles.y, ref moveScript.rotateVelocity, rotateSpeedForAttack * Time.deltaTime * 5);
        transform.eulerAngles = new Vector3(0, rotationY, 0);
    }

    void FireProjectile()
    {
        if (targetedEnemy != null)
        {
            // Force rotation toward the target (fixes broken aim when standing still)
            Quaternion lookRotation = Quaternion.LookRotation(targetedEnemy.transform.position - transform.position);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);

            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Projectile projectileScript = proj.GetComponent<Projectile>();
            projectileScript.damage = heroStats.attackDamage;

            Vector3 flatDirection = targetedEnemy.transform.position - projectileSpawnPoint.position;
            flatDirection.y = 0;

            projectileScript.direction = flatDirection.normalized;
            proj.transform.rotation = Quaternion.LookRotation(flatDirection);
        }
    }

}
