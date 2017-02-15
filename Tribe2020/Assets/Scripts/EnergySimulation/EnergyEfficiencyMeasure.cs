using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EEM", menuName = "TRIBE/EnergyEfficiencyMeasure", order = 1)]
public class EnergyEfficiencyMeasure : ScriptableObject {
	[TextArea(1, 10)]
	public string title;
	[TextArea(3, 10)]
	public string description;
	public Color color;
	public string category;

	public int cashCost;
	public int comfortCost;

	public float energyFactor = 0;
	public float gasFactor = 0;
	public float co2Factor = 0;
	public float moneyFactor = 0;
	public float comfortFactor = 0;

	public string callback;
	public string callbackArgument;

	public string deactivateDeviceName;
	public string activateDeviceName;

	public List<EnergyEfficiencyMeasure> requires;
	public List<EnergyEfficiencyMeasure> excludes;

	//public int cashProduction;
	//public int comfortPorduction;

	public bool performed;
	public bool passive;
	public bool hidden;
}
