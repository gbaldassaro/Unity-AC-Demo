using System.Collections;
using UnityEngine;

public enum EnemyState
{
    Searching,
    Engaged
}

public enum FightState
{
    Orbit,
    Chase
}

public class Enemy : MonoBehaviour
{
    private Transform player;

    [HideInInspector] public Vector3 velocitySendToPlayer;
    [HideInInspector] public Vector3 lastPosition;

    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileExitPoint;
    [SerializeField] private float projectileSpread;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float damagePerProjectile;
    [SerializeField] private float timeBetweenProjectiles;

    public float speed = 5f;
    public float magnitude = 10f;
    private Vector3 startPos;

    private bool canShoot = true;
    private bool startupFinished = false;

    void Start()
    {
        lastPosition = this.transform.position;

        startPos = this.transform.position;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(Startup());

    }

    private IEnumerator Startup()
    {
        startupFinished = false;
        yield return new WaitForSecondsRealtime(3.0f);
        startupFinished = true;
    }

    void Update()
    {
        if (!startupFinished)
        {
            return;
        }
        
        projectileExitPoint.transform.LookAt(player);
        if (canShoot && player.gameObject.activeInHierarchy)
        {
            Shoot();
        }

        float x = Mathf.Sin(Time.time * speed) * magnitude;
        this.transform.position = startPos + new Vector3(x, 0, 0);
        velocitySendToPlayer = (this.transform.position - lastPosition) / Time.deltaTime;
        lastPosition = this.transform.position;
    }

    private void Shoot()
    {      
        Vector2 spread = Random.insideUnitCircle * projectileSpread;
        Quaternion spreadAngle = Quaternion.Euler(spread.x, spread.y, 0);
        Projectile projectile = Instantiate(projectilePrefab, projectileExitPoint.position, projectileExitPoint.rotation * spreadAngle);
        projectile.owner = this.transform;
        projectile.speed = projectileSpeed;
        projectile.damage = damagePerProjectile;
        canShoot = false;
        StartCoroutine(ProjectileWaitTimer());
    }

    private IEnumerator ProjectileWaitTimer()
    { 
        yield return new WaitForSeconds(timeBetweenProjectiles);
        canShoot = true;
    }
}
