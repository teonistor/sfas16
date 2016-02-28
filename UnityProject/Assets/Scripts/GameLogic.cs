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
	private int mMissedEnemies;
	private State mGameStatus;
    private bool GameFrozen { get { return mCurrentDifficulty == null ? false : mCurrentDifficulty.GameFrozen; } }
    private bool FireAllowed { get { return mCurrentDifficulty == null ? false : mCurrentDifficulty.FireAllowed; } }
    private int SlowDownStage { get { return mCurrentDifficulty == null ? -1 : mCurrentDifficulty.SlowDownStage; } }

    private List<GameObject> mActiveEnemies { get {return EnemyFactory.GetActiveEnemies(); } }

    public static float GameDeltaTime { get; private set; }
	public static float EnemySpeed { get { return DifficultyCurve.EnemySpeed; } }
	public static float PlayerSpeed { get { return DifficultyCurve.PlayerSpeed; } }
	public static float BulletSpeed { get { return DifficultyCurve.BulletSpeed; } }
    public static float ScenerySpeed { get { return DifficultyCurve.ScenerySpeed; } }
    public static float ScreenBounds { get; private set; }
	public static float ScreenHeight { get; private set; }
    public static bool Paused;

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
        
        /*if (GameFrozen) {
            if (mCurrentDifficulty.Defrost()) {
                GameFrozen = false;
            }
        }*/

        if ( mGameStatus == State.Level )
		{
			mDistanceTravelled += ScenerySpeed * GameDeltaTime;
            UpdateText(string.Format( "Distance: {0:0.0} m", mDistanceTravelled));

            mLevelTimeLeft -= GameDeltaTime;

            if (!GameFrozen) {
                if (mLevelTimeLeft > 0f) {

                    //Get a bit pattern and spawn enemies according to it
                    int enemies = mCurrentDifficulty.SpawnPattern();
                    for (int ColumnCount = 0; ColumnCount < 3; ColumnCount++) {
                        if ((enemies & (1 << ColumnCount)) != 0) {
                            EnemyFactory.Dispatch((EnemyFactory.Column)ColumnCount);
                        }
                    }

                    PowerupFactory.DetectCollisions(mPlayerCharacter.transform.position);
                }
                else if (mActiveEnemies.Count == 0) {
                    switch (SlowDownStage) {
                        case 0: //We are just now starting to slow down
                            mCurrentDifficulty.SlowDown();
                            break;
                        //For case 1 - slowing down in progress - nothing is to be done
                        case 2: //Slowing down has finished; switch to boss state
                            mGameStatus = State.Boss;
                            boss = new Boss(GameplayCamera, EnemyMaterial);
                            mCurrentDifficulty.TriggerTutorial(11);
                            break;
                    }

                    //At this point, we know there are no enemies to check, so it is OK to skip the loop below
                    return;
                }
            }
            
            /* Traverse the list descendingly so that removals of already visited elements does not affect the
             * elements yet to be visited.
             */
            for (int count = mActiveEnemies.Count - 1; count >= 0; count--) {
                Vector3 ThisPosition = mActiveEnemies[count].transform.position;

                //Update each enemy's position according to the game speed
                ThisPosition.y -= GameDeltaTime * EnemySpeed;
                mActiveEnemies[count].transform.position = ThisPosition;

                //Check if the enemy has flown off screen. If so, return it to the factory and count it as missed.
                if (ThisPosition.y < ScreenHeight * -0.5f) {
                    EnemyFactory.Return(mActiveEnemies[count]);
                    mMissedEnemies++;
                }

                //Check if the enemy is too close to the player. If so, end the game.
                else if ((mPlayerCharacter.transform.position - ThisPosition).sqrMagnitude < PlayerKillDistance) {
                    mCurrentDifficulty.GameOver();
                    mGameOverTime = Time.timeSinceLevelLoad;
                    mGameStatus = State.GameOver;
                    UpdateText(string.Format("You died!\nTotal distance: {0:0.0} m", mDistanceTravelled));
                }

                //Check if the enemy has been hit by a bullet
                else {
                    for (int bullet = 0; bullet < mPlayerCharacter.Weapon.ActiveBullets.Count; bullet++) {
                        if (mPlayerCharacter.Weapon.ActiveBullets[bullet].CheckHit(ThisPosition, BulletKillDistance, false)) {
                            EnemyFactory.Return(mActiveEnemies[count]);
                            break;
                        }
                    }
                }
            }
			
            //Check if the player has been "invaded" (too many enemies were missed
			if( mMissedEnemies >= MaxMissedEnemies ) {
				mCurrentDifficulty.GameOver();
                mGameOverTime = Time.timeSinceLevelLoad;
                mGameStatus = State.GameOver;
                UpdateText(string.Format( "You have been invaded!\nTotal distance: {0:0.0} m", mDistanceTravelled));
			}
		}

        else if (mGameStatus == State.Boss)
        {
            if (boss!=null) {

                //Only if the boss is allowed to move
                if (!GameFrozen) {

                    //Move and rotate boss to point to player's position
                    boss.Update(mPlayerCharacter.transform.position);

                    //Check if boss has eaten the player and if so end the game
                    if (boss.HasEaten(mPlayerCharacter.transform.position)) {
                        mCurrentDifficulty.GameOver();
                        mGameOverTime = Time.timeSinceLevelLoad;
                        mGameStatus = State.GameOver;
                        UpdateText( string.Format("You have been eaten!\nTotal distance: {0:0.0} m", mDistanceTravelled));
                    }
                }

                //Check wether boss was hit by a bullet
                foreach (Bullet bullet in mPlayerCharacter.Weapon.ActiveBullets) {
                    if (bullet.CheckHit(boss.Position(), BossHitDistance, true)) {
                        if (boss.Hit(bullet.DamageValue)) {
                            boss = null;
                            mCurrentDifficulty.TriggerTutorial(13);
                        }
                        break;
                    }
                }

            }

            //No more boss, transitioning to normal
            else {
                mDistanceTravelled += ScenerySpeed * GameDeltaTime;
                UpdateText( string.Format("Distance: {0:0.0} m", mDistanceTravelled));

                if (mCurrentDifficulty.LevelUp()) {
                    mGameStatus = State.Level;
                    mMissedEnemies = 0;
                    mLevelTimeLeft = DifficultyCurve.LevelDuration;
                }
            }
        }

        else if (mGameStatus == State.TapToStart) {
            mCurrentDifficulty.TriggerTutorial(1);
            if (mCurrentDifficulty.TutorialStage != 0) {
                mGameStatus = State.Level;
            }
        }
    }

    private void UpdateText(string text) {
        //Text is only allowed to update when no tutorial is in progress
        if (mCurrentDifficulty.TutorialStage == 0) {
            GameText.text = text;
        }
    }

    private void Fire (Bullet.Type BulletType) {

        //Fire may be denied by tutorial
        if (FireAllowed) {

            int ShotBullet = mPlayerCharacter.Fire(BulletType);

            //If an ice bullet was shot, freeze the game
            if (ShotBullet == (int)Bullet.Type.Ice) {
                //GameFrozen = true;
                mCurrentDifficulty.Freeze();
            }
        }
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
        mCurrentDifficulty.Reset();
		EnemyFactory.Reset();
        PowerupFactory.Reset();
		mMissedEnemies = 0;
		mDistanceTravelled = 0.0f;
        mLevelTimeLeft = DifficultyCurve.LevelDuration;
    }

	private void HandleOnTap( Vector3 position )
	{
        switch (mGameStatus) {
            case State.TapToStart:
                Paused = false;
                mGameStatus = State.Level;
                break;
            case State.Level:
            case State.Boss:
                //The 1+ thingy is a little fragile, because it relies on a relationship between the 2 enums
                Fire((Bullet.Type)(1 + DisplayInventory.ButtonAt(new Vector2(position.x, position.y))));
                break;
            case State.GameOver:
                if (Time.timeSinceLevelLoad - mGameOverTime > WaitTime) {
                    Reset();
                    GameText.text = "Tap to start";
                    mGameStatus = State.TapToStart;
                }
                break;
        }
	}


    private void HandleOnSwipe(GameInput.Direction direction) {
        if (mGameStatus == State.Level || mGameStatus == State.Boss) {
            switch (direction) {
                case GameInput.Direction.Left:
                    mPlayerCharacter.MoveLeft();
                    break;
                case GameInput.Direction.Right:
                    mPlayerCharacter.MoveRight();
                    break;
            }
        }
    }

    private void HandleKeyPress(GameInput.Key key) {
        switch (mGameStatus) {
            case State.TapToStart:
                if (key == GameInput.Key.Fire) {
                    Paused = false;
                    mGameStatus = State.Level;
                }
                break;
            case State.Level:
            case State.Boss:
                switch (key) {
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
