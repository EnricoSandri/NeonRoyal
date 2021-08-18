using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour, IDamageable
{
    
    [Header("Interactions")]
    [SerializeField] private KeyCode interactionKey;
    [SerializeField] private float InteractionDistance;

    [Header("Weapons")]
    [SerializeField] private GameObject shootOrigin;
    [SerializeField] private GameObject rocketPrefab;
   
    [Header("Player Weapons Reference")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject OnPlayerPistol;
    [SerializeField] private GameObject OnPlayerRifle;
    [SerializeField] private GameObject OnPlayerSniper;
    [SerializeField] private GameObject OnPlayerShotgun;
    [SerializeField] private GameObject OnPlayerRocketLauncher;

    [Header("Audio")]
    [SerializeField] private AudioSource soundChangeWeapon;
    [SerializeField] private AudioSource[] soundsWeapons;
    [SerializeField] private AudioSource[] soundsFootsteps;
    [SerializeField] private AudioSource soundsJump;
    [SerializeField] private AudioSource soundsHit;
    [SerializeField] private AudioSource soundPickup;

    [SerializeField] private float StepInterval;

    [Header("Drop Mode / Visuals")]
    [SerializeField] private GameObject playerContainer;
    [SerializeField] private GameObject playerDropVFX;
    [SerializeField] private float dropSpeed;
    [SerializeField] private float dropMovingSpeed;

    //[Header("DEBUG ")]
    //[SerializeField] private GameObject debugPositionPrefab;
    
    [Header("Camera Rotation Point")]
    [SerializeField] private GameObject rotationPoint;
    [SerializeField] private GameObject focalPoint;
    
    //Refs
    private GameCamera gameCamera;
    private HUDController hud;
    private List<Weapon> weapons;  
    private Weapon weapon;
    private PlayerHealth health;
    private float stepTimer;
    private float stormDamageTimer;
    private string currentWeapon;
    public Rigidbody playerRigidbody;
    private StormManager stormManager;
    private bool isIngame;
    
    //Animators
    Animator PlayerAnimator;
    NetworkAnimator networkAnimator;

 //--------GETTERS and SETTERS----------------------------------------------------------------------------------------------------------  
    // Block drop movement getter
    private bool allowDropMovement;
    public bool AllowDropMovement
    {
        get { return allowDropMovement; }
        set 
        { 
            allowDropMovement = value; 
            if(value == true)
            {
                hud.ShowScreen("spawn");
                Cursor.lockState = CursorLockMode.Locked;// lock the cursor when drop starts
            };
        }
    }


    //Drop state get and set
    private bool isInDropMode;
    public bool IsInDropMode 
    {
        get { return isInDropMode; }
        set 
        {
            isInDropMode = value;

            if (value == true)
            {
                playerRigidbody.useGravity = false;
                playerDropVFX.SetActive(true);
                playerContainer.transform.localScale = Vector3.zero;
            }
            else
            {
                playerRigidbody.useGravity = true;
                playerDropVFX.SetActive(false);
                playerContainer.transform.localScale = Vector3.one;

                if (hud != null)
                {
                    hud.ShowScreen("inGame");
                }
            }
        } 
    }

//-------------------------------------------------------------------------------------------------------------------------------------




    void Start()
    {
        isIngame = true;
        IsInDropMode = true;
        //Initialize values
        
        weapons = new List<Weapon>();
        health = GetComponent<PlayerHealth>();
        health.OnHealthChanged += OnHealthChanged;
        if (isServer)
        {
            stormManager = FindObjectOfType<StormManager>();
            stormManager.OnShrink += OnStormShrink;
        }    

        if (isLocalPlayer)
        {
            //Game Camera
            gameCamera = FindObjectOfType<GameCamera>();
            gameCamera.Target = focalPoint;
            gameCamera.RotationPoint = rotationPoint;

            // HUD elements
            hud = FindObjectOfType<HUDController>();
            if (isServer)
            {
                hud.ShowScreen("server");
            }
            else if (isClient)
            {
                hud.ShowScreen("client");
            }
            hud.Health = health.HealthValue;
            hud.UpdateWeapon(null);
            hud.OnStartMatch += OnServerStartMatch;

            //Listen to events
            GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>().OnFootstep += OnFootstep;
            GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>().OnJump += OnJump;

            //Get Top animator layer
            PlayerAnimator = GetComponent<Animator>();
            networkAnimator = GetComponent<NetworkAnimator>();

            // show no weapon at start
            CmdShowPlayerWeapon(gameObject, "");

        }

    }
    void OnStormShrink()
    {
        if(!isServer) return;
        foreach (Player player in FindObjectsOfType<Player>())
        {
            player.RpcAlertShrinkl();
            stormManager.StormHorn.Play();///////////////////////
        }
    }
    [ClientRpc]
    public void RpcAlertShrinkl()
    {
        if (!isLocalPlayer) return;

        hud.Alert();
        //stormManager.StormHorn.Play();
    }

    public void OnServerStartMatch()
    {
        if (!isServer) return;
        AllowDropMovement = true;
        stormManager.ShouldShrink = true;

        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player != this)
            {
                player.RpcAllowMovement();
            }
        }
    }
    [ClientRpc]
    public void RpcAllowMovement()
    {
        if (!isLocalPlayer) return;
        AllowDropMovement = true;
    }


    private void FixedUpdate()
    {
       if (!isLocalPlayer) return;

        if (IsInDropMode)
        {
            if (allowDropMovement)
            {
                float horizontalSpeed = Input.GetAxis("Horizontal") * dropMovingSpeed;
                float verticalSpeed = Input.GetAxis("Vertical") * dropMovingSpeed;

                Vector3 cameraForward = Vector3.Scale(gameCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 moveVector = (horizontalSpeed * gameCamera.transform.right) + (verticalSpeed * cameraForward);

                playerRigidbody.velocity = new Vector3(moveVector.x, dropSpeed, moveVector.z);
            }
            else
            {
                playerRigidbody.velocity = Vector3.zero;
            }
        }
        
    }


    private void Update()
    {
        if (!isLocalPlayer) return;
        stepTimer -= Time.deltaTime;
        stormDamageTimer -= Time.deltaTime;
        CheckNumberOfPlayers();
        DrawRaycastLine();
        SelectWeapon();
        UpdateWeapon();
        CheckIfDropMode();
        ExitGame();
    }

    private void ExitGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape)&&isIngame)
        {
            Cursor.lockState = CursorLockMode.None;
            hud.ShowScreen("exit");
            isIngame = false;
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && !isIngame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            hud.ShowScreen("inGame");
            isIngame = true;
        }
    }

    private void CheckNumberOfPlayers()
    {
        if (!allowDropMovement)
        {
            hud.Players = FindObjectsOfType<Player>().Length;
        }
    }

    private void CheckIfDropMode()
    {
        

        if (IsInDropMode)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 0.1f))
            {
               if(hitInfo.transform.GetComponent<Player>() == null)
               {
                    CmdDeactivateDropMode(gameObject);
                    IsInDropMode = false;
               }
            }

        }
    }

    private void DrawRaycastLine()
    {
        //interaction logic
#if UNITY_EDITOR
        //draw a line from the camera in the unity editor
        Debug.DrawLine(gameCamera.transform.position, gameCamera.transform.position + gameCamera.transform.forward * InteractionDistance, Color.green);
#endif
        
    }

    private void UpdateWeapon()
    {
        if (weapon != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                weapon.Reload();
            }
            float timeElapsed = Time.deltaTime;
            bool isPressingTrigger = Input.GetAxis("Fire1") > 0.1f;

            bool hasShot = weapon.Update(timeElapsed, isPressingTrigger);
            hud.UpdateWeapon(weapon);
            if (hasShot)
            {
                Shoot();
            }
            //Zoom logic.
            if(weapon is Sniper)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    gameCamera.TriggerZoom();
                    hud.SniperAimVisibility = gameCamera.IsZoomedIn;
                }
            }
        }
    }

    private void AnimateShoot()
    {
        PlayerAnimator.SetTrigger("Shoot");
        networkAnimator.SetTrigger("Shoot");

    }

    private void Shoot()
    {
        //check if weapon is shotgun, so it can shoot multiple bulltes at a time
        int amountOBullets = 1;
        if (weapon is Shotgun)
        {
            amountOBullets = ((Shotgun)weapon).AmountOfBullets;
        }

        //Sound 
        if(weapon is Pistol)
        {
            CmdPlayWeaponSound(gameObject, 0);
        }
        else if (weapon is AssultRifle)
        {
            CmdPlayWeaponSound(gameObject, 1);
        }
        else if (weapon is Sniper)
        {
            CmdPlayWeaponSound(gameObject, 2);
        }
        else if(weapon is Shotgun)
        {
            CmdPlayWeaponSound(gameObject, 3);
        }

        AnimateShoot();


        for (int i = 0; i < amountOBullets; i++)
        {

            RaycastHit TargetHit;
            if (Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out TargetHit))
            {
                Vector3 hitPosition = TargetHit.point;
            
                //Adding randomness to the shooting.
                Vector3 shootDirection = (hitPosition - shootOrigin.transform.position).normalized;
                shootDirection = new Vector3
                    (
                        shootDirection.x + Random.Range(-weapon.AimVariation, weapon.AimVariation),
                        shootDirection.y + Random.Range(-weapon.AimVariation, weapon.AimVariation),
                        shootDirection.z + Random.Range(-weapon.AimVariation, weapon.AimVariation)
                    );
                shootDirection.Normalize();
                //////////
            
                if(!(weapon is RocketLauncher))// for weaopns that do not use RigidBody
                {
                    RaycastHit shootHit;
                    if(Physics.Raycast(shootOrigin.transform.position, shootDirection, out shootHit))
                    {   
                        // DEBUG FOR WEAPON SHOOTING POINT//
                       
                        CmdAddBullet(shootHit.point);
                        //--------------------------//

                        if (shootHit.transform.GetComponent<IDamageable>() != null)
                        {
                            CmdDamage(shootHit.transform.gameObject, weapon.Damage);  
                        }
                        else if (shootHit.transform.GetComponentInParent<IDamageable>() != null)
                        {
                            CmdDamage(shootHit.transform.parent.gameObject, weapon.Damage);
                           
                        }
#if UNITY_EDITOR
                        //draw a line from the camera in the unity editor
                        Debug.DrawLine(shootOrigin.transform.position, shootOrigin.transform.position + shootDirection * 1000, Color.red);
#endif
                    }
                }
                else
                {
                    //RPG Projectile
                    CmdSpawnRocket(shootDirection);
                }
            }       
        }
    }
    [Command]
    private void CmdSpawnRocket(Vector3 shootDirection)
    {
        GameObject rocket = Instantiate(rocketPrefab);
        rocket.transform.position = shootOrigin.transform.position + shootDirection;
        rocket.GetComponent<Rocket>().Shoot(shootDirection);

        NetworkServer.Spawn(rocket);
    }

    [Command]
    private void CmdDamage(GameObject target, float damage)
    {
        if(target != null)
        {
            target.GetComponent<IDamageable>().Damage(damage);
        }
    }

    public int Damage(float amount)
    {
        CmdHit(gameObject);
        GetComponent<PlayerHealth>().Damage(amount);
        return 0;
    }

    public void StormDamage()
    {
        if (!isLocalPlayer) return;
        if (stormDamageTimer <= 0)
        {
            stormDamageTimer = 1;
            CmdDamage(gameObject, 2);
        }
    }
    private void OnHealthChanged(float newHealth)
    {
        if (!isLocalPlayer) return;
        hud.Health = newHealth;
        if (newHealth < 0.01f)
        {
            Cursor.lockState = CursorLockMode.None;
            hud.ShowScreen("gameOver");
            CmdDestroy();
        } 
    }
    [Command]
    private void CmdDestroy()
    {
        Destroy(gameObject);
    }

    //Select weapons
    public void SelectWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchWeapon(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchWeapon(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SwitchWeapon(4);
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            weapon = null;
            hud.UpdateWeapon(weapon);
            gameCamera.ZoomOut();
            hud.SniperAimVisibility = false;
            CmdShowPlayerWeapon(gameObject, "");
            AnimateUnequip();
        }
    }

    private void AnimateUnequip()
    {
        PlayerAnimator.SetTrigger("HoldNothing");
        networkAnimator.SetTrigger("HoldNothing");
    }

    private void AnimateWeaponHold(String weaponName)
    {
        PlayerAnimator.SetTrigger("Hold " + weaponName);
        networkAnimator.SetTrigger("Hold " + weaponName);
    }

   

    private void  SwitchWeapon(int index)
    {
        
        if (index < weapons.Count)
        {
            soundChangeWeapon.Play();

            weapon = weapons[index];
            hud.UpdateWeapon(weapon);
            
            //send info to top animator layer to its corrispondent weapon animation----------------
            if (weapon is Pistol)
            {
                AnimateWeaponHold("Pistol");
            }
            else if (weapon is RocketLauncher)
            {
                AnimateWeaponHold("Rocket");
            }
            else if (weapon is AssultRifle || weapon is Sniper || weapon is Shotgun)
            {
                AnimateWeaponHold("Rifle");
            }
            ////-----------------------------------------------------------------------

            //Activate the right weapon on the player model---------------------------------
            if (weapon is Pistol)
            {
                CmdShowPlayerWeapon(gameObject,"Pistol");
            }
            else if (weapon is AssultRifle)
            {
                CmdShowPlayerWeapon(gameObject, "Rifle");
            }
           
            else if (weapon is Sniper)
            {
                CmdShowPlayerWeapon(gameObject, "Sniper");
            }
            else if(weapon is Shotgun)
            {
                CmdShowPlayerWeapon(gameObject, "Shotgun");
            }
            else if (weapon is RocketLauncher)
            {
                CmdShowPlayerWeapon(gameObject, "Rocket");
            }
            //----------------------------------------------------------------------------------

            if (!(weapon is Sniper))
            {
                gameCamera.ZoomOut();
                hud.SniperAimVisibility = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.gameObject.GetComponent<ItemBox>() != null)
        {
            ItemBox itemBox = other.gameObject.GetComponent<ItemBox>();

            Giveitem(itemBox.Type, itemBox.Amount);

            CmdCollectBox(other.gameObject);
        }
    }

    [Command]
    private void CmdCollectBox(GameObject box)
    {
        Destroy(box);
    }

    private void Giveitem (ItemBox.ItemType type, int amount)
    {
        CmdPickup(gameObject);
        //create a weapon ref
        Weapon Currentweapon = null;

        //cherch if we have already an instance of this weapon
        for (int i = 0; i < weapons.Count; i++)
        {
            if(type == ItemBox.ItemType.Pistol && weapons[i] is Pistol)
            {
                Currentweapon = weapons[i];
            }
            else if (type == ItemBox.ItemType.AssultRifle && weapons[i] is AssultRifle)
            {
                Currentweapon = weapons[i];
            }
            else if (type == ItemBox.ItemType.Shotgun && weapons[i] is Shotgun)
            {
                Currentweapon = weapons[i];
            }
            else if (type == ItemBox.ItemType.Sniper && weapons[i] is Sniper)
            {
                Currentweapon = weapons[i];
            }
            else if (type == ItemBox.ItemType.RocketLauncher && weapons[i] is RocketLauncher)
            {
                Currentweapon = weapons[i];
            }
        }

        // if we don't have a weapon of its type, create one and add it to the weapons list 
        if (Currentweapon == null)
        {
            if(type == ItemBox.ItemType.Pistol)
            {
                Currentweapon = new Pistol();
            }
            else if (type == ItemBox.ItemType.AssultRifle)
            {
                Currentweapon = new AssultRifle();
            }
            else if (type == ItemBox.ItemType.Shotgun)
            {
                Currentweapon = new Shotgun();
            }
            else if (type == ItemBox.ItemType.Sniper)
            {
                Currentweapon = new Sniper();
            }
            else if (type == ItemBox.ItemType.RocketLauncher)
            {
                Currentweapon = new RocketLauncher();
            }
            weapons.Add(Currentweapon);
        }
            
        Currentweapon.AddAmmunition(amount);
        Currentweapon.LoadClip();
            
        if(Currentweapon == weapon)
        {
            hud.UpdateWeapon(weapon);
        }
    }
    
//Networking ----------------------------------------------------------------------------------------------------------------------
    //WEAPONS Sound
    [Command]
    void CmdPlayWeaponSound(GameObject caller, int index) // server recives the info and sends it to all the clients.
    {
        if (!isServer) return;

        RpcPlayWeaponSound(caller, index);
    }

    [ClientRpc]
    void RpcPlayWeaponSound(GameObject caller, int index) // this will be called on every client 
    {
        caller.GetComponent<Player>().PlayWeaponSound(index);
    }

    public void PlayWeaponSound(int index)
    {
        soundsWeapons[index].Play();
    }
    //FOOTSTEP SOUNDS
    [Command]
    void CmdPlayFootstepSound(GameObject caller)// server recives the info and sends it to all the clients.
    {
        if (!isServer) return;

        RpcPlayFootstepSound(caller);
    }

    [ClientRpc]
    void RpcPlayFootstepSound(GameObject caller)
    {
        caller.GetComponent<Player>().PlayFootstepSound();
    }
    
    void PlayFootstepSound()
    {
        soundsFootsteps[Random.Range(0, soundsFootsteps.Length)].Play();
    }

    //this is emitted in the 3rd person
    void OnFootstep(float forwardAmount)
    {
        if (forwardAmount > 0.6f && stepTimer <= 0)
        {
            stepTimer = StepInterval;
            CmdPlayFootstepSound(gameObject);
        }
    }

    //JUMP SOUND
    void OnJump() //sends info to server
    {
        CmdJump(gameObject);
    }

    [Command]
    void CmdJump(GameObject caller) // server processes info and sends to all other players
    {
        if (!isServer) return;
        RpcJump(caller);
    }

    [ClientRpc]
    void RpcJump(GameObject caller) // all clients will get the caller and play the sound
    {
        caller.GetComponent<Player>().PlayJumpSound();
    }

    private void PlayJumpSound() //playes the sound
    {
        soundsJump.Play();
    }

    //HitSound

    [Command]
    void CmdHit(GameObject caller) 
    {
        if (!isServer) return;

        RpcHit(caller);
    }

    [ClientRpc]
    void RpcHit(GameObject caller)
    {
        if (!isLocalPlayer) return;
        caller.GetComponent<Player>().PlayHitSound();
    }

    void PlayHitSound()
    {
        
        soundsHit.Play();
    }

    //PickupSound

    [Command]
    void CmdPickup(GameObject caller)
    {
        if (!isServer) return;

        RpcPickup(caller);
    }

    [ClientRpc]
    void RpcPickup(GameObject caller)
    {
        caller.GetComponent<Player>().PlayPickupSound();
    }

    void PlayPickupSound()
    {
        soundPickup.Play();
    }

    //Weapon networked----------------------------------------------------------------------------------------
    //Sync the  intial players weapons with all players when they join
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        CmdRefreshWeapons();
    }

    [Command]
    void CmdRefreshWeapons()
    {
        if (!isServer) return; 

        foreach (Player player in FindObjectsOfType<Player>()) // get the current weapon fdrom each player 
        {
            player.RpcRefreshWeapons();
        }
    }

    [ClientRpc]
    public void RpcRefreshWeapons()// spread the info to the newly spawned player
    {
        if (!isLocalPlayer) return;//////////////////////////////////////////////////////////////////////////
        CmdShowPlayerWeapon(gameObject, currentWeapon);
    }

    //Show right weapon through the network----------------------------------------------------------------------------------------
    [Command]
    void CmdShowPlayerWeapon(GameObject caller, String weaponName)
    {
        if (!isServer) return;

        RpcShowModel(caller, weaponName);
    }

    [ClientRpc]
    void RpcShowModel(GameObject caller, String weaponName)
    {
        caller.GetComponent<Player>().ShowPlayerWeapon(weaponName);

    }

    public void ShowPlayerWeapon(String weaponName)   // sets active the model on the player 
    {
        currentWeapon = weaponName;

        OnPlayerPistol.SetActive(weaponName == "Pistol");
        OnPlayerSniper.SetActive(weaponName == "Sniper");
        OnPlayerRifle.SetActive(weaponName == "Rifle");
        OnPlayerShotgun.SetActive(weaponName == "Shotgun");
        OnPlayerRocketLauncher.SetActive(weaponName == "Rocket");
    }




    //De-activate Drop mode-----------------------------------------------------------------

    [Command]
    void CmdDeactivateDropMode(GameObject caller)
    {
        RpcDeactivateDropMode(caller);
    }

    [ClientRpc]
    void RpcDeactivateDropMode(GameObject caller)
    {
        DeactivateDropMode(caller);
    }
    
    void DeactivateDropMode(GameObject caller)
    {
     
        caller.GetComponent<Player>().IsInDropMode = false;
      
    }
    [Command]
    void CmdAddBullet(Vector3 position)
    {
        GameObject bulletInstance = Instantiate(bulletPrefab);
        bulletInstance.transform.position = position;
        NetworkServer.Spawn(bulletInstance);
        
        Destroy(bulletInstance, .5f);;
    }

}

