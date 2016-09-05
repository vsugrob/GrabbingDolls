using UnityEngine;
using System.Collections;
using System;

public class MaintainOrientationJoint : MonoBehaviour {
	public Vector3 Axis = Vector3.right;
	public Vector3 SecondaryAxis = Vector3.up;
	public Rigidbody ConnectedBody;
	public Quaternion TargetRotation = Quaternion.identity;
	public RotationDriveMode DriveMode = RotationDriveMode.XYAndZ;
	public OrientationDrive XDrive;
	public OrientationDrive YZDrive;
	public OrientationDrive SlerpDrive;
	public float ForceMultiplier = 1;
	public float DamperMultiplier = 1;
	public bool EnableCollision = false;
	private float prevForceMultiplier = float.NaN;
	private float prevDamperMultiplier = float.NaN;
	private ConfigurableJoint confJoint;

	public GameObject OrientationSource = null;
	public OrientationSourceCoordSystem OrientationSourceCoordSys;
	
	void FixedUpdate () {
		if ( confJoint.axis != Axis )
			confJoint.axis = Axis;

		if ( confJoint.secondaryAxis != SecondaryAxis )
			confJoint.secondaryAxis = SecondaryAxis;

		if ( confJoint.connectedBody != ConnectedBody )
			confJoint.connectedBody = ConnectedBody;

		if ( OrientationSource != null ) {
			Quaternion sourceRotationInJointCoordSys;

			if ( OrientationSourceCoordSys == OrientationSourceCoordSystem.XUpYForward ) {
				// x -> y
				// y -> z
				// z -> x

				var sourceRotation = OrientationSource.transform.rotation;
				sourceRotationInJointCoordSys = new Quaternion (
					sourceRotation.y,
					sourceRotation.z,
					sourceRotation.x,
					sourceRotation.w
				);
			} else
				sourceRotationInJointCoordSys = OrientationSource.transform.rotation;

			sourceRotationInJointCoordSys.w = -sourceRotationInJointCoordSys.w;

			if ( sourceRotationInJointCoordSys != TargetRotation )
				TargetRotation = sourceRotationInJointCoordSys;
		}

		if ( confJoint.targetRotation != TargetRotation )
			confJoint.targetRotation = TargetRotation;

		if ( confJoint.rotationDriveMode != DriveMode )
			confJoint.rotationDriveMode = DriveMode;

		bool updateDrives = false;

		if ( prevForceMultiplier != ForceMultiplier ) {
			prevForceMultiplier = ForceMultiplier;
			updateDrives = true;
		}

		if ( prevDamperMultiplier != DamperMultiplier ) {
			prevDamperMultiplier = DamperMultiplier;
			updateDrives = true;
		}

		if ( DriveMode == RotationDriveMode.XYAndZ ) {
			if ( updateDrives || UpdateIsNecessary ( confJoint.angularXDrive, XDrive ) ) {
				confJoint.angularXDrive = new JointDrive () {
					mode = XDrive.Enabled ? JointDriveMode.Position : JointDriveMode.None,
					positionSpring = XDrive.Force * ForceMultiplier,
					positionDamper = XDrive.Damper * DamperMultiplier,
					maximumForce = float.MaxValue
				};
			}

			if ( updateDrives || UpdateIsNecessary ( confJoint.angularYZDrive, YZDrive ) ) {
				confJoint.angularYZDrive = new JointDrive () {
					mode = YZDrive.Enabled ? JointDriveMode.Position : JointDriveMode.None,
					positionSpring = YZDrive.Force * ForceMultiplier,
					positionDamper = YZDrive.Damper * DamperMultiplier,
					maximumForce = float.MaxValue
				};
			}
		} else /*if ( DriveMode == RotationDriveMode.Slerp )*/ {
			if ( updateDrives || UpdateIsNecessary ( confJoint.slerpDrive, SlerpDrive ) ) {
				confJoint.slerpDrive = new JointDrive () {
					mode = SlerpDrive.Enabled ? JointDriveMode.Position : JointDriveMode.None,
					positionSpring = SlerpDrive.Force * ForceMultiplier,
					positionDamper = SlerpDrive.Damper * DamperMultiplier,
					maximumForce = float.MaxValue
				};
			}
		}

		if ( confJoint.enableCollision != EnableCollision )
			confJoint.enableCollision = EnableCollision;
	}

	private static bool UpdateIsNecessary ( JointDrive existingValue, OrientationDrive newValue ) {
		return	( existingValue.mode == JointDriveMode.Position ) != newValue.Enabled ||
			existingValue.positionSpring != newValue.Force ||
			existingValue.positionDamper != newValue.Damper;
	}

	void OnEnable () {
		confJoint = gameObject.AddComponent <ConfigurableJoint> ();
	}

	void OnDisable () {
		Destroy ( confJoint );
		confJoint = null;
	}

	[Serializable]
	public struct OrientationDrive {
		public bool Enabled;
		/// <summary>
		/// Strength of a rubber-band pull toward the defined direction.
		/// </summary>
		public float Force;
		/// <summary>
		/// Resistance strength against the Position Spring.
		/// </summary>
		public float Damper;
	}

	public enum OrientationSourceCoordSystem {
		XUpYForward,
		Normal
	}
}