using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillingBarWithPlayerStat : MonoBehaviour
{
    public StatComponent characterStats;
    public StatType statToDisplay = StatType.Health;
    public float _speed;
    protected Image _image;


    void Start()
    {
        _image = GetComponent<Image>();
        characterStats = FindObjectOfType<StatComponent>(); // Это работает, если в сцене только один StatComponent
    }


    void Update()
    {
        if (characterStats != null && _image != null)
        {
            float statPercentage = 0f;

            switch (statToDisplay)
            {
                case StatType.Health:
                    statPercentage = characterStats.GetCurrentHealth() / characterStats.maxHealth;
                    break;
                case StatType.Stamina:
                    statPercentage = characterStats.GetCurrentStamina() / characterStats.maxStamina;
                    break;
            }

            _image.fillAmount = Mathf.MoveTowards(_image.fillAmount, statPercentage, _speed * Time.deltaTime);
        }
    }


}

public enum StatType
{
    Health,
    Stamina
}

