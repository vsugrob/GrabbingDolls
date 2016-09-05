using UnityEngine;
using System.Collections;

public class MuscleTensionController : MonoBehaviour {
	public float ForceMultiplier = 1;
	public float DamperMultiplier = 1;
	public bool UseGravity = true;
	private float prevForceMultiplier = float.NaN;
	private float prevDamperMultiplier = float.NaN;
	private bool prevUseGravity;
	private MaintainOrientationJoint [] childJoints = new MaintainOrientationJoint [0];
	private Rigidbody [] childRigidbodies = new Rigidbody [0];

	void FixedUpdate () {
		bool updateMultipliers = false;

		if ( prevForceMultiplier != ForceMultiplier ) {
			prevForceMultiplier = ForceMultiplier;
			updateMultipliers = true;
		}

		if ( prevDamperMultiplier != DamperMultiplier ) {
			prevDamperMultiplier = DamperMultiplier;
			updateMultipliers = true;
		}

		if ( prevUseGravity != UseGravity ) {
			prevUseGravity = UseGravity;
			SetUseGravity ( UseGravity );
		}

		if ( updateMultipliers )
			SetMultipliers ( ForceMultiplier, DamperMultiplier );
	}

	private void SetMultipliers ( float forceMultiplier, float damperMultiplier ) {
		foreach ( var joint in childJoints ) {
			joint.ForceMultiplier = forceMultiplier;
			joint.DamperMultiplier = damperMultiplier;
		}
	}

	private void SetUseGravity ( bool useGravity ) {
		foreach ( var childRigidbody in childRigidbodies ) {
			childRigidbody.useGravity = useGravity;
		}
	}

	void OnEnable () {
		childJoints = GetComponentsInChildren <MaintainOrientationJoint> ();
		childRigidbodies = GetComponentsInChildren <Rigidbody> ();
	}

	void OnDisable () {
		SetMultipliers ( forceMultiplier : 1, damperMultiplier : 1 );
		SetUseGravity ( true );
	}

	public static void Stretch (
		MuscleTensionController controller,
		float forceMultiplier = 1, float damperMultiplier = 1, bool useGravity = false
	) {
		if ( controller != null )
			controller.SetValues ( forceMultiplier, damperMultiplier, useGravity );
	}

	public static void Relax (
		MuscleTensionController controller,
		float forceMultiplier = 0, float damperMultiplier = 0, bool useGravity = true
	) {
		if ( controller != null )
			controller.SetValues ( forceMultiplier, damperMultiplier, useGravity );
	}

	public void SetValues ( float forceMultiplier, float damperMultiplier, bool useGravity ) {
		ForceMultiplier = forceMultiplier;
		DamperMultiplier = damperMultiplier;
		UseGravity = useGravity;
	}
}
