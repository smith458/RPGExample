using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Engine.Test
{
    [TestFixture]
    public class When_Serializing_Deserializing
    {
        [Test]
        public void Default_Player()
        {
            Player _player = Player.CreateDefaultPlayer();
            string _playerString = _player.ToXmlString();
            Player _newPlayer = Player.CreatePlayerFromXmlString(_playerString);

            Assert.AreEqual(_player.ToXmlString(), _newPlayer.ToXmlString());
        }

        [Test]
        public void Player_With_Quests()
        {
            Player _player = Player.CreateDefaultPlayer();
            _player.Quests.Add(new PlayerQuest(World.QuestByID(1)));

            string _playerString = _player.ToXmlString();
            Player _newPlayer = Player.CreatePlayerFromXmlString(_playerString);

            Assert.AreEqual(_player.ToXmlString(), _newPlayer.ToXmlString());
        }

    }
}
