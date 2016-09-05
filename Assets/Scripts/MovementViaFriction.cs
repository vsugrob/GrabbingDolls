using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementViaFriction : MonoBehaviour {
	public float HorizontalAcceleration = 10;
	public float VerticalAcceleration = 5;
	public float AirHorizontalAcceleration = 3;
	public float AirVerticalAcceleration = 0;
	public float JumpHorizontalAcceleration = 40;
	public float JumpVerticalAcceleration = 150;
	public float JumpCooldown = 0.5f;
	public float AirControlLossDelay = 0.5f;
	public float MaxVelocity = 3;
	public float RotateVelocity = 120;
	public float IdleFriction = 1;
	public float SidewaysFriction = 1;
	public float MaxClimbAngle = 50;
	public ArmGrabController RightGrabController;
	public ArmGrabController LeftGrabController;
	public GameObject Root;

	[HideInInspector]
	public float MoveX;
	[HideInInspector]
	public float MoveY;
	[HideInInspector]
	public bool PerformJump;
	[HideInInspector]
	public bool PerformCrouch;

	private MuscleTensionController muscleTensionController;
	private MaintainOrientationJoint maintainOrientationJoint;

	private List <ContactPoint> prevFrameContacts = new List <ContactPoint> ();
	private List <ContactPoint> frameContacts = new List <ContactPoint> ();
	private ContactPoint floorContact;
	public bool IsGrounded { get; private set; }
	private float lastGroundedTimestamp;
	private float lastJumpTimestamp;
	private float totalBodyMass;
	private bool IsGrabbing { get { return	RightGrabController.IsGrabbing || LeftGrabController.IsGrabbing; } }

	void Awake () {
		muscleTensionController = GetComponent <MuscleTensionController> ();
		maintainOrientationJoint = GetComponent <MaintainOrientationJoint> ();
	}

	void Start () {
		var m = collider.material;
		m.staticFriction = 0;
		m.bounceCombine = PhysicMaterialCombine.Minimum;
		collider.material = m;

		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		totalBodyMass = Common.CalculateTotalMass ( Root );
	}

	void FixedUpdate () {
		TrackContacts ();

		float timeSinceLastGrounded = Time.fixedTime - lastGroundedTimestamp;
		bool readyToJump = Time.fixedTime - lastJumpTimestamp >= JumpCooldown;
		var move = new Vector3 ( MoveX, 0, MoveY );
		var m = collider.material;
		float moveAmount = move.magnitude;
		
		if ( moveAmount != 0 ) {
			m.dynamicFriction = 0;
			m.frictionCombine = PhysicMaterialCombine.Average;

			m.dynamicFriction2 = SidewaysFriction;
			m.staticFriction2 = SidewaysFriction;
			m.frictionDirection2 = transform.InverseTransformDirection ( Vector3.Cross ( move, Vector3.up ) );

			rigidbody.WakeUp ();

			var moveDir = move.normalized;

			if ( moveAmount > 1 )
				moveAmount = 1;

			var platformVelocity = Vector3.zero;
			var horzAccel = moveDir * moveAmount;
			var vertAccel = Vector3.up * moveAmount;

			if ( IsGrounded ) {
				var otherRigidbody = floorContact.otherCollider.attachedRigidbody;
				
				if ( otherRigidbody != null )
					platformVelocity = otherRigidbody.GetPointVelocity ( floorContact.point );

				if ( PerformJump && readyToJump ) {
					horzAccel *= JumpHorizontalAcceleration;
					// In this case vertical acceleration added by separate AddForce () call.
				} else {
					horzAccel *= HorizontalAcceleration;
					vertAccel *= VerticalAcceleration;
				}
			} else {
				horzAccel *= AirHorizontalAcceleration;
				vertAccel *= AirVerticalAcceleration;
			}

			var relVelocity = rigidbody.velocity - platformVelocity;
			var relHorzVelocity = new Vector3 ( relVelocity.x, 0, relVelocity.z );

			if ( relHorzVelocity.magnitude > MaxVelocity ) {
				var accelProj = Vector3.Project ( horzAccel, relHorzVelocity.normalized );	// TODO: is ".normalized" necessary?
				horzAccel -= accelProj;
			}

			var totalAccel = horzAccel + vertAccel;
			rigidbody.AddForce ( totalAccel * totalBodyMass, ForceMode.Force );

			if ( timeSinceLastGrounded < AirControlLossDelay || IsGrabbing ) {
				maintainOrientationJoint.TargetRotation = Quaternion.RotateTowards (
					maintainOrientationJoint.TargetRotation,
					Quaternion.Inverse ( Quaternion.LookRotation ( moveDir ) ),
					RotateVelocity * Time.fixedDeltaTime
				);
			}
		} else {
			m.dynamicFriction = IdleFriction;
			m.frictionCombine = PhysicMaterialCombine.Maximum;

			m.dynamicFriction2 = 0;
			m.staticFriction2 = 0;
			m.frictionDirection2 = Vector3.zero;
		}

		collider.material = m;

		if ( PerformJump && IsGrounded && readyToJump ) {
			var normalInPelvisCoords = transform.InverseTransformDirection ( floorContact.normal );
			float jumpAmount = Mathf.Clamp01 ( Vector3.Dot ( normalInPelvisCoords, Vector3.up ) );

			if ( jumpAmount > 0 ) {
				var accel = Vector3.up * JumpVerticalAcceleration * jumpAmount;
				rigidbody.AddForce ( accel * totalBodyMass, ForceMode.Force );
				lastJumpTimestamp = Time.fixedTime;
			}
		}

		if ( PerformCrouch )
			MuscleTensionController.Relax ( muscleTensionController, damperMultiplier : 0.01f );
		else {
			if ( IsGrounded || timeSinceLastGrounded < AirControlLossDelay )
				MuscleTensionController.Stretch ( muscleTensionController, useGravity : true );
			else if ( IsGrabbing )
				MuscleTensionController.Stretch ( muscleTensionController, 0.1f, 0.1f, useGravity : true );
			else
				MuscleTensionController.Relax ( muscleTensionController, damperMultiplier : 0.1f );
		}
	}

	void OnCollisionEnter ( Collision collision ) {
		frameContacts.AddRange ( collision.contacts );
	}

	void OnCollisionStay ( Collision collision ) {
		frameContacts.AddRange ( collision.contacts );
	}

	private void TrackContacts () {
		if ( rigidbody.IsSleeping () )
			frameContacts = prevFrameContacts;

		FindFloor ( frameContacts );

		prevFrameContacts = frameContacts;

		if ( frameContacts.Count != 0 )
			frameContacts = new List <ContactPoint> ();

		if ( IsGrounded ) {
			lastGroundedTimestamp = Time.fixedTime;
			//Debug.DrawRay ( floorContact.point, floorContact.normal * 0.1f, Color.green );
		}
	}

	private void FindFloor ( IEnumerable <ContactPoint> contacts ) {
		float minAngle = float.PositiveInfinity;
		IsGrounded = false;

		foreach ( var contact in contacts ) {
			if ( contact.otherCollider == null ) {
				// Collider object has been destroyed.

				continue;
			}

			//Debug.DrawRay ( contact.point, contact.normal * 0.1f, Color.yellow );
			float angle = Vector3.Angle ( contact.normal, Vector3.up );
			
			if ( angle < minAngle ) {
				minAngle = angle;
				
				if ( angle <= MaxClimbAngle ) {
					IsGrounded = true;
					floorContact = contact;
				}
			}
		}
	}
}
