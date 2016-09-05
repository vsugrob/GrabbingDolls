using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HeadLookAtController : MonoBehaviour {
	public CollisionDispatcher TargetPickerBounds;
	public Transform Chest;
	public float MinInterestDuration = 2;
	public float MaxInterestDuration = 7;
	public float RotationSpeed = 120;

	private MaintainOrientationJoint maintainOrientationJoint;

	public Collider CurrentTarget { get; private set; }
	private float interestDuration = 0;
	private float lastAcquisitionTimestamp = 0;

	private int playerLayerMask;

	void Awake () {
		maintainOrientationJoint = GetComponent <MaintainOrientationJoint> ();
		playerLayerMask = LayerMask.NameToLayer ( "Player" );
	}
	
	void FixedUpdate () {
		float timeSinceLastAcquisition = Time.fixedTime - lastAcquisitionTimestamp;

		if ( timeSinceLastAcquisition > interestDuration || !TargetPickerBounds.Colliders.Contains ( CurrentTarget ) )
			PickTarget ();
		
		if ( CurrentTarget != null ) {
			var targetPoint = CurrentTarget.ClosestPointOnBounds ( transform.position );
			var lookLocalDir = Chest.InverseTransformDirection ( targetPoint - transform.position ).normalized;
			var targetRotation = Quaternion.Inverse ( Quaternion.LookRotation ( lookLocalDir ) );
			var currentRotation = Quaternion.RotateTowards (
				maintainOrientationJoint.TargetRotation,
				targetRotation,
				RotationSpeed * Time.fixedDeltaTime
			);
			maintainOrientationJoint.TargetRotation = currentRotation;

			Debug.DrawLine ( transform.position, transform.position + Chest.TransformDirection ( lookLocalDir ), Color.red );
			Debug.DrawLine ( transform.position, targetPoint, Color.yellow );
		} else
			maintainOrientationJoint.TargetRotation = Quaternion.identity;
	}

	private void PickTarget () {
		IEnumerable <Collider> colliders = TargetPickerBounds.Colliders;

		/* TODO: assign weights to objects of different kind.
		 * Pick randomly according to weights. */
		// Prefer to look at player body parts.
		var playerColliders = colliders
			.Where ( c => ( c.gameObject.layer & playerLayerMask ) != 0 );

		if ( playerColliders.Any () )
			colliders = playerColliders;

		// Prefer colliders with rigidbody attached over static.
		var rigidbodyColliders = colliders
			.Where ( c => c.attachedRigidbody != null );

		if ( rigidbodyColliders.Any () )
			colliders = rigidbodyColliders;

		if ( colliders.Any () ) {
			var collidersArray = colliders.ToArray ();
			int targetIndex = Random.Range ( 0, collidersArray.Length );
			CurrentTarget = collidersArray [targetIndex];

			lastAcquisitionTimestamp = Time.fixedTime;
			interestDuration = Random.Range ( MinInterestDuration, MaxInterestDuration );
		} else
			CurrentTarget = null;
	}

	void OnDisable () {
		maintainOrientationJoint.TargetRotation = Quaternion.identity;
	}
}
