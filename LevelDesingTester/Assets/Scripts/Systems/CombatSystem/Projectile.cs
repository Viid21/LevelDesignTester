using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Projectile : MonoBehaviour
{
    public enum E_ATTACKMOVEMENT { FORWARD, PARABOLLIC }
    [SerializeField]
    private E_ATTACKMOVEMENT moveMode;
    public abstract E_ATTACKMOVEMENT MoveMode { get; protected set; }

    enum E_DAMAGEMODE { DIRECT, REACTION }
    [SerializeField]
    private E_DAMAGEMODE damageMode;

    enum E_PROJECTILETYPE { PROJECTILE, HITSCAN }
    [SerializeField]
    private E_PROJECTILETYPE projectilileType;    

    public float damage;
    protected float damageReactionRadius;
    protected float velocity;
    protected float timeToDestroy;
    protected List<string> avoidTags;
    private Collider col;
    private Rigidbody rb;
    protected Mesh mesh;

    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        EventManager.StopListening("ImpactEvent", OnImpact);
    }
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        mesh = GetComponent<Mesh>();
        SetWeaponType();
    }
    protected private void FixedUpdate()
    {
        if (moveMode == E_ATTACKMOVEMENT.FORWARD)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.AddForce(transform.forward * velocity);
        }
    }
    public virtual void SetWeaponType()
    {

    }
    public virtual void OnImpact()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        EventManager.TriggerEvent("ImpactEvent");

        if (avoidTags.Contains(other.tag))
        {
            return;
        }

        if (damageMode == E_DAMAGEMODE.DIRECT)
        {
            if (other.GetComponent<PlayerControllerDRM>() != null)
            {
                other.GetComponent<PlayerControllerDRM>().health -= damage;
                Destroy(gameObject);
            }
            if (other.GetComponent<Enemy>() != null)
            {
                other.GetComponent<Enemy>().health -= damage;
            }
        }
        else if (damageMode == E_DAMAGEMODE.REACTION)
        {
            Destroy(col);
            SphereCollider newCol = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
            newCol.isTrigger = true;
            newCol.radius = damageReactionRadius;
            damageMode = E_DAMAGEMODE.DIRECT;
            Destroy(gameObject, 0.2f);
            //TODO: ACTIVAR SISTEMA DE PARTICULAS DE EXPLOSION
        }
    }

    public abstract class Weapons : Projectile
    {
        public WeaponsSO weaponsSO;

        public override void SetWeaponType()
        {
            if (weaponsSO.weaponInfo.WeaponType == WeaponsSO.Info.E_WEAPONTYPE.PISTOL)
            {
                MoveMode = E_ATTACKMOVEMENT.FORWARD;
                damage = weaponsSO.weaponInfo.damage;
                velocity = weaponsSO.weaponInfo.speed;
                mesh = weaponsSO.weaponInfo.projectileMesh;
            }
        }
    }
}
