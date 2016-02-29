using UnityEngine;

/* Arrow-shaped mesh for player, enemies and background scenery
 */
public class CreateMesh : MonoBehaviour 
{
	[SerializeField] private Material Mat;

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

    private Vector4[] GetTans() {
        Vector4[] tan = new Vector4[7];

        tan[0] = new Vector4(0f, 1f, 0f, 1f);
        tan[1] = new Vector4(0f, 1f, 0f, 1f);
        tan[2] = new Vector4(0f, 1f, 0f, 1f);
        tan[3] = new Vector4(0f, 1f, 0f, 1f);
        tan[4] = new Vector4(0f, 1f, 0f, 1f);
        tan[5] = new Vector4(0f, 1f, 0f, 1f);
        tan[6] = new Vector4(0f, 1f, 0f, 1f);

        return tan;
    }

    private int [] GetTriangles()
	{
		int [] triangles = new int[9];
		
		triangles[0] = 0;
		triangles[1] = 2;
		triangles[2] = 1;
		triangles[3] = 3;
		triangles[4] = 4;
		triangles[5] = 5;
		triangles[6] = 4;
		triangles[7] = 6;
		triangles[8] = 5;

		return triangles;
	}
	
	private Mesh DoCreateMesh()
	{
		Mesh m = new Mesh();
		m.name = "ScriptedMesh";
		m.vertices = GetVerts(); 
		m.triangles = GetTriangles();
        m.uv = GetUVs();
        m.RecalculateNormals();
        m.tangents = GetTans();
		
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
