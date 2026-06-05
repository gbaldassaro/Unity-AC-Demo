using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float damage;

    void Awake()
    {
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other) 
    {
        Destroy(gameObject);
    }
}
