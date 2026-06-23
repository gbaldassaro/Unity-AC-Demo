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
    #endregion

    #region Weapon Methods
    private void Shoot()
    {
        projectileExitLight.SetActive(true);
        StartCoroutine(LightWaitTimer());
        
        Projectile projectile = Instantiate(currentRangedWeaponData.projectilePrefab, projectileExitPoint.position, projectileExitPoint.rotation);
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