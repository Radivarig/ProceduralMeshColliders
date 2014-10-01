using UnityEngine;
using System.Collections;

public class ProceduralMesh : MonoBehaviour {

	public float checkEvery = 2f;
	
	void Start () {

	}

	void Update () {
		if (TrueEverySeconds(checkEvery) && InspectorChanged()){

		}
	}


	//variables needed only for TrueEverySeconds
	private float time;
	bool TrueEverySeconds(float sec, int decimals = 1){
		//time init
		if (time ==null) time = Time.time;

		if (Time.time - time > sec){
			float tmp = Mathf.Pow(10, decimals);
			time = Mathf.Round(Time.time*tmp)/tmp;
			Debug.Log("time: " + Time.time);
				
			return true;
		}
		return false;
	}

	bool InspectorChanged(){
		return true;
	}
}


















