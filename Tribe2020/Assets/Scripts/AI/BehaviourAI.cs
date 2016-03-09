using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BehaviourAI : MonoBehaviour {
	private GameTime _timeMgr;

	private NavMeshAgent _agent;
	private GameObject _curTarget;
	private float _thirst;
	private float _need;
	private float _temperature;

	private List<AgentBehavior> _behaviors = new List<AgentBehavior>();
	private BaseBehaviour _curBehavior;
	public GameObject[] _appliances;

	private Transform _toilet, _coffee;

	private const string COFFEE = "Appliance";
	private const string TOILET = "toilet";


	public float heating_behaviour;
	public float cooling_behaviour;
	public float lighting_behaviour;
	public float appliance_behaviour;
	public float attitude;

	public List<string> schedule;
	private int _curActivityIndex = 0;

	public List<BaseBehaviour> behaviours;
	public string curState = "idle";
	public float delay = 0;

	// Use this for initialization
	void Start() {
		_timeMgr = GameTime.GetInstance();

		_agent = GetComponent<NavMeshAgent>();
		_thirst = 100;
		_need = 0;
		_temperature = 25;

		_curBehavior = behaviours[0];
		_curBehavior.Init(this);

		//behaviours.Add(new Idle(0.25f));
		//behaviours.Add(new DrinkCoffee(0.25f));
		//behaviours.Add(new UseBathroom(0.25f));
		//behaviours.Add(new WarmUp(0.25f));
	}

	// Update is called once per frame
	void Update() {
		float curTime = _timeMgr.GetDateTime().Hour * 60 + _timeMgr.GetDateTime().Minute;

		ScheduleItem curItem = ParseScheduleActivity(schedule[_curActivityIndex]);

		//Skip old schedule items
		while(curTime > curItem.time + 30) {
			_curActivityIndex++;
			curItem = ParseScheduleActivity(schedule[_curActivityIndex]);
		}

		_curBehavior.Step(this);

		if(_curTarget &&
			Vector3.Distance(transform.position, _curTarget.transform.position) < 2) {
			_curBehavior.OnHasReached(this, _curTarget.tag);
		}



		//	if(curTime > curActivityTime + 30) {
		//	_curActivityIndex++;
		//	Debug.Log("skipping that " + curActivity);
		//} else if(curTime > curActivityTime + 30) {
		//}


		//Debug.Log("cur time: " + (_timeMgr.GetDateTime().Hour * 60 + _timeMgr.GetDateTime().Minute));
		//Debug.Log("cur schedule time: " + curActivityTime);
	}

	//
	public ScheduleItem ParseScheduleActivity(string scheduleItem) {
		ScheduleItem item = new ScheduleItem();

		string[] scheduleParse = scheduleItem.Split(',');
		item.activity = scheduleParse[1];
		string[] curActivityTimeParse = scheduleParse[0].Split(':');
		item.time = int.Parse(curActivityTimeParse[0]) * 60 + int.Parse(curActivityTimeParse[1]);

		return item;
	}

	public void GoTo(string tag) {
		Debug.Log("Going for a place to " + tag);
		_curTarget = FindNearestObject(tag);

		if(_curTarget == null) {
			return;
		}

		_agent.SetDestination(_curTarget.transform.position);
	}

	public GameObject FindNearestObject(string tag) {
		GameObject target = null;
		float minDist = float.MaxValue;

		foreach(GameObject app in _appliances) {
			List<string> affordances = app.GetComponent<Affordance>().affordances;

			//Debug.Log("Affordances: " + affordances[0]);

			if(affordances.Contains(tag)) {
				float dist = Vector3.Distance(transform.position, app.transform.position);
				if(dist < minDist) {
					minDist = dist;
					target = app;
				}
			}
		}

		//if(GameObject.FindGameObjectsWithTag("Appliance").Length == 0){
		//	return null;
		//}

		//GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
		//GameObject target = gos[0];
		//float minDist = float.MaxValue;

		//foreach(GameObject go in gos){
		//	float dist = Vector3.Distance(transform.position, go.transform.position);
		//	if(dist < minDist){
		//		minDist = dist;
		//		target = go;
		//	}
		//}

		return target;
	}

	public void OnBehaviorOver() {
		curState = "idle";
		delay = 0;
		//_curBehavior = _behaviors[Random.Range(0, _behaviors.Count)];
		//_curBehavior.Start();

		//		Debug.Log ("done, now: " + _curBehavior);
	}
}

public struct ScheduleItem {
	public int time;
	public string activity;
}
