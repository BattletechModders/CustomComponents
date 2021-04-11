using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using HBS;

namespace CustomComponents
{
    public class HardpointController
    {
        private static HardpointController _instance;

        public static HardpointController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HardpointController();

                return _instance;
            }
        }

        private Dictionary<string, HardpointInfo> hardpoints = new Dictionary<string, HardpointInfo>();
        public List<HardpointInfo> Hardpoints;

        public void SetupDefaults()
        {
            var hp = new HardpointInfo()
            {
                ID = "Ballistic",
                Short = "B",
                DisplayName = "Ballistic",
                Visible = true,
            };
            hp.Complete();
            hardpoints["Ballistic"] = hp;

            hp = new HardpointInfo()
            {
                ID = "Energy",
                Short = "E",
                DisplayName = "Energy",
                Visible = true,
            };
            hp.Complete();
            hardpoints["Energy"] = hp;

            hp = new HardpointInfo()
            {
                ID = "Missile",
                Short = "M",
                DisplayName = "Missile",
                Visible = true,
            };
            hp.Complete();
            hardpoints["Missile"] = hp;

            hp = new HardpointInfo()
            {
                ID = "AntiPersonnel",
                Short = "S",
                DisplayName = "Support",
                Visible = true,
            };
            hp.Complete();
            hardpoints["AntiPersonnel"] = hp;
            
            hp = new HardpointInfo()
            {
                ID = "AMS",
                Short = "AM",
                DisplayName = "AMS",
                Visible = false,
            };
            hp.Complete();
            hardpoints["AMS"] = hp;
           
            hp = new HardpointInfo()
            {
                ID = "Melee",
                Short = "Ml",
                DisplayName = "Melee",
                Visible = false,
            };
            hp.Complete();
            hardpoints["Melee"] = hp;

        }

        public void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            SetupDefaults();

            foreach (var hp in SettingsResourcesTools.Enumerate<HardpointInfo>("CCHardpoints", customResources))
            {
                if (hp.Complete())
                {
                    Control.LogDebug(DType.Hardpoints, $"Hardpoint info: {hp.ID}, [{hp.Compatible.Aggregate((last,next) => last + " " + next)}]");
                    hardpoints[hp.ID] = hp;
                }
            }

            Hardpoints = hardpoints.Values.OrderBy(i => i.Compatible.Length).ToList();
        }

        public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
        {
            var lhepler = MechLabHelper.CurrentMechLab.GetLocationHelper(location);

            return string.Empty;
        }

        public string ReplaceValidatorDrop(MechLabItemSlotElement drop_item, ChassisLocations location, Queue<IChange> changes)
        {
        }

        public string PostValidatorDrop(MechLabItemSlotElement drop_item, MechDef mech, List<InvItem> new_inventory, List<IChange> changes)
        {
        }
    }
}