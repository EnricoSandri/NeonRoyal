using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerHealth : NetworkBehaviour
{
    // Event
    public delegate void HealthChangedHandler(float health);
    public event HealthChangedHandler OnHealthChanged;
    
    //Health
    private const float defalutHealth = 100f;
    
    [SyncVar(hook = "OnHealthSynced")]
    private float health = defalutHealth;
    //properties
    public float HealthValue { get { return health; } }

     
    public void Damage (float amount)
    {
        health -= amount;

        if(health < 0)
        {
            health = 0;
        }

    }
    private void OnHealthSynced(float newHealth)
    {
        if (OnHealthChanged != null)
        {
            OnHealthChanged(newHealth);
        }
    }
}
