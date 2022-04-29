using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InteractableAbstract : MonoBehaviour
{

    private Material _material = null;
    protected Material interactableMaterial
    {
        get {
            if( _material == null ) {
                _material = GetComponent<Renderer>().material;
            }
            return _material;
        }
    }

    private const string _outlineThicknessProperty = "_OutlineThickness";
    private const float _outlineActiveValue = 2f;
    private const float _outlineInactiveValue = 0f;

    [SerializeField] protected int _interactionLayer = 0;
    public int interactionLayer
    {
        get {
            return _interactionLayer;
        }
    }


    protected void Awake()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if( spriteRenderer != null ) {
            string layerName = spriteRenderer.sortingLayerName;
        }
    }

    public void ShowInteraction()
    {
        if( interactableMaterial != null ) {
            interactableMaterial.SetFloat( _outlineThicknessProperty, _outlineActiveValue );
        }
    }


    public void HideInteraction()
    {
        if( interactableMaterial != null ) {
            interactableMaterial.SetFloat( _outlineThicknessProperty, _outlineInactiveValue );
        }
    }
    

}
