using Asuna.CharManagement;
using Asuna.Dialogues;
using Asuna.Items;
using Modding;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace ItemExpansionMod
{
    public class ItemExpansionMod : ITCMod
    {
        public List<string> NewItemNames = new List<string>();

        public void OnDialogueStarted(Dialogue dialogue)
        {
            Debug.Log("Modded OnDialogueStarted");
        }

        public void OnFrame(float deltaTime)
        {
            //
        }

        public void OnLevelChanged(string oldLevel, string newLevel)
        {
            Debug.Log("Modded OnLevelChanged");
        }

        public void OnLineStarted(DialogueLine line)
        {
            Debug.Log("Modded OnLineStarted");
        }

        public static T Deserialize<T>(string xmlString)
        {
            if (xmlString == null) return default;
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public void OnModLoaded(ModManifest manifest)
        {
            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, "data\\ItemData.xml")))
            {
                string xml = reader.ReadToEnd();
                List<CustomEquipment> customEquipments = Deserialize<List<CustomEquipment>>(xml);

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    customEquipment.CustomInitialize(manifest.SpriteResolver);
                    CustomApparel item = CustomApparel.CreateWithStatModifiers(customEquipment.Name);
                    NewItemNames.Add(item.Name);
                    GiveItems.GiveToCharacter(Character.Get("Jenna"), false, false, true, item);

                    //ItemShopCatalogue catalogue = ScriptableObject.CreateInstance<ItemShopCatalogue>();
                    //catalogue.Items = new List<ShopItemInfo>()
                    //{
                    //    new ShopItemInfo()
                    //    {
                    //        Item = item,
                    //        Cost = 1000,
                    //    }
                    //};
                    //ItemVendor vendor = new ItemVendor()
                    //{
                    //    Catalogue = catalogue
                    //};
                }
            }

`            Debug.Log("Modded OnModLoaded");
        }

        public void OnModUnLoaded()
        {
            // Clean up items
            foreach (string itemName in NewItemNames)
            {
                Debug.Log("Remove " + itemName);
                Item.All.Remove(itemName.ToLower());
            }
            Debug.Log("Modded OnModUnLoaded");
        }

        private void SetStatModifier(Equipment equipment, string statName, int value)
        {
            List<StatModifierInfo> modifier = new List<StatModifierInfo>();
            StatModifierInfo statModifierInfo =
                new StatModifierInfo()
                {
                    Type = StatModifierType.Value,
                    StatName = statName,
                    ModifyAmount = value,
                };
            modifier.Add(statModifierInfo);

            typeof(Equipment)
               .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
               .SetValue(equipment, modifier);
        }
    }
}
