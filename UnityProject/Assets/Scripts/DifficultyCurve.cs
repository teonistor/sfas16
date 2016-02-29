using UnityEngine;
using System.Collections.Generic;

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
    private int SpawnMask;

    public static float EnemySpeed { get; private set; }
    public static float ScenerySpeed { get; private set; }
    public static float PlayerSpeed { get; private set; }
	public static float BulletSpeed { get; private set; }
    public static float LevelDuration { get; private set; }
    public static int BossStrength { get; private set; }
    public bool GameFrozen { get; private set; }
    public bool FireAllowed { get; private set; }
    public int SlowDownStage { get; private set; }
    public int TutorialStage { get; private set; }

    void Awake()
	{
        LevelColor = new Color(0.262f, 0.236f, 0.574f);
        BossTimeColor = Color.black;

        Reset();

        mTimeToDanger = new float [] { EnemyCheckGap, EnemyCheckGap, EnemyCheckGap };

        //This delegate will allow Player Character to trigger the last tutorial, on addition of first element to inventory
        PlayerCharacter.TriggerTutorial += TriggerTutorial;
    }

    public int SpawnPattern()
    {
        int pattern = 0;

        for (int ColumnCount = 0; ColumnCount < 3; ColumnCount++)
        {
            //Count down the time until we should ask ourselves whether there should be an enemy on this column
            mTimeToDanger[ColumnCount] -= GameLogic.GameDeltaTime;
            if (mTimeToDanger[ColumnCount] < 0f)
            {
                if (Random.Range(0f, 100f) < EnemyDensity)
                {
                    //The following line is a hack to make the spawning of an enemy more probable when some columns are
                    //blocked by the mask. It only works in one direction and it is only used during the tutorials.
                    //That's why it's a hack.
                    pattern |= 1 << ((SpawnMask&1<<ColumnCount)!=0? ColumnCount : ColumnCount+1);

                    //if an enemy appeared, ask again after a longer time
                    mTimeToDanger[ColumnCount] = EnemySafeGap;
                }
                else
                {   //if an enemy did not appear, ask again after a shorter time
                    mTimeToDanger[ColumnCount] = EnemyCheckGap;
                }
            }
        }
        return pattern&SpawnMask;
    }

    public void SlowDown() {
        NextGameSpeed = EnemySpeed * SpeedGain;
        SlowDownStage = 1;
    }

    public bool LevelUp()
    {
        SlowDownStage = 0;

        ScenerySpeed = Mathf.Lerp(ScenerySpeed, NextGameSpeed, GameLogic.GameDeltaTime * 1.5f);
        GameplayCamera.backgroundColor = Color.Lerp(GameplayCamera.backgroundColor, LevelColor, GameLogic.GameDeltaTime * 2f);
        if (Mathf.Abs (ScenerySpeed - NextGameSpeed) < 0.1f) {
            EnemySpeed = ScenerySpeed = NextGameSpeed;
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
        switch (TutorialStage) {
            //Automatically triggering next piece of tutorial
            case -1:
            case -2:
            case -5:
            case -6:
            case -8:
            case -9:
            case -13:
            case -14: TriggerTutorial(-TutorialStage+1); break;
            case -3:
                GameLogic.Paused = false;
                TriggerTutorial(4);
                break;
            case -4:
                //This will force enemies on the centre column
                SpawnMask = 2;
                FireAllowed = true;
                TriggerTutorial(5);
                break;
            case -7:
                //This will force enemies on the right
                SpawnMask = 4;
                TriggerTutorial(8);
                break;
            case -10:
                GameLogic.Paused = false;
                FireAllowed = true;
                //Allow enemies everywhere
                SpawnMask = 7;
                break;
            case -11:
                GameLogic.Paused = false;
                SpawnMask = 7;
                FireAllowed = true;
                TriggerTutorial(12);
                break;
            case -15:
                GameLogic.Paused = false;
                SpawnMask = 7;
                FireAllowed = true;
                break;
        }

        //Negative numbers from above mean tutorial is over
        if (TutorialStage < 0) {
            TutorialStage = 0;
        }

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
        NextGameSpeed = GameStartSpeed;

        PlayerSpeed = PlayerStartSpeed;
		BulletSpeed = BulletStartSpeed;

        LevelDuration = LevelStartDuration;
        EnemyDensity = EnemyStartDensity;
        BossStrength = BossStartStrength;

        GameplayCamera.backgroundColor = LevelColor;
        SpawnMask = 7;

        GameFrozen = false;
        FireAllowed = true;
        SlowDownStage = 0;
        TutorialStage = 0;
    }

    public void TriggerTutorial (int code) {
        List<bool> TutorialsLeft = gameObject.GetComponentInChildren<SaveLoad>().TutorialsLeft;
        
        if (TutorialsLeft[code-1]) {
            // Tutorial has not been done, so do it
            TutorialStage = code;
            gameObject.GetComponentInChildren<DisplayTutorial>().Display(code);

            //For some tutorials we ought to stop things from moving
            if (code == 1 || code == 2 || code == 3 || code== 9 || code == 11 || code == 13) {
                GameLogic.Paused = true;
            }

            //For some tutorials we ought to prevent enemies from spawning and forbid shooting
            if (code == 1 || code == 2 || code == 3 || code == 4 || code== 9 || code == 11 || code == 13) {
                SpawnMask = 0;
                FireAllowed = false;
            }
        }
        else {
            //tbc!!
        }
    }

    public void NotifyTutorialDone (int code) {
        // Make note when the last tutorial in a cascade has been done
        if (code==10 || code == 12 || code == 15 || code == 16) {
            gameObject.GetComponentInChildren<SaveLoad>().addCompletedTutorial(code);
        }
        TutorialStage = -code;
    }
}
