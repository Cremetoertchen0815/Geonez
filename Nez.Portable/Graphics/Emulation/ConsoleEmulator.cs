using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Nez;

public class ConsoleEmulator : RenderableComponent, IUpdatable
{
    public Color[] colorPalette;
    public IFont font;

    public Thread gameThread;
    public Vector2 glyphSize;
    private KeyboardState lastkstate;


    public ConsoleEmulator(Action gameStart)
    {
        gameThread = new Thread(() => gameStart()) { IsBackground = true };
    }

    public override float Width => ConsoleInterface.Width * glyphSize.X;
    public override float Height => ConsoleInterface.Height * glyphSize.Y;

    public void Update()
    {
        lock (ConsoleInterface._keylock)
        {
            //Fetch keybord state
            var kstate = Keyboard.GetState();

            //Grab regular key presses
            foreach (var key in kstate.GetPressedKeys())
            {
                if (lastkstate.IsKeyDown(key) || key == Keys.LeftShift || key == Keys.RightShift ||
                    key == Keys.LeftControl || key == Keys.RightControl || key == Keys.LeftAlt ||
                    key == Keys.RightAlt || key == Keys.None) continue; //Ignore last already pressed keys
                ConsoleInterface.pressedKeys.Add((ConsoleKey)key);
            }

            //Check for special keys
            ConsoleInterface.shiftP = (kstate.IsKeyDown(Keys.LeftShift) || kstate.IsKeyDown(Keys.RightShift)) ^
                                      kstate.CapsLock;
            ConsoleInterface.altP = kstate.IsKeyDown(Keys.LeftAlt) || kstate.IsKeyDown(Keys.RightAlt);
            ConsoleInterface.ctrlP = kstate.IsKeyDown(Keys.LeftControl) || kstate.IsKeyDown(Keys.RightControl);

            lastkstate = kstate;
        }
    }

    public override void Initialize()
    {
        //Hook Console
        ConsoleInterface.Initialize();
        gameThread.Start();
        glyphSize = new Vector2(11, 24);
        font = new NezSpriteFont(Core.Content.Load<SpriteFont>("font/ConsoleFont"));

        //Load default color Palette
        colorPalette =
        [
            Color.Black, Color.DarkBlue, Color.DarkGreen, Color.DarkCyan, Color.DarkRed, Color.DarkMagenta,
            Color.DarkOrange, Color.Gray, Color.DarkGray, Color.Blue, Color.Green, Color.Cyan, Color.Red, Color.Magenta,
            Color.Yellow, Color.White
        ];
    }
    //public override void OnRemovedFromEntity() => gameThread.Suspend();

    public override void Render(Batcher batcher, Camera camera)
    {
        lock (ConsoleInterface._renderlock)
        {
            for (var x = 0; x < ConsoleInterface.Width; x++)
            for (var y = 0; y < ConsoleInterface.Height; y++)
                batcher.DrawString(font, ConsoleInterface.Characters[x, y].ToString(), glyphSize * new Vector2(x, y),
                    colorPalette[(int)ConsoleInterface.Colors[x, y]]);
        }
    }
}