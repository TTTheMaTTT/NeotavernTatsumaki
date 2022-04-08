using UnityEngine;
using PixelCrushers.DialogueSystem;

public class CustomDialogueLuaFunctions : MonoBehaviour
{

    private DialogueSystemController dialogueSystemController;

    private void Start()
    {
        dialogueSystemController = GetComponent<DialogueSystemController>();
    }

    void OnEnable()
    {
        Lua.RegisterFunction( "GetActorId", this, SymbolExtensions.GetMethodInfo( () => GetActorId( string.Empty ) ) );
    }

    void OnDisable()
    {
        Lua.UnregisterFunction( "GetActorId" );
    }

    public double GetActorId( string name )
    {
        Actor actor = dialogueSystemController?.masterDatabase?.GetActor( name );
        return actor == null ? -1 : actor.id ;
    }
}