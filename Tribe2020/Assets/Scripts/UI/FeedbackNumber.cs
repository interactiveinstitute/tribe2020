using UnityEngine;
using System.Collections;

public class FeedbackNumber : MonoBehaviour{
	private TextMesh _textMesh;
	//private Vector3 velocity = Vector3.zero;

	// Use this for initialization
	void Start(){
		_textMesh = GetComponent<TextMesh>();

		Vector3 newPos = transform.position + Vector3.up;
		transform.position = newPos;
	}
	
	// Update is called once per frame
	void Update(){
		//float dt = Mathf.Clamp(Time.unscaledDeltaTime, 0, 0.2f);

		Vector3 newPos = transform.position + Vector3.up * Time.unscaledDeltaTime * 1f;
		_textMesh.transform.position = newPos;
		//Vector3 targetPos = transform.position;
		//targetPos.y = 3;
		//transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 0.3f);

		Color newColor = _textMesh.color - Color.black * Time.unscaledDeltaTime * 0.5f;
		_textMesh.color = newColor;

		if(_textMesh.color.a <= 0){
			Destroy(gameObject);
		}
	}

	//public Transform target;
	//public float smoothTime = 0.3F;
	//private Vector3 velocity = Vector3.zero;
	//void Update() {
	//	Vector3 targetPosition = target.TransformPoint(new Vector3(0, 5, -10));
	//	transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
	//}
}
