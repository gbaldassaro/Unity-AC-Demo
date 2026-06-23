using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public float speed;

    public ParticleSystem hitEffect; 

    private void Awake()
    {
        Destroy(gameObject, 10f);
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        // using raycast for bullet collision instead of rigidbody
        if (Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime))
        {
            Destroy(gameObject);
            Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            if (hit.collider.TryGetComponent<Health>(out Health health))
            {
                health.Damage(damage);
            }
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
