using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Nez.Console;

public partial class DebugConsole
{
    private const float UNDERSCORE_TIME = 0.5f;
    private const float OPACITY = 0.65f;

    // render constants
    private const int LINE_HEIGHT = 10;
    private const int TEXT_PADDING_X = 5;
    private const int TEXT_PADDING_Y = 4;

    /// <summary>
    ///     separation of the command entry and history boxes
    /// </summary>
    private const int COMMAND_HISTORY_PADDING = 10;

    /// <summary>
    ///     global padding on the left/right of the console
    /// </summary>
    private const int HORIZONTAL_PADDING = 10;

    private static readonly Regex _charFilter =
        new(@"[^a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/? \r\n]+", RegexOptions.Compiled);

    private readonly List<string> _commandHistory;
    private readonly Dictionary<string, CommandInfo> _commands;
    private readonly List<string> _drawCommands;

    /// <summary>
    ///     bind any custom Actions you would like to function keys
    /// </summary>
    private readonly Action[] _functionKeyActions;

    private readonly List<string> _sorted;
    private string _currentText = "";
#if TRACE
    public RuntimeInspector _runtimeInspector;
#endif
    private int _seekIndex = -1;
    private int _tabIndex = -1;
    private string _tabSearch;
    private bool _underscore;
    private float _underscoreCounter;
    public bool IsOpen;
    private bool wasMouseVisible;


    static DebugConsole()
    {
        Instance = new DebugConsole();
    }


    public DebugConsole()
    {
        _commandHistory = [];
        _drawCommands = [];
        _commands = new Dictionary<string, CommandInfo>();
        _sorted = [];
        _functionKeyActions = new Action[12];

        Core.Instance.Window.TextInput += TextInput;
        BuildCommandsList();
    }

    public static DebugConsole Instance { get; }

    /// <summary>
    ///     controls the scale of the console
    /// </summary>
    public static float RenderScale { get; set; } = 2f;

    private void TextInput(object sender, TextInputEventArgs e)
    {
        var c = e.Character;
        switch (c)
        {
            case '\b': //Backspace
                if (IsOpen && _currentText.Length > 0)
                    _currentText = _currentText.Substring(0, _currentText.Length - 1);
                break;
            case '\t': //Tab
                if (!IsOpen) break;
                if (InputUtils.IsShiftDown())
                {
                    if (_tabIndex == -1)
                    {
                        _tabSearch = _currentText;
                        FindLastTab();
                    }
                    else
                    {
                        _tabIndex--;
                        if (_tabIndex < 0 || (_tabSearch != "" && _sorted[_tabIndex].IndexOf(_tabSearch, StringComparison.Ordinal) != 0))
                            FindLastTab();
                    }
                }
                else
                {
                    if (_tabIndex == -1)
                    {
                        _tabSearch = _currentText;
                        FindFirstTab();
                    }
                    else
                    {
                        _tabIndex++;
                        if (_tabIndex >= _sorted.Count ||
                            (_tabSearch != "" && _sorted[_tabIndex].IndexOf(_tabSearch, StringComparison.Ordinal) != 0))
                            FindFirstTab();
                    }
                }

                if (_tabIndex != -1)
                    _currentText = _sorted[_tabIndex];
                break;
            case '\r': //Return
                if (IsOpen && _currentText.Length > 0)
                    EnterCommand();
                break;
            case '\u007F': //Delete
                _currentText = "";
                break;
            case '~': //Open/Close Console
                if (IsOpen)
                {
                    IsOpen = false;
                    Core.Instance.IsMouseVisible = wasMouseVisible;
                }
                else
                {
                    IsOpen = true;
                    wasMouseVisible = Core.Instance.IsMouseVisible;
                    Core.Instance.IsMouseVisible = true;
                }

                break;
            default:
                if (IsOpen) _currentText += _charFilter.Replace(c.ToString(), "");
                break;
        }
    }

    public void Log(Exception e)
    {
        Log(e.Message);

        var str = e.StackTrace;
        var parts = str!.Split(["\n"], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in parts)
        {
            var lineWithoutPath = MyRegex().Replace(line, "$1");
            Log(lineWithoutPath);
        }
    }


    public void Log(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }


    public void Log(object obj)
    {
        Log(obj.ToString());
    }


