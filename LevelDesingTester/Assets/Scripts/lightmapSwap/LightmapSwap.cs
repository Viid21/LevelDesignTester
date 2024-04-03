using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightmapSwap : MonoBehaviour
{
    public LightmapData dayLightmap;
    public LightmapData nightLightmap;

    void Update()
    {
        // Ejemplo de cambio de lightmap al presionar la tecla "D" para día y "N" para noche
        if (Input.GetKeyDown(KeyCode.D))
        {
            SwitchToDayLightmap();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            SwitchToNightLightmap();
        }
    }

    void SwitchToDayLightmap()
    {
        LightmapSettings.lightmaps = new LightmapData[] { dayLightmap };
    }

    void SwitchToNightLightmap()
    {
        LightmapSettings.lightmaps = new LightmapData[] { nightLightmap };
    }
}