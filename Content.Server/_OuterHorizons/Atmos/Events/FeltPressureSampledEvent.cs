namespace Content.Server._OuterHorizons.Atmos.Events;

/// <summary>
/// Raised by <c>BarotraumaSystem</c> on an entity every barotrauma tick, after the pressure
/// has been adjusted for the entity's pressure protection (the "felt" pressure).
/// Anything that depends on how fast the pressure around a mob changes (explosive decompression,
/// and later suit internal pressure) should listen to this instead of sampling the atmosphere again.
/// </summary>
/// <param name="Pressure">Felt pressure in kPa, after pressure protection was applied.</param>
/// <param name="Interval">Time between two samples, in seconds.</param>
[ByRefEvent]
public readonly record struct FeltPressureSampledEvent(float Pressure, float Interval);
