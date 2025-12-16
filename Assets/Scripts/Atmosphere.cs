using UnityEngine;

public class Atmosphere : MonoBehaviour {
    [SerializeField] private float seaAirDensity;
    [SerializeField] private float temperatureLapseRate;
    [SerializeField] private float seaTemperature;
    [SerializeField] private float polytropicIndex;
    [SerializeField] private float airSpecificHeat, airSpecificHeatRatio;
    
    public float ComputeTemperature(float altitude) => seaTemperature - temperatureLapseRate * altitude;

    public float ComputeAirDensity(float altitude) {
        float temperature = ComputeTemperature(altitude);
        return Mathf.Pow(temperature / seaTemperature, polytropicIndex - 1) * seaAirDensity;
    }

    public float GetAirSpecificHeat() => airSpecificHeat;
    public float GetAirSpecificHeatRatio() => airSpecificHeatRatio;
}
