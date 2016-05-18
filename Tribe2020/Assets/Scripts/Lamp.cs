using UnityEngine;
using System.Collections;

public class Lamp : ElectricDevice {

	[Tooltip("All light emitters to be controlled by the on and off.")]
	public Light[] lights;
	[Tooltip("If this is checked the light sources to be controlled will be found autmatically on start.")]
	public bool AutoFindLights;

	// Use this for initialization
	void Awake () {
		if (AutoFindLights)
			AutoFindLightSources ();
		
	}

	void AutoFindLightSources () {
		lights = GetComponentsInChildren<Light>();

	}

	// Update is called once per frame
	void Update () {
		if (continous_updates) {
			update_energy ();
		}
	}

	public override void SetRunlevel(int level) {
		base.SetRunlevel(level);

		if (runlevel >= runlevelOn)
			TurnOnLights();
		else
			TurnOffLights();
	}


	private void TurnOnLights() {
		foreach (Light l in lights) {
			l.enabled = true;
		}
	}

	private void TurnOffLights() {
		foreach (Light l in lights) {
			l.enabled = false;
		}
	}
}
