using UnityEngine;
using System;
using System.Collections;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class NarrationStepAttribute : PropertyAttribute {
	//The name of the bool field that will be in control
	public string parameter = "";
	public bool isTitle = false;
	////TRUE = Hide in inspector / FALSE = Disable in inspector 
	////public bool HideInInspector = false;

	public NarrationStepAttribute(string parameter, bool isTitle) {
		this.parameter = parameter;
		this.isTitle = isTitle;
		//this.HideInInspector = false;
	}

	public NarrationStepAttribute(string parameter) {
		this.parameter = parameter;
		//this.HideInInspector = false;
	}

	//public NarrationStepAttribute(string conditionalSourceField, bool hideInInspector) {
	//	this.ConditionalSourceField = conditionalSourceField;
	//	this.HideInInspector = hideInInspector;
	//}
}
