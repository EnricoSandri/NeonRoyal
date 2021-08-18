using System.Collections;
using System.Collections.Generic;


public class Sniper : Weapon
{
    public Sniper()
    {
        clipsize = 1;
        maxAmmunition = 10;
        reloadDuration = 2.0f;
        coolDownDuration = 0.5f;
        isAutomatic = false;
        name = "Sniper";
        aimVariation = 0.0f;
        damage = 50f;
    }
}
