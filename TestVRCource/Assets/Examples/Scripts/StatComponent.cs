using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatComponent : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    private float currentStamina;

    [Header("Death Settings")]
    public GameObject DeathScreen;
    public float DelayBeforeRestartLevel;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
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

        if (DeathScreen != null)
        {
            DeathScreen.SetActive(true);
        }

        // �������� ����� ������������� ������, ����� ������ ����� ������� ����� ������
        StartCoroutine(ReloadLevelAfterDelay(DelayBeforeRestartLevel));
    }

    private IEnumerator ReloadLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ������������ �������� ������
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
