﻿using Phoenix;
using Phoenix.Communication;
using Phoenix.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace EreborPhoenixExtension.Libs.Skills
{
    public delegate void Track(int choice, bool war);
    [Serializable]
    public class Tracking
    {
        [XmlArray]
        public List<string> Tracked { get; set; }
        [XmlArray]
        public List<string> Ignored { get; set; }

        bool first = true;
        uint dialogID;
        ushort menuid;
        ushort fake = 0;



        public Tracking()
        {
            Tracked = new List<string>();
            Ignored = new List<string>();
        }


        public void fillListBox(ListBox lb)
        {
            lb.Items.Clear();
            foreach (string e in Ignored)
            {
                lb.Items.Add(e);
            }

        }
        public void Add()
        {
            UOCharacter ch = new UOCharacter(UIManager.TargetObject());
            Ignored.Add(ch.Name);
        }
        public void Add(string Name)
        {
            if (Name == "NULL") return;
            string[] tmp = Name.Split('_');
            foreach (string s in tmp)
            {
                Ignored.Add(s);
            }
        }

        public void Remove(int index)
        {
            if (index >= 0 && index >= Ignored.Count) return;
            Ignored.RemoveAt(index);

        }

        public void Track(string var)
        {
            UO.PrintError("Spatny parametr");
            Track();
        }
        public void Track()
        {
            UO.PrintInformation("track vypisuje pouze jmena v okoli, tzn. 3x Srnec se vypise pouze jednou");
            UO.PrintInformation(",track 0  vypise vsechno v okoli");
            UO.PrintInformation(",track 1  vypise Animals");
            UO.PrintInformation(",track 2  vypise Players");
            UO.PrintInformation(",track 3  vypise Monsters");
            UO.PrintInformation(",track 4  vypise Humans");
            UO.PrintInformation(",track X true   po tracku atackne laststatus");
            UO.PrintInformation("pres GUI, nebo ,add \"jmeno\" lze pridat hrace do ignore listu");
        }

        public void Track(int choice, bool war)
        {
            Track(choice);
            UO.Attack(Aliases.GetObject("laststatus"));
        }
        public void Track(int choice)
        {
            Tracked.Clear();
            string choosed = "";
            switch (choice)
            {
                case 0:
                    choosed = "Anything that moves";
                    break;
                case 1:
                    choosed = "Animals";
                    break;
                case 2:
                    choosed = "Players";
                    break;
                case 3:
                    choosed = "Monsters";
                    break;
                case 4:
                    choosed = "Humans";
                    break;
            }
            if (choosed.Equals("") || choice < 0 || choice > 4)
            {
                UO.PrintError("Spatny parametr");
                UO.Say(",track");
                return;
            }
            first = true;
            UO.Warmode(false);
            Core.UnregisterServerMessageCallback(0x7C, onMenu);
            UO.WaitMenu("Tracking", choosed);
            Core.RegisterServerMessageCallback(0x7C, onMenu);
            UO.UseSkill(StandardSkill.Tracking);
        }

        private void printTrackList()
        {
            Tracked = Tracked.Distinct().ToList();
            foreach (string s in Tracked)
            {
                if (Ignored.Contains(s)) continue;
                UO.PrintError(s);
            }
        }

        CallbackResult onMenu(byte[] data, CallbackResult prevResult)
        {
            if (first)
            {
                first = false;
                return CallbackResult.Normal;
            }
            PacketReader pr = new PacketReader(data);

            pr.Skip(3);
            dialogID = pr.ReadUInt32();
            menuid = pr.ReadUInt16();
            int length = pr.ReadByte();
            pr.Skip(length);
            int responses = pr.ReadByte();
            for (int i = 0; i < responses; i++)
            {
                pr.Skip(4);
                length = pr.ReadByte();
                Tracked.Add(pr.ReadAnsiString(length));
            }
            printTrackList();
            Core.UnregisterServerMessageCallback(0x7C, onMenu);
            PacketWriter pw = new PacketWriter(0x7D);
            pw.Write(dialogID);
            pw.Write(menuid);
            pw.Write(fake);
            pw.Write(fake);
            pw.Write(fake);
            Core.SendToServer(pw.GetBytes());
            Core.SendToClient(pw.GetBytes());

            dialogID = 0;
            menuid = 0;
            return CallbackResult.Eat;
        }
    }
}
