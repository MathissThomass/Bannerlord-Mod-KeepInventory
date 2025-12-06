using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace KeepInventory.Settings;

internal sealed class KeepInventorySettings : AttributeGlobalSettings<KeepInventorySettings>
{
    public override string Id => "KeepInventorySettings_v1";
    public override string DisplayName => "Keep Inventory";
    public override string FolderName => "KeepInventory";
    public override string FormatType => "json";

    private bool _sendAllInventoryToHomeSettlement = true;
    private bool _keepItemsInInventory = false;
    private bool _sendOnlyLostItems = false;
    private bool _loseEverythingOnDefeat = false;

    /// <summary>
    /// Ensures mutual exclusivity between the different toogle options.
    /// When one options is selected, this method unchecks all others except the specified one.
    /// </summary>
    /// <param name="except">The name of the property that should remain checked.</param>
    private void UncheckOthers(string except)
    {
        if (except != nameof(SendAllInventoryToHomeSettlement))
            _sendAllInventoryToHomeSettlement = false;
        if (except != nameof(KeepItemsInInventory))
            _keepItemsInInventory = false;
        if (except != nameof(SendOnlyLostItems))
            _sendOnlyLostItems = false;
        if (except != nameof(LoseEverythingOnDefeat))
            _loseEverythingOnDefeat = false;

        OnPropertyChanged(null);
    }

    /// <summary>
    /// Checks whether any mutally exclusive option, other than the specified one is currently enabled.
    /// </summary>
    /// <param name="except">The property name that should be ignored during check</param>
    /// <returns>true if any option is enabled</returns>
    private bool AnyOtherTrue(string except)
    {
        return (except != nameof(SendAllInventoryToHomeSettlement) && _sendAllInventoryToHomeSettlement)
               || (except != nameof(KeepItemsInInventory) && _keepItemsInInventory)
               || (except != nameof(SendOnlyLostItems) && _sendOnlyLostItems)
               || (except != nameof(LoseEverythingOnDefeat) && _loseEverythingOnDefeat);
    }

    /// <summary>
    /// Applies mutually exclusive selection logic for a boolean MCM setting.
    /// </summary>
    /// <param name="field">Reference to the backing field of the setting being modified.</param>
    /// <param name="value">The new value requested by MCM.</param>
    /// <param name="propertyName">The name of the property being updated</param>
    private void SetExclusiveMode(ref bool field, bool value, string propertyName)
    {
        if (field == value)
            return;

        if (value)
        {
            field = true;
            UncheckOthers(propertyName);
        }
        else
        {
            if (!AnyOtherTrue(propertyName))
                return; 

            field = false;
        }

        OnPropertyChanged(propertyName);
    }

    // === Options ===

    [SettingPropertyBool("Send everything to home settlement stash",
        Order = 0,
        RequireRestart = false,
        HintText = "Sends your entire pre-battle inventory to your home settlement. Items that wouldn't be lost will also be transferred. If you don't own a settlement, your items will stay in your inventory instead.")]
    [SettingPropertyGroup("General")]
    public bool SendAllInventoryToHomeSettlement
    {
        get => _sendAllInventoryToHomeSettlement;
        set => SetExclusiveMode(ref _sendAllInventoryToHomeSettlement, value, nameof(SendAllInventoryToHomeSettlement));
    }

    [SettingPropertyBool("Keep everything in player inventory",
        Order = 1,
        RequireRestart = false,
        HintText = "Prevents the game from removing any items after defeat. You keep everything, regardless of what the game would normally take.")]
    [SettingPropertyGroup("General")]
    public bool KeepItemsInInventory
    {
        get => _keepItemsInInventory;
        set => SetExclusiveMode(ref _keepItemsInInventory, value, nameof(KeepItemsInInventory));
    }

    [SettingPropertyBool("Send only lost items",
        Order = 2,
        RequireRestart = false,
        HintText = "Sends only the items the game removes after defeat to your home settlement.")]
    [SettingPropertyGroup("General")]
    public bool SendOnlyLostItems
    {
        get => _sendOnlyLostItems;
        set => SetExclusiveMode(ref _sendOnlyLostItems, value, nameof(SendOnlyLostItems));
    }

    [SettingPropertyBool("Lose everything on defeat",
        Order = 3,
        RequireRestart = false,
        HintText = "Removes all non-quest items from your inventory after defeat. Nothing is kept or transferred."
    )]
    [SettingPropertyGroup("General")]
    public bool LoseEverythingOnDefeat
    {
        get => _loseEverythingOnDefeat;
        set => SetExclusiveMode(ref _loseEverythingOnDefeat, value, nameof(LoseEverythingOnDefeat));
    }
}