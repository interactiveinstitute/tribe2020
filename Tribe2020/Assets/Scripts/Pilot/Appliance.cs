using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SimpleJSON;
using System.Collections;

public class Appliance : MonoBehaviour, IPointerClickHandler {
	private PilotController _ctrlMgr;

	public string title;
	public string description;
	public List<EnergyEfficiencyMeasure> playerAffordances;
    public List<AffordanceResource> avatarAffordances;
    public List<AffordanceResource> temporaryAvatarAffordances;
	//public List<string> owners;
    public List<BehaviourAI> owners;
	private Room _zone;

    [System.Serializable]
    public class PoseSlot
    {
        public Vector3 position;
        public Quaternion rotation;
        public BehaviourAI occupant;
    }

    [System.Serializable]
    public class AffordanceResource
    {
        public Affordance affordance;
        public int nrOfSlots;
        public List<AvatarActivity> subscribingActivities = new List<AvatarActivity>();

        public int usedSlots()
        {
            return subscribingActivities.Count;
        }

        public bool AddUser(AvatarActivity activity)
        {
            if (usedSlots() < nrOfSlots)
            {
                subscribingActivities.Add(activity);
                return true;
            }
            return false;
        }

        public bool RemoveUser(AvatarActivity activity)
        {
            if (usedSlots() > 0)
            {
                subscribingActivities.Remove(activity);
                return true;
            }
            return false;
        }

        public bool RemoveAllUsers()
        {
            foreach(AvatarActivity activity in subscribingActivities.ToArray())
            {
                activity.LoseAffordanceSlot();
                subscribingActivities.Remove(activity);
            }
            return false;
        }
    }

	public List<EnergyEfficiencyMeasure> appliedEEMs;
	public Vector3 interactionPos;
    public List<PoseSlot> posePositions = new List<PoseSlot>();
    public float cashProduction;
	public float comfortPorduction;

	public List<EEMMeta> possibleEEMs;

	public GameObject harvestButtonRef;
	private GameObject _harvestButton;

	[System.Serializable]
	public class EEMMeta {
		public string title;
		public EnergyEfficiencyMeasure eem;
		public bool applied;
	}

    void Awake()
    {
        InteractionPoint ip = GetComponentInChildren<InteractionPoint>();
        if (ip != null)
        {
            interactionPos = ip.transform.position;
        }
        else
        {
            DebugManager.Log("didn't find interaction point for " + this.title + " with name " + this.name + ", usig the gameObjects transform instead", this);
            interactionPos = transform.position;
        }
    }

	// Use this for initialization
	void Start() {
		_ctrlMgr = PilotController.GetInstance();

		_harvestButton = Instantiate(harvestButtonRef) as GameObject;
		_harvestButton.transform.SetParent(transform, false);
		_harvestButton.transform.localPosition = Vector3.up * 0.5f;

		_harvestButton.GetComponentInChildren<Button>().
				onClick.AddListener(() => _ctrlMgr.OnHarvestTap(_harvestButton));

		_zone = GetComponentInParent<Room>();

		harvestButtonRef.SetActive(false);

		appliedEEMs = new List<EnergyEfficiencyMeasure>();

        //Setting the posePositions for this appliance. Retrieving them from the transforms of the PosePoint components in the gameobject.
        PosePoint[] poseArray = GetComponentsInChildren<PosePoint>();
        //if (posePositions == null)
        //{
        //    posePositions = new List<PoseSlot>();
        //}
        foreach (PosePoint point in poseArray)
        {
            PoseSlot item = new PoseSlot();
            item.position = point.transform.position;
            item.rotation = point.transform.rotation;
            posePositions.Add(item);
        }
	}
	
	// Update is called once per frame
	void Update() {
	}

	//
	public void OnPointerClick(PointerEventData eventData) {
		_ctrlMgr.SetCurrentUI(this);
	}

	//
	public void ApplyEEM(EnergyEfficiencyMeasure eem) {
		appliedEEMs.Add(eem);
	}

	//
	public List<EnergyEfficiencyMeasure> GetEEMs() {
		return playerAffordances;
	}

