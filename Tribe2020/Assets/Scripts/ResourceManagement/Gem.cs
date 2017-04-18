using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using UnityEngine.Events;

public class Gem : MonoBehaviour, IPointerClickHandler {
	//Click callback
	public UnityEvent clickCallback;

    float _yPivot;
	public string type;
    public int value = 1;
    float _scaleFactor = 0.1f;

    Action<Gem> onTapCallback = null;
	private ResourceManager _resourceMgr;

	// Use this for initialization
	void Start () {
        _yPivot = transform.position.y;
		_resourceMgr = ResourceManager.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
        //Animate transform
        transform.Rotate(Vector3.up, Time.deltaTime * 50.0f);
        Vector3 position = transform.position;
        position.y = _yPivot + 0.1f * Mathf.Sin(Time.time * 2.0f);
        transform.position = position;
	}

    public void SetValue(int v) {
        value = v;
        float scale = 1.0f + _scaleFactor * (value - 1.0f);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void AddValue(int v) {
        SetValue(value + v);
    }

    public void SetScaleFactor(float value) {
        _scaleFactor = value;
    }

    public void SetOnTapCallback(Action<Gem> callback) {
        onTapCallback = callback;
    }

    public void OnPointerClick(PointerEventData data) {
		//Debug.Log(name + " was clicked");
		clickCallback.Invoke();

  //      if (onTapCallback != null) {
  //          //onTapCallback(this);
  //      }
  //      Destroy(gameObject);
		//_resourceMgr.comfortHarvestCount--;
    }

}
