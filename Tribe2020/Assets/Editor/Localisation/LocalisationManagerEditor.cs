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
				for(int i = 0; i < script.template.groups.Count; i++) {
					if(i > l.groups.Count - 1) {
						l.groups.Add(script.template.groups[i]);
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
					for(int i = 0; i < g.values.Count; i++) {
						Language.KeyValue translatedValue = g.values[i];

						translatedValue.value = script.Translate(g.values[i].value, template.code, l.code);

						translatedValue.values = new List<string>();
						foreach(string v in g.values[i].values) {
							translatedValue.values.Add(script.Translate(v, template.code, l.code));
						}

						g.values[i] = translatedValue;
					}
				}
			}
		}
	}
}