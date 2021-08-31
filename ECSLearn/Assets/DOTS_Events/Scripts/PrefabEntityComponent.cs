using Unity.Entities;

namespace ECSEvents
{
    [GenerateAuthoringComponent]
    public struct PrefabEntityComponent : IComponentData {

        public Entity pfPipe;

    }

}
