using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour {
	//Singleton features
	private static QuestManager _instance;
	public static QuestManager GetInstance() {
		return _instance;
	}

	public List<Quest> quests;
	private List<Quest> _activeQuests;

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_activeQuests = new List<Quest>();
		_activeQuests.Add(quests[0]);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public List<Quest> GetQuests() {
		return _activeQuests;
	}
}
