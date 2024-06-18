using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerControllerDRM : MonoBehaviour
{
    CharacterController controller;
    Animator anim;
    PlayerInput playerInput;
    InputAction move, run, look, crouch, jump, dash, shoot;
    bool crouched, grounded, dashing;
    Quaternion rotation;

    Vector3 mouseWorldPosition;
    [SerializeField]
    float speed, walkSpeed, runSpeed, rotationSpeed, zDistance;
    [SerializeField, Range(1, 1.5f)]
    float zDistanceFactor = 1.1f;
    public bool aimming;

    Vector3 inputMovement;
    Vector3 finalMovement;
    [SerializeField]
    Vector3 dragForce;
    float animSpeed;

    public float health;
    [SerializeField]
    float jumpHeight = 3, gravity = -9.8f, dashDistance;
    bool stunned = false;

    public float activeTime = 2f;
    bool isTrailActive;
    public float meshRefreshRate;
    SkinnedMeshRenderer[] skinnedMeshRenderers;
    public Material mat;
    public float meshDestroyDelay = 3f, shaderVarRate = .1f, shaderVarRefreshRate = .05f;
    public Transform spawnClones;
    public string shaderVarRef;

    Quaternion startRotation;

    //[SerializeField] LayerMask terrainLayerMask = new LayerMask();
    [SerializeField] Transform aimTransform, lastPosition, spawnBulletString;

    GameObject objPivot;

    //private const string EVENT_SHOOT = "ShootEvent";
    [SerializeField] GameObject bulletPrefab;


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
        shoot = playerInput.currentActionMap["Shoot"];
        //GUARDAR LA ROTACION INICIAL
        transform.rotation = startRotation;
    }
    private void OnEnable()
    {
        //ACTIVO EL MAPEADO DE CONTROLES
        playerInput.currentActionMap.Enable();
    }
    // Start is called before the first frame update
    void Start()
    {
        objPivot = new GameObject("DummyPivotRifle");
        objPivot.transform.parent = transform;
        speed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //Stunned

        if (stunned == true)
        {
            return;//PARO LA EJECUCIÓN DEL RESTO DE LA FUNCIÓN Y DEJA DE HACER EL UPDATE
        }

        //Crouch

        if (crouch.triggered)
        {
            crouched = !crouched; //si crouched es false !crouched = true en cambio si crouched es true !crouched = false
            anim.SetBool("crouch", crouched);
        }

        grounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);

        //Run
        //Falta hacer un lerp para que no sea tan repentino el cambio !!!
        if (run.IsPressed())
        {
            speed = runSpeed;
        }
        else
        {
            speed = walkSpeed;
        }

        //Movement

        inputMovement = new Vector3(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
        finalMovement = new Vector3(inputMovement.x, finalMovement.y, inputMovement.z);
        animSpeed = (speed / runSpeed) * inputMovement.magnitude;
        anim.SetFloat("speed", animSpeed);

        controller.Move(finalMovement * speed * Time.deltaTime);

        //Player animations

        if (aimming)
        {
            if (transform.rotation.y < 0.45f || transform.rotation.y < -0.45f) 
            {
                anim.SetFloat("DirectionX", inputMovement.x * transform.rotation.y * 10);
                anim.SetFloat("DirectionY", inputMovement.z * transform.rotation.y * 10);
            }else if (transform.rotation.y < 2.25f || transform.rotation.y < -2.25f)
            {
                anim.SetFloat("DirectionX", inputMovement.x * transform.rotation.y * -10);
                anim.SetFloat("DirectionY", inputMovement.z * transform.rotation.y * -10);
            }
        }else
        {
            anim.SetFloat("DirectionX", 0f);
            anim.SetFloat("DirectionY", 1f);
        }
       

        //Player rotation
        if (aimming)
        {
            zDistance = transform.position.z - Camera.main.transform.position.z;
            zDistance *= zDistanceFactor; // zDistance = zDistance * zDistanceFactor;
            mouseWorldPosition = new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, zDistance);
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseWorldPosition);
            mouseWorldPosition.y = transform.position.y;
            rotation = Quaternion.LookRotation(mouseWorldPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            if (inputMovement.magnitude != 0)
            {
                rotation = Quaternion.LookRotation(inputMovement);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);

            }
        }    

        //Jump

        if (jump.triggered && !crouched && grounded)
        {
            anim.SetTrigger("jump");
            finalMovement.y += Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        if (dash.triggered && !crouched && grounded)
        {
            StartCoroutine(ActivateTrail(activeTime));
            finalMovement += Vector3.Scale(inputMovement, dashDistance * new Vector3(Mathf.Log(1f / (Time.deltaTime * dragForce.x + 1)) / -Time.deltaTime, 0, Mathf.Log(1f / (Time.deltaTime * dragForce.z + 1)) / -Time.deltaTime));
        }
        finalMovement.x /= 1 + dragForce.x * Time.deltaTime;
        finalMovement.z /= 1 + dragForce.z * Time.deltaTime;

        //Dash

        if (dashing)
        {
            if (Mathf.Abs(finalMovement.x) <= 1 && Mathf.Abs(finalMovement.z) <= 1)
            {
                dashing = false;
                anim.SetBool("dash", false);
            }
        }

        //Landing

        if (grounded && finalMovement.y < 0)
        {
            finalMovement.y = -Mathf.Epsilon; //EPSILON = UN FLOAT CASI 0 PERO NO ES 0
        }
        else
        {
            //SI NO ESTOY TOCANDO EL SUELO HAGO QUE VAYA BAJANDO EN Y SEGÚN LA VELOCIDAD
            finalMovement.y += gravity * Time.deltaTime;
        }

        //Attack
        
        if (shoot.triggered && grounded)
        {
            EventManager.TriggerEvent("ShootEvent");

            anim.SetTrigger("shooting"); 
            spawnBulletString = RecursiveFindChild(transform, "SpawnBullet");
            Instantiate(bulletPrefab, spawnBulletString.position, transform.rotation);
        }

        Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform found = RecursiveFindChild(child, childName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }

        //bullet.get

        IEnumerator ActivateTrail(float timeActive)
        {
            while (timeActive > 0)
            {
                timeActive -= meshRefreshRate;
                if (skinnedMeshRenderers == null)
                {
                    skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                }
                for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                {
                    GameObject gObj = new GameObject();
                    gObj.transform.SetPositionAndRotation(spawnClones.position, spawnClones.rotation);

                    MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                    MeshFilter mf = gObj.AddComponent<MeshFilter>();

                    Mesh mesh = new();
                    skinnedMeshRenderers[i].BakeMesh(mesh);

                    mf.mesh = mesh;
                    mr.material = mat;

                    StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                    Destroy(gObj, meshDestroyDelay);
                }
                yield return new WaitForSeconds(meshRefreshRate);
            }
        }

        IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
        {
            float valueToAnimate = mat.GetFloat(shaderVarRef);

            while (valueToAnimate > goal)
            {
                valueToAnimate -= rate;
                mat.SetFloat(shaderVarRef, valueToAnimate);
                yield return new WaitForSeconds(refreshRate);
            }
        }
    }
    public bool IsCrouched()
    {
        return crouched;
    }
    public void Damage(float damage)
    {
        if (stunned == true)
        {
            return;
        }
        health -= damage;
        Debug.Log(health);
        anim.SetTrigger("damage");
        stunned = true;

    }
    public void StopDamage()
    {
        stunned = false;
    }
    
    public void Aim()
    {
        aimming = true;
    }

    public void StopAim()
    {
        aimming = false;
    }
}
