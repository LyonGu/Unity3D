using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class TimeViewUniRX : MonoBehaviour
{
    
    public TimeCounterUniRx timeCounterUniRX;
    public TextMeshProUGUI textMeshProUGUI;
    // Start is called before the first frame update
    void Start()
    {
        timeCounterUniRX.OnTimeChanged
            .Subscribe(time =>
            {
                textMeshProUGUI.text = time.ToString("F3");
            });
    }

   
}
