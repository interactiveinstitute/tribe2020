using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AvatarStats : MonoBehaviour {

    //Dirty flag - for rendering character panel
    bool _isUpdated;

    public Sprite portrait;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _knowledge;
    public float knowledge {
        get { return _knowledge; }
        set {
            _knowledge = value;
            SetUpdated();
        }
    }

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _attitude;
    public float attitude {
        get { return _attitude; }
        set {
            _attitude = value;
            SetUpdated();
        }
    }

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _normSensititvity;
    public float normSensititvity {
        get { return _normSensititvity; }
        set {
            _normSensititvity = value;
            SetUpdated();
        }
    }

    [Range(0.0f, 1.0f)]
    public float conviction;

    [Range(0.0f, 1.0f)]
    public float energy;

    /*public float maxEfficiencyValue;
	public float lightingEfficiency;
	public float warmingEfficiency;
	public float coolingEfficiency;
	public float deviceEfficiency;*/

    //public enum Efficiencies { Lighting, Warming, Cooling, Device}

    // Use this for initialization
    void Start() {
    }
	
	// Update is called once per frame
	void Update () {
	}

    void SetUpdated() {
        _isUpdated = true;
    }

    public bool IsUpdated() {
        bool returnValue = _isUpdated;
        _isUpdated = false;
        return returnValue;
    }

    public float GetEnergyEfficiency()
    {
        //INSERT algorithm for generating energy efficiency here! For now, just return attitude and knowledge mean :-D
        return (attitude + knowledge) / 2.0f;
    }

    public bool RunEnergyEfficiencyTest() {
        float val = GetEnergyEfficiency();
        float r = Random.value;
        return val >= r;
    }

    //TestEnergyEfficiency()
    //First argument is always parameter to test, following parameters are success callback with up to 4 arguments
    //Always return success flag

    public bool TestEnergyEfficiency() {
        return RunEnergyEfficiencyTest();
    }

    public bool TestEnergyEfficiency(System.Action successCallback) {
        if (!RunEnergyEfficiencyTest()) {
            return false;
        }
        successCallback();
        return true;
    }

    public bool TestEnergyEfficiency<U1>(System.Action<U1> successCallback, U1 p1) {
        if (!RunEnergyEfficiencyTest()) {
            return false;
        }
        successCallback(p1);
        return true;
    }

    public bool TestEnergyEfficiency<U1, U2>(System.Action<U1, U2> successCallback, U1 p1, U2 p2) {
        if (!RunEnergyEfficiencyTest()) {
            return false;
        }
        successCallback(p1, p2);
        return true;
    }

    public bool TestEnergyEfficiency<U1, U2, U3>(System.Action<U1, U2, U3> successCallback, U1 p1, U2 p2, U3 p3) {
        if (!RunEnergyEfficiencyTest()) {
            return false;
        }
        successCallback(p1, p2, p3);
        return true;
    }

    public bool TestEnergyEfficiency<U1, U2, U3, U4>(System.Action<U1, U2, U3, U4> successCallback, U1 p1, U2 p2, U3 p3, U4 p4) {
        if (!RunEnergyEfficiencyTest()) {
            return false;
        }
        successCallback(p1, p2, p3, p4);
        return true;
    }

    //Challenge

    public void ChallengeAttitude(AvatarStats other) {
        //High norm sensitivity and low own conviction && high other's conviction increase chance of updated value
        if (Random.value < normSensititvity && Random.value > conviction && Random.value < other.conviction) {

            float diff = other.attitude - attitude;
            float randomEffect = 0.5f * (Random.value - 0.5f); //[-0.25, 0.25]
            attitude += (diff + randomEffect) * Random.value * 0.5f;
        }
    }

    public void ChallengeKnowledge(AvatarStats other) {
        //High norm sensitivity and low own conviction && high other's conviction and high other's knowledge increase chance of updated value
        if (Random.value < normSensititvity && Random.value > conviction && Random.value < other.conviction && Random.value < other.knowledge) {
            float diff = other.knowledge - knowledge;
            float randomEffect = 0.5f * (Random.value - 0.5f); //[-0.25, 0.25]
            knowledge += (diff + randomEffect) * Random.value;
        }
    }

}
