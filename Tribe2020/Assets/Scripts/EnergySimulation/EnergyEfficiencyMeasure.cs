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
	public Sprite icon;

	//private Appliance _relatedAppliance;
	[Header("Cost")]
	public int cashCost;
	public int comfortCost;

	[Header("Effects on Appliance")]
	public float energyModifier = 1;

	[Header("Effects on Baseline")]
	public float baseEnergyModifier = 1;
	public float baseGasModifier = 1;

	[Header("Special Effects")]
	public string callback;
	public string callbackArgument;
    public Affordance callbackAffordance;

    public GameObject replacementPrefab;

    [Header("Energy effeciency")]
    public bool setEnergyEffeciency;
    public EnergyEffeciencyLabels.Name energyEffeciency;

    [Header("Time Limitation")]
	public double discoveryTime;
	public double obsoletionTime;

	[Header("Dependencies")]
	public List<EnergyEfficiencyMeasure> requires;
	public List<EnergyEfficiencyMeasure> excludes;

	public bool performed = false;
	public bool passive = false;
	public bool hidden = false;

    public string shouldRenderCallback = "";

    public bool multipleUse = false;

    public bool IsAffordable(float money, float comfort) {
		return cashCost <= money && comfortCost <= comfort;
	}
}
