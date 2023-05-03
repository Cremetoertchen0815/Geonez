using Microsoft.Xna.Framework;

namespace Nez.GeonBit.Graphics.Lights
{
    public interface ILightSource
    {
        /// <summary>
        /// Is this light source currently visible?
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// So we can cache lights and identify when they were changed.
        /// </summary>
        uint ParamsVersion { get; set; }

        /// <summary>
        /// Return if this light is a directional light.
        /// </summary>
        public virtual bool IsDirectionalLight => Direction != null;

        /// <summary>
        /// Light position in world space.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Light direction, if its a directional light.
        /// </summary>
        public Vector3? Direction { get; }

        /// <summary>
        /// Light color and strength (A field = light strength).
        /// </summary>
        public Vector3 Diffuse { get; }

        /// <summary>
        /// Specular factor.
        /// </summary>
        public Vector3 Specular { get; }

        /// <summary>
        /// Remove self from parent lights manager.
        /// </summary>
        void Remove();

        /// <summary>
        /// Update light transformations.
        /// </summary>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        void UpdateTransforms(ref Matrix worldTransformations);

    }
}
