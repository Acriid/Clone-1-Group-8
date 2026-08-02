using UnityEngine;

[CreateAssetMenu(fileName = "BossSettingsLVL9", menuName = "Boss/BossSettingsLVL9")]
public class BossSettingsLVL9SO : ScriptableObject
{
    //3.25 bullets per second
    [Header("SpinningLaserAttack")]
    public float SpinningLaserRotationSpeed = 4f;
    public float SpinningLaserBackAndForthSpeed = 10f;
    public float SpinningLaserBulletShotSpeed = 1f/3f;
    [Header("SinWaveAttack")]
    public float SinWaveAttackTime = 8f;
    public float SinWaveShotInterval = 0.1f;
    [Header("StaticLaserAttack")]
    public float StaticLaserAttackSectionSpeed = 30f;
    public float StaticLaserAttackShotSpeed = 0.5f;
    public float StaticLaserAttackBulletSpeed = 20f;
    [Header("BulletSpreadAttack")]
    public float BulletSpreadAttackTime = 0.5f;
    public float BulletSpreadRotationDegrees = 360f/19f;
    public int BulletSpreadBulletAmount = 19;
    public int BulletSpreadBulletCircleAmount = 4;
    [Header("FourBullet Attack")]
    public float FourBulletAttackWait = 2f;
    public float FourBulletAttackTime = 0.1f;
    public float FourBulletOffset = 0f;
    public int FourBulletBulletAmount = 6;
    public int FourBulletBulletRingAmount = 4;
}
