using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public float speed;

    // hit particle effect prefab to use
    public ParticleSystem hitEffect; 
    // individual particle effect for parenting
    private ParticleSystem hitParticle;

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
            // destroy bullet and play hit effect
            Destroy(gameObject);
            hitParticle = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            
            // damage object only if it has health
            if (hit.collider.TryGetComponent<Health>(out Health health))
            {
                // sets enemy as parent of individual particle so that when enemy moves/is destroyed, particles do too
                hitParticle.transform.SetParent(hit.transform);
                health.Damage(damage);
            }
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
