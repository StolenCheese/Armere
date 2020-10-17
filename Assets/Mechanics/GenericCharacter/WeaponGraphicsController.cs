﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;


public class EquipmentSet<T>
{
    public T melee;
    public T sidearm;
    public T bow;

    public EquipmentSet(T melee, T sidearm, T bow)
    {
        this.melee = melee;
        this.sidearm = sidearm;
        this.bow = bow;
    }

    public T this[ItemType t]
    {
        get
        {
            switch (t)
            {
                case ItemType.Melee: return melee;
                case ItemType.SideArm: return sidearm;
                case ItemType.Bow: return bow;
                default: throw new System.Exception("No such type in set");
            }
        }
        set
        {
            switch (t)
            {
                case ItemType.Melee: melee = value; return;
                case ItemType.SideArm: sidearm = value; return;
                case ItemType.Bow: bow = value; return;
                default: throw new System.Exception("No such type in set");
            }
        }
    }
}


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
        HoldableItemData holdable;
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
            this.holdable = holdable;
            if (gameObject != null) Destroy(gameObject);
            gameObject = await holdable.CreatePlayerObject();
        }
        public void RemoveHeld()
        {
            if (gameObject != null)
                Addressables.ReleaseInstance(gameObject);
        }

        public void OnClank(AudioSource source)
        {
            if (holdable != null && holdable.clankSet != null && holdable.clankSet.Valid())
            {
                if (Random.Range(0f, 1f) > holdable.clankProbability)
                {
                    source.PlayOneShot(holdable.clankSet.SelectClip());
                }
            }
        }
    }



    public AudioSource source;
    public HoldableObject weapon;
    public HoldableObject bow;
    public HoldableObject sidearm;

    public EquipmentSet<HoldableObject> holdables;


    Animator animator;

    AnimationController animationController;

    public IEnumerator DrawItem(ItemType type, AnimationTransitionSet transitionSet)
    {
        if (type == ItemType.Melee)
        {
            animationController.TriggerTransition(transitionSet.drawSword);
            animator.SetBool("Holding Sword", true);
        }
        animationController.TriggerTransition(transitionSet.swordWalking);

        yield return new WaitForSeconds(0.1f);
        holdables[type].sheathed = false;
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator SheathItem(ItemType type, AnimationTransitionSet transitionSet)
    {
        if (type == ItemType.Melee)
        {
            animationController.TriggerTransition(transitionSet.sheathSword);
            animator.SetBool("Holding Sword", false);
        }

        animationController.TriggerTransition(transitionSet.freeMovement);

        yield return new WaitForSeconds(0.2f);

        holdables[type].sheathed = true;
    }


    private void Start()
    {
        animator = GetComponent<Animator>();
        animationController = GetComponent<AnimationController>();
        weapon.Init(animator);
        bow.Init(animator);
        sidearm.Init(animator);

        holdables = new EquipmentSet<HoldableObject>(weapon, sidearm, bow);
    }

    public void OnClank()
    {
        //Called by animator
        weapon.OnClank(source);
        bow.OnClank(source);
        sidearm.OnClank(source);
    }
    public void FootDown()
    {
        OnClank();
    }


    private void Update()
    {
        //Only lock objects to anchors if they exist
        weapon.Anchor();
        bow.Anchor();
        sidearm.Anchor();
    }
}
