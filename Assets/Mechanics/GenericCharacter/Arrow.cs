﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter)),
RequireComponent(typeof(MeshRenderer)),
RequireComponent(typeof(CapsuleCollider)),
RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    bool initialized = false;
    ItemName ammoName;
    Rigidbody rb;
    public void Initialize(ItemName ammoName, Vector3 position, Vector3 velocity, ItemDatabase db)
    {
        //Calibrate components
        this.ammoName = ammoName;
        if (db[this.ammoName].type != ItemType.Ammo)
        {
            //Make sure the item fired is actually ammo
            Debug.LogError("Non - Ammo type fired as arrow");
            return;
        }


        transform.position = position;

        // GetComponent<MeshFilter>().sharedMesh = db[ammoName].mesh;
        // GetComponent<MeshRenderer>().materials = db[ammoName].materials;

        rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
        transform.forward = velocity;

        var capsule = GetComponent<CapsuleCollider>();
        capsule.direction = 2; //0 = x, 1 = y, 2 = z
        capsule.radius = 0.05f;

        capsule.height = 0.8f;
        capsule.center = Vector3.forward * capsule.height * 0.5f;

        //Set initialzed
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
        {
            Debug.LogError("Arrow not initialized by update");
            return;
        }
        transform.forward = rb.velocity;
    }
    private void OnCollisionEnter(Collision other)
    {
        print("Arrow hit");
        if (other.gameObject.TryGetComponent<Health>(out var h))
        {
            h.Damage(10, gameObject);
        }
        //Turn arrow into an item if it is permitted
        Items.SpawnItem(ammoName, transform.position, transform.rotation, InventoryController.singleton.db);
        Destroy(gameObject);
    }
}
