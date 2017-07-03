using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InteractionListener {
	void OnTap(Vector3 position);
	void OnSwipe(Vector2 direction);
}
