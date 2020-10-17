﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[RequireComponent(typeof(Health), typeof(WeaponGraphicsController), typeof(Ragdoller))]
public class EnemyAI : AIBase
{
    public enum EnemyBehaviour { Guard, Patrol, }
    public enum SightMode { View, Range }


    public EnemyBehaviour enemyBehaviour;
    public SightMode sightMode;
    public ItemName meleeWeapon;


    [System.Serializable]
    public class PatrolData
    {
        public float patrolSpeed = 3.5f;
        public float waitTime = 2;
        public Vector3[] path = new Vector3[0];
    }
    [MyBox.ConditionalField("enemyBehaviour", false, EnemyBehaviour.Patrol)] public PatrolData patrolData;
    public Health health;
    [Header("Player Detection")]
    public Vector2 clippingPlanes = new Vector2(0.1f, 10f);
    [MyBox.ConditionalField("sightMode", false, SightMode.View)] [Range(1, 90)] public float fov = 45;
    public Transform eye;
    Collider playerCollider;
    public AnimationCurve investigateRateOverDistance = AnimationCurve.EaseInOut(0, 1, 1, 0.1f);

    [Header("Player Engagement")]

    public float engagementSpeed = 4;

    public float approachDistance = 1;
    float sqrApproachDistance => approachDistance * approachDistance;
    public bool approachPlayer = true;

    public float knockoutTime = 4f;

    Coroutine currentRoutine;
    bool investigateOnSight = false;
    bool engageOnAttack = true;
    public AnimationTransitionSet transitionSet;
    WeaponGraphicsController weaponGraphics;
    AnimationController animationController;
    Matrix4x4 viewMatrix;
    Plane[] viewPlanes = new Plane[6];
    [Header("Indicators")]
    public float height = 1.8f;
    AlertIndicatorUI alert;

    Ragdoller ragdoller;

    public void OnDamageTaken(GameObject attacker, GameObject victim)
    {
        //Push the ai back
        if (engageOnAttack)
            ChangeRoutine(Alert());
    }

    [MyBox.ButtonMethod()]
    public void Ragdoll()
    {
        ChangeRoutine(Knockout());
    }

    private void OnValidate()
    {
        if (clippingPlanes.x > clippingPlanes.y)
        {
            //If the lower value is bigger, make the upper value equal
            clippingPlanes.y = clippingPlanes.x;
        }
        else if (clippingPlanes.y < clippingPlanes.x)
        {
            //if the upper value is smaller, make the lower value equal
            clippingPlanes.x = clippingPlanes.y;
        }
    }

    public async void Die(GameObject attacker, GameObject victim)
    {
        weaponGraphics.weapon.RemoveHeld();
        weaponGraphics.bow.RemoveHeld();
        weaponGraphics.sidearm.RemoveHeld();
        ragdoller.RagdollEnabled = true;
        await Task.Delay(1000);
        Destroy(gameObject);
    }

    protected override void Start()
    {
        playerCollider = LevelInfo.currentLevelInfo.player.GetComponent<Collider>();

        health = GetComponent<Health>();
        weaponGraphics = GetComponent<WeaponGraphicsController>();
        animationController = GetComponent<AnimationController>();
        ragdoller = GetComponent<Ragdoller>();

        ragdoller.RagdollEnabled = false;

        health.onTakeDamage += OnDamageTaken;
        health.onDeath += Die;


        base.Start();
        StartBaseRoutine();

        weaponGraphics.weapon.SetHeld(InventoryController.singleton.db[meleeWeapon] as HoldableItemData);
    }


    IEnumerator DrawItem(ItemType type)
    {
        yield return weaponGraphics.DrawItem(type, transitionSet);
    }

    IEnumerator SheathItem(ItemType type)
    {
        yield return weaponGraphics.SheathItem(type, transitionSet);
    }


    void StartBaseRoutine()
    {
        if (alert != null) Destroy(alert.gameObject);

        if (enemyBehaviour == EnemyBehaviour.Patrol)
        {
            ChangeRoutine(PatrolRoutine());
        }
    }

    IEnumerator PatrolRoutine()
    {
        int i = 0;
        //If the player is seen, switch out of this routine
        investigateOnSight = true;
        agent.speed = patrolData.patrolSpeed;
        while (true)
        {
            yield return GoToPosition(patrolData.path[i]);
            yield return new WaitForSeconds(patrolData.waitTime);
            i++;
            if (i == patrolData.path.Length) i = 0;
        }

    }

