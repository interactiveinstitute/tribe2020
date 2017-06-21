using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Quiz", menuName = "Gameplay/Quiz", order = 1)]
public class Quiz : ScriptableObject {
	[TextArea(3, 10)]
	public string question;
	[TextArea(2, 10)]
	public List<string> options;
	public int rightChoice;
}
