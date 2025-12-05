using System;
using System.Numerics;
using NUnit.Framework;

/// <summary>
/// Holds the data needed for an aircraft where the nose points to the +Z axis.
/// </summary>
public abstract class Aircraft {
    /// <summary> Total effective mass of the aircraft for the computation of the forces. /summary>
    public float mass { get; protected set; }
    /// <summary> The angle of the aircraft's body relative to the direction of motion. </summary>
    public float AngleOfAttack => ComputeAOA();
    /// <summary> The velocity of the aircraft (airspeed * directionOfMotion). </summary>
    public Vector3 velocity { get; protected set; } = new (0, 0, 150); // 150m/s = 540 km/h
    /// <summary> Angle in radians the aircraft is offset from its resting position in the latteral axis, positive pitch means the nose moves up. </summary>
    public float pitch { get; protected set; } = 3 * (2*(float)Math.PI / 360f);
    /// <summary> Angle in radians the aircraft is offset from its resting position in the vertical axis. </summary>
    public float yaw { get; protected set; } = 0;
    /// <summary> Angle in radians the aircraft is offset from its resting position in the logitudinal axis, positive roll means the right wing moves down. </summary>
    public float roll { get; protected set; } = 0.05f;
    /// <summary>
    /// CG: Center of Gravity of the aircraft, this is the point where gravity will be applied and the point around which the aircraft will rotate if any
    /// external force is applied in a line where the CG is not included.
    /// </summary>
    protected Vector3 CG = Vector3.Zero;
    /// <summary>
    /// CL: Center of Lift of the aircraft, this is the point where lift will be applied,
    /// usually positioned aft (towards the tail) of the CG to favour stability.
    /// </summary>
    protected Vector3 CL = new (0, 0, -1);

    /// <summary>
    /// Computes the angle of attack of the aircraft based on the velocity vector and the aircrafts forward vector in global coordinates.
    /// </summary>
    /// <returns>The angle of attack of the aircraft in degrees.</returns>
    protected float ComputeAOA() {
        // Getting the forward vector of the aircraft in global coordinates.
        
        Matrix4x4 local2world = GetLocalToGlobalMatrix(yaw, pitch, roll);
        Vector3 forward = Vector3.Transform(Vector3.UnitZ, local2world);

        // Computing the angle between the forward vector and the velocity vector.
        float angle = (float)Math.Acos(Vector3.Dot(forward, velocity) / (forward.Length() * velocity.Length()));

        return angle * (360f / (2*(float)Math.PI)); // Converting from radians to degrees.
    }

    /// <summary>
    /// Computes the total lift due to all aerodynamic surfaces on the aircraft that will directly be applied to the CL.
    /// </summary>
    /// <returns>The lift vector in global coordinates.</returns>
    protected abstract Vector3 ComputeLift(float airDensity);
    /// <summary>
    /// Computes the total drag due to all aerodynamic surfaces on the aircraft.
    /// </summary>
    /// <returns>The drag vector in global coordinates</returns>
    protected abstract Vector3 ComputeDrag(float airDensity);
    /// <summary>
    /// Computes the total thrust force acting on the aircraft.
    /// </summary>
    /// <returns>The thrust force vector in global coordinates.</returns>
    protected abstract Vector3 ComputeThrust();
    /// <summary>
    /// Computes the weight force acting on the aircraft.
    /// </summary>
    /// <param name="gravity">The gravity acceleration constant.</param>
    /// <returns>The weight force vector in global coordinates.</returns>
    protected Vector3 ComputeWeight(float gravity) {
        return new Vector3 (0, -1, 0) * mass * gravity;
    }

    /// <summary>
    /// Computes all of the aerodynamic forces acting on the aircraft.
    /// </summary>
    /// <returns>The resulting force of adding all of the aerodinamic forces acting on it.</returns>
    public Vector3 ComputeTotalAerodynamicForce(float gravity, float airDensity) {
        Vector3 lift = ComputeLift(airDensity);
        Vector3 drag = ComputeDrag(airDensity);
        Vector3 thrust = ComputeThrust();
        Vector3 weight = ComputeWeight(gravity);

        TestContext.WriteLine("lift: " + lift);
        TestContext.WriteLine("drag: " + drag);
        TestContext.WriteLine("thrust: " + thrust);
        TestContext.WriteLine("weight: " + weight);

        return lift + drag + thrust + weight;
    }

    /// <summary>
    /// Computes the total moment of the aircraft.
    /// </summary>
    /// <returns>The moment of the aircraft in the form of a Vector 3: moment along (the latteral axis, the vertical axis, the longitudinal axis)</returns>
    public Vector3 ComputeTotalMoment() { return Vector3.Zero; }

    /// <summary>
    /// Calculates the rotation matrix to transform vectors from local space to global space taking into consideration that the vectors in local space
    /// live in a left handed coordinate system, so positive pitch implies nose up, and positive roll implies right wing down.
    /// </summary>
    /// <returns>The local to global transformation matrix.</returns>
    public static Matrix4x4 GetLocalToGlobalMatrix(float yaw, float pitch, float roll) {
        // System.Numerics uses a right handed coordinate system and so pitch and roll have to be inverted in order to match aerodynamic intuition.
        return Matrix4x4.CreateFromYawPitchRoll(yaw, -pitch, -roll);
    }
}
