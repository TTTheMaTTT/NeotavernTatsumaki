using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    public enum PortraitType
    {
        Texture,
        Sprite
    }

    public enum ActorArrangementActionType
    {
        None,
        ChangePosition,
        Leave
    }

    public enum ActorScreenPositionType
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// Description of actor state changes during the dialogue entry.
    /// Consists of custom fields, that can be described in the templates tab
    /// </summary>
    [System.Serializable]
    public class ActorState
    {

        public ActorState()
        {
            fields = new List<Field>();
            ActorID = -1;
            PortraitIndex = 1;// portraits counts from 1. 1 means default portrait
            PortraitType = PortraitType.Sprite;
        }

        /// <summary>
        /// The dialogue entry's field list. (See Field)
        /// </summary>
        public List<Field> fields = null;

        /// <summary>
        /// Gets or sets the ID of the line's actor
        /// </summary>
        /// <value>
        /// The ID of the actor.
        /// </value>
        public int ActorID
        {
            get {
                return Field.LookupInt( fields, "Actor" );
            }
            set {
                Field.SetValue( fields, "Actor", value.ToString(), FieldType.Actor );
            }
        }

        public Field ActorField
        {
            get {
                return Field.Lookup(fields, "Actor");
            }
        }

        /// <summary>
        /// Gets or sets the index of portrait
        /// </summary>
        /// <value>
        /// The index of the actor's portrait.
        /// </value>
        public int PortraitIndex
        {
            get {
                return Field.LookupInt( fields, "Portrait Index" );
            }
            set {
                Field.SetValue( fields, "Portrait Index", value.ToString(), FieldType.Number );
            }
        }

        /// <summary>
        /// What kind of portrait actor should use? If true, than textures, otherwise sprites.
        /// </summary>
        /// <value>
        /// The index of the actor's portrait.
        /// </value>
        public PortraitType PortraitType
        {
            get {
                string portraitTypeString = Field.LookupValue( fields, "Portrait Type" );
                return (PortraitType)Enum.Parse( typeof( PortraitType ), portraitTypeString, true );
            }
            set {
                Field.SetValue( fields, "Portrait Type", value.ToString(), FieldType.Boolean );
            }
        }

    }
}