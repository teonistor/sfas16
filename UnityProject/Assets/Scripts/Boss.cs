using UnityEngine;

public class Boss {
    
    private float Size = 8f;
    private float activeWidth;
    private float MoveTime;
    private Vector3 PrevPosition, NextPosition;
    private GameObject TheBoss;
    private int health;

    public Boss(Camera camera, Material material) { 
        TheBoss = new GameObject("Big Boss");
        CreateMesh m = TheBoss.AddComponent<CreateMesh>();
        m.Material = material;
        health = DifficultyCurve.BossStrength;
        activeWidth = GameLogic.ScreenHeight * camera.aspect * 0.4f;
        NextPosition = new Vector3(0f, GameLogic.ScreenHeight * 0.5f, 0f);
        TheBoss.transform.position = new Vector3(0f, GameLogic.ScreenHeight * 0.6f, 0f);
        TheBoss.transform.localScale = new Vector3(Size, Size, Size);
        TheBoss.transform.localRotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
    }
	
	public void Update (Vector3 playerPos) {
        if (System.Math.Abs( TheBoss.transform.position.x - NextPosition.x) < 0.4f) {
            //If destination has practically been touched, choose new destination

            //If boss is very close to the bottom, eat the player
            if (TheBoss.transform.position.y < GameLogic.ScreenHeight * -0.25f) {
                NextPosition = playerPos;
            }
            //Otherwise, just pick a random destination
            else {
                NextPosition = new Vector3(Random.Range(-1f, 1f) * activeWidth, NextPosition.y - 1.5f, 0f);
            }
            
            PrevPosition = TheBoss.transform.position;
            MoveTime = 0;
        }
        else {
            //move
            MoveTime += GameLogic.GameDeltaTime * 0.75f;
            TheBoss.transform.position = Vector3.Lerp(PrevPosition, NextPosition, MoveTime);
            //rotate
            TheBoss.transform.localRotation = Quaternion.AngleAxis((float)(Mathf.Atan2(playerPos.y - TheBoss.transform.position.y, playerPos.x - TheBoss.transform.position.x) * 180 / System.Math.PI - 90), Vector3.forward);
        }
	}

    public Vector3 Position() {
        return TheBoss.transform.position;
    }

    public bool HasEaten(Vector3 playerPos) {
        if ((TheBoss.transform.position - playerPos).sqrMagnitude < 5f)
            return true;
        return false;
    }

    //True = boss dead
    public bool Hit (int damage) {
        health -= damage;
        if (NextPosition.y < GameLogic.ScreenHeight * 0.4f) {
            NextPosition.y += 3f;
            PrevPosition = TheBoss.transform.position;
            MoveTime = 0f;
        }
        if (health < 0) {
            PowerupFactory.ProduceExplosion(TheBoss.transform.position);
            Destroy();
            return true;
        }
        return false;
    }
    
    public void Destroy() {
        GameObject.Destroy(TheBoss);
    }
}
