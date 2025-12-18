using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Rigidbody))]
public class Airplane : Aircraft {
    private Rigidbody rb;
    [SerializeField] private Atmosphere atmosphere;
    [SerializeField] private AirplaneConfig airplaneConfig;
    [Range(0, 1)] [SerializeField] private float throttle;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector3 position;
    
    [Header("Visualization parameters")]
    [SerializeField] private bool writeDataToFiles;

    [Header("READONLY for debugging purpouses")]
    [SerializeField] private float angleOfAttack;
    [SerializeField] private float speed;

    private AerodynamicSurface[] aeroSurfaces;
    private (Vector3 force, Vector3 position)[] lifts, drags;
    private Vector3 centerOfGravity;

    private Vector3 Weight { get => ComputeWeight(SimulationSettings.gravity); }
    private Vector3 Thrust { get => ComputeThrust(); }

    private int counter = 0;
    private float prevPitch = 0;
    private readonly string positionsPath = "./positions.txt";
    private readonly string pitchRatePath = "./pitch_rate.txt";

    public void SetPitchYawRoll(float pitch, float yaw, float roll) {
        Pitch = pitch;
        Yaw = yaw;
        Roll = roll;
    }

    private (Vector3, Vector3)[] ComputeLifts() {
        (Vector3, Vector3)[] lifts = new (Vector3, Vector3)[aeroSurfaces.Length];
        int i = 0;
        foreach (AerodynamicSurface surface in aeroSurfaces) {
            lifts[i] = (ComputeLift(surface), surface.GetCLWorldPos(transform));
            i++;
        }
        return lifts;
    }
    
    private (Vector3, Vector3)[] ComputeDrags() {
        (Vector3, Vector3)[] drags = new (Vector3, Vector3)[aeroSurfaces.Length];
        int i = 0;
        foreach (AerodynamicSurface surface in aeroSurfaces) {
            drags[i] = (ComputeDrag(surface), surface.GetCLWorldPos(transform));
            i++;
        }
        return drags;
    }

    public override Vector3 ComputeLift(AerodynamicSurface surface) {
        Vector3 localVelocity = Velocity + GetSurfaceRelativeTangentialVelocity(surface);
        if (localVelocity.magnitude == 0) return Vector3.zero;
        // Compute the lift produced by the surface.
        float airDensity = atmosphere.ComputeAirDensity(position.y);
        float liftMagnitude = surface.ComputeLift(AngleOfAttack, localVelocity.magnitude, airDensity);

        // Compute the direction in global coordinates in which the lift will be applied considering the orientation
        // of the aircraft and the mounting angle of the aerodynamic surfaces.
        float effectivePitch = this.Pitch + surface.GetMountAngle();
        Matrix4x4 local2world = GetLocalToGlobalMatrix(effectivePitch, this.Yaw, this.Roll);
        Vector3 liftDirection = local2world * Vector3.up;

        return liftMagnitude * liftDirection;
    }

    public override Vector3 ComputeDrag(AerodynamicSurface surface) {
        Vector3 localVelocity = Velocity + GetSurfaceRelativeTangentialVelocity(surface);
        if (localVelocity.magnitude == 0) return Vector3.zero;
        // Compute the drag produced by the surface.
        float airDensity = atmosphere.ComputeAirDensity(position.y);
        float dragMagnitude = surface.ComputeDrag(AngleOfAttack, localVelocity.magnitude, airDensity);

        // Drag always acts opposite to the direction of motion.
        Vector3 dragDirection = -localVelocity.normalized;

        return dragMagnitude * dragDirection;
    }

    public override Vector3 ComputeThrust() {
        //return Mathf.Lerp(0, 200000, throttle) * transform.forward;
        return airplaneConfig.EngineNumber * airplaneConfig.Engine.ComputeThrust(position.y, Velocity.magnitude, throttle, atmosphere) * transform.forward;
    }

