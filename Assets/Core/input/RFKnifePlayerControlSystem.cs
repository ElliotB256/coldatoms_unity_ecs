using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Updates the size of the RF knife entity so that the visual representation matches the knife radius.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class RFKnifePlayerControlSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Update RF knife radius if mouse is down and mouse position is moving.
        float3 mousePos = Input.mousePosition;
        float3 midPoint = new float3(Screen.width, Screen.height, 0f)/2f;

        float dT = Time.DeltaTime;
        Entities
            .WithAll<RFKnife>()
            .ForEach(
                (ref Radius radius, in PlayerInputs controls) =>
                {
                    //radius.Value *= (1 + controls.VerticalAxis * dT / 2);
                    float distance = math.length(mousePos - midPoint);
                    float normal = math.length(controls.clickPosition - midPoint);
                    float factor = math.abs(distance / normal);

                    if (controls.buttonDown)
                        radius.Value = radius.R0 * factor;
                    else
                        radius.R0 = radius.Value;
                }
            ).Schedule();
    }
}