using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossBrain : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private BoundsInt _arenaBounds;
    [SerializeField] private LaserSO _phase2LaserSOLVL2;
    [SerializeField] private LaserSO _phase2LaserSOLVL9;
    [SerializeField] private GameObject _sectionsParent;
    [SerializeField] private BossSection _negativeSection;
    [SerializeField] private BossSection _positiveSection;
    [SerializeField] private BossSettingsSO _bossSettingsSO;
    [SerializeField] private BossSettingsLVL9SO _lvl9BossSettingsSO;
    private BoundsInt _leftArenaBounds;
    [SerializeField] private List<BossSection> _sectionList = new(4);

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
    private int _bossLevel = 0;
    private event Action _onAttackDone;



    private BossSection _laserSection = null;
    private List<BossSection> _bulletSections = new(3)
    {
        null,
        null,
        null
    };
    void Start()
    {


        int splitX = _arenaBounds.xMin + _arenaBounds.size.x / 2;

       _leftArenaBounds = new BoundsInt(
            new Vector3Int(_arenaBounds.xMin, _arenaBounds.yMin, _arenaBounds.zMin),
            new Vector3Int(splitX - _arenaBounds.xMin, _arenaBounds.size.y, _arenaBounds.size.z));

        _bossLevel = _bossSettingsSO.BossLevel;

        _onAttackDone += SendAttack;
        _onAttackDone?.Invoke();

        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.OnSectionDestroyed += OnSectionBroken;
            bossSection.SetHealth(_bossSettingsSO.SectionHealthPhase1);

            if(_bossSettingsSO.BossLevel == 2)
            {
                if(_phase2LaserSOLVL9 != null)
                bossSection.SetLaserSO(_phase2LaserSOLVL9);
            }
        }


        //TestPhase2();
        // StartCoroutine(GoPhase2AfterTime());
    }

    void OnDisable()
    {
        _onAttackDone -= SendAttack;
        _onAttackDone -= SendPhase2Attack;

        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.OnSectionDestroyed -= OnSectionBroken;
        }
    }
    #region Phase2 Testing
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
    #endregion
    private void OnSectionBroken(BossSection bossSection)
    {
        bossSection.OnSectionDestroyed -= OnSectionBroken;
        _sectionsBroken[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsBroken)
        {
            if(!booleans) return;
        }

        foreach(BossSection bossSection1 in _sectionList)
        {
            bossSection1.StopLaser();
            bossSection1.StopAllRoutines();
            bossSection1.IndicateDestroyed();
        }
        _onAttackDone -= SendAttack;
        _onAttackDone += SendPhase2Attack;
        StartPhase2();
        StopAllCoroutines();
    }

    private void StartPhase2()
    {
        ResetSection();
        PutIntoSections();

        float xPosition = 0f;
        float yPosition = _arenaBounds.center.y + _bossSettingsSO.Phase2Offset;

        Vector2 movePosition = new(xPosition,yPosition);
        Vector2 sectionRotation = Vector2.up;

        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToPhase2);
            bossSection.OnFinishedMove += CanStartPhase2Attacks;
            bossSection.OnSectionDestroyed += StopLaser;

            bossSection.transform.up = sectionRotation;
            sectionRotation = Vector2.down;


            movePosition.y *= -1;
        }


        sectionRotation = Vector2.right;

        xPosition = _arenaBounds.center.x + _bossSettingsSO.Phase2Offset;
        yPosition = 0f;

        movePosition = new(xPosition,yPosition);

        foreach(BossSection bossSection in _section2)
        {
            bossSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToPhase2);
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

    //Shared bookkeeping used by every "wait for all sections to finish moving" handler.
    //Marks bossSection as finished, returns false until every section is finished,
    //then resets the tracking list and returns true.
    private bool AllSectionsFinishedMoving(BossSection bossSection)
    {
        _sectionsFinishedMoving[_sectionList.IndexOf(bossSection)] = true;

        foreach(bool booleans in _sectionsFinishedMoving)
        {
            if(!booleans) return false;
        }

        for(int i = 0; i < _sectionsFinishedMoving.Count ; i++)
        {
            _sectionsFinishedMoving[i] = false;
        }

        return true;
    }

    //Shared bookkeeping used by every "wait for all sections to finish their laser" handler.
    //Marks bossSection as finished, returns false until every section is finished,
    //then resets the tracking list and returns true.
    private bool AllSectionsFinishedLaser(BossSection bossSection)
    {
        _sectionsFinishedLaser[_sectionList.IndexOf(bossSection)] = true;

        foreach(bool booleans in _sectionsFinishedLaser)
        {
            if(!booleans) return false;
        }

        for(int i = 0; i < _sectionsFinishedLaser.Count ; i++)
        {
            _sectionsFinishedLaser[i] = false;
        }

        return true;
    }

    private void CanStartPhase2Attacks(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CanStartPhase2Attacks;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        foreach(BossSection bossSection1 in _sectionList)
        {
            bossSection1.StartPhase2(_bossSettingsSO.SectionHealthPhase2);
        }

        StartPhase2Attacks();
        StartCoroutine(RotateSections());
    }
    private IEnumerator RotateSections()
    {
        while(true)
        {
            _sectionsParent.transform.Rotate(0,0,_bossSettingsSO.SectionsParentRotationSpeed*Time.deltaTime);
            _positiveSection.transform.Rotate(0,0,_bossSettingsSO.BulletSectionRotationSpeed*Time.deltaTime);
            _negativeSection.transform.Rotate(0,0,-_bossSettingsSO.BulletSectionRotationSpeed*Time.deltaTime);
            yield return null;
        }
    }
    private void StartPhase2Attacks()
    {

        foreach(BossSection bossSection in _section1)
        {
            bossSection.SetLaserSO(_phase2LaserSOLVL2);
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
        yield return new WaitForSeconds(_bossSettingsSO.AttackDelay);
        if(_previousAttack == 0)
        {
            StartCoroutine(BulletSpreadAttack(_bossSettingsSO.BulletSpreadAttackTime));
        }
        else
        {
            StartCoroutine(FourBulletAttack(_bossSettingsSO.FourBulletAttackTime,_bossSettingsSO.FourBulletAttackWait));
        }
    }
    #region  SendAttack
    private void SendAttack()
    {
        StartCoroutine(WaitBeforeAttack());
    }
    private IEnumerator WaitBeforeAttack()
    {
        yield return new WaitForSeconds(_bossSettingsSO.AttackDelay);

        SendPhase1Attack();
    }

    private void SendPhase1Attack()
    {
        ResetSection();
        //Level 1 attacks
        if(_bossLevel == 1)
        {
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
        else
        {
            int randomAttack = Random.Range(0,4);
            while(randomAttack == _previousAttack)
            {
                randomAttack = Random.Range(0,4);
            }


            if(randomAttack == 0)
            {
                RotatingLaserAttack();
            }
            else if(randomAttack == 1)
            {
                StartSin9WaveAttack();
            }
            else if(randomAttack == 2)
            {
                StartClosingLaserAttack();
            }
            else if (randomAttack == 3)
            {
                StartLaserLineBulletAttack();
            }
        }
    }
    #endregion
    #region Level 2 Attacks
    private IEnumerator FourBulletAttack(float shotWaitTime,float negativeWaitTime)
    {
        int bulletAmount = _bossSettingsSO.FourBulletBulletAmount;
        for (int i = 0; i < _bossSettingsSO.FourBulletBulletRingAmount; i++)
        {
            if (i % 2 == 0)
            {
                _positiveSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount,
                BulletDirection.Up,_bossSettingsSO.FourBulletOffset);
                _positiveSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount,
                BulletDirection.Down,_bossSettingsSO.FourBulletOffset);
                _positiveSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount,
                BulletDirection.Left,_bossSettingsSO.FourBulletOffset);
                _positiveSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount,
                BulletDirection.Right,_bossSettingsSO.FourBulletOffset);
            }
            else
            {
                _negativeSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount,
                BulletDirection.Up,_bossSettingsSO.FourBulletOffset);
                _negativeSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount,
                BulletDirection.Down,_bossSettingsSO.FourBulletOffset);
                _negativeSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount,
                BulletDirection.Left,_bossSettingsSO.FourBulletOffset);
                _negativeSection.Phase2ShootFourDirections(shotWaitTime, bulletAmount,
                BulletDirection.Right,_bossSettingsSO.FourBulletOffset);
            }

            if (i < _bossSettingsSO.FourBulletBulletRingAmount - 1)
            {
                yield return new WaitForSeconds(negativeWaitTime);
            }
        }
        _previousAttack = 0;
        _onAttackDone?.Invoke();
    }
    private IEnumerator BulletSpreadAttack(float timeBetweenAttacks)
    {
        float phaseOffset = _bossSettingsSO.BulletSpreadRotationDegrees/2f;
        for (int i = 0; i < _bossSettingsSO.BulletSpreadBulletCircleAmount; i++)
        {
            Vector2 originalShootDirection = Vector2.right;
            if (i % 2 == 0)
            {
                for(int j = 0 ; j < _bossSettingsSO.BulletSpreadBulletAmount ; j++)
                {
                    _positiveSection.ShootBullet(originalShootDirection);
                    originalShootDirection = Quaternion.Euler(0f,0f,_bossSettingsSO.BulletSpreadRotationDegrees) 
                    * originalShootDirection;
                }
            }
            else
            {
                originalShootDirection = Quaternion.Euler(0f,0f,phaseOffset) * originalShootDirection;
                for(int j = 0 ; j < _bossSettingsSO.BulletSpreadBulletAmount ; j++)
                {
                    _negativeSection.ShootBullet(originalShootDirection);
                    originalShootDirection = Quaternion.Euler(0f,0f,_bossSettingsSO.BulletSpreadRotationDegrees) 
                    * originalShootDirection;
                }
            }

            if (i < _bossSettingsSO.BulletSpreadBulletCircleAmount - 1)
            {
                yield return new WaitForSeconds(timeBetweenAttacks);
            }    
        }  

        _previousAttack = 1;
        _onAttackDone?.Invoke(); 
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
            bossSection.MoveSection(sectionPosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanSpinningBulletAttack;


            bossSection.transform.up = sectionRotation;
            sectionRotation = Quaternion.Euler(0,0,-rotationOffset) * Vector2.right;

            sectionPosition.y *= -1;
        }

        sectionRotation = Quaternion.Euler(0,0,-rotationOffset) * Vector2.left;

        sectionPosition.x *= -1;
        foreach(BossSection bossSection in _section2)
        {
            bossSection.MoveSection(sectionPosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanSpinningBulletAttack;


            bossSection.transform.up = sectionRotation;
            sectionRotation = Quaternion.Euler(0,0,rotationOffset) * Vector2.up;


            sectionPosition.y *= -1;
        }
    }

    private void CheckIfCanSpinningBulletAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanSpinningBulletAttack;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        StartCoroutine(SpinningBulletAttack());
    }

    private IEnumerator SpinningBulletAttack()
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.RotateAndShoot(_bossSettingsSO.SpinningBulletShootTime,
            _bossSettingsSO.SpinningBulletRotateSpeed);
        }
        yield return new WaitForSeconds(_bossSettingsSO.SpinningBulletTime);
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.RotateAndShoot();
        }

        _previousAttack = 3;
        _onAttackDone?.Invoke();
    }

    //Shared by StartLineLaserAttack and StartSinAttack - both moved sections to the
    //same starting spots, differing only in which handler they subscribed to OnFinishedMove.
    private void MoveSectionsToLaserLineStartPositions(Action<BossSection> onFinishedMove)
    {
        float firstYPosition = _arenaBounds.yMax - _bossSettingsSO.SinMovementDistance - 1f;
        float yOffset = 2f * firstYPosition/3f;
        float xPosition = _arenaBounds.xMax * _bossSettingsSO.ArenaPaddingPercentageX;
        Vector2 movePosition = new(xPosition, firstYPosition);

        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += onFinishedMove;
            bossSection.transform.up = Vector2.left;
            movePosition.y -= yOffset;
        }
    }
    private void MoveSectionsToLaserLineStartPositions(Action<BossSection> onFinishedMove,List<BossSection> orderedMoveList)
    {
        float firstYPosition = _arenaBounds.yMax - _bossSettingsSO.SinMovementDistance - 1f;
        float yOffset = 2f * firstYPosition/3f;
        float xPosition = _arenaBounds.xMax * _bossSettingsSO.ArenaPaddingPercentageX;
        Vector2 movePosition = new(xPosition, firstYPosition);

        foreach(BossSection bossSection in orderedMoveList)
        {
            bossSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += onFinishedMove;
            bossSection.transform.up = Vector2.left;
            movePosition.y -= yOffset;
        }
    }
    private void StartLineLaserAttack()
    {
        MoveSectionsToLaserLineStartPositions(CheckIfCanLineLaserAttack);
    }
    private void CheckIfCanLineLaserAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanLineLaserAttack;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        LineLaserAttack();
    }

    private void LineLaserAttack()
    {
        AimAndShootLaser(FinishedLineLaserAttack);
    }

    private void FinishedLineLaserAttack(BossSection bossSection)
    {
        bossSection.OnLaserFinished -= FinishedLineLaserAttack;
        if(!AllSectionsFinishedLaser(bossSection)) return;

        _previousAttack = 2;
        _onAttackDone?.Invoke();
    }

    private void StartXLaserAttack()
    {
        PutIntoSections();
        float xPosition = _arenaBounds.xMin * _bossSettingsSO.ArenaPaddingPercentageX;
        float yPosition = _arenaBounds.yMax * _bossSettingsSO.ArenaPaddingPercentageY;
        Vector2 sectionPosition = new(xPosition,yPosition);
        
        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(sectionPosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanXLaserAttack;
            bossSection.transform.up = Vector2.right;
            sectionPosition.y *= -1;
        }

        sectionPosition.x *= -1;
        sectionPosition.y = yPosition / 4f;
        foreach(BossSection bossSection in _section2)
        {
            bossSection.MoveSection(sectionPosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanXLaserAttack;
            bossSection.transform.up = Vector2.left;
            sectionPosition.y *= -1;
        }
    }
    private void CheckIfCanXLaserAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanXLaserAttack;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        XLaserAttack();
    }
    //Shared by XLaserAttack and LineLaserAttack - both aim at FindLaserPosition and shoot,
    //differing only in which handler they subscribed to OnLaserFinished.
    private void AimAndShootLaser(Action<BossSection> onLaserFinished)
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.transform.up = (FindLaserPosition(bossSection.transform.position) 
            - (Vector2)bossSection.transform.position).normalized;
            bossSection.OnLaserFinished += onLaserFinished;
            bossSection.ShootLaser();
        }
    }
    private void XLaserAttack()
    {
        AimAndShootLaser(FinishedXLaserAttack);
    }
    private void FinishedXLaserAttack(BossSection bossSection)
    {
        bossSection.OnLaserFinished -= FinishedXLaserAttack;
        if(!AllSectionsFinishedLaser(bossSection)) return;

        _previousAttack = 1;
        _onAttackDone?.Invoke();
    }
    private void StartSinAttack()
    {
        MoveSectionsToLaserLineStartPositions(CheckIfCanSinAttack);
    }
    private void CheckIfCanSinAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanSinAttack;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        StartCoroutine(SinWaveAttack());
    }
    private IEnumerator SinWaveAttack()
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSinWave(_bossSettingsSO.SinShotSpeed,_bossSettingsSO.SinWaveBulletSpeed,
            _bossSettingsSO.SinSectionSpeed,_bossSettingsSO.SinMovementDistance);
        }
        yield return new WaitForSeconds(_bossSettingsSO.SinWaveAttackTime);
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSinWave(_bossSettingsSO.SinShotSpeed,_bossSettingsSO.SinWaveBulletSpeed,
            _bossSettingsSO.SinSectionSpeed,_bossSettingsSO.SinMovementDistance);
        }

        _previousAttack = 0;
        _onAttackDone?.Invoke();
    }
    #endregion
    #region Level 9 Attacks
    #region  RotatingLaser
    private void RotatingLaserAttack()
    {
        float xPosition = _leftArenaBounds.xMin / 3;
        float rotationOffset = 30f;

        Vector2 movePosition = new(xPosition,0f);

        _bulletSections = new(_sectionList);
        _laserSection = _bulletSections[Random.Range(0,_bulletSections.Count)];
        _bulletSections.Remove(_laserSection);

        _laserSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToAttackPosition);
        _laserSection.transform.up = Quaternion.Euler(0f,0f,rotationOffset) * new Vector3(0f,1f,0f);

        _laserSection.OnFinishedMove += CheckIfCanRotatingLaserAttack;

        xPosition = _arenaBounds.xMax * _bossSettingsSO.ArenaPaddingPercentageX;
        float yOffset = _arenaBounds.yMax /2f;
        movePosition = new(xPosition,yOffset);

        foreach(BossSection bossSection in _bulletSections)
        {
            bossSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanRotatingLaserAttack;
            bossSection.transform.up = Vector2.left;
            movePosition.y -= yOffset;
        }
    }
    private void CheckIfCanRotatingLaserAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanRotatingLaserAttack;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        StartCoroutine(StartRotatingLaserAttack());
    }
    //Rotates an exact 360 for the laser
    private IEnumerator StartRotatingLaserAttack()
    {
        
        RotateSection(_laserSection,360f,8f,0f,true,false);
        LaserSO originalLaser = _laserSection.GetLaserSO();
        _laserSection.ShootLaser();


        //Bullets rotate between 60 and 120
        foreach(BossSection bossSection in _bulletSections)
        {
            bossSection.RotateBetweenAnglesAndShoot(_lvl9BossSettingsSO.SpinningLaserBulletShotSpeed,2f,60f,120f,true,true);
        }

        yield return new WaitForSeconds(8f);
       
        foreach(BossSection bossSection in _bulletSections)
        {
            bossSection.RotateAndShoot();
        }


        _laserSection.StopLaser();

        _previousAttack = 0;
        _onAttackDone?.Invoke();

    }
    #endregion
    #region SinWaveLVL9
    private void StartSin9WaveAttack()
    {
        MoveSectionsToLaserLineStartPositions(CheckIfCanSin9Attack);
    }
    private void CheckIfCanSin9Attack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanSin9Attack;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        StartCoroutine(Sin9WaveAttack());
    }
    private IEnumerator Sin9WaveAttack()
    {
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.RotateBetweenAnglesAndShoot(_lvl9BossSettingsSO.SinWaveShotInterval,2f,80,100,true,true);
        }

        yield return new WaitForSeconds(_lvl9BossSettingsSO.SinWaveAttackTime);

        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.RotateAndShoot();
        }

        _previousAttack = 1;
        _onAttackDone?.Invoke();
    }
    #endregion
    #region ClosingLaser Attack
    private void StartClosingLaserAttack()
    {
        ResetSection();
        PutIntoSections();

        float yPosition = _arenaBounds.yMax * 96f/100f;
        float xPosition = _arenaBounds.xMax * 96f/100f;
        Vector2 movePosition = new(xPosition,yPosition);

        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanClosingLaserAttack;
            movePosition.x *= -1;

            bossSection.transform.up = Vector2.down;
        }

        yPosition = 2f;
        xPosition = _arenaBounds.xMax * 92f/100f;

        movePosition = new(xPosition,yPosition);

        foreach(BossSection bossSection in _section2)
        {
            bossSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanClosingLaserAttack;    
            movePosition.y *= -1;     

            bossSection.transform.up = Vector2.left;
        }

    }
    private void CheckIfCanClosingLaserAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanClosingLaserAttack;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        StartCoroutine(ClosingLaserAttack());
    }
    //Lasts for 10 seconds
    private IEnumerator ClosingLaserAttack()
    {
        float yPosition = _arenaBounds.yMax * 96f/100f;
        float xPosition = 2f;
        Vector2 movePosition = new(xPosition,yPosition);

        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(movePosition,10f);
            bossSection.ShootLaser();
            movePosition.x *= -1;
        }

        bool startGoingBottom = true;
        foreach(BossSection bossSection in _section2)
        {
            bossSection.RotateBetweenAnglesAndShoot(0.1f,2f,45f,45f + 90f,startGoingBottom,true);
            startGoingBottom = false;
        }
        yield return new WaitForSeconds(10f);

        foreach(BossSection bossSection in _section1)
        {
            bossSection.StopLaser();
        }  
        foreach(BossSection bossSection in _section2)
        {
            bossSection.RotateAndShoot();
        }   

        _previousAttack = 2;
        _onAttackDone?.Invoke();  
    }
    #endregion
    #region LaserLineBullet Attack
    private void StartLaserLineBulletAttack()
    {
        _laserSection = _sectionList[Random.Range(0,_sectionList.Count)];

        List<BossSection> _moveList = new(_sectionList);
        _moveList.Remove(_laserSection);
        _moveList.Add(_laserSection);

        MoveSectionsToLaserLineStartPositions(CheckIfCanStartLaserLineBulletAttack,_moveList);
    }
    private void CheckIfCanStartLaserLineBulletAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanStartLaserLineBulletAttack;
        if(!AllSectionsFinishedMoving(bossSection)) return;

        StartCoroutine(LaserLineBulletAttack());
    }
    private IEnumerator LaserLineBulletAttack()
    {
        _laserSection.ShootLaser();
        _laserSection.transform.up = Quaternion.Euler(0f,0f,-60f) * new Vector2(0f,1f);
        RotateSection(_laserSection,145f,5f,0f,false,false);


        foreach(BossSection bossSection in _sectionList)
        {
            if(bossSection == _laserSection) continue;

            bossSection.RotateBetweenAnglesAndShoot(0.15f,2f,45f,45f + 90f,true,true);
        }

        yield return new WaitForSeconds(4f);

        foreach(BossSection bossSection in _sectionList)
        {
            if(bossSection == _laserSection) continue;

            bossSection.RotateAndShoot();
        } 

        yield return new WaitForSeconds(1f);
        _laserSection.StopLaser();

        _previousAttack = 3;
        _onAttackDone?.Invoke();
    }
    #endregion
    #region StaticLaser Attack
    private void StartStaticLaserAttack()
    {
        ResetSection();
        PutIntoSections();

        float xPosition = _arenaBounds.xMax * 94/100;
        float yPosition = 2f;
        Vector2 rotationPosition = Quaternion.Euler(0f,0f,45f) * Vector2.right;

        Vector2 movePosition = new(xPosition,yPosition);

        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(movePosition,_bossSettingsSO.TimeToMoveToAttackPosition);
            bossSection.OnFinishedMove += CheckIfCanStaticLaserAttack;
            bossSection.transform.up = rotationPosition;
            rotationPosition.y *= -1;
            movePosition.y *= -1;
        }

        movePosition.y = _arenaBounds.yMax * 94/100;
        movePosition.x *= -1;
        
    }
    private void CheckIfCanStaticLaserAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanStaticLaserAttack;
        if(!AllSectionsFinishedMoving(bossSection)) return;


    }
    private IEnumerator StaticLaserAttack()
    {

        yield return null;
    }
    #endregion
    #endregion
    private void RotateSection(BossSection bossSection, float rotateAmount,float timeToRotate,
    float shootInterval,bool clockwise = false, bool shoot = true)
    {
        bossSection.RotateAndShoot(shootInterval,timeToRotate,rotateAmount,clockwise,shoot);
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

    public float GetMaxHealth()
    {
        return _bossSettingsSO.BossHealth;
    }
    public List<BossSection> GetBossSections()
    {
        return _sectionList;
    }
}