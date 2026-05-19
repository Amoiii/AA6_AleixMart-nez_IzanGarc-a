using UnityEngine;

public class MushroomInstanceColor : MonoBehaviour
{
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

      
        Color randomColor = Color.HSVToRGB(Random.value, 0.8f, 1f);

       
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

      
        rend.GetPropertyBlock(propBlock);

       
        propBlock.SetColor("_Color", randomColor);

        
        rend.SetPropertyBlock(propBlock);
    }
}