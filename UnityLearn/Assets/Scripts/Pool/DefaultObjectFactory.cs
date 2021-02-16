// <summary>
/// 默认对象工厂：相关对象是通过New 出来的
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultObjectFactory<T> : IObjectFactory<T> where T : new()
{
    public T Create()
    {
        return new T();
    }
}
