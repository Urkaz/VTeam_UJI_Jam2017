﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePuzzleActivator : MonoBehaviour {
    public GameObject puzzle;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals("Player"))
        {
            puzzle.GetComponent<FloorTileParent>().setCanInteract(true);
        }
    }
}
