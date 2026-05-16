using UnityEngine;

public class SnowTimeController : MonoBehaviour
{
    public Material snowMaterial;

    [Header("Velocidad del ciclo (menor = más lento)")]
    public float cycleSpeed = 1f;

    void Update()
    {
        if (snowMaterial == null) return;

      
        float snowLevel = Mathf.Clamp01(Mathf.Sin(Time.time * cycleSpeed));

     
        snowMaterial.SetFloat("_Snow_Amount", snowLevel);
    }
}