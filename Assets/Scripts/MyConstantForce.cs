using UnityEngine;
using System.Collections;

public class MyConstantForce : MonoBehaviour {
	public Vector3 Force;
	public ForceMode Mode = ForceMode.Force;
	
	// Update is called once per frame
	void FixedUpdate () {
		rigidbody.AddForce ( Force, Mode );
	}
}
