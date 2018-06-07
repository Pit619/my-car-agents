using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCarAcademy : Academy {

	// Use this for initialization
	public override void AcademyReset()
	{
		myTestCar.CheckpointIndex = 1;
		//Get all checkpoints
		checkpoints = GetComponentsInChildren<MyCheckpoint>();

		CalculateCheckpointPercentages();
	}
	
	// Update is called once per frame
	public override void AcademyStep()
	{
		//Update reward for each enabled car on the track
		//for (int i = 0; i < cars.Count; i++)
		//{
		//	RaceCar car = cars[i];
		//	car.Car.SetReward(GetCompletePerc(car.Car, ref car.CheckpointIndex));
		//}
		myTestCar.SetReward(GetCompletePerc(myTestCar, ref myTestCar.CheckpointIndex));
		Debug.Log (myTestCar.GetReward ());
	}

	public static MyCarAcademy Instance
	{
		get;
		private set;
	}
		
	[SerializeField]
	private Sprite NormalCarSprite;

	private MyCheckpoint[] checkpoints;

	public MyCarAgent myTestCar;

	// Struct for storing the current cars and their position on the track.
	private class RaceCar
	{
		public RaceCar(MyCarAgent car = null, uint checkpointIndex = 1)
		{
			this.Car = car;
			this.CheckpointIndex = checkpointIndex;
		}
		public MyCarAgent Car;
		public uint CheckpointIndex;
	}
	private List<RaceCar> cars = new List<RaceCar>();

	/// <summary>
	/// The length of the current track in Unity units (accumulated distance between successive checkpoints).
	/// </summary>
	public float TrackLength
	{
		get;
		private set;
	}

	/// <summary>
	/// Calculates the percentage of the complete track a checkpoint accounts for. This method will
	/// also refresh the <see cref="TrackLength"/> property.
	/// </summary>
	private void CalculateCheckpointPercentages()
	{
		checkpoints[0].AccumulatedDistance = 0; //First checkpoint is start
		//Iterate over remaining checkpoints and set distance to previous and accumulated track distance.
		for (int i = 1; i < checkpoints.Length; i++)
		{
			checkpoints[i].DistanceToPrevious = Vector2.Distance(checkpoints[i].transform.position, checkpoints[i - 1].transform.position);
			checkpoints[i].AccumulatedDistance = checkpoints[i - 1].AccumulatedDistance + checkpoints[i].DistanceToPrevious;
		}

		//Set track length to accumulated distance of last checkpoint
		TrackLength = checkpoints[checkpoints.Length - 1].AccumulatedDistance;

		//Calculate reward value for each checkpoint
		for (int i = 1; i < checkpoints.Length; i++)
		{
			checkpoints[i].RewardValue = (checkpoints[i].AccumulatedDistance / TrackLength) - checkpoints[i-1].AccumulatedReward;
			checkpoints[i].AccumulatedReward = checkpoints[i - 1].AccumulatedReward + checkpoints[i].RewardValue;
		}
	}

	// Calculates the completion percentage of given car with given completed last checkpoint.
	// This method will update the given checkpoint index accordingly to the current position.
	private float GetCompletePerc(MyCarAgent car, ref uint curCheckpointIndex)
	{
		//Already all checkpoints captured
		if (curCheckpointIndex >= checkpoints.Length)
			return 1;

		//Calculate distance to next checkpoint
		float checkPointDistance = Vector2.Distance(car.transform.position, checkpoints[curCheckpointIndex].transform.position);

		//Check if checkpoint can be captured
		if (checkPointDistance <= checkpoints[curCheckpointIndex].CaptureRadius)
		{
			curCheckpointIndex++;
			car.CheckpointCaptured(); //Inform car that it captured a checkpoint
			return GetCompletePerc(car, ref curCheckpointIndex); //Recursively check next checkpoint
		}
		else
		{
			//Return accumulated reward of last checkpoint + reward of distance to next checkpoint
			return checkpoints[curCheckpointIndex - 1].AccumulatedReward + checkpoints[curCheckpointIndex].GetRewardValue(checkPointDistance);
		}
	}
}