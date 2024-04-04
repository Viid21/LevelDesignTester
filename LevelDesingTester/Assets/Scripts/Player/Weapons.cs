using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class WeaponsSO : ScriptableObject
{
    [System.Serializable]
    public struct Info
    {
        public enum E_WEAPONTYPE { PISTOL, SHOTGUN, RIFLE, ARCTHROWER, SNIPER }
        [SerializeField]
        private E_WEAPONTYPE weaponType;
        public E_WEAPONTYPE WeaponType { get => weaponType; }

        public string name;
        public string description;

        public Sprite icon;

        public float damage;
        public float speed;
        public Mesh mesh;
    }
    [Header("Weapon info")]
    public Info weaponInfo;

    public class Modifiers : ScriptableObject
    {

    }
}

public class Weapons : Projectile
{
    public WeaponsSO weaponsSO;
    private void Start()
    {
        if (weaponsSO.weaponInfo.WeaponType == WeaponsSO.Info.E_WEAPONTYPE.PISTOL) 
        {
            
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WeaponsSO))]
public class WeaponEditor : Editor
{
    SerializedProperty m_WeaponInfoProperty;

    [MenuItem("Assets/Systems/NewWeapon", priority = 0)]
    public static void CreateNewWeapon()
    {
        var weapons = CreateInstance<WeaponsSO>();

        ProjectWindowUtil.CreateAsset(weapons, "NewWeapon.asset");
    }
    private void OnEnable()
    {
        m_WeaponInfoProperty = serializedObject.FindProperty(nameof(WeaponsSO.weaponInfo));
    }
    public override void OnInspectorGUI()
    {
        var child = m_WeaponInfoProperty.Copy();
        var depth = child.depth;
        EditorGUILayout.LabelField("Weapon info", EditorStyles.boldLabel);
        while (child.depth > depth)
        {
            EditorGUILayout.PropertyField(child, true);
            child.NextVisible(false);
        }
    }
}
#endif
