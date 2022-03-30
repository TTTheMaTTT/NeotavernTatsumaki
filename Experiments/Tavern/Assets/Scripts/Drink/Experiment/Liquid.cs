using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Liquid", menuName = "Tatsumaki/Liquid")]
public class Liquid : ScriptableObject
{
    [SerializeField]
    private new string name;

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    [SerializeField]
    private Color color;

    public Color LiquidColor { get { return color; } set { color = value; } }


    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }


    public override bool Equals( object obj )
    {
        return Equals( obj as Liquid );
    }


    public bool Equals( Liquid obj )
    {
        return obj != null && obj.Name == this.Name && obj.LiquidColor == this.LiquidColor;
    }

}
