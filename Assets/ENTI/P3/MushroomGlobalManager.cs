using UnityEngine;

public class MushroomGlobalManager : MonoBehaviour
{
    [Header("Jugador")]
    public Transform playerTransform;

    [Header("Ajustes Globales")]
    public float globalAffectRadius = 1.18f;
    public float globalAffectContrast = 0f;
    public float globalAffectIntensity = 0.5f;

    void Update()
    {
        if (playerTransform == null) return;

        // Ahora los nombres coinciden EXACTAMENTE con tu código HLSL
        Shader.SetGlobalVector("_PlayerPosition", playerTransform.position);
        Shader.SetGlobalFloat("_Affect_Radius", globalAffectRadius);
        Shader.SetGlobalFloat("_Affect_Contrast", globalAffectContrast);
        Shader.SetGlobalFloat("_Affect_Intensity", globalAffectIntensity);
    }
}