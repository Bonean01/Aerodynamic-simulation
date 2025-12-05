using System;
using System.Numerics;
using NUnit.Framework;

public class SimulationTester {
    public SimulationTester() { ; }
    [Test]
    public static void Test() {
        float gravity = 9.8f;
        float airDensity = 1.293f;

        Airplane airplane = new(mass: 41145);

        TestContext.WriteLine("Velocity: " + airplane.velocity + " m/s");
        TestContext.WriteLine("Angle Of Attack: " + airplane.AngleOfAttack + "º");

        Vector3 totalAerodynamicForce = airplane.ComputeTotalAerodynamicForce(gravity, airDensity);
        TestContext.WriteLine("Total aerodynamic force:" + totalAerodynamicForce + " N");

        Vector3 acceleration = totalAerodynamicForce / airplane.mass;
        TestContext.WriteLine("Acceleration:" + acceleration + " m/s^2");
    }
}
