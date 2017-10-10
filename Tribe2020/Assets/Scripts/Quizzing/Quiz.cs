using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Quiz", menuName = "Gameplay/Quiz", order = 1)]
public class Quiz : ScriptableObject {
	public enum Type { Quiz, Frame, Build, Gain, Present };
	public Type type = Type.Quiz;

	[TextArea(3, 10)]
	public string question;
	[TextArea(2, 10)]
	public List<string> options;
	public int rightChoice;

	public List<Sprite> goodImages;
	public List<Sprite> badImages;
	public int level = 0;
}
