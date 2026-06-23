using UnityEngine;

public class Health : MonoBehaviour
{

    public float maxHealth;
    public ParticleSystem deathEffect;

    public float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void Damage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            Kill();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    private void Kill()
    {
        Destroy(gameObject);
        Instantiate(deathEffect, transform.position, Quaternion.LookRotation(transform.up));
    }
}
