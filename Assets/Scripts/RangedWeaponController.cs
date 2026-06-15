using System.Collections;
using UnityEngine;

public class RangedWeaponController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Player Input")]
    [SerializeField] private InputHandler input;

    [Header("Weapon Data")]
    [SerializeField] private RangedWeaponData currentRangedWeaponData;

    [SerializeField] private bool rightHand;
    #endregion

    #region Private Fields
    private int currentAmmo;

    private GameObject gunModel;
    private Transform projectileExitPoint;
    private GameObject projectileExitLight;

    private bool canShoot = true;
    #endregion

    #region Game Loop
    void Awake()
    {
        currentAmmo = currentRangedWeaponData.magazineSize;
        gunModel = Instantiate(currentRangedWeaponData.gunModel, transform.position, transform.rotation);
        gunModel.transform.parent = gameObject.transform;
        projectileExitPoint = gunModel.transform.Find("Projectile Exit Point");
        projectileExitLight = projectileExitPoint.transform.Find("Projectile Exit Light").gameObject;
    }

    void Update()
    {
        if (input.shootRightHeld && rightHand || input.shootLeftHeld && !rightHand)
        {
            Shoot();
        }
    }
    #endregion

    #region Weapon Methods
    void Shoot()
    {
        if (canShoot)
        {
            projectileExitLight.SetActive(true);
            StartCoroutine(lightWaitTimer());
            
            Projectile projectile = Instantiate(currentRangedWeaponData.projectilePrefab, projectileExitPoint.position, projectileExitPoint.rotation);
            projectile.speed = currentRangedWeaponData.projectileSpeed;
            projectile.damage = currentRangedWeaponData.damagePerProjectile;
            canShoot = false;
            StartCoroutine(projectileWaitTimer());
        }
    }

    IEnumerator projectileWaitTimer()
    { 
        yield return new WaitForSeconds(currentRangedWeaponData.timeBetweenProjectiles);
        canShoot = true;
    }

    IEnumerator lightWaitTimer()
    { 
        yield return new WaitForSeconds(0.05f);
        projectileExitLight.SetActive(false);
    }
    #endregion
}