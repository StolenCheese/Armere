﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerController
{
    public class Swimming : MovementState
    {
        public override string StateName => "Swimming";

        GameObject waterTrail;
        WaterTrailController waterTrailController;

        bool onSurface = true;
        bool stopped = true;
        const string animatorVariable = "IsSwimming";
        void ChangeDive(bool diving)
        {
            onSurface = !diving;
            animator.SetBool(c.animatorVariables.isGrounded.id, onSurface);
            if (onSurface) transform.rotation = Quaternion.identity;

            if (diving) waterTrailController.StopTrail();
        }

        DebugMenu.DebugEntry<int, float> entry;

        public override void Start()
        {
            c.rb.useGravity = false;
            c.rb.drag = c.waterDrag;
            c.animationController.enableFeetIK = false;
            c.animator.SetBool(animatorVariable, true);

            waterTrail = MonoBehaviour.Instantiate(c.waterTrailPrefab, transform);
            //Place slightley above water to avoid z buffer clashing
            waterTrail.transform.localPosition = Vector3.up * (c.waterSittingDepth + 0.03f);

            waterTrailController = waterTrail.GetComponent<WaterTrailController>();

            entry = DebugMenu.CreateEntry("Player", "Hits: {0} Current Depth: {1}", 0, 0f);
        }
        public override void End()
        {
            c.rb.useGravity = true;
            c.animator.SetBool(animatorVariable, false);

            DebugMenu.RemoveEntry(entry);

            MonoBehaviour.Destroy(waterTrail);
        }



        public override void FixedUpdate()
        {
            //Test to see if the player is still in deep water

            RaycastHit[] waterHits = new RaycastHit[2];
            float heightOffset = 2;
            int hits = Physics.RaycastNonAlloc(
                transform.position + new Vector3(0, heightOffset, 0),
                Vector3.down, waterHits, c.maxWaterStrideDepth + heightOffset,
                //Scan for water and ground
                c.m_groundLayerMask | c.m_waterLayerMask, QueryTriggerInteraction.Collide);


            if (hits == 2)
            {
                WaterController w = waterHits[1].collider.GetComponentInParent<WaterController>();
                print("hitting ground");
                if (w != null)
                {
                    //Hit water and ground
                    float waterDepth = waterHits[0].distance - waterHits[1].distance;
                    if (waterDepth <= c.maxWaterStrideDepth)
                    {
                        //Within walkable water
                        c.ChangeToState<Walking>();
                    }
                }
            }
            //If underwater and going up but close to the surface, return to surface
            else if (!onSurface && c.rb.velocity.y >= 0 && hits > 0 && waterHits[0].distance < heightOffset && heightOffset - waterHits[0].distance < c.maxWaterStrideDepth)
                ChangeDive(false);

            if (onSurface && c.allCPs.Count > 0)
            {
                //Player is colliding with something
                for (int i = 0; i < c.allCPs.Count; i++)
                {

                    //If about perpendicular to wall
                    if (Mathf.Abs(Vector3.Dot(Vector3.up, c.allCPs[i].normal)) < 0.01f)
                    {

                        //Colliding against wall
                        //Test to see if we can vault up it into walking
                        Vector3 origin = transform.position + Vector3.up * (c.waterSittingDepth + c.collider.height) - c.allCPs[i].normal * c.collider.radius * 2;


                        Debug.DrawLine(origin, origin + Vector3.down * c.collider.height * 1.05f, Color.blue, Time.deltaTime);

                        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, c.collider.height * 1.05f, c.m_groundLayerMask, QueryTriggerInteraction.Ignore) &&
                         Vector3.Dot(Vector3.up, hit.normal) > c.m_maxGroundDot)
                        {
                            //Can move to hit spot
                            transform.position = hit.point;
                            c.ChangeToState<TransitionState<Walking>>(0.2f);
                            return;
                        }
                    }
                }
            }


            if (DebugMenu.menuEnabled)
            {
                entry.value0 = hits;
                if (hits == 2)
                {
                    entry.value1 = waterHits[0].distance - waterHits[1].distance;
                }
                else if (hits == 1)
                {
                    entry.value1 = heightOffset - waterHits[0].distance;
                }
                else if (hits == 0)
                {
                    entry.value1 = c.maxWaterStrideDepth;
                }
            }


            Vector3 playerDirection;

            if (onSurface)
                playerDirection = c.cameraController.TransformInput(c.input.horizontal) * c.waterMovementForce * Time.fixedDeltaTime;
            else
                playerDirection = GameCameras.s.cameraTransform.TransformDirection(new Vector3(c.input.horizontal.x, c.input.vertical, c.input.horizontal.y)) * c.waterMovementForce * Time.fixedDeltaTime;


            if (playerDirection.sqrMagnitude > 0)
            {
                transform.forward = playerDirection;
                if (onSurface && stopped)
                {
                    stopped = false;
                    waterTrailController.StartTrail();
                }
            }
            else if (onSurface && !stopped)
            {
                stopped = true;
                waterTrailController.StopTrail();
            }

            Vector3 requiredForce = playerDirection * c.waterMovementSpeed - c.rb.velocity;
            requiredForce.y = 0;

            requiredForce = Vector3.ClampMagnitude(requiredForce, c.waterMovementForce * Time.fixedDeltaTime);

            c.rb.AddForce(requiredForce);

            if (onSurface)
                //Always force player to be on water surface while simming
                transform.position = c.currentWater.waterVolume.ClosestPoint(transform.position + Vector3.up * 1000) - Vector3.up * c.waterSittingDepth;
            else
                transform.position = c.currentWater.waterVolume.ClosestPoint(transform.position);

            //Transition to dive if space pressed
            if (onSurface && c.mod.HasFlag(MovementModifiers.Crouching))
            {
                ChangeDive(true);
                transform.position -= Vector3.up * c.maxWaterStrideDepth;
            }
        }

        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.horizontal.id, 0);
        }
    }
}
