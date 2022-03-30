using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameUtils
{
       public class SaveFileCache {
        public byte[] data;
        public string saveFileName;
    }
    public static class DownloadTool
    {
        private static string savePath = Application.persistentDataPath;
        private static System.Object locker = new System.Object();
        private static Thread m_Thread;
        private static Queue<SaveFileCache> m_readyToSaveQueue = new Queue<SaveFileCache>();
        private static string version = "version1.0";
        public static void DownloadTexture(string url, string saveFileName = null, Action<Texture> callback = null)
        {
            var dir = Path.Combine(savePath, "tempCache");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!string.IsNullOrEmpty(saveFileName))
            {
                DownloadTextureFunc(url, GetMD5(saveFileName), true, callback);
            }

            else
                DownloadTextureFunc(url,null, false, callback);


        }

        private static void DownloadTextureFunc(string url, string saveFileName = null, bool fromLocal = false, Action<Texture> callback = null)
        {
            UnityWebRequest uwr;
            if (fromLocal)
            {
                var dir = Path.Combine(savePath, "tempCache");
                var path = Path.Combine(dir, saveFileName);
                var uri = new Uri(path);
                uwr = UnityWebRequestTexture.GetTexture(uri);
            }
            else
            {
                var path = url;
                var uri = new Uri(path);
                uwr = UnityWebRequestTexture.GetTexture(uri);
            }

            uwr.disposeDownloadHandlerOnDispose = true;

            uwr.SendWebRequest().completed += delegate (AsyncOperation t)
            {

                if (t.isDone)
                {
                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        if (fromLocal)
                        {
                            DownloadTextureFunc(url, saveFileName, false, callback);
                        }
                        else
                        {
                            callback?.Invoke(null);
                        }

                    }
                    else
                    {

                        Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                        int width = texture.width;
                        int height = texture.height;
                        if (width > 256)
                        {
                            int tempW = 256;
                            height = 256 * height / width;
                            width = tempW;
                            Texture2D newtexture = ScaleTexture(texture, width, height);
                            GameObject.Destroy(texture);
                            texture = newtexture;
                        }
                        else if (height > 256)
                        {
                            int tempH = 256;
                            width = 256 * width / height;
                            height = tempH;
                            Texture2D newtexture = ScaleTexture(texture, width, height);
                            GameObject.Destroy(texture);
                            texture = newtexture;
                        }
                        callback?.Invoke(texture);


                        if (!fromLocal && !string.IsNullOrEmpty(saveFileName))
                        {
                            byte[] bytes = uwr.downloadHandler.data;
                            System.Text.StringBuilder sb = new System.Text.StringBuilder(savePath);
                            sb.Append("/tempCache");
                            sb.Append("/");
                            sb.Append(saveFileName);
                            if (m_Thread == null)
                            {
                                m_Thread = new Thread(new ThreadStart(() =>
                                {
                                    SaveFile(sb.ToString(), bytes);
                                }));
                                m_Thread.Name = "DownloadTextureFunc";
                                m_Thread.IsBackground = true;
                                m_Thread.Start();

                            }
                            else
                            {
                                lock (locker)
                                {
                                    SaveFileCache fileCache = new SaveFileCache();
                                    fileCache.data = bytes;
                                    fileCache.saveFileName = sb.ToString();
                                    m_readyToSaveQueue.Enqueue(fileCache);
                                }

                            }


                        }

                    }
                    uwr.Dispose();
                }
            };
        }

        public static void SaveStringData(string saveName, string data)
        {
            string filePath = string.Concat(Application.persistentDataPath, "/", GetMD5(string.Concat(saveName,version)));
            StreamWriter writer = new StreamWriter((filePath), false, System.Text.Encoding.Default);
            var newData = MD5Encryption(data);
            writer.Write(newData);
            writer.Close();
        }

        public static string GetStringData(string saveName)
        {
            string filePath = string.Concat(Application.persistentDataPath, "/", GetMD5(string.Concat(saveName, version)));
            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader((filePath), System.Text.Encoding.Default);
                string data = reader.ReadToEnd();
                reader.Close();
                try
                {
                    data = MD5Decryption(data);
                }
                catch(Exception ex)
                {
                    Debug.LogError($"GetStringData error, {ex.ToString()}");
                    DeleteFile(saveName);
                    return null;
                }
                return data;
            }
            else
                return null;
        }

        public static void DeleteFile(string saveName)
        {
            string filePath = string.Concat(Application.persistentDataPath, "/", GetMD5(string.Concat(saveName, version)));
            File.Delete((filePath));
        }

        private static void SaveFile(string path, byte[] bytes)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(path);
            sb.Append("_temp");

            if (File.Exists(sb.ToString()))
            {
                File.Delete(sb.ToString());
            }




            File.WriteAllBytes(sb.ToString(), bytes);
            File.Copy(sb.ToString(), path, true);
            File.Delete(sb.ToString());
            SaveFileCache fileCache = null;
            lock (locker)
            {
                if (m_readyToSaveQueue.Count > 0)
                {
                    fileCache = m_readyToSaveQueue.Dequeue();
                }

            }

            if (fileCache == null)
            {
                m_Thread = null;
            }
            else
            {
                SaveFile(fileCache.saveFileName, fileCache.data);
            }



        }
        private static string GetMD5(string str)
        {

            byte[] resultBytes = System.Text.Encoding.UTF8.GetBytes(str);
            //创建一个MD5的对象
            MD5 md5 = new MD5CryptoServiceProvider();
            //调用MD5的ComputeHash方法将字节数组加密
            byte[] outPut = md5.ComputeHash(resultBytes);
            System.Text.StringBuilder hashString = new System.Text.StringBuilder();
            //最后把加密后的字节数组转为字符串
            for (int i = 0; i < outPut.Length; i++)
            {
                hashString.Append(Convert.ToString(outPut[i], 16).PadLeft(2, '0'));
            }
            md5.Dispose();
            return hashString.ToString();
        }

        private static string MD5Encryption(string inputData)
        {
            string hasKey = version; 
            byte[] bData = UTF8Encoding.UTF8.GetBytes(inputData);

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            TripleDESCryptoServiceProvider tripalDES = new TripleDESCryptoServiceProvider();

            tripalDES.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hasKey));
            tripalDES.Mode = CipherMode.ECB;

            ICryptoTransform trnsfrm = tripalDES.CreateEncryptor();
            byte[] result = trnsfrm.TransformFinalBlock(bData, 0, bData.Length);

            return Convert.ToBase64String(result);
        }
        private static string MD5Decryption(string inputData)
        {
            string hasKey = version; 
            byte[] bData = Convert.FromBase64String(inputData);

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            TripleDESCryptoServiceProvider tripalDES = new TripleDESCryptoServiceProvider();

            tripalDES.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hasKey));
            tripalDES.Mode = CipherMode.ECB;

            ICryptoTransform trnsfrm = tripalDES.CreateDecryptor();
            byte[] result = trnsfrm.TransformFinalBlock(bData, 0, bData.Length);

            return UTF8Encoding.UTF8.GetString(result);
        }

        private static Texture2D ScaleTexture(Texture2D source, float targetWidth, float targetHeight)
        {
            Texture2D result = new Texture2D((int)targetWidth, (int)targetHeight, source.format, false);

            float incX = (1.0f / targetWidth);
            float incY = (1.0f / targetHeight);

            for (int i = 0; i < result.height; ++i)
            {
                for (int j = 0; j < result.width; ++j)
                {
                    Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                    result.SetPixel(j, i, newColor);
                }
            }

            result.Apply();
            return result;
        }

    }

    
    //判断是否包含Emoji表情字符
    public static bool ContainsEmoji(string source)
    {
        if (string.IsNullOrEmpty(source))
            return false;

        int len = source.Length;
        bool isEmoji = false;
        for (int i = 0; i < len; i++)
        {
            char hs = source[i];
            if (0xd800 < hs && hs <= 0xdbff)
            {
                if (source.Length > 1)
                {
                    char ls = source[i + 1];
                    int uc = ((hs - 0xd800) * 0x400) + (ls - 0xdc00) + 0x10000;
                    if (0x1d000 < uc && uc <= 0x1f77f)
                        return true;
                }
            }
            else
            {
                if (0x2100 <= hs && hs <= 0x27ff && hs != 0x263b)
                {
                    return true;
                }
                else if (0x2B05 <= hs && hs <= 0x2b07)
                {
                    return true;
                }
                else if (0x2934 <= hs && hs <= 0x2935)
                {
                    return true;
                }
                else if (0x3297 <= hs && hs <= 0x3299)
                {
                    return true;
                }
                else if (hs == 0xa9 || hs == 0xae || hs == 0x303d || hs == 0x3030 || hs == 0x2b55 || hs == 0x2b1c || hs == 0x2b1b || hs == 0x2b50 || hs == 0x231a)
                {
                    return true;
                }
                if (!isEmoji && source.Length > 1 && i < source.Length - 1)
                {
                    char ls = source[i + 1];
                    if (ls == 0x20e3)
                    {
                        return true;
                    }
                }
            }
        }
        return isEmoji;
    }
    
    #region Unity组件事件注册相关

        public static void AddToggleValueChangeEvent(Toggle toggle, UnityAction<bool> valueChangeCall)
        {
            if (toggle != null && valueChangeCall != null)
            {
                toggle.onValueChanged.AddListener(valueChangeCall);
            }
        }

        public static void RemoveToggleValueChangeEvent(Toggle toggle, UnityAction<bool> valueChangeCall)
        {
            if (toggle != null && valueChangeCall != null)
            {
                //toggle.onValueChanged.RemoveListener(valueChangeCall);
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.Invoke(false);
            }
        }

        public static void AddButtonOnClickEvent(Button button, UnityAction onClickCall)
        {
            if (button != null && onClickCall != null)
            {
                button.onClick.AddListener(onClickCall);
            }
        }

        public static void RemoveButtonOnClickEvent(Button button, UnityAction onClickCall)
        {
            if (button != null && onClickCall != null)
            {
                //button.onClick.RemoveListener(onClickCall);
                button.onClick.RemoveAllListeners();
                button.onClick.Invoke();
            }
        }

        public static void AddSliderOnValueChangeEvent(Slider slider, UnityAction<float> valueChangeCall)
        {
            if (slider != null && valueChangeCall != null)
            {
                slider.onValueChanged.AddListener(valueChangeCall);
            }
        }

        public static void RemoveSliderOnValueChangeEvent(Slider slider, UnityAction<float> valueChangeCall)
        {
            if (slider != null && valueChangeCall != null)
            {
                //slider.onValueChanged.RemoveListener(valueChangeCall);
                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.Invoke(0);
            }
        }

        public static void AddInputFieldOnValueChangeEvent(InputField inputField, UnityAction<string> valueChangeCall)
        {
            if (inputField != null && valueChangeCall != null)
            {
                inputField.onValueChanged.AddListener(valueChangeCall);
            }
        }

        public static void RemoveInputFieldOnValueChangeEvent(InputField inputField,
            UnityAction<string> valueChangeCall)
        {
            if (inputField != null && valueChangeCall != null)
            {
                //inputField.onValueChanged.RemoveListener(valueChangeCall);
                inputField.onValueChanged.RemoveAllListeners();
                inputField.onValueChanged.Invoke(string.Empty);
            }
        }

        public static void AddTMInputFieldOnValueChangeEvent(TMP_InputField inputField,
            UnityAction<string> valueChangeCall)
        {
            if (inputField != null && valueChangeCall != null)
            {
                inputField.onValueChanged.AddListener(valueChangeCall);
            }
        }

        public static void RemoveTMInputFieldOnValueChangeEvent(TMP_InputField inputField,
            UnityAction<string> valueChangeCall)
        {
            if (inputField != null && valueChangeCall != null)
            {
                //inputField.onValueChanged.RemoveListener(valueChangeCall);
                inputField.onValueChanged.RemoveAllListeners();
                inputField.onValueChanged.Invoke(string.Empty);
            }
        }

        public static void AddDropDownOnValueChangeEvent(Dropdown dropdown, UnityAction<int> valueChangeCall)
        {
            if (dropdown != null && valueChangeCall != null)
            {
                dropdown.onValueChanged.AddListener(valueChangeCall);
            }
        }

        public static void RemoveDropDownOnValueChangeEvent(Dropdown dropdown, UnityAction<int> valueChangeCall)
        {
            if (dropdown != null && valueChangeCall != null)
            {
                //dropdown.onValueChanged.RemoveListener(valueChangeCall);
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.Invoke(0);
            }
        }

        public static void AddTMDropDownOnValueChangeEvent(TMP_Dropdown tMP_Dropdown, UnityAction<int> valueChangeCall)
        {
            if (tMP_Dropdown != null && valueChangeCall != null)
            {
                tMP_Dropdown.onValueChanged.AddListener(valueChangeCall);
            }
        }

        public static void RemoveTMDropDownOnValueChangeEvent(TMP_Dropdown tMP_Dropdown,
            UnityAction<int> valueChangeCall)
        {
            if (tMP_Dropdown != null && valueChangeCall != null)
            {
                //tMP_Dropdown.onValueChanged.RemoveListener(valueChangeCall);
                tMP_Dropdown.onValueChanged.RemoveAllListeners();
                tMP_Dropdown.onValueChanged.Invoke(0);
            }
        }

        #endregion
    
    //多边形区域判断
    public static bool IsInPolygon(float targetX, float targetY, List<float> posXList, List<float> posYList)
        {
            if (posXList.Count == 0 || posYList.Count == 0)
            {
                return false;
            }

            if (posXList.Count != posYList.Count)
            {
                return false;
            }

            int counter = 0;
            int i;
            float xinters;
            float p1_x = 0;
            float p1_y = 0;
            float p2_x = 0;
            float p2_y = 0;
            int pointCount = posXList.Count;
            p1_x = posXList[0];
            p1_y = posYList[0];
            for (i = 1; i <= pointCount; i++)
            {
                p2_x = posXList[i % pointCount];
                p2_y = posYList[i % pointCount];
                if (targetY > Mathf.Min(p1_y, p2_y) //校验点的Y大于线段端点的最小Y
                    && targetY <= Mathf.Max(p1_y, p2_y)) //校验点的Y小于线段端点的最大Y
                {
                    if (targetX <= Mathf.Max(p1_x, p2_x)) //校验点的X小于等线段端点的最大X(使用校验点的左射线判断).
                    {
                        if (p1_y != p2_y) //线段不平行于X轴
                        {
                            xinters = (targetY - p1_y) * (p2_x - p1_x) / (p2_y - p1_y) + p1_x;
                            if (p1_x == p2_x || targetX <= xinters)
                            {
                                counter++;
                            }
                        }
                    }
                }

                p1_x = p2_x;
                p1_y = p2_y;
            }

            if (counter % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    #region 下载网络图片

    public static void GetURLImage(string url, Action<Texture> DownloadHandler)
    {

        DownloadTool.DownloadTexture(url, "", DownloadHandler);

/*            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
            uwr.SendWebRequest().completed += delegate (AsyncOperation t)
            {

                if (t.isDone)
                {
                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    DownloadHandler?.Invoke(texture);
                }
            };*/
    }
    
    public static void GetURLImageAndSave(string url, string saveName, Action<Texture> DownloadHandler)
    {
        DownloadTool.DownloadTexture(url, saveName, DownloadHandler);
    }

    #endregion
    
    
    //修改layerIndex
    public static void ToChangeGameObjectLayer(GameObject obj, int layerValue, bool isEnableGraphicRaycaster)
    {

        if (obj == null)
            return;
        obj.layer = layerValue;
        var rootTrans = obj.transform;
        int childCount = rootTrans.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var t = rootTrans.GetChild(i);
            t.gameObject.layer = layerValue;
            if (t.childCount > 0)
            {
                ToChangeGameObjectLayer(t.gameObject, layerValue, isEnableGraphicRaycaster);
            }
        }
        
        GraphicRaycaster graphicRaycaster = obj.GetComponent<GraphicRaycaster>();
        if (graphicRaycaster)
        {
            graphicRaycaster.enabled = isEnableGraphicRaycaster;
        }

    }

    #region 坐标转化

     public static Vector2 Screen2UGUI(
         float screenPosX, 
         float screenPosY, 
         RectTransform target, 
         RectTransform parent, 
         Camera uiCamera)
    {
        Vector2 outVec2 = Vector2.zero;
        Vector2 screenPos = Vector2.zero;
        screenPos.x = screenPosX;
        screenPos.y = screenPosY;
        if (uiCamera == null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, null, out outVec2);
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, uiCamera, out outVec2);
        }

        Rect pRect = parent.rect;
        float pW = pRect.width;
        float pH = pRect.height;
        Vector2 anchorMax = target.anchorMax;
        Vector2 anchorMin = target.anchorMin;
        float anchorOffsetX, anchorOffsetY;

        float maxX = anchorMax.x;
        float maxY = anchorMax.y;
        float minX = anchorMin.x;
        float minY = anchorMin.y;

        if (Mathf.Abs(maxX - minX) <= 0.1f && Mathf.Abs(maxY - minY) <= 0.1f)
        {
            //锚点为一个点
            anchorOffsetX = 0.5f - maxX;
            anchorOffsetY = 0.5f - maxY;
        }
        else
        {
            //锚点为一个区域（拉伸）
            anchorOffsetX = 0.5f - (maxX + minX) * 0.5f;
            anchorOffsetY = 0.5f - (maxY + minY) * 0.5f;
        }

        outVec2.x = outVec2.x + anchorOffsetX * pW;
        outVec2.y = outVec2.y + anchorOffsetY * pH;
        return outVec2;
    }
     public static Vector2 UGUI2Screen(RectTransform rectTransform, GameObject UIRoot, Camera uiCamera)
     {
         Vector2 result = Vector2.zero;
         Vector3 wPos = rectTransform.position;
         Canvas rootCanvas = UIRoot.GetComponent<Canvas>();
         if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
         {
             //--overlay 模式，屏幕坐标系原点和世界坐标重合，所以世界坐标就是屏幕坐标
             result.x = wPos.x;
             result.y = wPos.y;
         }
         else
         {
             if (uiCamera != null)
             {
                 result = World2Screen(wPos.x, wPos.y, wPos.z, uiCamera);
             }
             else
             {
                 //--不设置摄像机，屏幕坐标系原点和世界坐标重合，所以世界坐标就是屏幕坐标
                 result.x = wPos.x;
                 result.y = wPos.y;
             }
         }

         return result;
     }
     
     public static Vector3 Screen2World(float screenPosX, float screenPosY, float posZ, Camera camera)
     {
         /*
         [==[
             1.使用ScreenToWorldPoint将获取的屏幕位置直接转成世界坐标：
                 坑1：获取的屏幕坐标Input.mousePosition是一个2d坐标，z轴值为0,
                      这个z值是相对于当前camera的，为零表示z轴与相机重合了，
                      因此给ScreenToWorlfdPoint传值时，不能直接传Input.mousePosition，否则获取的世界坐标永远只有一个值；

                 坑2：为了解决坑1，便使传入的z轴值，那么传什么值呢，可以取当前相机的z轴值绝对值，
                      大概意思就是距离相机的距离，生成点的位置最后会在一个平面（因为传入的z值固定了），比较适合一个平面内取点
         ]==]
         */
         Vector3 result = Vector3.zero;
         result = camera.ScreenToWorldPoint(new Vector3(screenPosX, screenPosY, posZ));
         return result;
     }
     public static Vector3 World2Screen(float x, float y, float z, Camera camera)
     {
         var wPos = new Vector3(x,y,z);
         if (camera == null)
         {
             camera = Camera.main;
         }

         if (camera)
         {
             return camera.WorldToScreenPoint(wPos);
         }

         return Vector3.zero;
     }
     
     public static Vector3 World2Viewport(float wx,float wy, Camera camera)
     {
         if (camera == null)
         {
             camera = Camera.main;
         }

         if (camera)
         {
             return camera.WorldToViewportPoint(new Vector3(wx, wy, 0));
         }

         return Vector3.zero;
     }

    #endregion
    
    public static UnityWebRequest HttpPost(string url, string json)
    {
        var request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");
        var raw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();
        return request;
    }
    
    /// <summary>
    /// 用于UnityEngine.Object及其子类对象的判空
    /// 在使用DestroyImmediate销毁一个UnityEngine.Object对象时，该对象会被Unity认为已经是null
    /// 但是C#并不认为它是null
    /// 因此在与Lua交互时，不能直接在Lua侧判断对象是否为nil（这样判断走的是C#的判空），应该调用此方法（走的是Unity的判空）
    /// </summary>
    public static bool IsNull(UnityEngine.Object target)
    {
        return target == null;
    }
    
    public static void AnimatorToStart(Animator animator, string stateName)
    {
        if (animator != null)
        {
            animator.Play(stateName, 0, 0);
            animator.Update(0);
        }
    }
    
    public static void AnimatorSetSpeed(Animator animator, float speedValue)
    {
        if (animator != null)
        {
            animator.speed = speedValue;
        }
    }
}
