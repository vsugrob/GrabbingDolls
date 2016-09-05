using UnityEngine;
using System.Collections;

public class MovementInputController : MonoBehaviour {
	public string HorizontalAxis = "Horizontal";
	public string VerticalAxis = "Vertical";
	public string JumpAxis = "Jump";
	public string CrouchAxis = "Crouch";

	private MovementViaFriction movementController;

	void Start () {
		movementController = GetComponent <MovementViaFriction> ();
	}

	void Update () {
		movementController.MoveX = Input.GetAxis ( HorizontalAxis );
		movementController.MoveY = Input.GetAxis ( VerticalAxis );
		movementController.PerformJump = Input.GetAxis ( JumpAxis ) > 0;
		movementController.PerformCrouch = Input.GetAxis ( CrouchAxis ) > 0;
	}
}
