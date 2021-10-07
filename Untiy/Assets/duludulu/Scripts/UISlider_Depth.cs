using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISlider_Depth : MonoBehaviour
{
    Slider slider;
    //WaterType _waterType;
    [Range(1.5f, 20f)]
    float _depth;
    GameObject AimCamera;


    void Start()
    {
        slider = gameObject.GetComponent<Slider>();
        AimCamera = GameObject.Find("camAim");
        //BackScatterFogEffect.WaterType _waterType1 = AimCamera.GetComponent<BackScatterFogEffect>()._waterType;
        //_waterType = _waterType1;
        _depth = AimCamera.GetComponent<BackScatterFogEffect>()._depth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetValue(float i)
    {
        Debug.Log(i);
        AimCamera.GetComponent<BackScatterFogEffect>().depthChangeWithUI(i);
    }
}
