using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class ProceduralMesh : MonoBehaviour {

	public float checkEvery = 2f;

	//base
	[Range(3, 32)] public int baseNumber;
	[Range(0.01f, 10f)]public float radius = 1f;

	//mesh
	private MeshFilter mf;
	private Mesh mesh;

	public Vector3[] vertices;
	public Vector2[] uv;
	public int[] triangles;

	void Start () {
		InitMesh();
		gameObject.AddComponent<MeshRenderer>();
	}

	void InitMesh(){
		mf = gameObject.GetComponent<MeshFilter>();
		if (mf ==null){
			mf = gameObject.AddComponent<MeshFilter>();
		}
		mesh = mf.mesh;
		if(mesh ==null){
			mesh = new Mesh();
			mf.mesh = mesh;
		}
	}

	void Update () {
		if (TrueEverySeconds(checkEvery) && InspectorChanged()){
			CreateMesh();
			ConnectVerticesWithCenter();
			RefreshMesh();

		}
	}

	void ConnectVerticesWithCenter(){
		List<int> tris = new List<int>();
		for (int i = 1; i < vertices.Length -1; i++){
			tris.Add(0);
			tris.Add(i);
			tris.Add(i+1);
		}
		tris.Add(0);
		tris.Add(vertices.Length-1);
		tris.Add(1);

		triangles = tris.ToArray();
	}

	void CreateMesh(){
		List<Vector3> verts = new List<Vector3>();
		//center
		verts.Add(Vector3.zero);

		for(int i = 0; i < baseNumber; i++){
			float radians = i * 360f/baseNumber * Mathf.Deg2Rad;
			float x = Mathf.Cos(radians)*radius;
			float z = Mathf.Sin(radians)*radius;
			verts.Add(new Vector3 (x, 0f, z));
		}

		vertices = verts.ToArray();
	}

	//variable needed only for TrueEverySeconds
	private float time = 0f;

	bool TrueEverySeconds(float sec, int decimals = 1){
		//time init
		if (time == 0f) time = Time.time;

		float difference = Time.time - time;
		if (difference >= sec || difference == 0f){
			float tmp = Mathf.Pow(10, decimals);
			time = Mathf.Round(Time.time*tmp)/tmp;
			//Debug.Log("time: " + Time.time);
				
			return true;
		}
		return false;
	}

	bool InspectorChanged(){
		return true;
	}
	
	public void RefreshMesh(){
		mesh.Clear();

		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
	}
}