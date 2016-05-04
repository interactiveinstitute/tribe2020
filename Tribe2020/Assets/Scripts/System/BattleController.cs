using UnityEngine;

public class BattleController : MonoBehaviour {
	//Singleton features
	private static BattleController _instance;
	public static BattleController GetInstance() {
		return _instance;
	}

	private BattleView _view;
	private CustomSceneManager _sceneMgr;

	public GameObject foeObject;
	public GameObject allyObject;

	private int foeCP = 100;
	private int foeEP = 100;

	private int allyCP = 100;
	private int allyEP = 100;

	//Sort use instead of constructor
	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		_view = BattleView.GetInstance();
		_sceneMgr = CustomSceneManager.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
		_view.foeCPNumber.text = foeCP + "/100";
		_view.foeEPNumber.text = foeEP + "/100";
		_view.foeCPBar.fillAmount = foeCP / 100f;
		_view.foeEPBar.fillAmount = foeEP / 100f;

		_view.allyCPNumber.text = allyCP + "/100";
		_view.allyEPNumber.text = allyEP + "/100";
		_view.allyCPBar.fillAmount = allyCP / 100f;
		_view.allyEPBar.fillAmount = allyEP / 100f;
	}

	//
	public void OnArguePressed() {
		int damage = Random.Range(10, 20);
		_view.CreateFeedback(foeObject.transform.position, "" + damage);
		foeCP = Mathf.Max(foeCP - damage, 0);
		if(foeCP == 0) {
			OnWin();
		}
	}

	//
	public void OnItemPressed() {
	}

	//
	public void OnSurrenderPressed() {
	}

	//
	public void OnWin() {
		_sceneMgr.LoadScene("ga_madrid_erik");
	}
}
