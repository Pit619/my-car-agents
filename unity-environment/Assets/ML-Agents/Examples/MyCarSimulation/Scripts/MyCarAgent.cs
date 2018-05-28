using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCarAgent : Agent {
	
	public GameObject car;

	void Start()
	{
		
	}

	public override void AgentAction(float[] action, string textAction)
	{
		//Set rotation
		Debug.Log(action[0]);
		car.transform.Rotate(new Vector3(0, 0, 1), action[0]);
	}

	public override void CollectObservations()
	{
		List<float> state = new List<float>();

	}

	public override void AgentReset()
	{
		
	}
}
