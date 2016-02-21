using UnityEngine;
using System.Collections;
using System;

public class Bullet {

    private GameObject bullet;
    private CreateMesh mesh;
    private Transform ParentTransform;
    private Type mType;
    private int[] mDamageValues = { 10, 20, 20, 20 };

    //There will be several types of bullets (under construction)
    public enum Type { Normal, Golden, Ice, Explosive }
    public int DamageValue { get { return mDamageValues[(int)mType]; } }


    public Bullet (int count, Transform ParentTransform, float BulletScale) {
        bullet = new GameObject("Bullet_Pool_ID_" + (count + 1));
        mesh = bullet.AddComponent<CreateMesh>();
        bullet.transform.localScale = new Vector3(BulletScale, BulletScale, BulletScale );
        bullet.transform.parent = ParentTransform;
        this.ParentTransform = ParentTransform;
	    bullet.SetActive( false );
	}

    public void Fire (Type BulletType, Vector3 InitialPosition){
        mType = BulletType;
        bullet.SetActive(true);
        mesh.Material = Weapon.BulletMaterials[(int)mType];
        bullet.transform.parent = null;
        bullet.transform.position = InitialPosition;
    }
	
	// Returns true if the bullet has flown off the screen
	public bool Update () {
	    Vector3 position = bullet.transform.position;
        position.y += GameLogic.GameDeltaTime * GameLogic.BulletSpeed;
        bullet.transform.position = position;
        if (position.y > GameLogic.ScreenHeight * 0.5f) {
            bullet.transform.parent = ParentTransform;
            bullet.SetActive(false);
            return true;
        }
        return false;
    }

    //Returns true if the bullet is active and has hit the enemy
    public bool CheckHit(Vector3 EnemyPosition, float TouchingDistance) {
        if (bullet.activeInHierarchy) {
            Vector3 diffToBullet = EnemyPosition - bullet.transform.position;
            if (diffToBullet.sqrMagnitude < TouchingDistance)
            {
                if (mType != Type.Golden)
                    bullet.SetActive(false);
                return true;
            }
        }
        return false;
    }

    public void SetActive (bool IsActive) {
        bullet.SetActive(IsActive);
    }
}
