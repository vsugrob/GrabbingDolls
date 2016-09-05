using UnityEngine;
using System.Collections;

public class ArmIKController : MonoBehaviour {
	public GameObject UpperArm;
	public GameObject LowerArm;
	public Transform Chest;
	public Vector3 WristAnchor = new Vector3 ( 0, -0.5f, 0 );
	public bool IsRight = true;
	public bool RaiseElbow = true;
	public bool RaiseElbowAtFixedAngle = false;
	public float RaiseElbowAngle = 50;
	public bool LowerArmSeeksTarget = true;
	public bool AverageSeekAndNormal = true;
	public Vector3 WristTargetPosition;
	public GameObject WristTargetPositionSource;
	public BreatheDisplacement Breathe = new BreatheDisplacement ();
	private CharacterJoint ShoulderJoint;
	private CharacterJoint ElbowJoint;
	private MaintainOrientationJoint upperArmOrientationJoint;
	private MaintainOrientationJoint lowerArmOrientationJoint;
	public Vector3 ShoulderWorldAnchor {
		get {
			if ( ShoulderJoint.connectedBody != null )
				return	ShoulderJoint.connectedBody.transform.localToWorldMatrix.MultiplyPoint ( ShoulderJoint.connectedAnchor );
			else
				return	ShoulderJoint.connectedAnchor;
		}
	}
	public Vector3 ElbowWorldAnchor { get { return	ShoulderJoint.transform.localToWorldMatrix.MultiplyPoint ( ElbowJoint.connectedAnchor ); } }
	public Vector3 WristWorldAnchor { get { return	ElbowJoint.transform.localToWorldMatrix.MultiplyPoint ( WristAnchor ); } }
	public float UpperArmLength { get; private set; }
	public float LowerArmLength { get; private set; }
	public float ArmLength { get { return	UpperArmLength + LowerArmLength; } }

	/// <summary>
	/// Initial lateral vector stored in coordinate space of ShoulderJoint.connectedBody.
	/// </summary>
	private Vector3 upperArmInitialVector;
	private Vector3 UpperArmInitialVectorWorld {
		get {
			if ( ShoulderJoint.connectedBody != null )
				return	ShoulderJoint.connectedBody.transform.TransformDirection ( upperArmInitialVector );
			else
				return	upperArmInitialVector;
		}
	}
	/// <summary>
	/// Stored in coordinate space of ShoulderJoint.connectedBody.
	/// </summary>
	private Vector3 upperArmAxis;
	private Vector3 UpperArmAxisWorld {
		get {
			if ( ShoulderJoint.connectedBody != null )
				return	ShoulderJoint.connectedBody.transform.TransformDirection ( upperArmAxis );
			else
				return	upperArmAxis;
		}
	}
	/// <summary>
	/// Stored in coordinate space of ShoulderJoint.connectedBody.
	/// </summary>
	private Vector3 upperArmSwingAxis;
	private Vector3 UpperArmSwingAxisWorld {
		get {
			if ( ShoulderJoint.connectedBody != null )
				return	ShoulderJoint.connectedBody.transform.TransformDirection ( upperArmSwingAxis );
			else
				return	upperArmSwingAxis;
		}
	}
	/// <summary>
	/// Stored in coordinate space of ShoulderJoint.connectedBody.
	/// </summary>
	private Vector3 upperArmSwing2Axis;
	private Vector3 UpperArmSwing2AxisWorld {
		get {
			if ( ShoulderJoint.connectedBody != null )
				return	ShoulderJoint.connectedBody.transform.TransformDirection ( upperArmSwing2Axis );
			else
				return	upperArmSwing2Axis;
		}
	}
	/// <summary>
	/// Stored in coordinate space of ShoulderJoint.connectedBody.
	/// </summary>
	private Quaternion upperArmInitialRotation;

