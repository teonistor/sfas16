using UnityEngine;
using System.Collections;

public class CreateMesh : MonoBehaviour 
{
	[SerializeField] private Material Mat;
	//[SerializeField] private float Size = 1.0f;

	private MeshRenderer mMeshRenderer;
	private MeshFilter mMesh;

	public Material Material {
		get { return Mat; }
		set { Mat = value; }
	}
	
	private Vector3 [] GetVerts() {
		Vector3 [] verts = new Vector3[7];
        //Dimensions are within a 1x1 square
		float wide = 0.5f;
		float narrow = 0.2f;
		
		verts[0] = new Vector3( 0.0f, wide, 0.0f );
		verts[1] = new Vector3( -wide, 0.0f, 0.0f );
		verts[2] = new Vector3( wide, 0.0f, 0.0f );
		verts[3] = new Vector3( -narrow, 0.0f, 0.0f );
		verts[4] = new Vector3( narrow, 0.0f, 0.0f );
		verts[5] = new Vector3( -narrow, -wide, 0.0f );
		verts[6] = new Vector3( narrow, -wide, 0.0f );

		return verts;
    }

    private Vector2[] GetUVs() {
        Vector2[] uv = new Vector2[7];
        float wide = 0.5f;
        float narrow = 0.2f;

        uv[0] = new Vector2(0.0f, wide);
        uv[1] = new Vector2(-wide, 0.0f);
        uv[2] = new Vector2(wide, 0.0f);
        uv[3] = new Vector2(-narrow, 0.0f);
        uv[4] = new Vector2(narrow, 0.0f);
        uv[5] = new Vector2(-narrow, -wide);
        uv[6] = new Vector2(narrow, -wide);

        return uv;
    }

    private int [] GetTriangles()
	{
		int [] starTriangles = new int[9];
		
		starTriangles[0] = 0;
		starTriangles[1] = 2;
		starTriangles[2] = 1;
		starTriangles[3] = 3;
		starTriangles[4] = 4;
		starTriangles[5] = 5;
		starTriangles[6] = 4;
		starTriangles[7] = 6;
		starTriangles[8] = 5;

		return starTriangles;
	}
	
	private Mesh DoCreateMesh()
	{
		Mesh m = new Mesh();
		m.name = "ScriptedMesh";
		m.vertices = GetVerts(); 
		m.triangles = GetTriangles();
        m.uv = GetUVs();
        m.RecalculateNormals();
		
		return m;
	}
	
	void Start() 
	{
		mMeshRenderer = gameObject.AddComponent<MeshRenderer>();
		mMesh = gameObject.AddComponent<MeshFilter>();
		mMesh.mesh = DoCreateMesh();
		mMeshRenderer.material = Mat;
	}
}
