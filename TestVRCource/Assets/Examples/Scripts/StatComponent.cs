using UnityEngine;

public class StatComponent : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    private float currentStamina;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        Invoke("TakeDamage", 5.0f);
    }

    public void TakeDamage()
    {
        TakeDamage(20.0f);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    public void Die()
    {
        // Обработка смерти персонажа
        Debug.Log(gameObject.name + " died.");
        // Здесь можно добавить деактивацию компонентов, воспроизведение анимации смерти и т.д.
    }

    // Методы для восстановления здоровья и выносливости (опционально)
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void RestoreStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    // Методы для получения текущих значений здоровья и выносливости
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }
}