    private Vector3 GetSurfaceRelativeTangentialVelocity(AerodynamicSurface surface) {
        Vector3 r = surface.GetCLWorldPos(transform) - centerOfGravity;
        Vector3 ω = rb.angularVelocity;
        return Vector3.Cross(ω, r);
    }

    private Vector3 ComputeTorque(Vector3 force, Vector3 forceApplicationPoint) {
        Vector3 r = forceApplicationPoint - centerOfGravity;
        return Vector3.Cross(r, force / Mass); // the mass of the airplane is set to 1 in the rb to avoid precision errors.
    }

    private Vector3 GetTotalTorque() {
        Vector3 torque = Vector3.zero;
        foreach (var lift in lifts) {
            torque += ComputeTorque(lift.force, lift.position);
        }
        foreach (var drag in drags) {
            torque += ComputeTorque(drag.force, drag.position);
        }
        return torque;
    }

    private Vector3 GetTotalForce() {
        Vector3 totalForce = Weight + Thrust;
        foreach (var lift in lifts) totalForce += lift.force;
        foreach (var drag in drags) totalForce += drag.force;

        return totalForce;
    }


    [ContextMenu("Apply pitch down disturbance")]
    private void PitchDownDisturbance() => rb.AddTorque(ComputeTorque(0.2f * Weight.magnitude * transform.up, airplaneConfig.Tail.GetCLWorldPos(transform)));

    void Start() {
        File.WriteAllText(positionsPath, "");
        File.WriteAllText(pitchRatePath, "");

        rb = GetComponent<Rigidbody>();

        aeroSurfaces = airplaneConfig.aerodynamicSurfaces;
        airplaneConfig.Tail.SetDownwashingSurface(airplaneConfig.RWing);
        centerOfGravity = transform.position;
        rb.centerOfMass = centerOfGravity;
        Velocity = velocity;
        rb.mass = 1;
    }

    void FixedUpdate() {
        Mass = airplaneConfig.Mass;
        angleOfAttack = AngleOfAttack;
        speed = Velocity.magnitude;
    
        velocity = Velocity;
        SetPitchYawRoll(-transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);

        lifts = ComputeLifts();
        drags = ComputeDrags();

        rb.AddTorque(GetTotalTorque());

        Velocity += GetTotalForce() / Mass * Time.fixedDeltaTime;
        position += Velocity * Time.fixedDeltaTime;

        WriteToFiles();
    }

    private void WriteToFiles() {
        counter++;
        if (counter % 60 == 0 && writeDataToFiles) {
            File.AppendAllText(positionsPath, position.z + "\t" + position.y + "\n");
            float pitchRate = (prevPitch - Pitch) / Time.fixedDeltaTime;
            File.AppendAllText(pitchRatePath, counter + "\t" + pitchRate + "\n");
            prevPitch = Pitch;
        }
        if (counter % 60 == 0) print($"airDensity: {atmosphere.ComputeAirDensity(position.y)}");
    }

    public void OnDrawGizmos() {
        DisplayForceAtPosition(transform.position, Weight, Color.red);
        Gizmos.DrawSphere(transform.position, .4f);
        
        if (!EditorApplication.isPlaying) {
            Gizmos.color = Color.green;
            foreach (AerodynamicSurface surface in airplaneConfig.aerodynamicSurfaces)
                Gizmos.DrawSphere(surface.GetCLWorldPos(transform), .4f);
            return;
        }


        foreach (var lift in lifts) {
            DisplayForceAtPosition(lift.position, lift.force, Color.green);
            Gizmos.DrawSphere(lift.position, .4f);
        }

        foreach (var drag in drags) DisplayForceAtPosition(drag.position, drag.force, Color.blue);

        Gizmos.color = Color.orange;
        DisplayForceAtPosition(transform.position, Thrust);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + Velocity);
    }

    private void DisplayForceAtPosition(Vector3 position, Vector3 force, Color? color = null) {
        if (color != null) Gizmos.color = (Color)color;
        Gizmos.DrawLine(position, position + force / Mass);
    }
}
