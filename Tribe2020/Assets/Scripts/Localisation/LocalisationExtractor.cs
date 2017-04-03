﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalisationExtractor : MonoBehaviour {
	public string key;
	public GameObject source;
	public List<Language.ValueGroup> groups;

	// Use this for initialization
	void Start() {
	}
	
	// Update is called once per frame
	void Update() {
	}

	//Search object for extractable text and prepare for insertion into a localization template
	public void ExtractText() {
		if(!source) {
			source = gameObject;
		}

		groups.Clear();

		//General wrapper
		if(source.GetComponentsInChildren<Text>().Length > 0) {
			List<Language.KeyValue> values = new List<Language.KeyValue>();
			foreach(Text t in source.GetComponentsInChildren<Text>()) {
				values.Add(new Language.KeyValue(t.name, t.text));
			}
			groups.Add(new Language.ValueGroup(key, values));
		}

		//Narrative wrapper
		if(source.GetComponent<NarrationManager>()) {
			foreach(Narrative n in gameObject.GetComponent<NarrationManager>().allNarratives) {
				List<Language.KeyValue> values = new List<Language.KeyValue>();
				foreach(Narrative.Step ns in n.steps) {
					if(ns.unityEvent.GetPersistentMethodName(0) == "ShowPrompt" ||
						ns.unityEvent.GetPersistentMethodName(0) == "ShowMessage" ||
						ns.unityEvent.GetPersistentMethodName(0) == "ShowCongratulations") {
						values.Add(new Language.KeyValue(ns.description, ns.textValue));
					}
				}
				groups.Add(new Language.ValueGroup("Narrative." + n.title, values));
			}
		}

		//Viewpoint wrapper
		if(source.GetComponentsInChildren<Viewpoint>().Length > 0) {
			List<Language.KeyValue> values = new List<Language.KeyValue>();
			foreach(Viewpoint vp in source.GetComponentsInChildren<Viewpoint>()) {
				values.Add(new Language.KeyValue(vp.name, vp.title));
			}
			groups.Add(new Language.ValueGroup(key, values));
		}

		//EEM wrapper
		if(source.GetComponent<EnergyEfficiencyMeasureContainer>()) {
			EnergyEfficiencyMeasureContainer eemContainer = source.GetComponent<EnergyEfficiencyMeasureContainer>();

			List<Language.KeyValue> envelopeValues = new List<Language.KeyValue>();
			foreach(EnergyEfficiencyMeasure eem in eemContainer.envelope) {
				List<string> values = new List<string>();
				values.Add(eem.title);
				values.Add(eem.description);
				envelopeValues.Add(new Language.KeyValue(eem.name, eem.title, values));
			}
			groups.Add(new Language.ValueGroup("EEM.Envelope", envelopeValues));

			List<Language.KeyValue> hvacValues = new List<Language.KeyValue>();
			foreach(EnergyEfficiencyMeasure eem in eemContainer.HVAC) {
				List<string> values = new List<string>();
				values.Add(eem.title);
				values.Add(eem.description);
				hvacValues.Add(new Language.KeyValue(eem.name, eem.title, values));
			}
			groups.Add(new Language.ValueGroup("EEM.HVAC", hvacValues));

			List<Language.KeyValue> dhwValues = new List<Language.KeyValue>();
			foreach(EnergyEfficiencyMeasure eem in eemContainer.DHW) {
				List<string> values = new List<string>();
				values.Add(eem.title);
				values.Add(eem.description);
				dhwValues.Add(new Language.KeyValue(eem.name, eem.title, values));
			}
			groups.Add(new Language.ValueGroup("EEM.DHW", dhwValues));

			List<Language.KeyValue> lightingValues = new List<Language.KeyValue>();
			foreach(EnergyEfficiencyMeasure eem in eemContainer.lighting) {
				List<string> values = new List<string>();
				values.Add(eem.title);
				values.Add(eem.description);
				lightingValues.Add(new Language.KeyValue(eem.name, eem.title, values));
			}
			groups.Add(new Language.ValueGroup("EEM.Lighting", lightingValues));

			List<Language.KeyValue> electricDeviceValues = new List<Language.KeyValue>();
			foreach(EnergyEfficiencyMeasure eem in eemContainer.electricDevices) {
				List<string> values = new List<string>();
				values.Add(eem.title);
				values.Add(eem.description);
				electricDeviceValues.Add(new Language.KeyValue(eem.name, eem.title, values));
			}
			groups.Add(new Language.ValueGroup("EEM.ElectDevices", electricDeviceValues));

			List<Language.KeyValue> otherValues = new List<Language.KeyValue>();
			foreach(EnergyEfficiencyMeasure eem in eemContainer.other) {
				List<string> values = new List<string>();
				values.Add(eem.title);
				values.Add(eem.description);
				otherValues.Add(new Language.KeyValue(eem.name, eem.title, values));
			}
			groups.Add(new Language.ValueGroup("EEM.Other", otherValues));

			List<Language.KeyValue> specialValues = new List<Language.KeyValue>();
			foreach(EnergyEfficiencyMeasure eem in eemContainer.special) {
				List<string> values = new List<string>();
				values.Add(eem.title);
				values.Add(eem.description);
				specialValues.Add(new Language.KeyValue(eem.name, eem.title, values));
			}
			groups.Add(new Language.ValueGroup("EEM.Special", specialValues));
		}

		//Appliance & Avatar wrapper
		if(source.GetComponent<ApplianceManager>()) {
			List<Language.KeyValue> appValues = new List<Language.KeyValue>();
			List<Language.KeyValue> avatarValues = new List<Language.KeyValue>();

			Appliance[] apps = Object.FindObjectsOfType<Appliance>();
			List<string> appInventory = new List<string>();
			foreach(Appliance a in apps) {
				if(a.GetComponent<BehaviourAI>()) {
					List<string> values = new List<string>();
					values.Add(a.description);
					avatarValues.Add(new Language.KeyValue(a.title, a.title, values));
				} else {
					if(!appInventory.Contains(a.title)) {
						appInventory.Add(a.title);
						List<string> values = new List<string>();
						values.Add(a.description);
						appValues.Add(new Language.KeyValue(a.title, a.title, values));
					}
				}
			}
			groups.Add(new Language.ValueGroup("Content.Avatars", avatarValues));
			groups.Add(new Language.ValueGroup("Content.Appliances", appValues));
		}
	}
}