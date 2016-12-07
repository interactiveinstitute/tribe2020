using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObjectInteraction : MonoBehaviour {
	private PilotController _controller;

	public Transform panelPrefab;
	private Transform _interactionPanel;

	private ElectricMeter _meter;

	private Button _powerButton;
	private Button _moneyButton;
	private Button _satisfactionButton;
	private Button _measureButton;

	public bool canTogglePower;
	public bool canHarvestMoney;
	public bool canHarvestSatisfaction;
	public bool canPerformMeasure;

	// Use this for initialization
	void Start () {
		_controller = PilotController.GetInstance();

		_interactionPanel = Instantiate(panelPrefab);
		_interactionPanel.SetParent(transform, false);
		_interactionPanel.transform.localPosition = Vector3.forward * 0.5f;

		_powerButton = _interactionPanel.GetChild(0).GetComponent<Button>();
		_moneyButton = _interactionPanel.GetChild(1).GetComponent<Button>();
		_satisfactionButton = _interactionPanel.GetChild(2).GetComponent<Button>();
		_measureButton = _interactionPanel.GetChild(3).GetComponent<Button>();

		if(GetComponent<ElectricMeter>()) {
			_meter = GetComponent<ElectricMeter>();
			_powerButton.onClick.AddListener(() => ToggleMeter());
		}

		//_moneyButton.onClick.AddListener(() => _controller.OnHarvestTap(gameObject));
	}
	
	// Update is called once per frame
	void Update () {
		_powerButton.gameObject.SetActive(canTogglePower);
		_moneyButton.gameObject.SetActive(canHarvestMoney);
		_satisfactionButton.gameObject.SetActive(canHarvestSatisfaction);
		_measureButton.gameObject.SetActive(canPerformMeasure);
	}

	//
	private void ToggleMeter() {
		_meter.Toggle();
		_controller.OnLightSwitchToggled(_meter);
	}
}
