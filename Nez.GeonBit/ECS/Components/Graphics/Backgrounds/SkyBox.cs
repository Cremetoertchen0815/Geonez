﻿#region LICENSE
//-----------------------------------------------------------------------------
// For the purpose of making video games, educational projects or gamification,
// GeonBit is distributed under the MIT license and is totally free to use.
// To use this source code or GeonBit as a whole for other purposes, please seek 
// permission from the library author, Ronen Ness.
// 
// Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
// Do not remove this license notice.
//-----------------------------------------------------------------------------
#endregion
#region File Description
//-----------------------------------------------------------------------------
// A component that renders a 3D skybox.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit
{

    /// <summary>
    /// This component renders a 3d skybox.
    /// </summary>
    public class SkyBox : ModelRenderer
    {
        /// <summary>
        /// Skybox texture path.
        /// </summary>
        public string TexturePath { get; private set; }

        /// <summary>
        /// Default skybox texture.
        /// </summary>
        public static string DefaultTexture = "engine/tex/skybox";

        /// <summary>
        /// Create the skybox renderer component.
        /// </summary>
        /// <param name="texture">Skybox texture path (leave null for default texture).</param>
        public SkyBox(string texture = null) : base(ShapeRenderer.ShapeModelsRoot + "skybox")
        {
            TexturePath = texture ?? DefaultTexture;
            _entity.RenderingQueue = RenderingQueue.SolidBackNoCull;
            _entity.SetMaterial(new Materials.SkyboxMaterial(TexturePath, true));
        }

        /// <summary>
        /// Create the skybox renderer component.
        /// </summary>
        /// <param name="texture">Skybox texture path (leave null for default texture).</param>
        public SkyBox(Texture2D texture) : base(ShapeRenderer.ShapeModelsRoot + "skybox")
        {
            _entity.RenderingQueue = RenderingQueue.SolidBackNoCull;
            _entity.SetMaterial(new Materials.SkyboxMaterial(texture, true));
        }

        /// <summary>
        /// Clone this component.
        /// </summary>
        /// <returns>Cloned copy of this component.</returns>
        public override Component Clone()
        {
            var ret = CopyBasics(new SkyBox(TexturePath)) as SkyBox;
            return ret;
        }

        /// <summary>
        /// Called when this component spawns.
        /// </summary>
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            Node.DisableCulling = true;
        }
    }
}
