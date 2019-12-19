using UnityEngine;
using System.Collections;
namespace CrowdBehavior
{
	[System.Serializable]
	public class FollowLeaderSteeringType : SteeringType
	{
		#region Params
		public float whenStartFollow = 5 ;
		public Rigidbody Leader;

		public float DistanceBehindTheLeader = 1.0f;
		#endregion
 
		private float counter = 0 ;
		#region Methods

		protected override Vector3 ProcessDesiredVelocity ()
		{

			counter += Time.deltaTime;
			if (counter < whenStartFollow)
				return Vector3.zero;
			else 
				return FollowLeader();
		}
 

		public Vector3 FollowLeader ()
		{
			Vector3 tv = Leader.velocity;
			Vector3 desiredVelocity = Vector3.zero;
			Vector3 behind = Vector3.zero;

			tv *= -1.0f;
			tv.Normalize();
			tv *= DistanceBehindTheLeader;


			steeringBehaviour.transform.rotation = Quaternion.LookRotation(Leader.transform.position - position);

			behind = Leader.position + tv;
			desiredVelocity += steeringBehaviour.ArriveSteeringType.CalculateArrive(behind);

			return desiredVelocity;
		}

		#endregion
	}

}