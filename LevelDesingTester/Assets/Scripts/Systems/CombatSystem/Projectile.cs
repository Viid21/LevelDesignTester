using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    enum E_ATTACKMOVEMENT { FORWARD, PARABOLLIC }
    [SerializeField]
    private E_ATTACKMOVEMENT moveMode;
    enum E_DAMAGEMODE { DIRECT, REACTION }
    [SerializeField]
    private E_DAMAGEMODE damageMode;

    public float damage;
    protected float velocity;
    protected float timeToDestroy;
    private Collider col;
    private Rigidbody rb;
    [SerializeField] Weapons weaponSO;

    // Start is called before the first frame update
    protected void Start()
    {
        Destroy(gameObject, timeToDestroy);
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {

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
    private void OnTriggerEnter(Collider other)
    {
        weaponSO.hitEvent.Invoke();
    }

    //tengo k juntar el script de proyectil de jordi junto al quest manager con scriptable object haciendo k las armas y los modifiers sean como las goals de ese script
}
