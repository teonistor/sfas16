using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour 
{
    //Fields to be accessed (read-only) globally
    public static Material[] BulletMaterials { get; private set; }
    public static Color[] BulletColors { get; private set; }
    public static float BulletScale { get; private set; }

    //Fields to be set within Unity
    [SerializeField] private Material BulletMaterialNormal;
	[SerializeField] private Material BulletMaterialGolden;
	[SerializeField] private Material BulletMaterialIce;
	[SerializeField] private Material BulletMaterialExplosive;
	[SerializeField] private float RechargeTime;
    [SerializeField] private float mBulletScale;

	[Range( 1, 100 )] [SerializeField] private int BulletPoolSize;
    
    //For keeping track of the bullet pool
	private List<Bullet> mActive;
	private List<Bullet> mInactive;

    //For allowing the gun to fire or not
	private float mCharging;

	public List<Bullet> ActiveBullets { get { return mActive; } }

	void Awake() {
        //Put materials in array
        BulletMaterials = new Material[] { BulletMaterialNormal, BulletMaterialGolden, BulletMaterialIce, BulletMaterialExplosive };
        
        //Put initial colors in array
        BulletColors = new Color[] {
            new Color(BulletMaterialNormal.color.r,BulletMaterialNormal.color.g,BulletMaterialNormal.color.b),
            new Color(BulletMaterialGolden.color.r,BulletMaterialGolden.color.g,BulletMaterialGolden.color.b),
            new Color(BulletMaterialIce.color.r,BulletMaterialIce.color.g,BulletMaterialIce.color.b),
            new Color(BulletMaterialExplosive.color.r,BulletMaterialExplosive.color.g,BulletMaterialExplosive.color.b)
        };
        BulletScale = mBulletScale;
        
        // Create active and available lists, and an array of all bullets
		mActive = new List<Bullet>();
		mInactive = new List<Bullet>();

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
}
