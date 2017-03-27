using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LocalisationManager))]
public class LocalisationManagerEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		LocalisationManager script = (LocalisationManager)target;


		if(GUILayout.Button("Apply Template", GUILayout.Width(100))) {
			ApplyTemplate();
		}

		if(GUILayout.Button("Translate", GUILayout.Width(100))) {
			Translate();
		}


	}

	//
	public void ApplyTemplate() {
		LocalisationManager script = (LocalisationManager)target;

		if(!script.template) { return; }

		foreach(Language l in script.languages) {
			//Don't apply on template
			if(l != script.template) {
				List<Language.ValueGroup> tmpGroups = script.template.groups;
				for(int i = 0; i < tmpGroups.Count; i++) {
					if(i > l.groups.Count - 1) {
						//Language too small, add template group
						l.groups.Add(tmpGroups[i]);
					} else if(tmpGroups[i].title != l.groups[i].title) {
						//No matching group at index, search and replace or insert, then check values
						bool groupExisted = false;
						foreach(Language.ValueGroup g in l.groups) {
							if(tmpGroups[i].title == g.title) {
								Language.ValueGroup tmpG = g;
								l.groups.Remove(g);
								l.groups.Insert(i, tmpG);
								groupExisted = true;
							}
						}
						if(!groupExisted) {
							l.groups.Insert(i, tmpGroups[i]);
						}
						ApplyTemplateGroup(tmpGroups[i], l.groups[i]);
					} else if(tmpGroups[i].title == l.groups[i].title) {
						//Group exists, just check values
						ApplyTemplateGroup(tmpGroups[i], l.groups[i]);
					}
				}

				EditorUtility.SetDirty(l);
				AssetDatabase.SaveAssets();
			}
		}
	}

	//Modify a valuegroup to match a template
	public void ApplyTemplateGroup(Language.ValueGroup template, Language.ValueGroup other) {
		List<Language.KeyValue> tValues = template.values;
		List<Language.KeyValue> oValues = other.values;
		for(int i = 0; i < tValues.Count; i++) {
			if(i > oValues.Count - 1) {
				//Values ended before iterating all template values
				oValues.Add(tValues[i]);
			} else if(tValues[i].key != oValues[i].key) {
				//No matching keyvalue at index, search and replace or insert, then check values
				bool valueExisted = false;
				foreach(Language.KeyValue kv in oValues) {
					if(tValues[i].key == kv.key) {
						Language.KeyValue tmpKV = kv;
						oValues.Remove(kv);
						oValues.Insert(i, tmpKV);
						valueExisted = true;
					}
				}
				if(!valueExisted) {
					oValues.Insert(i, tValues[i]);
				}
				ApplyTemplateValues(tValues[i], oValues[i]);
				//||s
				//tValues[i].values.Count != oValues[i].values.Count) {
				////Language value does not match template
				//oValues.Insert(i, tValues[i]);
			} else if(tValues[i].key == oValues[i].key) {
				ApplyTemplateValues(tValues[i], oValues[i]);
			}
		}
	}

	//Modify values to match template valuegroups
	public void ApplyTemplateValues(Language.KeyValue template, Language.KeyValue other) {
		//Add fields to values if missing
		for(int i = 0; i < template.values.Count; i++) {
			if(i > other.values.Count) {
				other.values.Add("");
			}
		}
	}

	//
	public void Translate() {
		LocalisationManager script = (LocalisationManager)target;
		Language template = script.template;

		foreach(Language l in script.languages) {
			//Don't translate the template
			if(l != template) {
				foreach(Language.ValueGroup g in l.groups) {
					if(!g.translated) {
						for(int i = 0; i < g.values.Count; i++) {
							Language.KeyValue translatedValue = g.values[i];

							translatedValue.value = script.Translate(g.values[i].value, template.code, l.code);

							translatedValue.values = new List<string>();
							foreach(string v in g.values[i].values) {
								translatedValue.values.Add(script.Translate(v, template.code, l.code));
							}

							g.values[i] = translatedValue;
						}
						g.translated = true;
					}
				}

				EditorUtility.SetDirty(l);
				AssetDatabase.SaveAssets();
			}
		}
	}
}