using Asuna.CharManagement;
using Asuna.Items;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ItemExpansionMod
{
    public class CustomEquipment : ModEquipment
    {
        public bool IsLocked;
        public List<StatModifierInfo> StatRequirements;
        public List<StatModifierInfo> StatModifierInfos;
        private CustomApparel _instance;

        public void CustomInitialize(ModSpriteResolver spriteResolver)
        {
            CustomApparel apparel = ScriptableObject.CreateInstance<CustomApparel>();
            apparel.Name = Name;
            apparel.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            apparel.DurabilityDisplayLayers.AddRange(Sprites.Select((ModEquipmentSprite x) => x.Get(spriteResolver)));
            apparel.Category = ItemCategory.Clothing;
            apparel.DisplaySpriteResource = spriteResolver.ResolveAsResource(PreviewImage);
            apparel.IsLocked = IsLocked;
            apparel.StatRequirements = StatRequirements;
            apparel.StatModifierInfos = StatModifierInfos;
            _instance = apparel;
            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }
            else
            {
                Debug.LogError("Did not register Item \"" + Name + "\", because an item with the same name already exists.");
            }
        }
    }
}