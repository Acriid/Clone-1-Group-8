using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthManager : MonoBehaviour
{
   [SerializeField] private float _maxHealth = 0f;
    [SerializeField] private float _currentHealth = 0f;
    [SerializeField] private BossBrain _bossBrain;
   private List<BossSection> _bossSections;
    [SerializeField] private Slider _HealthSlider;
    [SerializeField] private Slider _DamageSlider;


    void Awake()
    {
        _maxHealth = _bossBrain.GetMaxHealth();
        _bossSections = _bossBrain.GetBossSections();

        _currentHealth = _maxHealth;
        _HealthSlider.maxValue = _maxHealth;
        _HealthSlider.value = _currentHealth;
        _DamageSlider.maxValue = _maxHealth;
        _DamageSlider.value = _currentHealth;

        foreach (BossSection bossSection in _bossSections)
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
        _currentHealth -= damage;
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth); //makes sure current health doesnt fall below 0

    

        if (_currentHealth <= 0)
        {
           BossDeath();
        }

        StartCoroutine(HealthBarAnimation());

    }

    IEnumerator HealthBarAnimation()
    {
       //flash White
        //add shakes when health low
        while (_HealthSlider.value!>= _currentHealth)
        {
            _HealthSlider.value -= 1f;
            yield return new WaitForSeconds(0.05f);

        }

    }

    IEnumerator HealthBarAnimationEnd()
    {
        //_HealthSlider.value = _currentHealth;
        yield return new WaitForSeconds(2f);
        //add shakes when health low
        while (_HealthSlider.value! <= _DamageSlider.value)
        {
            _DamageSlider.value -= 10f;
            yield return new WaitForSeconds(0.05f);

        }

    }

    private void BossDeath()
    {
        StartCoroutine(HealthBarAnimationEnd());
    }

}
