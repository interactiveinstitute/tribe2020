using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EEMButton : MonoBehaviour {
	public Text title;
	public GameObject costPanel;
	public GameObject performedPanel;
	public GameObject moneyCost;
	public GameObject comfortCost;
	public GameObject powerImpact;
	public GameObject gasImpact;
	public GameObject co2Impact;
	public GameObject moneyImpact;
	public GameObject comfortImpact;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//
	public void SetPerformed(bool isPerformed) {
		costPanel.SetActive(!isPerformed);
		performedPanel.SetActive(isPerformed);
	}

	//
	public void SetCost(int money, int comfort) {
		moneyCost.SetActive(money != 0);
		comfortCost.SetActive(comfort != 0);

		moneyCost.GetComponentInChildren<Text>().text = "" + money;
		comfortCost.GetComponentInChildren<Text>().text = "" + comfort;
	}

	//
	public void SetImpact(int power, int gas, int co2, int money, int comfort) {
		powerImpact.SetActive(power != 0);
		gasImpact.SetActive(gas != 0);
		co2Impact.SetActive(co2 != 0);
		moneyImpact.SetActive(money != 0);
		comfortImpact.SetActive(comfort != 0);

		powerImpact.GetComponentInChildren<Text>().text = "-" + power + "%";
		gasImpact.GetComponentInChildren<Text>().text = "-" + gas + "%";
		co2Impact.GetComponentInChildren<Text>().text = "-" + co2 + "%";
		moneyImpact.GetComponentInChildren<Text>().text = "+" + money;
		comfortImpact.GetComponentInChildren<Text>().text = "+" + comfort;
	}
}
