using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceAttack : MonoBehaviour
{
    enum E_ATTACKMOVEMENT { FORWARD, PARABOLLIC}
    [SerializeField]
    private E_ATTACKMOVEMENT moveMode;
    enum E_DAMAGEMODE { DIRECT, REACTION}
    [SerializeField]
    private E_DAMAGEMODE damageMode;
    
    [SerializeField]
    private float damageReactionRadius;
    private Collider col;
    private PlayerController controller;
    private Rigidbody rb;
    [SerializeField]
    private float force;
    [SerializeField]
    private float timeToDestroy;
    [SerializeField]
    private List<string> avoidTags;

    private void FixedUpdate()
    {
        //SI EL TIPO DE PROYECTIL ES FORWARD USAMOS EL FIXEDUPDATE
        //PARA MOVER SU RIGIDBODY DE FORMA CONSTANTE HACIA DELANTE
        if (moveMode == E_ATTACKMOVEMENT.FORWARD)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.AddForce(transform.forward * force);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //PROGRAMAMOS LA AUTODESTRUCCIÓN DEL PROYECTIOL
        Destroy(gameObject, timeToDestroy);
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        //SI EL MOVIMIENTO ES PARABÓLICO 
        //LO LANZAMOS EN UN ÁNGULO DE UNOS 60 GRADOS
        if(moveMode == E_ATTACKMOVEMENT.PARABOLLIC)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce((transform.forward + transform.up * 2).normalized * force, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter (Collider other)
    {
        //SI EL PROYECTIL CHOCA CON ALGÚN OBJETO
        //MIRAMOS SI TENEMOS QUE IGNORARLO POR SU TAG
        if (avoidTags.Contains(other.tag))
        {
            return;
        }
        //SI EL TIPO DE DAÑO ES DIRECTO, INFLINGIMOS DAÑO AL PERSONAJE
        if (damageMode == E_DAMAGEMODE.DIRECT)
        {
            controller = other.GetComponent<PlayerController>();
            if(controller != null)
            {
                controller.Damage(GetComponent<Damage>().damage);
                Destroy(gameObject);
            }
        } else if (damageMode == E_DAMAGEMODE.REACTION)
        {
           //SI EL TIPO DE DAÑO ES POR REACCIÓN
           //DESTRUIMOS EL COLLIDER ACTUAL Y GENERAMOS UNO MÁS GRANDE
           //Y CAMBIAMOS EL TIPO DE PROYECTIL A DIRECTO Y ACORTAMOS EL TIEMPO 
           //DE AUTODESTRUCCIÓN
            
            Destroy(col);
            SphereCollider newCol = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
            newCol.isTrigger = true;
            newCol.radius = damageReactionRadius;
            damageMode = E_DAMAGEMODE.DIRECT;
            Destroy(gameObject, 0.2f);
            //TODO: ACTIVAR SISTEMA DE PARTICULAS DE EXPLOSION
        }
    }
}
