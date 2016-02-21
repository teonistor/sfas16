using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour 
{
	[SerializeField] private Material BulletMaterialNormal;
	[SerializeField] private Material BulletMaterialGolden;
	[SerializeField] private Material BulletMaterialIce;
	[SerializeField] private Material BulletMaterialExplosive;
	[SerializeField] private float BulletScale;
	[SerializeField] private float RechargeTime;

	[Range( 1, 100 )] [SerializeField] private int BulletPoolSize;

	private Bullet[] mPool;
	private List<Bullet> mActive;
	private List<Bullet> mInactive;
	private float mCharging;

    private Color [] BulletNormalColors;
    public enum Theme {Dark, Bright};
    public static Material[] BulletMaterials { get; private set;}

	public List<Bullet> ActiveBullets { get { return mActive; } }

	void Awake() {
        //Put materials in array
        BulletMaterials = new Material[] { BulletMaterialNormal, BulletMaterialGolden, BulletMaterialIce, BulletMaterialExplosive };

        //Create the 2 theme colors for the normal bullet
        BulletNormalColors = new Color[] {
            new Color (0.099f, 0.094f, 0.132f),
            new Color (0.9f, 0.9f, 0.92f)
        };
        
        // Create active and available lists, and an array of all bullets
		mActive = new List<Bullet>();
		mInactive = new List<Bullet>();
		mPool = new Bullet[BulletPoolSize];

        //Create bullets and add to lists
        for (int count = 0; count < mPool.Length; count++)
		{
            Bullet bullet = new Bullet (count,transform,BulletScale);
            mPool[count] = bullet;
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

        List<Bullet> ActiveLocal = new List<Bullet>(mActive);
        foreach (Bullet bullet in ActiveLocal) {
            if (bullet.Update()) {
                mActive.Remove(bullet);
                mInactive.Add(bullet);
            }
        }

		if( mCharging > 0.0f ) {
			mCharging -= GameLogic.GameDeltaTime;
		}
	}

	public bool Fire( Vector3 position )
	{
		// If the gun is not charging, fire the first bullet in the available list
		if( mInactive.Count > 0 && mCharging <= 0.0f )
		{
			mInactive[0].Fire (Bullet.Type.Golden, position);
			mActive.Add(mInactive[0]);
			mInactive.Remove(mInactive[0]);

            //Reset charging time
			mCharging = RechargeTime;
		}
        
		return false;
	}

    //Normal bullets can change color depending on the background color
    public void SetBulletTheme (Theme theme)
    {
        BulletMaterialNormal.color = BulletNormalColors[(int)theme];
    }
}
