using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerupFactory : MonoBehaviour {

    [Range( 1, 10 )] [SerializeField] private int PowerupPoolSize;
    [SerializeField] private float PowerupScale;
    [SerializeField] private float RotationAmount;
    [SerializeField] private float SpeedAmount;
    [SerializeField] private float CollectionDistance;
    [SerializeField] private Material GoldenMaterial;
    [SerializeField] private Material IceMaterial;
    [SerializeField] private Material ExplosiveMaterial;

    public enum Type { Golden, Ice, Explosive, NoPowerups}

    public static PowerupFactory mInstance { get; private set; }
    public Material[] mMaterials { get; private set; }
    private List<GameObject> mActive;
    private List<GameObject> mInactive;
    private Quaternion RotationVector;
    private List<ExplosionItem> ExplosionItems;
    //private const int MaterialCount = 3;

    void Awake () {
        if (mInstance == null)
        {
            mInstance = this;
            mMaterials = new Material[(int)Type.NoPowerups];
            mMaterials[(int)Type.Golden] = GoldenMaterial;
            mMaterials[(int)Type.Ice] = IceMaterial;
            mMaterials[(int)Type.Explosive] = ExplosiveMaterial;

            RotationVector = Quaternion.AngleAxis(RotationAmount, Vector3.forward);
            ExplosionItems = new List<ExplosionItem>();
            
            mActive = new List<GameObject>();
            mInactive = new List<GameObject>();
            for (int i = 0; i < PowerupPoolSize; i++) {
                GameObject ThisPowerup = new GameObject("Powerup_Pool_ID_" + (i + 1));
                ThisPowerup.AddComponent<CreateMesh2>();
                ThisPowerup.transform.localScale = new Vector3(PowerupScale, PowerupScale, PowerupScale);
                ThisPowerup.transform.parent = transform;
                ThisPowerup.SetActive(false);
                mInactive.Add(ThisPowerup);
            }
        }
        else
        {
            Debug.LogError("Only one PowerupFactory allowed! Destorying duplicate.");
            Destroy(this.gameObject);
        }
    }

    void Update() {
        //Take care of currently falling items after the explosion
        
        foreach (ExplosionItem ThisItem in ExplosionItems) {
            if (ThisItem.Update()) {
                Return(ThisItem.Powerup);
            }
        }
    }

    public static int DetectCollisions (Vector3 PlayerPosition) {
        //non-statically call method to detect collisions
        if (mInstance != null) {
            return mInstance.DetectCollisionsOnInstance(PlayerPosition);
        }
        return -1;
    }

    private int DetectCollisionsOnInstance(Vector3 PlayerPosition) {
        foreach (GameObject ThisPowerup in mActive) {
            if (ThisPowerup.activeInHierarchy) {
                Vector3 distance = PlayerPosition - ThisPowerup.transform.position;
                if (distance.sqrMagnitude < CollectionDistance) {
                    Material material = ThisPowerup.GetComponent<CreateMesh2>().Material;
                    ThisPowerup.SetActive(false);
                    for (int i= 0;i<(int)Type.NoPowerups; i++) {
                        if (material == mMaterials[i]) return i;
                    }
                }
            }
        }
        return -1;
    }

    public static void ProduceExplosion (Vector3 SourcePosition) {
        if (mInstance != null) {
            mInstance.ProduceExplosionOnInstance(SourcePosition);
        }
    }

    private void ProduceExplosionOnInstance (Vector3 SourcePosition) {
        int available = mInactive.Count;
        ExplosionItems.Clear();
        for (int i = 0; i < available; i++)
        {
            GameObject NewItem = Dispatch((Type)Random.Range(0, (int)Type.NoPowerups), SourcePosition);
            Vector3 NewSpeed = new Vector3(Random.Range(-SpeedAmount, SpeedAmount), Random.Range(-SpeedAmount, 0f), 0f);
            ExplosionItems.Add(new ExplosionItem(NewItem, NewSpeed));
        }
    }

	public GameObject Dispatch(Type ThisType, Vector3 InitialPosition) {
        if (mInactive.Count==0) return null;
        GameObject ThisPowerup = mInactive[mInactive.Count - 1];
        ThisPowerup.GetComponent<CreateMesh2>().Material = mMaterials[(int)ThisType];
        ThisPowerup.SetActive(true);
        ThisPowerup.transform.parent = null;
        ThisPowerup.transform.position = InitialPosition;
        mInactive.Remove(ThisPowerup);
        mActive.Add(ThisPowerup);
        return ThisPowerup;
    }

    public void Return (GameObject ThisPowerup) {
        if (mActive.Remove(ThisPowerup))
        {
            ThisPowerup.SetActive(false);
            mInactive.Add(ThisPowerup);
            ThisPowerup.transform.parent = transform;
        }
    }

    public static void Reset() {
        if (mInstance != null) {
            for (int count = 0; count < mInstance.mActive.Count; count++) {
                mInstance.mActive[count].SetActive(false);
                mInstance.mActive[count].transform.parent = mInstance.transform;
                mInstance.mInactive.Add(mInstance.mActive[count]);
            }

            mInstance.mActive.Clear();
        }
    }

    private class ExplosionItem {
        internal Vector3 speed { get; private set;}
        internal GameObject Powerup { get; private set;}

        internal ExplosionItem (GameObject Powerup, Vector3 speed) {
            this.speed = speed;
            this.Powerup = Powerup;
        }

        internal bool Update() {
            Powerup.transform.localRotation *= PowerupFactory.mInstance.RotationVector;
            Powerup.transform.position += speed * GameLogic.GameDeltaTime;
            speed = Vector3.Lerp(speed, Vector3.down*PowerupFactory.mInstance.SpeedAmount, GameLogic.GameDeltaTime);

            if (Powerup.transform.position.y < GameLogic.ScreenHeight * -0.5f)
                return true;
            return false;
        }
    }
}
