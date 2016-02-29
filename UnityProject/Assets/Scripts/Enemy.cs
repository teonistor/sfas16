using UnityEngine;

public class Enemy {

    private GameObject enemy;
    public Vector3 position { get { return enemy.transform.position; } private set { enemy.transform.position = value; } }

	public Enemy (Material material, int count) {
        enemy = new GameObject("Enemy_PoolID" + (count + 1));
        CreateMesh m = enemy.AddComponent<CreateMesh>();
        m.Material = material;
        enemy.transform.localRotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
        enemy.SetActive(false);
    }

    public void SetActive (bool isActive) {
        enemy.SetActive(isActive);
    }
}
