﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinController: MonoBehaviour {

    GameObject currentCollidedObject;
    Inventory inv;
    Rigidbody rb;
    PuzzleManager pipeManager;
	
	public int speed;
    public float rotSpeed;
    public int maxSpeed;
    private State currentState = State.ALIVE;
	
	public float lookSpeed = 10;
    private Vector3 curLoc;
    private Vector3 prevLoc;

    Vector3 lookAtThat;

    Animator anim;

    public float animSpeed;

    private GameManager gm;

    private GameObject currentDoorInteracted;

    public enum State
    {
        ALIVE,
        DEAD,
    };

	// Use this for initialization
	void Start () {
        anim = gameObject.GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        if (gm != null)
            inv = gm.gameObject.GetComponent<Inventory>();

        GameObject[] arraySpawns = GameObject.FindGameObjectsWithTag("Spawn");

        if(gm != null) {
            foreach (GameObject spawn in arraySpawns)
            {
                if (spawn.GetComponent<SpawnInfo>().SpawnIndex == gm.getSpawnTarget())
                {
                    transform.position = spawn.transform.position + Vector3.up * gameObject.GetComponent<CapsuleCollider>().bounds.extents.y;
                    break;
                }
            }
        }
	}
	

	// Update is called once per frame
	void Update () {
        if(!currentState.Equals(State.DEAD))
        {
            if (Input.GetAxis("PickUp")!=0)
            {
                PickUp();
            }
        }
        if (Input.GetAxis("ResetRoom") != 0 && currentState != State.DEAD)
        {
            Dead();
        }

        if ( currentDoorInteracted != null && !currentDoorInteracted.gameObject.GetComponent<AudioSource>().isPlaying)
        {
            if (gm != null)
            {
                int[] a = currentDoorInteracted.GetComponent<SwitchScene>().GetTargetDoor(); //0 = nivel , 1 = puerta
                gm.setSpawnTarget(a[1]);
                gm.LoadLevel((GameManager.Levels)a[0]);
                currentDoorInteracted = null;
            }
        }
    }

    private void FixedUpdate()
    {
        if(currentState.Equals(State.DEAD))
        {
            anim.SetFloat("VSpeed", 0.0f);
        }
        else
        {
            rb.velocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed;
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }

            if(Input.GetAxis("Horizontal")!= 0 && Input.GetAxis("Vertical") ==0){
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, Mathf.Sign(Input.GetAxis("Horizontal")) * 90, 0), Time.deltaTime * rotSpeed);
            }
            if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") != 0)
            {
                if (Input.GetAxis("Vertical") > 0)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0,0,0), Time.deltaTime * rotSpeed);
                }
                else if (Input.GetAxis("Vertical") < 0)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 180, 0), Time.deltaTime * rotSpeed);
                }
            }
            if(Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") != 0)
            {
                if (Input.GetAxis("Horizontal") >= 0 && Input.GetAxis("Vertical") >= 0)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 45, 0), Time.deltaTime * rotSpeed);
                }
                if (Input.GetAxis("Horizontal") <= 0 && Input.GetAxis("Vertical") <= 0)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 225, 0), Time.deltaTime * rotSpeed);
                }
                if (Input.GetAxis("Horizontal") >= 0 && Input.GetAxis("Vertical") <= 0)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 135, 0), Time.deltaTime * rotSpeed);
                }
                if (Input.GetAxis("Horizontal") <= 0 && Input.GetAxis("Vertical") >= 0)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 315, 0), Time.deltaTime * rotSpeed);
                }
            }
            if((Mathf.Abs(Input.GetAxis("Vertical"))) > 0)
            {
                animSpeed = Mathf.Abs(Input.GetAxis("Vertical"));
            }
            else if ((Mathf.Abs(Input.GetAxis("Horizontal"))) > 0)
            {
                animSpeed = Mathf.Abs(Input.GetAxis("Horizontal"));
            }
            anim.SetFloat("VSpeed", animSpeed);
        }
    }
	
    private void OnTriggerEnter (Collider collision)
    {
        if(collision.gameObject.tag.Equals("Pickable"))
        {
            currentCollidedObject = collision.gameObject;
        }
        else if (collision.gameObject.tag.Equals("Door"))
        {
            EnterDoor(collision.gameObject);
        }
    }

    private void OnTriggerExit (Collider collision)
    {
        if (currentCollidedObject == collision.gameObject)
            currentCollidedObject = null;
    }

    void PickUp()
    {
        if(currentCollidedObject != null)
        {
            if(inv != null) {
                if (!inv.Contains(currentCollidedObject.GetComponent<PickUpType>().type) )
                {
                    if((int) currentCollidedObject.GetComponent<PickUpType>().type >= (int) Inventory.Items.Pipe1 )
                    {
                        if( !(inv.Contains(Inventory.Items.Pipe1) || inv.Contains(Inventory.Items.Pipe2) || inv.Contains(Inventory.Items.Pipe3)))
                        {
                            inv.addObject((currentCollidedObject.GetComponent<PickUpType>().type));
                            Destroy(currentCollidedObject);
                            currentCollidedObject = null;
                        }
                    }
                    else { 
                        inv.addObject((currentCollidedObject.GetComponent<PickUpType>().type));
                        Destroy(currentCollidedObject);
                        currentCollidedObject = null;
                    }
                }
            }
        }
    }
    
    void EnterDoor(GameObject door)
    {
        if(door.GetComponent<SwitchScene>().neededItem.Equals(Inventory.Items.NONE) || inv.findObject(door.GetComponent<SwitchScene>().neededItem))
        {
            if (!door.GetComponent<SwitchScene>().neededItem.Equals(Inventory.Items.NONE))
                inv.removeObject(door.GetComponent<SwitchScene>().neededItem);

            //int[] a = door.GetComponent<SwitchScene>().GetTargetDoor(); //0 = nivel , 1 = puerta

            currentDoorInteracted = door;
            door.gameObject.GetComponent<AudioSource>().Play();
            
            /*if (gm != null)
            {

                gm.setSpawnTarget(a[1]);
                gm.LoadLevel((GameManager.Levels)a[0]);
            }*/
        }
        

    }

    public void Dead()
    {
        if(!currentState.Equals(State.DEAD))
        {
            gm.GetComponent<Inventory>().ResetInventory();
            currentState = State.DEAD;
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Fadeout_>().fade = true;
        }
    }
}
