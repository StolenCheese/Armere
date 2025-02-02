using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Armere.Inventory
{

    public abstract class HoldableItemData : PhysicsItemData
    {
        [Range(0, 1)]
        public float clankProbability = 0.7f;
        public AudioClipSet clankSet;

        public AssetReferenceGameObject holdableGameObject;
    }
}