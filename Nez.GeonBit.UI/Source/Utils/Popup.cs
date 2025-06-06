﻿#region File Description

//-----------------------------------------------------------------------------
// Generate message boxes and other prompts.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nez.GeonBit.UI.Entities;

namespace Nez.GeonBit.UI.Utils;

/// <summary>
///     GeonBit.UI.Utils contain different utilities and helper classes to use GeonBit.UI.
/// </summary>
[CompilerGenerated]
internal class NamespaceDoc
{
}

/// <summary>
///     Helper class to generate message boxes and prompts.
/// </summary>
public static class Popup
{
    public enum ButtonType
    {
        Confirm,
        Cancel,
        None
    }

    /// <summary>
    ///     Default size to use for message boxes.
    /// </summary>
    public static Vector2 DefaultMsgBoxSize = new(1000f, 750f);

    /// <summary>
    ///     Default text for OK button.
    /// </summary>
    public static string DefaultOkButtonText = "OK";

    /// <summary>
    ///     Will block and fade background with this color while messages are opened.
    /// </summary>
    public static Color BackgroundFaderColor = new(0, 0, 0, 180);

    /// <summary>
    ///     Count currently opened message boxes.
    /// </summary>
    public static int OpenedMsgBoxesCount { get; private set; }

    /// <summary>
    ///     Get if there's a message box currently opened.
    /// </summary>
    public static bool IsMsgBoxOpened => OpenedMsgBoxesCount > 0;

    /// <summary>
    ///     Show a message box with custom buttons and callbacks.
    /// </summary>
    /// <param name="header">Messagebox header.</param>
    /// <param name="text">Main text.</param>
    /// <param name="options">Msgbox response options.</param>
    /// <returns>Message box panel.</returns>
    public static Task<int> ShowAsync(string header, string text, params string[] options)
    {
        var cs = new TaskCompletionSource<int>();
        Show(header, text, options.Select((x, index) => new PopupButton(x, () =>
        {
            cs.SetResult(index);
            return true;
        })).ToArray(), null);
        return cs.Task;
    }

    /// <summary>
    ///     Show a message box with custom buttons and callbacks.
    /// </summary>
    /// <param name="header">Messagebox header.</param>
    /// <param name="text">Main text.</param>
    /// <param name="options">Msgbox response options.</param>
    /// <returns>Message box panel.</returns>
    public static Panel Show(string header, string text, params PopupButton[] options)
    {
        return Show(header, text, options, null);
    }

    /// <summary>
    ///     Show a message box with just "OK".
    /// </summary>
    /// <param name="header">Message box title.</param>
    /// <param name="text">Main text to write on the message box.</param>
    /// <param name="closeButtonTxt">Text for the closing button (if not provided will use default).</param>
    /// <param name="size">Message box size (if not provided will use default).</param>
    /// <returns>Message box panel.</returns>
    public static Panel Show(string header, string text, string closeButtonTxt = null, Vector2? size = null)
    {
        UserInterface.GetCursorMode = UserInterface.CursorMode.Roaming;
        return Show(header, text, new[]
        {
            new PopupButton(closeButtonTxt ?? DefaultOkButtonText, null)
        }, size: size ?? DefaultMsgBoxSize);
    }

