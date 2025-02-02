﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


namespace Armere.Inventory
{
	[CreateAssetMenu(menuName = "Game/Inventory/Spawned item saver")]
	public class SpawnedItemSaver : ScriptableObject, IGameDataSavable<SpawnedItemSaver>
	{

		//Save format:
		/*
		int COUNT:
		foreach in COUNT
			-Vec3 position
			-quat rotation
			-uint bundleSize
			-ulong,ulong itemData
		*/

		public SpawnedItemSaver Read(in GameDataReader reader)
		{
			if (reader.saveVersion != SaveManager.version)
			{
				Debug.LogWarning("Incorrect version");
			}
			Profiler.BeginSample("Restoring Item Spawnable Objects");

			int numItems = reader.ReadInt();
			for (int i = 0; i < numItems; i++)
			{
				Vector3 pos = reader.ReadVector3();
				Quaternion rot = reader.ReadQuaternion();
				uint bundleSize = reader.ReadUInt();
				ItemDatabase.ReadItemData(reader, item =>
				{
					if (item is PhysicsItemData data)
					{
						ItemSpawner.SpawnItem(data, pos, rot);
					}
				});

			}

			Profiler.EndSample();
			Profiler.BeginSample("Loading Item Spawners");

			int numSpawners = reader.ReadInt();


			var itemSpawnerData = new Dictionary<System.Guid, bool>();

			for (int i = 0; i < numSpawners; i++)
			{
				itemSpawnerData[reader.ReadGuid()] = reader.ReadBool();
			}

			foreach (var spawner in MonoBehaviour.FindObjectsOfType<ItemSpawner>())
			{
				if (itemSpawnerData.TryGetValue(spawner.GetComponent<GuidComponent>().GetGuid(), out var data))
				{
					//This should be set before the spawner's start value
					spawner.spawnedItem = data;
				}
			}

			Profiler.EndSample();

			return this;
		}

		public void Write(in GameDataWriter writer)
		{
			Profiler.BeginSample("Saving Item Spawnable Objects");

			var spawnedItems = FindObjectsOfType<ItemSpawnable>();
			writer.WritePrimitive(spawnedItems.Length);
			for (int i = 0; i < spawnedItems.Length; i++)
			{
				writer.WritePrimitive(spawnedItems[i].transform.position);
				writer.WritePrimitive(spawnedItems[i].transform.rotation);
				writer.WritePrimitive(spawnedItems[i].count);
				writer.Write(spawnedItems[i].item);
			}


			Profiler.EndSample();
			Profiler.BeginSample("Saving Item Spawners");

			var spawners = FindObjectsOfType<ItemSpawner>();
			writer.WritePrimitive(spawners.Length);
			for (int i = 0; i < spawners.Length; i++)
			{
				writer.WritePrimitive(spawners[i].GetComponent<GuidComponent>().GetGuid());
				writer.WritePrimitive(spawners[i].spawnedItem);
			}

			Profiler.EndSample();
		}

		public SpawnedItemSaver Init()
		{
			return this;
		}
	}
}