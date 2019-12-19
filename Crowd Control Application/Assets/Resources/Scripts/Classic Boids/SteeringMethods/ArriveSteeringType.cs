using UnityEngine;
using System.Collections;
namespace CrowdBehavior
{
	[System.Serializable]
	public class ArriveSteeringType : SteeringType 
	{
		#region Params
		[Tooltip ("Distance in units when object is going to start decelerating")]
		[Range (0.1f, 10.0f)]
		public float SlowingRadius = 1.0f;

		#endregion

	 
		#region Methods
		protected override Vector3 ProcessDesiredVelocity()
		{
			return CalculateArrive (target);
		}
	 
	 
		public Vector3 CalculateArrive (Vector3 targetPosition)
		{
			var desiredVelocity = targetPosition - position;
			float distance = desiredVelocity.magnitude;

			// Check the distance to detect whether the character
			// is inside the slowing area
			if (distance < SlowingRadius)
			{
				// Inside the slowing area
				desiredVelocity = desiredVelocity.normalized * maxVelocity * (distance / SlowingRadius);
			}
			else
			{
				// Outside the slowing area.
				desiredVelocity = desiredVelocity.normalized * maxVelocity;
			}

			return desiredVelocity - velocity;
		}
		#endregion
	}
}