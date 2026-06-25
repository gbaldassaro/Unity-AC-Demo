using System.Collections;
using UnityEngine;

public class RangedWeaponController : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private InputHandler input;

    [Header("Weapon Data")]
    [SerializeField] private RangedWeaponData currentRangedWeaponData;

    [SerializeField] private bool rightHand;

    [HideInInspector] public int currentAmmo;

    private GameObject gunModel;
    private Transform projectileExitPoint;
    private GameObject projectileExitLight;

    private bool canShoot = true;
    private bool reloading = false;

    [Header("Enemy Tracking")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Transform rightAimAtPoint;
    [SerializeField] private Transform leftAimAtPoint;
    private Enemy currentEnemy;
    private Vector3 currentEnemyVelocity;


    #region Game Loop
    private void Awake()
    {
        currentAmmo = currentRangedWeaponData.maxAmmo;
        gunModel = Instantiate(currentRangedWeaponData.gunModel, transform.position, transform.rotation);
        gunModel.transform.parent = gameObject.transform;
        projectileExitPoint = gunModel.transform.Find("Projectile Exit Point");
        projectileExitLight = projectileExitPoint.transform.Find("Projectile Exit Light").gameObject;
    }

    private void Update()
    {   
        if (!input.shiftControlHeld && canShoot && !reloading && currentAmmo != 0 && 
        ((input.shootRightHeld && rightHand && Time.realtimeSinceStartup - input.shootRightHeldStartTime > currentRangedWeaponData.firstShotDelay) || 
        (input.shootLeftHeld && !rightHand && Time.realtimeSinceStartup - input.shootLeftHeldStartTime > currentRangedWeaponData.firstShotDelay)))
        {
            Shoot();
        }

        if (input.shiftControlHeld && !reloading && 
        ((input.shootRightHeld && rightHand) || (input.shootLeftHeld && !rightHand)))
        {
            StartCoroutine(Reload());
        }
    }

    private void FixedUpdate()
    {
        TrackEnemy();
    }
    #endregion

    #region Weapon Methods
    private void TrackEnemy()
    {
        if (cameraController.currentEnemy != null)
        {
            currentEnemy = cameraController.currentEnemy;
            currentEnemyVelocity = currentEnemy.velocitySendToPlayer;
            float distanceToEnemy = (currentEnemy.transform.position - projectileExitPoint.position).magnitude;
            float angle = Vector3.Angle(currentEnemy.transform.position - projectileExitPoint.position, currentEnemyVelocity);
            float a = Mathf.Pow(currentRangedWeaponData.projectileSpeed, 2) - Mathf.Pow(currentEnemyVelocity.magnitude, 2);
            float b = 2 * distanceToEnemy * currentEnemyVelocity.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
            float c = -Mathf.Pow(distanceToEnemy, 2);
            float t;
            if (a != 0)
            {
                t = (-b + Mathf.Sqrt(b * b - (4 * a * c))) / (2 * a);
            }
            else t = 0;

            if (rightHand)
            {
                rightAimAtPoint.position = currentEnemy.transform.position + (currentEnemyVelocity * t);
            }
            else
            {
                leftAimAtPoint.position = currentEnemy.transform.position + (currentEnemyVelocity * t);
            }
        }
    }
    
    private void Shoot()
    {
        projectileExitLight.SetActive(true);
        StartCoroutine(LightWaitTimer());
        
        Vector2 spread = Random.insideUnitCircle * currentRangedWeaponData.projectileSpread;
        Quaternion spreadAngle = Quaternion.Euler(spread.x, spread.y, 0);
        Projectile projectile = Instantiate(currentRangedWeaponData.projectilePrefab, projectileExitPoint.position, projectileExitPoint.rotation * spreadAngle);
        projectile.speed = currentRangedWeaponData.projectileSpeed;
        projectile.damage = currentRangedWeaponData.damagePerProjectile;
        currentAmmo -= 1;
        canShoot = false;
        StartCoroutine(ProjectileWaitTimer());
    }
    
    private IEnumerator Reload()
    {
        reloading = true;
        yield return new WaitForSeconds(currentRangedWeaponData.reloadTime);
        currentAmmo = currentRangedWeaponData.maxAmmo;
        reloading = false;
    }
    #endregion

    #region Timers
    private IEnumerator ProjectileWaitTimer()
    { 
        yield return new WaitForSeconds(currentRangedWeaponData.timeBetweenProjectiles);
        canShoot = true;
    }

    private IEnumerator LightWaitTimer()
    { 
        yield return new WaitForSeconds(0.05f);
        projectileExitLight.SetActive(false);
    }
    #endregion
}