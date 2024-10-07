using TF2Stuff;

namespace TF2Stuff
{
    public class PassiveTemplate : PassiveItem
    {
        //Call this method from the GMStart() method of your ETGModule extension
        public static void Register()
        {
            string itemName = "";
            string resourceName = ""; // sprite path starting with [Namespace]/
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<Gunboats>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "";
            string longDesc = "";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            ID = item.PickupObjectId;
            item.quality = PickupObject.ItemQuality.D;
        }
        public static int ID;
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
        }
    }
}
