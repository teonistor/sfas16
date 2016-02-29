using UnityEngine;
using System.Collections;

/* Star-shaped mesh for powerups
To be done. 
 */
public class CreateMesh2 : MonoBehaviour 
{
	[SerializeField] private Material Mat;
	//[SerializeField] private float Size = 1.0f;

	private MeshRenderer mMeshRenderer;
	private MeshFilter mMesh;

	public Material Material {
		get { return Mat; }
		set { Mat = value; }
	}
	
	private Vector3[] GetVerts() {
		Vector3 [] verts = new Vector3[16];
        //Coordinates have been carefully calculated based on the normal map image using some LibreOffice Calc features

        verts[0] = new Vector3(-0.305f, -0.468f, 0f);
        verts[1] = new Vector3(0.011f, -0.204f, 0f);
        verts[2] = new Vector3(0.287f, -0.497f, 0f);
        verts[3] = new Vector3(0.236f, -0.187f, 0f);
        verts[4] = new Vector3(0.5f, -0.144f, 0f);
        verts[5] = new Vector3(0.247f, 0.014f, 0f);
        verts[6] = new Vector3(0.448f, 0.181f, 0f);
        verts[7] = new Vector3(0.227f, 0.164f, 0f);
        verts[8] = new Vector3(0.394f, 0.483f, 0f);
        verts[9] = new Vector3(0.034f, 0.23f, 0f);
        verts[10] = new Vector3(-0.138f, 0.5f, 0f);
        verts[11] = new Vector3(-0.078f, 0.121f, 0f);
        verts[12] = new Vector3(-0.497f, 0.095f, 0f);
        verts[13] = new Vector3(-0.204f, -0.017f, 0f);
        verts[14] = new Vector3(-0.356f, -0.147f, 0f);
        verts[15] = new Vector3(-0.195f, -0.147f, 0f);


        return verts;
    }

    private Vector2[] GetUVs(Vector3[] verts) {
        Vector2[] uv = new Vector2[16];
        for (int i = 0; i < 16; i++) {
            uv[i] = new Vector2(verts[i].x, verts[i].y);
        }

        return uv;
    }

    private int [] GetTriangles()
	{
		int [] triangles = new int[42];

        triangles[0] = 1;
        triangles[1] = 0;
        triangles[2] = 15;
        triangles[3] = 3;
        triangles[4] = 2;
        triangles[5] = 1;
        triangles[6] = 5;
        triangles[7] = 4;
        triangles[8] = 3;
        triangles[9] = 7;
        triangles[10] = 6;
        triangles[11] = 5;
        triangles[12] = 9;
        triangles[13] = 8;
        triangles[14] = 7;
        triangles[15] = 11;
        triangles[16] = 10;
        triangles[17] = 9;
        triangles[18] = 13;
        triangles[19] = 12;
        triangles[20] = 11;
        triangles[21] = 15;
        triangles[22] = 14;
        triangles[23] = 13;
        triangles[24] = 1;
        triangles[25] = 15;
        triangles[26] = 13;
        triangles[27] = 1;
        triangles[28] = 5;
        triangles[29] = 3;
        triangles[30] = 9;
        triangles[31] = 7;
        triangles[32] = 5;
        triangles[33] = 13;
        triangles[34] = 11;
        triangles[35] = 9;
        triangles[36] = 13;
        triangles[37] = 5;
        triangles[38] = 1;
        triangles[39] = 13;
        triangles[40] = 9;
        triangles[41] = 5;

        return triangles;
	}

    private Vector4[] getTans() {
        Vector4[] tans = new Vector4[16];
        for (int i = 0; i < 16; i++) {
            tans[i] = new Vector4(0, 1, 0, 1);
        }
        return tans;
    }
	
	private Mesh DoCreateMesh()
	{
		Mesh m = new Mesh();
		m.name = "ScriptedMesh2";
		m.vertices = GetVerts(); 
		m.triangles = GetTriangles();
        m.uv = GetUVs(m.vertices);
        m.RecalculateNormals();
        m.tangents = getTans();

		return m;
	}
	
	void Start() 
	{
		mMeshRenderer = gameObject.AddComponent<MeshRenderer>();
		mMesh = gameObject.AddComponent<MeshFilter>();
		mMesh.mesh = DoCreateMesh();
		mMeshRenderer.material = Mat;
    }

    void Update() {
        mMeshRenderer.material = Mat;
    }
}
