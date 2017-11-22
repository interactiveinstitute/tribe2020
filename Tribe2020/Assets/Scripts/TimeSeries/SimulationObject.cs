using UnityEngine;
using System.Collections;

public class SimulationObject : MonoBehaviour {

	[Header("Not used yet but will in the future")]
	public GameTime Time = null;
	[SerializeField]
	double Next = double.PositiveInfinity;
	[SerializeField]
	double Prev = double.NegativeInfinity;

	bool registered = false;


	//This is called on updates form the time object
	virtual public bool UpdateSim(double time) {
		return false;
	}

	public bool register(){
		
		if (Time == null) 
			Time = GameTime.GetInstance ();

		registered = Time.register (this);

		return registered;
	}

	public void SetNext(double ts){
		Next = ts;
		if (registered)
			Time.UpdateNext (this, ts);


	}

	public double GetNext(){
		return Next;
	}

	public void SetPrev(double ts){
		Prev = ts;

		if (registered)
			Time.UpdatePrev (this, ts);
	}

	public double GetPrev(){
		return Prev;
	}

}
