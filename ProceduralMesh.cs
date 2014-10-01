using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ProceduralMesh : MonoBehaviour {

	public enum Types{
		Ngon,
		Piramid,
		Prism,
		Dome,
	}
	//public Types ;
	public Types type = Types.Prism;



	public float checkEvery = 0.2f;

	//base
	[Range(3, 32)] public int baseNumber = 3;
	[Range(1, 5)] public int floorNumber = 2;

	public List<PairFloat> floorValues = new List<PairFloat>();

	//mesh
	private MeshFilter mf;
	private Mesh mesh;
	private MeshCollider mc;

	/*public Vector3[] vertices;
	public int[] triangles;*/

	void Start () {
		InitMesh();
		//gameObject.AddComponent<MeshRenderer>();
		mc = gameObject.AddComponent<MeshCollider>();
	}

	void InitMesh(){
		mf = gameObject.GetComponent<MeshFilter>();
		if (mf ==null){
			mf = gameObject.AddComponent<MeshFilter>();
		}
		mesh = mf.sharedMesh;
		if(mesh ==null){
			mesh = new Mesh();
			mesh.name = "procedural mesh";
			mf.sharedMesh = mesh;
		}
	}

	void Update () {
		if (TrueEverySeconds(checkEvery) && InspectorChanged()){
			while (floorValues.Count < floorNumber)
				floorValues.Add(new PairFloat());
			while (floorValues.Count > floorNumber)
				floorValues.RemoveAt(floorValues.Count-1);

		 	if(type == Types.Ngon) MakeNgon(true);
		 	else if(type == Types.Prism) MakePrism();
			else mesh.Clear();

			//mc.sharedMesh = mesh;
			mc.enabled = false;
			mc.enabled = true;
		}
	}
	
	public void MakePrism(){
		List<Vector3> vertices = new List<Vector3>();

		foreach(PairFloat floor in floorValues){
			vertices.AddRange(BaseVertices(baseNumber, floor.radius, floor.height));
		}

		List<int> tris = MakeTrianglesWithNextAndUp(vertices.Count/floorNumber);

		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();
	}

	public List<int> MakeTrianglesWithNextAndUp(int floorCount){
		List<int> tris = new List<int>();
		//int floorCount = floorValues[0].vertices.Count;
		for (int i = 0; i < floorCount -1 ; i++){
			for(int j = 0; j < floorNumber-1; j++){
				int k = j*floorCount + i;
				tris.Add(k + floorCount);
				tris.Add(k + 1);
				tris.Add(k);
				
				tris.Add(k + floorCount);
				tris.Add(k + floorCount + 1);
				tris.Add(k + 1);
			}
		}
		//connect last with first
		for(int j = 0; j < floorNumber-1; j++){
			int k = j*floorCount + floorCount -1;
			tris.Add(k + floorCount);	//last up
			tris.Add(k -floorCount +1);	//first
			tris.Add(k);	//last
			
			tris.Add(k + 1);	//first up //k -floorCount +1 - floorCount
			tris.Add(k - floorCount +1);
			tris.Add(k + floorCount);
		}

		//close top and bottom 
		for (int i = 0; i < floorCount -1; i++){
			tris.Add(0);
			tris.Add(i);	
			tris.Add(i +1);	

			int k = floorCount*(floorNumber-1);
			tris.Add(i +k +1);
			tris.Add(i +k);
			tris.Add(0 +k);
		}

		return tris;
	}

	public void MakeNgon(bool flipNormals = false){
		List<Vector3> baseVerts = BaseVertices(baseNumber, floorValues[0].radius, floorValues[0].height);

		baseVerts.Add(new Vector3(0f, floorValues[0].height, 0f));	//center

		List<int> tris = MakeTrianglesWithCenter(baseVerts);

		mesh.Clear();
		
		mesh.vertices = baseVerts.ToArray();
		mesh.triangles = tris.ToArray();

		if(flipNormals) FlipNormals(mesh);

	}

	//TODO maybe add flip, not important
	List<int> MakeTrianglesWithCenter(List<Vector3> baseVerts, float height = 0f){
		int count = baseVerts.Count;

		List<int> tris = new List<int>();
		for (int i = 0; i < count -1; i++){
			tris.Add(count -1);
			tris.Add(i);
			tris.Add(i+1);
		}
		tris.Add(count -2);
		tris.Add(0);
		tris.Add(count -1);

		return tris;
	}

	List<Vector3> BaseVertices(int baseNumber, float radius = 1f, float heigth = 0f){
		List<Vector3> verts = new List<Vector3>();

		for(int i = 0; i < baseNumber; i++){
			float radians = i * 360f/baseNumber * Mathf.Deg2Rad;
			float x = Mathf.Cos(radians)*radius;
			float z = Mathf.Sin(radians)*radius;
			verts.Add(new Vector3 (x, heigth, z));
		}
	
		return verts;
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
	
	/*public void RefreshMesh(){
		mesh.Clear();

		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
	}*/

	void FlipNormals(Mesh mesh){
		List<int> triangles = new List<int>();
		triangles.AddRange(mesh.triangles);

		for(int i = 1; i < triangles.Count -1; i++){
			int temp = triangles[i-1];
			triangles[i-1] = triangles[i+1];
			triangles[i+1] = temp;
			i += 2;
		}
		mesh.triangles = triangles.ToArray();
	}
}

[System.Serializable]
public class PairFloat{
	[Range(0, 3)] public float radius;
	[Range(0, 4)] public float height;

	public PairFloat(float radius = 1f, float height = 0f){
		this.radius = radius;
		this.height = height;
	}
}