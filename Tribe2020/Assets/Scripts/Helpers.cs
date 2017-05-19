using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helpers : MonoBehaviour {

    public static Bounds LargestBounds(Transform transform) {
        Bounds bounds = new Bounds();
        Renderer mr = transform.GetComponent<MeshRenderer>();
        if (mr) {
            bounds = mr.bounds;
        }
        else {
            SkinnedMeshRenderer smr = transform.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (smr) {
                bounds = smr.bounds;
            }
        }

        foreach(Transform child in transform) {
            Bounds bChild = LargestBounds(child);
            bounds.min = new Vector3(Mathf.Min(bounds.min.x, bChild.min.x), Mathf.Min(bounds.min.y, bChild.min.y), Mathf.Min(bounds.min.z, bChild.min.z));
            bounds.max = new Vector3(Mathf.Max(bounds.max.x, bChild.max.x), Mathf.Max(bounds.max.y, bChild.max.y), Mathf.Max(bounds.max.z, bChild.max.z));
        }
        return bounds;
    }

}
