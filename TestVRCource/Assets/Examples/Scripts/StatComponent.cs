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
        // ��������� ������ ���������
        Debug.Log(gameObject.name + " died.");
        // ����� ����� �������� ����������� �����������, ��������������� �������� ������ � �.�.
    }

    // ������ ��� �������������� �������� � ������������ (�����������)
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

    // ������ ��� ��������� ������� �������� �������� � ������������
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }
}
