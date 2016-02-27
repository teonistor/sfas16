using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogicNew : MonoBehaviour 
{
	[SerializeField] private TextMesh GameText;
	[SerializeField] private Camera GameplayCamera;
	[SerializeField] private Material EnemyMaterial;
	[SerializeField] private float PlayerKillDistance;
	[SerializeField] private float BulletKillDistance;
	[SerializeField] private float BossHitDistance;
    [SerializeField] private float WaitTime;
	[SerializeField] private int MaxMissedEnemies;

	public enum State { NotPlaying, Level, Boss, SlowDown, LevelUp, Tutorial, LFrozen, BFrozen, Paused}; 
    private Boss boss;
    //private bool bossAlive;

	//private List<GameObject> mActiveEnemies;
	private DifficultyCurve mCurrentDifficulty;
	private PlayerCharacter mPlayerCharacter;
    private float mGameOverTime;
    private float mLevelTimeLeft;
    private float mDistanceTravelled;
    //private bool GameFrozen;
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
	//public static bool Paused { get; private set; }

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
		mGameStatus = State.NotPlaying;
        mGameOverTime = Time.timeSinceLevelLoad;
		mMissedEnemies = 0;
        //TODO: Fix initial inv
        //TODO: Display inf
        //Inventory = new int[] { 0, 10, 10, 10 };
        //GameFrozen = false;
        //Paused = false;
	}

    void Start()
    {
        //This must be retrieved after the difficulty curve has "awaken"
        mLevelTimeLeft = DifficultyCurve.LevelDuration;
    }

	void Update()
	{
		//GameDeltaTime = Paused ? 0.0f : Time.deltaTime;
        //List<GameObject> mActiveEnemies = new List<GameObject> ( EnemyFactory.GetActiveEnemies());

        if (mGameStatus == State.LFrozen) {
            if (DifficultyCurveNew.StateSwitchFlag) {

            }
        }
        if (mGameStatus == State.Level || mGameStatus == State.LFrozen || mGameStatus == State.SlowDown) {
            int enemies = mCurrentDifficulty.SpawnPattern();
            for (int ColumnCount = 0; ColumnCount < 3; ColumnCount++) {
                if ((enemies & (1 << ColumnCount)) != 0) {
                    EnemyFactory.Dispatch((EnemyFactory.Column)ColumnCount);
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
