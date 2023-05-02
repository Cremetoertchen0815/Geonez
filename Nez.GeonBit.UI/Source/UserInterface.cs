#region File Description
//-----------------------------------------------------------------------------
// This file define the main class that manage and draw the UI.
// To use GeonBit.UI you first need to create an instance of this class and
// update / draw it every frame.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.UI.Entities;
using System.Collections.Generic;
using System.Xml.Serialization;
using UIEntity = Nez.GeonBit.UI.Entities.Entity;

namespace Nez.GeonBit.UI
{
    /// <summary>
    /// GeonBit.UI is part of the GeonBit project, and provide a simple yet extensive UI framework for MonoGame based projects.
    /// This is the main GeonBit.UI namespace. It contains the UserInterface manager and other important helpers.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    internal class NamespaceDoc
    {
    }

    /// <summary>
    /// A callback function you can register on entity events, like on-click, on-mouse-leave, etc.
    /// </summary>
    /// <param name="entity">The entity instance the event came from.</param>
    public delegate void EventCallback(UIEntity entity);

    /// <summary>
    /// A function used to generate tooltip entity.
    /// Used when the user points on an entity with a tooltip text and show present it.
    /// </summary>
    /// <param name="entity">The entity instance the tooltip came from.</param>
    public delegate UIEntity GenerateTooltipFunc(UIEntity entity);

    /// <summary>
    /// Callback to generate default paragraph type for internal entities.
    /// </summary>
    /// <param name="text">Paragraph starting text.</param>
    /// <param name="anchor">Paragraph anchor.</param>
    /// <param name="color">Optional fill color.</param>
    /// <param name="scale">Optional scale.</param>
    /// <param name="size">Optional size.</param>
    /// <param name="offset">Optional offset</param>
    /// <returns></returns>
    public delegate Paragraph DefaultParagraphGenerator(string text, Anchor anchor, Color? color = null, float? scale = null, Vector2? size = null, Vector2? offset = null);

    /// <summary>
    /// Curser styles / types.
    /// </summary>
    public enum CursorType
    {
        /// <summary>Default cursor.</summary>
        Default,

        /// <summary>Pointing hand cursor.</summary>
        Pointer,

        /// <summary>Text-input I-beam cursor.</summary>
        IBeam,
    };

    /// <summary>
    /// Enum with all the built-in themes.
    /// </summary>
    public enum BuiltinThemes
    {
        /// <summary>
        /// Old-school style theme with hi-res textures.
        /// </summary>
        hd,

        /// <summary>
        /// Old-school style theme with low-res textures.
        /// </summary>
        lowres,

        /// <summary>
        /// Clean, editor-like theme.
        /// </summary>
        editor,
    }

    /// <summary>
    /// Main GeonBit.UI class that manage and draw all the UI entities.
    /// This is the main manager you use to update, draw, and add entities to.
    /// </summary>
    public class UserInterface : System.IDisposable
    {
        /// <summary>Current GeonBit.UI version identifier.</summary>
        public const string VERSION = "3.2.0.1";

        /// <summary>
        /// The currently active user interface instance.
        /// </summary>
        public static UserInterface Active = null;

        // input manager
        public static InputHelper Input;

        // content manager
        private static ContentManager _content;

        // the main render target we render everything on
        private RenderTarget2D _renderTarget = null;

        // are we currently in use-render-target mode
        private bool _useRenderTarget = false;

        // are we currently during deserialization phase?
        internal bool _isDeserializing = false;

        /// <summary>
        /// If true, GeonBit.UI will not raise exceptions on sanity checks, validations, and errors which are not critical.
        /// For example, trying to select a value that doesn't exist from a list would do nothing instead of throwing exception.
        /// </summary>
#if RELEASE
		public bool SilentSoftErrors = true;
#else
        public bool SilentSoftErrors = false;
#endif

        /// <summary>
        /// If true, will add debug rendering to UI.
        /// </summary>
        public bool DebugDraw = false;

        public static bool GamePadModeEnabled = false;

        public static bool StainedCanvasEnabled = true;

