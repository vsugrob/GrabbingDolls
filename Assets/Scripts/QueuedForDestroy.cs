using UnityEngine;
using System.Collections;

public class QueuedForDestroy : MonoBehaviour {
	public static bool Marked ( GameObject gameObject ) {
		return	gameObject.GetComponent <QueuedForDestroy> () != null;
	}
}
