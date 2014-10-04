﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

//TODO scale radius by piOffset
//TODO global pivot
//TODO proper uv unwrap
//TODO remove doubles for non-rendered collider-only mesh when floor radius is 0
[ExecuteInEditMode]
public class ProceduralMesh : MonoBehaviour {
	public bool renderMesh = true;
	public bool exportToObj = false;
	public enum Types{ 
		Plane,
		Prism,
		Dome,
	}public Types type = Types.Prism;

	private float checkEvery = 0.2f;

	//base
	[Range(3, 32)] public int baseNumber = 3;
	[Range(1, 64)] public int rows = 1;
	[Range(1, 64)] public int columns = 1;
	[Range(0, 2)] public float piOffset = 0.25f;
	//public bool useSharpEdgedEnds = true;

	public List<PairFloat> floorValues = new List<PairFloat>();

	//mesh
	private Mesh mesh;
	private MeshFilter mf;
	private MeshRenderer mr;
	private MeshCollider mc;

	void Update () {
		if(exportToObj) {
			HandleExport();
		}

		if (TrueEverySeconds(checkEvery) && InspectorChanged())
		{
			if(mesh ==null) InitMesh();
			if(floorValues.Count == 0) floorValues.Add(new PairFloat());

			//get mesh collider
			if (mc ==null) GetMeshCollider();
			//get mesh filter and mesh renderer
			if(renderMesh){
				if(mf ==null) GetMeshFilter();
				if(mr ==null) GetMeshRenderer();
			}
			else if (mf !=null){
				DestroyImmediate(mr);
				DestroyImmediate(mf);
			}
			
		 	if(type == Types.Prism) MakePrism();
			else if (type == Types.Plane) MakePlane();
			else mesh.Clear();

			mc.enabled = false;
			mc.enabled = true;
		}
	}
	
	void GetMeshRenderer(){
		mr = gameObject.GetComponent<MeshRenderer>();
		if (mr ==null) {
			mr = gameObject.AddComponent<MeshRenderer>();
			mr.sharedMaterial = new Material(Shader.Find("Diffuse"));;
		}
	}

	void GetMeshFilter(){
		mf = gameObject.GetComponent<MeshFilter>();
		if (mf ==null) mf = gameObject.AddComponent<MeshFilter>();
		mf.sharedMesh = mesh;
	}

	void GetMeshCollider(){
		mc = gameObject.GetComponent<MeshCollider>();
		if (mc ==null) mc = gameObject.AddComponent<MeshCollider>();
		mc.sharedMesh = mesh;
	}

	void HandleExport(){
		if(renderMesh ==false){
			GetMeshFilter();
			GetMeshRenderer();
		}

		string path = "/Abiogenesis/Procedural Primitives and Colliders/Exports/";
		string fullPath = Application.dataPath + path;
		if(Directory.Exists(fullPath) ==false)
			Directory.CreateDirectory(fullPath);
		MeshToFile(mf, Application.dataPath + path + name + ".obj");
		AssetDatabase.Refresh();
		exportToObj = false;	
	}

	void InitMesh(){
		mesh = new Mesh();
		mesh.name = "procedural mesh";
	}

	public void MakePlane(){
		List<Vector3> vertices = new List<Vector3>();
		float radius = floorValues[0].radius;
		for(int i = 0; i <= rows; i++){
			for(int j = 0; j <= columns; j++){
				vertices.Add(new Vector3(j*radius/columns, 0f, i*radius/rows));
			}
		}

		List<int> tris = MakeTrianglesWithNextAndUp(rows, columns);

		ApplyToMesh(vertices, tris);
	}

	public void MakePrism(){
		List<Vector3> vertices = new List<Vector3>();

		//duplicate sharp floors 
		int c = 0;
		List<PairFloat> duplicates = new List<PairFloat>();
		foreach(PairFloat floor in floorValues.ToArray()){
			if(floor.sharpEdge) {
				duplicates.Add(floor);
				floorValues.Insert(++c, floor.CopyThis());
				c++;
			}
		}

		foreach(PairFloat floor in floorValues){
			List<Vector3> baseVerts = BaseVertices(baseNumber, floor.radius, floor.position);
			if (floor.rotation != Vector3.zero){
				for(int i = 0; i < baseVerts.Count; i++){

					baseVerts[i] -= floor.GetPivot();

					Quaternion rotation = Quaternion.Euler(floor.rotation);
					Vector3 newRotation = rotation * baseVerts[i];

					float x = baseVerts[i].x;
					float y = baseVerts[i].y;
					float z = baseVerts[i].z;
					if (floor.freezeAxisX ==false) x = newRotation.x;
					if (floor.freezeAxisY ==false) y = newRotation.y;
					if (floor.freezeAxisZ ==false) z = newRotation.z;

					baseVerts[i] = new Vector3(x, y, z);

					baseVerts[i] += floor.GetPivot();
				}
			}
			vertices.AddRange(baseVerts);
		}

		List<int> tris = MakeTrianglesWithNextAndUpClosed(vertices.Count/floorValues.Count);

		ApplyToMesh(vertices, tris);

		//remove duplicated floors
		foreach(PairFloat dup in duplicates)
			floorValues.Remove(dup);
	}

