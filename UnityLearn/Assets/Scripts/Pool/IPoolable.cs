/// <summary>
/// I pool able.
/// </summary>
public interface IPoolable
{
    void OnRecycled();
    bool IsRecycled { get; set; }  //标记对象是否被回收
}


/// <summary>
/// I cache type.
/// </summary>
public interface IPoolType
{
    void Recycle2Cache();
}
