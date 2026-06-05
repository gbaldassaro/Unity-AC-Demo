using UnityEngine;

[CreateAssetMenu(fileName = "RangedWeaponData", menuName = "Scriptable Objects/RangedWeaponData")]
public class RangedWeaponData : ScriptableObject
{
    public string weaponName;

    public GameObject gunModel;

    public Bullet bulletPrefab;
    public float bulletSpeed;

    public float timeBetweenBullets;
    public float damagePerBullet;

    public int magazineSize;
    public float reloadTime;

    public float bulletSpread;
}
