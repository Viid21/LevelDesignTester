using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


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
        public GameObject weaponPrefab;
        public Mesh projectileMesh;
        public Button weaponButton;
    }
    [Header("Weapon info")]
    public Info weaponInfo;

    public abstract class Modifiers : ScriptableObject
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WeaponsSO))]
public class WeaponEditor : Editor
{
    SerializedProperty m_WeaponInfoProperty;

    [MenuItem("Assets/Systems/CombatSystem/NewWeapon", priority = 0)]
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

        while (child.depth >= depth)
        {
            EditorGUILayout.PropertyField(child, true);
            child.NextVisible(false);
            serializedObject.ApplyModifiedProperties();
        }        
    }
}
#endif
