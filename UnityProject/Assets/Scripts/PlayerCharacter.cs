using UnityEngine;
using System.Collections;

public class PlayerCharacter : MonoBehaviour 
{
	[SerializeField] private Camera GameplayCamera;
	[SerializeField] private float FireOffset;
    [SerializeField] private float PlayerScale;

	private Weapon mGun;
	private float mTargetPosition;
    private float mStartPosition;
    private float mMovementTime;
    private float mColumnSize;
	private float mStartY;
    private int[] Inventory;

    //How much movement of the column width can be left when we consider a new movement in the same direction
    private const float ColumnSafety = 0.3f;

	public Weapon Weapon { get { return mGun; } }
	public int Column { get; private set; }

	void Start() 
	{
		mColumnSize = ( GameLogic.ScreenHeight * GameplayCamera.aspect * 0.8f ) / 3;
        mStartY = GameLogic.ScreenHeight * -0.4f;

        //Set the starting position
        //Vector3 position = transform.position;
        //position.y = GameLogic.ScreenHeight * -0.4f;
        //mStartY = position.y; 
        //transform.position = position;

        //Set position, target position; clear inventory
        Reset();

        //Set the size
        transform.localScale = new Vector3(PlayerScale, PlayerScale, PlayerScale);

		// Look for the gun
		mGun = GetComponentInChildren<Weapon>();

		//Column = 1;
	}

	void Update()
	{
        //TODO: SmoothDamp? Or do this right?!
        //Nicely move the caracter
		Vector3 position = transform.position;
		if( mTargetPosition != position.x )
		{
            mMovementTime += GameLogic.GameDeltaTime * GameLogic.PlayerSpeed;
            position.x = Mathf.SmoothStep( mStartPosition, mTargetPosition, mMovementTime);
			transform.position = position;
		}

        //Search for powerups
        int CollectedPowerup = PowerupFactory.DetectCollisions(transform.position);
        if (CollectedPowerup >= 0)
        {
            Inventory[CollectedPowerup]++;
        }
    }

	public void Reset()
	{
        //Vector3 position = new Vector3( 0.0f, mStartY, 0.0f );
        transform.position = new Vector3(0f, mStartY, 0f);
		mTargetPosition = 0.0f;
        Inventory = new int[] { 0, 0, 0, 0 };

		Column = 1;
	}

	public int Fire (Bullet.Type BulletType, bool permissive)
	{
		if( mGun != null )
		{
            //Decision
            if (BulletType!=Bullet.Type.Normal && Inventory[(int)BulletType] > 0) {
                Inventory[(int)BulletType]--;
            }
            else if (BulletType != Bullet.Type.Normal && permissive) {
                BulletType = Bullet.Type.Normal;
            }
            else return -1;
            
            //Action
            Vector3 position = transform.position;
			position.y += FireOffset;
			mGun.Fire( position, BulletType);
            return (int)BulletType;
		}
        return -1;
	}

	public void MoveLeft()
	{
		if( Column >= 1 && GameLogic.GameDeltaTime > 0.0f && transform.position.x-mTargetPosition < mColumnSize * ColumnSafety)
		{
            mStartPosition = transform.position.x;
			mTargetPosition -= mColumnSize;
			Column--;
            mMovementTime = 0;
        }
	}

	public void MoveRight()
	{
		if( Column <= 1 && GameLogic.GameDeltaTime > 0.0f && mTargetPosition - transform.position.x < mColumnSize * ColumnSafety)
        {
            mStartPosition = transform.position.x;
            mTargetPosition += mColumnSize;
			Column++;
            mMovementTime = 0;

        }
	}
}
