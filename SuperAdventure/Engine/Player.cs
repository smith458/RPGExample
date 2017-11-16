using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Player
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        public int Level
        {
            get
            {
                return (ExperiencePoints / 100) + 1;
            }
        }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }
        public int CurrentHitPoints { get; set; }
        public int MaximumHitPoints
        {
            get
            {
                return (Level + 1) * 5;
            }
        }
        
        public Player(int gold, int experiencePoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            CurrentHitPoints = MaximumHitPoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                return true;
            }

            return Inventory.Exists(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
        }

        public bool HasThisQuest(Quest quest)
        {
            return Quests.Exists(q => q.Details.ID == quest.ID);
        }

        public bool CompletedThisQuest(Quest quest)
        {
            if (HasThisQuest(quest))
            {
                return Quests.First(q => q.Details.ID == quest.ID).IsCompleted;
            }

            return false;
        }

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                //Return false if the player doesn't have the item or if they have the item, but not enough
                if (!Inventory.Exists(ii => ii.Details.ID == qci.Details.ID && ii.Quantity > qci.Quantity))
                {
                    return false;
                }
            }

            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);
                
                if (item != null)
                {
                    //Subtract quest quantity from inventory
                    item.Quantity -= qci.Quantity;
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
            {
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
            else
            {
                item.Quantity++;
            }
        }

        public void MarkQuestCompleted(Quest quest)
        {
            PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);

            if (playerQuest != null)
            {
                playerQuest.IsCompleted = true;
            }
        }
    }
}
