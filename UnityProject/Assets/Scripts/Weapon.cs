using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour 
{
    //Fields to be accessed (read-only) globally
    //public enum Theme { Dark, Bright };
    public static Material[] BulletMaterials { get; private set; }
    public static Color[] BulletColors { get; private set; }
    public static float BulletScale { get; private set; }



    //Fields to be set within Unity
    //[SerializeField] private Color[] BulletColors;
    //[SerializeField] private Color[] NormalBulletColors;
    /* In case Unity loses the state of this:
       = new Color[] {new Color (0.099f, 0.094f, 0.132f), new Color (0.9f, 0.9f, 0.92f)};
     */
     
    [SerializeField] private Material BulletMaterialNormal;
	[SerializeField] private Material BulletMaterialGolden;
	[SerializeField] private Material BulletMaterialIce;
	[SerializeField] private Material BulletMaterialExplosive;
	[SerializeField] private float RechargeTime;
    [SerializeField] private float mBulletScale;

	[Range( 1, 100 )] [SerializeField] private int BulletPoolSize;

	//private Bullet[] mPool;
    //For keeping track of the bullet pool
	private List<Bullet> mActive;
	private List<Bullet> mInactive;

    //For allowing the gun to fire or not
	private float mCharging;

    //private Color [] NormalBulletColors;

	public List<Bullet> ActiveBullets { get { return mActive; } }

	void Awake() {
        //Put materials in array
        BulletMaterials = new Material[] { BulletMaterialNormal, BulletMaterialGolden, BulletMaterialIce, BulletMaterialExplosive };
        
        //Put initial colors in array - to do
        BulletColors = new Color[] {
            new Color(BulletMaterialNormal.color.r,BulletMaterialNormal.color.g,BulletMaterialNormal.color.b),
            new Color(BulletMaterialGolden.color.r,BulletMaterialGolden.color.g,BulletMaterialGolden.color.b),
            new Color(BulletMaterialIce.color.r,BulletMaterialIce.color.g,BulletMaterialIce.color.b),
            new Color(BulletMaterialExplosive.color.r,BulletMaterialExplosive.color.g,BulletMaterialExplosive.color.b)
        };
        BulletScale = mBulletScale;
        /*Create the 2 theme colors for the normal bullet
        NormalBulletColors = new Color[] {
            new Color (0.099f, 0.094f, 0.132f),
            new Color (0.9f, 0.9f, 0.92f)
        };*/
        
        // Create active and available lists, and an array of all bullets
		mActive = new List<Bullet>();
		mInactive = new List<Bullet>();
		//mPool = new Bullet[BulletPoolSize];

        //Create bullets and add to lists
        for (int count = 0; count < BulletPoolSize; count++)
		{
            Bullet bullet = new Bullet (count,transform);
            //mPool[count] = bullet;
			mInactive.Add( bullet );
		}
		mCharging = 0.0f;
	}

	void Update()
	{
        // Update the position of each active bullet, keep a track of bullets which have gone off screen 
        /*List<GameObject> oldBullets = new List<GameObject>(); 
		for( int count = 0; count < mActive.Count; count++ )
		{
			Vector3 position = mActive[count].transform.position;
			position.y += GameLogic.GameDeltaTime * GameLogic.BulletSpeed;
			mActive[count].transform.position = position;
			if( position.y > GameLogic.ScreenHeight * 0.5f )
			{
				mActive[count].SetActive( false );
				oldBullets.Add( mActive[count] ); 
			}
		}

		// Remove the bullets which have gone off screen, return them to the available list
		for( int count = 0; count < oldBullets.Count; count++ )
		{
            oldBullets[count].transform.parent = transform;
            mActive.Remove(oldBullets[count]);
			mInactive.Add( oldBullets[count] ); 
		}*/

        //List<Bullet> ActiveLocal = new List<Bullet>(mActive);
        for (int i = mActive.Count - 1; i>= 0; i--) {
            if (mActive[i].Update()) {
                mInactive.Add(mActive[i]);
                mActive.RemoveAt(i);
            }
        }

		if( mCharging > 0.0f ) {
			mCharging -= GameLogic.GameDeltaTime;
		}
	}

	public bool Fire( Vector3 position, Bullet.Type type )
	{
		// If the gun is not charging, fire the first bullet in the available list
		if( mInactive.Count > 0 && mCharging <= 0.0f )
		{
			mInactive[0].Fire (type, position);
			mActive.Add(mInactive[0]);
			mInactive.RemoveAt(0);

            //Reset charging time
			mCharging = RechargeTime;

            //Confirm that bullet has been shot
            return true;
		}
        
		return false;
	}

    /*Normal bullets can change color depending on the background color
    public void SetBulletTheme (Theme theme)
    {
        BulletColors[(int)Bullet.Type.Normal] = NormalBulletColors[(int)theme];
        BulletMaterialNormal.color = BulletColors[(int)Bullet.Type.Normal];
    }*/
}
