using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class ProceduralMesh : MonoBehaviour {

	public float checkEvery = 2f;

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
			RefreshMesh();

		}
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


















