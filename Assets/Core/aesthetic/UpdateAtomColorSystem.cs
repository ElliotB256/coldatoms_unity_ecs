// Not yet supported! :(

//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Rendering;

//[UpdateBefore(typeof(ForceCalculationSystems))]
//public class UpdateAtomColorSystem : JobComponentSystem
//{
//    [BurstCompile]
//    struct UpdateAtomColorJob : IJobForEach<CollisionStats, RenderMesh>
//    {   
//        public void Execute(
//            ref CollisionStats stats,
//            ref RenderMesh mesh)
//        {
            
//        }
//    }
    
//    protected override JobHandle OnUpdate(JobHandle inputDependencies)
//    {
//        return new UpdateAtomColorJob().Schedule(this, inputDependencies);
//    }
//}