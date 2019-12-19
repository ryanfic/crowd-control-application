using UnityEngine;
using System.Collections;

namespace CrowdBehavior
{
	[System.Serializable]

	public class FleeSteeringType : SteeringType 
	{
		#region Variables
		public Vector3 FleeTarget;
		public Transform FleeTransform;
		public float safeDistance = 20 ;
		#endregion

		#region Unity Methods
		#endregion

		#region Public Methods
		protected override Vector3 ProcessDesiredVelocity()
		{

			return Flee ();
		}
		#endregion

		#region Private Methods
		Vector3 Flee ()
		{
			if (FleeTransform != null)
			{
				FleeTarget = FleeTransform.position;
			}

			var desiredVelocity = (position - FleeTarget).normalized * maxVelocity;

			return desiredVelocity - velocity;
		}



		#endregion
	}
}