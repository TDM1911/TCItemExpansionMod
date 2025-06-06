using ANToolkit.Controllers;
using ANToolkit.Save;
using Asuna.CharManagement;
using Asuna.Dialogues;
using Asuna.Items;
using Modding;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace ItemExpansionMod
{
    public class ItemExpansionMod : ITCMod
    {
        ItemVendor vendor;
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
            if (newLevel == "Carceburg")
            {
                Debug.Log("Spawn NPC");
                var interactableGameObject = new GameObject();
                interactableGameObject.transform.position = new Vector3(8f, -18f);
                var boxCollider = interactableGameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1f, 1f);

                var interactable = interactableGameObject.AddComponent<Interactable>();
                interactable.TypeOfInteraction = InteractionType.Talk;
                interactable.OnInteracted.AddListener(x =>
                {
                    vendor.Catalogue.OpenShop();
                });
            }
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
                List<ShopItemInfo> shopItems = new List<ShopItemInfo>();

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    customEquipment.CustomInitialize(manifest.SpriteResolver);
                    CustomApparel item = CustomApparel.CreateWithStatModifiers(customEquipment.Name);
                    NewItemNames.Add(item.Name);

                    shopItems.Add(
                        new ShopItemInfo()
                        {
                            Item = item,
                            Cost = 1000,
                        }
                    );
                }
                ItemShopCatalogue catalogue = ScriptableObject.CreateInstance<ItemShopCatalogue>();
                catalogue.Items = shopItems;
                ANToolkit.ResourceManagement.ANResourceSprite resource = manifest.SpriteResolver.ResolveAsResource("assets\\sprites\\ANDR-047139.png");

                var dialogue = ScriptableObject.CreateInstance<Dialogue>();
                vendor = new ItemVendor()
                {
                    Catalogue = catalogue,
                    TargetDialogue = dialogue,
                };
            }

            Debug.Log("Modded OnModLoaded");
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
    }
}
