using Microsoft.Xna.Framework;

namespace Nez.GeonBit.Graphics.Lights
{
    public interface IRangedLight : ILightSource
    {
        /// <summary>
        /// Light bounding sphere.
        /// </summary>
        public BoundingSphere BoundingSphere { get; }

        /// <summary>
        /// Light range.
        /// </summary>
        public float Range { get; set; }

        /// <summary>
        /// Recalculate light bounding sphere after transformations or radius change.
        /// </summary>
        /// <param name="updateInLightsManager">If true, will also update light position in lights manager.</param>
        void RecalcBoundingSphere(bool updateInLightsManager = true);

        /// <summary>
        /// Min lights region index this light is currently in.
        /// </summary>
        Vector3 MinRegionIndex { get; set; }

        /// <summary>
        /// Max lights region index this light is currently in.
        /// </summary>
        Vector3 MaxRegionIndex { get; set; }
    }
}
