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

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var DataPoints = GetBufferFromEntity<DataPoint>(true);
            var Ranges = GetComponentDataFromEntity<DataRange>(true);

            var Vertices = new NativeArray<float3>(MAX_GRAPH_POINTS * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var Normals = new NativeArray<float3>(MAX_GRAPH_POINTS * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
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

                    // We are now going to create a mesh.
                    int j = 0;
                    for (int i = 0; i < count; i++)
                    {
                        var x1 = XData[i].Value;
                        var y1 = YData[i].Value;
                        var x2 = XData[i + 1].Value;
                        var y2 = YData[i + 1].Value;

                        Vertices[4 * j + 0] = new float3(x1, y1 - graph.LineWidth, 0f);
                        Vertices[4 * j + 1] = new float3(x1, y1 + graph.LineWidth, 0f);
                        Vertices[4 * j + 2] = new float3(x2, y2 - graph.LineWidth, 0f);
                        Vertices[4 * j + 3] = new float3(x2, y2 + graph.LineWidth, 0f);

                        Normals[4 * j + 0] = new float3(0f, 0f, 1f);
                        Normals[4 * j + 1] = new float3(0f, 0f, 1f);
                        Normals[4 * j + 2] = new float3(0f, 0f, 1f);
                        Normals[4 * j + 3] = new float3(0f, 0f, 1f);

                        int it = 6 * j;
                        Triangles[it + 0] = 4 * j + 0;
                        Triangles[it + 1] = 4 * j + 1;
                        Triangles[it + 2] = 4 * j + 2;
                        Triangles[it + 3] = 4 * j + 1;
                        Triangles[it + 4] = 4 * j + 3;
                        Triangles[it + 5] = 4 * j + 2;
                        j++;
                    }

                    renderMesh.mesh.Clear();
                    renderMesh.mesh.SetVertices(Vertices, 0, count * 4);
                    renderMesh.mesh.SetTriangles(Triangles.ToArray(), 0, count * 6, 0);
                    renderMesh.mesh.SetNormals(Normals, 0, count * 4);
                    renderMesh.mesh.bounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f));
                }
                )
                .WithDeallocateOnJobCompletion(Vertices)
                .WithDeallocateOnJobCompletion(Normals)
                .WithDeallocateOnJobCompletion(Triangles)
                .WithName("CreateGraphMesh")
                .WithoutBurst()
                .Run();

            return inputDeps;
        }
    }
}