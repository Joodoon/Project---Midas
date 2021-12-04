using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Timer : MonoBehaviour
{
    public bool timerActive = false;
    float currentTime;
    public Text currentTimeText;

    [SerializeField] public CapsuleCollider2D cc;
    [SerializeField] public LayerMask endLayer;


    // Start is called before the first frame update
    void Start()
    {
        currentTime = 0;
        timerActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerActive == true)
        {
            currentTime = currentTime + Time.deltaTime;
        }
        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        currentTimeText.text = time.ToString(@"mm\:ss\:fff");
        endStage();
    }

    public void StartTimer()
    {
        timerActive = true;
    }

    public void StopTimer()
    {
        timerActive = false;
    }

    public void endStage()
    {
        if(cc.IsTouchingLayers(endLayer))
        {
            StopTimer();
        }
    }
}
