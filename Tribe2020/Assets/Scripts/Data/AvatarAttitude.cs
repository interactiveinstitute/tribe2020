using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Attitude", menuName = "Avatar/Attitude", order = 1)]
public class AvatarAttitude : ScriptableObject {
	public string attitudeName;
	public List<string> listeners;
}
