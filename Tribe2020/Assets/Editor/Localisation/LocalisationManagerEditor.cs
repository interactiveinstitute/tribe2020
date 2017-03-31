using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LocalisationManager))]
public class LocalisationManagerEditor : Editor {

	//Add elements while drawing the inspector
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		LocalisationManager script = (LocalisationManager)target;


		if(GUILayout.Button("Extract Text to Template", GUILayout.Width(175))) {
			ExtractTextToTemplate();
		}

		if(GUILayout.Button("Apply Template", GUILayout.Width(100))) {
			ApplyTemplate();
		}

		if(GUILayout.Button("Translate", GUILayout.Width(100))) {
			Translate();
		}
	}

	//Find all localization extractor in the scene and add their value groups to the template language
	public void ExtractTextToTemplate() {
		LocalisationManager script = (LocalisationManager)target;
		if(!script.template) { return; }

		LocalisationExtractor[] extractors = Object.FindObjectsOfType<LocalisationExtractor>();
		foreach(LocalisationExtractor le in extractors) {
			le.ExtractText();
			foreach(Language.ValueGroup eg in le.groups) {
				//bool groupExisted = false;
				for(int i = script.template.groups.Count - 1; i >= 0; i--) {
					if(eg.title == script.template.groups[i].title) {
						script.template.groups.Remove(script.template.groups[i]);
					}
				}
				//foreach(Language.ValueGroup g in script.template.groups) {
				//	if(eg.title == g.title) {
				//		script.template.groups.Remove(g);
				//	}
				//}
				script.template.groups.Add(eg);
			}
		}
		//Refresh template file
		EditorUtility.SetDirty(script.template);
		AssetDatabase.SaveAssets();
	}

	//Foreach non-template language, adapt to value format of chosen template
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

					if(script.debug) {
						Debug.Log("Applied template to " + l.groups[i].title + " in " + l.name);
					}
				}
				//Refresh files
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
			} else if(tValues[i].key == oValues[i].key) {
				ApplyTemplateValues(tValues[i], oValues[i]);
			}
		}
		//Remove superfluous values
		if(oValues.Count > tValues.Count) {
			oValues.RemoveRange(tValues.Count, oValues.Count - tValues.Count);
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
				for(int g = 0; g < l.groups.Count; g++) {
					Language.ValueGroup group = l.groups[g];
					//Ignore groups tagged as translated
					if(!group.translated) {
						for(int v = 0; v < group.values.Count; v++) {
							//Clone existing format
							Language.KeyValue translation = group.values[v];
							//Replace value field with translation of template's
							translation.value = script.Translate(template.groups[g].values[v].value, template.code, l.code);
							//Replace values array with translations of template's
							translation.values = new List<string>();
							foreach(string value in template.groups[g].values[v].values) {
								translation.values.Add(script.Translate(value, template.code, l.code));
							}
							//Replace value with translated value
							group.values[v] = translation;
						}
						group.translated = true;
						if(script.debug) {
							Debug.Log("Translated " + group.title + " from " + template.name + " to " + l.name);
						}
					}
				}
				//Refresh files
				EditorUtility.SetDirty(l);
				AssetDatabase.SaveAssets();
			}
		}
	}
}