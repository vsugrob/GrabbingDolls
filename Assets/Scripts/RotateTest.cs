using UnityEngine;
using System.Collections;

public class RotateTest : MonoBehaviour {
	public Vector3 Angles;
	
	void Update () {
		transform.rotation = Quaternion.Euler ( Angles );

		// Anti-Euler order (yxz).
		//transform.rotation =
		//	Quaternion.AngleAxis ( Angles.z, Vector3.forward ) *
		//	Quaternion.AngleAxis ( Angles.x, Vector3.right ) *
		//	Quaternion.AngleAxis ( Angles.y, Vector3.up ) *
		//	Quaternion.identity
		//	;

		// Euler order (zxy).
		//transform.rotation =
		//	Quaternion.AngleAxis ( Angles.y, Vector3.up ) *
		//	Quaternion.AngleAxis ( Angles.x, Vector3.right ) *
		//	Quaternion.AngleAxis ( Angles.z, Vector3.forward ) *
		//	Quaternion.identity
		//	;
	}
}
