using UnityEngine;

public class MushroomInstanceColor : MonoBehaviour
{
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        // Creamos un color aleatorio (tipo neón/vibrante para que se vea bien)
        Color randomColor = Color.HSVToRGB(Random.value, 0.8f, 1f);

        // Creamos el PropertyBlock. Esto modifica las variables del shader
        // SOLO para este objeto, sin crear un material nuevo.
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        // Leemos si el objeto ya tiene otras propiedades
        rend.GetPropertyBlock(propBlock);

        // Le inyectamos el color. OJO: "_Color" debe ser el Reference exacto de tu Shader Graph
        propBlock.SetColor("_Color", randomColor);

        // Se lo devolvemos al renderizador
        rend.SetPropertyBlock(propBlock);
    }
}