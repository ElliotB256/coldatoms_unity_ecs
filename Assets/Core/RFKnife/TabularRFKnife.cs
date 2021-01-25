using System;
using Unity.Entities;
using Unity.Jobs;
using Integration;

/// <summary>
/// Component that signifies RF knife should be controlled via tabular data.
/// </summary>
[Serializable]
[GenerateAuthoringComponent]
public struct TabularRFKnife : IComponentData
{
    public float CurrentTime;
}

[Serializable]
// [GenerateAuthoringComponent]
public struct TabularRFKnifeElement : IBufferElementData
{
    public float Duration;
    public float Radius;
}

/// <summary>
/// A system that updates the RF knife radius according to a table.
/// </summary>
public class TabularRFKnifeSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dt = FixedUpdateGroup.FIXED_TIME_DELTA;
        return Entities.ForEach(
            (DynamicBuffer<TabularRFKnifeElement> elements, ref TabularRFKnife knife, ref Radius radius)
                =>
                {
                    knife.CurrentTime += dt;

                    // get current table index
                    int index = 0;
                    float tableTime = 0f;
                    float startRadius = elements[0].Radius;
                    float endRadius = elements[0].Radius;
                    for (int i = 0; i < elements.Length; i++)
                    {
                        index = i;
                        if (tableTime + elements[i].Duration > knife.CurrentTime)
                        {
                            endRadius = elements[i].Radius;
                            break;
                        }
                        else
                        {
                            tableTime += elements[i].Duration;
                            startRadius = elements[i].Radius;
                        }
                    }

                    // lerp radius
                    float nt = (knife.CurrentTime - tableTime)/(elements[index].Duration);
                    nt = Math.Min(Math.Max(0.0f, nt), 1.0f);
                    radius.Value = (1 - nt) * startRadius + nt * endRadius;
                }
            ).Schedule(inputDeps);
    }
}

