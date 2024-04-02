using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;
    Animator anim;
    PlayerInput playerInput;
    InputAction move;
    InputAction run;
    InputAction look;
    InputAction crouch;
    InputAction jump;
    InputAction dash;
    InputAction melee;
    InputAction distance;

    [SerializeField]
    float maxHealth;
    float health;
    bool stunned;
    bool dashing;
    bool crouched;
    float speed;
    [SerializeField]
    float walkSpeed;
    [SerializeField]
    float runSpeed;

    [SerializeField]
    float dashDistance;
    [SerializeField]
    Vector3 dragForce;

    [SerializeField]
    float jumpHeight = 3; //ALTURA DE MI SALTO
    [SerializeField]
    float gravity = -9.8f;
    bool grounded;

    Vector3 inputMovement;
    Vector3 finalMovement;
    float animSpeed;
    Quaternion rotation;
    [SerializeField]
    float rotationSpeed;
    enum E_CONTROLLER {ORBITAL, THIRD_PERSON, TOP_DOWN};
    [SerializeField]
    E_CONTROLLER controlType;
    E_CONTROLLER oldControlType;
    [SerializeField]
    GameObject camOrbital;
    [SerializeField]
    GameObject camIsometric;
    Vector3 camForward;
    Vector3 camRight;
    Vector3 mouseWorldPosition;
    float zDistance;
    [SerializeField, Range(1, 1.5f)]
    float zDistanceFactor = 1.1f;

    [SerializeField]
    private GameObject attackMeleeWeapon;
    [SerializeField]
    private GameObject attackDistanceWeapon;
    [SerializeField]
    private Transform attackDistanceSpawnPos;
    [SerializeField]
    private GameObject attackDistanceBulletPrefab;


    private void ChangeControlType()
    {
        oldControlType = controlType;
        //DEPENDIENDO DEL TIPO DE CÁMARA ACTIVO LA VIRTUAL CÁMARA CORRESPONDIENTE
        if (controlType == E_CONTROLLER.ORBITAL)
        {
            camOrbital.SetActive(true);
            camIsometric.SetActive(false);
        }
        else
        {
            camOrbital.SetActive(false);
            camIsometric.SetActive(true);
        }
    }

    private void Awake()
    {
        //ASIGNO LAS REFERENCIAS A LOS COMPONENTES
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        //ASIGNO LAS REFERENCIAS A LOS INPUTACTION
        move = playerInput.currentActionMap["Move"];
        run = playerInput.currentActionMap["Run"];
        look = playerInput.currentActionMap["Look"];
        crouch = playerInput.currentActionMap["Crouch"];
        jump = playerInput.currentActionMap["Jump"];
        dash = playerInput.currentActionMap["Dash"];
        melee = playerInput.currentActionMap["Melee"];
        distance = playerInput.currentActionMap["Distance"];
    }

    private void OnEnable()
    {
        //ACTIVO EL MAPEADO DE CONTROLES
        playerInput.currentActionMap.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        //CUANDO INICIO EL JUEGO PONGO COMO VELOCIDAD LA VELOCIDAD DE ANDAR
        speed = walkSpeed;
        ChangeControlType();
    }

    // Update is called once per frame
    void Update()
    {
        //SI EL PLAYER ESTÁ ESTUNEADO
        if(stunned == true)
        {
            return;//PARO LA EJECUCIÓN DEL RESTO DE LA FUNCIÓN Y DEJA DE HACER EL UPDATE
        }
        //COMPRUEBO SI ME AGACHO O ME LEVANTO
        if (crouch.triggered)
        {
            crouched = !crouched; //si crouched es false !crouched = true en cambio si crouched es true !crouched = false
            anim.SetBool("crouch", crouched);
        }
        //&& -> AND 
        //|| -> OR
        //COMPRUEBO SI ATACO A MELEE Y NO ESTOY NI AGACHADO Y ESTOY EN EL SUELO
        if (melee.triggered && crouched==false /*!crouched*/ && grounded)
        {
            anim.SetTrigger("melee");
            //SI SIEMPRE TENGO EL ARMA VISIBLE COMENTO EL SETACTIVE
            attackMeleeWeapon.SetActive(true);
        }
        //COMPRUEBO SI ESTOY ATACANDO A DISTANCIA Y NO ESTOY NI AGACHADO Y ESTOY EN EL SUELO
        if (distance.triggered && crouched == false && grounded == true)
        {
            anim.SetTrigger("distance");
            //SI SIEMPRE TENGO EL ARMA VISIBLE COMENTO EL SETACTIVE
            attackDistanceWeapon.SetActive(true);
        }

        //COMPRUEBO SI ESTOY TOCANDO EL SUELO LANZANDO UN RAYO HACIA ABAJO Y MIRANDO SI COLISIONA CONTRA ALGO
        grounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
        anim.SetBool("grounded", grounded);
        
        //COMPRUEBO QUE NO SE HAYA CAMBIADO EL TIPO DE CONTROL
        if(controlType != oldControlType)
        {
            ChangeControlType();
        }
        //SI ESTOY PRESIONANDO EL BOTÓN DE ACCIÓN DE CORRER MODIFICO SU VELOCIDAD
        if (run.IsPressed())
        {
            speed = runSpeed;
        } else
        {
            speed = walkSpeed;
        }

        //ME QUEDO CON LOS VALORES DEL INPUT ACTION DE MOVE Y LOS JUNTO EN UN VECTOR PARA MOVERME SOBRE EL PLANO XZ
        inputMovement = new Vector3(move.ReadValue<Vector2>().x,0, move.ReadValue<Vector2>().y);
        //HACEMOS CAMBIOS DEL MOVIMIENTO PARA UN CONTROL ORBITAL
        if(controlType == E_CONTROLLER.ORBITAL)
        {
            //SI LA CÁMARA ES ORBITAL MODIFICO LA DIRECCIÓN EN FUNCIÓN DE LA DIRECCIÓN DONDE MIRA LA CÁMARA
            camForward = Camera.main.transform.TransformDirection(Vector3.forward);
            camRight = Camera.main.transform.TransformDirection(Vector3.right);
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            inputMovement = inputMovement.x * camRight + inputMovement.z * camForward;
        }
        
        //EL MOVIMIENTO FINAL EN EL EJE DE LAS Y MANTENDRÁ EL VALOR DEL FRAME ANTERIOR ANTES DE MODIFICARLO
        finalMovement = new Vector3(inputMovement.x, finalMovement.y, inputMovement.z);
        //CALCULO LA VELOCIDAD PARA EL ANIMATOR
        animSpeed = (speed / runSpeed) * inputMovement.magnitude;
        anim.SetFloat("speed", animSpeed);

        //SI QUIERO QUE EL PERSONAJE ROTE CON LA DIRECCIÓN EN LA QUE LO MUEVO
        if (inputMovement.magnitude != 0)
        {
            if (controlType != E_CONTROLLER.TOP_DOWN)
            {
                //ROTACIÓN PARA CONTROL ORBITAL Y THIRD_PERSON
                rotation = Quaternion.LookRotation(inputMovement);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation,
                    rotationSpeed * Time.deltaTime);
            }
        }
        //SI QUIERO QUE ROTE SEGÚN LA POSICIÓN DEL MOUSE
        if(controlType == E_CONTROLLER.TOP_DOWN)
        {
            zDistance = transform.position.z - Camera.main.transform.position.z;
            zDistance *= zDistanceFactor; // zDistance = zDistance * zDistanceFactor;
            mouseWorldPosition = new Vector3(Mouse.current.position.ReadValue().x,
                Mouse.current.position.ReadValue().y, zDistance);
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseWorldPosition);
            mouseWorldPosition.y = transform.position.y;
            rotation = Quaternion.LookRotation(mouseWorldPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 
                rotationSpeed * Time.deltaTime);
           
                Mouse.current.WarpCursorPosition(Mouse.current.position.ReadValue() +
                    look.ReadValue<Vector2>());
        }

        //SI APRIETO EL BOTÓN DE SALTO HAGO SALTAR AL PERSONAJE
        //&& OPERADOR LÓGICO AND(Y) SIGNIFICA QUE SE TIENEN QUE CUMPLIR LAS DOS CONDICIONES A LA VEZ
        if (jump.triggered && !crouched && grounded)    //if(jump.triggered == true && crouched == false && grounded== true)  
        {
            anim.SetTrigger("jump");
            finalMovement.y += Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        //SI APRIETO EL BOTÓN DE DASH Y NO ESTOY AGACHADO Y ESTOY EN EL SUELO
        if(dash.triggered && !crouched && grounded)
        {
            anim.SetBool("dash", true);
            dashing = true;
            //APLICO UN DESPLAZAMIENTO BRUSCO EN FUNCIÓN DEL DRAG Y LA DISTANCIA
            //VECTOR3.SCALE = MULTIPLICA DOS VECTORES
            //transform.forward ME DICE HACIA DONDE ESTÁ MIRANDO EL PERSONAJE
            //inputMovement ME DICE HACIA DONDE SE ESTÁ MOVIENDO EL PERSONAJE - ASÍ QUE SOLO USAREMOS EL DASH EN MOVIMIENTO

            finalMovement += Vector3.Scale(inputMovement,
                dashDistance * new Vector3(Mathf.Log(1f / (Time.deltaTime * dragForce.x + 1)) / -Time.deltaTime, 0,
                            Mathf.Log(1f / (Time.deltaTime * dragForce.z + 1)) / -Time.deltaTime));
        }
        //FRENO EL PERSONAJE SEGÚN EL DRAGFORCE
        finalMovement.x /= 1 + dragForce.x * Time.deltaTime; // finalMovement.x /= ---> finalMovement.x = finalMovement.x / 
        finalMovement.z /= 1 + dragForce.z * Time.deltaTime;
        if(dashing) //if(dashing==true)
        {
            if(Mathf.Abs(finalMovement.x) <=1 && Mathf.Abs(finalMovement.z) <=1)
            {
                dashing = false;
                anim.SetBool("dash", false);
            }
        }
        
        //SI TOCO EL SUELO Y AÚN ESTOY CAYENDO PONGO UN VALOR DE Y CASI IGUAL A 0
        if (grounded && finalMovement.y < 0)
        {
            finalMovement.y = -Mathf.Epsilon; //EPSILON = UN FLOAT CASI 0 PERO NO ES 0
        }
        else
        {
            //SI NO ESTOY TOCANDO EL SUELO HAGO QUE VAYA BAJANDO EN Y SEGÚN LA VELOCIDAD
            finalMovement.y += gravity * Time.deltaTime;
        }

        //DESPLAZO AL PERSONAJE TENIENDO EN CUENTA TODO LO ANTERIOR
        controller.Move(finalMovement * speed * Time.deltaTime);
    }

    public bool IsCrouched()
    {
        //SIRVE PARA COMPROBAR DESDE OTRO SCRIPT SI ESTOY AGACHADO
        return crouched;
    }

    //FUNCIÓN DE DAÑO
    public void Damage(float damage)
    {
        //RESTA VIDA Y EJECUTA LA ANIMACIÓN DE DAÑO
        if(stunned == true)
        {
            return;
        }
        health -= damage;
        Debug.Log(health);
        anim.SetTrigger("damage");
        stunned = true;

    }

    //CUANDO FINALIZA LA ANIMACIÓN DE DAÑO DEJO DE ESTAR ESTUNEADO Y ME PUEDO MOVER
    public void StopDamage()
    {
        stunned = false;
    }

    //CUANDO EMPIEZA LA ANIMACIÓN EFECTIVA DE ATAQUE ACTIVO EL COLLIDER DEL ARMA
    public void StartMeleeAttack()
    {
        Collider col = attackMeleeWeapon.GetComponent<Collider>();
        if(col!=null)
        {
            col.enabled = true;
        }
    }

    //CUANDO ACABA LA ANIMACIÓN EFECTIVA DE ATAQUE ACTIVO EL COLLIDER DEL ARMA
    public void StopMeleeAttack()
    {
        Collider col = attackMeleeWeapon.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        //SI SIEMPRE TENGO EL ARMA VISIBLE COMENTO EL SETACTIVE
        attackMeleeWeapon.SetActive(false);
    }

    //CUANDO EMPIEZA LA ANIMACIÓN EFECTIVA DE ATAQUE GENERO LOS PROYECTILES
    public void StartDistanceAttack()
    {
        Instantiate(attackDistanceBulletPrefab, attackDistanceSpawnPos.position,
            transform.rotation);
        //QUATERNION.IDENTITY, DEJA AL OBJETO CON SU ROTACIÓN ORIGINAL
    }

    //CUANDO ACABA LA ANIMACIÓN EFECTIVA DE ATAQUE DESACTIVO EL ARMA
    public void StopDistanceAttack()
    {
        //SI SIEMPRE TENGO EL ARMA VISIBLE COMENTO EL SETACTIVE
        attackDistanceWeapon.SetActive(false);
    }
}
