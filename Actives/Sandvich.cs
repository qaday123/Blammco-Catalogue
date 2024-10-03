/* NOTES:
 * debate on whether this should be an active or passive
 * Make stun effect scale based on time in air? how do
 * should this be cursed??
 * Casey syngergy - idk what it would do tho
*/
namespace TF2Stuff
{
    public class Sandvich : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Sandvich";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "TF2Items/Resources/actives/sandvich_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Sandvich>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 500f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Om Nom Nom";
            string longDesc = "Throw a tasty snack at your enemies. If the resulting blow kills them, heal up some damage. Otherwise, too bad! If you miss, pick it back up and " +
                "give it another shot. Read your enemies well!\n\nThere's nothing like eating a healthy, balanced combination of ingredients in the middle of an ensuing firefight.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");
            item.consumable = false;
            item.quality = PickupObject.ItemQuality.B;
            ID = item.PickupObjectId;

            sandwich = (PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0].InstantiateAndFakeprefab();
            sandwich.baseData.damage = 5f;
            sandwich.baseData.speed = 30f;
            sandwich.baseData.range = 2000f;
            sandwich.SetProjectileSpriteRight("sandvich_projectile", 16, 16, false, anchor: tk2dBaseSprite.Anchor.MiddleCenter, 14, 14);
            sandwich.pierceMinorBreakables = true;
            sandwich.collidesWithPlayer = true;
            sandwich.allowSelfShooting = true;

            List<VFXObject> vfxList = new();
            List<string> debrisFilePaths = CodeShortcuts.GenerateFilePaths("TF2Items/Resources/Debris/sandvich_debris/sandvich_debris_", 11);

            foreach (var d in BreakableAPIToolbox.GenerateDebrisObjects(debrisFilePaths.ToArray(), true, AngularVelocity: 0, AngularVelocityVariance: 60))
            {
                d.gameObject.GetComponent<tk2dBaseSprite>().CurrentSprite.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleCenter);
                vfxList.Add(new()
                {
                    attached = true,
                    persistsOnDeath = true,
                    usesZHeight = false,
                    zHeight = 0f,
                    alignment = VFXAlignment.VelocityAligned,
                    destructible = false,
                    orphaned = true,
                    effect = d.gameObject,
                });
            }

            VFXPool debrisImpact = new VFXPool()
            {
                type = VFXPoolType.All,
                effects = new VFXComplex[] 
                { new VFXComplex()
                    {
                        effects = vfxList.ToArray(),
                    }
                }
            };
            sandwich.hitEffects.HasProjectileDeathVFX = true;
            sandwich.hitEffects.deathEnemy = debrisImpact;

            var def = sandwich.sprite.CurrentSprite;
            var mats = new List<Material>() { def.material, def.materialInst };

            foreach (var mat in mats)
            {
                if (mat == null)
                    continue;
                mat.SetFloat("_EmissivePower", 0f);
                //mat.shader = ShaderCache.Acquire("tk2d/CutoutVertexColorTintable");
            }
            

            PickupableProjectile pickup = sandwich.gameObject.GetOrAddComponent<PickupableProjectile>();
            pickup.BecomesDebris = false;
            pickup.hasAfterImageEffect = false;
            pickup.DebrisTime = 10000f;
            pickup.spinDegreeRate = 540f;
            pickup.spinDecayRate = 0.9f;
            pickup.speedDecayRate = 0.3f;

            BounceProjModifier bounce = sandwich.gameObject.GetOrAddComponent<BounceProjModifier>();
            bounce.numberOfBounces += 4;
        }
        //private static Projectile ball; // this is causing the registered projectile to not work
        public static int ID;
        public static Projectile sandwich;
        public override void DoEffect(PlayerController user)
        {
            float angle = (user.CurrentGun == null) ? 0f : user.CurrentGun.CurrentAngle;
            //Vector2 offset = new(Mathf.Cos(angle * (Mathf.PI / 180)), Mathf.Sin(angle * (Mathf.PI / 180)));
            GameObject gameObject = SpawnManager.SpawnProjectile(sandwich.gameObject, user.sprite.WorldCenter, Quaternion.Euler(0f, 0f, angle), true);
            Projectile proj = gameObject.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Owner = user;
                proj.Shooter = user.specRigidbody;
                proj.OnHitEnemy += OnHitEnemy;
                proj.specRigidbody.OnTileCollision += HitWall;
            }
            PickupableProjectile pickup = gameObject.GetComponent<PickupableProjectile>();
            if (pickup)
            {
                pickup.OnPickup += OnPickup;
            }
        }
        public void HitWall(CollisionData data)
        {
            string[] bounceSounds = { "baseball_hitworld1", "baseball_hitworld2", "baseball_hitworld3" };
            AkSoundEngine.PostEvent(bounceSounds[UnityEngine.Random.Range(0, bounceSounds.Length)], base.gameObject);
        }
        public void OnHitEnemy(Projectile projectile, SpeculativeRigidbody enemy, bool fatal)
        {
            if (enemy != null && enemy.aiActor != null)
            {
                AkSoundEngine.PostEvent("Play_OBJ_pot_shatter_01", base.gameObject);
                if (fatal) LastOwner.healthHaver.ApplyHealingAdvanced(1f); // in CodeShortcuts.cs
            }
        }
        public void OnPickup()
        {
            if (this && this.gameObject)
            {
                AkSoundEngine.PostEvent("recharged", base.gameObject);
                ClearCooldowns();
            }
        }
    }
}