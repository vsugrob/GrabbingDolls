using UnityEngine;
using System.Collections;

public class PrintTotalMass : MonoBehaviour {
	void Start () {
		print ( name + " mass: " + Common.CalculateTotalMass ( gameObject ) + "kg" );
	}
}
