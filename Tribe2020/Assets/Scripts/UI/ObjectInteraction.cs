﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObjectInteraction : MonoBehaviour {
	public Transform panelPrefab;
	private Transform _interactionPanel;

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
		_interactionPanel = Instantiate(panelPrefab);
		_interactionPanel.SetParent(transform, false);

		_powerButton = _interactionPanel.GetChild(0).GetComponent<Button>();
		_moneyButton = _interactionPanel.GetChild(1).GetComponent<Button>();
		_satisfactionButton = _interactionPanel.GetChild(2).GetComponent<Button>();
		_measureButton = _interactionPanel.GetChild(3).GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
		_powerButton.gameObject.SetActive(canTogglePower);
		_moneyButton.gameObject.SetActive(canHarvestMoney);
		_satisfactionButton.gameObject.SetActive(canHarvestSatisfaction);
		_measureButton.gameObject.SetActive(canPerformMeasure);
	}
}
