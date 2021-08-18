using System.Collections;
using System.Collections.Generic;

public class AssultRifle : Weapon
{
    public AssultRifle()
    {
        clipsize = 25;
        maxAmmunition = 260;
        reloadDuration = 3.2f;
        coolDownDuration = 0.083f;
        isAutomatic = true;
        name = "AssultRifle";
        aimVariation = 0.04f;
        damage = 10f;
    }
}
