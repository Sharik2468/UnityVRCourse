using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingTextWithPlayerStat : MonoBehaviour
{
    public StatComponent characterStats;
    public StatType statToDisplay = StatType.Health;
    private Text _text;


    void Start()
    {
        _text = GetComponent<Text>();
        characterStats = FindObjectOfType<StatComponent>(); // ��� ��������, ���� � ����� ������ ���� StatComponent
    }


    void Update()
    {
        if (characterStats != null && _text != null)
        {
            switch (statToDisplay)
            {
                case StatType.Health:
                    _text.text = characterStats.GetCurrentHealth().ToString();
                    break;
                case StatType.Stamina:
                    _text.text = characterStats.GetCurrentStamina().ToString();
                    break;
            }
        }
    }


}

