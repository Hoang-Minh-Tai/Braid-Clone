using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData_SO", menuName = "Data/SceneData_SO")]
public class SceneData_SO : ScriptableObject
{
    public string sceneName;
    public AudioClip backgroundMusic;
    public AudioClip backgroundMusic_Reverse;
    public GateData[] gates;
}

[Serializable]
public class GateData
{
    public SceneData_SO targetScene;
    public int targetGateIndex;
    public Vector3 location;
    [Range(-1, 1)]
    public int direction = 1;
}
