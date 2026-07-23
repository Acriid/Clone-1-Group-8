using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossSection : MonoBehaviour
{
    private float _sectionHealth = 400;
    private bool _sectionDestroyed = false;

    public event Action<float> OnBossDamage;
    private Coroutine _moveRoutine = null;
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

        OnBossDamage?.Invoke(damage);
        
    }

    private void DestroySection()
    {
        //TODO - implement the sprite change
    }

    public void MoveSection(Vector2 movePosition, float timeToMove)
    {
        if(_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
        else
        {
            _moveRoutine = StartCoroutine(MoveSectionEnumerator(movePosition,timeToMove));
        }
    }

    private IEnumerator MoveSectionEnumerator(Vector2 movePosition,float timeToMove)
    {
        Vector2 startPosition = transform.position;
        float timeElapsed = 0f;
        while(timeElapsed < timeToMove)
        {
            transform.position = Vector2.Lerp(startPosition, movePosition, timeElapsed / timeToMove);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = movePosition;
    }

}
