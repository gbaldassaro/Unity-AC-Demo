using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;

    void Awake()
    {
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) 
    {
        Destroy(gameObject);
    }
}
