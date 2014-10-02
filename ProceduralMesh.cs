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

	private float checkEvery = 0.2f;

	//base
	[Range(3, 32)] public int baseNumber = 3;
	[Range(1, 64)] public int rows = 1;
	[Range(1, 64)] public int columns = 1;
	[Range(0.1f, 3f)] public float unit = 1;
	[Range(0, 2)] public float piOffset = 0.25f;


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
				else if (type == Types.Plane) MakePlane();
				else mesh.Clear();
			}

			mc.enabled = false;
			mc.enabled = true;
		}
	}

	public void MakePlane(){
		List<Vector3> vertices = new List<Vector3>();
		
		for(int i = 0; i <= rows; i++){
			for(int j = 0; j <= columns; j++){
				vertices.Add(new Vector3(j*unit, 0f, i* unit));
			}
		}

		List<int> tris = MakeTrianglesWithNextAndUp(rows, columns);
		
		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();
	}

	public void MakePrism(){
		List<Vector3> vertices = new List<Vector3>();

		foreach(PairFloat floor in floorValues){
			List<Vector3> baseVerts = BaseVertices(baseNumber, floor.radius, floor.position);
			if (floor.rotation != Vector3.zero){
				for(int i = 0; i < baseVerts.Count; i++){
					Quaternion rotation = Quaternion.Euler(floor.rotation);
					Vector3 newRotation = rotation * baseVerts[i];

					float x = baseVerts[i].x;
					float y = baseVerts[i].y;
					float z = baseVerts[i].z;
					if (floor.freezeAxis.x == 0f) x = newRotation.x;
					if (floor.freezeAxis.y == 0f) y = newRotation.y;
					if (floor.freezeAxis.z == 0f) z = newRotation.z;
					
					baseVerts[i] = new Vector3(x, y, z);
				}
			}
			vertices.AddRange(baseVerts);
		}

		List<int> tris = MakeTrianglesWithNextAndUpClosed(vertices.Count/floorValues.Count);

		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();
	}

	public List<int> MakeTrianglesWithNextAndUp(int rowNo, int colNo){
		List<int> tris = new List<int>();

		for (int i = 0; i < colNo; i++){
			for(int j = 0; j < rowNo; j++){
				int k = j*colNo + i;
				tris.Add(k + colNo);
				tris.Add(k + 1);
				tris.Add(k);
				
				tris.Add(k + colNo);
				tris.Add(k + colNo + 1);
				tris.Add(k + 1);
			}
		}
		return tris;
	}

	public List<int> MakeTrianglesWithNextAndUpClosed(int floorCount){
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
	
	List<Vector3> BaseVertices(int baseNumber, float radius = 1f, Vector3 position = default(Vector3)){
		List<Vector3> verts = new List<Vector3>();

		for(int i = 0; i < baseNumber; i++){
			float radians = i * 360f/baseNumber * Mathf.Deg2Rad;
			float x = Mathf.Cos(radians +Mathf.PI*piOffset)*radius;
			float z = Mathf.Sin(radians +Mathf.PI*piOffset)*radius;
			verts.Add(new Vector3 (x + position.x, position.y, z + position.z));
		}
	
		return verts;
	}

	//variable needed only for TrueEverySeconds
	private float time = 0f;
	//TODO EditorApplication.timeSinceStartup for edit mode
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
	public Vector3 rotation;
	public Vector3 freezeAxis;
	[Range(0, 3)] public float radius;

	public PairFloat(Vector3 position = default(Vector3), float radius = 1f){
		this.position = position;
		this.radius = radius;
	}
}