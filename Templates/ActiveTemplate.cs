namespace TF2Stuff
{
    public class ActiveTemplate : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            string itemName = "";
            string resourceName = ""; // file path starting with [Namespace]/
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<ActiveTemplate>();

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 500f);
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "";
            string longDesc = "";

            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false; // whether item disappears after use or goes into cooldown
            item.quality = PickupObject.ItemQuality.D;
            ID = item.PickupObjectId;
        }
        public static int ID;
        public override void DoEffect(PlayerController user)
        {
            
        }
    }
}