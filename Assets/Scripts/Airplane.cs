using System;
using UnityEngine;
//using NUnit.Framework;

public class Airplane : Aircraft {
    /// <summary>
    /// The wings of the airplane, both of them have been merged into one big aerodynamic surface
    /// with double the area to avoid computing the same stuff twice.
    /// </summary>
    private AerodynamicSurface wings;

    public Airplane(float mass) {
        Mass = mass;
        wings = new (surfaceArea: 125, mountAngle: 10);
    }

    public Airplane(float mass, float wingSurfaceArea) {
        Mass = mass;
        wings = new (wingSurfaceArea, mountAngle: 10);
    }

    public Airplane(float mass, float wingSurfaceArea, float wingMountAngle, float wingsCriticalAngle) {
        Mass = mass;
        wings = new (wingSurfaceArea, wingMountAngle, wingsCriticalAngle);
    }

    public void SetPitchYawRoll(float pitch, float yaw, float roll) {
        Pitch = pitch;
        Yaw = yaw;
        Roll = roll;
    }

    public override Vector3 ComputeLift(float airDensity) {
        if (Velocity.magnitude == 0) return Vector3.zero;
        // Compute the lift produced by the wings.
        float liftMagnitude = wings.ComputeLift(AngleOfAttack, Velocity.magnitude, airDensity);
        //TestContext.WriteLine("Wing lift magnitude: " + liftMagnitude);

        // Compute the direction in global coordinates in which the lift will be applied considering the orientation
        // of the aircraft and the mounting angle of the aerodynamic surfaces.
        float effectivePitch = this.Pitch + wings.GetMountAngle();
        Matrix4x4 local2world = GetLocalToGlobalMatrix(effectivePitch, this.Yaw, this.Roll);
        Vector3 liftDirection = local2world * Vector3.up;
        //Debug.Log("liftDirection: " + liftDirection);
        return liftMagnitude * liftDirection;
    }

    public override Vector3 ComputeDrag(float airDensity) {
        if (Velocity.magnitude == 0) return Vector3.zero;
        // Compute the drag produced by the wings.
        float dragMagnitude = wings.ComputeDrag(AngleOfAttack, Velocity.magnitude, airDensity);

        // Drag always acts opposite to the direction of motion.
        Vector3 dragDirection = -Velocity.normalized;

        return dragMagnitude * dragDirection;
    }

    public override Vector3 ComputeThrust() {
        return Vector3.zero;
    }
}
