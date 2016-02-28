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
    private int[] Inventory /*{ get; private set; }*/;

    //How much movement of the column width can be left when we consider a new movement in the same direction
    private const float ColumnSafety = 0.3f;

	public Weapon Weapon { get { return mGun; } }
	public int Column { get; private set; }

    //Through delegate, Difficulty Curve will allow this script to trigger a tutorial when the first inventory element is added
    public delegate void TriggerTutorial16(int code);
    public static event TriggerTutorial16 TriggerTutorial;

    /*
    void Awake() {
        
    }*/

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

        //Allow inventory display script to access Inventory array
        DisplayInventory.GetInventoryDelegate += GetInventory;
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
            TriggerTutorial(16);
        }
    }

	public void Reset()
	{
        //Vector3 position = new Vector3( 0.0f, mStartY, 0.0f );
        transform.position = new Vector3(0f, mStartY, 0f);
		mTargetPosition = 0.0f;
        //Inventory = new int[(int)PowerupFactory.Type.NoPowerups];
        Inventory = new int[] { 0, 12, 3 };

		Column = 1;
	}

	public int Fire (Bullet.Type BulletType/*, bool permissive*/)
	{
		if( mGun != null )
		{
            /*Decision
            if (BulletType!=Bullet.Type.Normal && Inventory[(int)BulletType] > 0) {
                Inventory[(int)BulletType]--;
            }
            else if (BulletType != Bullet.Type.Normal && permissive) {
                //BulletType = Bullet.Type.Normal;
            }
            else if (!permissive)
                return -1;*/

            //Decision
            //Inventory is based on the Powerups enum, which is incompatible with the Bullet enum
            //A math expression could be used, but it would be difficult to understand and would break if anything changed about either of the 2 objects
            int invIndex = -1;
            switch (BulletType) {
                case Bullet.Type.Ice:
                    invIndex = (int)PowerupFactory.Type.Ice;
                    break;
                case Bullet.Type.Golden:
                    invIndex = (int)PowerupFactory.Type.Golden;
                    break;
                case Bullet.Type.Explosive:
                    invIndex = (int)PowerupFactory.Type.Explosive;
                    break;
            }

            //If a bullet that exists in finite supply has been chosen, check there is enough of it
            if (invIndex >= 0 && Inventory[invIndex] <= 0)
                return -1;
            
            //Action
            Vector3 position = transform.position;
			position.y += FireOffset;

            //Only decrease inventory if the bullet was actually fired
            if (mGun.Fire(position, BulletType) && invIndex >= 0)
                Inventory[invIndex]--;
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

    private int[] GetInventory() {
        return Inventory;
    }
}
