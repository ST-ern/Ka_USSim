using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDropDown_waterType : MonoBehaviour
{
    Dropdown dropdown;
    GameObject AimCamera;
    BackScatterFogEffect.WaterType waterType;
    int index;

    enum type { };
    // Start is called before the first frame update
    void Start()
    {
        dropdown = gameObject.GetComponent<Dropdown>();
        AimCamera = GameObject.Find("camAim");
        //waterType = AimCamera.GetComponent<BackScatterFogEffect>()._waterType;
        index = AimCamera.GetComponent<BackScatterFogEffect>().waterTypeIndex;

        dropdown.value = index;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetValue(int i)
    {
        Debug.Log(i);

        if (i==0)
        {
            waterType = BackScatterFogEffect.WaterType.JerlovI;
        }
        else if (i == 1)
        {
            waterType = BackScatterFogEffect.WaterType.JerlovIA;
        }
        else if (i == 2)
        {
            waterType = BackScatterFogEffect.WaterType.JerlovIB;
        }
        else if (i == 3)
        {
            waterType = BackScatterFogEffect.WaterType.JerlovII;
        }
        else if (i == 4)
        {
            waterType = BackScatterFogEffect.WaterType.JerlovIII;
        }
        else if (i == 5)
        {
            waterType = BackScatterFogEffect.WaterType.Jerlov1C;
        }
        else if (i == 6)
        {
            waterType = BackScatterFogEffect.WaterType.Jerlov3C;
        }
        else if (i == 7)
        {
            waterType = BackScatterFogEffect.WaterType.Jerlov5C;
        }
        else if (i == 8)
        {
            waterType = BackScatterFogEffect.WaterType.Jerlov7C;
        }
        else if (i == 9)
        {
            waterType = BackScatterFogEffect.WaterType.Jerlov9C;
        }

        AimCamera.GetComponent<BackScatterFogEffect>().waterTypeChangeWithUI(waterType);
    }
}
