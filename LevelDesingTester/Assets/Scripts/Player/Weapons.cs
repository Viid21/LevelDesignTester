using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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
        public Mesh mesh;
    }
    [Header("Weapon info")]
    public Info weaponInfo;

    public UnityEvent hitEvent;

    public class Modifiers : ScriptableObject
    {

    }

    public void Initialize()
    {
        hitEvent = new UnityEvent();
    }
}
public class HitEvent : UnityEvent<Weapons> { }
#if UNITY_EDITOR
[CustomEditor(typeof(Weapons))]
public class WeaponEditor : Editor
{
    SerializedProperty m_WeaponInfoProperty;

    [MenuItem("Assets/Systems/NewWeapon", priority = 0)]
    public static void CreateNewWeapon()
    {
        var weapons = CreateInstance<Weapons>();

        ProjectWindowUtil.CreateAsset(weapons, "weapons.asset");
    }
    private void OnEnable()
    {
        m_WeaponInfoProperty = serializedObject.FindProperty(nameof(Weapons.weaponInfo));
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
