using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace Com.LakeWalk.MadGuns
{
    public class FireAndRecoil : MonoBehaviourPun
    {
        // Player's movement script
        private RigidbodyFirstPersonController player;

        // Player's Camera related
        public Camera cam;// The camera on the player's head
        public float normalFOV = 57f;// The feild of view of the camera

        // Object lists
        public List<Weapon> weapons;// Contains all the weapons that the player is holding
        public List<GameObject> gadgets;// Contains all the gadgets that the player is holding
        public List<GameObject> weaponGameObjects;

        // Buttons booleans
        public static bool startFire;// True when the fire button is pressed
        public static bool scope;// True when the scope button is pressed
        public static bool weaponSelected;// True when a weapon button is pressed in the weapon shop
        public static bool switchWeapon;
        public static bool reload;
        private bool pressedShoot;// The opposite of startFire
        private bool pressedScope = true;// The opposite of scope

        // Weapon related
        private float recoilSpeed = 0f;// Transitions between fastRecoil and slow Recoil
        private float fastRecoil = 5f;// The recoil when you fire a weapon
        private float slowRecoil = 2f;// When you stop shooting
        private Quaternion nextRecoil;// The rotation of the next recoil taken from the recoil array located in script "Weapon"
        private int i = 0;// The index of an array of a weapon which is responcible for its recoil pattern
        private int index = 0;// The index of the equiped weapon, 1st index is for main weapons, 2nd index is for pistols and the rest is for gadgets
        private int lastIndex;// The last equiped weapon
        public float weaponThrowForce;// The force that a weapon will be launched with when getting rid of
        private bool isReloading;// Is true when the player is reloading

        // Recoil
        private float scopeRecoilReduction;// When the player scopes the recoil is reduced (it is constant for now)
        private float recoilAdditions;// Can be used to reduce recoil or increase recoil deppending on the situation
        private bool delayAfterFired = false;// There is a certain delay with specific weapons when they are fired

        // Tags
        public LayerMask canBeShot;// Tags that can be shot by the player

        // For the player and damage
        public float range = 100f;// The range of the weapon (they all have the same range for now)

        // Object Poolling
        public GameObjectPool gameObjectPool;// Bullet decal
        public GameObjectPool BulletTracerPool;// Bullet Tracer

        // Hit Marker
        private bool targetHit;// When a target is hit
        private float timer;// The time after hitting an enemy
        public float timerTimeout;// The time in which after the crosshair will return to the default color

        // Weapon Holders
        public GameObject weaponHolder;// Holds every other holder
        public Transform weaponContainer;// Check if used
        public GameObject ARHolder;// When you buy a rifle it spawns in the ARHolder

        // Gadgets
        public GameObject gadgetHolder;// When you buy a gadget it spawns in the gadget holder
        public float launchForce;// The force that a gadget will be launched with

        // Animations
        public Animator weaponAnimator;// for weapon movement and hands(hands animations are for reload animation)

        // Audio
        public AudioManager audioManager;// Responcible for all gun sounds 

        // Photon
        private bool isMine;// If connected
        public static bool isOffline = false;// Determines whether the player needs to have an internet connection or not

        // Effects
        //public GameObject bulletTracer;
        public Transform shootPoint;// The point where the bulletTracers come from
        public ParticleSystem smokeParticles;
        public ParticleSystem muzzleParticles;

        // Checked by other clients
        public bool crossHairActivated = true;// Not used yet

        void Start()
        {
            isMine = photonView.IsMine;
            
            if (SceneManager.GetActiveScene().buildIndex == 2)// The buildIndex of the offline scene
            {
                isMine = true;
                isOffline = true;
            }
            if (isMine)
			{
                player = GetComponent<RigidbodyFirstPersonController>();
                weaponContainer.transform.SetParent(cam.transform);
                // Load the sensitivity of the scope
            }
            BulletTracerPool.spawnerPosition = shootPoint.localPosition;
        }

        void Update()
        {
            if (isMine)
            {
                //startFire = Fire.Pressed;
                //scope = ADS.aim;
                //weaponSelected = BuyWeapons.buttonPressed;

                
                if (scope == pressedScope)
                {
                    if (!delayAfterFired)
                    {
                        Scope(pressedScope);
                        pressedScope = !pressedScope;
                    }
                }
				
                if (startFire == pressedShoot)
                {
                    FireTrigger(pressedShoot);
                    pressedShoot = !pressedShoot;
                }

                if (weaponSelected)
                {
                    int index = BuyWeapons.instance.index;
                    if (isOffline)
                        ChangeWeapon(index);
                    else
                        photonView.RPC("ChangeWeapon", RpcTarget.AllViaServer, index);
                    
                    weaponSelected = false;
                }
                if (reload)
                {
                    ReloadRPC();
                    reload = false;
                    scope = false;
                    ADS.aim = false;
                    Debug.Log("Reload Weapon");
                }
                if (targetHit)
                {
                    timer += Time.deltaTime;
                    if (timer >= timerTimeout)
                    {
                        CanvasManager.instance.CrossHairHitColor(false);
                        targetHit = false;
                        timer = 0f;
                    }
                }
				if (switchWeapon)
				{
                    scope = false;
                    int tempIndex = WeaponSelect.weaponIndex;
                    if (tempIndex >= 0)
                    {
                        index = tempIndex;
                        weaponSwitched();
                    }
                    else
                    {
                        photonView.RPC("CallThrowWeapon", RpcTarget.All);
                    }
                    switchWeapon = false;
                }
            }
        }
        private void FixedUpdate()
        {
            if (!isMine)
                return;

            fire();
            //BulletForce();
            //if (startFire) //&& !isReloading)
            //{
            //    fire();
            //}
            // cam.transform.localRotation = Quaternion.Slerp(cam.transform.localRotation, nextRecoil, recoilSpeed * Time.deltaTime);
        }

        [PunRPC]
        private void ChangeWeapon(int index)
        {
            Object weapon = BuyWeapons.instance.GetWeapon(index);
            int weaponIndex;
            if (weapon as Weapon)// Checks if the weapon is a main weapon
            {
                weaponIndex = ((Weapon)weapon).weaponIndex;
                if (weapons[weaponIndex] != null)
                {
                    throwWeapon();
                    //photonView.RPC("CallThrowWeapon", RpcTarget.All);// Throws the old weapon
                }
                weapons[weaponIndex] = ((Weapon)weapon);// This should be an RPC
                                                        //Equip(weaponIndex);
                Equip(weaponIndex);
                //Debug.Log("weapon spawn: ");
            }
            else// If it is a gadget
            {
                GameObject gadget = ((GameObject)weapon);
                //Gadgets gadget = ((GameObject)weapon).GetComponent<Gadgets>();
                weaponIndex = gadget.GetComponent<Gadgets>().gadgetIndex;

                EquipGadget(weaponIndex, gadget);
            }
            // Will change to RPC
            weaponSelected = false;
        }

        private void fire()
        {
            Weapon weapon = ((Weapon)weapons[index]);
            bool canShoot = startFire && weapon.FireBullet() && !delayAfterFired && !isReloading;
            bool reachedTargetRecoil = nextRecoil == cam.transform.localRotation;
            if (reachedTargetRecoil && canShoot)// && delayEnded)
            {
                recoilSpeed = fastRecoil;
                if (!isOffline)
                {
                    photonView.RPC("Shoot", RpcTarget.All);
                }
                else
                    ShootOffline();

                nextRecoil *= Quaternion.Euler(weapon.recoil[i].y + recoilAdditions, weapon.recoil[i].x, 0f);
                if (weapon.isSniper)
                {
                    StartCoroutine(FireDelay(weapon.fireDelay));
                    //ADS.aim = false;// UnScopes if you are scoped in
                    i--;
                }
                i++;
                weapon.decreasBullets();
                CanvasManager.instance.bulletsInMagText.text = weapon.GetBulletsInMag().ToString();
            }
            else if (reachedTargetRecoil && !canShoot)
            {
                //weaponAnimator.SetBool(((Weapon)weapons[index]).shootAnimation, false);
                EndFire();
                //weaponAnimator.SetBool(weapon.shootAnimation, false);
                //Debug.Log("Ended");
            }
            cam.transform.localRotation = Quaternion.Slerp(cam.transform.localRotation, nextRecoil, recoilSpeed * Time.deltaTime);
        }

        [PunRPC]
        private void Shoot()
        {
            RaycastHit t_hit = new RaycastHit(); ;
            
            if (isMine)
            {
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out t_hit, range, canBeShot))
                {
                    int bulletDamage = ((Weapon)weapons[index]).damage;
                    float angle = 0f;
                    SpawnDecal(t_hit);
                    //shooting other player on network
                    if (t_hit.collider.gameObject.layer == 16)// Wallbange
                    {
                        bulletDamage /= 4;
                        Debug.Log("Wall bang");
                    }
                    
                    if (t_hit.collider.gameObject.layer == 11)
                    {
                        //t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[index].damage / 2);//, PhotonNetwork.LocalPlayer.ActorNumber);
                        Debug.Log("Hit");
                    }
                    // Head Shot
                    if (t_hit.collider.gameObject.layer == 14)
                    {
                        t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.AllViaServer, bulletDamage * 2, (int)angle, PhotonNetwork.LocalPlayer.ActorNumber);
                        //if (t_hit.collider.gameObject.GetComponent<CheckIfDead>().PlayerIsDead())
                        //{
                        //    StartCoroutine(HeadShotAnimation());// Calls the animation when you headshot someone
                        //}
                        Debug.Log("HeadShot");
                    }

                    // Body Shot
                    if (t_hit.collider.gameObject.layer == 15)
                    {
                        //Vector3 magnitude = Vector3.Normalize(this.transform.position - t_hit.collider.transform.position);
                        //angle = Mathf.Atan2(magnitude.z, magnitude.x) * Mathf.Rad2Deg;
                        t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.AllViaServer, bulletDamage / 2, (int)angle, PhotonNetwork.LocalPlayer.ActorNumber);
                        Debug.Log("Body Shot");
                    }

                    if(t_hit.collider.gameObject.layer == 17)// Wallbange
                    {
                        Destroy(t_hit.transform.gameObject);// Destroy the raycast
                        Debug.Log("Destroy Bullet");
                    }
                    

                    targetHit = true;
                    //Debug.Log("hit position " + t_hit.transform.position);
                    // Show hitmarker
                    CanvasManager.instance.CrossHairHitColor(true);// Changes the color of the crosshair to another color when hitting an enemy
                }
            }
            //gunShot.Play();
            //AudioManager.instance.Play(((Weapon)weapons[index]).fireSound);
            if (scope)
                weaponAnimator.Play("ADSFire", 0, 0f);
            else
            {
                weaponAnimator.Play("FireWeapon", 0, 0f);
                smokeParticles.Emit(1);
            }
            muzzleParticles.Emit(1);
            SpawnBulletTracer();
            audioManager.Play(((Weapon)weapons[index]).fireSound);
        }

        void ShootOffline()
        {
            RaycastHit t_hit = new RaycastHit();
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out t_hit, range, canBeShot))
            {
                Debug.Log("hit position " + t_hit.transform.position);
                BotScript botScript = t_hit.collider.transform.root.gameObject.GetComponentInParent<BotScript>();
                int bulletDamage = ((Weapon)weapons[index]).damage;
                //float angle = 0f;
                SpawnDecal(t_hit);
                //shooting other player on network
                if (t_hit.collider.gameObject.layer == 16)// Wallbange
                {
                    bulletDamage /= 4;
                    Debug.Log("Wall bang");
                }

                if (t_hit.collider.gameObject.layer == 11)
                {
                    //t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[index].damage / 2);//, PhotonNetwork.LocalPlayer.ActorNumber);
                    Debug.Log("Hit");
                }
                // Head Shot
                if (t_hit.collider.gameObject.layer == 14)
                {
                    botScript.TakeDamage(bulletDamage * 2);
                    Debug.Log("HeadShot");
                }

                // Body Shot
                if (t_hit.collider.gameObject.layer == 15)
                {
                    botScript.TakeDamage(bulletDamage / 2);
                    Debug.Log("Body Shot");
                }

                if (t_hit.collider.gameObject.layer == 17)// Wallbang
                {
                    Destroy(t_hit.transform.gameObject);// Destroy the raycast
                    Debug.Log("Destroy Bullet");
                }
                //Debug.Log("hit position " + t_hit.point);
                targetHit = true;
                // Show hitmarker
                CanvasManager.instance.CrossHairHitColor(true);// Changes the color of the crosshair to another color when hitting an enemy
            }
            //gunShot.Play();
            //AudioManager.instance.Play(((Weapon)weapons[index]).fireSound);
            if (scope)
                weaponAnimator.Play("ADSFire", 0, 0f);
            else
            {
                weaponAnimator.Play("FireWeapon", 0, 0f);
                smokeParticles.Emit(1);
            }
            muzzleParticles.Emit(1);
            SpawnBulletTracer();
            audioManager.Play(((Weapon)weapons[index]).fireSound);
        }

        private void EndFire()
        {
            nextRecoil = Quaternion.Euler(0f, 0f, 0f);
            recoilSpeed = slowRecoil;
        }

        private IEnumerator FireDelay(float delay)
        {
            Scope(false);
            Weapon weapon = ((Weapon)weapons[index]);
            //i = 0;
            //EndFire();
            delayAfterFired = true;
            weaponAnimator.SetBool(weapon.shootAnimation, false);
            yield return new WaitForSeconds(delay);
            delayAfterFired = false;
            pressedScope = true;
            if (weapon.FireBullet())
                weaponAnimator.SetBool(weapon.shootAnimation, true);
        }

        public void CallGadgetRPC()
        {
            photonView.RPC("throwGadget", RpcTarget.All);
        }

        [PunRPC]
        public void throwGadget()
        {
            GameObject gadget = gadgets[index-3];
            Rigidbody rb = gadget.GetComponent<Rigidbody>();// There should be an index instead of 0
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddForce(gadgetHolder.transform.right * launchForce, ForceMode.Impulse);
            gadget.GetComponent<SphereCollider>().enabled = true; 
            //gadgets[0].transform.localRotation = gadgetHolder.transform.rotation;
            gadget.transform.SetParent(null);
            weaponAnimator.SetTrigger("FireGadget");
        }
        [PunRPC]
        public void CallThrowWeapon()
        {
            throwWeapon();
        }

        public void throwWeapon()
        {
            GameObject weapon = weaponGameObjects[index];
            Rigidbody rb = weapon.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddForce(weaponHolder.transform.right * weaponThrowForce, ForceMode.Impulse);
            weapon.GetComponent<BoxCollider>().enabled = true;
            //gadgets[0].transform.localRotation = gadgetHolder.transform.rotation;
            weapon.transform.SetParent(null);
            weapons[index] = null;
        }

        [PunRPC]
        private void TakeDamage(int p_damage, int hitDirection, int p_actor)
        {
            //Debug.Log("Take Damage");
            GetComponent<RigidbodyFirstPersonController>().TakeDamage(p_damage, hitDirection, p_actor);
        }

        private void weaponSwitched()// Also you should add stop coroutine so that it stops reloading the previous weapon
        {
            if (index < 3)
            {
                if (weapons[index] != null)
			    {
                    Weapon weapon = ((Weapon)weapons[index]);

                    fastRecoil = weapon.recoilSpeed;
                    slowRecoil = weapon.slowRecoil;
                    scopeRecoilReduction = weapon.scopeRecoilReduction;
                    // if the index is 0 play the switch to knife animation
                    weaponAnimator.SetTrigger(weapon.equipAnimation);
                    weapon.Initialize();
                    // There should be a boolean that indicates whether those two lines should be used or not
                    if (isMine)
                    {
                        //CanvasManager.instance.EnableADS(weapon.ADSNumber);
                        CanvasManager.instance.crossHairContainer.SetActive(!weapons[index].isSniper);
                    }
                }
            }
            else
            {
                if (gadgets[index-3] != null)
                {
                    gadgets[lastIndex].SetActive(false);
                    gadgets[index-3].SetActive(true);
                    lastIndex = index - 3;
                    weaponAnimator.SetTrigger("SwitchToGadget");
                    if (isMine)
                    {
                        CanvasManager.instance.crossHairContainer.SetActive(true);
                    }
                }
            }
        }
        [PunRPC]
        private void Equip(int newIndex)
        {
            index = newIndex;
            //if (newIndex < 3)// The first 3 elements are for the weapons
            //{
            GameObject newWeapon = Instantiate(weapons[newIndex].prefab, ARHolder.transform.position, ARHolder.transform.rotation, ARHolder.transform) as GameObject;
            weaponGameObjects[index] = newWeapon;
                weaponSwitched();

            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localEulerAngles = Vector3.zero;
        }

        private void EquipGadget(int newIndex, GameObject gadget)
        {
            GameObject newWeapon = Instantiate(gadget, gadgetHolder.transform.position, gadgetHolder.transform.rotation, gadgetHolder.transform);
            weaponSwitched();// The boolean indicates that it is not a rifle it is a gadget
            gadgets[newIndex] = newWeapon;
        }

        public void FireTrigger(bool pressed)
        {
            if (pressed)
            {
                i = 0;
                nextRecoil = cam.transform.localRotation;
                //weaponAnimator.SetBool(((Weapon)weapons[index]).shootAnimation, true);
            }
            else
            {
                recoilSpeed = slowRecoil;
                //Debug.Log("shooting");
            }
        }

        private void SpawnDecal(RaycastHit hitInfo)
        {
            var decal = gameObjectPool.Get();
            decal.transform.position = hitInfo.point;
            decal.transform.forward = hitInfo.normal * -1f;
            decal.gameObject.SetActive(true);
            //Debug.Log("Decal");
        }

        private void SpawnBulletTracer()
        {
            var bulletTracer = BulletTracerPool.Get();
            bulletTracer.transform.position = shootPoint.position;
            bulletTracer.transform.rotation =  shootPoint.rotation;
            bulletTracer.gameObject.SetActive(true);
        }

        void Scope(bool pressed)
		{
            Weapon weapon = weapons[index];
			if (pressed)
			{
                cam.fieldOfView = weapon.scopeFOV;
                if (weapon.isSniper)
                {
                    CanvasManager.instance.EnableScope(true);
                    weaponGameObjects[index].SetActive(false);
                }
                else
                {
                    weaponAnimator.Play("SwitchToADS");
                    weaponAnimator.SetBool(weapon.ScopeAnimation, true);
                }
                    
                recoilAdditions = scopeRecoilReduction;
                AudioManager.instance.Play(weapon.scopeSound);
                FixedTouchField.scoped(true);
                
            }
			else
			{
                cam.fieldOfView = normalFOV;
                weaponAnimator.SetBool(weapon.ScopeAnimation, false);
                if (weapon.isSniper)
                {
                    CanvasManager.instance.EnableScope(false);
                    weaponGameObjects[index].SetActive(true);
                }
                player.movementSettings.scopeSpeedReduction = 0f;
                recoilAdditions = 0f;
                FixedTouchField.scoped(false);
            }
        }

		//[PunRPC]
		private void ReloadRPC()
		{
			StartCoroutine(Reload(((Weapon)weapons[index]).reloadTime));
            //weaponAnimator.SetTrigger("SwitchToAR");
		}

		IEnumerator Reload(float duration)
		{
            isReloading = true;
            Scope(false);
            delayAfterFired = true;
            yield return new WaitForSeconds(duration);
            delayAfterFired = false;
            Weapon weapon = ((Weapon)weapons[index]);
            weapon.Reload();
            CanvasManager.instance.bulletsInMagText.text = weapon.GetBulletsInMag().ToString();
            CanvasManager.instance.bulletsCarriedText.text = weapon.GetBulletsCarried().ToString();
			isReloading = false;
		}
	}
}