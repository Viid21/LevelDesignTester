using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour
{
    
    private PlayerController controller;

    
    private void OnTriggerEnter (Collider other)
    {
        //SI EL PROYECTIL CHOCA CON EL PLAYER, LE INFLINJIMOS DAÑO
        controller = other.GetComponent<PlayerController>();
        if(controller!=null)
        {
            controller.Damage(GetComponent<Damage>().damage);
        }
    }
}
