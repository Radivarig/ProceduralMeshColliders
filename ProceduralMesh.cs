using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ProceduralMesh : MonoBehaviour {

	public enum Types{ 
		Plane,
		Prism,
		Dome,
	}
	//public Types ;
	public Types type = Types.Prism;

	public float checkEvery = 0.2f;

	//base
	[Range(3, 32)] public int baseNumber = 3;

	public List<PairFloat> floorValues = new List<PairFloat>();

	//mesh
	private Mesh mesh;
	private MeshCollider mc;

	void Start () {
		InitMesh();
		//gameObject.AddComponent<MeshRenderer>();
		mc = gameObject.GetComponent<MeshCollider>();
		if (mc ==null) mc = gameObject.AddComponent<MeshCollider>();
		mc.sharedMesh = mesh;
		if(floorValues.Count == 0) floorValues.Add(new PairFloat());
	}

	void InitMesh(){
		mesh = new Mesh();
		mesh.name = "procedural mesh";
	}

	void Update () {
		if (TrueEverySeconds(checkEvery) && InspectorChanged()){
			if (floorValues.Count != 0){
				if (mc ==null){
					mc = gameObject.AddComponent<MeshCollider>();
					mc.sharedMesh = mesh;
				}

			 	if(type == Types.Prism) MakePrism();
				else mesh.Clear();
			}

			mc.enabled = false;
			mc.enabled = true;
		}
	}
	
	public void MakePrism(){
		List<Vector3> vertices = new List<Vector3>();

		foreach(PairFloat floor in floorValues){
			vertices.AddRange(BaseVertices(baseNumber, floor.radius, floor.position.y));
		}

		List<int> tris = MakeTrianglesWithNextAndUp(vertices.Count/floorValues.Count);

		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();
	}

	public List<int> MakeTrianglesWithNextAndUp(int floorCount){
		List<int> tris = new List<int>();
		//int floorCount = floorValues[0].vertices.Count;
		for (int i = 0; i < floorCount -1 ; i++){
			for(int j = 0; j < floorValues.Count-1; j++){
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
		for(int j = 0; j < floorValues.Count-1; j++){
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

			int k = floorCount*(floorValues.Count-1);
			tris.Add(i +k +1);
			tris.Add(i +k);
			tris.Add(0 +k);
		}

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

	//reversing triangles array might also work
	/*void FlipNormals(Mesh mesh){
		List<int> triangles = new List<int>();
		triangles.AddRange(mesh.triangles);

		for(int i = 1; i < triangles.Count -1; i++){
			int temp = triangles[i-1];
			triangles[i-1] = triangles[i+1];
			triangles[i+1] = temp;
			i += 2;
		}
		mesh.triangles = triangles.ToArray();
	}*/
}

[System.Serializable]
public class PairFloat{
	public Vector3 position;
	[Range(0, 3)] public float radius;

	public PairFloat(Vector3 position = default(Vector3), float radius = 1f){
		this.position = position;
		this.radius = radius;
	}
}