using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBlock : ScriptableObject
{
    [SerializeField] ItemSlot[] equippableSlots;

    [SerializeField, Range(0, 4)] int bulk;

    Dictionary<DamageType, DamageProtection> protections = new Dictionary<DamageType, DamageProtection>();
}

public enum ItemSlot
{
    Head,
    RightHand,
    LeftHand,
    Armor,
    Accessory1,
    Accessory2, 
    Accessory3
}
