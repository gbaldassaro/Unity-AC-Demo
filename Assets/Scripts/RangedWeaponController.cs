using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedWeaponController : MonoBehaviour
{

    [SerializeField] private RangedWeaponData currentRangedWeaponData;

    [SerializeField] private bool rightHand;

    private int currentAmmo;

    public GameObject gunModel;
    private Transform muzzle;

    private bool shootHeld = false;
    private bool canShoot = true;

    void Awake()
    {
        currentAmmo = currentRangedWeaponData.magazineSize;
        gunModel = Instantiate(currentRangedWeaponData.gunModel, transform.position, transform.rotation);
        gunModel.transform.parent = gameObject.transform;
        muzzle = gunModel.transform.Find("Muzzle");
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
            Bullet bullet = Instantiate(currentRangedWeaponData.bulletPrefab, muzzle.position, muzzle.rotation);
            bullet.speed = currentRangedWeaponData.bulletSpeed;
            bullet.damage = currentRangedWeaponData.damagePerBullet;
            canShoot = false;
            StartCoroutine(bulletWaitTimer());
        }
    }

    IEnumerator bulletWaitTimer()
    { 
        yield return new WaitForSeconds(currentRangedWeaponData.timeBetweenBullets);
        canShoot = true;
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