	void Start () {
		ShoulderJoint = UpperArm.GetComponent <CharacterJoint> ();
		ElbowJoint = LowerArm.GetComponent <CharacterJoint> ();
		upperArmOrientationJoint = UpperArm.GetComponent <MaintainOrientationJoint> ();
		lowerArmOrientationJoint = LowerArm.GetComponent <MaintainOrientationJoint> ();
		
		var shoulderWorldAnchor = ShoulderWorldAnchor;
		var elbowWorldAnchor = ElbowWorldAnchor;
		UpperArmLength = Vector3.Distance ( elbowWorldAnchor, shoulderWorldAnchor );
		LowerArmLength = Vector3.Distance ( elbowWorldAnchor, WristWorldAnchor );

		var connectedTransform = ShoulderJoint.connectedBody != null ? ShoulderJoint.connectedBody.transform : null;
		upperArmInitialVector = elbowWorldAnchor - shoulderWorldAnchor;

		if ( connectedTransform != null )
			upperArmInitialVector = connectedTransform.InverseTransformDirection ( upperArmInitialVector );

		upperArmAxis = Common.ChangeDirectionCoordSys (
			ShoulderJoint.axis,
			ShoulderJoint.transform,
			connectedTransform
		);
		upperArmSwingAxis = Common.ChangeDirectionCoordSys (
			ShoulderJoint.swingAxis,
			ShoulderJoint.transform,
			connectedTransform
		);
		upperArmSwing2Axis = Common.ChangeDirectionCoordSys (
			Vector3.Cross ( ShoulderJoint.axis, ShoulderJoint.swingAxis ),
			ShoulderJoint.transform,
			connectedTransform
		);

		upperArmInitialRotation = UpperArm.transform.rotation;

		if ( connectedTransform != null )
			upperArmInitialRotation *= Quaternion.Inverse ( connectedTransform.rotation );
	}
	
	void FixedUpdate () {
		if ( WristTargetPositionSource != null )
			WristTargetPosition = WristTargetPositionSource.transform.position;

		Vector3 wristDisplacedPosition;

		if ( Breathe.Enabled ) {
			var gravityDir = Physics.gravity.normalized;
			var forwardDir = Chest.TransformDirection ( Vector3.forward );
			var sideDir = Chest.TransformDirection ( Vector3.right );
			float armLength = UpperArmLength + LowerArmLength;

			var wristBreatheDisplacement = Breathe.GetDisplacement ( gravityDir, forwardDir, sideDir, armLength );
			wristDisplacedPosition = WristTargetPosition + wristBreatheDisplacement;
		} else
			wristDisplacedPosition = WristTargetPosition;

		// Calculate angles.
		var cVector = wristDisplacedPosition - ShoulderWorldAnchor;

		float initialToCAngle = Common.SignedAngle (
			UpperArmInitialVectorWorld,
			cVector,
			UpperArmAxisWorld
		);

		// Negate because PhysX uses right handed coordinate system.
		initialToCAngle = -initialToCAngle;

		float a = UpperArmLength;
		float b = LowerArmLength;
		float c = cVector.magnitude;
		float elbowAngleX, shoulderAngleX;

		if ( a + b > c ) {
			// Note: see "Doc/ArmIKDev.txt" for details.
			float d = ( a * a - b * b + c * c ) / ( 2 * c );
			float e = c - d;
			float acAngle = Mathf.Acos ( d / a );
			float angleA = Mathf.PI / 2 - acAngle;
			float angleB = Mathf.PI / 2 - Mathf.Acos ( e / b );
			elbowAngleX = Mathf.PI - ( angleA + angleB );
			shoulderAngleX = initialToCAngle - acAngle;
		} else {
			elbowAngleX = 0;
			shoulderAngleX = initialToCAngle;
		}

		if ( LowerArmSeeksTarget ) {
			// Turn lower arm towards wrist target position.
			var eVector = wristDisplacedPosition - ElbowWorldAnchor;
			eVector = UpperArm.transform.InverseTransformDirection ( eVector ).normalized;
			eVector.x = 0;

			if ( eVector.sqrMagnitude > 0.01f ) {	// TODO: invent some threshold constant.
				float seekingElbowAngleX = Common.SignedAngle ( Vector3.down, eVector, Vector3.left );
				
				if ( seekingElbowAngleX < 0 || seekingElbowAngleX > ElbowJoint.highTwistLimit.limit * Mathf.Deg2Rad )
					seekingElbowAngleX = 0;

				if ( AverageSeekAndNormal )
					elbowAngleX = ( elbowAngleX + seekingElbowAngleX ) * 0.5f;
				else
					elbowAngleX = seekingElbowAngleX;
			}
		}

		//print ( "elbowAngleX: " + elbowAngleX * Mathf.Rad2Deg );
		
		float projX = Vector3.Dot ( UpperArmAxisWorld, cVector );
		float projY = Vector3.Dot ( UpperArmSwingAxisWorld, cVector );
		float projZ = Vector3.Dot ( UpperArmSwing2AxisWorld, cVector );
		float shoulderAngleY = Mathf.Atan2 ( projX, projZ );

		// Negate because PhysX uses right handed coordinate system.
		shoulderAngleY = -shoulderAngleY;

		float shoulderAngleZ;

		if ( Mathf.Abs ( shoulderAngleY ) > Mathf.PI / 2 ) {
			shoulderAngleY += Mathf.PI;
			shoulderAngleZ = 0;
		} else {
			if ( RaiseElbow ) {
				if ( RaiseElbowAtFixedAngle )
					shoulderAngleZ = RaiseElbowAngle;
				else
					shoulderAngleZ = Mathf.Abs ( shoulderAngleY ) * Mathf.Rad2Deg;

				if ( !IsRight )
					shoulderAngleZ = -shoulderAngleZ;
			} else
				shoulderAngleZ = 0;
		}

		// Negate because PhysX uses right handed coordinate system.
		shoulderAngleZ = -shoulderAngleZ;

		elbowAngleX *= Mathf.Rad2Deg;
		shoulderAngleX *= Mathf.Rad2Deg;
		shoulderAngleY *= Mathf.Rad2Deg;

		//print ( "shoulderAngleX: " + shoulderAngleX );
		//print ( "shoulderAngleY: " + shoulderAngleY );
		
		// Set angles.
		upperArmOrientationJoint.TargetRotation =
			Quaternion.AngleAxis ( shoulderAngleX, upperArmOrientationJoint.Axis ) *
			Quaternion.AngleAxis ( shoulderAngleY, upperArmOrientationJoint.SecondaryAxis ) *
			Quaternion.AngleAxis ( shoulderAngleZ, new Vector3 ( projX, projY, projZ ) );
		lowerArmOrientationJoint.TargetRotation = Quaternion.AngleAxis ( elbowAngleX, lowerArmOrientationJoint.Axis );
	}

