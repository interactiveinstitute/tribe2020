using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour{
	//Singleton features
	private static UIManager _instance;
	public static UIManager GetInstance(){
		return _instance;
	}

	private InteractionManager _ixnMgr;

	public Transform title;
	public Transform date;

	public Transform cash;
	public Transform comfort;
	public Transform temperature;
	public Transform power;
	public Transform co2;

	public Transform inspectorAction;
	public GameObject actionButton;

	public GameObject FeedbackNumber;

	//Sort use instead of constructor
	void Awake(){
		_instance = this;
	}

	//Use this for initialization
	void Start(){
		_ixnMgr = InteractionManager.GetInstance();
	}
	
	//Update is called once per frame
	void Update(){
	}

	//
	public void SetActions(List<BaseAction> actions){
		RemoveChildren(inspectorAction);
		
		foreach(BaseAction a in actions){
			Debug.Log("action: " + a.name);

			if(!a.performed){
				BaseAction curAction = a;
				GameObject actionObj;
				actionObj =
					Instantiate(actionButton, Vector3.zero, Quaternion.identity) as GameObject;
				actionObj.GetComponent<Button>().
					onClick.AddListener(()=> _ixnMgr.OnAction(curAction, actionObj));

				Text[] texts = actionObj.GetComponentsInChildren<Text>();
				texts[0].text = a.name;
				texts[1].text = "€" + a.cashCost;
				texts[2].transform.parent.gameObject.SetActive(false);

				if(a.cashProduction != 0){
					texts[3].text = a.cashProduction + "/s";
				} else {
					texts[3].transform.parent.gameObject.SetActive(false);
				}

				if(a.comfortPorduction != 0){
					texts[4].text = a.comfortPorduction + "/s";
				} else {
					texts[4].transform.parent.gameObject.SetActive(false);
				}

				texts[5].transform.parent.gameObject.SetActive(false);

				actionObj.transform.SetParent(inspectorAction, false);
			}
		}
	}

	//
	public void RemoveChildren(Transform parent){
		List<Transform> children = new List<Transform>();
		foreach(Transform child in parent){
			children.Add(child);
		}
		children.ForEach(child => Destroy(child.gameObject));
	}

	//
	public GameObject CreateFeedback(Vector3 pos, string feedback){
		GameObject fb = Instantiate(FeedbackNumber, pos, Quaternion.identity) as GameObject;
		fb.GetComponent<TextMesh>().text = feedback;
		return fb;
	}
}
