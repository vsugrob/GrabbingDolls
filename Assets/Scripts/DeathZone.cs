using UnityEngine;
using System.Collections;

public class DeathZone : MonoBehaviour {
	private int playerLayerMask;
	private GameObject characterPrefab;

	void Awake () {
		playerLayerMask = LayerMask.NameToLayer ( "Player" );
	}

	void Start () {
		characterPrefab = Resources.Load <GameObject> ( "Character" );
	}

	void OnTriggerEnter ( Collider otherCollider ) {
		var objLayer = otherCollider.gameObject.layer;

		if ( ( objLayer & playerLayerMask ) != 0 ) {
			var characterTf = otherCollider.transform;

			while ( !characterTf.gameObject.CompareTag ( "Player" ) ) {
				characterTf = characterTf.parent;
			}
			
			var oldChar = characterTf.gameObject;

			if ( QueuedForDestroy.Marked ( oldChar ) )
				return;

			var respawns = GameObject.FindGameObjectsWithTag ( "Respawn" );

			if ( respawns.Length > 0 ) {
				int respIdx = Random.Range ( 0, respawns.Length );
				var respawn = respawns [respIdx];
				var newChar = Instantiate ( characterPrefab, respawn.transform.position, respawn.transform.rotation ) as GameObject;
				newChar.name = oldChar.name;
				newChar.transform.parent = oldChar.transform.parent;

				newChar.GetComponent <ColorController> ().Color = oldChar.GetComponent <ColorController> ().Color;

				SetArmControllerInput ( oldChar, newChar, "Right Arm" );
				SetArmControllerInput ( oldChar, newChar, "Left Arm" );

				var oldMoveInput = oldChar.transform.Find ( "Pelvis" ).GetComponent <MovementInputController> ();
				var newMoveInput = newChar.transform.Find ( "Pelvis" ).GetComponent <MovementInputController> ();
				newMoveInput.HorizontalAxis = oldMoveInput.HorizontalAxis;
				newMoveInput.VerticalAxis = oldMoveInput.VerticalAxis;
				newMoveInput.JumpAxis = oldMoveInput.JumpAxis;
				newMoveInput.CrouchAxis = oldMoveInput.CrouchAxis;
			}

			oldChar.AddComponent <QueuedForDestroy> ();
			Destroy ( oldChar );

			return;
		}

		Destroy ( otherCollider.gameObject );
	}

	private void SetArmControllerInput ( GameObject oldChar, GameObject newChar, string armName ) {
		var oldArmInput = oldChar.transform.Find ( armName ).GetComponent <ArmInputController> ();
		var newArmInput = newChar.transform.Find ( armName ).GetComponent <ArmInputController> ();
		newArmInput.GrabOrPunchAxis = oldArmInput.GrabOrPunchAxis;
		newArmInput.RaiseAxis = oldArmInput.RaiseAxis;
	}
}
