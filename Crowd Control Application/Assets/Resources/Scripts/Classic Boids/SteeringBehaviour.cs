using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace CrowdBehavior
{
	public enum WalkingState
	{
		Idle,
		Walking,
		Runnning,
		attack
	}


	public class SteeringBehaviour : MonoBehaviour
	{
		#region Params boid
		public ArriveSteeringType ArriveSteeringType;
		public SeekSteeringType SeekSteeringType;
		public FleeSteeringType FleeSteeringType;
		public WallAvoidanceSteeringType WallAvoidanceSteeringType;
		public FollowLeaderSteeringType FollowLeaderSteeringType;
		//public ObstacleAvoidanceSteeringType ObstacleAvoidanceSteeringType;
		public FlockingSteeringType FlockingSteeringType;
		// Maximum velocity
		//[HideInInspector]
		public float MaxVelocity = 0.5f;
		// Maximum steering radius
		//[HideInInspector]
		public float MaxSteering = 2.0f;
		// Steering vectory
		[HideInInspector]
		public Vector3 Steering;
		// Group behaviours
		private Vector3 velocity;
		#endregion


		#region Params Simulations
		public WalkingState WalkingState = WalkingState.Idle;
		// Target
		public bool Enabled = true; // go for target
		public bool LookAt = true ;
		
		public Vector3 Target;
		public Transform TargetTransform;// TargetTransform's position will be used instead of Target if is not null.
		#endregion

		private Vector3 lastPosition ;
		private bool isMouving ;
		#region MonoBehaviour
		void Awake()
		{
			Initialize();
			FlockingSteeringType.SeparationForce += Random.Range (0.1f, 5.5f);
			lastPosition = transform.position;
		}
		public float magVelocety ;

		void FixedUpdate()
		{
			if (!Enabled )
			{
				return;
			}
			if (transform.position == lastPosition) {
				isMouving = false;
			} else 
			{
				isMouving = true ;
				lastPosition = transform.position ;
			}
 	

			var vel = GetComponent<Rigidbody>().velocity;

			if (LookAt && GetComponent<Rigidbody>().velocity != Vector3.zero  && !vel.Equals(Vector3.zero))
			{
				transform.rotation = Quaternion.LookRotation(vel);
			}

			Steering = Vector3.zero;

			//ObstacleAvoidanceSteeringType.Process();
			WallAvoidanceSteeringType.Process();
			FollowLeaderSteeringType.Process();
			FlockingSteeringType.Process();
			ArriveSteeringType.Process();
			SeekSteeringType.Process();
			FleeSteeringType.Process();

			ApplySteering();

			magVelocety = this.GetComponent<Rigidbody>().velocity.magnitude;

			if (magVelocety < 0.3f || !isMouving)
			{
				WalkingState = WalkingState.Idle;
			}
			else if (magVelocety > 0.3f && magVelocety < 3.8f)
			{
				WalkingState = WalkingState.Walking;
			}
			else if (magVelocety > 3.8f  )
			{
				WalkingState = WalkingState.Runnning;
			}
		}

		void ApplySteering()
		{
			Vector3 acceleration = Steering / this.GetComponent<Rigidbody>().mass;
			velocity += acceleration;

			velocity = velocity.Truncate(MaxVelocity);

			if (velocity.magnitude < 0.1f)
			{
				velocity = Vector3.zero;
			}
			this.GetComponent<Rigidbody>().velocity = velocity;
		}
 
		#endregion
		#region Methodes
		void Initialize()
		{
			ArriveSteeringType.SetSteeringBehaviour(this);
			SeekSteeringType.SetSteeringBehaviour(this);
			FleeSteeringType.SetSteeringBehaviour(this);
			WallAvoidanceSteeringType.SetSteeringBehaviour(this);
			FollowLeaderSteeringType.SetSteeringBehaviour(this);
			//ObstacleAvoidanceSteeringType.SetSteeringBehaviour(this);
			FlockingSteeringType.SetSteeringBehaviour(this);
		}
		public void ResetForce()
		{
			GetComponent<Rigidbody> ().velocity = Vector3.zero;
			GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
			WalkingState = WalkingState.Idle;

		}
		#endregion

	}
}
