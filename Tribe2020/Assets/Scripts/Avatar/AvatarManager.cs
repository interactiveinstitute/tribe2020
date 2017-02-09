using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class AvatarManager : MonoBehaviour {
	//Singleton hack
	private static AvatarManager _instance;
	public static AvatarManager GetInstance() {
		return _instance;
	}

    public enum Gender { Male, Female }

	[SerializeField]
	private List<BehaviourAI> _avatars;
	public AvatarConversation conversation;
    public AvatarLooks looks;
	public Sprite playerPortrait;

	//
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_avatars = new List<BehaviourAI>(UnityEngine.Object.FindObjectsOfType<BehaviourAI>());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public void OnLightToggled(Room zone, bool lightOn) {
		List<BehaviourAI> occupants = zone.GetOccupants();

        if (!lightOn) {
            foreach (BehaviourAI occupant in occupants) {
                occupant.gameObject.GetComponent<AvatarMood>().SetMood(AvatarMood.Mood.angry);
            }
        }

        if (occupants.Count == 0) return;
		//Let's try to only notify one avatar. So we don't get several characters running for the light switch
		occupants[0].CheckLighting(AvatarActivity.SessionType.TurnOn);
	}

	//
	public void ControlAvatar(string id, string action, Vector3 pos) {
		foreach(BehaviourAI avatar in _avatars) {
			if(avatar.name == id) {
				avatar.TakeControlOfAvatar();
				avatar.WalkTo(pos);
			}
		}
	}

	//
	public void ControlAvatar(string id, UnityEngine.Object action) {
		foreach(BehaviourAI avatar in _avatars) {
			if(avatar.name == id) {
				AvatarActivity newAct = UnityEngine.ScriptableObject.Instantiate<AvatarActivity>(action as AvatarActivity);
				avatar.StartTemporaryActivity(newAct);
			}
		}
	}

	//
	public AvatarStats GetAvatar(string name) {
		foreach(BehaviourAI ai in _avatars) {
			if(ai.GetComponent<Appliance>().name.Equals(name)) {
				return ai.GetComponent<AvatarStats>();
			}
		}
		return null;
	}

	//Serialize state as json for a save file
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		JSONArray avatarsJSON = new JSONArray();
		foreach(BehaviourAI avatar in _avatars) {
			avatarsJSON.Add(avatar.Encode());
		}

		json.Add("avatars", avatarsJSON);
		return json;
	}

	//Deserialize json and apply states to aspects handled by the manager
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			JSONArray avatarsJSON = json["avatars"].AsArray;

			foreach(JSONClass avatarJSON in avatarsJSON) {
				foreach(BehaviourAI avatar in _avatars) {
					string loadedName = avatarJSON["name"];
					if(avatar.name == loadedName) {
						avatar.Decode(avatarJSON);
					}
				}
			}
		}
	}
}
