using UnityEngine;
using System.Collections;

public interface CameraInterface {
	void OnAnimationEvent(string animationEvent);
	void OnNewViewpoint(Viewpoint curView, Viewpoint[][] viewMatrix, bool overview);
}
