using UnityEngine;

[ExecuteAlways]
public class MushroomGlobalManager : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Ajustes Globales")]
    public float globalAffectRadius = 5f;
    public float globalAffectContrast = 1f;
    public float globalAffectIntensity = 1f;

    void Update()
    {
        if (playerTransform == null) return;

        Shader.SetGlobalVector("_PlayerPosition", playerTransform.position);
        Shader.SetGlobalFloat("_Affect_Radius", globalAffectRadius);
        Shader.SetGlobalFloat("_Affect_Contrast", globalAffectContrast);
        Shader.SetGlobalFloat("_Affect_Intensity", globalAffectIntensity);
    }
}