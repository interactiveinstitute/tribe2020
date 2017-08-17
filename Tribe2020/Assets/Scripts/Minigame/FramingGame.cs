using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FramingGame : MonoBehaviour {
	private BattleController _controller;
	private Transform _gameFrame;
	private List<Sprite> _goodSprites;
	private List<Sprite> _badSprites;

	public float playTimeThreshold = 10;
	private float _playTimer = 0;


	// Use this for initialization
	void Start () {
		_controller = BattleController.GetInstance();
		_gameFrame = _controller.GetMinigameFrame();

		Reset();
	}
	
	// Update is called once per frame
	void Update () {
		if(_playTimer < playTimeThreshold) {
			_playTimer += Time.deltaTime;
			if(_playTimer > playTimeThreshold) {
				ResolveGame();
			}
		}
	}

	//
	public void Reset() {
		ClearMinigame();
		AddSubject(_goodSprites[0], new Vector2(0, 0), new Vector2(70, 70), true);
		AddSubject(_goodSprites[0], new Vector2(0, 0), new Vector2(70, 70), true);
		AddSubject(_goodSprites[0], new Vector2(0, 0), new Vector2(70, 70), true);
		AddSubject(_goodSprites[0], new Vector2(0, 0), new Vector2(70, 70), true);
		AddSubject(_badSprites[0], new Vector2(0, 0), new Vector2(70, 70), true);
	}

	//
	public void ResolveGame() {
		
	}

	//
	public void AddDiscussionFrame() {
		GameObject newBlock = new GameObject();
		newBlock.name = "Frame of Discussion";
		newBlock.transform.SetParent(_gameFrame.transform, false);
	}

	//
	public void AddSubject(Sprite img, Vector2 pos, Vector2 size, bool isRelevant) {
		GameObject newBlock = new GameObject();
		newBlock.name = "Subject";
		newBlock.transform.SetParent(_gameFrame.transform, false);
		Image newImg = newBlock.AddComponent<Image>();
		newImg.sprite = img;
		Rigidbody2D newRB = newBlock.AddComponent<Rigidbody2D>();
		newRB.gravityScale = 0;
		BoxCollider2D newColl = newBlock.AddComponent<BoxCollider2D>();
		newColl.size = size;
		newBlock.AddComponent<Draggable>();
	}

	//
	public void ClearMinigame() {
		foreach(Transform t in _gameFrame) {
			if(t.gameObject.name != "Bounds") {
				Destroy(t.gameObject);
			}
		}
	}
}
