using ANToolkit.Controllers;
using ANToolkit.Level;
using ANToolkit.Save;
using Asuna.CharManagement;
using ANToolkit.ResourceManagement;
using Asuna.Dialogues;
using Asuna.Items;
using Asuna.Weather;
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
        public ItemVendor vendor;
        public List<string> NewItemNames = new List<string>();
        public List<Dialogue> dialogues = new List<Dialogue>();
        public Dictionary<string, ANResourceSprite> resourceSprites = new Dictionary<string, ANResourceSprite>();

        public void OnDialogueStarted(Dialogue dialogue)
        {
            Debug.Log("Modded OnDialogueStarted");
        }

        public void OnFrame(float deltaTime)
        {
            // If you decompile this to check for malware :3 Hi!!! :D
        }

        public void OnLevelChanged(string oldLevel, string newLevel)
        {
            Character.Player.OnItemEquipped.AddListener(x =>
            {
                //
            });
            string baseCustomShopLevel = "Motel_Tunnel";
            if (SaveManager.GetKey("inAndrNPCRoom", false) == true && newLevel != baseCustomShopLevel)
            {
                SaveManager.SetKey("inAndrNPCRoom", false);
                Character.Player.Handlers.First().transform.position = new Vector3(8f, -18f);
            }
            if (SaveManager.GetKey("inAndrNPCRoom", false) == true && newLevel == baseCustomShopLevel)
            {
                List<CustomApparel> lockedItems = new List<CustomApparel>();
                foreach (CustomApparel item in Character.Player.EquippedItems.GetAll<CustomApparel>())
                {
                    if (item.IsLocked)
                    {
                        lockedItems.Add(item);
                        item.IsLocked = false;
                    }
                }

                foreach (CustomApparel item in Character.Player.Inventory.GetAll<CustomApparel>())
                {
                    if (item.IsLocked)
                    {
                        lockedItems.Add(item);
                        item.IsLocked = false;
                    }
                }
                GameObject wallLeft = new GameObject();
                wallLeft.transform.position = new Vector3(-4.5f, 1f);
                BoxCollider boxColliderLeft = wallLeft.AddComponent<BoxCollider>();
                boxColliderLeft.size = new Vector3(2f, 2f);

                GameObject wallRight = new GameObject();
                wallRight.transform.position = new Vector3(8f, 1f);
                BoxCollider boxColliderRight = wallRight.AddComponent<BoxCollider>();
                boxColliderRight.size = new Vector3(2f, 2f);

                GameObject door = new GameObject();
                door.transform.position = new Vector3(2.5f, -0.45f);
                BoxCollider doorCollider = door.AddComponent<BoxCollider>();
                doorCollider.size = new Vector3(2f, 1f);

                GameObject doorSprite = new GameObject();
                doorSprite.transform.position = new Vector3(2.5f, -0.45f);
                BoxCollider doorSpriteCollider = doorSprite.AddComponent<BoxCollider>();
                doorSpriteCollider.size = new Vector3(2f, 1f);
                SpriteRenderer doorSpriteRenderer = doorSprite.AddComponent<SpriteRenderer>();
                doorSpriteRenderer.sprite = resourceSprites["door"];
                doorSpriteRenderer.transform.localScale = new Vector3(2f, 1f);

                Interactable doorInteractable = door.AddComponent<Interactable>();
                doorInteractable.TypeOfInteraction = InteractionType.Door;
                doorInteractable.OnInteracted.AddListener(x =>
                {
                    foreach (CustomApparel item in lockedItems)
                    {
                        item.IsLocked = true;
                    }
                    LevelTransition.Instance.ToLevel("Carceburg");
                });

                GameObject andrNPC = new GameObject();
                andrNPC.transform.position = new Vector3(5f, 0.2f);
                BoxCollider andrNPCCollider = andrNPC.AddComponent<BoxCollider>();
                andrNPCCollider.size = new Vector3(1f, 1f);
                GameObject andrNPCSprite = new GameObject();
                andrNPCSprite.transform.position = new Vector3(4.2f, 0f);
                SpriteRenderer andrNPCSpriteRenderer = andrNPCSprite.AddComponent<SpriteRenderer>();
                andrNPCSpriteRenderer.sprite = resourceSprites["ANDR-047139_Overworld"];
                andrNPCSpriteRenderer.transform.localScale = new Vector3(1f, 1f);

                Interactable andrNPCInteractable = andrNPC.AddComponent<Interactable>();
                andrNPCInteractable.TypeOfInteraction = InteractionType.Talk;
                andrNPCInteractable.OnInteracted.AddListener(x =>
                {
                    vendor.Catalogue.OpenShop();
                });
            } 
            else if (newLevel == "Carceburg")
            {
                GameObject interactableGameObject = new GameObject();
                interactableGameObject.transform.position = new Vector3(8f, -18f);
                BoxCollider boxCollider = interactableGameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1f, 1f);

                Interactable interactable = interactableGameObject.AddComponent<Interactable>();
                interactable.TypeOfInteraction = InteractionType.Talk;
                interactable.OnInteracted.AddListener(x =>
                {
                    if (DNCTime.Instance.time.Hours > 20)
                {
                        bool isTalkedToANDR = SaveManager.GetKey("isTalkedToANDR", false);
                        if (!isTalkedToANDR)
                        {
                            DialogueManager.StartDialogue(dialogues[1]);
                            SaveManager.SetKey("isTalkedToANDR", true);
                        }
                        else
                        {
                            SaveManager.SetKey("inAndrNPCRoom", true);
                            LevelTransition.Instance.ToLevel(baseCustomShopLevel);
                        }
                    }
                    else
                    {
                        DialogueManager.StartDialogue(dialogues[0]);
                    }
                });
            }
            Debug.Log("Modded OnLevelChanged");
        }

        public void OnLineStarted(DialogueLine line)
        {
            //
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
                            Cost = item.Price,
                        }
                    );
                }
                ItemShopCatalogue catalogue = ScriptableObject.CreateInstance<ItemShopCatalogue>();
                catalogue.Items = shopItems;

                Dialogue dialogueDay = DialogueIngameEditor.LoadDialogue(Path.Combine(manifest.ModPath, "dialogue\\ANDR_day.dialogue"));
                Dialogue dialogueFirstConvo = DialogueIngameEditor.LoadDialogue(Path.Combine(manifest.ModPath, "dialogue\\ANDR_first_convo.dialogue"));

                ANResourceSprite resource = manifest.SpriteResolver.ResolveAsResource("assets\\sprites\\ANDR-047139_Sprite.png");
                foreach (DialogueLine line in dialogueFirstConvo.Lines)
                {
                    if (line.NameOverride == "A" || line.NameOverride == "Anonymous Person") {
                        line.BackgroundSpriteResource = resource;
                    };
                }
                dialogues.Add(dialogueDay);
                dialogues.Add(dialogueFirstConvo);

                vendor = new ItemVendor()
                {
                    Catalogue = catalogue,
                };
                LoadAllResources(manifest);
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

        private void LoadAllResources(ModManifest manifest)
        {
            resourceSprites.Add("ANDR-047139_Overworld", manifest.SpriteResolver.ResolveAsResource("assets\\sprites\\ANDR-047139_Overworld.png"));
            resourceSprites.Add("wall", manifest.SpriteResolver.ResolveAsResource("assets\\sprites\\room_wall.png"));
            resourceSprites.Add("door", manifest.SpriteResolver.ResolveAsResource("assets\\sprites\\door.png"));
        }
    }
}
