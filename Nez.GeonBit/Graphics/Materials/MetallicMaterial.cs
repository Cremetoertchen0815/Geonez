using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Materials;

public class MetallicMaterial : MaterialAPI
{
    // empty effect instance to clone when creating new material
    private static readonly EnvironmentMapEffect _emptyEffect = new(Core.GraphicsDevice);

    // the effect instance of this material.
    private readonly EnvironmentMapEffect _effect;

    private float _environmentAmount;

    private TextureCube _environmentMap;

    private Color _environmentSpecular;

    private float _fresnelFactor;

    /// <summary>
    ///     Create the default material from empty effect.
    /// </summary>
    public MetallicMaterial() : this(_emptyEffect)
    {
    }

    /// <summary>
    ///     Create the material from another material instance.
    /// </summary>
    /// <param name="other">Other material to clone.</param>
    public MetallicMaterial(MetallicMaterial other)
    {
        _effect = other._effect.Clone() as EnvironmentMapEffect;
        MaterialAPI asBase = this;
        other.CloneBasics(ref asBase);
    }

    /// <summary>
    ///     Create the default material.
    /// </summary>
    /// <param name="fromEffect">Effect to create material from.</param>
    /// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
    public MetallicMaterial(EnvironmentMapEffect fromEffect, bool copyEffectProperties = true)
    {
        // store effect and set default properties
        _effect = fromEffect.Clone() as EnvironmentMapEffect;
        SetDefaults();

        // copy properties from effect itself
        if (copyEffectProperties)
        {
            // set effect defaults
            Texture = fromEffect.Texture;
            Alpha = fromEffect.Alpha;
            DiffuseColor = new Color(fromEffect.DiffuseColor.X, fromEffect.DiffuseColor.Y, fromEffect.DiffuseColor.Z);

            // enable lightings by default
            _effect.EnableDefaultLighting();
        }
    }

    /// <summary>
    ///     Get the effect instance.
    /// </summary>
    public override Effect Effect => _effect;

    public TextureCube EnvironmentMap
    {
        get => _environmentMap;
        set
        {
            _environmentMap = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }

    public float EnvironmentAmount
    {
        get => _environmentAmount;
        set
        {
            _environmentAmount = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }

    public Color EnvironmentSpecular
    {
        get => _environmentSpecular;
        set
        {
            _environmentSpecular = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }

    public float FresnelFactor
    {
        get => _fresnelFactor;
        set
        {
            _fresnelFactor = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }

    /// <summary>
    ///     Apply this material.
    /// </summary>
    protected override void MaterialSpecificApply(bool wasLastMaterial)
    {
        // set world matrix
        if (IsDirty(MaterialDirtyFlags.World)) _effect.World = World;

        // if it was last material used, stop here - no need for the following settings
        if (wasLastMaterial) return;

        // set all effect params
        if (IsDirty(MaterialDirtyFlags.EnvironmentMap))
        {
            _effect.Texture = Texture;
            _effect.EnvironmentMap = EnvironmentMap;
            _effect.EnvironmentMapAmount = EnvironmentAmount;
            _effect.EnvironmentMapSpecular = EnvironmentSpecular.ToVector3();
            _effect.FresnelFactor = FresnelFactor;
        }

        if (IsDirty(MaterialDirtyFlags.Alpha)) _effect.Alpha = Alpha;
        if (IsDirty(MaterialDirtyFlags.AmbientLight)) _effect.AmbientLightColor = AmbientLight.ToVector3();
        if (IsDirty(MaterialDirtyFlags.EmissiveLight)) _effect.EmissiveColor = EmissiveLight.ToVector3();
        if (IsDirty(MaterialDirtyFlags.MaterialColors)) _effect.DiffuseColor = DiffuseColor.ToVector3();
    }

    /// <summary>
    ///     Update material view matrix.
    /// </summary>
    /// <param name="view">New view to set.</param>
    protected override void UpdateView(ref Matrix view)
    {
        _effect.View = View;
    }

    /// <summary>
    ///     Update material projection matrix.
    /// </summary>
    /// <param name="projection">New projection to set.</param>
    protected override void UpdateProjection(ref Matrix projection)
    {
        _effect.Projection = Projection;
    }

    /// <summary>
    ///     Clone this material.
    /// </summary>
    /// <returns>Copy of this material.</returns>
    public override MaterialAPI Clone()
    {
        return new MetallicMaterial(this);
    }
}