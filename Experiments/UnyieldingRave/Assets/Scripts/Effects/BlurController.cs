using UnityEngine;
using UnityEngine.UI;

// Управление параметрами материала, взятых из blurShader'а
public class BlurController : MonoBehaviour
{

    public float BlurSize = 0;

    private Material material;
    private float prevSize = 0;

    private void Awake()
    {
        material = Material.Instantiate( GetComponent<Image>().material );
        GetComponent<Image>().material = material;
    }

    // Update is called once per frame
    void Update()
    {
        if( prevSize != BlurSize ) {
            prevSize = BlurSize;
            material.SetFloat( "_Size", BlurSize );
        }
    }
}
