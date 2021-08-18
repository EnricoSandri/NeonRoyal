using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTarget : TargetBaseClass
{
    [Header("Flying Properties")]
    [SerializeField] private float distanceFromFloor = 3;
    [SerializeField] private float bounceAmplitude;
    [SerializeField] private float bounceSpeed;
    [SerializeField] private float hoverSmoothness;


    [Header("Chasing Properties")]
    [SerializeField] private float chasingRange;
    [SerializeField] private float chasingSpeed;
    [SerializeField] private float chasingSmoothness;


    [Header("Attacking")]
    [SerializeField] private float attackingRange;
    [SerializeField] private float bounceBackSpeed;


    private float bounceAngle;
    private Player playerTarget;


    // Update is called once per frame
    void Update()
    {
        //Chase the player
        TargetChase();
    }

    private void FixedUpdate()
    {
        //make target fly
        TargetFly();
    }


    private void TargetFly()
    {
        
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            
            //get the ground position
            Vector3 targetPosition = hit.point;
            
            //move the enemy up acording to height below it
            targetPosition = new Vector3(targetPosition.x, targetPosition.y + distanceFromFloor, targetPosition.z);

            // swing the target
            bounceAngle += Time.deltaTime * bounceSpeed;
            float offset = Mathf.Cos(bounceAngle) * bounceAmplitude;
            targetPosition = new Vector3(targetPosition.x, targetPosition.y + offset, targetPosition.z);

            //"Attack" move towards the player position if any. 
            if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.transform.position) <= attackingRange)
            {
                targetPosition = new Vector3(targetPosition.x, playerTarget.transform.position.y + 1, targetPosition.z);
            }

            //apply the position
            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, targetPosition.x, Time.deltaTime * hoverSmoothness),
                Mathf.Lerp(transform.position.y, targetPosition.y, Time.deltaTime * hoverSmoothness),
                Mathf.Lerp(transform.position.z, targetPosition.z, Time.deltaTime * hoverSmoothness)
                );

        }
    }
    
    private void TargetChase()
    {
        Vector3 playerVelocity = Vector3.zero;

        //Find a Palyer Target
        if (playerTarget == null)
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, chasingRange / 2, Vector3.down);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.GetComponent<Player>() != null)
                {
                    playerTarget = hit.transform.GetComponent<Player>();
                    
                }

            } 
        }
        if (playerTarget !=null && Vector3.Distance(transform.position, playerTarget.transform.position) > chasingRange)
        {
            playerTarget = null;
        }

        //chase the playertarget if any
        if (playerTarget != null)
        {
            Vector3 direction =  (playerTarget.transform.position - transform.position).normalized;

            // remove the vertical componetn so the it moves above the head of the player
            direction = new Vector3(direction.x, 0, direction.z);
            direction.Normalize();

            // calculate palyer velocity
            playerVelocity = direction * chasingSpeed;

            //move the target
            targetRigidbody.velocity = new Vector3(
                Mathf.Lerp(targetRigidbody.velocity.x, playerVelocity.x, Time.deltaTime * chasingSmoothness),
                Mathf.Lerp(targetRigidbody.velocity.y, playerVelocity.y, Time.deltaTime * chasingSmoothness),
                Mathf.Lerp(targetRigidbody.velocity.z, playerVelocity.z, Time.deltaTime * chasingSmoothness)
                );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            collision.gameObject.GetComponent<IDamageable>().Damage(damage);

            Vector3 direction = (transform.position - collision.transform.position).normalized;
            targetRigidbody.velocity = direction * bounceBackSpeed;
        }
    }
}
