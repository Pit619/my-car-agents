﻿using System.Collections;
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

	Vector3 carStartPosition;
	Quaternion carStartDirection;

	//Movement constants
	private const float MAX_VEL = 20f;
	private const float ACCELERATION = 8f;
	private const float VEL_FRICT = 2f;
	private const float TURN_SPEED = 100;

	private float timeSinceLastCheckpoint;

	//Input
	private double engineForceInput, turningForceInput;

	// Maximum delay in seconds between the collection of two checkpoints until this car dies.
	private const float MAX_CHECKPOINT_DELAY = 7;

	/// <summary>
	/// The current velocity of the car.
	/// </summary>
	public float Velocity
	{
		get;
		private set;
	}

	public uint CheckpointIndex;

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
		CheckpointIndex = 1;
		carStartPosition = car.transform.position;
		carStartDirection = car.transform.rotation;
		HitWall += Done;
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

		timeSinceLastCheckpoint += Time.deltaTime;

		if (timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
		{
			Done();
		}
		if (GetReward() == 1) {
			Done();
		}
	}

	public override void CollectObservations()
	{
		List<float> state = new List<float>();
		AddVectorObs(Sensor1.Output);
		AddVectorObs(Sensor2.Output);
		AddVectorObs(Sensor3.Output);
		AddVectorObs(Sensor4.Output);
		AddVectorObs(Sensor5.Output);
	}

	// Unity method, triggered when collision was detected.
	void OnCollisionEnter2D()
	{
		if (HitWall != null)
			HitWall();
	}

	public override void AgentReset()
	{
		timeSinceLastCheckpoint = 0;
		CheckpointIndex = 1;
		Velocity = 0;
		car.transform.position = carStartPosition;
		car.transform.rotation = carStartDirection;
	}
		
	//This method can be used to remove the agent from the scene.
	public override void AgentOnDone()
	{
		
	}

	public void CheckpointCaptured()
	{
		timeSinceLastCheckpoint = 0;
	}
}
