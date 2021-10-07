using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class BackScatterFogEffect : MonoBehaviour
{

    public enum WaterType { JerlovI, JerlovIA, JerlovIB, JerlovII, JerlovIII, Jerlov1C, Jerlov3C, Jerlov5C, Jerlov7C, Jerlov9C };

    public Material _mat;
    public Color _fogColor;
    public float _depthStart;
    public float _depthDistance;
    public float _unitToMeter;
    public float _attenuationCoef_r;
    public float _attenuationCoef_g;
    public float _attenuationCoef_b;
    [Range(0.01f, 1f)]
    public float _attenuationCoef_handle_ocean;  // 由于unity的深度是按照上下限来控制的，所以需要加入一个控制倍率的参数来调整效果:大洋
    [Range(0.1f, 1f)]
    public float _attenuationCoef_handle_nearby;  // 由于unity的深度是按照上下限来控制的，所以需要加入一个控制倍率的参数来调整效果:近海岸
    [Range(0.1f, 3f)]
    public float _Kd_handle;  // Kd下行衰减系数的调整常数

    public WaterType _waterType;
    public int waterTypeIndex;

    [Range(1.5f, 20f)]
    public float _depth;

    float[,] Kd = {
            { 0.163f, 0.0398f, 0.0182f },
            { 0.174f, 0.0460f, 0.0253f },
            { 0.186f, 0.0513f, 0.0325f },
            { 0.223f, 0.0780f, 0.0619f },
            { 0.288f, 0.122f, 0.117f },
            { 0.192f, 0.122f, 0.134f },
            { 0.248f, 0.198f, 0.223f },
            { 0.357f, 0.315f, 0.4f },
            { 0.462f, 0.494f, 0.693f },
            { 0.580f, 0.777f, 1.24f },
    };
    float[,] attenuationCoef = {
        { 0.229f, 0.048f, 0.022f },
        { 0.231f, 0.0508f, 0.027f },
        { 0.2278f+0.0488f, 0.0469f+0.0565f, 0.0225f+0.0635f },
        { 0.228f+0.309f, 0.0469f+0.387f, 0.0228f+0.459f },
        { 0.229f+0.845f, 0.0507f+1.06f, 0.0335f+1.26f },
        { 0.236f+0.314f, 0.068f+0.395f, 0.077f+0.469f },
        { 0.239f+0.916f, 0.078f+1.15f, 0.105f+1.36f },
        { 0.259f+1.13f, 0.127f+1.44f, 0.204f+1.71f },
        { 0.301f+2.03f, 0.233f+2.54f, 0.388f+3.01f },
        { 0.39f+2.69f, 0.43f+3.38f, 0.709f+4.01f },
    };
    float[] particlesL = { 0.0002f, 0.005f, 0.083f, 0.011f, 0.006f, 0.004f, 0.005f, 0.022f, 0.067f, 0.016f };  // 水中大颗粒的浓度 g/m-3
    float[] particlesS = { 0.00008f, 0.002f, 0.03f, 0.401f, 1.1f, 0.402f, 1.21f, 1.50f, 2.64f, 3.54f };  // 水中小颗粒的浓度 g/m-3
    float[] Chl = { 0.01f, 0.027f, 0.037f, 0.044f, 0.177f, 1f, 1.28f, 3.95f, 8.4f, 9.1f };  // 水中小颗粒的浓度 mg/m-3


    Camera cam;
    ParticleSystem underParticle_L;
    ParticleSystem aboveParticle_L;
    ParticleSystem underParticle_S;
    //Color modelGray;
    //ParticleSystem aboveParticle_Chl;

    // 正式注释： 需要变色的灯光系统在这里声明并绑定
    public Light[] lights;
    // public Light sideLight;
    public Color lightModelColor;
    public Material[] materials;
    float emissionIntensity;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        // GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
        cam.clearFlags = CameraClearFlags.SolidColor;

        //_depth = 5f;
        //_waterType = WaterType.JerlovI;
        //_attenuationCoef_handle = 1.00f;
        //_Kd_handle = 1.50f;


        underParticle_L = GameObject.Find("underParticle_L").GetComponentInChildren<ParticleSystem>();
        aboveParticle_L = GameObject.Find("aboveParticle_L").GetComponentInChildren<ParticleSystem>();
        underParticle_S = GameObject.Find("underParticle_S").GetComponentInChildren<ParticleSystem>();
        //aboveParticle_Chl = GameObject.Find("aboveParticle_Chl").GetComponentInChildren<ParticleSystem>();

        
        //ColorUtility.TryParseHtmlString("#D4D4D4", out modelGray);


    }

    // Update is called once per frame
    void Update()
    {
        //if (_depth < 5f)
        //{
        //    _depth = 5f;
        //}

        getWaterTypeIndex();

        if (waterTypeIndex > 0)
        {
            waterTypeIndex -= 1;
        }
        else
        {
            // 应该不会出现
        }
        float Kd_r = Kd[waterTypeIndex, 0] * _Kd_handle;
        float Kd_g = Kd[waterTypeIndex, 1] * _Kd_handle;
        float Kd_b = Kd[waterTypeIndex, 2] * _Kd_handle;

        float reduced_r = Mathf.Exp(-1 * Kd_r * _depth) * 0.9504f; // * D65_r
        float reduced_g = Mathf.Exp(-1 * Kd_g * _depth) * 1f; // * D65_g
        float reduced_b = Mathf.Exp(-1 * Kd_b * _depth) * 1.0889f; // * D65_b

        float reduced_X = 0.49f * reduced_r + 0.31f * reduced_g + 0.2f * reduced_b;
        float reduced_Y = 0.17697f * reduced_r + 0.8124f * reduced_g + 0.01063f * reduced_b;
        float reduced_Z = 0.01f * reduced_g + 0.99f * reduced_b;

        float R = 3.2404542f * reduced_X - 1.5371385f * reduced_Y - 0.4985314f * reduced_Z;
        float G = -0.969266f * reduced_X + 1.8760108f * reduced_Y + 0.041556f * reduced_Z;
        float B = 0.0556434f * reduced_X - 0.2040259f * reduced_Y + 1.0572252f * reduced_Z;

        float r = judgeRGB(R);
        float g = judgeRGB(G);
        float b = judgeRGB(B);
        _fogColor = new Color(r, g, b);

        float light_r = r + lightModelColor.r < 1f ? r + lightModelColor.r : 1f;
        float light_g = g + lightModelColor.g < 1f ? g + lightModelColor.g : 1f;
        float light_b = b + lightModelColor.b < 1f ? b + lightModelColor.b : 1f;

        Color lightColor = new Color(light_r, light_g, light_b);

        cam.backgroundColor = _fogColor;  // 仅在运行时不是空引用

        emissionIntensity = 0.5f - _depth * 0.015f;

        // 正式注释：在这里一个个修改灯光颜色
        // topLight.color = lightColor;
        // sideLight.color = lightColor;
        foreach ( Light l in lights)
        {
            l.color = lightColor;
        }
        foreach (Material m in materials)
        {
            //m.EnableKeyword("_Emission");
            m.SetColor("_EmissionColor", lightColor * emissionIntensity);
        }

        //if (RenderSettings.skybox.HasProperty("_Tint"))
        //    RenderSettings.skybox.SetColor("_Tint", _fogColor);
        //else if (RenderSettings.skybox.HasProperty("_SkyTint"))
        //    RenderSettings.skybox.SetColor("_SkyTint", _fogColor);
        //RenderSettings.skybox.SetColor("_Tint", _fogColor);

        _attenuationCoef_r = attenuationCoef[waterTypeIndex, 0];
        _attenuationCoef_g = attenuationCoef[waterTypeIndex, 1];
        _attenuationCoef_b = attenuationCoef[waterTypeIndex, 2];


        //print("_AttenuationCoef_b:" + _attenuationCoef_b);

        _mat.SetColor("_FogColor", _fogColor);
        _mat.SetFloat("_DepthStart", _depthStart);
        _mat.SetFloat("_DepthDistance", _depthDistance);
        _mat.SetFloat("_UnitToMeter", _unitToMeter);
        _mat.SetFloat("_VerticalDepth", _depth);

        float _attenuationCoef_handle;
        if (waterTypeIndex <= 5)
        {
            _attenuationCoef_handle = _attenuationCoef_handle_ocean;
        }
        else
        {
            _attenuationCoef_handle = _attenuationCoef_handle_nearby;
        }
        _mat.SetFloat("_AttenuationCoef_r", _attenuationCoef_r * _attenuationCoef_handle);
        _mat.SetFloat("_AttenuationCoef_g", _attenuationCoef_g * _attenuationCoef_handle);
        _mat.SetFloat("_AttenuationCoef_b", _attenuationCoef_b * _attenuationCoef_handle);


        // 粒子系统参数调整
        float watertype_BL = particlesL[waterTypeIndex];
        float watertype_BS = particlesS[waterTypeIndex];
        //float watertype_Chl = Chl[waterTypeIndex];


        float emiRateBL = watertype_BL * 1000f;
        if (emiRateBL < 2f)
            emiRateBL = 2f;
        ParticleSystem.EmissionModule underEmiBL = underParticle_L.emission;
        ParticleSystem.EmissionModule aboveEmiBL = aboveParticle_L.emission;
        underEmiBL.rateOverDistanceMultiplier = emiRateBL;
        aboveEmiBL.rateOverDistanceMultiplier = emiRateBL;
        float emiRateBS = watertype_BS * 40f;
        if (emiRateBS < 3f)
            emiRateBS = 3f;
        ParticleSystem.EmissionModule underEmiBS = underParticle_S.emission;
        underEmiBS.rateOverDistanceMultiplier = emiRateBS;

        //Color res = modelGray * _fogColor;
        //ParticleSystem.MainModule underMainBL = underParticle_L.main;
        //underMainBL.startColor = res;

        //underParticleMain_L.startSizeXMultiplier 


        //ParticleSystem underParticle = GameObject.Find("underParticle").GetComponentInChildren<ParticleSystem>();
        //ParticleSystem.MainModule underParticleMain = underParticle.main;
        //underParticleMain.startSizeXMultiplier = 5f;
        //print(underParticleMain.startSizeXMultiplier);


        //ParticleSystem aboveParticle = GameObject.Find("aboveParticle").GetComponentInChildren<ParticleSystem>();
        //ParticleSystem.MainModule aboveParticleMain = aboveParticle.main;

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, _mat);
    }

    private void getWaterTypeIndex()
    {
        int index = 1;
        if (_waterType == WaterType.JerlovI)
        {
            index = 1;
        }
        else if (_waterType == WaterType.JerlovIA)
        {
            index = 2;
        }
        else if (_waterType == WaterType.JerlovIB)
        {
            index = 3;
        }
        else if (_waterType == WaterType.JerlovII)
        {
            index = 4;
        }
        else if (_waterType == WaterType.JerlovIII)
        {
            index = 5;
        }
        else if (_waterType == WaterType.Jerlov1C)
        {
            index = 6;
        }
        else if (_waterType == WaterType.Jerlov3C)
        {
            index = 7;
        }
        else if (_waterType == WaterType.Jerlov5C)
        {
            index = 8;
        }
        else if (_waterType == WaterType.Jerlov7C)
        {
            index = 9;
        }
        else if (_waterType == WaterType.Jerlov9C)
        {
            index = 10;
        }
        else
        {
            index = 0;
        }
        waterTypeIndex = index;
    }

    private float judgeRGB(float c)
    {
        if (c > 1)
        {
            c = 1;
        }
        else if (c < 0)
        {
            c = 0;
        }
        else if (c <= 0.0031308f)
        {
            c *= 12.92f;
        }
        else
        {
            c = 1.055f * Mathf.Pow(c, 1 / 2.4f) - 0.055f;
        }
        return c;
    }


    public void depthChangeWithUI(float d)
    {
        _depth = d;
    }
    public void waterTypeChangeWithUI(WaterType t)
    {
        _waterType = t;
    }
}
