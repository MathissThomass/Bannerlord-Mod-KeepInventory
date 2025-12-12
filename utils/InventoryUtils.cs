using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace KeepInventory.utils;

public static class InventoryUtils
{
    /// <summary>
    /// Get the player inventory, or null if errors or if nothing in the inventory
    /// </summary>
    /// <returns>Player inventory</returns>
    public static ItemRoster? GetPlayerInventory()
    {
        if (MobileParty.MainParty == null)
        {
            return null;
        }

        var roster = MobileParty.MainParty.ItemRoster;

        if (roster == null || roster.Count == 0)
        {
            return null;
        }

        return roster;
    }

    /// <summary>
    /// Get the player home settlements, if the settlement is under siege, get the first settlement available
    /// </summary>
    /// <returns>The preferred settlement to send the inventory</returns>
    public static Settlement? GetPreferredStashSettlement()
    {
        var hero = Hero.MainHero;

        var home = hero.HomeSettlement;

        if (home == null || home.OwnerClan != Clan.PlayerClan || home.Stash == null)
        {
            return null;
        }

        if (!home.IsUnderSiege)
        {
            return home;
        }
        else
        {
            return hero.Clan.Settlements
                .FirstOrDefault(settlement =>
                    settlement != home &&
                    settlement.OwnerClan == Clan.PlayerClan &&
                    settlement is { Stash: not null, IsUnderSiege: false, IsVillage: false });
        }
    }

    /// <summary>
    /// Print in the game log the inventory, used for debug
    /// </summary>
    /// <param name="inventory"></param>
    private static void PrintInventory(ItemRoster inventory)
    {
        foreach (var item in inventory)
        {
            if (item.IsEmpty) continue;

            string name = item.EquipmentElement.Item?.Name?.ToString() ?? "Item inconnu";
            int amount = item.Amount;
            InformationManager.DisplayMessage(
                new InformationMessage($"[KeepInventory] {name} x{amount}"));
        }
    }

    /// <summary>
    /// Clone a roster to make a snapshot of it
    /// </summary>
    /// <param name="source">The roster to copy</param>
    /// <returns>Cloned roster</returns>
    public static ItemRoster CloneRoster(ItemRoster source)
    {
        var clone = new ItemRoster();

        foreach (var element in source)
        {
            if (element.IsEmpty)
                continue;

            var eq = element.EquipmentElement;
            var amount = element.Amount;

            clone.Add(new ItemRosterElement(eq, amount));
        }

        return clone;
    }

    /// <summary>
    /// Check if an item is protected, if it is a quest item you should not mess with this item.
    /// </summary>
    /// <param name="element">Item to check</param>
    /// <returns>True if it's protected</returns>
    public static bool IsProtectedItem(ItemRosterElement element)
    {
        var eq = element.EquipmentElement;
        var item = eq.Item;
        return item == null || eq.IsQuestItem;
    }

    /// <summary>
    /// Give an item to the player inventory
    /// </summary>
    /// <param name="itemId">name of the item</param>
    /// <param name="amount"></param>
    public static void GiveItemToPlayer(string itemId, int amount)
    {
        var item = Items.All.Find(i => i.StringId == itemId);
        if (item == null)
        {
            InformationManager.DisplayMessage(
                new InformationMessage($"[KeepInventory] Item introuvable : {itemId}")
            );
            return;
        }

        var playerInventory = GetPlayerInventory();
        if (playerInventory == null) return;

        playerInventory.AddToCounts(item, amount);
    }
}