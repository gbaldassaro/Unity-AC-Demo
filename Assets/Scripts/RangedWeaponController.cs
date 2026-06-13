using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedWeaponController : MonoBehaviour
{

    [SerializeField] private RangedWeaponData currentRangedWeaponData;

    [SerializeField] private bool rightHand;

    private int currentAmmo;

    private GameObject gunModel;
    private Transform projectileExitPoint;
    private GameObject projectileExitLight;

    private bool shootHeld = false;
    private bool canShoot = true;

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
        if (shootHeld)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (canShoot)
        {
            projectileExitLight.SetActive(true);
            StartCoroutine(lightWaitTimer());
            Projectile projectile = Instantiate(currentRangedWeaponData.projectilePrefab, projectileExitPoint.position, projectileExitPoint.rotation);
            projectile.GetComponent<Rigidbody>().linearVelocity = projectile.transform.forward * currentRangedWeaponData.projectileSpeed;
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

    public void OnShootRight(InputAction.CallbackContext context)
    {
        if (context.performed && rightHand)
        {
            shootHeld = !shootHeld;
        }
    }

    public void OnShootLeft(InputAction.CallbackContext context)
    {
        if (context.performed && !rightHand)
        {
            shootHeld = !shootHeld;
        }
    }
}