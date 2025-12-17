using UnityEngine;

public class Atmosphere : MonoBehaviour {
    private const float UNIVERSAL_GAS_CONSTANT = 8.314462618f;
    [SerializeField] private float seaAirDensity;
    [SerializeField] private float temperatureLapseRate;
    [SerializeField] private float seaTemperature, seaPressure;
    [SerializeField] private float airSpecificHeat, airSpecificHeatRatio, airSpecificGasConstant, dryAirMolarMass;
    
    // Only applies for temperature in the troposphere.
    public float ComputeTemperature(float altitude) => seaTemperature - temperatureLapseRate * altitude;

    public float ComputeAirDensity(float altitude) {
        float temperature = ComputeTemperature(altitude);
        float pressure = ComputePressure(altitude);
        return pressure * dryAirMolarMass / (UNIVERSAL_GAS_CONSTANT * temperature);
    }

    public float ComputePressure(float altitude) {
        float exp = -SimulationSettings.gravity * dryAirMolarMass / (UNIVERSAL_GAS_CONSTANT * temperatureLapseRate);
        return seaPressure * Mathf.Pow(1 + temperatureLapseRate * altitude / seaTemperature, exp);
    }

    public float GetAirSpecificHeat() => airSpecificHeat;
    public float GetAirSpecificHeatRatio() => airSpecificHeatRatio;
    public float GetAirSpecificGasConstant() => airSpecificGasConstant;
}
