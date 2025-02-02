using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
namespace Armere.Inventory.UI
{
	public class BuyInventoryUI : MonoBehaviour
	{
		public GameObject template;
		public Transform inventoryDisplayHolder;
		public GameObject holder;

		[SerializeField] BuyMenuItem[] inventory;
		public ItemInfoDisplay selectedDisplay;
		int selected;
		System.Action onItemSelected;
		bool waitingForConfirmation = false;
		public string outOfStockAlert = "Out Of Stock";

		InventoryController playerInventory;

		public async void ShowInventory(BuyMenuItem[] inventory, InventoryController playerInventory, System.Action onItemSelected)
		{
			print("Showing Buy Menu");
			this.inventory = inventory;
			this.playerInventory = playerInventory;
			this.onItemSelected = onItemSelected;
			waitingForConfirmation = false;
			holder.SetActive(true);
			for (int i = 0; i < inventory.Length; i++)
			{
				var item = Instantiate(template, inventoryDisplayHolder);
				var buyMenuItem = item.GetComponent<BuyInventoryUIItem>();

				if (inventory[i].count == 1)
					buyMenuItem.title.text = inventory[i].item.name;
				else
					buyMenuItem.title.text = string.Format("{0} x{1}", inventory[i].item.name, inventory[i].count);

				buyMenuItem.stock.text = inventory[i].stock.ToString();


				buyMenuItem.UpdateCost(
					inventory[i].cost,
					playerInventory.currency.ItemCount(0)
					);

				buyMenuItem.controller = this;

				buyMenuItem.index = i;
				//Inject the index of the button into the callback
				buyMenuItem.selectButton.onClick.AddListener(() => SelectItem(buyMenuItem.index));

				buyMenuItem.thumbnail.sprite = await Addressables.LoadAssetAsync<Sprite>(inventory[i].item.thumbnail).Task;
			}
		}

		public void SelectItem(int index)
		{
			if (!waitingForConfirmation)
			{
				selected = index;
				onItemSelected?.Invoke();
				WaitForBuyConfirmation();
			}
		}

		public void ShowInfo(int index)
		{
			selectedDisplay.ShowInfo(new ItemStackBase(inventory[index].item));
		}
		public void WaitForBuyConfirmation()
		{
			waitingForConfirmation = true;
		}
		public void ConfirmBuy()
		{
			uint amount = 1;


			if (inventory[selected].stock - amount < 0)
			{
				print("Cannot purchase this many items");
			}

			//Will return true if the currency is successfully removed
			//Only need to take the item being sold as it will interpret that as the value
			else if (playerInventory.currency.TakeValue(inventory[selected].cost * amount))
			{
				inventory[selected].stock -= amount;

				playerInventory.TryAddItem(inventory[selected].item, inventory[selected].count, true);

				waitingForConfirmation = false;
				if (inventory[selected].stock == 0)
				{
					inventoryDisplayHolder.GetChild(selected).GetComponent<BuyInventoryUIItem>().selectButton.interactable = false;
					inventoryDisplayHolder.GetChild(selected).GetComponent<BuyInventoryUIItem>().stock.text = outOfStockAlert;
				}
				else
				{
					inventoryDisplayHolder.GetChild(selected).GetComponent<BuyInventoryUIItem>().stock.text = inventory[selected].stock.ToString();
				}
				//Update all the currency displays
				for (int i = 0; i < inventoryDisplayHolder.childCount; i++)
				{
					inventoryDisplayHolder.GetChild(i).GetComponent<BuyInventoryUIItem>().UpdateCost(
						inventory[i].cost,
						playerInventory.currency.ItemCount(0));


				}
			}
			else
			{
				//Not enough money for transaction
				Debug.LogWarning("Attempted to purchase item without required funds");
			}

		}
		public void CancelBuy()
		{
			waitingForConfirmation = false;
		}
		public BuyMenuItem[] CloseInventory()
		{
			holder.SetActive(false);
			//Clean up the menu for the next opening
			for (int i = 0; i < inventoryDisplayHolder.childCount; i++)
			{
				Destroy(inventoryDisplayHolder.GetChild(i).gameObject);
			}
			return inventory;
		}

	}
}