	void OnEnable () {
		Breathe.StartTime = Time.fixedTime;

		if ( Breathe.Randomize ) {
			Breathe.StartTime += Random.Range ( -Mathf.PI * 2, 0 );
		}
	}

	void OnDrawGizmos () {
		if ( enabled && upperArmOrientationJoint != null ) {
			// Draw line connecting shoulder origin and point that the wrist is trying to reach.
			Gizmos.color = new Color ( 1, 0.5f, 0.25f );
			Gizmos.DrawLine ( ShoulderWorldAnchor, WristTargetPosition );

			// Draw arm final position.
			var upperArmRotation = upperArmInitialRotation;
			var connectedTransform = ShoulderJoint.connectedBody != null ? ShoulderJoint.connectedBody.transform : null;

			if ( connectedTransform != null )
				upperArmRotation *= connectedTransform.rotation;

			var upperArmTargetRotation = Quaternion.Inverse ( upperArmOrientationJoint.TargetRotation );
			Gizmos.color = Color.green;

			// Draw upper arm.
			Vector3 upperArmVector = upperArmRotation * upperArmTargetRotation * Vector3.down * UpperArmLength;
			var upperArmEnd = ShoulderWorldAnchor + upperArmVector;
			Gizmos.DrawLine ( ShoulderWorldAnchor, upperArmEnd );

			// Draw lower arm.
			var lowerArmTargetRotation = Quaternion.Inverse ( lowerArmOrientationJoint.TargetRotation );
			var lowerArmVector = upperArmRotation * upperArmTargetRotation * lowerArmTargetRotation * Vector3.down * LowerArmLength;
			var lowerArmEnd = upperArmEnd + lowerArmVector;
			Gizmos.DrawLine ( upperArmEnd, lowerArmEnd );
		}
	}

	public Vector3 SymmetricalPosition ( Vector3 pos ) {
		if ( !IsRight ) {
			pos = Chest.InverseTransformPoint ( pos );
			pos.x = -pos.x;
			pos = Chest.TransformPoint ( pos );
		}

		return	pos;
	}

	[System.Serializable]
	public class BreatheDisplacement {
		[HideInInspector]
		public float StartTime;
		public bool Enabled = true;
		public float VerticalPeriod = 2;
		public float VerticalDisplacement = 0.025f;
		public float ForwardPeriod = 3;
		public float ForwardDisplacement = 0.03f;
		public float SidePeriod = 4;
		public float SideDisplacement = 0.02f;
		public bool Randomize = true;

		public Vector3 GetDisplacement ( Vector3 gravityDir, Vector3 forwardDir, Vector3 sideDir, float armLength ) {
			float t = Time.fixedTime - StartTime;
			float a = t * Mathf.PI * 2;

			return	gravityDir * ( armLength * VerticalDisplacement   ) * Mathf.Sin ( a / VerticalPeriod ) +
					forwardDir * ( armLength * ForwardDisplacement ) * Mathf.Cos ( a / ForwardPeriod ) +
					sideDir * ( armLength * SideDisplacement ) * Mathf.Cos ( a / SidePeriod );
		}
	}
}
