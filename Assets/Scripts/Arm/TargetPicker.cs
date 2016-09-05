using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class TargetPicker {
	public static Collider ClosestPointOnBounds ( IEnumerable <Collider> colliders, Vector3 origin, out Vector3 closestPoint ) {
		Collider closestCollider = null;
		closestPoint = Vector3.zero;
		float minDistSqr = float.PositiveInfinity;
		
		foreach ( var c in colliders ) {
			var p = c.ClosestPointOnBounds ( origin );
			float distSqr = Common.DistanceSqr ( origin, p );

			if ( distSqr < minDistSqr ) {
				closestCollider = c;
				closestPoint = p;
				minDistSqr = distSqr;
			}
		}

		return	closestCollider;
	}

	public static Collider PickClosest (
		CollisionDispatcher boundingFigure, Vector3 origin,
		System.Func <IEnumerable <Collider>, IEnumerable <Collider>> filter,
		out Vector3 closestPoint
	) {
		var colliders = boundingFigure.Colliders
			.Where ( c => c != null );	// Some of the accumulated colliders could be destroyed.

		colliders = filter ( colliders );

		var closestCollider = TargetPicker.ClosestPointOnBounds (
			colliders,
			origin,
			out closestPoint
		);

		return	closestCollider;
	}
}
