using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class MonsterMaker
    {
        public static Monster Create(int id)
        {
            Monster _monster;

            switch (id)
            {
                case World.MONSTER_ID_RAT:
                    _monster = new Monster(World.MONSTER_ID_RAT, "Rat", 5, 3, 10, 3, 3);
                    _monster.LootTable.Add(new LootItem(World.ItemByID(World.ITEM_ID_RAT_TAIL), 75, false));
                    _monster.LootTable.Add(new LootItem(World.ItemByID(World.ITEM_ID_PIECE_OF_FUR), 75, true));
                    break;
                case World.MONSTER_ID_SNAKE:
                    _monster = new Monster(World.MONSTER_ID_SNAKE, "Snake", 5, 3, 10, 3, 3);
                    _monster.LootTable.Add(new LootItem(World.ItemByID(World.ITEM_ID_SNAKE_FANG), 75, false));
                    _monster.LootTable.Add(new LootItem(World.ItemByID(World.ITEM_ID_SNAKESKIN), 75, true));
                    break;
                case World.MONSTER_ID_GIANT_SPIDER:
                    _monster = new Monster(World.MONSTER_ID_GIANT_SPIDER, "Giant spider", 20, 5, 40, 10, 10);
                    _monster.LootTable.Add(new LootItem(World.ItemByID(World.ITEM_ID_SPIDER_FANG), 75, true));
                    _monster.LootTable.Add(new LootItem(World.ItemByID(World.ITEM_ID_SPIDER_SILK), 25, false));
                    break;
                default:
                    throw new ArgumentException();
            }

            return _monster;
        }
    }
}