        /// <summary>
        /// Supported GamePad Cursor behavior.
        /// </summary>
        public enum CursorMode
        {
            ///<summary>The mouse cursor will snap to an entity</summary>
            Snapping,
            ///<summary>The mouse cursor is freely moveable</summary>
            Roaming
        };

        /// <summary>
        /// The cursor mode.
        /// </summary>
        private static CursorMode _cursorMode;
        /// <summary>
        /// Choose how the mouse cursor should behave.
        /// </summary>
        public static CursorMode GetCursorMode
        {
            get => _cursorMode;
            set => _cursorMode = value;
        }

        /// <summary>
        /// Create a default paragraph instance.
        /// GeonBit.UI entities use this method when need to create a paragraph, so you can override this to change which paragraph type the built-in
        /// entities will use by-default (for example Buttons text, SelectList items, etc.).
        /// </summary>
        public static DefaultParagraphGenerator DefaultParagraph =
            (string text, Anchor anchor, Color? color, float? scale, Vector2? size, Vector2? offset) =>
            {
                if (color != null)
                {
                    return new MulticolorParagraph(text, anchor, color.Value, scale, size, offset);
                }
                return new MulticolorParagraph(text, anchor, size, offset, scale);
            };


        /// <summary>
        /// If true, will draw the UI on a render target before drawing on screen.
        /// This mode is required for some of the features.
        /// </summary>
        public bool UseRenderTarget
        {
            get => _useRenderTarget;
            set { _useRenderTarget = value; DisposeRenderTarget(); }
        }

        /// <summary>
        /// Get the main render target all the UI draws on.
        /// </summary>
        public RenderTarget2D RenderTarget => _renderTarget;

        /// <summary>
        /// Get the root entity.
        /// </summary>
        public RootPanel Root { get; private set; }

        /// <summary>
        /// Blend state to use when rendering UI.
        /// </summary>
        public BlendState BlendState = BlendState.AlphaBlend;

        /// <summary>
        /// Sampler state to use when rendering UI.
        /// </summary>
        public SamplerState SamplerState = SamplerState.PointClamp;

        // the entity currently being dragged
        private UIEntity _dragTarget;

        // current global scale
        private float _scale = 1f;

        /// <summary>Scale the entire UI and all the entities in it. This is useful for smaller device screens.</summary>
        public float GlobalScale
        {
            get => _scale;
            set { _scale = value; Root.MarkAsDirty(); }
        }

        /// <summary>Cursor rendering size.</summary>
        public float CursorScale = 1f;

        /// <summary>Screen width.</summary>
        public int ScreenWidth = Screen.Width;

        /// <summary>Screen height.</summary>
        public int ScreenHeight = Screen.Height;

        /// <summary>Draw utils helper. Contain general drawing functionality and handle effects replacement.</summary>
        public DrawUtils DrawUtils = null;

        /// <summary>Current active entity, eg last entity user interacted with.</summary>
        public UIEntity ActiveEntity = null;

        /// <summary>The current target entity, eg what cursor points on. Can be null if cursor don't point on any entity.</summary>
        public UIEntity TargetEntity { get; private set; }

        /// <summary>Callback to execute when mouse button is pressed over an entity (called once when button is pressed).</summary>
        public EventCallback OnMouseDown = null;

        /// <summary>Callback to execute when right mouse button is pressed over an entity (called once when button is pressed).</summary>
        public EventCallback OnRightMouseDown = null;

        /// <summary>Callback to execute when mouse button is released over an entity (called once when button is released).</summary>
        public EventCallback OnMouseReleased = null;

        /// <summary>Callback to execute when gamepad button is pressed over an entity (called once when button is pressed).</summary>
        public EventCallback OnGamePadPressed = null;

        /// <summary>Callback to execute when gamepad button is released over an entity (called once when button is released).</summary>
        public EventCallback OnGamePadReleased = null;

        /// <summary>Callback to execute every frame while mouse button is pressed over an entity.</summary>
        public EventCallback WhileMouseDown = null;

        /// <summary>Callback to execute every frame while right mouse button is pressed over an entity.</summary>
        public EventCallback WhileRightMouseDown = null;

