using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace CrowdBehavior
{
	public class Feeler
	{
		public Vector3 Dir;
		public float Length;

		public Feeler(Vector3 dir, float length)
		{
			Dir = dir;
			Length = length;
		}
	}

	/*[System.Serializable]
	public class ObstacleAvoidanceSteeringType : SteeringType 
	{
		#region Params
		//public List<PolliceOfficerBehaviour> policeNeighbor;
		public float policeDescoveringRadius = 10 ;
		// Feelers are vectors that will be traced to check for obstacles
		private Feeler[] Feelers;

		[Range (0.1f, 100.0f)]
		public float AvoidanceRadius;

		[Range (0.1f, 100.0f)]
		public float AvoidanceForce;
		// Layer mask that will be considered as obstacle
		public LayerMask ConsiderObstacle;

		#endregion

		#region MonoBehaviour
		private ObstacleAvoidanceSteeringType()
		{
			
		}
		void OnDrawGizmo() {
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(position, policeDescoveringRadius);
		}
		#endregion

		#region Methods



		protected override Vector3 ProcessDesiredVelocity()
		{
			getPoliceNeighbor ();
			return PoliceOfficerAvoidance ();
		}
	 
		private Vector3 ObstacleAvoidance ()
		{
			Vector3 result = Vector3.zero;

			CreateFeelers();
			RaycastHit closestHit = new RaycastHit ();
			Vector3 avoidance = Vector3.zero;
		
			for (int i = 0; i < Feelers.Length; i++)
			{
				RaycastHit hit;
				Physics.Raycast(position, Feelers [i].Dir, out hit, Feelers [i].Length, ConsiderObstacle);

				if (hit.collider != null)
				{
					avoidance = (position + Feelers[i].Dir) - hit.collider.transform.position;
					float k = 6.0f - avoidance.magnitude;
					avoidance = avoidance.normalized * k;
					//avoidance.Normalize();
					avoidance.Scale(new Vector3(AvoidanceForce, AvoidanceForce, AvoidanceForce));
				}
			}

			return avoidance;
		}
		public void getPoliceNeighbor() 
		{
			policeNeighbor.Clear ();
			Collider[] hitColliders = Physics.OverlapSphere(position, this.policeDescoveringRadius);
			int i = 0;
			while (i < hitColliders.Length) {
				PolliceOfficerBehaviour _tempPolice = hitColliders[i].GetComponent<PolliceOfficerBehaviour>();
				if(_tempPolice && !policeNeighbor.Contains(_tempPolice))
					policeNeighbor.Add(hitColliders[i].GetComponent<PolliceOfficerBehaviour>()) ;
				i++;
			}
		}
		private Vector3 PoliceOfficerAvoidance()
		{
			PolliceOfficerBehaviour curPolliceOfficer;
			Vector3 desiredVelocity = Vector3.zero;
			int counter = 0;
			if (policeNeighbor.Count > 0) {
				for (int i = 0; i < policeNeighbor.Count; i++) {
					curPolliceOfficer = policeNeighbor [i];

					if (steeringBehaviour != curPolliceOfficer) {
						Vector3 toAgent = position - curPolliceOfficer.transform.position;

						if (toAgent.magnitude < curPolliceOfficer.Radius) {
							float k = (curPolliceOfficer.Radius - toAgent.magnitude);

							desiredVelocity += toAgent.normalized * k;
							counter++;
						}

					}
				}
				desiredVelocity *= AvoidanceForce;
			}
			return desiredVelocity;
		}

		private void CreateFeelers ()
		{
			this.Feelers = new Feeler[1];
			this.Feelers [0] = new Feeler(this.velocity.normalized, 2.0f);
			//this.Feelers [1] = new Feeler(Quaternion.Euler(0.0f, -45.0f, 0.0f) * this.velocity.normalized, 6.0f);
			//this.Feelers [2] = new Feeler(Quaternion.Euler(0.0f, 45.0f, 0.0f) * this.velocity.normalized, 6.0f);
		}
		#endregion
	}*/
}