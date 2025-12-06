using KeepInventory.utils;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace KeepInventory.Services;

public static class InventoryTransferServices
{
    /// <summary>
    /// Send all your items to the player home settlement.
    /// </summary>
    /// <param name="inventory"> The player inventory to transfer</param>
    /// <param name="home"> The home settlement of the player</param>
    public static void SendAllItem(ItemRoster inventory, Settlement home)
    {
        var stash = home.Stash;
        if (stash == null)
        {
            InformationManager.DisplayMessage(
                new InformationMessage("[KeepInventory] The settlement has no stash available.", Colors.Red)
            );
            return;
        }

        foreach (var element in inventory)
        {
            if (element.IsEmpty)
                continue;

            if (InventoryUtils.IsProtectedItem(element))
                continue;

            stash.Add(element);
        }
    }

    /// <summary>
    /// Send only the items that the game remove from the player inventory to the player home settlement.
    /// </summary>
    /// <param name="inventoryBeforeBattle">The player inventory snapshot before battle</param>
    /// <param name="currentInventory">The current player inventory</param>
    /// <param name="home">The player home settlement</param>
    public static void SendOnlyLostInventory(ItemRoster inventoryBeforeBattle, ItemRoster currentInventory,
        Settlement home)
    {
        if (home.Stash == null)
        {
            InformationManager.DisplayMessage(
                new InformationMessage("[KeepInventory] The settlement has no stash available.", Colors.Red)
            );
            return;
        }

        SendItemRosterDifference(inventoryBeforeBattle, currentInventory, home.Stash);
    }

    /// <summary>
    /// Restore the items removed by the game in the player inventory
    /// </summary>
    /// <param name="inventoryBeforeBattle">The player inventory snapshot before battle</param>
    /// <param name="currentInventory">The current player inventory</param>
    public static void RestoreLostInventory(ItemRoster inventoryBeforeBattle, ItemRoster currentInventory)
    {
        SendItemRosterDifference(inventoryBeforeBattle, currentInventory, currentInventory);
    }

    /// <summary>
    /// Remove from the roster every element
    /// </summary>
    /// <param name="roster">An inventory like the player inventory</param>
    public static void ClearAllNonQuestItems(ItemRoster roster)
    {
        for (int i = roster.Count - 1; i >= 0; i--)
        {
            var element = roster.GetElementCopyAtIndex(i);
            if (element.IsEmpty)
                continue;

            if (InventoryUtils.IsProtectedItem(element))
            {
                continue;
            }

            roster.Remove(element);
        }
    }

    /// <summary>
    /// Send the difference between two ItemRoster to a chosen roster
    /// </summary>
    /// <param name="inventoryBeforeBattle">The player inventory snapshot before battle</param>
    /// <param name="currentInventory">The current player inventory</param>
    /// <param name="rosterToSendItem">The ItemRoster you want the item to be sent (home / player inventory)</param>
    private static void SendItemRosterDifference(ItemRoster inventoryBeforeBattle, ItemRoster currentInventory,
        ItemRoster rosterToSendItem)
    {
        foreach (var beforeElement in inventoryBeforeBattle)
        {
            if (beforeElement.IsEmpty)
                continue;

            if (InventoryUtils.IsProtectedItem(beforeElement))
                continue;

            var equipment = beforeElement.EquipmentElement;
            var countBefore = beforeElement.Amount;

            // Count how many identical stacks remain in the current inventory
            var countNow = 0;
            foreach (var currentElement in currentInventory)
            {
                if (currentElement.IsEmpty)
                    continue;

                if (currentElement.EquipmentElement.Equals(equipment))
                {
                    countNow += currentElement.Amount;
                }
            }

            var lostCount = countBefore - countNow;
            if (lostCount <= 0)
                continue;

            var toAdd = new ItemRosterElement(equipment, lostCount);
            rosterToSendItem.Add(toAdd);
        }
    }
}