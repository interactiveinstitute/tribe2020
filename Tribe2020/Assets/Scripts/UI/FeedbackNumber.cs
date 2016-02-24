using UnityEngine;
using System.Collections;

public class FeedbackNumber : MonoBehaviour{
	private TextMesh _textMesh;

	// Use this for initialization
	void Start(){
		_textMesh = GetComponent<TextMesh>();

		Vector3 newPos = transform.position + Vector3.up;
		transform.position = newPos;
	}
	
	// Update is called once per frame
	void Update(){
		Vector3 newPos = transform.position + Vector3.up * Time.deltaTime * 1f;
		transform.position = newPos;

		Color newColor = _textMesh.color - Color.black * Time.deltaTime * 0.5f;
		_textMesh.color = newColor;

		if(_textMesh.color.a <= 0){
			Destroy(gameObject);
		}
	}
}
