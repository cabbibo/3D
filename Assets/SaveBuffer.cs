using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveBuffer : MonoBehaviour {


	// Use this for initialization
	void Start () {
		
	}

  public void Save( float[] val , string name ){
    BinaryFormatter bf = new BinaryFormatter();
    FileStream stream = new FileStream(Application.dataPath + "/"+name+".sdf",FileMode.Create);
    bf.Serialize(stream,val);
    stream.Close();
  }

  public float[] Load(string name){

    if( File.Exists(Application.dataPath + "/"+name+".sdf")){
       BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.dataPath + "/"+name+".sdf",FileMode.Open);

        float[] data = bf.Deserialize(stream) as float[];

        stream.Close();
        return data;
    }else{
      return new float[1];
    }


  }
	
	// Update is called once per frame
	void Update () {
		
	}
}