    public void Log(string str)
    {
        str = _charFilter.Replace(str, "#");
        // split up multi-line logs and log each line seperately
        var parts = str.Split(["\n"], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1)
        {
            foreach (var line in parts)
                Log(line);
            return;
        }

        // Split the string if you overlow horizontally
        var maxWidth = Core.GraphicsDevice.PresentationParameters.BackBufferWidth - 40;
        var screenHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;

        while (Graphics.Instance.BitmapFont.MeasureString(str).X * RenderScale > maxWidth)
        {
            var split = -1;
            for (var i = 0; i < str.Length; i++)
                if (str[i] == ' ')
                {
                    if (Graphics.Instance.BitmapFont.MeasureString(str.Substring(0, i)).X * RenderScale <= maxWidth)
                        split = i;
                    else
                        break;
                }

            if (split == -1)
                break;

            _drawCommands.Insert(0, str.Substring(0, split));
            str = str.Substring(split + 1);
        }

        _drawCommands.Insert(0, str);

        // Don't overflow top of window
        var maxCommands = (screenHeight - 100) / 30;
        while (_drawCommands.Count > maxCommands)
            _drawCommands.RemoveAt(_drawCommands.Count - 1);
    }


    #region Updating and Rendering

    internal void Update()
    {
        //Handle function keys
        for (var i = 0; i < _functionKeyActions.Length; i++)
            if (Input.IsKeyPressed(Keys.F1 + i))
                ExecuteFunctionKeyAction(i);

        if (IsOpen)
        {
            //Make cursor blink
            _underscoreCounter += Time.UnscaledDeltaTime;
            while (_underscoreCounter >= UNDERSCORE_TIME)
            {
                _underscoreCounter -= UNDERSCORE_TIME;
                _underscore = !_underscore;
            }

            if (Input.IsKeyPressed(Keys.Up))
            {
                if (_seekIndex < _commandHistory.Count - 1)
                {
                    _seekIndex++;
                    _currentText = string.Join(" ", _commandHistory[_seekIndex]);
                }
            }
            else if (Input.IsKeyPressed(Keys.Down))
            {
                if (_seekIndex > -1)
                {
                    _seekIndex--;
                    if (_seekIndex == -1)
                        _currentText = "";
                    else
                        _currentText = string.Join(" ", _commandHistory[_seekIndex]);
                }
            }
        }
    }

    private void EnterCommand()
    {
        var data = _currentText.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (_commandHistory.Count == 0 || _commandHistory[0] != _currentText)
            _commandHistory.Insert(0, _currentText);
        _drawCommands.Insert(0, "> " + _currentText);
        _currentText = "";
        _seekIndex = -1;

        if (data.Length == 0)
            return;

        var args = new string[data.Length - 1];
        for (var i = 1; i < data.Length; i++)
            args[i - 1] = data[i];
        ExecuteCommand(data[0].ToLower(), args);
    }

    private void FindFirstTab()
    {
        for (var i = 0; i < _sorted.Count; i++)
            if (_tabSearch == "" || _sorted[i].IndexOf(_tabSearch, StringComparison.Ordinal) == 0)
            {
                _tabIndex = i;
                break;
            }
    }

    private void FindLastTab()
    {
        for (var i = 0; i < _sorted.Count; i++)
            if (_tabSearch == "" || _sorted[i].IndexOf(_tabSearch, StringComparison.Ordinal) == 0)
                _tabIndex = i;
    }