    /// <summary>
    ///     Show a message box with custom buttons and callbacks.
    /// </summary>
    /// <param name="header">Messagebox header.</param>
    /// <param name="text">Main text.</param>
    /// <param name="options">Msgbox response options.</param>
    /// <param name="append">Optional array of entities to add to msg box under the text and above the buttons.</param>
    /// <param name="size">Alternative size to use.</param>
    /// <param name="onDone">Optional callback to call when this msgbox closes.</param>
    /// <returns>Message box panel.</returns>
    public static Panel Show(string header, string text, PopupButton[] options, Entities.Entity[] append = null,
        Vector2? size = null, Action onDone = null)
    {
        UserInterface.GamePadModeEnabled = Input.GamePads[0]?.IsConnected() ?? false;

        // create panel for messagebox
        size = size ?? DefaultMsgBoxSize;
        var panel = new Panel(size.Value);
        panel.AddChild(new Header(header));
        panel.AddChild(new HorizontalLine());
        panel.AddChild(new Paragraph(text));

        // add to opened boxes counter
        OpenedMsgBoxesCount++;

        // add rectangle to hide and lock background
        ColoredRectangle fader = null;
        if (BackgroundFaderColor.A != 0)
        {
            fader = new ColoredRectangle(Vector2.Zero, Anchor.Center)
            {
                FillColor = new Color(0, 0, 0, 180),
                OutlineWidth = 0,
                ClickThrough = false
            };
            UserInterface.Active.AddEntity(fader);
        }

        // add custom appended entities
        if (append != null)
            foreach (var entity in append)
                panel.AddChild(entity);

        // add bottom buttons panel
        var buttonsPanel = new Panel(new Vector2(0, 105f), PanelSkin.None, Anchor.BottomCenter)
        {
            Padding = Vector2.Zero
        };
        panel.AddChild(buttonsPanel);

        // add all option buttons
        var btnSize = new Vector2(options.Length == 1 ? 0f : 1f / options.Length, 90);
        for (var i = 0; i < options.Length; i++)
        {
            var option = options[i];
            // add button entity
            var button = new Button(option.Title, anchor: Anchor.AutoInline, size: btnSize)
                { Selectable = true, IsFirstSelection = i == 0 };

            // set click event
            button.OnClick += ent =>
            {
                // if need to close message box after clicking this button, close it:
                if (option.Callback == null || option.Callback())
                {
                    // remove fader and msg box panel
                    if (fader != null) fader.RemoveFromParent();
                    panel.RemoveFromParent();

                    // decrease msg boxes count
                    OpenedMsgBoxesCount--;

                    // call on-done callback
                    onDone?.Invoke();
                }
            };

            if (i == 0 && UserInterface.GamePadModeEnabled) button.Select();

            // add button to buttons panel
            buttonsPanel.AddChild(button);
        }

        // add panel to active ui
        UserInterface.Active.AddEntity(panel);
        return panel;
    }

    public static Task<string> ShowInputAsync(string header, string text, string defValue, string confirmText,
        string cancelText)
    {
        var cs = new TaskCompletionSource<string>();
        ShowInput(header, text, defValue, new[]
        {
            new PopupInputOption(cancelText, _ =>
            {
                cs.SetResult(null);
                return true;
            }, ButtonType.Cancel),
            new PopupInputOption(confirmText, x =>
            {
                cs.SetResult(x);
                return true;
            }, ButtonType.Confirm)
        }, null);
        return cs.Task;
    }

    /// <summary>
    ///     Show an input box with custom buttons and callbacks.
    /// </summary>
    /// <param name="header">Inputbox header.</param>
    /// <param name="text">Main text.</param>
    /// <param name="options">Input response options.</param>
    /// <returns>Input box panel.</returns>
    public static Panel ShowInput(string header, string text, string defVal = null, params PopupInputOption[] options)
    {
        return ShowInput(header, text, defVal, options, null);
    }

    public static Panel ShowInput(string header, string text, params PopupInputOption[] options)
    {
        return ShowInput(header, text, null, options, null);
    }

