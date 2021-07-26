using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Player : MonoBehaviour
{
    public TimeCounterUniRx timeCounterUniRX;
    public float moveSpeed = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        timeCounterUniRX.OnTimeChanged
            .Where(x => x <= 0)
            .Subscribe(x =>
            {
                Debug.Log("X=======" + x);
                transform.localPosition = Vector3.zero;
            }).AddTo(this.gameObject); //让流的生命周期跟gemeObject保持一致
    }

    private void Update()
    {
        var xzValue = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (xzValue.magnitude > 0.1f)
        {
            transform.localPosition += xzValue * moveSpeed * Time.deltaTime;
        }
        if (transform.localPosition.x > 5)
        {
            Debug.Log("Game Over");
            Destroy(this.gameObject);
        }
    }
}
