using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    //ESTADOS DEL PERSONAJE
    enum E_STATES { WAIT, PATROL, SEEK, ATTACK, DAMAGED, DEAD }
    [SerializeField]
    private E_STATES actualState;
    //SISTEMA DE PRATULLAJE
    enum E_PATROL { WAYPOINTS, RANDOM, STATIC }
    [SerializeField]
    private E_PATROL patrolMode;
    //SISTEMA DE DETECCIÓN
    enum E_SEEK { HEAR, SEE, STATIC }
    [SerializeField]
    private E_SEEK seekMode;
    //SISTEMA DE ATAQUE
    enum E_ATTACK { DISTANCE, MELEE }
    [SerializeField]
    private E_ATTACK attackMode;

    private Animator anim;
    private Rigidbody rb;
    private Collider mainCollider;
    private NavMeshAgent agent;
    //VARIABLES DE DESPLAZAMIENTO
    [SerializeField]
    private float timeToWait = 2.0f;
    [SerializeField]
    private float walkSpeed = 3.5f;
    [SerializeField]
    private float runSpeed = 8.0f;
    [SerializeField]
    private int idleIndexAnim = 3;

    //VARIABLES DE PATRULLAJE
    [SerializeField]
    private Transform[] waypoints;    //[] = ARRAY -> ALGO PARECIDO A UNA LISTA
    private int actualWpIndex = 0; //LOS ARRAYS TIENEN INDICES (EL ORDEN EN LA LISTA) Y SE EMPIEZA POR 0
    private float wpDistance = Mathf.Infinity;
    private Vector3 nextPatrolPoint;
    [SerializeField]
    private float randomPatrolRange = 100.0f;
    //VARIABLES DE DETECCIÓN DEL JUGADOR
    [SerializeField]
    private LayerMask playerLayer;
    private Collider[] playerColliders;
    private Vector3 playerDirection;
    private float playerDistance;
    [SerializeField]
    private float seeRadius = 0.5f;
    [SerializeField]
    private float seekRange = 8f;
    private bool mustSeek = false;
    private RaycastHit hit;
    //VARIABLES DE ATAQUE
    [SerializeField]
    private float attackRange = 3f;
    private bool mustAttack = false;
    [SerializeField]
    private float attackWaitTime = 1f;
    [SerializeField]
    private GameObject attackWeapon;
    [SerializeField]
    private GameObject attackDistancePrefab;
    [SerializeField]
    private int shoots = 8;
    private float step;
    [SerializeField]
    private Transform attackDistanceSocket;
    //VARIABLES DE VIDA
    [SerializeField]
    private float maxHealth = 5;
    private float health;
    //VARIABLES DEL RAGDOLL
    private List<Rigidbody> ragdollRbs = new List<Rigidbody>();
    //private Rigibody[] ragdollRbs;
    private List<Collider> ragdollColliders = new List<Collider>();


    private void Awake()
    {
        //AL INICIAR ME QUEDO CON TODOS LOS COMPONENTES NECESARIOS
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
        agent = GetComponent<NavMeshAgent>();
        //PONGO EL ESTADO Y LA VIDA POR DEFECTO
        actualState = E_STATES.WAIT;
        health = maxHealth;
        //INICIO EL RAGDOLL
        InitRagdoll();
    }

    void InitRagdoll()
    {
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        //ME QUEDO CON TODOS LOS COLLIDERS QUE TENGA EL OBJETO Y SUS HIJOS
        foreach(Rigidbody r in rbs) //PARA CADAUNO DE LOS COLLIDERS DEL OBJETO
        {
            if(r==rb) //SI ES EL COLLIDER PRINCIPAL
            {
                continue; // ME SALTO ESTE LOOP Y VOY AL SIGUIENTE
            }
            //SI NO ES EL COLLIDER PRINCIPAL
            r.isKinematic = true; //LO HAGO KINEMATICO PARA QUE NO TENGA FÍSICAS
            ragdollRbs.Add(r);//LO AÑADO A LA LISTA DE COLLIDERS DEL RAGDOLL
            Collider c = r.GetComponent<Collider>(); //BUSCO EL COLLIDER QUE ESTÁ
            //EN EL MISMO OBJETO QUE EL RIGIDBODY QUE ESTOY MIRANDO
            c.isTrigger = true; //HAGO EL COLLIDER TRIGGER
            c.enabled = false; // Y LO DESACTIVO PARA QUE NO INTERACTUE CON NADA
            ragdollColliders.Add(c); //AÑADO EL COLLIDER A  LA  LISTA DE COLLIDERS DEL RAGDOLL

        }
    }

    [ContextMenu("Ragdoll")]
    void EnableRagdoll()
    {
        //CUANDO SE ACTIVA EL RAGDOLL
        //DESACTIVO EL ANIMATOR PARA QUE NO HAGA LA ANIMACIÓN
        anim.enabled = false;
        //DESACTIVO EL RIGIDBODY Y EL COLLIDER PARA QUE NO LE AFECTEN LAS FÍSICAS
        rb.isKinematic = true;
        mainCollider.isTrigger = true;
        mainCollider.enabled = false;
        //HAGO QUE TODOS LOS RIGIDBODIES DEL RAGDOLL SE VEAN AFECTADOS POR LA FÍSICA 
        foreach (Rigidbody r in ragdollRbs)
        {
            r.isKinematic = false;
        }
        //ACTIVO TODOS LOS COLLIDERS DEL RAGDOLL
        foreach (Collider c in ragdollColliders)
        {
            c.isTrigger = false;
            c.enabled = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Wait");
    }

    // Update is called once per frame
    void Update()
    {
        //DIBUJO UNA ESFERA Y MIRO TODOS LOS OBJETOS DE LAS CAPAS INDICADAS QUE QUEDAN DENTRO
        playerColliders = Physics.OverlapSphere(transform.position, seekRange, playerLayer);
        //PARA SABER SI HE DETECTO AL PLAYER, PLAYERCOLLIDERS TIENE QUE TENER UNA LONGITUD MAYOR QUE 0, ES DECIR, HAY ALGÚN ELEMENTO
        if (playerColliders.Length > 0 && actualState != E_STATES.DAMAGED)
        {
            //MIRO SI PUEDO PERSEGUIR AL JUGADOR
            mustSeek = CheckSeekPlayer(out mustAttack);
            if (mustSeek == true && actualState != E_STATES.SEEK)
            {
                StopAllCoroutines();//DETIENE CUALQUIER CORUTINA QUE ESTÉ EN CURSO (SI ESTOY ESPERANDO O PATRULLANDO Y TENGO QUE PERSEGUIR, DEJO LO QUE ESTABA HACIENDO)
                StartCoroutine("Seek");
            }
            //LA FUNCIÓN DE CHECKSEEKPLAYER TAMBIÉN DEVUELVE SI DEBO ATACAR
            //SI DEBO ATACAR Y NO ESTOY ATACANDO PASO A ESTADO DE ATAQUE
            if (mustAttack == true && actualState != E_STATES.ATTACK)
            {
                StopAllCoroutines();
                StartCoroutine("Attack");
            }
        } else
        {
            //SI NO DETECTO AL PLAYER DENTRO DEL RADIO LE DIGO QUE DEBE DEJAR DE PERSEGUIRLO
            mustSeek = false;
            mustAttack = false;
        }
    }

    //FUNCIÓN QUE COMPRUEBA SI TENGO QUE PERSEGUIR AL PLAYER
    private bool CheckSeekPlayer(out bool attack)
    {
        //COMPRUEBO LA DIRECCIÓN Y LA DISTANCIA A LA QUE ESTÁ EL PLAYER
        playerDirection = playerColliders[0].transform.position - transform.position;
        playerDistance = playerDirection.magnitude;
        attack = false;
        //DEVOLVERÁ TRUE SI TIENE QUE PERSEGUIR AL JUGADOR O FALSE EN CASO CONTRARIO
        if (seekMode == E_SEEK.HEAR)
        {
            //SI LO HE OIDO Y LA ESTOY EN DISTANCIA DE ATAQUE, DEJO DE PERSEGUIR Y DEBO ATACAR
            if (playerDistance <= attackRange)
            {
                attack = true;
                return false;
            }
            //SI NO ESTOY EN DISTANCIA DE ATAQUE PERSIGO
            return true; //SI ENTRO EN EL CHECK ES PORQUE YA HAY UN PLAYER EN RANGO
            //SI HAGO UN RETURN, SE ANULA EL RESTO DE CÓDIGO DE LA FUNCIÓN Y SE SALE FUERA
        }
        else if (seekMode == E_SEEK.SEE)
        {

            Physics.SphereCast(transform.position, seeRadius, playerDirection.normalized, out hit, playerDistance);
            if (hit.collider != null)//SI LAS ESFERAS HAN CHOCADO CONTRA ALGÚN COLLIDER
            {
                //COMPRUEBO QUE EL COLLIDER TENGA LAS LAYERS DE PLAYERLAYER
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    if (playerDistance <= attackRange)
                    {
                        attack = true;
                        return false;
                    }
                    return true;
                }
            }
        } else if (seekMode == E_SEEK.STATIC) //SI ES ESTÁTICO COMPRUEBO SI ESTÁ EN RANGO DE ATAQUE
        {
            if (playerDistance <= attackRange)
            {
                attack = true;
                return false;
            }
        }
        return false;
    }

    //CORUTINA -> SON UN TIPO DE FUNCIÓN QUE SE EJECUTA AL RITMO DEL UPDATE Y QUE PODEMOS PARAR DURANTE UN TIEMPO
    private IEnumerator Wait()
    {
        //CUANDO ENTRO EN ESTADO DE ESPERA, DETENGO EL NAVMESHAGENT
        actualState = E_STATES.WAIT;
        agent.speed = 0;
        agent.isStopped = true;
        //PONGO UNA ANIMACIÓN DE IDLE ALEATORIA
        anim.SetInteger("idleIndex", Random.Range(0, idleIndexAnim));
        anim.SetFloat("speed", 0);
        wpDistance = Mathf.Infinity;
        //HAY QUE INDICAR LA CONDICIÓN QUE MIRA CUANTO SE REPITE(SI ES NECESARIO)


        //Y SIEMPRE DEVUELVEN EL VALOR DE ESPERA
        //yield retun null; --> DEVUELVE UNA ESPERA (yield) NULA (nula), ES DECIR NO SE ESPERARÍA
        yield return new WaitForSeconds(timeToWait);
        StartCoroutine("Patrol");
    }

    private IEnumerator Patrol()
    {
        //CUANDO ENTRO EN ESTADO DE PATRULLAJE, ACTIVO EL NAVMESHAGENT Y LE DOY VELOCIDAD DE ANDAR
        actualState = E_STATES.PATROL;
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.enabled = true;
        anim.SetFloat("speed", agent.speed / runSpeed);
        //SI PATRULLA POR WAYPOINTS ME QUEDO CON EL PRIMER DESTINO
        if (patrolMode == E_PATROL.WAYPOINTS)
        {
            nextPatrolPoint = waypoints[actualWpIndex].position;
        } else if (patrolMode == E_PATROL.RANDOM)
        {
            //SI PATRULLO RANDOM BUSCO UN PUNTO DE DESTINO
            nextPatrolPoint = Random.insideUnitCircle * randomPatrolRange;
            nextPatrolPoint = new Vector3(nextPatrolPoint.x, transform.position.y, nextPatrolPoint.y);
        }
        agent.SetDestination(nextPatrolPoint);
        //COMPRUEBO SI ME HE ACERCADO AL PUNTO DE DESTINO
        while (wpDistance > 1.0f)
        {
            wpDistance = Vector3.Distance(nextPatrolPoint, transform.position);
            if (patrolMode == E_PATROL.STATIC)
            {
                wpDistance = 0;
            }
            yield return null;
        }
        //SI PATRULLO POR WAYPOINTS Y HE LLEGADO A MI DESTINO BUSCO EL SIGUIENTE PUTNO
        if (patrolMode == E_PATROL.WAYPOINTS)
        {
            actualWpIndex++; //actualWpIndex = actualWpIndex +1; ++ -> incrementar en 1
            if (actualWpIndex >= waypoints.Length)
            {
                actualWpIndex = 0;
            }
        }
        //ESPERO 
        StartCoroutine("Wait");
    }

    private IEnumerator Seek()
    {
        //CUANDO ENTRO EN ESTADO DE ESPERA, ACTIVO EL NAVMESHAGENT Y LE PONGO VELOCIDAD DE CORRER
        actualState = E_STATES.SEEK;
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.enabled = true;
        anim.SetFloat("speed", agent.speed / runSpeed);
        //MIENTRAS TENGA QUE PERSEGUIR ACTUALIZO EL PUNTO DE DESTINO DEL NAVMESHAGENT A LA POSICIÓN DEL PLAYER
        while (mustSeek == true)
        {
            agent.SetDestination(playerColliders[0].transform.position);
            yield return null;
        }
        //SI TENGO QUE DEJAR DE PERSEGUIR PERO TAMPOCO TENGO QUE ATACAR ENTRO EN MODO DE ESPERA
        if (mustAttack == false)
        {
            StartCoroutine("Wait");
        }
    }

    private IEnumerator Attack()
    {
        //CUANDO ENTRO EN ESTADO DE ATAQUE, DETENGO EL NAVMESHAGENT
        actualState = E_STATES.ATTACK;

        agent.isStopped = true;
        agent.speed = 0;
        agent.enabled = false;
        anim.SetFloat("speed", 0);
        //MIENTRAS TENGA QUE ATACAR
        while (mustAttack == true)
        {
            //REORIENTO AL ENEMIGO PARA QUE ENCARE AL PLAYER
            transform.rotation = Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z));
            //MUESTRO EL ARMA
            if (attackMode == E_ATTACK.DISTANCE)
            {
                anim.SetTrigger("distance");
            } else if (attackMode == E_ATTACK.MELEE)
            {
                //EJECUTO ANIMACIÓN ATAQUE
                anim.SetTrigger("melee");
            }
            //DESPUÉS DE CADA ATAQUE ESPERO
            yield return new WaitForSeconds(attackWaitTime);
        }
        agent.enabled = true;
    }

    
   //SI EL ENEMIGO RECIBE DAÑO DEL PLAYER 
   //RESTAMOS DAÑO Y LO PONEMOS EN ESTADO DE DAÑO
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag("PlayerAttack") && actualState != E_STATES.DAMAGED)
        {
            health -= other.GetComponent<Damage>().damage; //TODO: ESTO HAY QUE CAMBIARLO POR EL DAÑO DEL BATE, NO LO DEJAREMOS ASÍ
            Debug.Log("healthZombie:" + health);
            actualState = E_STATES.DAMAGED;
            anim.SetTrigger("damage");
            StopAllCoroutines();
            


        }
    }

    #region ANIMATION_EVENTS
    //CUANDO LLEGO AL FRAME DE ATAQUE EFECTIVO DE LA ANIMACIÓN DE ATAQUE A DISTANCIA
    public void StartAttackDistance()
    {
        //INSTANCIO TANTOS PROYECTILES COMO HAGA FALTA
        step = 360.0f / shoots;
        for (int i = 0; i < shoots; i++)
        {
            Instantiate(attackDistancePrefab, attackDistanceSocket.transform.position,
                transform.rotation * Quaternion.Euler(0, step * i, 0));
        }
    }

    public void StopAttackDistance()
    {

    }

    public void StartAttackMelee()
    {
        //CUANDO LLEGO AL FRAME DE ATAQUE EFECTIVO DE LA ANIMACIÓN DE ATAQUE A MELEE, ACTIVO EL ARMA
        attackWeapon.SetActive(true);
    }

    public void StopAttackMelee()
    {
        //CUANDO LLEGO AL FRAME DE FIN DE ATAQUE EFECTIVO DE LA ANIMACIÓN DE ATAQUE A MELEE, ACTIVO EL ARMA
        attackWeapon.SetActive(false);
    }
    public void StopStun() 
    {
        //CUANDO LLEGO AL FRAME DE FIN EFECTIVO DE STUN ACTIVO EL 
        actualState = E_STATES.SEEK;
    }
    #endregion  ANIMATION_EVENTS
}
