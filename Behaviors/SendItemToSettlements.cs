using System;
using KeepInventory.Services;
using KeepInventory.Settings;
using KeepInventory.utils;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace KeepInventory.Behaviors;

public class SendItemToSettlements : CampaignBehaviorBase
{
    private ItemRoster? InventoryBeforeBattle { get; set; }


    public override void RegisterEvents()
    {
        CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnBattleStarted);
        CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
    }

    public override void SyncData(IDataStore dataStore)
    {
        var inventoryBeforeBattle = InventoryBeforeBattle;
        dataStore.SyncData("KeepInventory_InventoryBeforeBattle", ref inventoryBeforeBattle);
        InventoryBeforeBattle = inventoryBeforeBattle;
    }

    /// <summary>
    /// When start a battle, take a snapshot of the inventory, because when taken prisoner the inventory is already changed by the game.
    /// </summary>
    private void OnBattleStarted(MapEvent mapEvent, PartyBase attacker, PartyBase defender)
    {
        if (!mapEvent.IsPlayerMapEvent) return;

        var inventory = InventoryUtils.GetPlayerInventory();

        if (inventory == null) return;
        InventoryBeforeBattle = InventoryUtils.CloneRoster(inventory);
    }

    /// <summary>
    /// This method applies the behavior selected in the MCM settings:
    /// - Send the entire pre-battle inventory to the home settlement.
    /// - Restore all lost items so the player keeps their inventory.
    /// - Send only the items that were actually lost.
    /// - Remove all non-quest items from the inventory.
    ///
    /// The logic relies on the inventory snapshot captured before the battle
    /// (InventoryBeforeBattle) to compare, restore, or transfer items.
    ///
    /// If the player has no valid home settlement, the system restores the inventory
    /// instead of attempting an invalid transfer.
    /// </summary>
    private void OnHeroPrisonerTaken(PartyBase partyBase, Hero hero)
    {
        try
        {
            if (hero != Hero.MainHero)
                return;

            var settings = GlobalSettings<KeepInventorySettings>.Instance;

            if (settings == null)
            {
                return;
            }

            var currentInventory = InventoryUtils.GetPlayerInventory();
            if (currentInventory == null)
                return;

            if (InventoryBeforeBattle == null)
            {
                return;
            }

            if (settings.LoseEverythingOnDefeat)
            {
                InventoryTransferServices.ClearAllNonQuestItems(currentInventory);
                //InventoryUtils.GiveItemToPlayer("grain", 1);
            }
            else if (settings.KeepItemsInInventory)
            {
                InventoryTransferServices.RestoreLostInventory(InventoryBeforeBattle!, currentInventory);
            }
            else
            {
                var homeSettlement = InventoryUtils.GetPlayerHomeSettlement();
                if (homeSettlement == null)
                {
                    InventoryTransferServices.RestoreLostInventory(InventoryBeforeBattle!, currentInventory);
                    return;
                }

                if (settings.SendAllInventoryToHomeSettlement)
                {
                    InventoryTransferServices.SendAllItem(InventoryBeforeBattle!, homeSettlement);
                    InventoryTransferServices.ClearAllNonQuestItems(currentInventory);
                    //InventoryUtils.GiveItemToPlayer("grain", 1);
                }
                else if (settings.SendOnlyLostItems)
                {
                    InventoryTransferServices.SendOnlyLostInventory(InventoryBeforeBattle!, currentInventory,
                        homeSettlement);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}