        /// <summary>Callback to execute every frame while mouse is hovering over an entity, unless mouse button is down.</summary>
        public EventCallback WhileMouseHover = null;

        /// <summary>Callback to execute every frame while mouse is hovering over an entity, even if mouse button is down.</summary>
        public EventCallback WhileMouseHoverOrDown = null;

        /// <summary>Callback to execute when user clicks on an entity (eg release mouse over it).</summary>
        public EventCallback OnClick = null;

        /// <summary>Callback to execute when user clicks on an entity with right mouse button (eg release mouse over it).</summary>
        public EventCallback OnRightClick = null;

        /// <summary>Callback to execute when any entity value changes (relevant only for entities with value).</summary>
        public EventCallback OnValueChange = null;

        /// <summary>Callback to execute when mouse start hovering over an entity (eg enters its region).</summary>
        public EventCallback OnMouseEnter = null;

        /// <summary>Callback to execute when mouse stop hovering over an entity (eg leaves its region).</summary>
        public EventCallback OnMouseLeave = null;

        /// <summary>Callback to execute when mouse wheel scrolls and an entity is the active entity.</summary>
        public EventCallback OnMouseWheelScroll = null;

        /// <summary>Called when entity starts getting dragged (only if draggable).</summary>
        public EventCallback OnStartDrag = null;

        /// <summary>Called when entity stop getting dragged (only if draggable).</summary>
        public EventCallback OnStopDrag = null;

        /// <summary>Called every frame while entity is being dragged.</summary>
        public EventCallback WhileDragging = null;

        /// <summary>Callback to execute every frame before entity update.</summary>
        public EventCallback BeforeUpdate = null;

        /// <summary>Callback to execute every frame after entity update.</summary>
        public EventCallback AfterUpdate = null;

        /// <summary>Callback to execute every frame before entity is rendered.</summary>
        public EventCallback BeforeDraw = null;

        /// <summary>Callback to execute every frame after entity is rendered.</summary>
        public EventCallback AfterDraw = null;

        /// <summary>Callback to execute every time the visibility property of an entity change.</summary>
        public EventCallback OnVisiblityChange = null;

        /// <summary>Callback to execute every time a new entity is spawned (note: spawn = first time Update() is called on this entity).</summary>
        public EventCallback OnEntitySpawn = null;

        /// <summary>Callback to execute every time an entity focus changes.</summary>
        public EventCallback OnFocusChange = null;

        // cursor texture.
        private Texture2D _cursorTexture = null;

        // cursor width.
        private int _cursorWidth = 32;

        // cursor offset from mouse actual position.
        private Point _cursorOffset = Point.Zero;

        // time until we show tooltip text.
        private float _timeUntilTooltip = 0f;

        // the current tooltip entity.
        private UIEntity _tooltipEntity;

        // current tooltip target entity (eg entity we point on with tooltip).
        private UIEntity _tooltipTargetEntity;

        internal List<PostProcessor> _stainedCanvasesPPFX = new();
        internal List<RenderTarget2D> _stainedCanvasesTargets = new();

        /// <summary>
        /// How long to wait before showing tooltip texts.
        /// </summary>
        public static float TimeToShowTooltipText = 2f;

        /// <summary>Whether or not to draw the cursor.</summary>
        public bool ShowCursor = true;

        /// <summary>Weather or not to lock the cursor position.</summary>
        private static bool _LockCursorPosition = false;

        /// <summary>
        /// Lock the mouse cursor position so it's only moveable with gamepad (need to set AFTER Initilization of UserInterface!).
        /// </summary>
        public static bool LockCursorPosition
        {
            private get => _LockCursorPosition;
            set => Input.LockMousePosition = _LockCursorPosition = value;
        }

        /// <summary>
        /// Optional transformation matrix to apply when drawing with render targets.
        /// </summary>
        public Matrix? RenderTargetTransformMatrix = null;

        /// <summary>
        /// If using render targets, should the curser be rendered inside of it?
        /// If false, cursor will draw outside the render target, when presenting it.
        /// </summary>
        public bool IncludeCursorInRenderTarget = true;