    /// <summary>
    ///     Show an input box with custom buttons and callbacks.
    /// </summary>
    /// <param name="header">Inputbox header.</param>
    /// <param name="text">Main text.</param>
    /// <param name="options">Input response options.</param>
    /// <param name="append">Optional array of entities to add to msg box under the text and above the buttons.</param>
    /// <param name="size">Alternative size to use.</param>
    /// <param name="onDone">Optional callback to call when this inputbox closes.</param>
    /// <returns>Input box panel.</returns>
    public static Panel ShowInput(string header, string text, string defVal, PopupInputOption[] options,
        Entities.Entity[] append = null, Vector2? size = null, Action onDone = null)
    {
        UserInterface.GamePadModeEnabled = Input.GamePads[0]?.IsConnected() ?? false;

        // create panel for messagebox
        size = size ?? DefaultMsgBoxSize;
        var panel = new Panel(size.Value);
        panel.AddChild(new Header(header));
        panel.AddChild(new HorizontalLine());
        panel.AddChild(new Paragraph(text));
        var txt = panel.AddChild(new TextInput(false) { IsFocused = true, Value = defVal ?? string.Empty });

        // add to opened boxes counter
        OpenedMsgBoxesCount++;

        // add rectangle to hide and lock background
        ColoredRectangle fader = null;
        if (BackgroundFaderColor.A != 0)
        {
            fader = new ColoredRectangle(Vector2.Zero, Anchor.Center)
            {
                FillColor = new Color(0, 0, 0, 180),
                OutlineWidth = 0,
                ClickThrough = false
            };
            UserInterface.Active.AddEntity(fader);
        }

        // add custom appended entities
        if (append != null)
            foreach (var entity in append)
                panel.AddChild(entity);

        // add bottom buttons panel
        var buttonsPanel = new Panel(new Vector2(0, 105f), PanelSkin.None, Anchor.BottomCenter)
        {
            Padding = Vector2.Zero
        };
        panel.AddChild(buttonsPanel);

        // add all option buttons
        var btnSize = new Vector2(options.Length == 1 ? 0f : 1f / options.Length, 90);
        for (var i = 0; i < options.Length; i++)
        {
            var option = options[i];
            // add button entity
            var button = new Button(option.Title, anchor: Anchor.AutoInline, size: btnSize)
                { Selectable = true, IsFirstSelection = i == 0 };

            // set click event
            button.OnClick += ent =>
            {
                // if need to close message box after clicking this button, close it:
                if (option.Callback == null || option.Callback(txt.Value))
                {
                    // remove fader and msg box panel
                    if (fader != null) fader.RemoveFromParent();
                    panel.RemoveFromParent();

                    // decrease msg boxes count
                    OpenedMsgBoxesCount--;

                    // call on-done callback
                    onDone?.Invoke();
                }
            };

            // add button to buttons panel
            buttonsPanel.AddChild(button);

            if (i == 0 && UserInterface.GamePadModeEnabled) button.Select();
        }

        // add panel to active ui
        UserInterface.Active.AddEntity(panel);
        return panel;
    }

    /// <summary>
    ///     A button / option for a message box.
    /// </summary>
    public class PopupButton
    {
	    /// <summary>
	    ///     Callback to run when clicked. Return false to leave message box opened (true will close it).
	    /// </summary>
	    public Func<bool> Callback;

	    /// <summary>
	    ///     Option title (for the button).
	    /// </summary>
	    public string Title;

	    /// <summary>
	    ///     Determines the option type, which correlates to button shortcuts.
	    /// </summary>
	    public ButtonType Type;

	    /// <summary>
	    ///     Create the message box option.
	    /// </summary>
	    /// <param name="title">Text to write on the button.</param>
	    /// <param name="callback">
	    ///     Action when clicked. Return false if you want to abort and leave the message opened, return true
	    ///     to close it.
	    /// </param>
	    public PopupButton(string title, Func<bool> callback, ButtonType type = ButtonType.None)
        {
            Title = title;
            Callback = callback;
            Type = type;
        }
    }

    /// <summary>
    ///     A button / option for a message box.
    /// </summary>
    public class PopupInputOption
    {
	    /// <summary>
	    ///     Callback to run when clicked. Return false to leave message box opened (true will close it).
	    /// </summary>
	    public Func<string, bool> Callback;

	    /// <summary>
	    ///     Option title (for the button).
	    /// </summary>
	    public string Title;

	    /// <summary>
	    ///     Determines the option type, which correlates to button shortcuts.
	    /// </summary>
	    public ButtonType Type;

	    /// <summary>
	    ///     Create the message box option.
	    /// </summary>
	    /// <param name="title">Text to write on the button.</param>
	    /// <param name="callback">
	    ///     Action when clicked. Return false if you want to abort and leave the message opened, return true
	    ///     to close it.
	    /// </param>
	    public PopupInputOption(string title, Func<string, bool> callback, ButtonType type = ButtonType.None)
        {
            Title = title;
            Callback = callback;
            Type = type;
        }
    }
}