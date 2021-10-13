using System;


/*
 非托管资源的回收时机：1 手动调用（IDisposable的接口） 2 垃圾回收期自动回收
 */
public class DisposableObject : IDisposable
{
    private Boolean mDisposed = false;

    ~DisposableObject()
    {
        Dispose(false);
    }

    public virtual void Dispose()
    {
        Dispose(true);
        //// 通知垃圾回收机制不再调用终结器（析构器）
        GC.SuppressFinalize(this); 
    }

    // Overrides it, to dispose managed resources.
    protected virtual void DisposeGC()
    {
    }

    // Overrides it, to dispose unmanaged resources
    protected virtual void DisposeNGC()
    {
    }

    private void Dispose(bool disposing)
    {
        if (mDisposed)
        {
            return;
        }

        if (disposing)
        {
            DisposeGC();
        }

        DisposeNGC();

        mDisposed = true;
    }
}