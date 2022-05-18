// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A location asset. In Chat Mapper, locations are usually used to track the information about 
    /// locations within the simulation. The dialogue system doesn't do anything with locations, 
    /// but you're free to use them in your Lua code.
    /// </summary>
    [System.Serializable]
    public class Location : Asset
    {
        /// <summary>
        /// Location sprites.
        /// </summary>
        public List<Sprite> sprites = new List<Sprite>();

        /// <summary>
        /// Initializes a new Location.
        /// </summary>
        public Location() { }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceLocation">Source location.</param>
        public Location(Location sourceLocation) : base(sourceLocation as Asset) 
        {
            this.sprites = new List<Sprite>( sourceLocation.sprites );
        }

        /// <summary>
        /// Initializes a new Location copied from a Chat Mapper location asset.
        /// </summary>
        /// <param name='chatMapperLocation'>
        /// The Chat Mapper location.
        /// </param>
        public Location(ChatMapper.Location chatMapperLocation)
        {
            Assign(chatMapperLocation);
        }

        /// <summary>
        /// Copies a Chat Mapper location asset.
        /// </summary>
        /// <param name='chatMapperLocation'>
        /// The Chat Mapper location.
        /// </param>
        public void Assign(ChatMapper.Location chatMapperLocation)
        {
            if (chatMapperLocation != null) Assign(chatMapperLocation.ID, chatMapperLocation.Fields);
        }


        /// <summary>
        /// Gets the location sprite at a specific index, where <c>1</c> is the default
        /// </summary>
        /// <returns>The portrait image.</returns>
        /// <param name="i">The index number of the portrait image.</param>
        public Sprite GetPortraitSprite( int i )
        {
            int index = i - 1;
            return (0 <= index && index < sprites.Count) ? sprites[index] : null;
        }

        /// <summary>
        /// Gets the first location sprite.
        /// </summary>
        /// <returns></returns>
        public Sprite GetDefaultPortraitSprite()
        {
            return GetPortraitSprite( 1 );
        }

    }

}
