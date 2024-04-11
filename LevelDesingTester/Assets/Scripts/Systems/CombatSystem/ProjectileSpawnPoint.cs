using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawnPoint : MonoBehaviour
{
    private void Start()
    {
        EventManager.StartListening("ShootEvent", SpawnBullet);
    }

    public void SpawnBullet()
    {
        Debug.Log("Pium");
    }
}
