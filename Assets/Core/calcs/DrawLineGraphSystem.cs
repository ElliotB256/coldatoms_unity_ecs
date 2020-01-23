using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Calculation
{
    [UpdateInGroup(typeof(CalculationSystemGroup))]
    public class DrawLineGraphSystem : JobComponentSystem
    {
        public const int MAX_GRAPH_POINTS = 1024;
        public const int V_PER_SEGMENT = 8;
        public const int I_PER_SEGMENT = 6 * 3;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var DataPoints = GetBufferFromEntity<DataPoint>(true);
            var Ranges = GetComponentDataFromEntity<AxisLimit>(true);

            var Vertices = new NativeArray<float3>(MAX_GRAPH_POINTS * V_PER_SEGMENT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var UVs = new NativeArray<float4>(MAX_GRAPH_POINTS * V_PER_SEGMENT, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var Triangles = new NativeArray<int>(MAX_GRAPH_POINTS * 6, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            inputDeps.Complete();

            Entities.ForEach(
                (in RenderMesh renderMesh, in Graph graph) =>
                {

                    if (!DataPoints.Exists(graph.X) || !DataPoints.Exists(graph.Y))
                        return;
                    if (!Ranges.Exists(graph.X) || !Ranges.Exists(graph.Y))
                        return;

                    var XData = DataPoints[graph.X];
                    var YData = DataPoints[graph.Y];

                    var count = math.min(math.min(math.min(XData.Length, YData.Length), graph.Length), MAX_GRAPH_POINTS) - 1;

                    var xRange = Ranges[graph.X];
                    var yRange = Ranges[graph.Y];

                    // Iterate through points.
                    int j = 0;
                    for (int i = 0; i < count; i++)
                    {
                        var x1 = (XData[i].Value - xRange.Min) / (xRange.Max-xRange.Min);
                        var y1 = (YData[i].Value - yRange.Min) / (yRange.Max - yRange.Min);
                        var x2 = (XData[i + 1].Value - xRange.Min) / (xRange.Max - xRange.Min);
                        var y2 = (YData[i + 1].Value - yRange.Min) / (yRange.Max - yRange.Min);

                        float2 delta = new float2(x2 - x1, y2 - y1);
                        float mag = math.length(delta);
                        if (mag <= 0.0f)
                            continue;
                        delta = delta / mag;
                        float2 transverse = new float2(delta.y, -delta.x);

                        // four vertices at each grid point.
                        Vertices[V_PER_SEGMENT * j + 0] = new float3(x1, y1, 0f);
                        Vertices[V_PER_SEGMENT * j + 1] = new float3(x1, y1, 0f);
                        Vertices[V_PER_SEGMENT * j + 2] = new float3(x1, y1, 0f);
                        Vertices[V_PER_SEGMENT * j + 3] = new float3(x1, y1, 0f);
                        Vertices[V_PER_SEGMENT * j + 4] = new float3(x2, y2, 0f);
                        Vertices[V_PER_SEGMENT * j + 5] = new float3(x2, y2, 0f);
                        Vertices[V_PER_SEGMENT * j + 6] = new float3(x2, y2, 0f);
                        Vertices[V_PER_SEGMENT * j + 7] = new float3(x2, y2, 0f);

                        UVs[V_PER_SEGMENT * j + 0] = new float4(-delta, -transverse);
                        UVs[V_PER_SEGMENT * j + 1] = new float4(-delta, transverse);
                        UVs[V_PER_SEGMENT * j + 2] = new float4(new float2(), -transverse);
                        UVs[V_PER_SEGMENT * j + 3] = new float4(new float2(), transverse);
                        UVs[V_PER_SEGMENT * j + 4] = new float4(new float2(), -transverse);
                        UVs[V_PER_SEGMENT * j + 5] = new float4(new float2(), transverse);
                        UVs[V_PER_SEGMENT * j + 6] = new float4(delta, -transverse);
                        UVs[V_PER_SEGMENT * j + 7] = new float4(delta, transverse);

                        int it = I_PER_SEGMENT * j;
                        Triangles[it + 0] = V_PER_SEGMENT * j + 0;
                        Triangles[it + 1] = V_PER_SEGMENT * j + 1;
                        Triangles[it + 2] = V_PER_SEGMENT * j + 2;
                        Triangles[it + 3] = V_PER_SEGMENT * j + 1;
                        Triangles[it + 4] = V_PER_SEGMENT * j + 3;
                        Triangles[it + 5] = V_PER_SEGMENT * j + 2;
                        Triangles[it + 6] = V_PER_SEGMENT * j + 0 + 2;
                        Triangles[it + 7] = V_PER_SEGMENT * j + 1 + 2;
                        Triangles[it + 8] = V_PER_SEGMENT * j + 2 + 2;
                        Triangles[it + 9] = V_PER_SEGMENT * j + 1 + 2;
                        Triangles[it + 10] = V_PER_SEGMENT * j + 3 + 2;
                        Triangles[it + 11] = V_PER_SEGMENT * j + 2 + 2;
                        Triangles[it + 12] = V_PER_SEGMENT * j + 0 + 4;
                        Triangles[it + 13] = V_PER_SEGMENT * j + 1 + 4;
                        Triangles[it + 14] = V_PER_SEGMENT * j + 2 + 4;
                        Triangles[it + 15] = V_PER_SEGMENT * j + 1 + 4;
                        Triangles[it + 16] = V_PER_SEGMENT * j + 3 + 4;
                        Triangles[it + 17] = V_PER_SEGMENT * j + 2 + 4;
                        j++;
                    }

                    renderMesh.mesh.Clear();
                    renderMesh.mesh.SetVertices(Vertices, 0, j * V_PER_SEGMENT);
                    renderMesh.mesh.SetTriangles(Triangles.ToArray(), 0, j * I_PER_SEGMENT, 0);
                    renderMesh.mesh.SetUVs(0, UVs, 0, j * V_PER_SEGMENT);
                    renderMesh.mesh.bounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f));
                }
                )
                .WithDeallocateOnJobCompletion(Vertices)
                .WithDeallocateOnJobCompletion(UVs)
                .WithDeallocateOnJobCompletion(Triangles)
                .WithName("CreateGraphMesh")
                .WithoutBurst()
                .Run();

            return inputDeps;
        }
    }
}