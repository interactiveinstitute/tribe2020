using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour {

	public Vector3[] ElevatorPositions;

	public float elevatorspeed = 2;
	public float doorspeed = 2;

	public string Tag = "Player";

	public GameObject elevator;
	public GameObject leftdoor;
	public GameObject rightdoor;

	bool dooropen = false;
	bool doorclose = false;
	bool callForElevator = false;
	bool elevatorleavenow = false;

	private Vector3 rightopenpos;
	private Vector3 leftopenpos;
	private Vector3 rightclosedpos;
	private Vector3 leftclosedpos;




void Start () {
		// Closed doors position
		rightclosedpos = rightdoor.transform.position;
		leftclosedpos = leftdoor.transform.position;

		// Opened doors position
		rightopenpos = rightdoor.transform.position + new Vector3(0f, 0f, -0.45f);
		leftopenpos = leftdoor.transform.position + new Vector3(0f, 0f, 0.45f);


}
	

	void Update () {

		if (callForElevator == true) {
			StartCoroutine(elevator_come());
		}


		if (elevatorleavenow == true) {
			StartCoroutine(elevatorleave());

		}


		if (dooropen == true) {
			StartCoroutine(open_elevator_doors());

		}


		if (doorclose == true) {
			StartCoroutine(close_elevator_doors());
		}
	}



	void OnTriggerEnter(Collider Player)
	{
		if (Player.transform.tag == Tag) 
		{
			callForElevator = true;
		}
	}
	void OnTriggerExit(Collider Player)
	{
		if (Player.transform.tag == Tag) 
		{
			doorclose = true;
		}
	}	


	// Function to find the current Ai standing at the elevator and pick the closest elevator position for the elevator

	Vector3 GetClosestEelevatorPosition(Vector3[] ElevatorPositions)
	{
		Vector3 tMin = new Vector3() ;
		float minDist = Mathf.Infinity;
		GameObject currentCharacterAi = GameObject.FindGameObjectWithTag (Tag);

		Vector3 currentPos = currentCharacterAi.transform.position;
		foreach (Vector3 t in ElevatorPositions)
		{
			float dist = Vector3.Distance(t, currentPos);
			if (dist < minDist)
			{
				tMin = t;
				minDist = dist;
			}
		}
		return tMin;
	}
		


	// animate the elevator to the current position (level) where Ai stands

	IEnumerator elevator_come() {
		callForElevator = false;

		Vector3 level = GetClosestEelevatorPosition(ElevatorPositions);
		while (Vector3.Distance (elevator.transform.position, level) > 0.005f) {
			
			elevator.transform.position = Vector3.MoveTowards (elevator.transform.position, level, elevatorspeed * Time.deltaTime);
			yield return null;
		}
			
		yield return new WaitForSeconds(2);
		dooropen = true;
	}
	// Animate elevator to idle position at top of building 

	IEnumerator elevatorleave() {
		elevatorleavenow = false;
		yield return new WaitForSeconds(2);

		// idle position at top of building
		Vector3 idlePosition = new Vector3 (2.43410f, 10f, 3.024317f);
		while (Vector3.Distance (elevator.transform.position, idlePosition) > 0.005f) {

			elevator.transform.position = Vector3.MoveTowards (elevator.transform.position, idlePosition, elevatorspeed * Time.deltaTime);
			yield return null;
		}
	}

	// Animate the doors to open
	IEnumerator open_elevator_doors() {

		dooropen = false;

	
		while (Vector3.Distance (rightdoor.transform.position, rightopenpos) > 0.005f) {

			rightdoor.transform.position = Vector3.MoveTowards (rightdoor.transform.position, rightopenpos, doorspeed * Time.deltaTime);
			leftdoor.transform.position = Vector3.MoveTowards (leftdoor.transform.position, leftopenpos, doorspeed * Time.deltaTime);

			yield return null;
		}


	
	}

	// Animate the doors to close
	IEnumerator close_elevator_doors() {
		
		doorclose = false;
		while (Vector3.Distance (rightdoor.transform.position, rightclosedpos) > 0.005f) {

			rightdoor.transform.position = Vector3.MoveTowards (rightdoor.transform.position, rightclosedpos, doorspeed * Time.deltaTime);
			leftdoor.transform.position = Vector3.MoveTowards (leftdoor.transform.position, leftclosedpos, doorspeed * Time.deltaTime);


			yield return null;
		}

		// start the courutine to leave and return to idle pos with booleancheck elevatorleavenow = true
		yield return new WaitForSeconds(2);
		elevatorleavenow = true;
	}




}


