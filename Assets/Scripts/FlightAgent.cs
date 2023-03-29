using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;

public enum Team {
    Red = 0,
    Blue = 1
}

public class FlightAgent : Agent {
    public Team team;
    private BattleEnvController envController;
    public List<AeroSurface> controlSurfaces = null;
    public List<WheelCollider> wheels = null;
    public float rollControlSensitivity = 0.2f;
    public float pitchControlSensitivity = 0.2f;
    public float yawControlSensitivity = 0.2f;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    private float Flap;
    
    [HideInInspector]
    public bool Living = true;

    float thrustPercent;
    float brakesTorque = 0;

    private float Health;
    private float ShotRange;
    private bool isShooting;
    private float ShootCooldown;

    public GameObject ShootAnimation;
    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    public override void Initialize() {
        envController = GetComponentInParent<BattleEnvController>();
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
        Health = envController.PlaneHealth;
        ShotRange = envController.ShotRange;
        ShootCooldown = envController.ShotCooldown;
    }

    public override void OnEpisodeBegin() {
        Living = true;
        Health = envController.PlaneHealth;
        thrustPercent = 0;
        Flap = 0;
        Yaw = 0;
        Pitch = 0;
        Roll = 0;
    }

    public override void CollectObservations(VectorSensor sensor) {
        if (!Living) {
            sensor.AddObservation(new float[54]);
            return;
        }

        // Observations for all planes:
        foreach (var item in envController.AgentsList) {
            //Pad if the plane is dead
            if (!item.Agent.Living) {
                sensor.AddObservation(new float[12]); // 12
                continue;
            }

            sensor.AddObservation(item.Agent.transform.position); // 3
            sensor.AddObservation(item.Agent.transform.localEulerAngles); // 3
            sensor.AddObservation(item.Rb.velocity); // 3
            sensor.AddObservation(item.Rb.angularVelocity); // 3
        }

        // Observations for bases:
        sensor.AddObservation(envController.RedBase.transform.position); // 3
        sensor.AddObservation(envController.BlueBase.transform.position); // 3

        // Total: 4*12 + 6 = 54 Observations
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;
        continuousActionsOut[0] = Input.GetAxis("Yaw");
        continuousActionsOut[1] = Input.GetAxis("Pitch");
        continuousActionsOut[2] = Input.GetAxis("Roll");
        discreteActionsOut[0] = Input.GetAxis("Throttle") > 0 ? 1 : 0;
        discreteActionsOut[1] = Input.GetAxis("Flap") > 0 ? 1 : 0;
        discreteActionsOut[2] = Input.GetAxis("Fire1") > 0 ? 1 : 0;
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if (!Living) {
            return;
        }
        
        if (this.gameObject.name == "Red_Plane1") {
            Debug.Log("Vel: " + rb.velocity + ", Pos: " + this.transform.position);
        }

        // Apply actions
        var continuousActions = actions.ContinuousActions;
        var discreteActions = actions.DiscreteActions;
        Yaw = continuousActions[0];
        Pitch = continuousActions[1];
        Roll = continuousActions[2];
        thrustPercent = discreteActions[0] > 0 ? 1f : 0f;
        Flap = discreteActions[1] > 0 ? 0.3f : 0f;
        bool fire = discreteActions[2] > 0;

        // Fire
        if (fire && !isShooting) {
            RaycastHit hit;
            Instantiate(ShootAnimation, this.transform.position, this.transform.rotation);
            isShooting = true;
            Invoke("ResetShoot", ShootCooldown);
            if (Physics.Raycast(this.transform.localPosition, this.transform.forward, out hit, ShotRange)) {
                envController.Shot(this, hit.transform.gameObject);
            }
        }

        if (this.transform.position.y > 1)  {
            AddReward(envController.FlightReward);
        }

        // Check for out of bounds, and kill agent if out of bounds
        float size = envController.FieldSize;
        if (this.transform.localPosition.x > size || this.transform.localPosition.x < -size || this.transform.localPosition.z > size || this.transform.localPosition.z < -size) {
            Die();
        }
    }

    private void ResetShoot() {
        isShooting = false;
        var animations = GameObject.FindGameObjectsWithTag("ShootAnimation");
        foreach (var anim in animations) {
            Destroy(anim);
        }
    }

    public void Hit() {
        Health--;
        if (Health <= 0) {
            Die();
        }
    }

    private void FixedUpdate() {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque; // Braking action removed since it was not used
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }
    }

    public void Die() {
        Living = false;
        this.gameObject.SetActive(false);
        envController.LivingAgents--;
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap) {
        foreach (var surface in controlSurfaces) {
            if (surface == null || !surface.IsControlSurface) continue;

            switch (surface.InputType) {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
    }

    void OnCollisionEnter(Collision collision) {
        Vector3 collisionForce = collision.impulse / Time.fixedDeltaTime;
        if (collisionForce.magnitude > 3000) {
            Die();
            AddReward(envController.CrashPenalty);
        }
    }
}
