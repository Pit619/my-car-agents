using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCarAgent : Agent {

//	#region Members
	/// <summary>
	/// Event for when the car hit a wall.
	/// </summary>
	public event System.Action HitWall;

	public Sensor Sensor1;
	public Sensor Sensor2;
	public Sensor Sensor3;
	public Sensor Sensor4;
	public Sensor Sensor5;
	public GameObject car;

	//Movement constants
	private const float MAX_VEL = 20f;
	private const float ACCELERATION = 8f;
	private const float VEL_FRICT = 2f;
	private const float TURN_SPEED = 100;

	//Input
	private double engineForceInput, turningForceInput;

	/// <summary>
	/// The current velocity of the car.
	/// </summary>
	public float Velocity
	{
		get;
		private set;
	}

	/// <summary>
	/// The current rotation of the car.
	/// </summary>
	public Quaternion Rotation
	{
		get;
		private set;
	}

	// Applies the current velocity to the position of the car.
	private void ApplyVelocity()
	{
		Vector3 direction = new Vector3(0, 1, 0);
		car.transform.rotation = Rotation;
		direction = Rotation * direction;

		this.transform.position += direction * Velocity * Time.deltaTime;
	}

	// Applies some friction to velocity
	private void ApplyFriction()
	{
		if (engineForceInput == 0)
		{
			if (Velocity > 0)
			{
				Velocity -= VEL_FRICT * Time.deltaTime;
				if (Velocity < 0)
					Velocity = 0;
			}
			else if (Velocity < 0)
			{
				Velocity += VEL_FRICT * Time.deltaTime;
				if (Velocity > 0)
					Velocity = 0;            
			}
		}
	}
	


	void Start()
	{
		
	}

	public override void AgentAction(float[] action, string textAction)
	{
		//Cap input
		engineForceInput = action[0];
		turningForceInput = action[1];
		if (engineForceInput > 1)
			engineForceInput = 1;
		else if (engineForceInput < -1)
			engineForceInput = -1;

		if (turningForceInput > 1)
			turningForceInput = 1;
		else if (turningForceInput < -1)
			turningForceInput = -1;

		//Car can only accelerate further if velocity is lower than engineForce * MAX_VEL
		bool canAccelerate = false;
		if (engineForceInput < 0)
			canAccelerate = Velocity > engineForceInput * MAX_VEL;
		else if (engineForceInput > 0)
			canAccelerate = Velocity < engineForceInput * MAX_VEL;

		//Set velocity
		if (canAccelerate)
		{
			Velocity += (float)engineForceInput * ACCELERATION * Time.deltaTime;

			//Cap velocity
			if (Velocity > MAX_VEL)
				Velocity = MAX_VEL;
			else if (Velocity < -MAX_VEL)
				Velocity = -MAX_VEL;
		}

		//Set rotation
		Rotation = car.transform.rotation;
		Rotation *= Quaternion.AngleAxis((float)-turningForceInput * TURN_SPEED * Time.deltaTime, new Vector3(0, 0, 1));
		//car.transform.Rotate(new Vector3(0, 0, 1), action[0]);


		Vector3 direction = new Vector3(0, 1, 0);

		car.transform.rotation = Rotation;
		direction = Rotation * direction;
		car.transform.position += direction * Velocity * Time.deltaTime;

		//this.transform.rotation *= Quaternion.AngleAxis((float)-turningForceInput * TURN_SPEED * Time.deltaTime, new Vector3(0, 0, 1));
		ApplyFriction();
	}

	public override void CollectObservations()
	{
		List<float> state = new List<float>();
		state.Add (Sensor1.Output);
		state.Add (Sensor2.Output);
		state.Add (Sensor3.Output);
		state.Add (Sensor4.Output);
		state.Add (Sensor5.Output);
	}

	// Unity method, triggered when collision was detected.
	void OnCollisionEnter2D()
	{
		if (HitWall != null)
			HitWall();
	}

	public override void AgentReset()
	{
		
	}
}
