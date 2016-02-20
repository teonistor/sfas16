using UnityEngine;
//using System;
using System.Collections;

public class DifficultyCurve : MonoBehaviour 
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
    [SerializeField] private Camera GameplayCamera;

    private float [] mTimeToDanger;
    private float NextGameSpeed;
    private float EnemyDensity;

    private Color LevelColor;
    private Color BossColor;

    public static float GameSpeed { get; private set; }
	public static float PlayerSpeed { get; private set; }
	public static float BulletSpeed { get; private set; }
    public static float LevelDuration { get; private set; }
    public static int BossStrength { get; private set; }

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
            NextGameSpeed = GameSpeed * SpeedGain;
        }
        GameSpeed = Mathf.Lerp(GameSpeed, 0f, GameLogic.GameDeltaTime * 1.5f);
        if (GameSpeed < 0.1f) {
            GameSpeed = 0f;
            return true;
        }
        return false;
    }

    public bool LevelUp()
    {
        GameSpeed = Mathf.Lerp(GameSpeed, NextGameSpeed, GameLogic.GameDeltaTime * 1.5f);
        GameplayCamera.backgroundColor = Color.Lerp(GameplayCamera.backgroundColor, LevelColor, GameLogic.GameDeltaTime * 2f);
        if (Mathf.Abs (GameSpeed-NextGameSpeed) < 0.1f) {
            GameSpeed = NextGameSpeed;
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

	public void Stop()
	{
		GameSpeed = 0.0f;
		PlayerSpeed = 0.0f;
		BulletSpeed = 0.0f;
	}

	public void Reset()
	{
		GameSpeed = GameStartSpeed;
		PlayerSpeed = PlayerStartSpeed;
		BulletSpeed = BulletStartSpeed;
        LevelDuration = LevelStartDuration;
        EnemyDensity = EnemyStartDensity;
        BossStrength = BossStartStrength;
        GameplayCamera.backgroundColor = LevelColor;
    }
}
