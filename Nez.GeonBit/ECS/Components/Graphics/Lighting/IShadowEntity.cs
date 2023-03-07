using Microsoft.Xna.Framework;

namespace Nez.GeonBit;
public interface IShadowCaster
{
    int PrimaryLight { get; set; }
    bool CastsShadow { get; set; }
    int ShadowCasterLOD { get; set; }
    void RenderShadows(Matrix worldTransform);
}