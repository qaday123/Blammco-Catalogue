using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;

namespace ExampleMod
{
    public class SynergyForms
    {
        public static void AddSynergyFormes()
        {
            //AddSynergyForm(26, Nailgun.ID, "What Could Have Been");
        }
        public static void AddSynergyForm(int baseGun, int newGun, string synergy)
        {
            AdvancedTransformGunSynergyProcessor forme = (PickupObjectDatabase.GetById(baseGun) as Gun).gameObject.AddComponent<AdvancedTransformGunSynergyProcessor>();
            forme.NonSynergyGunId = baseGun;
            forme.SynergyGunId = newGun;
            forme.SynergyToCheck = synergy;
        }
        public static void AddDualWield(int gun1, int gun2, string synergy)
        {
            AdvancedDualWieldSynergyProcessor gun1DUAL = (PickupObjectDatabase.GetById(gun1) as Gun).gameObject.AddComponent<AdvancedDualWieldSynergyProcessor>();
            gun1DUAL.PartnerGunID = gun2;
            gun1DUAL.SynergyNameToCheck = synergy;
            AdvancedDualWieldSynergyProcessor gun2DUAL = (PickupObjectDatabase.GetById(gun2) as Gun).gameObject.AddComponent<AdvancedDualWieldSynergyProcessor>();
            gun2DUAL.PartnerGunID = gun1;
            gun2DUAL.SynergyNameToCheck = synergy;
        }
    }
}
