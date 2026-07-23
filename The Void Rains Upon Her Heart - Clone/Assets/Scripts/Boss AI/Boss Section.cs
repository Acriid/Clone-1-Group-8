using System;
using UnityEngine;

public class BossSection : MonoBehaviour
{
    private float _sectionHealth = 400;
    private bool _sectionDestroyed = false;

    public event Action<float> OnBossDamage;
    public void Damage(float damage)
    {
        if(_sectionDestroyed)
        {
            damage *= 0.1f;
        }
        else
        {
            _sectionHealth -= damage;
            if(_sectionHealth <= 0)
            {
                _sectionDestroyed = true;
            }
        }




    }

    private void DestroySection()
    {
        //TODO - implement the sprite change
    }
}
