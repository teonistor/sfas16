using UnityEngine;
//using System;
using System.Collections;

public class DifficultyCurveNew : MonoBehaviour 
{
	[SerializeField] private float GameStartSpeed;
	[SerializeField] private float PlayerStartSpeed;
	[SerializeField] private float BulletStartSpeed;
    [SerializeField] private float LevelStartDuration;
    [SerializeField] private float EnemyStartDensity;
    [SerializeField] private int BossStartStrength;
    
    [SerializeField] private float SpeedGain;
    [SerializeField] private float EnemySafeGap;
    [SerializeField] private float EnemyCheckGap;
    [SerializeField] private float FreezeTime;
    [SerializeField] private Camera GameplayCamera;

    private float [] mTimeToDanger;
    private float NextGameSpeed;
    private float EnemyDensity;
    private float DefrostTime;

    private Color LevelColor;
    private Color BossColor;

    public static float EnemySpeed { get; private set; }
    public static float ScenerySpeed { get; private set; }
    public static float PlayerSpeed { get; private set; }
	public static float BulletSpeed { get; private set; }
    public static float LevelDuration { get; private set; }
    public static int BossStrength { get; private set; }
    public static bool StateSwitchFlag { get; private set; }

    void Awake()
	{
        LevelColor = new Color(0.262f, 0.236f, 0.574f);
        BossColor = Color.black;

        Reset();

        mTimeToDanger = new float [] { EnemyCheckGap, EnemyCheckGap, EnemyCheckGap };
    }

    public int SpawnPattern()
    {
        int pattern = 0;

        for (int ColumnCount = 0; ColumnCount < 3; ColumnCount++)
        {
            mTimeToDanger[ColumnCount] -= GameLogic.GameDeltaTime;
            if (mTimeToDanger[ColumnCount] < 0f)
            {
                if (Random.Range(0f, 100f) < EnemyDensity)
                {
                    pattern |= 1 << ColumnCount;
                    mTimeToDanger[ColumnCount] = EnemySafeGap;
                }
                else
                {
                    mTimeToDanger[ColumnCount] = EnemyCheckGap;
                }
            }
        }
        return pattern;
    }

    public bool SlowDown()
    {
        GameplayCamera.backgroundColor = Color.Lerp(GameplayCamera.backgroundColor, BossColor, GameLogic.GameDeltaTime * 2f);
        if (NextGameSpeed == 0f) {
            NextGameSpeed = EnemySpeed * SpeedGain;
        }
        EnemySpeed = ScenerySpeed = Mathf.Lerp(EnemySpeed, 0f, GameLogic.GameDeltaTime * 1.5f);
        if (EnemySpeed < 0.1f) {
            EnemySpeed = ScenerySpeed = 0f;
            return true;
        }
        return false;
    }

    public bool LevelUp()
    {
        EnemySpeed = Mathf.Lerp(EnemySpeed, NextGameSpeed, GameLogic.GameDeltaTime * 1.5f);
        ScenerySpeed = EnemySpeed;
        GameplayCamera.backgroundColor = Color.Lerp(GameplayCamera.backgroundColor, LevelColor, GameLogic.GameDeltaTime * 2f);
        if (Mathf.Abs (EnemySpeed-NextGameSpeed) < 0.1f) {
            EnemySpeed = ScenerySpeed = NextGameSpeed;
            NextGameSpeed = 0f;
            PlayerSpeed *= SpeedGain;
            BulletSpeed *= SpeedGain;
            LevelDuration *= SpeedGain;
            EnemyDensity *= SpeedGain;
            BossStrength *= (int)SpeedGain;
            return true;
        }
        return false;
    }

    public void Freeze()
    {
        EnemySpeed = 0f;
        //Some recycling
        DefrostTime = -FreezeTime;
    }

    public bool Defrost()
    {
        DefrostTime += GameLogic.GameDeltaTime;
        if (DefrostTime>0)
            EnemySpeed = Mathf.Lerp(0f, ScenerySpeed, DefrostTime);
        if (EnemySpeed >= ScenerySpeed)
            return true;
        return false;
    }

	public void GameOver()
	{
		EnemySpeed = 0.0f;
        ScenerySpeed = 0.0f;
        PlayerSpeed = 0.0f;
		BulletSpeed = 0.0f;
	}

	public void Reset()
	{
		EnemySpeed = GameStartSpeed;
        ScenerySpeed = GameStartSpeed;
        PlayerSpeed = PlayerStartSpeed;
		BulletSpeed = BulletStartSpeed;
        LevelDuration = LevelStartDuration;
        EnemyDensity = EnemyStartDensity;
        BossStrength = BossStartStrength;
        GameplayCamera.backgroundColor = LevelColor;
    }
}
