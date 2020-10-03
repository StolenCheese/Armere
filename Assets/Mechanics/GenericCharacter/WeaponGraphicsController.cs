﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
public class WeaponGraphicsController : MonoBehaviour
{
    //Class to sort out locking objects to parts of the body
    [System.Serializable]
    public class HoldPoint
    {
        public HumanBodyBones anchor;
        public Vector3 posOffset;
        public Quaternion rotOffset;
        Transform anchorTrans;
        public void Init(Animator animator)
        {
            anchorTrans = animator.GetBoneTransform(anchor);
        }
        public void Anchor(Transform t)
        {
            t.SetPositionAndRotation(anchorTrans.TransformPoint(posOffset), anchorTrans.rotation * rotOffset);
        }

    }
    [System.Serializable]
    public class HoldableObject
    {
        public HoldPoint holdPoint;
        public HoldPoint sheathedPoint;
        public bool sheathed = true;
        [HideInInspector] public GameObject gameObject;

        public void Init(Animator a)
        {
            holdPoint.Init(a);
            sheathedPoint.Init(a);
        }

        public void Anchor()
        {
            if (gameObject != null)
                if (sheathed)
                    sheathedPoint.Anchor(gameObject.transform);
                else
                    holdPoint.Anchor(gameObject.transform);
        }

        public async void SetHeld(HoldableItemData holdable)
        {
            if (gameObject != null) Destroy(gameObject);
            gameObject = await holdable.CreatePlayerObject();
        }
        public void RemoveHeld()
        {
            if (gameObject != null)
                Addressables.ReleaseInstance(gameObject);
        }

    }

    public HoldableObject weapon;
    public HoldableObject bow;
    public HoldableObject sidearm;




    private void Start()
    {
        Animator a = GetComponent<Animator>();
        weapon.Init(a);
        bow.Init(a);
        sidearm.Init(a);
    }


    private void Update()
    {
        //Only lock objects to anchors if they exist
        weapon.Anchor();
        bow.Anchor();
        sidearm.Anchor();
    }
}
