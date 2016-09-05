using UnityEngine;
using System.Collections;

public class PullTowardsObjectBehaviour : MonoBehaviour {
	public GameObject TargetObject = null;
	public float SpringForce = 2;
	public float Damper = 0.2f;
	public Vector3 Anchor = new Vector3 ( 0, 1, 0 );
	public float MaxSpringLength = float.PositiveInfinity;
	private SpringJoint springJoint = null;

	private Vector3 AnchorWorldPos { get { return	transform.localToWorldMatrix.MultiplyPoint ( Anchor ); } }
	private Vector3 ConnectedAnchorWorldPos {
		get {
			var anchorWorldPos = AnchorWorldPos;
			var dPos = TargetObject.transform.position - anchorWorldPos;
			float distance = dPos.magnitude;

			if ( distance > MaxSpringLength ) {
				dPos.Normalize ();
				dPos *= MaxSpringLength;

				return	anchorWorldPos + dPos;
			} else
				return	TargetObject.transform.position;
		}
	}
	
	void FixedUpdate () {
	    if ( TargetObject != null && TargetObject.activeInHierarchy ) {
			if ( springJoint == null )
				CreateJoint ();
			
			springJoint.anchor = Anchor;
			springJoint.connectedAnchor = ConnectedAnchorWorldPos;
			springJoint.spring = SpringForce;
			springJoint.damper = Damper;
		} else
			DestroyJoint ();
	}

	void OnDisable () {
		DestroyJoint ();
	}

	void CreateJoint () {
		springJoint = gameObject.AddComponent <SpringJoint> ();
		springJoint.minDistance = 0;
		springJoint.maxDistance = 0;
		springJoint.autoConfigureConnectedAnchor = false;
	}

	void DestroyJoint () {
		if ( springJoint != null ) {
			Destroy ( springJoint );
			springJoint = null;
		}
	}

	void OnDrawGizmos () {
		if ( TargetObject != null ) {
			if ( enabled && TargetObject.activeInHierarchy )
				Gizmos.color = Color.green;
			else
				Gizmos.color = new Color ( 0, 1, 0, 0.15f );

			var connAnchorWorldPos = ConnectedAnchorWorldPos;

			Gizmos.DrawLine (
				AnchorWorldPos,
				connAnchorWorldPos
			);

			Gizmos.color = new Color ( 0, 1, 0, 0.15f );

			Gizmos.DrawLine (
				connAnchorWorldPos,
				TargetObject.transform.position
			);
		}
	}
}
