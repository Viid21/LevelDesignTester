using System.Collections.Generic;
using Unity.VisualScripting;
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

    public UnityEvent hitEvent;

    public float damage;
    protected float damageReactionRadius;
    protected float velocity;
    protected float timeToDestroy;
    protected List<string> avoidTags;
    private Collider col;
    private Rigidbody rb;
    protected Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        mesh = GetComponent<Mesh>();
        hitEvent = new UnityEvent();
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

    protected private void OnTriggerEnter(Collider other)
    {
        hitEvent.Invoke();
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
            if(other.GetComponent<Enemy>() != null)
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
    public class HitEvent : UnityEvent<Projectile> { }

    //tengo k juntar el script de proyectil de jordi junto al quest manager con scriptable object haciendo k las armas y los modifiers sean como las goals de ese script
}
