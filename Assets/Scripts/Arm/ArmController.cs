using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent ( typeof ( ArmIdleController ) )]
[RequireComponent ( typeof ( ArmGrabController ) )]
[RequireComponent ( typeof ( ArmRaiseController ) )]
[RequireComponent ( typeof ( ArmPunchController ) )]
public class ArmController : MonoBehaviour {
	public Transform Root;
	public Transform Chest;

	[HideInInspector]
	public bool PerformGrab;
	[HideInInspector]
	public bool PerformRaise;
	[HideInInspector]
	public bool PerformPunch;
	[HideInInspector]
	public bool PerformStretch;

	private ArmIKController ikController;
	private MonoBehaviour [] actionControllers;
	private ArmIdleController idleController;
	private ArmGrabController grabController;
	private ArmRaiseController raiseController;
	private ArmPunchController punchController;
	private ArmStretchController stretchController;	// TODO: remove? Development code.
	private bool isInitialized = false;
	private bool armStretched = false;

	void Start () {
		ikController = GetComponent <ArmIKController> ();
		idleController = GetComponent <ArmIdleController> ();
		grabController = GetComponent <ArmGrabController> ();
		raiseController = GetComponent <ArmRaiseController> ();
		punchController = GetComponent <ArmPunchController> ();
		stretchController = GetComponent <ArmStretchController> ();
		actionControllers = new MonoBehaviour [] { idleController, grabController, raiseController, punchController, stretchController };

		// Assign Root and Chest.
		var dispatchers = GetComponentsInChildren <CollisionDispatcher> ();

		foreach ( var dispatcher in dispatchers ) {
			dispatcher.Root = Root;
		}

		ikController.Chest = Chest;
		idleController.Chest = Chest;
	}
	
	void FixedUpdate () {
		Initialize ();
		
		if ( punchController.enabled ) {
			// Punching is in progress.
			if ( punchController.ActionCompleted )
				EnableSingleController ( idleController );
			else {
				// All other actions ignored till punching is done.
				return;
			}
		}

		if ( PerformGrab ) {
			if ( PerformRaise && grabController.IsGrabbing ) {
				// Enable raise without disabling grab.
				raiseController.enabled = true;
			} else
				EnableSingleController ( grabController );
		} else if ( PerformPunch )
			EnableSingleController ( punchController );
		else if ( PerformRaise ) {
			if ( grabController.enabled ) {
				/* In previous frame arm probably was rasing and grabbing
				 * simultaneously. In order to switch to solely raising
				 * we need to disable all controllers prior to turning
				 * raise controller on. */
				DisableAllControllers ();
			}

			EnableSingleController ( raiseController );
		} else if ( PerformStretch ) {
			if ( !stretchController.enabled )
				armStretched = true;
			else
				armStretched = false;
		} else {
			if ( armStretched )
				EnableSingleController ( stretchController );
			else
				EnableSingleController ( idleController );
		}
	}

	private void Initialize () {
		if ( !isInitialized ) {
			isInitialized = true;

			// Disable all controllers.
			foreach ( var c in actionControllers ) {
				c.enabled = false;
			}

			// Enable the one which is in charge now.
			EnableSingleController ( idleController );
		}
	}

	private void EnableSingleController ( MonoBehaviour controller ) {
		foreach ( var c in actionControllers ) {
			if ( c != controller )
				c.enabled = false;
		}

		controller.enabled = true;
	}

	private void DisableAllControllers () {
		foreach ( var c in actionControllers ) {
			c.enabled = false;
		}
	}
}
