using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossBrain : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private BoundsInt _arenaBounds;
    [SerializeField] private LaserSO _phase2LaserSO;
    [SerializeField] private GameObject _sectionsParent;
    [SerializeField] private BossSection _negativeSection;
    [SerializeField] private BossSection _positiveSection;
    
    [SerializeField] private float _sectionsParentRotationSpeed = 40f;
    [SerializeField] private float _bulletSectionRotationSpeed = 60f;
    private BoundsInt _leftArenaBounds;
    [SerializeField] private List<BossSection> _sectionList = new(4);
    [Header("General Attacks")]
    [SerializeField] private float _attackDelay = 2f;
    [SerializeField] private float _timeToMoveToAttackPosition = 2f;
    [Header("SinWave Attack")]
    [SerializeField] private float _sinWaveAttackTime = 6.5f;
    [SerializeField] private float _sinShotSpeed = 0.2f;
    [Header("SpinningBullet Attack")]
    [SerializeField] private float _spinningBulletTime = 3f;

    private List<BossSection> _section1 = new(2);
    private List<BossSection> _section2 = new(2);
    private List<bool> _sectionsFinishedMoving = new(4)
    {
        false,
        false,
        false,
        false,
    };

    private List<bool> _sectionsFinishedLaser = new(4)
    {
        false,
        false,
        false,
        false,
    };

    private List<bool> _sectionsBroken = new(4)
    {
        false,
        false,
        false,
        false,
    };

    //SinWave = 0
    //XLaser = 1
    //LineLaser = 2
    //SpinningBullet = 3

    //Laser attacks will always aim at the other side of the screen past the middle.
    //2/3 for x 1/2 for y all from the centre

    //Phase 2
    //All attacks happen one after another
    //FourBullets = 0
    //BulletSpread = 1
    private int _previousAttack = -1;
    private event Action _onAttackDone;
    void Start()
    {


        int splitX = _arenaBounds.xMin + _arenaBounds.size.x / 2;

       _leftArenaBounds = new BoundsInt(
            new Vector3Int(_arenaBounds.xMin, _arenaBounds.yMin, _arenaBounds.zMin),
            new Vector3Int(splitX - _arenaBounds.xMin, _arenaBounds.size.y, _arenaBounds.size.z));


        _onAttackDone += SendAttack;
        _onAttackDone?.Invoke();

        //TestPhase2();
        //StartCoroutine(GoPhase2AfterTime());
    }

    void OnDisable()
    {
        _onAttackDone -= SendAttack;
        _onAttackDone -= SendPhase2Attack;
    }
    private IEnumerator GoPhase2AfterTime()
    {
        yield return new WaitForSeconds(60f);
        TestPhase2();
    }
    private void TestPhase2()
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.TESTDESTROYSECTION();
            OnSectionBroken(bossSection);
        }
    }
    private void OnSectionBroken(BossSection bossSection)
    {
        bossSection.OnSectionDestroyed -= OnSectionBroken;
        _sectionsBroken[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsBroken)
        {
            if(!booleans) return;
        }

        StopAllCoroutines();
        foreach(BossSection bossSection1 in _sectionList)
        {
            bossSection1.StopLaser();
            bossSection1.StopAllRoutines();
        }
        _onAttackDone -= SendAttack;
        _onAttackDone += SendPhase2Attack;
        StartPhase2();
    }

    private void StartPhase2()
    {
        ResetSection();
        PutIntoSections();

        float xPosition = 0f;
        float yPosition = _arenaBounds.center.y + 2;

        Vector2 movePosition = new(xPosition,yPosition);
        Vector2 sectionRotation = Vector2.up;

        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(movePosition,3f);
            bossSection.OnFinishedMove += CanStartPhase2Attacks;
            bossSection.OnSectionDestroyed += StopLaser;

            bossSection.transform.up = sectionRotation;
            sectionRotation = Vector2.down;


            movePosition.y *= -1;
        }


        sectionRotation = Vector2.right;

        xPosition = _arenaBounds.center.x + 2;
        yPosition = 0f;

        movePosition = new(xPosition,yPosition);

        foreach(BossSection bossSection in _section2)
        {
            bossSection.MoveSection(movePosition,3f);
            bossSection.OnFinishedMove += CanStartPhase2Attacks;

            bossSection.transform.up = sectionRotation;
            sectionRotation = Vector2.left;

            movePosition.x *= -1;         
        }
    }
    private void StopLaser(BossSection bossSection)
    {
        bossSection.OnSectionDestroyed -= StopLaser;
        bossSection.StopLaser();
    }
    private void CanStartPhase2Attacks(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CanStartPhase2Attacks;
        _sectionsFinishedMoving[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsFinishedMoving)
        {
            if(!booleans) return;
        }

        //Reset Movement
        for(int i = 0; i < _sectionsFinishedMoving.Count ; i++)
        {
            _sectionsFinishedMoving[i] = false;
        }


        StartPhase2Attacks();
        StartCoroutine(RotateSections());
    }
    private IEnumerator RotateSections()
    {
        while(true)
        {
            _sectionsParent.transform.Rotate(0,0,_sectionsParentRotationSpeed*Time.deltaTime);
            _positiveSection.transform.Rotate(0,0,_bulletSectionRotationSpeed*Time.deltaTime);
            _negativeSection.transform.Rotate(0,0,-_bulletSectionRotationSpeed*Time.deltaTime);
            yield return null;
        }
    }
    private void StartPhase2Attacks()
    {

        foreach(BossSection bossSection in _section1)
        {
            bossSection.SetLaserSO(_phase2LaserSO);
            bossSection.ShootLaser();
        }
        _previousAttack = Random.Range(0,2);
        SendPhase2Attack();
    }
    private void SendPhase2Attack()
    {
        StartCoroutine(WaitBeforePhase2Attack());
    }
    private IEnumerator WaitBeforePhase2Attack()
    {
        yield return new WaitForSeconds(_attackDelay);
        if(_previousAttack == 0)
        {
            StartCoroutine(BulletSpreadAttack(1.5f));
        }
        else
        {
            StartCoroutine(FourBulletAttack(0.1f,2f));
        }
    }
    private IEnumerator FourBulletAttack(float shotWaitTime,float negativeWaitTime)
    {
        int bulletAmount = 6;
        for (int i = 0; i < 4; i++)
        {
            if (i % 2 == 0)
            {
                _positiveSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount);
            }
            else
            {
                _negativeSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount);
            }

            if (i < 3)
            {
                yield return new WaitForSeconds(negativeWaitTime);
            }
        }
        _previousAttack = 0;
        _onAttackDone?.Invoke();
    }
    private IEnumerator BulletSpreadAttack(float timeBetweenAttacks)
    {
        float degreeToRotate = 360f/7f;
        float phaseOffset = degreeToRotate/2f;
        for (int i = 0; i < 4; i++)
        {
            Vector2 originalShootDirection = Vector2.right;
            if (i % 2 == 0)
            {
                for(int j = 0 ; j < 7 ; j++)
                {
                    _positiveSection.ShootBullet(originalShootDirection);
                    originalShootDirection = Quaternion.Euler(0f,0f,degreeToRotate) * originalShootDirection;
                }
            }
            else
            {
                originalShootDirection = Quaternion.Euler(0f,0f,phaseOffset) * originalShootDirection;
                for(int j = 0 ; j < 7 ; j++)
                {
                    _negativeSection.ShootBullet(originalShootDirection);
                    originalShootDirection = Quaternion.Euler(0f,0f,degreeToRotate) * originalShootDirection;
                }
            }

            if (i < 3)
            {
                yield return new WaitForSeconds(timeBetweenAttacks);
            }    
        }  

        _previousAttack = 1;
        _onAttackDone?.Invoke(); 
    }
    private IEnumerator SevenBulletsAttack()
    {
        yield return null;
    }
    private void SendAttack()
    {
        StartCoroutine(WaitBeforeAttack());
    }
    private IEnumerator WaitBeforeAttack()
    {
        yield return new WaitForSeconds(_attackDelay);

        SendPhase1Attack();
    }

    private void SendPhase1Attack()
    {
        ResetSection();
        int randomAttack = Random.Range(0,4);
        while(randomAttack == _previousAttack)
        {
            randomAttack = Random.Range(0,4);
        }

        if(randomAttack == 0)
        {
            StartSinAttack();

        }
        else if(randomAttack == 1)
        {
            StartXLaserAttack();

        }
        else if(randomAttack == 2)
        {
            StartLineLaserAttack();
        }
        else if(randomAttack == 3)
        {
            StartSpinningBulletAttack();
        }
    }
    private void StartSpinningBulletAttack()
    {

        PutIntoSections();
        float yPosition = _arenaBounds.yMax / 2;
        float xPosition = _arenaBounds.xMin * 2/3;
        float rotationOffset = 30f;
        Vector2 sectionPosition = new(xPosition, yPosition);
        Vector2 sectionRotation = Quaternion.Euler(0,0,rotationOffset) * Vector2.down;

        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(sectionPosition,_timeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanSpinningBulletAttack;


            bossSection.transform.up = sectionRotation;
            sectionRotation = Quaternion.Euler(0,0,-rotationOffset) * Vector2.right;

            sectionPosition.y *= -1;
        }

        sectionRotation = Quaternion.Euler(0,0,-rotationOffset) * Vector2.left;

        sectionPosition.x *= -1;
        foreach(BossSection bossSection in _section2)
        {
            bossSection.MoveSection(sectionPosition,_timeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanSpinningBulletAttack;


            bossSection.transform.up = sectionRotation;
            sectionRotation = Quaternion.Euler(0,0,rotationOffset) * Vector2.up;


            sectionPosition.y *= -1;
        }
    }

    private void CheckIfCanSpinningBulletAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanSpinningBulletAttack;
        _sectionsFinishedMoving[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsFinishedMoving)
        {
            if(!booleans) return;
        }

        //Reset Movement
        for(int i = 0; i < _sectionsFinishedMoving.Count ; i++)
        {
            _sectionsFinishedMoving[i] = false;
        }

        StartCoroutine(SpinningBulletAttack());
    }

    private IEnumerator SpinningBulletAttack()
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.RotateAndShoot(0.5f,40f);
        }
        yield return new WaitForSeconds(_spinningBulletTime);
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.RotateAndShoot();
        }

        _previousAttack = 3;
        _onAttackDone?.Invoke();
    }

    private void StartLineLaserAttack()
    {
        float yOffset = 2 *_arenaBounds.yMax / 5;
        float xPosition = _arenaBounds.xMax * 96/100;
        Vector2 movePosition = new(xPosition, _arenaBounds.yMax - yOffset);

        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSection(movePosition,_timeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanLineLaserAttack;
            bossSection.transform.up = Vector2.left;
            movePosition.y -= yOffset;
        }        
    }
    private void CheckIfCanLineLaserAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanLineLaserAttack;
        _sectionsFinishedMoving[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsFinishedMoving)
        {
            if(!booleans) return;
        }

        //Reset Movement
        for(int i = 0; i < _sectionsFinishedMoving.Count ; i++)
        {
            _sectionsFinishedMoving[i] = false;
        }

        LineLaserAttack();
    }

    private void LineLaserAttack()
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.transform.up = (FindLaserPosition(bossSection.transform.position) 
            - (Vector2)bossSection.transform.position).normalized;
            bossSection.OnLaserFinished += FinishedLineLaserAttack;
            bossSection.ShootLaser();
        }

        
    }

    private void FinishedLineLaserAttack(BossSection bossSection)
    {
        bossSection.OnLaserFinished -= FinishedLineLaserAttack;

        _sectionsFinishedLaser[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsFinishedLaser)
        {
            if(!booleans) return;
        }

        //Reset Movement
        for(int i = 0; i < _sectionsFinishedLaser.Count ; i++)
        {
            _sectionsFinishedLaser[i] = false;
        }

        _previousAttack = 2;
        _onAttackDone?.Invoke();
    }

    private void StartXLaserAttack()
    {
        PutIntoSections();
        float xPosition = _arenaBounds.xMin * 96/100;
        float yPosition = _arenaBounds.yMax * 96/100;
        Vector2 sectionPosition = new(xPosition,yPosition);
        
        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(sectionPosition,_timeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanXLaserAttack;
            bossSection.transform.up = Vector2.right;
            sectionPosition.y *= -1;
        }

        sectionPosition.x *= -1;
        sectionPosition.y = yPosition / 4f;
        foreach(BossSection bossSection in _section2)
        {
            bossSection.MoveSection(sectionPosition,_timeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanXLaserAttack;
            bossSection.transform.up = Vector2.left;
            sectionPosition.y *= -1;
        }
    }
    private void CheckIfCanXLaserAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanXLaserAttack;
        _sectionsFinishedMoving[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsFinishedMoving)
        {
            if(!booleans) return;
        }

        //Reset Movement
        for(int i = 0; i < _sectionsFinishedMoving.Count ; i++)
        {
            _sectionsFinishedMoving[i] = false;
        }

        XLaserAttack();
    }
    private void XLaserAttack()
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.transform.up = (FindLaserPosition(bossSection.transform.position) 
            - (Vector2)bossSection.transform.position).normalized;
            bossSection.OnLaserFinished += FinishedXLaserAttack;
            bossSection.ShootLaser();
        }

        
    }
    private void FinishedXLaserAttack(BossSection bossSection)
    {
        bossSection.OnLaserFinished -= FinishedXLaserAttack;

        _sectionsFinishedLaser[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsFinishedLaser)
        {
            if(!booleans) return;
        }

        //Reset Movement
        for(int i = 0; i < _sectionsFinishedLaser.Count ; i++)
        {
            _sectionsFinishedLaser[i] = false;
        }

        _previousAttack = 1;
        _onAttackDone?.Invoke();
    }
    private void StartSinAttack()
    {
        float yOffset = 2 *_arenaBounds.yMax / 5;
        float xPosition = _arenaBounds.xMax * 96/100;
        Vector2 movePosition = new(xPosition, _arenaBounds.yMax - yOffset);

        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSection(movePosition,_timeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanSinAttack;
            bossSection.transform.up = Vector2.left;
            movePosition.y -= yOffset;
        }
    }
    private void CheckIfCanSinAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanSinAttack;
        _sectionsFinishedMoving[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsFinishedMoving)
        {
            if(!booleans) return;
        }

        //Reset Movement
        for(int i = 0; i < _sectionsFinishedMoving.Count ; i++)
        {
            _sectionsFinishedMoving[i] = false;
        }

        StartCoroutine(SinWaveAttack());
    }
    private IEnumerator SinWaveAttack()
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSinWave(_sinShotSpeed);
        }
        yield return new WaitForSeconds(_sinWaveAttackTime);
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSinWave(_sinShotSpeed);
        }

        _previousAttack = 0;
        _onAttackDone?.Invoke();
    }



    private void PutIntoSections()
    {
        List<BossSection> randomList = new(_sectionList);

        //Section 1
        _section1.Add(randomList[Random.Range(0,randomList.Count)]);
        randomList.Remove(_section1[0]);
        _section1.Add(randomList[Random.Range(0,randomList.Count)]);
        randomList.Remove(_section1[1]);

        //Section 2
        _section2.Add(randomList[Random.Range(0,randomList.Count)]);
        randomList.Remove(_section2[0]);
        _section2.Add(randomList[0]);
        randomList.Remove(_section2[1]);       
    }
    private void ResetSection()
    {
        _section1.Clear();
        _section2.Clear();
    }
    private Vector2 FindLaserPosition(Vector2 startPosition)
    {
        
        float randomX = Random.Range(_leftArenaBounds.xMin,0);
        float randomY = Random.Range(_leftArenaBounds.yMin,_leftArenaBounds.yMax);
        Vector2 resultPosition = new(randomX,randomY);

        if(startPosition.x < 0)
        {
            resultPosition.x *= -1;
        }

        return resultPosition;
    }
}
