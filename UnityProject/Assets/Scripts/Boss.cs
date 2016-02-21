﻿using UnityEngine;

public class Boss {
    
	//[SerializeField] private Camera GameplayCamera;
    //[SerializeField] private Material BossMaterial;
    private float Size = 8f;
    private float activeWidth;
    private Vector3 NextPosition;
    private GameObject TheBoss;
    private int health;

    public Boss(Camera camera, Material material) { 
        TheBoss = new GameObject("Big Boss");
        CreateMesh m = TheBoss.AddComponent<CreateMesh>();
        m.Material = material;
        health = DifficultyCurve.BossStrength;
        activeWidth = GameLogic.ScreenHeight * camera.aspect * 0.4f;
           // activeWidth = 10; //for now
        NextPosition = new Vector3(0f, GameLogic.ScreenHeight * 0.5f, 0f);
        TheBoss.transform.position = NextPosition;
        TheBoss.transform.localScale = new Vector3(Size, Size, Size);
        TheBoss.transform.localRotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
    }
	
	public void Update (Vector3 playerPos) {
        if (System.Math.Abs( TheBoss.transform.position.x - NextPosition.x) < 0.4f) {
            NextPosition = new Vector3(Random.Range(-1f, 1f) * activeWidth, NextPosition.y - 1.5f, 0f);
        }
        else {
            TheBoss.transform.position = Vector3.Lerp(TheBoss.transform.position, NextPosition, GameLogic.GameDeltaTime*2f);
            //rotate
            TheBoss.transform.localRotation = Quaternion.AngleAxis((float)(Mathf.Atan2(playerPos.y - TheBoss.transform.position.y, playerPos.x - TheBoss.transform.position.x) * 180 / System.Math.PI - 90), Vector3.forward);
        }
	}

    public Vector3 Position() {
        return TheBoss.transform.position;
    }

    public bool Invade(Vector3 playerPos) {
        NextPosition = playerPos;
        if ((TheBoss.transform.position - playerPos).sqrMagnitude < 6)
            return true;
        return false;
    }

    //True = boss dead
    public bool Hit (int damage) {
        health -= damage;
        if (NextPosition.y < GameLogic.ScreenHeight * 0.4f)
            NextPosition.y += 3f;
        if (health < 0) {
            Destroy();
            return true;
        }
        return false;
    }
    
    public void Destroy() {
        GameObject.Destroy(TheBoss);
    }
}