using UnityEngine;
using System.Collections;

public class SolverSettings : MonoBehaviour {
	public int SolverIterationCount = 15;
	private Rigidbody [] childRigidbodies = new Rigidbody [0];

	private int prevSolverIterationCount = -1;

	void FixedUpdate () {
		if ( prevSolverIterationCount != SolverIterationCount ) {
			SetSolverIterationCount ( SolverIterationCount );
			prevSolverIterationCount = SolverIterationCount;
		}
	}

	void OnEnable () {
		childRigidbodies = GetComponentsInChildren <Rigidbody> ();
	}

	void OnDisable () {
		SetSolverIterationCount ( Physics.solverIterationCount );
	}

	private void SetSolverIterationCount ( int iterationCount ) {
		foreach ( var childRigidbody in childRigidbodies ) {
			childRigidbody.solverIterationCount = iterationCount;
		}
	}
}
