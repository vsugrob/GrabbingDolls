using UnityEngine;
using System.Collections;

public class PullingJoint : MonoBehaviour {
	public GameObject TargetObject = null;
	public float Force = 5;
	public Vector3 Anchor = new Vector3 ( 0, 1, 0 );
	public Vector3 ConnectedAnchor = new Vector3 ( 0, 0, 0 );
	public Vector3 AnchorWorldPos { get { return	transform.localToWorldMatrix.MultiplyPoint ( Anchor ); } }
	public Vector3 ConnectedAnchorWorldPos {
		get {
			if ( TargetObject != null )
				return	TargetObject.transform.localToWorldMatrix.MultiplyPoint ( ConnectedAnchor );
			else
				return	ConnectedAnchor;
		}
	}
	private static Color gizmoCubeColor = new Color ( 0.75f, 0.5f, 0.25f );
	private static Vector3 gizmoCubeSize = new Vector3 ( 0.1f, 0.1f, 0.1f );
	private static Color gizmoSpringColor = new Color ( 0, 1, 0 );
	
	void FixedUpdate () {
		if ( TargetObject != null && !TargetObject.activeInHierarchy )
			return;

		var anchorWorldPos = AnchorWorldPos;
		var dir = ( ConnectedAnchorWorldPos - anchorWorldPos ).normalized;
		var force = dir * Force;
		rigidbody.AddForceAtPosition ( force, anchorWorldPos, ForceMode.Force );
	}

	void OnDrawGizmos () {
		if ( TargetObject != null && !TargetObject.activeInHierarchy )
			return;

		if ( enabled ) {
			var anchorWorldPos = AnchorWorldPos;
			var connectedAnchorWorldPos = ConnectedAnchorWorldPos;
			
			Gizmos.color = gizmoCubeColor;
			Gizmos.DrawCube ( anchorWorldPos, gizmoCubeSize );
			Gizmos.DrawCube ( connectedAnchorWorldPos, gizmoCubeSize );

			Gizmos.color = gizmoSpringColor;
			Gizmos.DrawLine ( anchorWorldPos, connectedAnchorWorldPos );
		}
	}
}
