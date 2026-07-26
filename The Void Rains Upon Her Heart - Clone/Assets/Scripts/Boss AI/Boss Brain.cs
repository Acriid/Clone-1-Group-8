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
    private BoundsInt _leftArenaBounds;
    [SerializeField] private List<BossSection> _sectionList = new(4);
    [Header("General Attacks")]
    [SerializeField] private float _attackDelay = 2f;
    [Header("SinWave Attack")]
    [SerializeField] private float _sinWaveX = 10f;
    [SerializeField] private float _sinWaveYOffset = 4f;
    [SerializeField] private float _sinWaveAttackTime = 6.5f;
    [Header("XLaser Attack")]



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

    //SinWave = 0
    //XLaser = 1
    //LineLaser = 2
    //SpinningBullet = 3



    //Laser attacks will always aim at the other side of the screen past the middle.
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
    }

    void OnDisable()
    {
        _onAttackDone -= SendAttack;
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
        int randomAttack = Random.Range(0,2);
        while(randomAttack == _previousAttack)
        {
            randomAttack = Random.Range(0,2);
        }

        if(randomAttack == 0)
        {
            StartSinAttack();
        }
        else if(randomAttack == 1)
        {
            StartXLaserAttack();
        }
    }

    public void StartXLaserAttack()
    {
        PutIntoSections();
        float xPosition = _arenaBounds.xMin * 95/100;
        float yPosition = _arenaBounds.yMax * 95/100;
        Vector2 sectionPosition = new(xPosition,yPosition);
        
        foreach(BossSection bossSection in _section1)
        {
            bossSection.MoveSection(sectionPosition,2f);
            bossSection.OnFinishedMove += CheckIfCanXLaserAttack;
            sectionPosition.y *= -1;
        }

        sectionPosition.x *= -1;
        sectionPosition.y = yPosition / 4f;
        foreach(BossSection bossSection in _section2)
        {
            bossSection.MoveSection(sectionPosition,2f);
            bossSection.OnFinishedMove += CheckIfCanXLaserAttack;
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
    public void StartSinAttack()
    {
        float yOffset = _sinWaveYOffset;
        Vector2 movePosition = new(_arenaBounds.xMax - 5, 2f* yOffset);


        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSection(movePosition,1f);
            bossSection.OnFinishedMove += CheckIfCanSinAttack;
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

        StartCoroutine(SinWaveAttack(_attackDelay));
    }
    private IEnumerator SinWaveAttack(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSinWave();
        }
        yield return new WaitForSeconds(_sinWaveAttackTime);
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSinWave();
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
