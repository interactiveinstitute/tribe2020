using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Quest", menuName = "Gameplay/Quest", order = 1)]
public class Quest : ScriptableObject {
	public string title;
	public string sender;
	public string message;
}
