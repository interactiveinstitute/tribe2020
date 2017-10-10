using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildGoal : MonoBehaviour {
	public float winThreshold = 3;
	public float playTimeThreshold = 10;

	private bool _isColliding = false;
	[SerializeField]
	private List<Draggable> _collidingDraggables;
	private List<Draggable> _winningDraggables;
	private int _collisionCount = 0;
	private float _collideTimer = 0;
	private float _playTimer = 0;

	private BattleController _controller;

	// Use this for initialization
	void Start() {
		_controller = BattleController.GetInstance();

		_collidingDraggables = new List<Draggable>();
		_winningDraggables = new List<Draggable>();
	}
	
	// Update is called once per frame
	void Update() {
		_winningDraggables.Clear();
		foreach(Draggable d in _collidingDraggables) {
			if(!d.IsDragged()) {
				_winningDraggables.Add(d);
				//Now there is one undragged draggable colliding
				if(!_isColliding) {
					_controller.OnBuildEnter();
					_isColliding = true;
				}
			}
		}
		//No undragged draggables colliding anymore
		if(_isColliding && _winningDraggables.Count == 0) {
			_controller.OnBuildExit();
			_isColliding = false;
			_collideTimer = 0;
		}
		//Tick winning counter
		if(_isColliding && _collideTimer < winThreshold) {
			_collideTimer += Time.deltaTime;
			//Victory!
			if(_collideTimer > winThreshold) {
				_controller.OnMinigameWin();
			}
		}
		//Tick play time limit
		if(_playTimer < playTimeThreshold) {
			_playTimer += Time.deltaTime;
			if(_playTimer > playTimeThreshold) {
				_controller.OnMinigameLose();
			}
		}
	}

	//
	void OnTriggerEnter2D(Collider2D other) {
		if(other.GetComponent<Draggable>()) {
			_collidingDraggables.Add(other.GetComponent<Draggable>());
		}
	}

	//
	void OnTriggerExit2D(Collider2D other) {
		if(other.GetComponent<Draggable>()) {
			_collidingDraggables.Remove(other.GetComponent<Draggable>());
		}
	}
}
