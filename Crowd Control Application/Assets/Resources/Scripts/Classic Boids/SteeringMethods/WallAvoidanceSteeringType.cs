using UnityEngine;
using System.Collections;
namespace CrowdBehavior
{
	[System.Serializable]
	public class WallAvoidanceSteeringType : SteeringType 
	{
		#region Variables
		// Feelers are vectors that will be traced to check for walls
		private Feeler[] Feelers;

		// Layer mask that will be considered as obstacle
		public LayerMask ConsiderObstacle;

		#endregion
 

		#region  Methods
		protected override Vector3 ProcessDesiredVelocity()
		{
			return WallAvoidance ();
		}

		private Vector3 WallAvoidance ()
		{
			Vector3 result = Vector3.zero;
			CreateFeelers();

			RaycastHit closestHit = new RaycastHit ();

			for (int i = 0; i < Feelers.Length; i++)
			{
				RaycastHit hit;
				Physics.Raycast(position, Feelers [i].Dir, out hit, Feelers [i].Length, ConsiderObstacle);

				if (hit.collider != null)
				{
					
					// Calculate by what distance vehicle will overshoot the wall
					float overshoot = Feelers[i].Length - (hit.point - position).magnitude;

					Debug.DrawLine(hit.point, hit.point + hit.normal);

					result += (hit.normal * overshoot);
				}
			}
			Debug.Log(result);
			return result;
		}

		private void CreateFeelers ()
		{
			this.Feelers = new Feeler[3];
			this.Feelers [0] = new Feeler(this.velocity.normalized, 6.0f);
			this.Feelers [1] = new Feeler(Quaternion.Euler(0.0f, -45.0f, 0.0f) * this.velocity.normalized, 3.0f);
			this.Feelers [2] = new Feeler(Quaternion.Euler(0.0f, 45.0f, 0.0f) * this.velocity.normalized, 3.0f);
		}
		#endregion
	}
}