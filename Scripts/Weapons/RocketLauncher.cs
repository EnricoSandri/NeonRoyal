using System.Collections;
using System.Collections.Generic;


public class RocketLauncher : Weapon
{
    public RocketLauncher()
    {
        clipsize = 1;
        maxAmmunition = 4;
        reloadDuration = 3.0f;
        coolDownDuration = 0.5f;
        isAutomatic = false;
        name = "RPG";
        aimVariation = 0.01f;
    }
}
