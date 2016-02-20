using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour 
{
	[SerializeField] private TextMesh GameText;
	[SerializeField] private Camera GameplayCamera;
	[SerializeField] private Material EnemyMaterial; 
	[SerializeField] private float PlayerKillDistance;  
	[SerializeField] private float BulletKillDistance; 
	[SerializeField] private float BossHitDistance;
    [SerializeField] private float WaitTime;
	[SerializeField] private int MaxMissedEnemies;

	private enum State { TapToStart, Level, Boss, GameOver };
    private Boss boss;
    private bool bossAlive;

	private List<GameObject> mActiveEnemies;
	private DifficultyCurve mCurrentDifficulty;
	private PlayerCharacter mPlayerCharacter;
    private float mGameOverTime;
    private float mLevelTimeLeft;
    private float mDistanceTravelled;
	private int mMissedEnemies;
	private State mGameStatus;

	public static float GameDeltaTime { get; private set; }
	public static float GameSpeed { get { return DifficultyCurve.GameSpeed; } }
	public static float PlayerSpeed { get { return DifficultyCurve.PlayerSpeed; } }
	public static float BulletSpeed { get { return DifficultyCurve.BulletSpeed; } }
	public static float ScreenBounds { get; private set; }
	public static float ScreenHeight { get; private set; }
	public static bool Paused { get; private set; }

	void Awake()
	{
		float distance = transform.position.z - GameplayCamera.transform.position.z;
		ScreenHeight = CameraUtils.FrustumHeightAtDistance( distance, GameplayCamera.fieldOfView );
		ScreenBounds = ScreenHeight * GameplayCamera.aspect * 0.5f;

		GameInput.OnTap += HandleOnTap;
        GameInput.OnSwipe += HandleOnSwipe;
        GameInput.OnKeyPress += HandleKeyPress;

        mActiveEnemies = new List<GameObject>();
		mCurrentDifficulty = GetComponentInChildren<DifficultyCurve>();
		mPlayerCharacter = GetComponentInChildren<PlayerCharacter>();
		mGameStatus = State.TapToStart;
        mGameOverTime = Time.timeSinceLevelLoad;
		mMissedEnemies = 0;
        Paused = false;
	}

    void Start()
    {
        //This must be retrieved after the difficulty curve has been "awaken"
        mLevelTimeLeft = DifficultyCurve.LevelDuration;
    }

	void Update()
	{
		GameDeltaTime = Paused ? 0.0f : Time.deltaTime;

		if( mGameStatus == State.Level )
		{
			mDistanceTravelled += GameSpeed * GameDeltaTime;
			GameText.text = string.Format( "Distance: {0:0.0} m", mDistanceTravelled );

            mLevelTimeLeft -= GameDeltaTime;
            if (mLevelTimeLeft > 0f) {
                int enemies = mCurrentDifficulty.SpawnPattern();
                for (int ColumnCount = 0; ColumnCount < 3; ColumnCount++)
                {
                    if ((enemies & (1 << ColumnCount)) != 0)
                    {
                        mActiveEnemies.Add(EnemyFactory.Dispatch((EnemyFactory.Column)ColumnCount));
                    }
                }
            }
            else if (mActiveEnemies.Count == 0)
            {
                //Switching game type
                if (mCurrentDifficulty.SlowDown()) {
                    mGameStatus = State.Boss;
                    bossAlive = true;
                    boss = new Boss(GameplayCamera, EnemyMaterial);
                }
            }

            // Update the position of each active enemy, keep a track of enemies which have gone off screen 
            List<GameObject> oldEnemys = new List<GameObject>(); 
			for( int count = 0; count < mActiveEnemies.Count; count++ )
            { 
				Vector3 position = mActiveEnemies[count].transform.position;
				position.y -= GameDeltaTime * GameSpeed;
				mActiveEnemies[count].transform.position = position;
				if( position.y < ScreenHeight * -0.5f )
				{
					EnemyFactory.Return( mActiveEnemies[count] );
					oldEnemys.Add( mActiveEnemies[count] ); 
					mMissedEnemies++;
				}
				else
				{
					Vector3 diff = mPlayerCharacter.transform.position - mActiveEnemies[count].transform.position;
					if( diff.sqrMagnitude < PlayerKillDistance )
					{
						// Touched enemny - Game over
						mCurrentDifficulty.Stop();
                        mGameOverTime = Time.timeSinceLevelLoad;
						mGameStatus = State.GameOver;
						GameText.text = string.Format( "You died!\nTotal distance: {0:0.0} m", mDistanceTravelled );
					}
					else
					{
						for( int bullet = 0; bullet < mPlayerCharacter.Weapon.ActiveBullets.Count; bullet++ )
						{
							if( mPlayerCharacter.Weapon.ActiveBullets[bullet].activeInHierarchy )
							{
								Vector3 diffToBullet = mActiveEnemies[count].transform.position - mPlayerCharacter.Weapon.ActiveBullets[bullet].transform.position;
								if( diffToBullet.sqrMagnitude < BulletKillDistance )
								{
									EnemyFactory.Return( mActiveEnemies[count] );
									oldEnemys.Add( mActiveEnemies[count] ); 
									mPlayerCharacter.Weapon.ActiveBullets[bullet].SetActive( false );
									break;
								}
							}
						}
					}
				}
			}

			if( mMissedEnemies >= MaxMissedEnemies )
			{
                // Too many missed enemies - Game over
				mCurrentDifficulty.Stop();
                mGameOverTime = Time.timeSinceLevelLoad;
                mGameStatus = State.GameOver;
				GameText.text = string.Format( "You have been invaded!\nTotal distance: {0:0.0} m", mDistanceTravelled );
			}

			for( int count = 0; count < oldEnemys.Count; count++ )
			{
				mActiveEnemies.Remove( oldEnemys[count] );
			}
		}

        if (mGameStatus == State.Boss)
        {
            if (bossAlive) {

                //Move and rotate boss
                boss.Update(mPlayerCharacter.transform.position);

                //Check doom height
                if (boss.Position().y < ScreenHeight * -0.25f)
                {
                    //Perform game over sh%t
                    mCurrentDifficulty.Stop();
                    mGameOverTime = Time.timeSinceLevelLoad;
                    if (boss.Invade(mPlayerCharacter.transform.position))
                    {
                        mGameStatus = State.GameOver;
                        GameText.text = string.Format("You have been eaten!\nTotal distance: {0:0.0} m", mDistanceTravelled);
                    }
                }

                //Check bullets
                for (int bullet = 0; bullet < mPlayerCharacter.Weapon.ActiveBullets.Count; bullet++)
                {
                    if (mPlayerCharacter.Weapon.ActiveBullets[bullet].activeInHierarchy)
                    {
                        Vector3 diffToBullet = boss.Position() - mPlayerCharacter.Weapon.ActiveBullets[bullet].transform.position;
                        if (diffToBullet.sqrMagnitude < BossHitDistance)
                        {
                            mPlayerCharacter.Weapon.ActiveBullets[bullet].SetActive(false);
                            if (boss.Hit(10))
                            {
                                boss = null;
                                bossAlive = false;
                            }
                            break;
                        }
                    }
                }

            }
            else {
                //No more boss, transitioning to normal
                mDistanceTravelled += GameSpeed * GameDeltaTime;
                GameText.text = string.Format("Distance: {0:0.0} m", mDistanceTravelled);

                if (mCurrentDifficulty.LevelUp()) {
                    mGameStatus = State.Level;
                    mMissedEnemies = 0;
                    mLevelTimeLeft = DifficultyCurve.LevelDuration;
                    print(mLevelTimeLeft);
                }
            }
        }
    }

	private void Reset()
	{
        for (int bullet = 0; bullet < mPlayerCharacter.Weapon.ActiveBullets.Count; bullet++) {
            mPlayerCharacter.Weapon.ActiveBullets[bullet].SetActive(false);
        }
        if (boss != null)
            boss.Destroy();
        mPlayerCharacter.Reset();
		mCurrentDifficulty.Reset();
		EnemyFactory.Reset();
		mActiveEnemies.Clear();
		mMissedEnemies = 0;
		mDistanceTravelled = 0.0f;
        mLevelTimeLeft = DifficultyCurve.LevelDuration;
    }

	private void HandleOnTap( Vector3 position )
	{
		switch( mGameStatus )
		{
		case State.TapToStart:
			Paused = false;
			mGameStatus = State.Level;
			break;
        case State.Level:
        case State.Boss:
                mPlayerCharacter.Fire();
			break;
		case State.GameOver:
            if (Time.timeSinceLevelLoad - mGameOverTime > WaitTime)
            { 
			    Reset();
			    GameText.text = "Tap to Start";
			    mGameStatus = State.TapToStart;
			}
            break;
		}
	}

	
	private void HandleOnSwipe( GameInput.Direction direction )
	{
		if( mGameStatus == State.Level || mGameStatus == State.Boss )
		{
			switch( direction )
			{
			case GameInput.Direction.Left:
				mPlayerCharacter.MoveLeft();
				break;
			case GameInput.Direction.Right:
				mPlayerCharacter.MoveRight();
				break;
			}
		}
	}

    private void HandleKeyPress (GameInput.Direction direction)
    {
        switch (mGameStatus)
        {
            case State.TapToStart:
                if (direction == GameInput.Direction.Up || direction == GameInput.Direction.Through)
                {
                    Paused = false;
                    mGameStatus = State.Level;
                }
                break;
            case State.Level:
            case State.Boss:
                switch (direction)
                {
                    case GameInput.Direction.Left:
                        mPlayerCharacter.MoveLeft();
                        break;
                    case GameInput.Direction.Right:
                        mPlayerCharacter.MoveRight();
                        break;
                    case GameInput.Direction.Up:
                    case GameInput.Direction.Through:
                        mPlayerCharacter.Fire();
                        break;
                }
                break;
            case State.GameOver:
                if (Time.timeSinceLevelLoad - mGameOverTime > WaitTime && 
                    (direction == GameInput.Direction.Up || direction == GameInput.Direction.Through))
                {
                    Reset();
                    GameText.text = "Tap to Start";
                    mGameStatus = State.TapToStart;
                }
                break;
        }
    }
}
