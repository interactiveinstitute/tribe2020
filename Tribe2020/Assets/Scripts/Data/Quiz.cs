using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Quiz", menuName = "Gameplay/Quiz", order = 1)]
public class Quiz : ScriptableObject {
	public string question;
	public List<string> options;
	public int rightChoice;
}
