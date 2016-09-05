using UnityEngine;
using System.Linq;

public static class Common {
	public static Vector3 GetConnectedAnchorWorldPos ( Joint joint ) {
		if ( joint.connectedBody != null )
			return	joint.connectedBody.transform.localToWorldMatrix.MultiplyPoint ( joint.connectedAnchor );
		else
			return	joint.connectedAnchor;
	}

	public static void SetConnectedAnchorWorldPos ( Joint joint, Vector3 pos ) {
		if ( joint.connectedBody != null )
			joint.connectedAnchor = joint.connectedBody.transform.worldToLocalMatrix.MultiplyPoint ( pos );
		else
			joint.connectedAnchor = pos;
	}

	public static float SignedAngle ( Vector3 from, Vector3 to, Vector3 axis ) {
		float dot = Mathf.Clamp ( Vector3.Dot ( from.normalized, to.normalized ), -1, 1 );
		float angle = Mathf.Acos ( dot );
		var cross = Vector3.Cross ( from, to );

		if ( Vector3.Dot ( axis, cross ) < 0 )
			angle = -angle;

		return	angle;
	}

	public static float SignedAngleDeg ( Vector3 from, Vector3 to, Vector3 axis ) {
		float angle = Vector3.Angle ( from, to );
		var cross = Vector3.Cross ( from, to );

		if ( Vector3.Dot ( axis, cross ) < 0 )
			angle = -angle;

		return	angle;
	}

	public static Vector3 ChangeDirectionCoordSys ( Vector3 v, Transform tFrom, Transform tTo ) {
		v = tFrom.TransformDirection ( v );

		if ( tTo != null )
			v = tTo.InverseTransformDirection ( v );

		return	v;
	}

	public static float DistanceSqr ( Vector3 from, Vector3 to ) {
		float dx = to.x - from.x;
		float dy = to.y - from.y;
		float dz = to.z - from.z;

		return	dx * dx + dy * dy + dz * dz;
	}

	public static float CalculateTotalMass ( GameObject root ) {
		var bodies = root.GetComponentsInChildren <Rigidbody> ();

		return	bodies.Sum ( rigidbody => rigidbody.mass );
	}
}
