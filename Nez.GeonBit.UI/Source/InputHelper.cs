#region File Description
//-----------------------------------------------------------------------------
// Helper utility to get keyboard and mouse input.
// It provides easier access to the Input API, and in addition functions to
// measure changes between frames.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace Nez.GeonBit.UI
{

	/// <summary>
	/// Supported GamePad Buttons
	/// </summary>
	public enum GamePadButton
	{
		///<summary>GamePad A button.</summary>
		A_Button,
		///<summary>GamePad down button.</summary>
		DPadDown,
		///<summary>GamePad up button.</summary>
		/// <summary>
		/// DPAD-Up button
		/// </summary>
		DPadUp,
		/// <summary>
		/// DPAD-Left button
		/// </summary>
		DPadLeft,
		/// <summary>
		/// DPAD-Right button
		/// </summary>
		DPadRight
	};

	/// <summary>
	/// Provide easier keyboard and mouse access, keyboard text input, and other user input utils.
	/// </summary>
	public class InputHelper
	{
		// locks the mouse cursor position
		private bool _LockMousePosition = false;
		/// <summary>
		/// LockMousePosition
		/// </summary>
		public bool LockMousePosition
		{
			private get => _LockMousePosition;
			set => _LockMousePosition = value;
		}

		/// <summary>
		/// Is the A button on the gamepad currently being pressed?
		/// </summary>
		public bool IsAButtonPressed = false;

		// store current & previous keyboard states so we can detect key release
		private KeyboardState _newKeyboardState;
		private KeyboardState _oldKeyboardState;

		// store current & previous mouse states so we can detect key release and diff
		private MouseState _newMouseState;
		private MouseState _oldMouseState;
		private GamePadState _newGamePadState;
		private GamePadState _oldGamePadState;
		private Vector2 _newMousePos;
		private Keys[] _allKeyboardKeys = System.Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray();

        // store current frame gametime
        private GameTime _currTime;

		/// <summary>An artificial "lag" after a key is pressed when typing text input, to prevent mistake duplications.</summary>
		public float KeysTypeCooldown = 0.6f;

		// last character that was pressed down
		private char _currCharacterInput { get; set; } = '\0';

		private char? _kbdChar { get; set; } = null;

		/// <summary>
		/// Current mouse wheel value.
		/// </summary>
		public int MouseWheel = 0;

		/// <summary>
		/// Mouse wheel change sign (eg 0, 1 or -1) since last frame.
		/// </summary>
		public int MouseWheelChange = 0;

		/// <summary>
		/// ThumbStickLeftChange. Used for mouse cursor control (after dragging)
		/// </summary>
		public Vector2 ThumbStickLeftChange = Vector2.Zero;
		/// <summary>
		/// Defines if the left thumbstick should be used for dragging (sliders etc.)
		/// </summary>
		private bool _ThumbStickLeftDragging = true;
		/// <summary>
		/// Set this to true if you want to use the left thumbstick of your gamepad for dragging sliders etc.
		/// </summary>
		public bool ThumbStickLeftDragging
		{
			get => _ThumbStickLeftDragging;
			set => _ThumbStickLeftDragging = value;
		}

		/// <summary>
		/// Defines if the left thumbstick can be used.
		/// </summary>
		private bool _ThumbStickLeftCanDrag = false;
		/// <summary>
		/// Defines if the left thumbstick can be used (don't changed it manually! It's automatically set for you).
		/// </summary>
		public bool ThumbStickLeftCanDrag
		{
			get => _ThumbStickLeftCanDrag;
			set => _ThumbStickLeftCanDrag = value;
		}

		private float ThumbStickLeftCoolDown = 12f;
		private float ThumbStickLeftCoolDownMax = 12f;

		/// <summary>
		/// Create the input helper.
		/// </summary>
		public InputHelper()
		{
			// init keyboard states
			_newKeyboardState = _oldKeyboardState;

			// init mouse and gamepad states
			_newGamePadState = _oldGamePadState;
			_newMouseState = _oldMouseState;

			// call first update to get starting positions
			Update(new GameTime());
		}

		/// <summary>
		/// Current frame game time.
		/// </summary>
		public GameTime CurrGameTime => _currTime;

		/// <summary>
		/// Update current states.
		/// If used outside GeonBit.UI, this function should be called first thing inside your game 'Update()' function,
		/// and before you make any use of this class.
		/// </summary>
		/// <param name="gameTime">Current game time.</param>
		public void Update(GameTime gameTime)
		{
			// store game time
			_currTime = gameTime;

			// store previous states
			_oldMouseState = _newMouseState;
			_oldKeyboardState = _newKeyboardState;
			_oldGamePadState = _newGamePadState;

			// get new states
			_newMouseState = Mouse.GetState();
			_newKeyboardState = Keyboard.GetState();
			_newGamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

			// If the mouse was used, set to roaming cursor mode (as opposed to snapping mode for gamepad)
			if (Input.MousePositionDelta.X != 0 && Input.MousePositionDelta.Y != 0)
			{
				UserInterface.GetCursorMode = UserInterface.CursorMode.Roaming;
			}

			if (!_LockMousePosition)
			{
				_newMousePos = Input.MousePosition;
			}

			// get thumbstickleft state
			if (ThumbStickLeftDragging)
			{
				ThumbStickLeftChange = _newGamePadState.ThumbSticks.Right;
				if (ThumbStickLeftCanDrag == false && ThumbStickLeftCoolDown > 0)
				{
					ThumbStickLeftCoolDown--;
				}
				else
				{
					ThumbStickLeftCanDrag = true;
					ThumbStickLeftCoolDown = ThumbStickLeftCoolDownMax;
				}
			}

			// Update mouse state's position if cursor mode is set to roaming (used by gamepad)
			if (_newGamePadState.ThumbSticks.Right != Vector2.Zero && UserInterface.GamePadModeEnabled)
			{
				UserInterface.GetCursorMode = UserInterface.CursorMode.Roaming;
				UpdateCursorPosition(new Vector2(
				Input.RawMousePosition.X + (_newGamePadState.ThumbSticks.Right.X * 10),
				Input.RawMousePosition.Y + (-_newGamePadState.ThumbSticks.Right.Y * 10)));
			}

			// get mouse wheel state
			int prevMouseWheel = MouseWheel;
			MouseWheel = Input.MouseWheel;
			MouseWheelChange = System.Math.Sign(MouseWheel - prevMouseWheel);

			if (_kbdChar != null)
			{
				_currCharacterInput = _kbdChar.Value;
				_kbdChar = null;
				return;
			}

			_currCharacterInput = '\0';

			// send key-down events
			foreach (Keys key in _allKeyboardKeys)
			{
				if (_newKeyboardState.IsKeyDown(key) && !_oldKeyboardState.IsKeyDown(key))
				{

					// handle special keys and characters
					switch (key)
					{
						case Keys.Left:
							_currCharacterInput = (char)SpecialChars.ArrowLeft;
							return;

						case Keys.Right:
							_currCharacterInput = (char)SpecialChars.ArrowRight;
							return;

					};
				}
			}
		}

		/// <summary>
		/// Move the cursor to be at the center of the screen.
		/// </summary>
		/// <param name="pos">New mouse position.</param>
		public void UpdateCursorPosition(Vector2 pos)
		{
			// move mouse position back to center
			Mouse.SetPosition((int)pos.X, (int)pos.Y);
			_newMousePos = pos;
		}

		/// <summary>
		/// Calculate and return current cursor position transformed by a matrix.
		/// </summary>
		/// <param name="transform">Matrix to transform cursor position by.</param>
		/// <returns>Cursor position with optional transform applied.</returns>
		public Vector2 TransformCursorPos(Matrix? transform)
		{
			var newMousePos = _newMousePos;
			if (transform != null)
			{
				return Vector2.Transform(newMousePos, transform.Value) - new Vector2(transform.Value.Translation.X, transform.Value.Translation.Y);
			}
			return newMousePos;
		}

		/// <summary>
		/// Called every time a keyboard key is pressed (called once on the frame key was pressed).
		/// </summary>
		/// <param name="key">Key code that is being pressed on this frame.</param>
		public void TextInput(char e) => _kbdChar = e;

		/// <summary>
		/// Get textual input from keyboard.
		/// If user enter keys it will push them into string, if delete or backspace will remove chars, etc.
		/// This also handles keyboard cooldown, to make it feel like windows-input.
		/// </summary>
		/// <param name="txt">String to push text input into.</param>
		/// <param name="pos">Position to insert / remove characters. -1 to push at the end of string. After done, will contain actual new caret position.</param>
		/// <returns>String after text input applied on it.</returns>
		public string GetTextInput(string txt, ref int pos)
		{

			// if no valid characters are currently input
			if (_currCharacterInput == '\0')
			{
				return txt;
			}

			// get default position
			if (pos == -1)
			{
				pos = txt.Length;
			}

			// handle special chars
			switch (_currCharacterInput)
			{
				case (char)SpecialChars.ArrowLeft:
					if (--pos < 0) { pos = 0; }
					return txt;

				case (char)SpecialChars.ArrowRight:
					if (++pos > txt.Length) { pos = txt.Length; }
					return txt;

				case '\b': //Backspace
					pos--;
					return (pos < txt.Length && pos >= 0 && txt.Length > 0) ? txt.Remove(pos, 1) : txt;

				case '\u007F': //Delete
					return (pos < txt.Length && txt.Length > 0) ? txt.Remove(pos, 1) : txt;
			}

			// add current character
			return txt.Insert(pos++, _currCharacterInput.ToString());
		}

		/// <summary>
		/// Get current mouse poisition.
		/// </summary>
		public Vector2 MousePosition => _newMousePos;

		/// <summary>
		/// Get mouse position change since last frame.
		/// </summary>
		/// <return>Mouse position change as a 2d vector.</return>
		public Vector2 MousePositionDiff => Input.MousePositionDelta.ToVector2();

		/// <summary>
		/// Check if a given mouse button is down.
		/// </summary>
		/// <param name="button">Mouse button to check.</param>
		/// <return>True if given mouse button is down.</return>
		public bool MouseButtonHeldDown(MouseButton button = MouseButton.Left) => GetMouseButtonState(button) == ButtonState.Pressed;

		/// <summary>
		/// Return if any of mouse buttons is down.
		/// </summary>
		/// <returns>True if any mouse button is currently down.</returns>
		public bool AnyMouseButtonDown() => MouseButtonHeldDown(MouseButton.Left) ||
				MouseButtonHeldDown(MouseButton.Right) ||
				MouseButtonHeldDown(MouseButton.Middle);

		/// <summary>
		/// Check if a given mouse button was released in current frame.
		/// </summary>
		/// <param name="button">Mouse button to check.</param>
		/// <return>True if given mouse button was released in this frame.</return>
		public bool MouseButtonReleased(MouseButton button = MouseButton.Left) => GetMouseButtonState(button) == ButtonState.Released && GetMousePreviousButtonState(button) == ButtonState.Pressed;

		/// <summary>
		/// Check if a given mouse button is held down.
		/// </summary>
		/// <param name="button">Bouse button to check.</param>
		/// <return>True if given button is down.</return>
		public bool GamePadButtonHeldDown(GamePadButton button) => UserInterface.GamePadModeEnabled && GetGamePadButtonState(button) == ButtonState.Pressed;

		/// <summary>
		/// Check if a given gamepad button was released in current frame.
		/// </summary>
		/// <param name="button">GamePad button to check.</param>
		/// <return>True if given gamepad button was released in this frame.</return>
		public bool GamePadButtonReleased(GamePadButton button) => UserInterface.GamePadModeEnabled && GetGamePadButtonState(button) == ButtonState.Released && GetGamePadPreviousButtonState(button) == ButtonState.Pressed;

		/// <summary>
		/// Check if a given gamepad button was pressed in current frame.
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		public bool GamePadButtonPressed(GamePadButton button) => UserInterface.GamePadModeEnabled && GetGamePadButtonState(button) == ButtonState.Pressed && GetGamePadPreviousButtonState(button) == ButtonState.Released;

		/// <summary>
		/// Return if any mouse button was released this frame.
		/// </summary>
		/// <returns>True if any mouse button was released.</returns>
		public bool AnyMouseButtonReleased() => MouseButtonReleased(MouseButton.Left) ||
				MouseButtonReleased(MouseButton.Right) ||
				MouseButtonReleased(MouseButton.Middle);

		/// <summary>
		/// Check if a given mouse button was pressed in current frame.
		/// </summary>
		/// <param name="button">Mouse button to check.</param>
		/// <return>True if given mouse button was pressed in this frame.</return>
		public bool MouseButtonPressed(MouseButton button = MouseButton.Left) => GetMouseButtonState(button) == ButtonState.Pressed && GetMousePreviousButtonState(button) == ButtonState.Released;

		/// <summary>
		/// Return if any mouse button was pressed in current frame.
		/// </summary>
		/// <returns>True if any mouse button was pressed in current frame..</returns>
		public bool AnyMouseButtonPressed() => MouseButtonPressed(MouseButton.Left) ||
				MouseButtonPressed(MouseButton.Right) ||
				MouseButtonPressed(MouseButton.Middle);

		/// <summary>
		/// Check if a given mouse button was just clicked (eg released after being pressed down)
		/// </summary>
		/// <param name="button">Mouse button to check.</param>
		/// <return>True if given mouse button is clicked.</return>
		public bool MouseButtonClick(MouseButton button = MouseButton.Left) => GetMouseButtonState(button) == ButtonState.Released && GetMousePreviousButtonState(button) == ButtonState.Pressed;

		/// <summary>
		/// Return if any of mouse buttons was clicked this frame.
		/// </summary>
		/// <returns>True if any mouse button was clicked.</returns>
		public bool AnyMouseButtonClicked() => MouseButtonClick(MouseButton.Left) ||
				MouseButtonClick(MouseButton.Right) ||
				MouseButtonClick(MouseButton.Middle);

		/// <summary>
		/// Return the state of a mouse button (up / down).
		/// </summary>
		/// <param name="button">Button to check.</param>
		/// <returns>Mouse button state.</returns>
		private ButtonState GetMouseButtonState(MouseButton button = MouseButton.Left)
		{
			switch (button)
			{
				case MouseButton.Left:
					return _newMouseState.LeftButton;
				case MouseButton.Right:
					return _newMouseState.RightButton;
				case MouseButton.Middle:
					return _newMouseState.MiddleButton;
			}
			return ButtonState.Released;
		}

		/// <summary>
		/// Return the state of a gamepad button (up / down).
		/// </summary>
		/// <param name="button">Button to check.</param>
		/// <returns>GamePad button state.</returns>
		private ButtonState GetGamePadButtonState(GamePadButton button)
		{
			switch (button)
			{
				case GamePadButton.DPadUp:
					return (_newGamePadState.DPad.Up == ButtonState.Pressed || _newGamePadState.ThumbSticks.Left.Y > Input.DEFAULT_DEADZONE) ? ButtonState.Pressed : ButtonState.Released;
				case GamePadButton.DPadRight:
					return (_newGamePadState.DPad.Right == ButtonState.Pressed || _newGamePadState.ThumbSticks.Left.X > Input.DEFAULT_DEADZONE) ? ButtonState.Pressed : ButtonState.Released;
				case GamePadButton.DPadDown:
					return (_newGamePadState.DPad.Down == ButtonState.Pressed || _newGamePadState.ThumbSticks.Left.Y < -Input.DEFAULT_DEADZONE) ? ButtonState.Pressed : ButtonState.Released;
				case GamePadButton.DPadLeft:
					return (_newGamePadState.DPad.Left == ButtonState.Pressed || _newGamePadState.ThumbSticks.Left.X < -Input.DEFAULT_DEADZONE) ? ButtonState.Pressed : ButtonState.Released;
				case GamePadButton.A_Button:
					return _newGamePadState.Buttons.A;
			}
			return ButtonState.Released;
		}

		/// <summary>
		/// Return the state of a mouse button (up / down), in previous frame.
		/// </summary>
		/// <param name="button">Button to check.</param>
		/// <returns>Mouse button state.</returns>
		private ButtonState GetMousePreviousButtonState(MouseButton button = MouseButton.Left)
		{
			switch (button)
			{
				case MouseButton.Left:
					return _oldMouseState.LeftButton;
				case MouseButton.Right:
					return _oldMouseState.RightButton;
				case MouseButton.Middle:
					return _oldMouseState.MiddleButton;
			}
			return ButtonState.Released;
		}

		/// <summary>
		/// Get GamePad state in previous frame.
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		private ButtonState GetGamePadPreviousButtonState(GamePadButton button = GamePadButton.A_Button)
		{
			switch (button)
			{
				case GamePadButton.DPadUp:
					return (_oldGamePadState.DPad.Up == ButtonState.Pressed || _oldGamePadState.ThumbSticks.Left.Y > Input.DEFAULT_DEADZONE) ? ButtonState.Pressed : ButtonState.Released;
				case GamePadButton.DPadRight:
					return (_oldGamePadState.DPad.Right == ButtonState.Pressed || _oldGamePadState.ThumbSticks.Left.X > Input.DEFAULT_DEADZONE) ? ButtonState.Pressed : ButtonState.Released;
				case GamePadButton.DPadDown:
					return (_oldGamePadState.DPad.Down == ButtonState.Pressed || _oldGamePadState.ThumbSticks.Left.Y < -Input.DEFAULT_DEADZONE) ? ButtonState.Pressed : ButtonState.Released;
				case GamePadButton.DPadLeft:
					return (_oldGamePadState.DPad.Left == ButtonState.Pressed || _oldGamePadState.ThumbSticks.Left.X < -Input.DEFAULT_DEADZONE) ? ButtonState.Pressed : ButtonState.Released;
				case GamePadButton.A_Button:
					return _oldGamePadState.Buttons.A;
			}
			return ButtonState.Released;
		}

		/// <summary>
		/// Check if a given keyboard key is down.
		/// </summary>
		/// <param name="key">Key button to check.</param>
		/// <return>True if given key button is down.</return>
		public bool IsKeyDown(Keys key) => _newKeyboardState.IsKeyDown(key);

		/// <summary>
		/// Check if a given keyboard key was previously pressed down and now released in this frame.
		/// </summary>
		/// <param name="key">Key button to check.</param>
		/// <return>True if given key button was just released.</return>
		public bool IsKeyReleased(Keys key) => _oldKeyboardState.IsKeyDown(key) &&
				   _newKeyboardState.IsKeyUp(key);
	}
}
