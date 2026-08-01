using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossHealthManager : MonoBehaviour
{
   [SerializeField] private float _maxHealth = 0f;
   [SerializeField] private BossBrain _bossBrain;
   private List<BossSection> _bossSections;

    void Awake()
    {
        _maxHealth = _bossBrain.GetMaxHealth();
        _bossSections = _bossBrain.GetBossSections();

        foreach(BossSection bossSection in _bossSections)
        {
            bossSection.OnBossDamage += TakeDamage;
        }
    }

    void OnDisable()
    {
        foreach(BossSection bossSection in _bossSections)
        {
            bossSection.OnBossDamage -= TakeDamage;
        }       
    }
    private void TakeDamage(float damage)
    {
        _maxHealth -= damage;
    }
}
