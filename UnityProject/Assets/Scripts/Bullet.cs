using UnityEngine;

public class Bullet
{
    //There will be several types of bullets (under construction)
    public enum Type { Normal, Golden, Ice, Explosive }
    public int DamageValue { get { return mDamageValues[(int)mType]; } }

    private GameObject bullet;
    private CreateMesh mesh;
    private Transform ParentTransform;
    private Type mType;
    private int[] mDamageValues = { 10, 20, 0, 25 };


    //These only applies to 'inflating' bullets
    private float Inflation;
    private const float InflationRate = 1.5f;
    private Color NormalColor;

    public Bullet (int count, Transform ParentTransform) {
        bullet = new GameObject("Bullet_Pool_ID_" + (count + 1));
        mesh = bullet.AddComponent<CreateMesh>();
        bullet.transform.localScale = new Vector3(Weapon.BulletScale, Weapon.BulletScale, Weapon.BulletScale );
        bullet.transform.parent = ParentTransform;
        this.ParentTransform = ParentTransform;
	    bullet.SetActive( false );
	}

    public void Fire (Type BulletType, Vector3 InitialPosition){
        mType = BulletType;
        mesh.Material = Weapon.BulletMaterials[(int)mType];
        bullet.transform.parent = null;
        bullet.transform.position = InitialPosition;
        Inflation = 0f;
        bullet.SetActive(true);
    }
	
	// Returns true if the bullet has flown off the screen
	public bool Update () {
        Vector3 position;

        //In principle, there are 2 types of bullets: those that fly and those that inflate
        switch (mType) {
            case Type.Normal:
            case Type.Golden:
                position = bullet.transform.position;
                position.y += GameLogic.GameDeltaTime * GameLogic.BulletSpeed;
                bullet.transform.position = position;
                if (position.y > GameLogic.ScreenHeight * 0.5f)
                {
                    bullet.transform.parent = ParentTransform;
                    bullet.SetActive(false);
                    return true;
                }
                break;
            case Type.Explosive:
            case Type.Ice:
                position = bullet.transform.position;
                Vector3 scale = bullet.transform.localScale;
                Color color = bullet.GetComponent<CreateMesh>().Material.color;

                Inflation += GameLogic.GameDeltaTime;
                position.y += GameLogic.GameDeltaTime * GameLogic.BulletSpeed;
                scale = Vector3.Lerp(scale, new Vector3(GameLogic.ScreenHeight, GameLogic.ScreenHeight, GameLogic.ScreenHeight)*5f, Inflation);
                color = Color.Lerp(color, Color.clear, Inflation);

                bullet.transform.position = position;
                bullet.transform.localScale = scale;
                bullet.GetComponent<CreateMesh>().Material.color = color;
                if (color == Color.clear)
                {
                    bullet.transform.parent = ParentTransform;
                    bullet.SetActive(false);
                    Weapon.BulletMaterials[(int)mType].color = Weapon.BulletColors[(int)mType];
                    bullet.transform.localScale = new Vector3(Weapon.BulletScale, Weapon.BulletScale, Weapon.BulletScale);
                    return true;
                }
                break;
        }
	    
        return false;
    }

    //Returns true if the bullet is active and has hit the enemy
    public bool CheckHit(Vector3 EnemyPosition, float TouchingDistance, bool EnemyIsBoss) {
        if (bullet.activeInHierarchy) {
            float diffToBullet;

            if (mType == Type.Explosive && bullet.GetComponent<CreateMesh>().Material.color.a<0.5f)
                return true;

            if (mType == Type.Ice)
                return false;

            diffToBullet = (EnemyPosition - bullet.transform.position).sqrMagnitude;
            if (diffToBullet < TouchingDistance)
            {
                if (mType == Type.Normal || (mType == Type.Golden && EnemyIsBoss)) {
                    bullet.SetActive(false);
                }
                return true;
            }
        }
        return false;
    }

    public void SetActive (bool IsActive) {
        bullet.SetActive(IsActive);
    }
}
