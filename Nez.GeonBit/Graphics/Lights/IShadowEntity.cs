using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Graphics.Lights;
public interface IShadowCaster
{
    /// <summary>
    /// Determines which shadow map this shadow should be rendered onto.
    /// </summary>
    int PrimaryLight { get; set; }

    /// <summary>
    /// Is true if the shadow caster shall be rendered.
    /// </summary>
    bool CastsShadow { get; set; }

    /// <summary>
    /// Determines which Level Of Detail Model to select, default is 0, so the regular model.
    /// </summary>
    int ShadowCasterLOD { get; set; }

    /// <summary>
    /// The rasterizer state the shadow caster will be rendered with. If null, the default will be CullClockwise in order to only render the backfaces.
    /// </summary>
    RasterizerState ShadowRasterizerState { get; set; }

    /// <summary>
    /// Renders the shadow caster onto the shadow map.
    /// </summary>
    void RenderShadows(Matrix worldTransform);
}