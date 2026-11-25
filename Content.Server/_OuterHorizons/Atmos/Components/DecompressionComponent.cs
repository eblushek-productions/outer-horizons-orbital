using Content.Server._OuterHorizons.Atmos.EntitySystems;
using Content.Shared.Damage;

namespace Content.Server._OuterHorizons.Atmos.Components;

/// <summary>
/// Explosive decompression: if the pressure felt by the entity drops too fast,
/// it takes blunt damage and stamina damage scaled by how fast the pressure was lost.
/// Works together with <c>BarotraumaComponent</c>, which supplies the felt pressure.
/// </summary>
[RegisterComponent, Access(typeof(DecompressionSystem))]
public sealed partial class DecompressionComponent : Component
{
    /// <summary>
    /// Pressure loss rate (kPa per second) below which nothing happens.
    /// </summary>
    [DataField]
    public float MinDropRate = 15f;

    /// <summary>
    /// Pressure loss rate (kPa per second) at which the damage coefficient is exactly 1.
    /// </summary>
    [DataField]
    public float ReferenceDropRate = 30f;

    /// <summary>
    /// Exponent applied to (dropRate / ReferenceDropRate). With 1 the total damage of an event only depends on how much
    /// pressure was lost, not on how fast. Values above 1 make faster drops disproportionately more damaging.
    /// </summary>
    [DataField]
    public float RateExponent = 2f;

    /// <summary>
    /// Upper cap for the damage coefficient, applied after the exponent.
    /// </summary>
    [DataField]
    public float MaxCoefficient = 6f;

    /// <summary>
    /// Base damage per tick at coefficient 1. Resistances are ignored,
    /// protection is handled by pressure protection gear.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// Base stamina damage per tick at coefficient 1.
    /// </summary>
    [DataField]
    public float StaminaDamage = 25f;

    /// <summary>
    /// Minimum time between two popups shown to the victim.
    /// </summary>
    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Felt pressure at the previous sample. Null until the first sample, so spawning never triggers decompression.
    /// </summary>
    [ViewVariables]
    public float? LastPressure;

    [ViewVariables]
    public TimeSpan NextPopupTime;
}