	public void ApplyToMesh(List<Vector3> verts, List<int> tris){
		mesh.Clear();
		mesh.vertices = verts.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.RecalculateNormals();
		
		mesh.RecalculateBounds();

		Vector2[] uvs = new Vector2[verts.Count];
		for (int i = 0; i < uvs.Length; i++){
			uvs[i] = new Vector2(verts[i].x, verts[i].z);
		}
		mesh.uv = uvs;

		mesh.Optimize();
	}

	public List<int> MakeTrianglesWithNextAndUp(int rowNo, int colNo){
		List<int> tris = new List<int>();

		for (int i = 0; i < rowNo; i++){
			for(int j = 0; j < colNo; j++){

				int k =i*(colNo+1) + j;
				tris.Add(k + colNo + 1);
				tris.Add(k + 1);
				tris.Add(k);
				
				tris.Add(k + 1);
				tris.Add(k + colNo + 1);
				tris.Add(k + colNo + 2);
			}
		}
		return tris;
	}

	public List<int> MakeTrianglesWithNextAndUpClosed(int floorCount){
		List<int> tris = new List<int>();
		for (int i = 0; i < floorCount -1; i++){
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
			int k = floorCount*(floorValues.Count-1);
			tris.Add(i +k +1);
			tris.Add(i +k);
			tris.Add(0 +k);

			if(floorValues.Count == 1) continue;
			tris.Add(0);
			tris.Add(i);	
			tris.Add(i +1);	
		}
		return tris;
	}
	
	List<Vector3> BaseVertices(int baseNumber, float radius = 1f, Vector3 position = default(Vector3)){
		List<Vector3> verts = new List<Vector3>();

		for(int i = 0; i < baseNumber; i++){
			float radians = i * 360f/baseNumber * Mathf.Deg2Rad;
			float offset = Mathf.PI*piOffset;
			float x = Mathf.Cos(radians + offset)*radius;
			float z = Mathf.Sin(radians + offset)*radius;
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
	/* FlipNormals(Mesh mesh){
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

	public static string MeshToString(MeshFilter mf) {
		Mesh m = mf.sharedMesh;

		StringBuilder sb = new StringBuilder();
		
		sb.Append("g ").Append(mf.name).Append("\n");
		foreach(Vector3 v in m.vertices) {
			sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,v.z));
		}
		sb.Append("\n");
		foreach(Vector3 v in m.normals) {
			sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
		}
		sb.Append("\n");
		foreach(Vector3 v in m.uv) {
			sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
		}

		int[] triangles = m.GetTriangles(0);
		for (int i=0;i<triangles.Length;i+=3) {
			sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
			                        triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));

		}
		return sb.ToString();
	}
	
	public static void MeshToFile(MeshFilter mf, string filename) {
		using (StreamWriter sw = new StreamWriter(filename)) {
			sw.Write(MeshToString(mf));
		}
	}
}

[System.Serializable]
public class PairFloat{
	public enum Pivot{Center, Left, Right, Front, Back,}
	public Pivot pivotType;
	public bool sharpEdge;
	public Vector3 position;
	[Range(0, 3)] public float radius;
	public Vector3 rotation;
	public bool freezeAxisX;
	public bool freezeAxisY;
	public bool freezeAxisZ;

	public PairFloat(Pivot pivotType = Pivot.Center, bool sharpEdge = false, float radius = 1f,
	                 Vector3 position = default(Vector3), Vector3 rotation = default(Vector3),
	                 bool freezeX = false, bool freezeY = false, bool freezeZ = false){
		this.pivotType = pivotType;
		this.sharpEdge= sharpEdge;
		this.radius = radius;
		this.position = position;
		this.rotation = rotation;
		this.freezeAxisX = freezeX;
		this.freezeAxisY = freezeY;
		this.freezeAxisZ = freezeZ;
	}

	public PairFloat CopyFrom(PairFloat target){
		return new PairFloat(target.pivotType, target.sharpEdge, target.radius,
		                     target.position, target.rotation, 
		                     target.freezeAxisX, target.freezeAxisY, target.freezeAxisZ);
	}

	public PairFloat CopyThis(){
		return CopyFrom(this);
	}

	public Vector3 GetPivot(){
		float distanceToEdge = 0.5f*Mathf.Sqrt(2f)*radius;
	switch(pivotType){
		case Pivot.Center: return new Vector3(0f, 0f, 0f) + position;
		case Pivot.Left: return new Vector3(-distanceToEdge, 0f, 0f) + position;
		case Pivot.Right: return new Vector3(distanceToEdge, 0f, 0f) + position;
		case Pivot.Front: return new Vector3(0f, 0f, distanceToEdge) + position;
		case Pivot.Back: return new Vector3(0f, 0f, -distanceToEdge) + position;
		}
		return Vector3.zero;
	}
}