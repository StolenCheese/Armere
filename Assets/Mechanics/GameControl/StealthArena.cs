﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthArena : MonoBehaviour
{
    public Barrier[] lockdownBarriers;
    public EnemyAISpawner[] enemyAISpawners;
    public void OnEnemyDetectedPlayer(EnemyAI enemy)
    {
        //Pause the game

        //Focus on the enemy
        GameCameras.s.FocusCutsceneCameraToTargets(enemy.transform);
        //Lock down the area
        foreach (var barrier in lockdownBarriers)
        {
            barrier.Close();
        }

        StartCoroutine(GiveControlBackToPlayer());
    }

    IEnumerator GiveControlBackToPlayer()
    {
        yield return new WaitForSeconds(2);

        //Return camera to player so he can be killed by op characters
        GameCameras.s.DisableCutsceneCamera();
    }
    public void RemoveAllEnemies()
    {
        foreach (var s in enemyAISpawners)
        {
            s.body.Destroy();
        }
    }
}