        /// <summary>
        /// The function used to generate tooltip text on entities.
        /// </summary>
        public GenerateTooltipFunc GenerateTooltipFunc = DefaultGenerateTooltipFunc;

        /// <summary>
        /// Initialize UI manager (mostly load resources and set some defaults).
        /// </summary>
        /// <param name="contentManager">Content manager.</param>
        /// <param name="theme">Which UI theme to use (see options in Content/GeonBit.UI/themes/). This affect the appearance of all textures and effects.</param>
        public static void Initialize(ContentManager contentManager, string theme = "hd")
        {
            // store the content manager
            _content = contentManager;

            // init resources (textures etc)
            Resources.LoadContent(_content, theme);

            // create a default active user interface
            Active = new UserInterface();
        }

        /// <summary>
        /// Dispose unmanaged resources of this user interface.
        /// </summary>
        public void Dispose() => DisposeRenderTarget();

        /// <summary>
        /// UserInterface destructor.
        /// </summary>
        ~UserInterface()
        {
            Dispose();
        }

        /// <summary>
        /// Default function we use to generate tooltip text entities.
        /// </summary>
        /// <param name="source">Source entity.</param>
        /// <returns>Entity to use for tooltip text.</returns>
        private static UIEntity DefaultGenerateTooltipFunc(UIEntity source)
        {
            // no tooltip text? return null
            if (source.ToolTipText == null) return null;

            // create tooltip paragraph
            var tooltip = new Paragraph(source.ToolTipText, size: new Vector2(500, -1))
            {
                BackgroundColor = Color.Black
            };

            // add callback to update tooltip position
            tooltip.BeforeDraw += (UIEntity ent) =>
            {
                // get dest rect and calculate tooltip position based on size and mouse position
                var destRect = tooltip.GetActualDestRect();
                var position = UserInterface.Active.GetTransformedCursorPos(new Vector2(-destRect.Width / 2, -destRect.Height - 20));

                // make sure tooltip is not out of screen boundaries
                var screenBounds = Active.Root.GetActualDestRect();
                if (position.Y < screenBounds.Top) position.Y = screenBounds.Top;
                if (position.Y > screenBounds.Bottom - destRect.Height) position.Y = screenBounds.Bottom - destRect.Height;
                if (position.X < screenBounds.Left) position.X = screenBounds.Left;
                if (position.X > screenBounds.Right - destRect.Width) position.X = screenBounds.Right - destRect.Width;

                // update tooltip position
                tooltip.SetPosition(Anchor.TopLeft, position / Active.GlobalScale);
            };
            tooltip.CalcTextActualRectWithWrap();
            tooltip.BeforeDraw(tooltip);

            // return tooltip object
            return tooltip;
        }

        /// <summary>
        /// Initialize UI manager (mostly load resources and set some defaults).
        /// </summary>
        /// <param name="contentManager">Content manager.</param>
        /// <param name="theme">Which UI theme to use. This affect the appearance of all textures and effects.</param>
        public static void Initialize(ContentManager contentManager, BuiltinThemes theme) => Initialize(contentManager, theme.ToString());

        public void OnSceneChange()
        {
            Clear();

            foreach (var item in _stainedCanvasesPPFX)
            {
                item.Unload();
                if (Core.Scene is null) continue;
                item.OnAddedToScene(Core.Scene);
            }
        }

        /// <summary>
        /// Create the user interface instance.
        /// </summary>
        public UserInterface()
        {
            // sanity test
            if (_content == null)
            {
                throw new Exceptions.InvalidStateException("Cannot create a UserInterface before calling UserInterface.Initialize()!");
            }

            // create draw utils
            DrawUtils = new DrawUtils();

            // create input helper
            if (Input == null)
                Input = new InputHelper();

            // create the root panel
            Root = new RootPanel();

            // set default cursor
            SetCursor(CursorType.Default);
        }

        /// <summary>
        /// Set a new mouse cursor position based on gamepad entity selection
        /// </summary>
        /// <param name="position">The new position of the mouse cursor.</param>
        public static void SetCursorPosition(Vector2 position) => Input.UpdateCursorPosition(position);

