﻿using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Console;

namespace Nez.GeonBit.UI;

public class GeonUIManager : GlobalManager, IFinalRenderDelegate
{
    private static readonly Regex _charFilter =
        new(@"[^a-zÀ-ÿA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/? \r\n\b\u007F]+", RegexOptions.Compiled);

    private readonly SpriteBatch _batch = new(Core.GraphicsDevice);
    private readonly GameTime _gt = new();
    private readonly UserInterface _ui;

    public GeonUIManager()
    {
        if (UserInterface.Active == null) UserInterface.Initialize(Core.Content);
        _ui = UserInterface.Active;
        _ui.UseRenderTarget = true;
        Core.Instance.Window.TextInput += (o, e) =>
            UserInterface.Input.TextInput(_charFilter.Replace(e.Character.ToString(), "#")[0]);
        //Let the 
        Core.Emitter.AddObserver(CoreEvents.SceneChanged, OnSceneChanged);
    }

    public void HandleFinalRender(RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source,
        Rectangle finalRenderDestinationRect, SamplerState samplerState)
    {
#if TRACE
        var seg = DeltaAnalyzer.MeasureSegment("GeonUIManager", null, DeltaAnalyzer.DeltaSegmentType.Draw);
#endif
        _ui.Draw(_batch, source);

        _batch.Begin();
        _batch.Draw(source, finalRenderDestinationRect, Color.White);
        _batch.End();

        _ui.DrawMainRenderTarget(_batch, finalRenderDestinationRect);

#if TRACE
        seg.Stop();
#endif
    }

    public void OnAddedToScene(Scene scene)
    {
    }

    public void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
    {
        //Tell UI that it shall change resolution
    }

    public void Unload()
    {
    }

    private void OnSceneChanged()
    {
        UserInterface.Active.OnSceneChange();
        if (Core.Scene is null) return;
        Core.Scene.FinalRenderDelegate = this;
    }


    [Command("disable-geon", "Exits the game.")]
    public static void Disable()
    {
        Core.Scene.FinalRenderDelegate = null;
    }

    public override void Update()
    {
        _gt.ElapsedGameTime = TimeSpan.FromMilliseconds(Time.DeltaTime);
        _ui.Update(_gt);
    }
}