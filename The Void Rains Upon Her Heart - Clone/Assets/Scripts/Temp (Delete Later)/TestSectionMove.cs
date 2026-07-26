using UnityEngine;
using System;

public class TestSectionMove : MonoBehaviour
{
    public BossBrain BossBrain;
    public Laser TestLaser;
    public float speed = 100f;

    [SerializeField] private float _sectionSpeed = 4f;
    [SerializeField] private float _movementDistance = 2f;
    [SerializeField] private float _phaseOffset = 0f;

    private Vector2 _startPosition;

    void Awake()
    {
        BossBrain.StartSinAttack();
        TestLaser.ShootLaser(transform);

        QualitySettings.vSyncCount = 0;

        
        Application.targetFrameRate = 60;

        _startPosition = transform.position;
    }

    void Update()
    {
        //transform.Rotate(0f, 0f, speed * Time.deltaTime);


        float movementOffset = Mathf.Sin(Time.time * _sectionSpeed + _phaseOffset) * _movementDistance;

        transform.position = _startPosition + Vector2.up * movementOffset;
    }


}
