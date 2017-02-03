using UnityEngine;
using System.Collections;

public class DoorOpenLeft : MonoBehaviour
{
	public string tag = "Player";
	public GameObject door;
	public GameObject triggerA;
	public float closeDoorDist = 3f;
	public float doorClosingSpeed = 0.3f;
	float doorOpenAngle = 90.0f;
	float doorCloseAngle = 0.0f;
	private Quaternion doorOpen = Quaternion.identity;
	private Quaternion doorClose = Quaternion.identity;
	private bool enter = false;
	private Transform playerTrans = null;


	void Start(){
		// init door Quaternions and player tag once
		doorOpen = Quaternion.Euler (0, doorOpenAngle, 0);
		doorClose = Quaternion.Euler (0, doorCloseAngle, 0);
		playerTrans = GameObject.FindWithTag(tag).transform;
	}


	void  OnTriggerEnter ( Collider other  )
	{
		if (other.gameObject.tag == tag)
		{
			enter = true;
			// disable trigger on other side of door
			triggerA.transform.GetComponent<Collider>().enabled = false;
		}
	}


	void Update()
	{
		if (enter == true)
		{
			// Courutine takes property (Quaternion dest)
			StartCoroutine(openDoor(doorOpen));

		}
		// Check if distance between player and Trigger is further away than CloseDoordist, if true start Coroutine
		if (Vector3.Distance(playerTrans.position, this.transform.position) > closeDoorDist) 
		{
			// Courutine takes property (Quaternion dest)
			StartCoroutine(closeDoor(doorClose));
		}  

	}

	IEnumerator openDoor(Quaternion dest) {
		
		// Check if angle less than 1 before stop rotating door
		while (Quaternion.Angle (door.transform.localRotation, dest) > 1.0f) {
			door.transform.localRotation = Quaternion.Slerp (door.transform.localRotation, dest, Time.deltaTime * doorClosingSpeed);


			yield return null;
		}

		enter = false;






	}

	IEnumerator closeDoor(Quaternion dest) {

		// Check if angle less than 1 before stop rotating door
		while (Quaternion.Angle (door.transform.localRotation, dest) > 1.0f) {
			door.transform.localRotation = Quaternion.Slerp(door.transform.localRotation, dest, Time.deltaTime * doorClosingSpeed);


			yield return null;
		}
		// enable trigger on other side of door
		triggerA.transform.GetComponent<Collider>().enabled = true;

	}
		
}
