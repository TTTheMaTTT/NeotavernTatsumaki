using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    public enum ActorArrangementActionType
    {
        ChangePosition,
        Leave
    }

    public enum ActorScreenPositionType
    {
        Left,
        Center,
        Right,
        Custom
    }

    /// <summary>
    /// Description of actor state changes during the dialogue entry.
    /// Consists of custom fields, that can be described in the templates tab
    /// </summary>
    public class ActorState
    {
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

        public bool HasPortraitIndex()
        {
            return Field.LookupValue( fields, "Portrait Index" ) != null;
        }

    }
}