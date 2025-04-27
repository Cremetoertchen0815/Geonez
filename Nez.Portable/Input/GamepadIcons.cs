using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public class GamepadIcons
{
    public enum GamepadButton
    {
        A,
        B,
        X,
        Y,
        LS,
        RS,
        RB,
        LB,
        RT,
        LT
    }

    private readonly Dictionary<GamepadButton, Texture2D> icons = new();

    private GamepadIcons()
    {
    }

    public static GamepadIcons Instance { get; private set; } = new();

    public Texture2D GetIcon(GamepadButton g)
    {
        return icons[g];
    }

    public void LoadXboxIcons()
    {
        icons.Clear();
        icons.Add(GamepadButton.A, Core.Content.LoadTexture("engine/tex/gamepad/xbox/a"));
        icons.Add(GamepadButton.B, Core.Content.LoadTexture("engine/tex/gamepad/xbox/b"));
        icons.Add(GamepadButton.X, Core.Content.LoadTexture("engine/tex/gamepad/xbox/x"));
        icons.Add(GamepadButton.Y, Core.Content.LoadTexture("engine/tex/gamepad/xbox/y"));
        icons.Add(GamepadButton.RB, Core.Content.LoadTexture("engine/tex/gamepad/xbox/bumper_right"));
        icons.Add(GamepadButton.LB, Core.Content.LoadTexture("engine/tex/gamepad/xbox/bumper_right"));
        icons.Add(GamepadButton.LT, Core.Content.LoadTexture("engine/tex/gamepad/xbox/trigger_left"));
        icons.Add(GamepadButton.RT, Core.Content.LoadTexture("engine/tex/gamepad/xbox/trigger_right"));
    }
}