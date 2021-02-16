/// <summary>
/// I pool able.
/// </summary>
public interface IPoolable
{
    void OnRecycled();
    bool IsRecycled { get; set; }  //标记对象是否被回收
}
