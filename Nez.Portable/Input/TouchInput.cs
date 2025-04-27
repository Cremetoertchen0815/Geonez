using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;

namespace Nez;

/// <summary>
///     to enable touch input you must first call enableTouchSupport()
/// </summary>
public class TouchInput
{
    public bool IsConnected { get; private set; }

    public TouchCollection CurrentTouches { get; private set; }

    public TouchCollection PreviousTouches { get; private set; }

    public List<GestureSample> PreviousGestures { get; } = new();

    public List<GestureSample> CurrentGestures { get; } = new();

    private void OnGraphicsDeviceReset()
    {
        TouchPanel.DisplayWidth = Core.GraphicsDevice.Viewport.Width;
        TouchPanel.DisplayHeight = Core.GraphicsDevice.Viewport.Height;
        TouchPanel.DisplayOrientation = Core.GraphicsDevice.PresentationParameters.DisplayOrientation;
    }


    internal void Update()
    {
        if (!IsConnected)
            return;

        PreviousTouches = CurrentTouches;
        CurrentTouches = TouchPanel.GetState();

        PreviousGestures.Clear();
        PreviousGestures.AddRange(CurrentGestures);
        CurrentGestures.Clear();
        while (TouchPanel.IsGestureAvailable)
            CurrentGestures.Add(TouchPanel.ReadGesture());
    }


    public void EnableTouchSupport()
    {
        IsConnected = TouchPanel.GetCapabilities().IsConnected;

        if (IsConnected)
        {
            Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
            Core.Emitter.AddObserver(CoreEvents.OrientationChanged, OnGraphicsDeviceReset);
            OnGraphicsDeviceReset();
        }
    }
}