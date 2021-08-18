using System.Collections;
using System.Collections.Generic;


public class Pistol : Weapon
{
    public Pistol()
    {
        clipsize = 12;
        maxAmmunition = 60;
        reloadDuration = 2.0f;
        coolDownDuration = 0.3f;
        isAutomatic = false;
        name = "Pistol";
        aimVariation = 0.01f;
        damage = 10f;
    }
}
