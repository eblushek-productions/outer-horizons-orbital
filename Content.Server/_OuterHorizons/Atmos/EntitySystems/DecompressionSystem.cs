using Content.Server._OuterHorizons.Atmos.Components;
using Content.Server._OuterHorizons.Atmos.Events;
using Content.Server.Administration.Logs;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._OuterHorizons.Atmos.EntitySystems;

/// <summary>
/// Explosive decompression. Listens to the felt pressure sampled by <c>BarotraumaSystem</c>
/// and punishes sudden pressure drops with blunt damage and stamina damage,
/// both multiplied by a coefficient that grows with the pressure loss rate.
/// </summary>
public sealed partial class DecompressionSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IGameTiming _timing = default!;

    [SubscribeLocalEvent]
    private void OnPressureSampled(Entity<DecompressionComponent> ent, ref FeltPressureSampledEvent args)
    {
        var comp = ent.Comp;

        var previous = comp.LastPressure;
        comp.LastPressure = args.Pressure;

        // First sample, nothing to compare with yet.
        if (previous is not { } last || args.Interval <= 0f)
            return;

        // kPa lost per second. Rising pressure gives a negative value and is ignored.
        var dropRate = (last - args.Pressure) / args.Interval;
        if (dropRate < comp.MinDropRate)
            return;

        if (_mobState.IsDead(ent.Owner))
            return;

        var ratio = dropRate / MathF.Max(comp.ReferenceDropRate, 0.01f);
        var coefficient = MathF.Min(MathF.Pow(ratio, comp.RateExponent), comp.MaxCoefficient);

        // Same flags as BarotraumaSystem: ignore resistances, resistance comes from pressure protection.
        _damageable.TryChangeDamage(ent.Owner, comp.Damage * coefficient, true, false);
        _stamina.TakeStaminaDamage(ent.Owner, comp.StaminaDamage * coefficient, visual: false, ignoreResist: true);

        _adminLogger.Add(LogType.Barotrauma, LogImpact.Low,
            $"{ToPrettyString(ent.Owner):entity} suffered decompression (pressure loss {dropRate:F1} kPa/s, coefficient {coefficient:F2})");

        if (_timing.CurTime >= comp.NextPopupTime)
        {
            comp.NextPopupTime = _timing.CurTime + comp.PopupCooldown;
            _popup.PopupEntity(Loc.GetString("decompression-popup"), ent.Owner, ent.Owner, PopupType.MediumCaution);
        }
    }
}
