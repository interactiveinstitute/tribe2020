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
			if(l != script.template) {
				List<Language.ValueGroup> tmpGroups = script.template.groups;
				for(int i = 0; i < tmpGroups.Count; i++) {
					if(i < l.groups.Count && tmpGroups[i].title != l.groups[i].title) {
						//matching group does not exist on same index as template
						l.groups.Insert(i, tmpGroups[i]);
					} else if(i > l.groups.Count - 1) {
						//groups ended before iterating all template groups
						l.groups.Add(tmpGroups[i]);
					} else if(tmpGroups[i].title == l.groups[i].title) {
						//group exists, check values
						List<Language.KeyValue> tmpValues = tmpGroups[i].values;
						List<Language.KeyValue> lngValues = l.groups[i].values;
						for(int v = 0; v < tmpValues.Count; v++) {
							if(v < lngValues.Count && tmpValues[v].value != lngValues[v].value) {
								//language value does not match template
								lngValues.Insert(v, tmpValues[v]);
							} else if(i > l.groups.Count - 1) {
								//values ended before iterating all template values
								lngValues.Add(tmpValues[v]);
							}
						}
					}
				}

				EditorUtility.SetDirty(l);
				AssetDatabase.SaveAssets();
			}
		}
	}

	//
	public void Translate() {
		LocalisationManager script = (LocalisationManager)target;
		Language template = script.template;

		foreach(Language l in script.languages) {
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