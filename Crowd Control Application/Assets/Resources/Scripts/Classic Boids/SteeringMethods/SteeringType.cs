using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace CrowdBehavior
{
	[System.Serializable]
	public class SteeringType
	{
		#region Params
		[Tooltip ("Whether is being processed every frame")]
		public bool isEnabled;


		[Tooltip ("Influnce of this steering type")]
		[Range (0.0f, 1.0f)]
		public float Influence = 0.5f;

		protected SteeringBehaviour steeringBehaviour;

		Vector3 _target;

		protected Vector3 target
		{
			get
			{
				if (steeringBehaviour.TargetTransform != null)
				{
					return steeringBehaviour.TargetTransform.position;
				}
				return steeringBehaviour.Target;
			}
			set
			{
				steeringBehaviour.Target = value;
			}
		}

		private Vector3 _position;

		protected Vector3 position
		{
			get
			{
				return steeringBehaviour.transform.position;
			}
			set
			{
				steeringBehaviour.transform.position = value;
			}
		}

		private Vector3 _velocity;

		public Vector3 velocity
		{
			get
			{
				return steeringBehaviour.GetComponent<Rigidbody>().velocity;
			}
			set
			{
				steeringBehaviour.GetComponent<Rigidbody>().velocity = value;
			}
		}

		private float _maxVelocity;

		public float maxVelocity
		{
			get
			{
				return steeringBehaviour.MaxVelocity;
			}
			set
			{
				steeringBehaviour.MaxVelocity = value;
			}
		}

		private Vector3 _steering;

		private Vector3 steering
		{
			get
			{
				return steeringBehaviour.Steering;
			}
			set
			{
				steeringBehaviour.Steering = value;
			}
		}

		private float _maxSteering;

		public float maxSteering
		{
			get
			{
				return steeringBehaviour.MaxSteering;
			}
			set
			{
				steeringBehaviour.MaxSteering = value;
			}
		}
	 

		#endregion

	 

		#region Methods
		public void Process()
		{
			if (isEnabled)
			{
				Vector3 influencedDesiredVelocity = ProcessDesiredVelocity() * this.Influence;

				AccumulateSteering(influencedDesiredVelocity);
			}
		}

		protected virtual Vector3 ProcessDesiredVelocity()
		{
			return Vector3.zero;
		}

		public void DebugDraw()
		{
			
		}

		public void SetSteeringBehaviour(SteeringBehaviour _steeringBehaviour)
		{
			steeringBehaviour = _steeringBehaviour;
		}


		private void AccumulateSteering(Vector3 velocityToAccumulate)
		{
			float magnitudeSoFar = steering.magnitude;
			float magnitudeRemaining = maxSteering - magnitudeSoFar;

			Vector3 addedSteering = Vector3.zero;

			if (magnitudeRemaining <= 0.0f)
			{
				return ;
			}

			float magnitudeToAdd = velocityToAccumulate.magnitude;

			if (magnitudeToAdd < magnitudeRemaining)
			{
				addedSteering = velocityToAccumulate;

			}
			else
			{
				addedSteering = (velocityToAccumulate.normalized * magnitudeRemaining);
			}
	//		var addedSteering = velocityToAccumulate;
			steering += addedSteering;
		}
		#endregion
	}
}