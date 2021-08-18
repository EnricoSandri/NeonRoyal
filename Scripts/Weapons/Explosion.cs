using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Explosion : NetworkBehaviour
{
    public void Explode(float range, float damage)
    {
        transform.GetChild(0).localScale = Vector3.one * range * 2;
       
        if (isServer)
        {

            RaycastHit[] hits =  Physics.SphereCastAll(transform.position, range, transform.up);
            foreach (RaycastHit hit in hits)
            {
                Debug.Log(hit.transform.name);
                if(hit.transform.GetComponent<IDamageable>() != null)
                {
                    hit.transform.GetComponent<IDamageable>().Damage(damage);
                }
            }
            Destroy(gameObject,3.75f);
        }
       
        
    }
}
