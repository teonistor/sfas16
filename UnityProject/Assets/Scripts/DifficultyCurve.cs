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
    [SerializeField] private float FreezeTime;
    [SerializeField] private Camera GameplayCamera;

    private float [] mTimeToDanger;
    private float NextGameSpeed;
    private float EnemyDensity;
    private float DefrostTime;

    private Color LevelColor;
    private Color BossTimeColor;

    public static float EnemySpeed { get; private set; }
    public static float ScenerySpeed { get; private set; }
    public static float PlayerSpeed { get; private set; }
	public static float BulletSpeed { get; private set; }
    public static float LevelDuration { get; private set; }
    public static int BossStrength { get; private set; }
    public bool GameFrozen { get; private set; }
    public int SlowDownStage { get; private set; }

    void Awake()
	{
        LevelColor = new Color(0.262f, 0.236f, 0.574f);
        BossTimeColor = Color.black;

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

    public void SlowDown() {
        NextGameSpeed = EnemySpeed * SpeedGain;
        SlowDownStage = 1;
    }

    public bool LevelUp()
    {
        SlowDownStage = 0;

        ScenerySpeed = Mathf.Lerp(ScenerySpeed, NextGameSpeed, GameLogic.GameDeltaTime * 1.5f);
        //The glitch of the scenery during level up if ice is shot lies in the following line:
        GameplayCamera.backgroundColor = Color.Lerp(GameplayCamera.backgroundColor, LevelColor, GameLogic.GameDeltaTime * 2f);
        if (Mathf.Abs (ScenerySpeed - NextGameSpeed) < 0.1f) {
            EnemySpeed = ScenerySpeed = NextGameSpeed;
            //NextGameSpeed = 0f;
            PlayerSpeed *= SpeedGain;
            BulletSpeed *= SpeedGain;
            LevelDuration *= SpeedGain;
            EnemyDensity *= SpeedGain;
            BossStrength *= (int)SpeedGain;
            return true;
        }
        return false;
    }

    public void Freeze() {
        EnemySpeed = 0f;
        GameFrozen = true;
        //Some recycling
        DefrostTime = -FreezeTime;
    }

    void Update() {
        if (GameFrozen) {
            DefrostTime += GameLogic.GameDeltaTime;
            if (DefrostTime > 0)
                EnemySpeed = Mathf.Lerp(0f, NextGameSpeed, DefrostTime);
            GameFrozen = EnemySpeed < NextGameSpeed;
        }
        if (SlowDownStage == 1) {
            GameplayCamera.backgroundColor = Color.Lerp(GameplayCamera.backgroundColor, BossTimeColor, GameLogic.GameDeltaTime * 2f);

            ScenerySpeed = Mathf.Lerp(ScenerySpeed, 0f, GameLogic.GameDeltaTime * 1.5f);
            if (ScenerySpeed < 0.1f) {
                ScenerySpeed = 0f;
                SlowDownStage=2;
            }
        }
    }

    /*Check if the effect of ice bullet has worn off
    public bool Ddefrost() {
        if (EnemySpeed >= NextGameSpeed)
            return true;
        return false;
    }*/

	public void GameOver() {
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
        GameFrozen = false;
    }
}
