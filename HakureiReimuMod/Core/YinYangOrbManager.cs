using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class YinYangOrbManager
    {
        public int Capacity = 7;
        public List<YinYangOrb> Orbs { get; private set; }
        public Player Player{get; private set;}

        public YinYangOrbManager(Player player)
        {
            Player = player;
            Orbs = new List<YinYangOrb>(Capacity);
        }
        public void Clear()=> Orbs.Clear();

        public void Add(YinYangOrb orb)
        {
            Orbs.Add(orb);
        }
    }
}