using UnityEngine;
using System.Collections;
using System.Linq;

// TODO: consider making base class managing armIKController, muscleTensionController, Stretch and Relax.
[RequireComponent ( typeof ( ArmIKController ), typeof ( MuscleTensionController ) )]
public class ArmGrabController : MonoBehaviour {
	public float BreakForce = float.PositiveInfinity;
	public float BreakTorque = float.PositiveInfinity;
	public GameObject LowerArm;
	/// <summary>
	/// Local to lower arm transform.
	/// </summary>
	public Vector3 GrabAnchor = new Vector3 ( 0, -0.5f, 0 );
	public CollisionDispatcher TargetPickerBounds;
	public Transform NoTargetAnchor;
	public CollisionDispatcher WristCollisionDispatcher;

	private ArmIKController armIKController;
	private MuscleTensionController muscleTensionController;

	private ConfigurableJoint fixedJoint;
	public Collider GrabbedCollider { get; private set; }
	public bool IsGrabbing { get { return	fixedJoint != null; } }
	public bool WantsToGrab { get; private set; }

	private int playerLayerMask;

	void Awake () {
		playerLayerMask = LayerMask.NameToLayer ( "Player" );
	}

	void Start () {
		WristCollisionDispatcher.TriggerEnter += wrist_OnCollisionEnterEvent;
		WristCollisionDispatcher.TriggerStay += wrist_OnCollisionEnterEvent;
		armIKController = GetComponent <ArmIKController> ();
		muscleTensionController = GetComponent <MuscleTensionController> ();
	}

	void FixedUpdate () {
		Vector3 closestPoint;

		var closestCollider = TargetPicker.PickClosest (
			TargetPickerBounds, armIKController.SymmetricalPosition ( NoTargetAnchor.position ), //armIKController.WristWorldAnchor,
			colliders => {
				// Prefer to grab player body parts.
				var playerColliders = colliders
					.Where ( c => ( c.gameObject.layer & playerLayerMask ) != 0 );

				if ( playerColliders.Any () )
					colliders = playerColliders;

				// Prefer colliders with rigidbody attached over static.
				var rigidbodyColliders = colliders
					.Where ( c => c.attachedRigidbody != null );

				if ( rigidbodyColliders.Any () )
					colliders = rigidbodyColliders;

				return	colliders;
			}, out closestPoint
		);
		
		if ( closestCollider != null ) {
			// Stretch arm toward closest point on suitable collider.
			armIKController.WristTargetPosition = closestPoint;
		} else {
			// Stretch arm forward.
			armIKController.WristTargetPosition = armIKController.SymmetricalPosition ( NoTargetAnchor.position );
		}
	}

	void OnEnable () {
		PerformStartGrabbing ();
	}

	void OnDisable () {
		PerformRelease ();
	}

	// TODO: remove?
	public void StartGrabbing () {
		this.enabled = true;
	}

	private void PerformStartGrabbing () {
		if ( IsGrabbing )
			PerformRelease ();
		
		WantsToGrab = true;

		MuscleTensionController.Stretch ( muscleTensionController );

		if ( armIKController != null )
			armIKController.enabled = true;
	}

	// TODO: remove?
	public void Release () {
		this.enabled = false;
	}

	private void PerformRelease () {
		if ( IsGrabbing ) {
			Destroy ( fixedJoint );
			fixedJoint = null;
			GrabbedCollider = null;
		}

		WantsToGrab = false;
		MuscleTensionController.Relax ( muscleTensionController );
		armIKController.enabled = false;
	}

	void wrist_OnCollisionEnterEvent ( GameObject sender, Collider otherCollider ) {
		if ( WantsToGrab ) {
			// Fix position, allow rotation.
			fixedJoint = LowerArm.AddComponent <ConfigurableJoint> ();
			fixedJoint.connectedBody = otherCollider.attachedRigidbody;
			fixedJoint.xMotion = ConfigurableJointMotion.Locked;
			fixedJoint.yMotion = ConfigurableJointMotion.Locked;
			fixedJoint.zMotion = ConfigurableJointMotion.Locked;
			fixedJoint.breakForce = BreakForce;
			fixedJoint.breakTorque = BreakTorque;
			fixedJoint.anchor = GrabAnchor;

			GrabbedCollider = otherCollider;
			WantsToGrab = false;
			MuscleTensionController.Relax ( muscleTensionController );
			armIKController.enabled = false;
		}
	}
}
