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

    private float [] mTimeToDanger;
    private float PrevGameSpeed;
    private float EnemyDensity;

    public static float GameSpeed { get; private set; }
	public static float PlayerSpeed { get; private set; }
	public static float BulletSpeed { get; private set; }
    public static float LevelDuration { get; private set; }
    public static int BossStrength { get; private set; }

    void Awake()
	{
		Reset();

        mTimeToDanger = new float [] { EnemyCheckGap, EnemyCheckGap, EnemyCheckGap };
        /*
		EnemyWave [] waves = { 
			new EnemyWave( 1, 1 ), 
			new EnemyWave( 1, 2 ), 
			new EnemyWave( 1, 3 ), 
			new EnemyWave( 1, 4 ), 
			new EnemyWave( 2, 3 ), 
			new EnemyWave( 2, 4 ), 
			new EnemyWave( 2, 5 ), 
			new EnemyWave( 3, 3 ), 
			new EnemyWave( 3, 4 ), 
			new EnemyWave( 3, 5 ), 
			new EnemyWave( 3, 6 ), 
			new EnemyWave( 3, 8 ) 
		};

		mWaves = waves;*/
    }

    /*
	void Start()
	{
		GameSpeed = GameStartSpeed;
		PlayerSpeed = PlayerStartSpeed;
		BulletSpeed = BulletStartSpeed;
        LevelDuration = LevelStartDuration;

    }*/

    /*
	public int SpawnCount()
	{
		int enemiesToSpawn = 0;

		if( mCurrentRow < mWaves[mCurrentWave].NumberOfRows )
		{
			mTimeToNextRow -= GameLogic.GameDeltaTime;
			if( mTimeToNextRow <= 0.0f )
			{
				mCurrentRow++;
				enemiesToSpawn = mWaves[mCurrentWave].EnemiesPerRow;
				mTimeToNextRow = TimeBetweenRows;
			}
		}
		else
		{
			mTimeToNextWave -= GameLogic.GameDeltaTime;
			if( mTimeToNextWave <= 0.0f )
			{
				if( ( mCurrentWave + 1 ) < mWaves.Length )
				{
					GameSpeed += GameSpeedRamp;
					PlayerSpeed += PlayerSpeedRamp;
					BulletSpeed += BulletSpeedRamp;
					mCurrentWave++;
				}
				mTimeToNextWave = TimeBetweenWaves;
				mCurrentRow = 0;
			}
		}

		return enemiesToSpawn;
	}*/

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

    public void BossFight()
    {
        PrevGameSpeed = GameSpeed;
        GameSpeed = 0f;
    }

    public void LevelUp()
    {
        GameSpeed = PrevGameSpeed * SpeedGain;
        PlayerSpeed *= SpeedGain;
        BulletSpeed *= SpeedGain;
        LevelDuration *= SpeedGain;
        EnemyDensity *= SpeedGain;
        BossStrength *= (int) SpeedGain;
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
    }
}
