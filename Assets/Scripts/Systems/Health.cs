using UnityEngine;

public class Health : MonoBehaviour
{

    public float maxHealth;
    public ParticleSystem deathEffect;

    public float currentHealth;

    [SerializeField] private bool player;

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
            currentHealth = 0;
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
        if (player)
        {
            gameObject.SetActive(false);
            MenuManager.singleton.ShowDeathMenu();
        }
        if (!player)
        {
            Destroy(gameObject);
        }
        Instantiate(deathEffect, transform.position, Quaternion.LookRotation(transform.up));
    }

    public void Reset()
    {
        currentHealth = maxHealth;
    }
}
