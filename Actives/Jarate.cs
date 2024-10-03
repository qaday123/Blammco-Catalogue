using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Collections;
using Dungeonator;
using Gungeon;

namespace TF2Stuff
{
    public class Jarate : SpawnObjectPlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Bootleg Jarate";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "TF2Items/Resources/ThrowableActives/jarate_icon"; // make sprite

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Jarate>();

            // Cooldown type
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 100f);

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            /*string shortDesc = "But... Why?";
            string longDesc = "Throws a jar of piss at your enemies... pissed on enemies will take a lot more damage... for some reason.\n\n"+
                "Nobody understands why sniper keeps jars of piss to throw at his enemies. It was a good spy catcher at first, but now it " +
                "just seems cruel to use it on everybody, including the gundead. Either way, it's part of the arsenal of the gungeon now, " +
                "so it's probably just a good idea to make use of it.";*/
            string shortDesc = "Worse Than The Worst";
            string longDesc = "Throws a jar of piss at your enemies.\n\nA race was initiated between two gungeon engineers to see who could make " +
                "a bottle of piss the fastest. Well actually, only one of the engineers was racing, since the other, more competent one was completely " +
                "oblivious of his endeavors. Well... this won, so I suppose it must be alright?\nThanks Nevernamed. :)";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            item.consumable = false;
            item.objectToSpawn = Jarate.BuildPrefab();
            item.tossForce = 10f;
            item.canBounce = false;
            item.IsCigarettes = false;
            item.RequireEnemiesInRoom = false;
            item.SpawnRadialCopies = false;
            item.RadialCopiesToSpawn = 0;
            item.AudioEvent = null;
            item.IsKageBunshinItem = false;
            item.quality = PickupObject.ItemQuality.D;
            ID = item.PickupObjectId;
        }
        public static int ID;        

        public static GameObject BuildPrefab()
        {
            GameObject gameObject = SpriteBuilder.SpriteFromResource("TF2Items/Resources/ThrowableActives/jarate_spin_001.png", new GameObject("Lvl2Molotov"));
            gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            tk2dSpriteAnimator tk2dSpriteAnimator = gameObject.AddComponent<tk2dSpriteAnimator>();
            tk2dSpriteCollectionData spriteCollection = (PickupObjectDatabase.GetById(108) as SpawnObjectPlayerItem).objectToSpawn.GetComponent<tk2dSpriteAnimator>().Library.clips[0].frames[0].spriteCollection;
            tk2dSpriteAnimationClip tk2dSpriteAnimationClip = SpriteBuilder.AddAnimation(tk2dSpriteAnimator, spriteCollection, new List<int>
            {
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_spin_004.png", spriteCollection)
            }, "jarate_throw", tk2dSpriteAnimationClip.WrapMode.Once);
            tk2dSpriteAnimationClip.fps = 12f;
            foreach (tk2dSpriteAnimationFrame tk2dSpriteAnimationFrame in tk2dSpriteAnimationClip.frames)
            {
                tk2dSpriteAnimationFrame.eventLerpEmissiveTime = 0.5f;
                tk2dSpriteAnimationFrame.eventLerpEmissivePower = 30f;
            }
            tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = SpriteBuilder.AddAnimation(tk2dSpriteAnimator, spriteCollection, new List<int>
            {
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_burst_001.png", spriteCollection),
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_burst_002.png", spriteCollection),
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_burst_003.png", spriteCollection),
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_burst_004.png", spriteCollection)
            }, "jarate_burst", tk2dSpriteAnimationClip.WrapMode.Once);
            tk2dSpriteAnimationClip2.fps = 16f;
            foreach (tk2dSpriteAnimationFrame tk2dSpriteAnimationFrame2 in tk2dSpriteAnimationClip2.frames)
            {
                tk2dSpriteAnimationFrame2.eventLerpEmissiveTime = 0.5f;
                tk2dSpriteAnimationFrame2.eventLerpEmissivePower = 30f;
            }
            tk2dSpriteAnimationClip tk2dSpriteAnimationClip3 = SpriteBuilder.AddAnimation(tk2dSpriteAnimator, spriteCollection, new List<int>
            {
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_spin_001.png", spriteCollection),
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_spin_002.png", spriteCollection),
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_spin_003.png", spriteCollection),
                SpriteBuilder.AddSpriteToCollection("TF2Items/Resources/ThrowableActives/jarate_spin_004.png", spriteCollection)
            }, "jarate", tk2dSpriteAnimationClip.WrapMode.LoopSection);
            tk2dSpriteAnimationClip3.fps = 10f;
            tk2dSpriteAnimationClip3.loopStart = 0;
            foreach (tk2dSpriteAnimationFrame tk2dSpriteAnimationFrame3 in tk2dSpriteAnimationClip3.frames)
            {
                tk2dSpriteAnimationFrame3.eventLerpEmissiveTime = 0.5f;
                tk2dSpriteAnimationFrame3.eventLerpEmissivePower = 30f;
            }
            CustomThrowableObject customThrowableObject = new CustomThrowableObject
            {
                doEffectOnHitGround = true,
                OnThrownAnimation = "jarate_throw",
                OnHitGroundAnimation = "jarate_burst",
                DefaultAnim = "jarate",
                destroyOnHitGround = false,
                thrownSoundEffect = "Play_OBJ_item_throw_01",
                effectSoundEffect = "Play_OBJ_glassbottle_shatter_01"
            };
            SpriteBuilder.AddComponent<CustomThrowableObject>(gameObject, customThrowableObject);
            gameObject.AddComponent<Jarate.JarateEffect>();
            return gameObject;
        }
        public class JarateEffect : CustomThrowableEffectDoer
        {
            public override void OnEffect(GameObject obj)
            {
                DeadlyDeadlyGoopManager goop = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(CustomGoops.JarateGoop);
                // A Null Exception occurs here but I have no idea why
                goop.TimedAddGoopCircle(obj.transform.position + new Vector3(14f/16f,14f/16f), 3.25f, 0.75f, true);
                RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
                List<AIActor> enemiesInRoom = new List<AIActor>();

                if (absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear) != null)
                {
                    foreach (AIActor m_Enemy in absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
                    {
                        enemiesInRoom.Add(m_Enemy);
                    }
                }
                if (enemiesInRoom != null)
                {
                    for (int i = 0; i < enemiesInRoom.Count; i++)
                    {
                        AIActor enemy = enemiesInRoom[i];
                        if (Vector2.Distance(enemy.CenterPosition, obj.transform.position + new Vector3(14f / 16f, 14f / 16f)) <= 3.25)
                        {
                            enemy.ApplyEffect(StaticStatusEffects.StandardJarateEffect);
                        }
                        //enemy.ApplyEffect(StaticStatusEffects.StandardJarateEffect);
                    }
                }
                StartCoroutine(Kill(obj));
            }
            private IEnumerator Kill(GameObject obj)
            {
                yield return new WaitForSeconds(0.25f);
                UnityEngine.Object.Destroy(obj);
                yield break;
            }
        }
    }
}