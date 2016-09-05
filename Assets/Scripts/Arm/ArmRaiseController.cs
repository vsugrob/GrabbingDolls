using UnityEngine;
using System.Collections;

// TODO: consider making base class managing armIKController, muscleTensionController, Stretch and Relax.
[RequireComponent ( typeof ( ArmIKController ), typeof ( MuscleTensionController ) )]
public class ArmRaiseController : MonoBehaviour {
	public Transform RaiseAnchor;

	private ArmIKController armIKController;
	private MuscleTensionController muscleTensionController;

	void Start () {
		armIKController = GetComponent <ArmIKController> ();
		muscleTensionController = GetComponent <MuscleTensionController> ();
	}
	
	void FixedUpdate () {
		armIKController.WristTargetPosition = armIKController.SymmetricalPosition ( RaiseAnchor.position );
	}

	void OnEnable () {
		MuscleTensionController.Stretch ( muscleTensionController );
		
		if ( armIKController != null )
			armIKController.enabled = true;
	}

	void OnDisable () {
		MuscleTensionController.Relax ( muscleTensionController );
		armIKController.enabled = false;
	}
}
