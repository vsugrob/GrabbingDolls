using UnityEngine;
using System.Collections;

// TODO: consider making base class managing armIKController, muscleTensionController, Stretch and Relax.
[RequireComponent ( typeof ( ArmIKController ), typeof ( MuscleTensionController ) )]
public class ArmIdleController : MonoBehaviour {
	public Transform Chest;
	public float DownDisplacement = 0.95f;
	public float ForwardDisplacement = 0.05f;
	public float SideDisplacement = 0;

	private ArmIKController armIKController;
	private MuscleTensionController muscleTensionController;

	void Start () {
		armIKController = GetComponent <ArmIKController> ();
		muscleTensionController = GetComponent <MuscleTensionController> ();
	}
	
	void FixedUpdate () {
		var gravityDir = Physics.gravity.normalized;
		var forwardDir = Chest.TransformDirection ( Vector3.forward );
		Vector3 sideDir;
		
		if ( armIKController.IsRight )
			sideDir = Chest.TransformDirection ( Vector3.right );
		else
			sideDir = Chest.TransformDirection ( Vector3.left );

		float armLength = armIKController.ArmLength;

		var wristTargetPosition = armIKController.ShoulderWorldAnchor +
			gravityDir * ( armLength * DownDisplacement ) +
			forwardDir * ( armLength * ForwardDisplacement ) +
			sideDir * ( armLength * SideDisplacement );

		armIKController.WristTargetPosition = wristTargetPosition;
	}

	void OnEnable () {
		MuscleTensionController.Stretch ( muscleTensionController, 0.1f, 0.2f, true );
		
		if ( armIKController != null )
			armIKController.enabled = true;
	}

	void OnDisable () {
		MuscleTensionController.Relax ( muscleTensionController );
		armIKController.enabled = false;
	}
}
