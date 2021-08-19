using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SimpleBlobAnimationAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //把一个asset转换成Blob，可以理解Blob其实就是一些数据存储的容器，
        var blob = SimpleAnimationBlob.CreateBlob(Curve, Allocator.Persistent);
        
        // 把生成的 blob asset添加到blob asset store里，
        // Add the generated blob asset to the blob asset store.
        
        //创建时发现已经存在同一个blob asset 会共用同一个
        // if another component generates the exact same blob asset, it will automatically be shared.
        
        //BlobAsset被加入到BlobAssetStore里后，生命周期受BlobAssetStore管理
        // Ownership of the blob asset is passed to the BlobAssetStore,
        // it will automatically manage the lifetime of the blob asset.
        conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref blob);

        dstManager.AddComponentData(entity, new SimpleBlobAnimation { Anim = blob });
    }
}

// 具有blobAsset的Component, 使用BlobAssetReference
public struct SimpleBlobAnimation : IComponentData
{
    public BlobAssetReference<SimpleAnimationBlob> Anim;
    public float T;
}

partial class SimpleBlobAnimationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        Entities.ForEach((ref SimpleBlobAnimation anim, ref Translation translation) =>
        {
            anim.T += dt;
            translation.Value.y = anim.Anim.Value.Evaluate(anim.T);
        }).Run(); //主线程执行
    }
}
