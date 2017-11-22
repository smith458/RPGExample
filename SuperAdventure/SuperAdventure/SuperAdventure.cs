using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        private const String PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public SuperAdventure()
        {
            InitializeComponent();

            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                _player = Player.CreateDefaultPlayer();
            }
            
            MoveTo(_player.CurrentLocation);
            
            UpdatePlayerStatsUI();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            //Can we move to the locaiton
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                WriteToRtb(rtbMessages, "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine);
                return;
            }
            
            // Update the player's current location
            _player.CurrentLocation = newLocation;

            // Show/hide available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // Display current location name and description
            rtbLocations.Text = newLocation.Name + Environment.NewLine;
            rtbLocations.Text += newLocation.Description + Environment.NewLine;

            // Completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            // Update Hit Points in UI
            UpdatePlayerStatsUI();

            // Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                // See if the player already has the quest, and if they've completed it
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);
                
                // See if the player already has the quest
                if (playerAlreadyHasQuest)
                {
                    // If the player has not completed the quest yet
                    if (!playerAlreadyCompletedQuest)
                    {
                        // See if the player has all the items needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        // The player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            // Display message
                            WriteToRtb(rtbMessages, Environment.NewLine);
                            WriteToRtb(rtbMessages, "You complete the '" + newLocation.QuestAvailableHere.Name + "' quest." + Environment.NewLine);

                            // Remove quest items from inventory
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // Give quest rewards
                            WriteToRtb(rtbMessages, "You receive: " + Environment.NewLine);
                            WriteToRtb(rtbMessages, newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine);
                            WriteToRtb(rtbMessages, newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine);
                            WriteToRtb(rtbMessages, newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine);
                            WriteToRtb(rtbMessages, Environment.NewLine);

                            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            // Add the reward item to the player's inventory
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // Mark the quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);

                            UpdatePlayerStatsUI();
                            
                        }
                    }
                }
                else
                {
                    // The player does not already have the quest

                    // Display the messages
                    WriteToRtb(rtbMessages, "You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine);
                    WriteToRtb(rtbMessages, Environment.NewLine);
                    WriteToRtb(rtbMessages, newLocation.QuestAvailableHere.Description + Environment.NewLine);
                    WriteToRtb(rtbMessages, Environment.NewLine);
                    WriteToRtb(rtbMessages, "To complete it, return with:" + Environment.NewLine);
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            WriteToRtb(rtbMessages, qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine);
                        }
                        else
                        {
                            WriteToRtb(rtbMessages, qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine);
                        }
                    }
                    WriteToRtb(rtbMessages, Environment.NewLine);

                    // Add the quest to the player's quest list
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                WriteToRtb(rtbMessages, "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine);

                // Make a new monster, using the values from the standard monster in the World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage,
                    standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                SetItemControlsVisibility(true);
            }
            else
            {
                _currentMonster = null;

                SetItemControlsVisibility(false);
            }

            // Refresh player's inventory list
            UpdateInventoryListInUI();

            // Refresh player's quest list
            UpdateQuestListInUI();

            // Refresh player's weapons combobox
            UpdateItemOptionsInUI<Weapon>(cboWeapons, btnUseWeapon, _player.CurrentWeapon);

            // Refresh player's potions combobox
            UpdateItemOptionsInUI<HealingPotion>(cboPotions, btnUsePotion);
        }

        private void UpdateInventoryListInUI()
        {
            SetupUIList(dgvInventory, "Quantity");

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { ii.Details.Name, ii.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            SetupUIList(dgvQuests, "Done?");

            foreach (PlayerQuest pq in _player.Quests)
            { 
                dgvQuests.Rows.Add(new[] { pq.Details.Name, pq.IsCompleted.ToString() });
            }
        }

        private void SetupUIList(DataGridView grid, string value)
        {
            grid.RowHeadersVisible = false;

            grid.ColumnCount = 2;
            grid.Columns[0].Name = "Name";
            grid.Columns[0].Width = 197;
            grid.Columns[1].Name = value;

            grid.Rows.Clear();
        }

        private void UpdateItemOptionsInUI<T>(ComboBox cb, Button b, T selected = null) where T: Item
        {
            List<T> item = new List<T>();

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details is T)
                {
                    if (ii.Quantity > 0)
                    {
                        item.Add((T)ii.Details);
                    }
                }
            }

            if (item.Count == 0)
            {
                cb.Visible = false;
                b.Visible = false;
            }
            else
            {
                cb.DataSource = item;
                cb.DisplayMember = "Name";
                cb.ValueMember = "ID";
                if (selected != null)
                {
                    cb.SelectedItem = selected;
                }

                cb.SelectedIndex = 0;
            }
        }

        private void SetItemControlsVisibility(bool status)
        {
            cboWeapons.Visible   = status;
            cboPotions.Visible   = status;
            btnUseWeapon.Visible = status;
            btnUsePotion.Visible = status;
        }

        private void WriteToRtb(RichTextBox rtb, string message)
        {
            rtb.Text += message;
            rtb.SelectionStart = rtb.Text.Length;
            rtb.ScrollToCaret();
        }

        public void UpdatePlayerStatsUI()
        {
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            //Get the currently selected item
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            //Determine the amount of damage to do
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            //Apply the damage to the monster's current hit points
            _currentMonster.CurrentHitPoints -= damageToMonster;

            //Display message
            WriteToRtb(rtbMessages, "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " points." + Environment.NewLine);

            //Check is monster is dead
            if (_currentMonster.CurrentHitPoints <= 0)
            {
                //Monster is dead
                WriteToRtb(rtbMessages, Environment.NewLine);
                WriteToRtb(rtbMessages, "You defeated the " + _currentMonster.Name + Environment.NewLine);

                //Give player experience for killing the monster
                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                WriteToRtb(rtbMessages, "You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points." + Environment.NewLine);

                //Give player gold
                _player.Gold += _currentMonster.RewardGold;
                WriteToRtb(rtbMessages, "You receive " + _currentMonster.RewardGold + " gold." + Environment.NewLine);

                //Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                //Add items to the looted list
                foreach (LootItem loot in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= loot.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(loot.Details, 1));
                    }
                }


                //If no item is randomly selected
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem loot in _currentMonster.LootTable)
                    {
                        if (loot.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(loot.Details, 1));
                        }
                    }
                }

                //Add the looted items to player's inventory
                foreach (InventoryItem item in lootedItems)
                {
                    _player.AddItemToInventory(item.Details);

                    if (item.Quantity == 1)
                    {
                        WriteToRtb(rtbMessages, "You loot " + item.Quantity.ToString() + " " + item.Details.Name + Environment.NewLine);
                    }
                    else
                    {
                        WriteToRtb(rtbMessages, "You loot " + item.Quantity.ToString() + " " + item.Details.NamePlural + Environment.NewLine);
                    }
                }

                //Refresh player information
                UpdatePlayerStatsUI();

                UpdateInventoryListInUI();
                UpdateItemOptionsInUI<Weapon>(cboWeapons, btnUseWeapon, _player.CurrentWeapon);
                UpdateItemOptionsInUI<HealingPotion>(cboPotions, btnUsePotion);

                // Add a blank line to the messages box, just for appearance.
                WriteToRtb(rtbMessages, Environment.NewLine);

                // Move player to current location (to heal player and create a new monster to fight)
                MoveTo(_player.CurrentLocation);
            }
            else
            {
                //Monster is still alive

                //Determine the amount of damage the monster does
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                //Display message
                WriteToRtb(rtbMessages, "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine);

                //Subtract damage from player
                _player.CurrentHitPoints -= damageToPlayer;

                //Refresh player data in UI
                UpdatePlayerStatsUI();

                if (_player.CurrentHitPoints <= 0)
                {
                    //Display message
                    WriteToRtb(rtbMessages, "The " + _currentMonster.Name + " killed you." + Environment.NewLine);

                    //Move player home
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
            
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            // Add healing amount to the player's current hit points
            _player.CurrentHitPoints = (_player.CurrentHitPoints + potion.AmountToHeal);

            // CurrentHitPoints cannot exceed player's MaximumHitPoints
            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            // Remove the potion from the player's inventory
            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            // Display message
            WriteToRtb(rtbMessages, "You drink a " + potion.Name + Environment.NewLine);

            // Monster gets their turn to attack

            // Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            // Display message
            WriteToRtb(rtbMessages, "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine);

            // Subtract damage from player
            _player.CurrentHitPoints -= damageToPlayer;

            if (_player.CurrentHitPoints <= 0)
            {
                // Display message
                WriteToRtb(rtbMessages, "The " + _currentMonster.Name + " killed you." + Environment.NewLine);

                // Move player to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // Refresh player data in UI
            UpdatePlayerStatsUI();
            UpdateInventoryListInUI();
            UpdateItemOptionsInUI<HealingPotion>(cboPotions, btnUsePotion);
            Console.WriteLine();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }

        private void cboWeapons_SelectedIndexChanges(object sender, EventArgs e)
        {
            _player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }
    }
}