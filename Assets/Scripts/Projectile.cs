using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public float speed;

    void Awake()
    {
        Destroy(gameObject, 10f);
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        // using raycast for bullet collision instead of rigidbody
        if (Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime))
        {
            Destroy(gameObject);
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
