using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    public RawImage rawImage;
    public Text text;

    // Start is called before the first frame update
    void Start()
    {
        // 获取图片
        UnityWebRequestManager.Instance.GetTexture(Application.dataPath + "/TestAssets/Cat.jpg", 
            SetTexttureToRawImage);

        // 获取文本
        UnityWebRequestManager.Instance.Get(Application.dataPath + "/TestAssets/TextFile.txt", 
            (uwr) => { text.text = uwr.downloadHandler.text;  });

        // 下载文件
        UnityWebRequestManager.Instance.DownloadFile(Application.dataPath + "/TestAssets/TextFile.txt",
            Application.dataPath + "/TestAssets/DownloadTextFile.txt",
            (uwr) => { Debug.Log("文件下载成功：" + Application.dataPath + "/TestAssets/DownloadTextFile.txt");  });

        // 获取音频文件
        UnityWebRequestManager.Instance.GetAudioClip(Application.dataPath + "/TestAssets/AttackFire.wav",
            (audioclip) => {
                this.gameObject.AddComponent<AudioSource>().clip = audioclip;
                this.gameObject.GetComponent<AudioSource>().Play();
            },
            AudioType.WAV
            );
    }



    void SetTexttureToRawImage(Texture texture) {
        rawImage.texture = texture;
    }
}
