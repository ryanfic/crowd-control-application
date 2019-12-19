using UnityEngine;
using System.Collections;
namespace CrowdBehavior
{
	[System.Serializable]
	public class SeekSteeringType : SteeringType 
	{
 

		#region Methods
		protected override Vector3 ProcessDesiredVelocity()
		{
			return Seek ();
		}
 
		Vector3 Seek ()
		{
			var desiredVelocity = (target - position).normalized * maxVelocity;

			return desiredVelocity - velocity;
		}
		#endregion
	}
}