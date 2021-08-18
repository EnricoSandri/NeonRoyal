using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Rocket : NetworkBehaviour
{
    [Header("Rocket Properties")]
    [SerializeField] private float speed;
    [SerializeField] private float lifetime;
    [SerializeField] private float explosionRange;
    [SerializeField] private float ExplosionDamage;
    [SerializeField] private GameObject explosionPrefab;


    private Rigidbody rocketRigidbody;
    private float timer;


    // Start is called before the first frame update
    void Awake()
    {
        rocketRigidbody = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifetime)
        {
            //Explode
            Explode();
        }      
    }


    public void Shoot(Vector3 direction)
    {
        transform.forward = direction;
        rocketRigidbody.velocity = direction * speed;
    }


    public void OnTriggerEnter(Collider otherCollider)
    {
        Explode();
       
    }

    private void Explode()
    {
        if (isServer)
        {
            GameObject explosion = Instantiate(explosionPrefab);
            explosion.transform.position = transform.position;
            NetworkServer.Spawn(explosion);

            explosion.GetComponent<Explosion>().Explode(explosionRange, ExplosionDamage);
            Destroy(gameObject);
        }
    }

}