        /// <summary>
        /// Set cursor style.
        /// </summary>
        /// <param name="type">What type of cursor to show.</param>
        public void SetCursor(CursorType type)
        {
            var data = Resources.CursorsData[(int)type];
            SetCursor(Resources.Cursors[type], data.DrawWidth, new Point(data.OffsetX, data.OffsetY));
        }

        /// <summary>
        /// Set cursor graphics from a custom texture.
        /// </summary>
        /// <param name="texture">Texture to use for cursor.</param>
        /// <param name="drawWidth">Width, in pixels to draw the cursor. Height will be calculated automatically to fit texture propotions.</param>
        /// <param name="offset">Cursor offset from mouse position (if not provided will draw cursor with top-left corner on mouse position).</param>
        public void SetCursor(Texture2D texture, int drawWidth = 32, Point? offset = null)
        {
            _cursorTexture = texture;
            _cursorWidth = drawWidth;
            _cursorOffset = offset ?? Point.Zero;
        }

        /// <summary>
        /// Draw the cursor.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw the cursor.</param>
        public void DrawCursor(SpriteBatch spriteBatch)
        {
            // start drawing for cursor
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            // calculate cursor size
            float cursorSize = CursorScale * GlobalScale * (_cursorWidth / (float)_cursorTexture.Width);

            // get cursor position and draw it
            var cursorPos = Nez.Input.MousePosition;
            spriteBatch.Draw(_cursorTexture,
                new Rectangle(
                    (int)(cursorPos.X + _cursorOffset.X * cursorSize), (int)(cursorPos.Y + _cursorOffset.Y * cursorSize),
                    (int)(_cursorTexture.Width * cursorSize), (int)(_cursorTexture.Height * cursorSize)),
                Color.White);

            // end drawing
            spriteBatch.End();
        }

        /// <summary>
        /// Allows you to add a "stained canvas", a customly post-processed copy of the screen that can be used as background for entites.
        /// </summary>
        /// <param name="nr"></param>
        /// <param name="postProcessor"></param>
        public void AddStainedCanvas(PostProcessor postProcessor)
        {
            _stainedCanvasesPPFX.Add(postProcessor);
            _stainedCanvasesTargets.Add(null);
        }

        public Texture2D GetCanvasTexture(int nr) => _stainedCanvasesTargets[nr];

        /// <summary>
        /// Add an entity to screen.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public T AddEntity<T>(T entity) where T : UIEntity => Root.AddChild(entity);

        /// <summary>
        /// Remove an entity from screen.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public void RemoveEntity(UIEntity entity) => Root.RemoveChild(entity);

        /// <summary>
        /// Remove all entities from screen.
        /// </summary>
        public void Clear() => Root.ClearChildren();

        /// <summary>
        /// Update the UI manager. This function should be called from your Game 'Update()' function, as early as possible (eg before you update your game state).
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            // update input manager
            Input.Update(gameTime);

            // unset the drag target if the mouse was released
            if (_dragTarget != null && !Input.MouseButtonHeldDown(MouseButton.Left))
            {
                _dragTarget = null;
            }

            // update root panel
            UIEntity target = null;
            bool wasEventHandled = false;
            Root.Update(ref target, ref _dragTarget, ref wasEventHandled, Point.Zero);

            // set active entity
            if (Input.MouseButtonHeldDown(MouseButton.Left) || Input.GamePadButtonHeldDown(GamePadButton.A_Button))
            {
                ActiveEntity = target;
            }

            // update tooltip
            UpdateTooltipText(gameTime, target);

            // default active entity is root panel
            ActiveEntity = ActiveEntity ?? Root;

            // set current target entity
            TargetEntity = target;
        }

