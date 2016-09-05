using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent ( typeof ( Camera ) )]
public class MultiActorCamera : MonoBehaviour {
	public string ActorTag = "Player";
	public float Angle;
	public float BaseDistance = 4;
	public float Distance = 5;
	//public float Velocity = 2;
	//public float AngularVelocity = 50;

	void Update () {
		var objects = GameObject.FindGameObjectsWithTag ( ActorTag );

		if ( objects.Length == 0 )
			return;

		var positions = objects
			.Select ( obj => camera.transform.InverseTransformPoint ( obj.transform.position ) )
			.ToArray ();

		var center = positions.Aggregate ( Vector3.zero, ( sum, pos ) => sum + pos );
		center = center / positions.Length;
		
		var center2d = new Vector3 ( center.x, center.y, 0 );
		float maxDistSqr = float.NegativeInfinity;

		foreach ( var p in positions ) {
			var p2d = new Vector3 ( p.x, p.y, 0 );
			float distSqr = Common.DistanceSqr ( p2d, center2d );

			if ( distSqr > maxDistSqr )
				maxDistSqr = distSqr;
		}

		float maxDist = Mathf.Sqrt ( maxDistSqr );
		
		center = camera.transform.TransformPoint ( center );

		var cameraDisplacement = Vector3.forward * maxDist * Distance + Vector3.forward * BaseDistance;
		var rot = Quaternion.Euler ( Angle, 0, 0 );
		cameraDisplacement = rot * cameraDisplacement;
		var newCameraPos = center - cameraDisplacement;

		var cTransform = camera.transform;
		//cTransform.position = Vector3.MoveTowards ( cTransform.position, newCameraPos, Velocity * Time.deltaTime );
		cTransform.position = Vector3.Lerp ( cTransform.position, newCameraPos, 0.05f );

		var newCameraRot = Quaternion.LookRotation ( cameraDisplacement );
		//cTransform.rotation = Quaternion.RotateTowards ( cTransform.rotation, newCameraRot, AngularVelocity * Time.deltaTime );
		cTransform.rotation = Quaternion.Lerp ( cTransform.rotation, newCameraRot, 0.05f );
	}
}
