using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Osscilate : MonoBehaviour {

  public Transform target;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    transform.position += Vector3.left * Mathf.Sin( Time.time * 1) * .01f;
    transform.position += Vector3.up * Mathf.Sin( Time.time * 1.3f +2) * .013f;
    transform.position += Vector3.forward * Mathf.Sin( Time.time * 1.5f + 1) * .015f;
    transform.LookAt( target );
		
	}
}
