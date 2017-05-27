﻿using UnityEngine;
using System.Collections;

public interface NarrationInterface{
	string GetCurrentDate();
	void LimitInteraction(string limitation);
	void ControlInterface(string id, string action);
	void PlaySound(string sound);
	void ShowMessage(string cmdJSON);
	void ShowPrompt(string cmdJSON);
	void ShowCongratualations(string cmdJSON);
	void ShowMessage(string key, string message, Sprite portrait, bool showButton);
	void CreateHarvest(string commandJSON);
	void ControlAvatar(string id, string action, Vector3 targetPosition);
	void ControlAvatar(string id, Object action);
	void UnlockView(int x, int y);
	//void ShowCongratualations(string text);
	void ClearView();
	void SaveGameState();
	void SetSimulationTimeScale(float timeScale);
	void RequestCurrentView();
	void MoveCamera(string animation);
	void OnAnimationEvent(string animationEvent);
	void StopCamera();
}
