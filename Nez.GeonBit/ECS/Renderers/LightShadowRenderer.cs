﻿using Nez.GeonBit.Lights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.GeonBit.ECS.Renderers
{
	/// <summary>
	/// Renders the shadow maps for all GeonBit light sources.
	/// Needs to be rendered before the main GeonDefaultRenderer!
	/// </summary>
	public class GeonShadowMapRenderer : Renderer
	{
		private GeonDefaultRenderer renderer = null;
        public GeonShadowMapRenderer(int layer) : base(layer)
		{ }
		
		public override bool WantsToRenderToSceneRenderTarget => true;

        public override void Render(Scene scene)
        {
            if (scene is not GeonScene gs) return;
            (renderer ?? (renderer = scene.GetRenderer<GeonDefaultRenderer>())).RenderShadows(gs);
        }
    }
}
