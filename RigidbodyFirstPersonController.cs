using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Com.LakeWalk.MadGuns
{
    [RequireComponent(typeof (Rigidbody))]
    [RequireComponent(typeof (CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviourPunCallbacks
    {
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;   // Speed when sprinting
	    public KeyCode RunKey = KeyCode.LeftShift;// Should by changed or removed
            public float JumpForce = 30f;
            public float scopeSpeedReduction;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

	    #if !MOBILE_INPUT
            private bool m_Running;
	    #endif
            
            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	    	if (input == Vector2.zero)
                {
                    return;
                }
                    
		if (input.x > 0.25f || input.x < 0.25f)
		{
			//strafe
			CurrentTargetSpeed = StrafeSpeed - scopeSpeedReduction;
		}
		if (input.y < 0)
		{
			//backwards
			CurrentTargetSpeed = BackwardSpeed - scopeSpeedReduction;
		}
		if (input.y > 0)
		{
			//forwards
			//handled last as if strafing and moving forward at the same time forwards speed should take precedence
			CurrentTargetSpeed = ForwardSpeed - scopeSpeedReduction;
                }
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this)
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public bool airControl; // can the user control the direction that is being moved in the air ?
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }

        private FireAndRecoil fireAndRecoil;
        public GameObject cam;
        public GameObject cameraContainer;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();

        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;//, canJump, startCounting;

        public static Vector2 moveInput;// Controled from the JoyStick class
        [HideInInspector]
        public Vector2 RunAxis;
        [HideInInspector]
        public bool JumpAxis;
	
        private Vector3 holdingPos = new Vector3(-1.48f, 0.15f, 1.33f);// Remove
        private Quaternion holdingRot = Quaternion.Euler(0f, -90f, 6.8f);// Remove

        // Player's health
        [SerializeField]
        private int max_health = 100;
        private int current_health;
        [SerializeField]
        private Manager manager;// Responsible for spawing the player and sending and recieving data between other players like amount of kills and deaths etc...
        [SerializeField]
        private Animator animator;

        public GameObject playerBody;
        public GameObject playerHands;
        public AudioSource[] footSteps;// 0 is for running 1 is for jumping 2 is for landing 

        private bool teamSelected;
        private bool landed;

        private Slider healthBar;// Represents player's HP
        private bool gotHit;// Is true when someone shoots you
        private float pointerTimer;// The timer of the hit pointer
        private float pointerLifeTime = 5f;
        private float hitDirection;// The angle from where you got shot from

        private bool isDead = false;// True when the player is dead

        [HideInInspector]
        public ProfileData playerProfile;

        private bool isMine;

        public static bool aiming = false;

        public GameObject headCollider;
        public GameObject bodyCollider;

        public GameObject HandsAndWeapons;// Holds the weapons and the hands and it is a child of the camera

        public Vector2 swayDistance;
        public float swaySpeed;
        public static float swayReduction = 1f;

        public bool myTeam = true;// True if the player is with the same team of that of the master client

        private bool isOffline;

        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
 		#if !MOBILE_INPUT
		    return movementSettings.Running;
		#else
	            return false;
		#endif
            }
        }

        private void Awake()
        {
            isMine = photonView.IsMine;
            current_health = max_health;
            fireAndRecoil = GetComponent<FireAndRecoil>();

            if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                isMine = isOffline = true;
                //PhotonNetwork.OfflineMode = true;
                Debug.Log("This is offline");
            }
            else
            {
                manager = GameObject.Find("Manager").GetComponent<Manager>();
                
            }
        }

        private void Start()
        {            
            if (isMine)
            {
                m_RigidBody = GetComponent<Rigidbody>();
                healthBar = CanvasManager.instance.healthBar;
                cam.SetActive(true);
                playerBody.SetActive(false);
                playerHands.SetActive(true);
                m_RigidBody.useGravity = true;
                isDead = false;
                CanvasManager.instance.inGameControls.SetActive(false);// When the player dies the controls are hidden thus he can't move
                mouseLook.Init (transform, cam.transform);
                if (!isOffline)
                {
                    photonView.RPC("ShareTeamIdentity", RpcTarget.OthersBuffered, manager.blueTeam);
                }
            }
            m_Capsule = GetComponent<CapsuleCollider>();
            animator = GetComponent<Animator>();
        }


        private void Update()
        {
            if (!isMine)
                return;

	    // Sets the animation of the player's character according to the player's velocity
            var localVelocity = Quaternion.Inverse(transform.rotation) * (m_RigidBody.velocity / movementSettings.CurrentTargetSpeed);
            animator.SetFloat("PosX", localVelocity.x);
            animator.SetFloat("PosY", localVelocity.z);

            // For input from canvas
            RunAxis = moveInput;
            JumpAxis = FixedButton.Pressed;
            mouseLook.LookAxis = FixedTouchField.TouchDist;

            RotateView();

            if (JumpAxis)
			{
                if(!m_Jump)
                {
                    m_Jump = true;
                }
                FixedButton.Pressed = false;
            }
        }

        private void FixedUpdate()
        {
            if (!isMine)
                return;

            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);

                    if (aiming)
                    {
                        movementSettings.scopeSpeedReduction = Mathf.SmoothStep(movementSettings.scopeSpeedReduction, 2f, 4.5f * Time.deltaTime);
                    }
                }
            }
            else
                movementSettings.scopeSpeedReduction = 0;

            if (gotHit)
            {
                pointerTimer += Time.deltaTime;
                GameObject hitPointer = CanvasManager.instance.hitPointer;
                if (pointerTimer >= pointerLifeTime)
                {
                    gotHit = false;
                    pointerTimer = 0f;
                    hitPointer.SetActive(false);
                }
            }

            HandsSway(RunAxis);
	    
            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;
                if (landed)
                {
                    footSteps[2].Play();
                    landed = false;
                }

                if (m_Jump)
                {
                    footSteps[1].Play();// Should be turned into an RPC Method()
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();

                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }
            m_Jump = false;
        }

        private void ChangeSwipeValue(Vector2 value)
        {
            mouseLook.LookAxis = value;
            Debug.Log("Swipping new input system");
        }

        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput()
        {
            
            Vector2 input = new Vector2
                {
                    x = RunAxis.x,
                    y = RunAxis.y
                };
			movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation (transform, cam.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation*m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                landed = true;
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }
        public int GetCurrentHealth()
        {
            return current_health;
        }

        private void DamageIndicator(float hitDirection)
        {
            GameObject hitPointer = CanvasManager.instance.hitPointer;
            gotHit = true;
            this.hitDirection = hitDirection;
            hitPointer.SetActive(true);
            hitPointer.transform.localRotation = Quaternion.Euler(0f, 0f, hitDirection + m_RigidBody.transform.eulerAngles.y);
            pointerTimer = 0f;
        }

        public void TrySync()
        {
            if (!isMine) return;
            // Enables other player to see your username and your level
            photonView.RPC("SyncProfile", RpcTarget.All, Launcher.myProfile.usernameInput, Launcher.myProfile.level, Launcher.myProfile.xp);
        }

        [PunRPC]
        private void SyncProfile(string p_username, int p_level, int p_xp)
        {
            playerProfile = new ProfileData(p_username, p_level, p_xp);
        }

        public void TakeDamage(int p_damage, int hitDirection, int p_actor)
        {
            if (isMine)
            {
                current_health -= p_damage;
                healthBar.value = current_health;// The health bar value decreases when getting shot
                DamageIndicator(hitDirection);

                if (current_health <= 0 && !isDead)
                {
                    Debug.Log("You Died");
                    manager.ChangeStat_S(p_actor, PhotonNetwork.LocalPlayer.ActorNumber);
                    // When the player dies he should see his body instead of his hands when the camera zooms out
                    playerBody.SetActive(true);
                    playerHands.SetActive(false);
                    StartCoroutine(DestroyAfter(3f));
                    isDead = true;
                    CanvasManager.instance.inGameControls.SetActive(true);// When the player dies the controls are hidden thus he can't move
                    photonView.RPC("Dead", RpcTarget.AllViaServer);
                }
            }
        }
        [PunRPC]
        private void Dead()
        {
            cameraContainer.transform.localPosition = new Vector3(0f, 1f, -1.84f);
            cameraContainer.transform.localRotation = Quaternion.Euler(58f, 0f, 0f);
            animator.Play("Dead");
            fireAndRecoil.throwWeapon();
            headCollider.layer = 0;
            bodyCollider.layer = 0;
        }

        private IEnumerator DestroyAfter(float time)
        {
            yield return new WaitForSeconds(time);
            manager.Spawn();// The player will respawn
            PhotonNetwork.Destroy(gameObject);// The player will get destroyed over the network 
        }
	
	// Lerps the parent containing the hands and weapons in the opposite direction of the player's movement
        private void HandsSway(Vector2 direction)
        {
            Vector3 swayDistance;
            swayDistance.x = (this.swayDistance.x * swayReduction) * (-Mathf.Round(direction.x));
            swayDistance.y = (this.swayDistance.y * swayReduction) * (-Mathf.Round(m_RigidBody.velocity.y));
            swayDistance.z = 0f;
            HandsAndWeapons.transform.localPosition = Vector3.Lerp(HandsAndWeapons.transform.localPosition, swayDistance, swaySpeed * Time.deltaTime);
        }
        [PunRPC]
        public void ShareTeamIdentity(bool isTeammate)// If you are with the same team of a certain client
        {
            if (manager.blueTeam == isTeammate)
            {
                headCollider.layer = 0;
                bodyCollider.layer = 0;
                myTeam = manager.blueTeam;
            }
        }
    }
}
