using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modifiers : ScriptableObject
{
    [System.Serializable]
    public struct Info
    {
        public string name;
        public string description;

        public Sprite icon;        
        enum E_WEAPONTYPE { NONE, MOREMUNITION, MOREPOJECTILES, MORERANGE}
    }
}
