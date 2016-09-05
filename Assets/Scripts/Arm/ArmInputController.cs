using UnityEngine;
using System.Collections;

public class ArmInputController : MonoBehaviour {
	public float ShortDuration = 0.2f;
	public string GrabOrPunchAxis = "GrabOrPunch";
	public string RaiseAxis = "Raise";
	private ArmController armController;
	private float grabOrPunchTimestamp;
	private bool grabOrPunchWasDown = false;

	void Start () {
		armController = GetComponent <ArmController> ();
	}
	
	void Update () {
		armController.PerformGrab = false;
		armController.PerformPunch = false;

		if ( Input.GetAxis ( GrabOrPunchAxis ) > 0 ) {
			armController.PerformGrab = true;

			if ( !grabOrPunchWasDown ) {
				grabOrPunchTimestamp = Time.time;
				grabOrPunchWasDown = true;
			}
		} else {
			if ( grabOrPunchWasDown && Time.time - grabOrPunchTimestamp <= ShortDuration )
				armController.PerformPunch = true;

			grabOrPunchWasDown = false;
		}
		
		armController.PerformRaise = Input.GetAxis ( RaiseAxis ) > 0;
		//armController.StretchAction = Input.GetAxis ( "Stretch" ) > 0;	// TODO: remove, this is dev code.
	}
}
