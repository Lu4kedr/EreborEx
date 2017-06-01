﻿using Phoenix;
using Phoenix.WorldData;
using System;
using System.Xml.Serialization;

namespace EreborPhoenixExtension.Libs.Skills
{
    [Serializable]
    public class Poisoning
    {
        [XmlIgnore]
        private UOItem poisonBottle = null;
        public uint PoisonBottle
        {
            get
            {
                return poisonBottle==null? 0:(uint)poisonBottle.Serial;
            }
            set
            {
                poisonBottle = new UOItem(value);
            }
        }

        public void pois()
        {
            if (poisonBottle == null || poisonBottle.Serial == 0xFFFFFFFF || poisonBottle.Serial == 0x00) UO.PrintError("Nemas nastaveny poison");
            UOItem zbran = World.Player.Layers[Layer.RightHand];

            UOItem pois = World.Player.Backpack.Items.FindType(poisonBottle.Graphic, poisonBottle.Color);

            zbran.WaitTarget();
            pois.Use();
        }


    }

}
