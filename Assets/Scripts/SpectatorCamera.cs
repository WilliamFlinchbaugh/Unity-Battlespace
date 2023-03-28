using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{   
    public List<Transform> Targets;
    private List<Transform> LivingTargets;
    public Vector3 Offset;
    private Vector3 maxPoint, centerPoint;

    void Start() {
        LivingTargets = new List<Transform>(Targets);
    }

    void LateUpdate() {
        // Remove dead targets
        for (int i = 0; i < Targets.Count; i++) {
            if (!Targets[i].gameObject.activeSelf) {
                LivingTargets.Remove(Targets[i]);
            }
        }
        
        GetPoints();
        Vector3 newPosition = maxPoint + Offset;
        transform.position = newPosition;
        transform.LookAt(centerPoint);
    }

    void GetPoints() {
        if (LivingTargets.Count <= 0) {
            maxPoint = Vector3.zero;
            centerPoint = Vector3.zero;
            return;
        }
        var bounds = new Bounds(LivingTargets[0].position, Vector3.zero);
        for (int i = 0; i < LivingTargets.Count; i++) {
            bounds.Encapsulate(LivingTargets[i].position);
        }
        maxPoint = bounds.max;
        centerPoint = bounds.center;
    }

    public void ResetCam() {
        LivingTargets = new List<Transform>(Targets);
    }
}
