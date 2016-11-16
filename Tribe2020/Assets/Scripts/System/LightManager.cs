using UnityEngine;
using System.Collections;

public class LightManager : MonoBehaviour {

    public LightShadows lightShadows;
    public LightShadowResolution lightShadowResolution;

    public GameObject pilotMesh;
    public UnityEngine.Rendering.ShadowCastingMode meshShadows;

    public GameObject pilotContent;
    public UnityEngine.Rendering.ShadowCastingMode applianceShadows;

    public GameObject avatars;
    public UnityEngine.Rendering.ShadowCastingMode avatarShadows;
    

    // Use this for initialization
    void Start () {
        UpdateShadowSettings();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnValidate()
    {
        UpdateShadowSettings();
    }

    void UpdateShadowSettings()
    {
        if(pilotContent != null)
        {
            foreach (Light light in pilotContent.GetComponentsInChildren<Light>())
            {
                light.shadows = lightShadows;
                light.shadowResolution = lightShadowResolution;
            }
            foreach (MeshRenderer mesh in pilotContent.GetComponentsInChildren<MeshRenderer>())
            {
                mesh.shadowCastingMode = applianceShadows;
            }
        }
        
        if(avatars != null)
        {
            foreach (SkinnedMeshRenderer mesh in avatars.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                mesh.shadowCastingMode = avatarShadows;
            }
        }

        if (pilotMesh != null)
        {
            foreach (MeshRenderer mesh in pilotMesh.GetComponentsInChildren<MeshRenderer>())
            {
                mesh.shadowCastingMode = meshShadows;
            }
        }

        if(pilotContent == null || pilotMesh == null || avatars == null)
        {
            DebugManager.LogError("Missing some references in light manager", this.gameObject, this);
        }
    }

}
