using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopController : MonoBehaviour{
	public GameObject go;
    void Start(){
    }

    void Update(){
		Vector3 pos = new Vector3 (Random.Range(-5,5),6,Random.Range(-5,5));
        if(Input.GetKeyDown (KeyCode.Space)){
			Instantiate(go,pos,transform.rotation);
		}
    }
}
