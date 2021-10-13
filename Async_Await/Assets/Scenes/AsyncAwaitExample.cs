using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncAwaitExample : MonoBehaviour
{

    private bool isContinue = false;
    
    // Start is called before the first frame update
    async void Start()
    {
        //虽然被标记async异步关键字，但是，还是在当前线程执行 主线程执行
        Debug.Log($"测试 Async await ====1 ThreadId:{Thread.CurrentThread.ManagedThreadId}");
        bool result = await StopProgram();
        Debug.Log($"测试 Async await ====2 ThreadId:{Thread.CurrentThread.ManagedThreadId}");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isContinue = true;
        }
    }

    //异步获取(实际是开一个线程轮询结果)
    private async Task<bool> GetIsContinue()
    {
        return await Task.Run<bool>(() => {
                while (true) {
                    if (isContinue) 
                        return true; 
                }
            });
    }

    private async Task<bool> StopProgram()
    {
        bool result = await GetIsContinue();
        Debug.Log($"Program is to==== ThreadId:{Thread.CurrentThread.ManagedThreadId}");
        return result;
        
    }
}
