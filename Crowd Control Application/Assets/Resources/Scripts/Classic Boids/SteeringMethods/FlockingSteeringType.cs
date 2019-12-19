using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CrowdBehavior
{
	[System.Serializable]

	public class FlockingSteeringType : SteeringType 
	{
		#region Variables
		[Range (0.1f, 100.0f)]
		public float CohesionRadius;

		[Range(0.1f, 100.0f)]
		public float CohesionForce;

		[Range (0.1f, 100.0f)]
		public float SeparationRadius;

		[Range(0.1f, 100.0f)]
		public float SeparationForce = 3 ;

		[Range (0.1f, 100.0f)]
		public float AlignmentRadius;

		[Range(0.1f, 100.0f)]
		public float AlignmentForce;

		public List<SteeringBehaviour> neighboursFollower;

		#endregion

	 

		#region  Methods

		protected override Vector3 ProcessDesiredVelocity()
		{
			getAgentNeighbor ();
			return Cohesion() + Separation() ;
		}

		public void getAgentNeighbor() 
		{
			neighboursFollower.Clear ();

			float distance = Mathf.Max (this.SeparationRadius, this.CohesionRadius);

			Collider[] hitColliders = Physics.OverlapSphere(position, distance /5);
			int i = 0;
			while (i < hitColliders.Length) {
				SteeringBehaviour _tempFollower = hitColliders[i].GetComponent<SteeringBehaviour>();
				if(_tempFollower!=null && !this.Equals(_tempFollower) && !neighboursFollower.Contains(_tempFollower))
					neighboursFollower.Add(hitColliders[i].GetComponent<SteeringBehaviour>()) ;
				i++;
			}
		}
		private Vector3 Cohesion()
		{
			SteeringBehaviour curNeighbour;
			Vector3 desiredVelocity = Vector3.zero;
			int counter = 1;
			for (int i = 0; i < neighboursFollower.Count; i++)
			{
				curNeighbour = neighboursFollower [i];

				if (steeringBehaviour != curNeighbour)
				{
					Vector3 toAgent = position - curNeighbour.transform.position;

					if (toAgent.magnitude < CohesionRadius)
					{
						desiredVelocity += curNeighbour.transform.position;
						counter++;
					}

				}
			}


			desiredVelocity /= (float)counter;

			desiredVelocity -= position;

			desiredVelocity *= CohesionForce;
			return desiredVelocity;
		}

		public Vector3 Separation()
		{
			SteeringBehaviour curNeighbour;
			Vector3 desiredVelocity = Vector3.zero;
			int counter = 0;
			for (int i = 0; i < neighboursFollower.Count; i++)
			{
				curNeighbour = neighboursFollower [i];

				if (steeringBehaviour != curNeighbour)
				{
					Vector3 toAgent = position - curNeighbour.transform.position;

					if (toAgent.magnitude < SeparationRadius)
					{
						float k = (SeparationRadius - toAgent.magnitude);

						desiredVelocity += toAgent.normalized * k;
						counter++;
					}

				}
			}

			desiredVelocity *= SeparationForce;
			return desiredVelocity;
		}

		private Vector3 Alignment()
		{
			SteeringBehaviour curNeighbour;
			Vector3 desiredVelocity = Vector3.zero;
			int counter = 1;
			for (int i = 0; i < neighboursFollower.Count; i++)
			{
				curNeighbour = neighboursFollower [i];

				if (steeringBehaviour != curNeighbour)
				{
					Vector3 toAgent = position - curNeighbour.transform.position;

					if (toAgent.magnitude < AlignmentRadius)
					{
						desiredVelocity += curNeighbour.GetComponent<Rigidbody>().velocity;
						counter++;
					}

				}
			}


			desiredVelocity /= (float)counter;

			desiredVelocity *= AlignmentForce;
			return desiredVelocity;
		}
		#endregion
	}
}