using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OsscilateGrid : MonoBehaviour {

  public float Speed;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += Vector3.up * Mathf.Sin( Time.time * Speed ) * .01f;
	}


}
