using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableAbstract : MonoBehaviour
{

    private void OnMouseDown()
    {
        StartInteraction();
    }

    private void OnMouseUp()
    {
        StopInteraction();
    }

    protected void StartInteraction()
    {
        var gameControllerInstance = GameControllerAbstract.Instance() as GameControllerBarInside;
        if( gameControllerInstance != null ) {
            gameControllerInstance.StartInteraction( this );
        }
    }

    protected void StopInteraction()
    {
        var gameControllerInstance = GameControllerAbstract.Instance() as GameControllerBarInside;
        if( gameControllerInstance != null ) {
            gameControllerInstance.StopInteraction( this );
        }
    }

}