        /// <summary>
        /// Update tooltip text related stuff.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="target">Current target entity.</param>
        private void UpdateTooltipText(GameTime gameTime, UIEntity target)
        {
            // fix tooltip target to be an actual entity
            while (target != null && target._hiddenInternalEntity)
                target = target.Parent;

            // if target entity changed, zero time to show tooltip text
            if (_tooltipTargetEntity != target || target == null)
            {
                // zero time until showing tooltip text
                _timeUntilTooltip = 0f;

                // if we currently have a tooltip we show, remove it
                if (_tooltipEntity != null && _tooltipEntity.Parent != null)
                {
                    _tooltipEntity.RemoveFromParent();
                    _tooltipEntity = null;
                }
            }

            // set current tooltip target
            _tooltipTargetEntity = target;

            // if we currently not showing any tooltip entity
            if (_tooltipEntity == null)
            {
                // decrease time until showing tooltip
                _timeUntilTooltip += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // if its time to show tooltip text, create it.
                // note: we create even if the target have no tooltip text, to allow our custom function to create default tooltip or generate based on entity type.
                // if the entity should not show tooltip text, the function to generate it should just return null.
                if (_timeUntilTooltip > TimeToShowTooltipText)
                {
                    // create tooltip text entity
                    _tooltipEntity = GenerateTooltipFunc(_tooltipTargetEntity);

                    // if got a result lock it and add to UI
                    if (_tooltipEntity != null)
                    {
                        _tooltipEntity.Locked = true;
                        _tooltipEntity.ClickThrough = true;
                        AddEntity(_tooltipEntity);
                    }
                }
            }
        }

        /// <summary>
        /// Dispose the render target (only if use) and set it to null.
        /// </summary>
        private void DisposeRenderTarget()
        {
            if (_renderTarget != null)
            {
                _renderTarget.Dispose();
                _renderTarget = null;
            }
        }

        /// <summary>
        /// Draw the UI. This function should be called from your Game 'Draw()' function.
        /// Note: if UseRenderTarget is true, this function should be called FIRST in your draw function.
        /// If UseRenderTarget is false, this function should be called LAST in your draw function.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw on.</param>
        public void Draw(SpriteBatch spriteBatch, RenderTarget2D sourceTarget)
        {
            int newScreenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            int newScreenHeight = spriteBatch.GraphicsDevice.Viewport.Height;

            // update screen size
            if (ScreenWidth != newScreenWidth || ScreenHeight != newScreenHeight)
            {
                ScreenWidth = newScreenWidth;
                ScreenHeight = newScreenHeight;
                Root.MarkAsDirty();
            }

            // if using rendering targets
            if (UseRenderTarget)
            {
                //Process stained canvases
                if (StainedCanvasEnabled)
                {
                    for (int i = 0; i < _stainedCanvasesPPFX.Count; i++)
                    {
                        var item = _stainedCanvasesPPFX[i];
                        var rt = _stainedCanvasesTargets[i];
                        if (rt == null ||
                        rt.Width != ScreenWidth ||
                        rt.Height != ScreenHeight)
                        {
                            // recreate render target
                            rt?.Dispose();
                            rt = _stainedCanvasesTargets[i] = new RenderTarget2D(spriteBatch.GraphicsDevice,
                                ScreenWidth, ScreenHeight, false,
                                spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat,
                                spriteBatch.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0,
                                RenderTargetUsage.PreserveContents);
                        }

                        spriteBatch.GraphicsDevice.SetRenderTarget(rt);
                        spriteBatch.GraphicsDevice.Clear(Color.Transparent);

                        item.Process(sourceTarget, rt);
                    }
                }

                // check if screen size changed or don't have a render target yet. if so, create the render target.
                if (_renderTarget == null ||
                    _renderTarget.Width != ScreenWidth ||
                    _renderTarget.Height != ScreenHeight)
                {
                    // recreate render target
                    DisposeRenderTarget();
                    _renderTarget = new RenderTarget2D(spriteBatch.GraphicsDevice,
                        ScreenWidth, ScreenHeight, false,
                        spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat,
                        spriteBatch.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0,
                        RenderTargetUsage.PreserveContents);
                }

                spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
                spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            }

            // draw root panel
            Root.Draw(spriteBatch);

            // draw cursor (unless using render targets and should draw cursor outside of it)
            if (ShowCursor && (IncludeCursorInRenderTarget || !UseRenderTarget))
            {
                DrawCursor(spriteBatch);
            }

            // reset render target
            if (UseRenderTarget)
            {
                spriteBatch.GraphicsDevice.SetRenderTarget(null);
            }
        }

