using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distribute : MonoBehaviour {

	public GameObject meshPrefab;
	public int maxNumberOfMeshes = 100;

	void Start () {
		int[] triangles = GetComponent<MeshFilter>().sharedMesh.triangles;
		Vector3[] vertices = GetComponent<MeshFilter>().sharedMesh.vertices;

		float totalArea = 0;
		for (int i = 0; i < triangles.Length; i += 3) {
			Vector3 a = vertices [triangles [i + 0]];
			Vector3 b = vertices [triangles [i + 1]];
			Vector3 c = vertices [triangles [i + 2]];
		
			totalArea += (Vector3.Cross (b - a, c - a) * 0.5f).magnitude;
		}

		totalArea *= transform.localScale.x;
	
		float numberOfMeshesCreated = 0;
		float increasedChanceForNextTriangle = 0f;

		for (int i = 0; i < triangles.Length; i += 3) {
			Vector3 a = vertices [triangles [i + 0]];
			Vector3 b = vertices [triangles [i + 1]];
			Vector3 c = vertices [triangles [i + 2]];

			float area = (Vector3.Cross (b - a, c - a) * 0.5f).magnitude * transform.localScale.x;

			float maxNumberOfMeshesOnThisTriangle = area * maxNumberOfMeshes / totalArea;
			float numberOfMeshesCreatedOnThisTriangle = 0;

			while (maxNumberOfMeshesOnThisTriangle - numberOfMeshesCreatedOnThisTriangle > 1) {
				CreateMesh (a, b, c);
				numberOfMeshesCreatedOnThisTriangle++;
			}
		
			float chanceForMesh = maxNumberOfMeshesOnThisTriangle + increasedChanceForNextTriangle - numberOfMeshesCreatedOnThisTriangle;
			if (Random.value < chanceForMesh) {
				CreateMesh (a, b, c);
				numberOfMeshesCreatedOnThisTriangle++;

				increasedChanceForNextTriangle = -chanceForMesh;
			} else {
				increasedChanceForNextTriangle = chanceForMesh;
			}

			numberOfMeshesCreated += (int)numberOfMeshesCreatedOnThisTriangle;
		}

		Debug.Log ("Number of meshes created: " + numberOfMeshesCreated);
	}

	private void CreateMesh(Vector3 a, Vector3 b, Vector3 c) {
		float aa = Random.value;
		float bb = Random.value * (1 - aa);
		float cc = 1 - aa - bb;

		Vector3 position = transform.position + a * aa + b * bb + c * cc;

		GameObject go = Instantiate (meshPrefab);
		go.transform.position = position;
		go.transform.rotation = Random.rotation;
	}
}
