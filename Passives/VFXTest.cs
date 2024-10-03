using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using JetBrains.Annotations;

namespace TF2Stuff
{
    public class VFXTest : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "VFX";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "TF2Items/Resources/passives/example_item_sprite";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<VFXTest>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Pain";
            string longDesc = "This is going to be hard";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "qad");

            //Adds the actual passive effect to the item
            GameObject plagueVFXObject = SpriteBuilder.SpriteFromResource("TF2Items/Resources/OtherVFX/vfxtest1/vfxtest1_001", new GameObject("LeadSoulOverhead"));
            plagueVFXObject.SetActive(false);
            tk2dBaseSprite plaguevfxSprite = plagueVFXObject.GetOrAddComponent<tk2dBaseSprite>();
            plaguevfxSprite.GetCurrentSpriteDef().ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerCenter, plaguevfxSprite.GetCurrentSpriteDef().position3);
            FakePrefab.MarkAsFakePrefab(plagueVFXObject);
            UnityEngine.Object.DontDestroyOnLoad(plagueVFXObject);
            //Animating the overhead
            tk2dSpriteAnimator plagueanimator = plagueVFXObject.AddComponent<tk2dSpriteAnimator>();
            plagueanimator.Library = plagueVFXObject.AddComponent<tk2dSpriteAnimation>();
            plagueanimator.Library.clips = new tk2dSpriteAnimationClip[0];

            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip { name = "LeadSoulOverheadClip", fps = 10, frames = new tk2dSpriteAnimationFrame[0] };
            foreach (string path in VFXPaths)
            {
                int spriteId = SpriteBuilder.AddSpriteToCollection(path, plagueVFXObject.GetComponent<tk2dBaseSprite>().Collection);

                plagueVFXObject.GetComponent<tk2dBaseSprite>().Collection.spriteDefinitions[spriteId].ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerCenter);

                tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame { spriteId = spriteId, spriteCollection = plagueVFXObject.GetComponent<tk2dBaseSprite>().Collection };
                clip.frames = clip.frames.Concat(new tk2dSpriteAnimationFrame[] { frame }).ToArray();
            }

            plagueanimator.Library.clips = plagueanimator.Library.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            plagueanimator.playAutomatically = true;
            plagueanimator.DefaultClipId = plagueanimator.GetClipIdByName("LeadSoulOverheadClip");
            VFXPrefab = plagueVFXObject;
            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }
        public static GameObject VFXPrefab;
        private GameObject extantOverhead;
        public static List<string> VFXPaths = new List<string>()
        {
            "TF2Items/Resources/OtherVFX/vfxtest1/vfxtest1_001",
            "TF2Items/Resources/OtherVFX/vfxtest1/vfxtest1_002",
            "TF2Items/Resources/OtherVFX/vfxtest1/vfxtest1_003",
            "TF2Items/Resources/OtherVFX/vfxtest1/vfxtest1_004",
            "TF2Items/Resources/OtherVFX/vfxtest1/vfxtest1_005"
        };

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            AddOverhead();
        }

        public override DebrisObject Drop(PlayerController player)
        {
            ClearOverhead();
            return base.Drop(player);
        }
        private void AddOverhead()
        {
            if (VFXPrefab && Owner)
            {
                //AkSoundEngine.PostEvent("Play_OBJ_metalskin_deflect_01", Owner.gameObject);
                GameObject newSprite = Instantiate(VFXPrefab);
                newSprite.transform.parent = Owner.transform;
                newSprite.transform.position = (Owner.transform.position + new Vector3(0.7f, 2f));
                extantOverhead = newSprite;
                UnityEngine.Object.Instantiate<GameObject>((PickupObjectDatabase.GetById(37) as Gun).DefaultModule.chargeProjectiles[0].Projectile.hitEffects.overrideMidairDeathVFX, extantOverhead.GetComponent<tk2dBaseSprite>().WorldCenter, Quaternion.identity);
            }
        }
        private void ClearOverhead()
        {
            if (extantOverhead)
            {
                UnityEngine.Object.Instantiate<GameObject>((PickupObjectDatabase.GetById(37) as Gun).DefaultModule.chargeProjectiles[0].Projectile.hitEffects.overrideMidairDeathVFX, extantOverhead.GetComponent<tk2dBaseSprite>().WorldCenter, Quaternion.identity); ; ;
                Destroy(extantOverhead);
                extantOverhead = null;
            }
        }
    }
}