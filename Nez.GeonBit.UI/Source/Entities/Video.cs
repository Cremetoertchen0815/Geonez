using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;

namespace Nez.GeonBit.UI.Entities
{
    /// <summary>
    /// A renderable Video (draw custom texture on UI entities).
    /// </summary>
    [System.Serializable]
    public class VideoBox : Entity
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static VideoBox() => Entity.MakeSerializable(typeof(VideoBox));

        /// <summary>How to draw the texture.</summary>
        public ImageDrawMode DrawMode;

        /// <summary>When in Panel draw mode, this will be the frame width in texture percents.</summary>
        public Vector2 FrameWidth = Vector2.One * 0.15f;

        /// <summary>Texture to draw.</summary>
        [System.Xml.Serialization.XmlIgnore]
        public Video Video { get; private set; }


        [System.Xml.Serialization.XmlIgnore]
        private VideoPlayer _player = new();

        /// <summary>Default styling for Videos. Note: loaded from UI theme xml file.</summary>
        public static new StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>If provided, will be used as a source rectangle when drawing Videos in Stretch mode.</summary>
        public Rectangle? SourceRectangle = null;

        /// <summary>
        /// Create the new Video entity.
        /// </summary>
        /// <param name="texture">Video texture.</param>
        /// <param name="size">Video size.</param>
        /// <param name="drawMode">How to draw the Video (see ImageDrawMode for more info).</param>
        /// <param name="anchor">Poisition anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public VideoBox(Video video, Vector2? size = null, ImageDrawMode drawMode = ImageDrawMode.Stretch, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
            base(size, anchor, offset)
        {
            // store Video DrawMode and texture
            DrawMode = drawMode;
            Video = video;

            // update style
            UpdateStyle(DefaultStyle);
        }

        /// <summary>
        /// Create Video without texture.
        /// </summary>
        public VideoBox() : this(null)
        {
        }

        public void Play() => _player.Play(Video);
        public void Pause() => _player.Pause();
        public void Stop() => _player.Stop();
        public bool IsLooped
        {
            get => _player.IsLooped;
            set => _player.IsLooped = value;
        }

        public MediaState VideoState => _player.State;
        public TimeSpan PlayPosition => _player.PlayPosition;

        /// <summary>
        /// Convert a given position to texture coords of this Video.
        /// </summary>
        /// <param name="pos">Position to convert.</param>
        /// <returns>Texture coords from position.</returns>
        public Point GetTextureCoordsAt(Vector2 pos)
        {
            // draw mode must be stretch for it to work
            if (DrawMode != ImageDrawMode.Stretch)
            {
                throw new Exceptions.InvalidStateException("Cannot get texture coords on Video that is not in stretched mode!");
            }

            // make sure in boundaries
            if (!IsInsideEntity(pos))
            {
                throw new Exceptions.InvalidValueException("Position to get coords for must be inside entity boundaries!");
            }

            // get actual dest rect
            CalcDestRect();
            var rect = GetActualDestRect();

            // calc uv
            var relativePos = new Vector2(rect.Right - pos.X, rect.Bottom - pos.Y);
            var uv = new Vector2(1f - relativePos.X / rect.Width, 1f - relativePos.Y / rect.Height);

            // convert to final texture coords
            var textCoords = new Point((int)(uv.X * Video.Width), (int)(uv.Y * Video.Height));
            return textCoords;
        }

        /// <summary>
        /// Get texture color at a given coordinates.
        /// </summary>
        /// <param name="textureCoords">Texture coords to get color for.</param>
        /// <returns>Color of texture at the given texture coords.</returns>
        public Color GetColorAt(Point textureCoords)
        {
            var data = new Color[Video.Width * Video.Height];
            int index = textureCoords.X + (textureCoords.Y * Video.Width);
            _player.GetTexture().GetData<Color>(data);
            return data[index];
        }

        /// <summary>
        /// Calculate width automatically based on height, to maintain texture's original ratio.
        /// For example if you have a texture of 400x200 pixels (eg 2:1 ratio) and its height in pixels is currently
        /// 100 units, calling this function will update this Video width to be 100 x 2 = 200.
        /// </summary>
        public void CalcAutoWidth()
        {
            UpdateDestinationRectsIfDirty();
            float width = (_destRect.Height / (float)Video.Height) * Video.Width;
            Size = new Vector2(width, _size.Y);
        }

        /// <summary>
        /// Calculate height automatically based on width, to maintain texture's original ratio.
        /// For example if you have a texture of 400x200 pixels (eg 2:1 ratio) and its width in pixels is currently
        /// 100 units, calling this function will update this Video height to be 100 / 2 = 50.
        /// </summary>
        public void CalcAutoHeight()
        {
            UpdateDestinationRectsIfDirty();
            float height = (_destRect.Width / (float)Video.Width) * Video.Height;
            Size = new Vector2(_size.X, height);
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            var frame = _player.GetTexture();

            // draw Video based on DrawMode
            switch (DrawMode)
            {
                // panel mode
                case ImageDrawMode.Panel:
                    UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, frame, _destRect, FrameWidth, Scale, FillColor);
                    break;

                // stretch mode
                case ImageDrawMode.Stretch:
                    UserInterface.Active.DrawUtils.DrawImage(spriteBatch, frame, _destRect, FillColor, Scale, SourceRectangle);
                    break;
            }

            // call base draw function
            base.DrawEntity(spriteBatch, phase);
        }
    }
}