        /// <summary>
        /// Finalize the draw frame and draw all the UI on screen.
        /// This function only works if we are in UseRenderTarget mode.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        public void DrawMainRenderTarget(SpriteBatch spriteBatch, Rectangle screenSpace)
        {
            // draw the main render target
            if (RenderTarget != null && !RenderTarget.IsDisposed)
            {
                // draw render target
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: RenderTargetTransformMatrix);
                spriteBatch.Draw(RenderTarget, screenSpace, Color.White);
                spriteBatch.End();
            }

            // draw cursor
            if (ShowCursor && !IncludeCursorInRenderTarget)
            {
                DrawCursor(spriteBatch);
            }
        }

        /// <summary>
        /// Get transformed cursoer position for collision detection.
        /// If have transform matrix and curser is included in render target, will transform cursor position too.
        /// If don't use transform matrix or drawing cursor outside, will not transform cursor position.
        /// </summary>
        /// <returns>Transformed cursor position.</returns>
        public Vector2 GetTransformedCursorPos(Vector2? addVector)
        {
            // default add vector
            addVector = addVector ?? Vector2.Zero;

            // return transformed cursor position
            if (UseRenderTarget && RenderTargetTransformMatrix != null && !IncludeCursorInRenderTarget)
            {
                var matrix = Matrix.Invert(RenderTargetTransformMatrix.Value);
                return Input.TransformCursorPos(matrix) + Vector2.Transform(addVector.Value, matrix);
            }

            // return raw cursor pos
            return Input.MousePosition + addVector.Value;
        }

        /// <summary>
        /// Get xml serializer.
        /// </summary>
        /// <returns>XML serializer instance.</returns>
        protected virtual XmlSerializer GetXmlSerializer() => new XmlSerializer(Root.GetType(), UIEntity._serializableTypes.ToArray());

        /// <summary>
        /// Serialize the whole UI to stream.
        /// Note: serialization have some limitation and things that will not be included in xml,
        /// like even handlers. Please read docs carefuly to know what to expect.
        /// </summary>
        /// <param name="stream">Stream to serialize to.</param>
        public void Serialize(System.IO.Stream stream)
        {
            var writer = GetXmlSerializer();
            writer.Serialize(stream, Root);
        }

        /// <summary>
        /// Deserialize the whole UI from stream.
        /// Note: serialization have some limitation and things that will not be included in xml,
        /// like even handlers. Please read docs carefuly to know what to expect.
        /// </summary>
        /// <param name="stream">Stream to deserialize from.</param>
        public void Deserialize(System.IO.Stream stream)
        {
            // started deserializing..
            _isDeserializing = true;

            // do deserialize
            try
            {
                var reader = GetXmlSerializer();
                Root = (RootPanel)reader.Deserialize(stream);
            }
            // handle errors
            catch
            {
                _isDeserializing = false;
                throw;
            }

            // init after finish deserializing
            _isDeserializing = false;
            Root.InitAfterDeserialize();
        }

        /// <summary>
        /// Serialize the whole UI to filename.
        /// Note: serialization have some limitation and things that will not be included in xml,
        /// like even handlers. Please read docs carefuly to know what to expect.
        /// </summary>
        /// <param name="path">Filename to serialize into.</param>
        public void Serialize(string path)
        {
            var file = System.IO.File.Create(path);
            Serialize(file);
            file.Close();
        }

        /// <summary>
        /// Deserialize the whole UI from filename.
        /// Note: serialization have some limitation and things that will not be included in xml,
        /// like even handlers. Please read docs carefuly to know what to expect.
        /// </summary>
        /// <param name="path">Filename to deserialize from.</param>
        public void Deserialize(string path)
        {
            var file = System.IO.File.OpenRead(path);
            Deserialize(file);
            file.Close();
        }
    }
}
