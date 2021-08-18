using System.Collections;
using System.Collections.Generic;

public abstract class Weapon 
{
    //Ammunition Fields.
    private int clipAmmuniton; 
    private int totalAmmunition;
    
    //Weapons setting.(customizable on weapon classes)
    protected int clipsize = 0;
    protected int maxAmmunition = 0;

    protected float reloadDuration = 0.0f;
    protected float coolDownDuration = 0.0f;
    protected bool isAutomatic = false;
    protected string name = "";
    protected float aimVariation = 0.0f;
    protected float damage = 0.0f;

    //private fields.
    private float reloadTimer = -1.0f;
    private float cooldownTimer = 0;
    private bool pressedTrigger = false;

    // set the "getters" and the nessacery "setters" for the weapons variables
    //Properties
    public int ClipAmmunition { get { return clipAmmuniton; } set { clipAmmuniton = value; } }
    public int TotalAmmuition { get { return totalAmmunition; } set { totalAmmunition = value; } }

    public int ClipSize { get { return clipsize; } }
    public int MaxAmmunition { get { return maxAmmunition; } }
    public float RealoadDuration { get { return reloadDuration; } }
    public float CoolDownDuration { get { return coolDownDuration; } }
    public bool IsAutomatic { get { return isAutomatic; } }
    public string Name { get { return name; } }
    public float AimVariation { get { return aimVariation; } }
    public float Damage { get { return damage; } }

    public float ReloadTimer { get { return reloadTimer; } }


    //Methods
    public void AddAmmunition(int amount)
    {
        totalAmmunition = System.Math.Min(totalAmmunition + amount, maxAmmunition);
    }


    public void LoadClip()
    {
        int maximumAmmunitionToLoad = ClipSize - clipAmmuniton;
        int ammunitionToLoad = System.Math.Min(maximumAmmunitionToLoad, totalAmmunition);

        clipAmmuniton += ammunitionToLoad;
        totalAmmunition -= ammunitionToLoad;
    }

    public bool Update(float deltaTime, bool isPressingTrigger)
    {
        bool hasShot = false;
        
        //Cooldown Logic
        cooldownTimer -= deltaTime;
        if (cooldownTimer <= 0)
        {
            bool canshoot = false;
            if (isAutomatic)
            {
                canshoot = isPressingTrigger;
            }
            else if (!pressedTrigger && isPressingTrigger)
            {
                canshoot = true;
            }

            if (canshoot && reloadTimer <= 0.0f)
            {
                cooldownTimer = coolDownDuration;
                //Only shoot if avelible bullets
                if(clipAmmuniton > 0)
                {
                    clipAmmuniton--;
                    hasShot = true;
                }
                if (clipAmmuniton == 0)
                {
                    //automatically reload the weapon
                    Reload();
                }
            }
            pressedTrigger = isPressingTrigger;
        }

        //reload logic

        if (reloadTimer > 0.0f)
        {
            reloadTimer -= deltaTime;
            if (reloadTimer <= 0.0f)
            {
                LoadClip();
            }
        }
        return hasShot;
    }

    public void Reload()
    {
        //Only reload if the weapom is not currently reloading and we have more bullets left

        if (reloadTimer <= 0.0f && clipAmmuniton < clipsize && totalAmmunition > 0)
        {
            reloadTimer = RealoadDuration;
        }
    }
















}
