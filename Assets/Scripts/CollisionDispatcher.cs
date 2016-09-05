using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public delegate void OnTriggerEventHandler ( GameObject sender, Collider otherCollider );
public delegate void OnCollisionEventHandler ( GameObject sender, Collision collision );

public class CollisionDispatcher : MonoBehaviour {
	public event OnTriggerEventHandler TriggerEnter;
	public event OnTriggerEventHandler TriggerStay;
	public event OnTriggerEventHandler TriggerExit;
	public event OnCollisionEventHandler CollisionEnter;
	public event OnCollisionEventHandler CollisionStay;
	public event OnCollisionEventHandler CollisionExit;
	public Transform Root;
	public bool IgnoreTriggers = true;
	public bool AccumulateColliderList = false;
	public List <Collider> Colliders { get; private set; }

	void Awake () {
		Colliders = new List <Collider> ();
	}

	void OnTriggerEnter ( Collider otherCollider ) {
		if ( AccumulateColliderList && IsAcceptableCollider ( otherCollider ) )
			Colliders.Add ( otherCollider );

		DispatchTriggerEvent ( TriggerEnter, otherCollider );
	}

	void OnTriggerStay ( Collider otherCollider ) {
		DispatchTriggerEvent ( TriggerStay, otherCollider );
	}

	void OnTriggerExit ( Collider otherCollider ) {
		if ( AccumulateColliderList && IsAcceptableCollider ( otherCollider ) )
			Colliders.Remove ( otherCollider );

		DispatchTriggerEvent ( TriggerExit, otherCollider );
	}

	void OnCollisionEnter ( Collision collision ) {
		DispatchCollisionEvent ( CollisionEnter, collision );
	}

	void OnCollisionStay ( Collision collision ) {
		DispatchCollisionEvent ( CollisionStay, collision );
	}

	void OnCollisionExit ( Collision collision ) {
		DispatchCollisionEvent ( CollisionExit, collision );
	}

	private void DispatchTriggerEvent ( OnTriggerEventHandler delegateFunc, Collider otherCollider ) {
		if ( delegateFunc != null && IsAcceptableCollider ( otherCollider ) )
			delegateFunc ( gameObject, otherCollider );
	}

	private void DispatchCollisionEvent ( OnCollisionEventHandler delegateFunc, Collision collision ) {
		if ( delegateFunc != null && IsAcceptableCollider ( collision.collider ) )
			delegateFunc ( gameObject, collision );
	}

	private bool IsAcceptableCollider ( Collider collider ) {
		if ( IgnoreTriggers && collider.isTrigger )
			return	false;

		// Ignore self-intersection.
		if ( Root != null && collider.transform.IsChildOf ( Root ) )
			return	false;

		return	true;
	}
}