	//
	public void OnUsage(Affordance affordance) {
		AddHarvest();
	}

	//
	public void AddHarvest() {
		_harvestButton.SetActive(true);
	}

    public List<Appliance.AffordanceResource> GetTemporaryAvatarAffordances()
    {
        return temporaryAvatarAffordances;
    }

    public void SetTemporaryAvatarAffordances(List<Appliance.AffordanceResource> affordances)
    {
        temporaryAvatarAffordances = affordances;
    }

    public void ClearTemporaryAvatarAffordances()
    {
        temporaryAvatarAffordances.Clear();
    }

    public bool DecreaseNrOfAffordanceSlots(Affordance affordance, int count = 1)
    {
        for(int i = 0; i < avatarAffordances.Count; i++)
        {
            if(avatarAffordances[i].affordance == affordance)
            {
                AffordanceResource resource = avatarAffordances[i];

                int margin = resource.nrOfSlots - resource.usedSlots();
                int mustBeRemoved = count - margin;

                if (mustBeRemoved < 0)
                    mustBeRemoved = 0;

                for(int j = 0; j < mustBeRemoved; j++)
                {
                    resource.subscribingActivities[0].LoseAffordanceSlot();
                }

                resource.nrOfSlots -= count;
                if(resource.nrOfSlots < 0)
                {
                    resource.nrOfSlots = 0;
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    public bool IncreaseNrOfAffordanceSlots(Affordance affordance, int count = 1)
    {
        for (int i = 0; i < avatarAffordances.Count; i++)
        {
            if (avatarAffordances[i].affordance == affordance)
            {
                avatarAffordances[i].nrOfSlots += count;
                return true;
            }
        }
        return false;
    }

    public bool RemoveAllUsersOfAffordance(Affordance affordance)
    {
        foreach(AffordanceResource affordanceResource in avatarAffordances)
        {
            if(affordanceResource.affordance == affordance)
            {
                return affordanceResource.RemoveAllUsers();

            }
        }
        return false;
    }

    public bool TakeAffordanceSlot(Affordance affordance, AvatarActivity activity)
    {
        foreach (AffordanceResource affordanceResource in avatarAffordances)
        {
            if (affordanceResource.affordance == affordance)
            {
                return affordanceResource.AddUser(activity);
            }
        }
        return false;
    }

    public bool ReleaseAffordanceSlot(Affordance affordance, AvatarActivity activity)
    {
        foreach(AffordanceResource affordanceResource in avatarAffordances)
        {
            if(affordanceResource.affordance == affordance)
            {
                return affordanceResource.RemoveUser(activity);
            }
        }
        return false;
    }

    //Releases all pose slots that are currently occupied by the supplied occupant
    public void ReleasePoseSlot(BehaviourAI occupant)
    {
        foreach(PoseSlot slot in posePositions)
        {
            if(slot.occupant == occupant)
            {
                slot.occupant = null;
            }
        }
    }

	//Get the zone where the appliance iz @
	public Room GetZone() {
		return _zone;
	}

	//
	public JSONClass SerializeAsJSON() {
		JSONClass json = new JSONClass();

		json.Add("id", GetComponent<UniqueId>().uniqueId);
		json.Add("title", title);

		JSONArray eemsJSON = new JSONArray();
		foreach(EnergyEfficiencyMeasure eem in appliedEEMs) {
			JSONClass eemJSON = new JSONClass();
			eemJSON.Add("title", eem.title);
			eemsJSON.Add(eemJSON);
		}
		json.Add("appliedEEMs", eemsJSON);

		return json;
	}

	//
	public void DeserializeFromJSON(JSONClass json) {
		if(json != null) {
			JSONArray eemsJSON = json["appliedEEMs"].AsArray;
			foreach(JSONClass appliedEEMJSON in eemsJSON) {
				foreach(EnergyEfficiencyMeasure eem in playerAffordances) {
					if(eem.title.Equals(appliedEEMJSON["title"])) {
						ApplyEEM(eem);
					}
				}
			}
		}
	}
}