    internal void Render()
    {
#if TRACE
        if (_runtimeInspector != null)
        {
            _runtimeInspector.Update();
            _runtimeInspector.Render();
        }
#endif

        if (!IsOpen)
            return;

        var screenWidth = Screen.BackbufferWidth;
        var screenHeight = Screen.BackbufferHeight;
        var workingWidth = screenWidth - 2 * HORIZONTAL_PADDING;

        Graphics.Instance.Batcher.Begin();

        // setup the rect that encompases the command entry section
        var commandEntryRect = RectangleExt.FromFloats(HORIZONTAL_PADDING, screenHeight - LINE_HEIGHT * RenderScale,
            workingWidth, LINE_HEIGHT * RenderScale);

        // take into account text padding. move our location up a bit and expand the Rect to accommodate
        commandEntryRect.Location -= new Point(0, TEXT_PADDING_Y * 2);
        commandEntryRect.Height += TEXT_PADDING_Y * 2;

        Graphics.Instance.Batcher.DrawRect(commandEntryRect, Color.Black * OPACITY);
        var commandLineString = "> " + _currentText;
        if (_underscore)
            commandLineString += "_";

        var commandTextPosition =
            commandEntryRect.Location.ToVector2() + new Vector2(TEXT_PADDING_X, TEXT_PADDING_Y);
        Graphics.Instance.Batcher.DrawString(Graphics.Instance.BitmapFont, commandLineString, commandTextPosition,
            Color.White, 0, Vector2.Zero, new Vector2(RenderScale), SpriteEffects.None, 0);

        if (_drawCommands.Count > 0)
        {
            // start with the total height of the text then add in padding. We have an extra padding because we pad each line and the top/bottom
            var height = LINE_HEIGHT * RenderScale * _drawCommands.Count;
            height += (_drawCommands.Count + 1) * TEXT_PADDING_Y;

            var topOfHistoryRect = commandEntryRect.Y - height - COMMAND_HISTORY_PADDING;
            Graphics.Instance.Batcher.DrawRect(HORIZONTAL_PADDING, topOfHistoryRect, workingWidth, height,
                Color.Black * OPACITY);

            var yPosFirstLine = topOfHistoryRect + height - TEXT_PADDING_Y - LINE_HEIGHT * RenderScale;
            for (var i = 0; i < _drawCommands.Count; i++)
            {
                var yPosCurrentLineAddition = i * LINE_HEIGHT * RenderScale + i * TEXT_PADDING_Y;
                var position = new Vector2(HORIZONTAL_PADDING + TEXT_PADDING_X,
                    yPosFirstLine - yPosCurrentLineAddition);
                var color = _drawCommands[i].IndexOf(">", StringComparison.Ordinal) == 0 ? Color.Yellow : Color.White;
                Graphics.Instance.Batcher.DrawString(Graphics.Instance.BitmapFont, _drawCommands[i], position,
                    color, 0, Vector2.Zero, new Vector2(RenderScale), SpriteEffects.None, 0);
            }
        }

        Graphics.Instance.Batcher.End();
    }

    #endregion


    #region Execute

    private void ExecuteCommand(string command, string[] args)
    {
        if (_commands.TryGetValue(command, out var command1))
            command1.Action(args);
        else
            Log("Command '" + command + "' not found! Type 'help' for list of commands");
    }

    private void ExecuteFunctionKeyAction(int num)
    {
        if (_functionKeyActions[num] != null)
            _functionKeyActions[num]();
    }


    public static void BindActionToFunctionKey(int functionKey, Action action)
    {
        Instance._functionKeyActions[functionKey - 1] = action;
    }

    /// <summary>
    ///     binds a debug console command to a function key
    /// </summary>
    /// <param name="functionKey">The function (e.g. 1 for F1).</param>
    /// <param name="command">The name of the command.</param>
    /// <param name="args">Optional list of arguments.</param>
    public static void BindCommandToFunctionKey(int functionKey, string command, params string[] args)
    {
        Instance._functionKeyActions[functionKey - 1] = () => Instance.ExecuteCommand(command, args);
    }

    #endregion


    #region Parse Commands

    private void BuildCommandsList()
    {
        // this will get us the Nez assembly
        ProcessAssembly(typeof(DebugConsole).GetTypeInfo().Assembly);

        // this will get us the current executables assembly in 99.9% of cases
        // for now we will let the next section handle loading this. If it doesnt work out we'll uncomment this
        ProcessAssembly(Core._instance.GetType().GetTypeInfo().Assembly);

        try
        {
            // this is a nasty hack that lets us get at all the assemblies. It is only allowed to exist because this will never get
            // hit in a release build.
            var appDomainType = typeof(string).GetTypeInfo().Assembly.GetType("System.AppDomain");
            var domain = appDomainType?.GetRuntimeProperty("CurrentDomain")
                ?.GetMethod?.Invoke(null, []);
            var assembliesMethod = ReflectionUtils.GetMethodInfo(domain, "GetAssemblies", []);

            // not sure about arguments, detect in runtime
            var methodCallParams = assembliesMethod.GetParameters().Length == 0
                ? Array.Empty<object>()
                : new object[] { false };
            var assemblies = (Assembly[])assembliesMethod.Invoke(domain, methodCallParams);

            var ignoredAssemblies = new[]
            {
                "mscorlib", "MonoMac", "MonoGame.Framework", "Mono.Security", "System", "OpenTK",
                "ObjCImplementations", "Nez", "Steamworks.NET"
            };
            foreach (var assembly in assemblies!)
            {
                var name = assembly.GetName().Name!;
                if (name.StartsWith("System.") || ignoredAssemblies.Contains(name))
                    continue;

                ProcessAssembly(assembly);
            }
        }
        catch (Exception e)
        {
            Debug.Log("DebugConsole pooped itself trying to get all the loaded assemblies. {0}", e);
        }


        // Maintain the sorted command list
        foreach (var command in _commands)
            _sorted.Add(command.Key);
        _sorted.Sort();
    }