    IEnumerator Investigate()
    {
        //Do not re-enter investigate
        investigateOnSight = false;
        engageOnAttack = true;
        if (alert == null || alert.gameObject == null)
            alert = IndicatorsUIController.singleton.CreateAlertIndicator(transform, Vector3.up * height);

        alert.EnableInvestigate(true);
        alert.EnableAlert(false);

        float investProgress = 0;
        //Try to look at the player long enough to alert
        while (true)
        {
            alert.SetInvestigation(investProgress);
            if (CanSeeBounds(playerCollider.bounds))
            {

                //can see player
                //Distance is the 0-1 scale where 0 is closestest visiable and 1 is furthest video
                float playerDistance = Mathf.InverseLerp(clippingPlanes.x, clippingPlanes.y, Vector3.Distance(eye.position, playerCollider.transform.position));
                //Invest the player slower if they are further away
                investProgress += Time.deltaTime * investigateRateOverDistance.Evaluate(playerDistance);

            }
            else
            {
                investProgress -= Time.deltaTime;
            }
            if (investProgress < -1)
            {
                //Cannot see player
                StartBaseRoutine();
            }
            else if (investProgress >= 1)
            {
                //Seen player
                ChangeRoutine(Alert());
            }


            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator Knockout()
    {
        ragdoller.RagdollEnabled = true;
        yield return new WaitForSeconds(knockoutTime);
        ragdoller.RagdollEnabled = false;
    }

    IEnumerator Alert()
    {
        yield return DrawItem(ItemType.Melee);

        //If alert is null create one
        alert = alert ?? IndicatorsUIController.singleton.CreateAlertIndicator(transform, Vector3.up * height);

        alert.EnableInvestigate(false);
        alert.EnableAlert(true);
        yield return new WaitForSeconds(1);
        print("Alerted");
        Destroy(alert.gameObject);
        ChangeRoutine(EngagePlayer());
    }


    IEnumerator EngagePlayer()
    {
        //Once they player has attacked or been seen, do not stop engageing until circumstances change
        engageOnAttack = false;
        investigateOnSight = false;

        agent.isStopped = true;

        Vector3 directionToPlayer;
        print("Engaged player");

        while (true)
        {
            directionToPlayer = playerCollider.transform.position - transform.position;
            if (approachPlayer && directionToPlayer.sqrMagnitude > sqrApproachDistance)
            {
                agent.Move(directionToPlayer.normalized * Time.deltaTime * engagementSpeed);
            }
            else
            {
                //Within sword range of player
                //Swing sword
                yield return SwingSword();
            }
            directionToPlayer.y = 0;
            transform.forward = directionToPlayer;

            yield return new WaitForEndOfFrame();
        }
    }


    IEnumerator SwingSword()
    {
        agent.isStopped = true; //Stop the player moving
        //swing the sword

        //This is easier. Animation graphs suck
        animationController.TriggerTransition(transitionSet.swingSword);

        MeshCollider collider = null;
        WeaponTrigger trigger = null;



        void AddTrigger()
        {
            collider = weaponGraphics.weapon.gameObject.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.isTrigger = true;
            trigger = weaponGraphics.weapon.gameObject.AddComponent<WeaponTrigger>();
            trigger.weaponItem = meleeWeapon;
            trigger.controller = gameObject;
        }

        void RemoveTrigger()
        {
            //Clean up the trigger detection of the sword
            Destroy(collider);
            Destroy(trigger);

            onSwingStateChanged = null;
        }

        onSwingStateChanged = (bool on) =>
        {
            if (on) AddTrigger();
            else RemoveTrigger();
        };

        yield return new WaitForSeconds(1);
    }
    //Triggered by animations
    public System.Action<bool> onSwingStateChanged;
    public void SwingStart() => onSwingStateChanged?.Invoke(true);
    public void SwingEnd() => onSwingStateChanged?.Invoke(false);


    void ChangeRoutine(IEnumerator newRoutine)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(newRoutine);
    }

    public bool CanSeeBounds(Bounds b)
    {
        if (sightMode == SightMode.View)
        {
            viewMatrix = Matrix4x4.Perspective(fov, 1, clippingPlanes.x, clippingPlanes.y) * Matrix4x4.Scale(new Vector3(1, 1, -1));
            GeometryUtility.CalculateFrustumPlanes(viewMatrix * eye.worldToLocalMatrix, viewPlanes);
            return GeometryUtility.TestPlanesAABB(viewPlanes, b);
        }
        else if (sightMode == SightMode.Range)
        {
            float sqrDistance = (b.center - eye.position).sqrMagnitude;
            if (sqrDistance < clippingPlanes.y * clippingPlanes.y && sqrDistance > clippingPlanes.x * clippingPlanes.x)
            {
                return true;
            }
            else return false;
        }
        return false;
    }

    private void Update()
    {
        //Test if the enemy can see the player at this point
        if (investigateOnSight)
        {
            var b = playerCollider.bounds;
            if (CanSeeBounds(b))
            {
                //can see the player, inturrupt current routine
                ChangeRoutine(Investigate());
            }
        }

        float speed = Mathf.Sign(agent.speed);

        anim.SetBool("Idle", speed == 0);
        anim.SetFloat("InputVertical", speed, 0.2f, Time.deltaTime);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        LookAtPlayer(playerCollider.transform.position);
    }
    private void OnDrawGizmos()
    {
        if (patrolData.path.Length >= 2)
        {
            for (int i = 0; i < patrolData.path.Length - 1; i++)
            {
                Gizmos.DrawLine(patrolData.path[i], patrolData.path[i + 1]);
            }
            Gizmos.DrawLine(patrolData.path[0], patrolData.path[patrolData.path.Length - 1]);
        }
        if (playerCollider != null)
        {
            var b = playerCollider.bounds;
            if (CanSeeBounds(b))
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawWireCube(b.center, b.size);
            if (sightMode == SightMode.View)
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = eye.localToWorldMatrix;
                Gizmos.DrawFrustum(Vector3.zero, fov, clippingPlanes.y, clippingPlanes.x, 1f);
            }
            else if (sightMode == SightMode.Range)
            {
                Gizmos.DrawWireSphere(eye.position, clippingPlanes.x);
                Gizmos.DrawWireSphere(eye.position, clippingPlanes.y);
            }
        }
    }
}
