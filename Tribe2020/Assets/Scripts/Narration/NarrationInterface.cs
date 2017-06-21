using UnityEngine;
using System.Collections;

public interface NarrationInterface{
	void OnNarrativeAction(Narrative narrative, Narrative.Step step, string callback, string[] parameters);
	void OnNarrativeCompleted(Narrative narrative);
	void OnNarrativeActivated(Narrative narrative);
	bool HasEventFired(Narrative narrative, Narrative.Step step);
	bool NarrativeCheck(string callback);
}
