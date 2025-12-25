using UnityEngine;

[CreateAssetMenu(fileName = "AirplaneConfig", menuName = "Scriptable Objects/AirplaneConfig")]
public class AirplaneConfig : ScriptableObject {
    [Header("Airplane specifications")]
    [SerializeField] private float mass;
    [SerializeField] private AerodynamicSurface rWing, lWing, tail, tailFin;

    [Header("Engine specifications")]
    [SerializeField] private int engineNumber;
    [SerializeField] private SubsonicEngine engine;

    public float Mass => mass;
    public AerodynamicSurface RWing => rWing;
    public AerodynamicSurface LWing => lWing;
    public AerodynamicSurface Tail => tail;
    public int EngineNumber => engineNumber;
    public SubsonicEngine Engine => engine;
    public AerodynamicSurface[] AerodynamicSurfaces => new AerodynamicSurface[] {rWing, lWing, tail, tailFin};
}
