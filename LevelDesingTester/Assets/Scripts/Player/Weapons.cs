using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : ScriptableObject
{
    [System.Serializable]
    public struct Info
    {
        public string name;
        public string description;

        public Sprite icon;

        public float damage;
        public float speed;
        enum E_WEAPONTYPE { GUN, SHOTGUN, RIFLE, ARCTHROWER, SNIPER}
    }
}
