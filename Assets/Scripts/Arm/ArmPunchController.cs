using UnityEngine;
using System.Linq;
using System.Collections;

// TODO: consider making base class managing armIKController, muscleTensionController, Stretch and Relax.
[RequireComponent ( typeof ( ArmIKController ), typeof ( MuscleTensionController ) )]
public class ArmPunchController : MonoBehaviour {
	public Transform BendAnchor;
	public CollisionDispatcher TargetPickerBounds;
	public Transform NoTargetAnchor;
	public CollisionDispatcher LowerArmCollisionDispatcher;
	public float BendMaxTime = 0.23f;
	public float PunchMaxTime = 0.4f;
	public float ForceMultiplier = 1;
	public float DamperMultiplier = 0.01f;
	public float RaiseElbowAngle = 80;

	private ArmIKController armIKController;
	private MuscleTensionController muscleTensionController;

	public State CurrentState { get; private set; }
	public bool ActionCompleted { get; private set; }
	private float bendTimeStart;
	private float punchTimeStart;

	private int playerLayerMask;

	void Awake () {
		playerLayerMask = LayerMask.NameToLayer ( "Player" );
	}

	void Start () {
		LowerArmCollisionDispatcher.CollisionEnter += lowerArm_OnCollisionEnterEvent;
		armIKController = GetComponent <ArmIKController> ();
		muscleTensionController = GetComponent <MuscleTensionController> ();
	}

	void FixedUpdate () {
		if ( CurrentState == State.Bending ) {
			float bendTimeElapsed = Time.fixedTime - bendTimeStart;

			if ( bendTimeElapsed > BendMaxTime ) {
				CurrentState = State.Punching;
				punchTimeStart = Time.fixedTime;
			} else {
				// Bend arm.
				armIKController.WristTargetPosition = armIKController.SymmetricalPosition ( BendAnchor.position );
			}
		} else /*if ( CurrentState == State.Punching )*/ {
			float punchTimeElapsed = Time.fixedTime - punchTimeStart;

			if ( punchTimeElapsed > PunchMaxTime ) {
				// Punching is done.
				ActionCompleted = true;
			} else {
				// Rapidly stretch arm towards the target.
				Vector3 targetPoint;

				if ( !GetTargetPoint ( out targetPoint ) )
					targetPoint = armIKController.SymmetricalPosition ( NoTargetAnchor.position );

				var hitDir = ( targetPoint - armIKController.SymmetricalPosition ( BendAnchor.position ) ).normalized;
				targetPoint += hitDir * armIKController.ArmLength;

				armIKController.WristTargetPosition = targetPoint;
			}
		}
	}

	void lowerArm_OnCollisionEnterEvent ( GameObject sender, Collision collision ) {
		if ( !this.enabled )
			return;

		if ( CurrentState == State.Punching ) {
			/* TODO: make action completed when sum of all impacts becomes greater than some threshold.
			 * UPD: consider CollisionStay event for this purpose. */
			//if ( collision.relativeVelocity.magnitude >= 4 )
			//	ActionCompleted = true;
			
			print ( "hit: " + collision.relativeVelocity.magnitude );
		}
	}

	private bool GetTargetPoint ( out Vector3 targetPoint ) {
		var closestCollider = TargetPicker.PickClosest (
			TargetPickerBounds, armIKController.SymmetricalPosition ( NoTargetAnchor.position ),
			colliders => {
				// Prefer to grab player body parts.
				var playerColliders = colliders
					.Where ( c => ( c.gameObject.layer & playerLayerMask ) != 0 );

				if ( playerColliders.Any () )
					colliders = playerColliders;

				// Try to find at least collider with rigidbody.
				var rigidbodyColliders = colliders
					.Where ( c => c.rigidbody != null );

				if ( rigidbodyColliders.Any () )
					colliders = rigidbodyColliders;
				else
					colliders = Enumerable.Empty <Collider> ();

				return	colliders;
			}, out targetPoint
		);

		return	closestCollider != null;
	}

	void OnEnable () {
		bendTimeStart = Time.fixedTime;
		CurrentState = State.Bending;
		ActionCompleted = false;

		// TODO: get components in Awake rather than Start. This check should be removed after.
		if ( armIKController != null ) {
			armIKController.RaiseElbowAtFixedAngle = true;
			armIKController.RaiseElbowAngle = RaiseElbowAngle;
		}

		MuscleTensionController.Stretch ( muscleTensionController, ForceMultiplier, DamperMultiplier );
		
		// TODO: get components in Awake rather than Start. This check should be removed after.
		if ( armIKController != null )
			armIKController.enabled = true;
	}

	void OnDisable () {
		armIKController.RaiseElbowAtFixedAngle = false;

		MuscleTensionController.Relax ( muscleTensionController );
		armIKController.enabled = false;
	}

	public enum State {
		Bending,
		Punching
	}
}
