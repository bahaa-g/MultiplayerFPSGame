using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.LakeWalk.MadGuns
{
    public class Gadgets : MonoBehaviour
    {

        public int gadgetIndex;
        protected int duration;// How long the gadget is going to last

        // After colliding with the ground
        public int damage;// The damage that the gadget can do to the player
        protected bool collidedWithGround;// If the gadget collided with the ground
        public float detectionRate;// How many times the gadget is going to scan its surroundings
        protected float detectionTimer;
        public float explosionLifeTime;// The lifetime of the gadget after it detonates
        public float explosionRadius;// The range of the explosion of the gadget
        protected bool callAbility;// Determines whether the player should be dragged or not

        protected GameObject gadget;// The gameobject of the gadget
        public GameObject player;// The player
        public GameObject gadgetHolder;// The parent object of the gadget
        public RigidbodyFirstPersonController PlayerScript;// The script of the player
        public GameObject effect;

        private void Start()
        {
            //player = Manager.clone;
            PlayerScript = player.GetComponent<RigidbodyFirstPersonController>();
            Debug.Log("Gadget Start");
        }

        private void FixedUpdate()
        {
            if (collidedWithGround)
            {
                detectionTimer += Time.deltaTime;
                if (detectionTimer >= detectionRate)
                {
                    if (DetectSurroundings())
                    {
                        Debug.Log("Player detected");
                        callAbility = true;
                    }
                    else
                        callAbility = false;
                    detectionTimer = 0f;
                }
                if (callAbility)
                {
                    Ability();
                }
            }
        }

        protected virtual void Ability()// The stuff that the gadget will do
        {
            // This method is overridden in the derived class
        }

        protected bool DetectSurroundings()
        {
            //return (Vector3.Distance(Manager.clone.transform.position, this.transform.position) <= explosionRadius);
            return false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ground")// If the gadget collides with the ground
            {
                Rigidbody rb = this.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;
                //player = Manager.clone;
                PlayerScript = player.GetComponent<RigidbodyFirstPersonController>();
                StartCoroutine(ExplosionLiftime(explosionLifeTime));
                effect.SetActive(true);
                rb.useGravity = true;
                SphereCollider collider = this.GetComponent<SphereCollider>();
                collider.enabled = false;
                Debug.Log("Gadget collided");
            }
        }

        protected IEnumerator ExplosionLiftime(float explosionLifeTime)
        {
            collidedWithGround = true;
            yield return new WaitForSeconds(explosionLifeTime);
            collidedWithGround = false;
            //RetreiveGadget();
        }

        protected void RetreiveGadget()
        {
            this.gameObject.SetActive(false);
            this.transform.localPosition = gadgetHolder.transform.position;
        }
    }
}