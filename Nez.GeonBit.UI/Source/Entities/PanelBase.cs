#region File Description
//-----------------------------------------------------------------------------
// Base class for panel and other panel-based entities.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.UI.Entities
{
	/// <summary>
	/// Different panel textures you can use.
	/// </summary>
	public enum PanelSkin
	{
		/// <summary>No skin, eg panel itself is invisible.</summary>
		None = -1,

		/// <summary>Default panel texture.</summary>
		Default = 0,

		/// <summary>Alternative panel texture.</summary>
		Fancy = 1,

		/// <summary>Simple, grey panel. Useful for internal frames, eg when inside another panel.</summary>
		Simple = 2,

		/// <summary>Shiny golden panel.</summary>
		Golden = 3,

		/// <summary>Special panel skin used for lists and input background.</summary>
		ListBackground = 4,
	}

	/// <summary>
	/// A graphical panel or form you can create and add entities to.
	/// Used to group together entities with common logic.
	/// </summary>
	[System.Serializable]
	public class PanelBase : Entity
	{
		/// <summary>
		/// Static ctor.
		/// </summary>
		static PanelBase() => Entity.MakeSerializable(typeof(PanelBase));

		// panel style
		private PanelSkin _skin;

		/// <summary>
		/// Create the panel.
		/// </summary>
		/// <param name="size">Panel size.</param>
		/// <param name="skin">Panel skin (texture to use). Use PanelSkin.None for invisible panels.</param>
		/// <param name="anchor">Position anchor.</param>
		/// <param name="offset">Offset from anchor position.</param>
		public PanelBase(Vector2 size, PanelSkin skin = PanelSkin.Default, Anchor anchor = Anchor.Center, Vector2? offset = null) :
			base(size, anchor, offset)
		{
			_skin = skin;
			UpdateStyle(Panel.DefaultStyle);

			if (_skin != PanelSkin.None) MilkFactor = 0.65f;
		}

		/// <summary>
		/// Create the panel with default params.
		/// </summary>
		public PanelBase() :
			this(new Vector2(500, 500))
		{
		}

		/// <summary>
		/// Panel destructor.
		/// </summary>
		~PanelBase()
		{
		}

		/// <summary>
		/// Set / get current panel skin.
		/// </summary>
		public PanelSkin Skin
		{
			get => _skin;
			set => _skin = value;
		}

		/// <summary>
		/// Draw the entity.
		/// </summary>
		/// <param name="spriteBatch">Sprite batch to draw on.</param>
		/// <param name="phase">The phase we are currently drawing.</param>
		protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
		{
			// draw panel itself, but only if got style
			if (_skin != PanelSkin.None)
			{
				// get texture based on skin
				var data = Resources.PanelData[(int)_skin];

				if (data.StainedCanvasID < 0)
				{

					var texture = Resources.PanelTextures[_skin];
					var frameSize = new Vector2(data.FrameWidth, data.FrameHeight);

					// draw panel
					UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, frameSize, 1f, FillColor, Scale);
				}
				else
				{

                    if (UserInterface.StainedCanvasEnabled)
                    {
                        var tex = UserInterface.Active.GetCanvasTexture(data.StainedCanvasID);
                        var nuSize = new Vector2(data.FrameWidth, data.FrameHeight) * _destRect.Size.ToVector2();
                        var srcRect = new Rectangle((_destRect.Center.ToVector2() - nuSize * 0.5f).ToPoint(),
                                                    nuSize.ToPoint());
                        UserInterface.Active.DrawUtils.DrawImage(spriteBatch, tex, _destRect, FillColor, 1, srcRect);
                        spriteBatch.DrawRect(_destRect, FillColor with { A = 255 } * MilkFactor);
                    } else
                    {
                        spriteBatch.DrawRect(_destRect, FillColor with { A = 255 });
                    }

				}
			}

			// call base draw function
			base.DrawEntity(spriteBatch, phase);
		}
	}
}
