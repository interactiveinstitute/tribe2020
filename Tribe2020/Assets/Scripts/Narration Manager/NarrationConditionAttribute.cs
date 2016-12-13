using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class NarrationConditionAttribute : PropertyAttribute {
	//The name of the bool field that will be in control
	public string parameter = "";
	////TRUE = Hide in inspector / FALSE = Disable in inspector 
	////public bool HideInInspector = false;

	public NarrationConditionAttribute() {
		//this.ConditionalSourceField = conditionalSourceField;
		//this.HideInInspector = false;
	}

	public NarrationConditionAttribute(string parameter) {
		this.parameter = parameter;
		//this.HideInInspector = false;
	}

	//public NarrationStepAttribute(string conditionalSourceField, bool hideInInspector) {
	//	this.ConditionalSourceField = conditionalSourceField;
	//	this.HideInInspector = hideInInspector;
	//}
}
