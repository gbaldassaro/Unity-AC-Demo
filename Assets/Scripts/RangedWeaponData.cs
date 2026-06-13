using UnityEngine;

[CreateAssetMenu(fileName = "RangedWeaponData", menuName = "Scriptable Objects/RangedWeaponData")]
public class RangedWeaponData : ScriptableObject
{
    public string weaponName;

    public GameObject gunModel;

    public Projectile projectilePrefab;
    public float projectileSpeed;

    public float timeBetweenProjectiles;
    public float damagePerProjectile;

    public int magazineSize;
    public float reloadTime;

    public float projectileSpread;
}
