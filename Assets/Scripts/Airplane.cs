using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(Rigidbody))]
public class Airplane : Aircraft {
    [SerializeField] private Rigidbody rb;
    [Header("Airplane specifications")]
    [SerializeField] private float mass;
    [Range(0, 5)] [SerializeField] private float acceleration;
    [SerializeField] private AerodynamicSurface wings, tail;
    private AerodynamicSurface[] aeroSurfaces;
    private Transform centerOfGravity;

    [Header("Initial conditions")]
    [SerializeField] private Vector3 initialVelocity;
    
    [Header("Visualization parameters")]
    [Range(0, 1)] [SerializeField] private float forceDisplayScale;

    [Header("READONLY for debugging purpouses")]
    [SerializeField] private float angleOfAttack, speed;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float totalMoment;
    [SerializeField] private Vector3 position;

    private IEnumerable<(Vector3 force, Vector3 position)> Lifts {
        get {
            foreach (AerodynamicSurface surface in aeroSurfaces)
                yield return (ComputeLift(surface), surface.GetCLPosition());
        }
    }

    private IEnumerable<(Vector3 force, Vector3 position)> Drags {
        get {
            foreach (AerodynamicSurface surface in aeroSurfaces)
                yield return (ComputeDrag(surface), surface.GetCLPosition());
        }
    }

    private Vector3 Weight { get => ComputeWeight(SimulationSettings.gravity); }
    private Vector3 Thrust { get => ComputeThrust(); }


    private int counter = 0;

    public void SetPitchYawRoll(float pitch, float yaw, float roll) {
        Pitch = pitch;
        Yaw = yaw;
        Roll = roll;
    }

    public override Vector3 ComputeLift(AerodynamicSurface surface) {
        Vector3 localVelocity = Velocity + GetSurfaceRelativeTangentialVelocity(surface);
        if (localVelocity.magnitude == 0) return Vector3.zero;
        // Compute the lift produced by the surface.
        float liftMagnitude = surface.ComputeLift(AngleOfAttack, localVelocity.magnitude, SimulationSettings.airDensity);

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
        float dragMagnitude = surface.ComputeDrag(AngleOfAttack, localVelocity.magnitude, SimulationSettings.airDensity);

        // Drag always acts opposite to the direction of motion.
        Vector3 dragDirection = -localVelocity.normalized;

        return dragMagnitude * dragDirection;
    }

    public override Vector3 ComputeThrust() {
        return acceleration * Mass * transform.forward;
    }

    private Vector3 GetSurfaceRelativeTangentialVelocity(AerodynamicSurface surface) {
        Vector3 r = surface.GetCLPosition() - centerOfGravity.position;
        Vector3 ω = rb.angularVelocity;
        return Vector3.Cross(ω, r);
    }

    private Vector3 ComputeTorque(Vector3 force, Vector3 forceApplicationPoint) {
        Vector3 r = forceApplicationPoint - centerOfGravity.position;
        return Vector3.Cross(r, force / mass); // the mass of the airplane is set to 1 in the rb to avoid precision errors.
    }

    private Vector3 GetTotalTorque() {
        Vector3 torque = Vector3.zero;
        foreach (var lift in Lifts) {
            torque += ComputeTorque(lift.force, lift.position);
        }
        foreach (var drag in Drags) {
            torque += ComputeTorque(drag.force, drag.position);
        }
        return torque;
    }

    private Vector3 GetTotalAeroForce() {
        Vector3 totalForce = Weight + Thrust;
        foreach (var lift in Lifts) totalForce += lift.force;
        foreach (var drag in Drags) totalForce += drag.force;

        return totalForce;
    }


    void Start() {
        aeroSurfaces = new AerodynamicSurface[] {wings, tail};
        tail.SetDownwashingSurface(wings);
        centerOfGravity = transform;
        rb.centerOfMass = centerOfGravity.position;
        Velocity = Vector3.zero;
        rb.mass = 1;

        Velocity = initialVelocity;
    }

    void FixedUpdate() {
        Mass = mass;
        angleOfAttack = AngleOfAttack;
        speed = Velocity.magnitude;
    
        velocity = Velocity;
        SetPitchYawRoll(-transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);

        rb.AddTorque(GetTotalTorque());

        Velocity += GetTotalAeroForce() / Mass * Time.fixedDeltaTime;
        position += Velocity * Time.fixedDeltaTime;

        //counter++;
        //if (counter % 60 == 0)
        //    File.AppendAllText("./positions.txt", position.y + "\t" + position.z + "\n");
    }



    public void OnDrawGizmos() {
        DisplayForceAtPosition(transform.position, Weight, Color.red);
        Gizmos.DrawSphere(transform.position, .2f);

        foreach (var lift in Lifts) {
            DisplayForceAtPosition(lift.position, lift.force, Color.green);
            Gizmos.DrawSphere(lift.position, .2f);
        }

        foreach (var drag in Drags) DisplayForceAtPosition(drag.position, drag.force, Color.blue);

        Gizmos.color = Color.orange;
        DisplayForceAtPosition(transform.position, Thrust);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + Velocity);
    }

    private void DisplayForceAtPosition(Vector3 position, Vector3 force, Color? color = null) {
        if (color != null) Gizmos.color = (Color)color;
        Gizmos.DrawLine(position, position + force / mass * forceDisplayScale);
    }
}
