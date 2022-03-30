using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassFillEventController : MonoBehaviour
{
    public SpriteRenderer LiquidInsideSprite;

    private void Awake()
    {
        if( LiquidInsideSprite != null ) {
            var color = LiquidInsideSprite.color;
            LiquidInsideSprite.color = new Color( color.r, color.g, color.b, 0f );
        }
    }

    public void FillGlass()
    {
        if( LiquidInsideSprite != null ) {
            var color = LiquidInsideSprite.color;
            LiquidInsideSprite.color = new Color( color.r, color.g, color.b, 1f );
        }
    }

}
