using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
	public static GameRoot Instance { get; private set; }
	[SerializeField] private int resolutionLimit = 1080;
    void Start()
    {
	    Application.runInBackground = true;
	    Application.targetFrameRate = 30;
	    
	    // Record the singleton.
	    Instance = this;
	    
	    // Listen low memory event.
	    // Application.lowMemory -= this.OnLowMemory;
	    // Application.lowMemory += this.OnLowMemory;

	    // Setup the screen resolution limit.
	    ScreenUtil.CheckAndLimitScreen(this.resolutionLimit);
	    
	    GameMain.Init(GameStart);
    }

    void GameStart()
    {
	    //启动lua
	    LuaManager.GetInstance().Init();
	    LuaManager.GetInstance().StartLua();
    }

}
