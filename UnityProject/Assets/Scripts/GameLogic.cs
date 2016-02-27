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

	private enum State { TapToStart, Level, Boss, GameOver }; //will add: tutorial, freeze (?)
    private Boss boss;
    //private bool bossAlive;

	//private List<GameObject> mActiveEnemies;
	private DifficultyCurve mCurrentDifficulty;
	private PlayerCharacter mPlayerCharacter;
    private float mGameOverTime;
    private float mLevelTimeLeft;
    private float mDistanceTravelled;
    private bool GameFrozen;
	private int mMissedEnemies;
	private State mGameStatus;

    private List<GameObject> mActiveEnemies { get {return EnemyFactory.GetActiveEnemies(); } }

    public static float GameDeltaTime { get; private set; }
	public static float EnemySpeed { get { return DifficultyCurve.EnemySpeed; } }
	public static float PlayerSpeed { get { return DifficultyCurve.PlayerSpeed; } }
	public static float BulletSpeed { get { return DifficultyCurve.BulletSpeed; } }
    public static float ScenerySpeed { get { return DifficultyCurve.ScenerySpeed; } }
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

        //mActiveEnemies = new List<GameObject>();
		mCurrentDifficulty = GetComponentInChildren<DifficultyCurve>();
		mPlayerCharacter = GetComponentInChildren<PlayerCharacter>();
		mGameStatus = State.TapToStart;
        mGameOverTime = Time.timeSinceLevelLoad;
		mMissedEnemies = 0;
        //TODO: Fix initial inv
        //TODO: Display inf
        //Inventory = new int[] { 0, 10, 10, 10 };
        GameFrozen = false;
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
        //List<GameObject> mActiveEnemies = new List<GameObject> ( EnemyFactory.GetActiveEnemies());
        
        if (GameFrozen) {
            if (mCurrentDifficulty.Defrost()) {
                GameFrozen = false;
            }
        }

        if ( mGameStatus == State.Level )
		{
			mDistanceTravelled += ScenerySpeed * GameDeltaTime;
			GameText.text = string.Format( "Distance: {0:0.0} m", mDistanceTravelled );

            mLevelTimeLeft -= GameDeltaTime;

            if (!GameFrozen && mLevelTimeLeft > 0f) {
                int enemies = mCurrentDifficulty.SpawnPattern();
                for (int ColumnCount = 0; ColumnCount < 3; ColumnCount++)
                {
                    if ((enemies & (1 << ColumnCount)) != 0)
                    {
                        //The double-speed problem lies here: enemies are added twice!
                        //mActiveEnemies.Add(EnemyFactory.Dispatch((EnemyFactory.Column)ColumnCount));
                        EnemyFactory.Dispatch((EnemyFactory.Column)ColumnCount);
                    }
                }

                PowerupFactory.DetectCollisions(mPlayerCharacter.transform.position);
            }
            else if (mActiveEnemies.Count == 0)
            {
                //Switching game type
                if (mCurrentDifficulty.SlowDown()) {
                    mGameStatus = State.Boss;
                    //bossAlive = true;
                    boss = new Boss(GameplayCamera, EnemyMaterial);

                    //mPlayerCharacter.Weapon.SetBulletTheme(Weapon.Theme.Bright);
                }

                //At this point, we know there are no enemies to check, so it is OK to skip the loop below
                return;
            }

            // Update the position of each active enemy, keep a track of enemies which have gone off screen 
            //List<GameObject> oldEnemys = new List<GameObject>();

            /* Traverse the list descendingly so that removals of already visited elements does not affect the
             * elements yet to be visited.
             */
			for( int count = mActiveEnemies.Count-1; count >=0; count-- )
            { 
				Vector3 ThisPosition = mActiveEnemies[count].transform.position;

                //Update each enemy's position according to the game speed
				ThisPosition.y -= GameDeltaTime * EnemySpeed;
				mActiveEnemies[count].transform.position = ThisPosition;

                //Check if the enemy has flown off screen. If so, return it to the factory and count it as missed.
				if( ThisPosition.y < ScreenHeight * -0.5f )
				{
					EnemyFactory.Return( mActiveEnemies[count] );
					///oldEnemys.Add( mActiveEnemies[count] ); 
					mMissedEnemies++;
				}
				else
				{
                    //Check if the enemy is too close to the player. If so, end the game.
					Vector3 diff = mPlayerCharacter.transform.position - ThisPosition;
					if( diff.sqrMagnitude < PlayerKillDistance) {
						// Touched enemny - Game over
						mCurrentDifficulty.GameOver();
                        mGameOverTime = Time.timeSinceLevelLoad;
						mGameStatus = State.GameOver;
						GameText.text = string.Format( "You died!\nTotal distance: {0:0.0} m", mDistanceTravelled );
					}
					else
					{
                        //List<Bullet> TempBullets = mPlayerCharacter.Weapon.ActiveBullets.Clo

                        for ( int bullet = 0; bullet < mPlayerCharacter.Weapon.ActiveBullets.Count; bullet++ )
						{
                            /*if( mPlayerCharacter.Weapon.ActiveBullets[bullet].activeInHierarchy )
							{
								Vector3 diffToBullet = mActiveEnemies[count].transform.position - mPlayerCharacter.Weapon.ActiveBullets[bullet].transform.position;
								if( diffToBullet.sqrMagnitude < BulletKillDistance )
								{
									EnemyFactory.Return( mActiveEnemies[count] );
									oldEnemys.Add( mActiveEnemies[count] ); 
									mPlayerCharacter.Weapon.ActiveBullets[bullet].SetActive( false );
									break;
								}
							}*/
                            if (mPlayerCharacter.Weapon.ActiveBullets[bullet].CheckHit(ThisPosition, BulletKillDistance,false))
                            {
                                EnemyFactory.Return(mActiveEnemies[count]);
                                //oldEnemys.Add(mActiveEnemies[count]);
                                break;
                            }
                        }
					}
				}
			}

			if( mMissedEnemies >= MaxMissedEnemies )
			{
                // Too many missed enemies - Game over
				mCurrentDifficulty.GameOver();
                mGameOverTime = Time.timeSinceLevelLoad;
                mGameStatus = State.GameOver;
				GameText.text = string.Format( "You have been invaded!\nTotal distance: {0:0.0} m", mDistanceTravelled );
			}

			/*for( int count = 0; count < oldEnemys.Count; count++ )
			{
				mActiveEnemies.Remove( oldEnemys[count] );
			}*/
		}

        if (mGameStatus == State.Boss)
        {
            if (boss!=null) {

                //Only if the boss is allowed to move
                if (!GameFrozen) {

                    //Move and rotate boss
                    boss.Update(mPlayerCharacter.transform.position);

                    //Check if boss has eaten the player and if so end the game
                    if (boss.HasEaten(mPlayerCharacter.transform.position)) {
                        mCurrentDifficulty.GameOver();
                        mGameOverTime = Time.timeSinceLevelLoad;
                        mGameStatus = State.GameOver;
                        GameText.text = string.Format("You have been eaten!\nTotal distance: {0:0.0} m", mDistanceTravelled);
                    }
                }

                //Check bullets
                foreach (Bullet bullet in mPlayerCharacter.Weapon.ActiveBullets) {
                    /*if (mPlayerCharacter.Weapon.ActiveBullets[bullet].activeInHierarchy)
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
                    }*/
                    if (bullet.CheckHit(boss.Position(), BossHitDistance, true)) {
                        if (boss.Hit(bullet.DamageValue)) {
                            boss = null;
                            //bossAlive = false;
                            //mPlayerCharacter.Weapon.SetBulletTheme(Weapon.Theme.Dark);
                        }
                        break;
                    }
                }

            }
            else {
                //No more boss, transitioning to normal
                mDistanceTravelled += ScenerySpeed * GameDeltaTime;
                GameText.text = string.Format("Distance: {0:0.0} m", mDistanceTravelled);

                /*int CollectedPowerup = PowerupFactory.DetectCollisions(mPlayerCharacter.transform.position);
                if (CollectedPowerup >= 0) {
                    Inventory[CollectedPowerup]++;
                }*/

                if (mCurrentDifficulty.LevelUp()) {
                    mGameStatus = State.Level;
                    mMissedEnemies = 0;
                    mLevelTimeLeft = DifficultyCurve.LevelDuration;
                }
            }
        }
    }

    private void Fire (Bullet.Type BulletType/*, bool permissive*/)
    {
        //Until we fix the input
        bool permissive = true;
        //Bullet.Type BulletType = Bullet.Type.Normal;

        int ShotBullet = mPlayerCharacter.Fire(BulletType);

        if (ShotBullet == (int)Bullet.Type.Ice) {
            GameFrozen = true;
            mCurrentDifficulty.Freeze();
        }
        /*
        if (BulletType == Bullet.Type.Normal) {
            mPlayerCharacter.Fire(BulletType);
        }
        else {
            if (Inventory[(int)BulletType] > 0) {
                Inventory[(int)BulletType]--;
                mPlayerCharacter.Fire(BulletType);
                if (BulletType == Bullet.Type.Ice) {
                    GameFrozen = true;
                    mCurrentDifficulty.Freeze();
                }
            }
            else if (permissive) {
                mPlayerCharacter.Fire(Bullet.Type.Normal);
            }
        }*/
    }

	private void Reset()
	{
        for (int bullet = 0; bullet < mPlayerCharacter.Weapon.ActiveBullets.Count; bullet++) {
            mPlayerCharacter.Weapon.ActiveBullets[bullet].SetActive(false);
        }
        if (boss != null) {
            boss.Destroy();
            boss = null;
        }
        mPlayerCharacter.Reset();
        //mPlayerCharacter.Weapon.SetBulletTheme(Weapon.Theme.Dark);
        mCurrentDifficulty.Reset();
		EnemyFactory.Reset();
        PowerupFactory.Reset();
		//mActiveEnemies.Clear();
		mMissedEnemies = 0;
		mDistanceTravelled = 0.0f;
        mLevelTimeLeft = DifficultyCurve.LevelDuration;
        //Inventory = new int[] { 0, 0, 0, 0 };
        GameFrozen = false;
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
                Fire((Bullet.Type)(1+DisplayInventory.ButtonAt(new Vector2(position.x,position.y))));
			break;
		case State.GameOver:
            if (Time.timeSinceLevelLoad - mGameOverTime > WaitTime)
            { 
			    Reset();
			    GameText.text = "Tap to start";
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

    private void HandleKeyPress (GameInput.Key key)
    {
        switch (mGameStatus)
        {
            case State.TapToStart:
                if (key == GameInput.Key.Fire)
                {
                    Paused = false;
                    mGameStatus = State.Level;
                }
                break;
            case State.Level:
            case State.Boss:
                switch (key)
                {
                    case GameInput.Key.Left:
                        mPlayerCharacter.MoveLeft();
                        break;
                    case GameInput.Key.Right:
                        mPlayerCharacter.MoveRight();
                        break;
                    case GameInput.Key.Fire:
                        Fire(Bullet.Type.Normal);
                        break;
                    case GameInput.Key.Fire1:
                        Fire(Bullet.Type.Explosive);
                        break;
                    case GameInput.Key.Fire2:
                        Fire(Bullet.Type.Ice);
                        break;
                    case GameInput.Key.Fire3:
                        Fire(Bullet.Type.Golden);
                        break;
                }
                break;
            case State.GameOver:
                if (Time.timeSinceLevelLoad - mGameOverTime > WaitTime && key == GameInput.Key.Fire) {
                    Reset();
                    GameText.text = "Tap to start";
                    mGameStatus = State.TapToStart;
                }
                break;
        }
    }
}
