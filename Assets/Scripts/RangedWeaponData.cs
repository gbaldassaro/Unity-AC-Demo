using UnityEngine;

[CreateAssetMenu(fileName = "RangedWeaponData", menuName = "Scriptable Objects/RangedWeaponData")]
public class RangedWeaponData : ScriptableObject
{
    public string weaponName;

    public GameObject gunModel;

    public Projectile projectilePrefab;
    public float projectileSpeed;

    public float timeBetweenProjectiles;
    public float firstShotDelay;
    public float damagePerProjectile;

    public int maxAmmo;
    public float reloadTime;

    public float projectileSpread;
}
