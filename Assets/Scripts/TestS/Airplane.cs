using System.Numerics;
using NUnit.Framework;
using NUnit.Framework.Internal;

public class Airplane : Aircraft {
    /// <summary>
    /// The wings of the airplane, both of them have been merged into one big aerodynamic surface
    /// with double the area to avoid computing the same stuff twice.
    /// </summary>
    private AerodynamicSurface wings;

    public Airplane(float mass) {
        this.mass = mass;
        wings = new (surfaceArea: 125);
    }

    protected override Vector3 ComputeLift(float airDensity) {
        if (velocity.Length() == 0) return Vector3.Zero;
        // Compute the lift produced by the wings.
        float liftMagnitude = wings.ComputeLift(AngleOfAttack, velocity.Length(), airDensity);
        TestContext.WriteLine("Wing lift magnitude: " + liftMagnitude);

        // Compute the direction in global coordinates in which the lift will be applied considering the orientation
        // of the aircraft and the mounting angle of the aerodynamic surfaces.
        float effectivePitch = this.pitch + wings.GetMountAngle();
        Matrix4x4 local2world = GetLocalToGlobalMatrix(this.yaw, effectivePitch, this.roll);
        Vector3 liftDirection = Vector3.Transform(Vector3.UnitY, local2world);
        TestContext.WriteLine("liftDirection: " + liftDirection);

        return liftMagnitude * liftDirection;
    }

    protected override Vector3 ComputeDrag(float airDensity) {
        if (velocity.Length() == 0) return Vector3.Zero;
        // Compute the drag produced by the wings.
        float dragMagnitude = wings.ComputeDrag(AngleOfAttack, velocity.Length(), airDensity);

        // Drag always acts opposite to the direction of motion.
        Vector3 dragDirection = -Vector3.Normalize(velocity);

        return dragMagnitude * dragDirection;
    }

    protected override Vector3 ComputeThrust() {
        return Vector3.Zero;
    }
}