    private void ProcessAssembly(Assembly assembly)
    {
        try
        {
            foreach (var type in assembly.DefinedTypes)
            foreach (var method in type.DeclaredMethods)
            {
                CommandAttribute attr = null;
                attr = method.GetCustomAttribute<CommandAttribute>(false);

                if (attr != null)
                    ProcessMethod(method, attr);
            }
        }
        catch (Exception)
        {
        }
    }

    private void ProcessMethod(MethodInfo method, CommandAttribute attr)
    {
        if (!method.IsStatic)
            throw new Exception(method.DeclaringType!.Name + "." + method.Name +
                                " is marked as a command, but is not static");

        var info = new CommandInfo
        {
            Help = attr.Help
        };

        var parameters = method.GetParameters();
        var defaults = new object[parameters.Length];
        var usage = new string[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            usage[i] = p.Name + ":";

            if (p.ParameterType == typeof(string))
                usage[i] += "string";
            else if (p.ParameterType == typeof(int))
                usage[i] += "int";
            else if (p.ParameterType == typeof(float))
                usage[i] += "float";
            else if (p.ParameterType == typeof(bool))
                usage[i] += "bool";
            else
                throw new Exception(method.DeclaringType!.Name + "." + method.Name +
                                    " is marked as a command, but has an invalid parameter type. Allowed types are: string, int, float, and bool");

            // no System.DBNull in PCL so we fake it
            if (p.DefaultValue!.GetType().FullName == "System.DBNull")
            {
                defaults[i] = null;
            }
            else if (p.DefaultValue != null)
            {
                defaults[i] = p.DefaultValue;
                if (p.ParameterType == typeof(string))
                    usage[i] += "=\"" + p.DefaultValue + "\"";
                else
                    usage[i] += "=" + p.DefaultValue;
            }
            else
            {
                defaults[i] = null;
            }
        }

        if (usage.Length == 0)
            info.Usage = "";
        else
            info.Usage = "[" + string.Join(" ", usage) + "]";

        info.Action = args =>
        {
            if (parameters.Length == 0)
            {
                method.Invoke(null, null);
            }
            else
            {
                var param = (object[])defaults.Clone();

                for (var i = 0; i < param.Length && i < args.Length; i++)
                    if (parameters[i].ParameterType == typeof(string))
                        param[i] = ArgString(args[i]);
                    else if (parameters[i].ParameterType == typeof(int))
                        param[i] = ArgInt(args[i]);
                    else if (parameters[i].ParameterType == typeof(float))
                        param[i] = ArgFloat(args[i]);
                    else if (parameters[i].ParameterType == typeof(bool))
                        param[i] = ArgBool(args[i]);

                try
                {
                    method.Invoke(null, param);
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        };

        _commands[attr.Name] = info;
    }

    private struct CommandInfo
    {
        public Action<string[]> Action;
        public string Help;
        public string Usage;
    }


    #region Parsing Arguments

    private static string ArgString(string arg)
    {
        if (arg == null)
            return "";
        return arg;
    }

    private static bool ArgBool(string arg)
    {
        if (arg != null)
            return !(arg == "0" || arg.ToLower() == "false" || arg.ToLower() == "f");
        return false;
    }

    private static int ArgInt(string arg)
    {
        try
        {
            return Convert.ToInt32(arg);
        }
        catch
        {
            return 0;
        }
    }

    private static float ArgFloat(string arg)
    {
        try
        {
            return Convert.ToSingle(arg);
        }
        catch
        {
            return 0;
        }
    }

    [GeneratedRegex(@"in\s\/.*?\/.*?(\w+\.cs)")]
    private static partial Regex MyRegex();

    #endregion

    #endregion
}