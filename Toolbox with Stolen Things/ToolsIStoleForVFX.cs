using Alexandria.ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TF2Stuff
{
    class VFXToolbox
    {

        private static GameObject VFXScapeGoat;
#pragma warning disable IDE0044 // Add readonly modifier
        private static tk2dSpriteCollectionData PrivateVFXCollection;
#pragma warning restore IDE0044 // Add readonly modifier
        public static tk2dSpriteCollectionData VFXCollection
        {
            get
            {
                return PrivateVFXCollection;
            }
        }
        public static void InitVFX()
        {
            VFXScapeGoat = new GameObject();
            UnityEngine.Object.DontDestroyOnLoad(VFXScapeGoat);
            //PrivateVFXCollection = SpriteBuilder.ConstructCollection(VFXScapeGoat, "OMITBVFXCollection");


            laserSightPrefab = LoadHelper.LoadAssetFromAnywhere("assets/resourcesbundle/global vfx/vfx_lasersight.prefab") as GameObject;
        }
        public static GameObject laserSightPrefab;
        public static GameObject RenderLaserSight(Vector2 position, float length, float width, float angle, bool alterColour = false, Color? colour = null)
        {
            GameObject gameObject = SpawnManager.SpawnVFX(laserSightPrefab, position, Quaternion.Euler(0, 0, angle));

            tk2dTiledSprite component2 = gameObject.GetComponent<tk2dTiledSprite>();
            float newWidth = 1f;
            if (width != -1) newWidth = width;
            component2.dimensions = new Vector2(length, newWidth);
            if (alterColour && colour != null)
            {
                component2.usesOverrideMaterial = true;
                component2.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                component2.sprite.renderer.material.SetColor("_OverrideColor", (Color)colour);
                component2.sprite.renderer.material.SetColor("_EmissiveColor", (Color)colour);
                component2.sprite.renderer.material.SetFloat("_EmissivePower", 100);
                component2.sprite.renderer.material.SetFloat("_EmissiveColorPower", 1.55f);
            }
            return gameObject;
        }
        public static void GlitchScreenForSeconds(float seconds)
        {
            GameManager.Instance.StartCoroutine(DoScreenGlitch(seconds));
        }
        private static IEnumerator DoScreenGlitch(float seconds)
        {
            Material glitchPass = new Material(Shader.Find("Brave/Internal/GlitchUnlit"));
            Pixelator.Instance.RegisterAdditionalRenderPass(glitchPass);
            yield return new WaitForSeconds(seconds);
            Pixelator.Instance.DeregisterAdditionalRenderPass(glitchPass);
            yield break;
        }
        public static void DoStringSquirt(string text, Vector2 point, Color colour, float heightOffGround = 3f, float opacity = 1f)
        {

            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("DamagePopupLabel", ".prefab"), GameUIRoot.Instance.transform);

            dfLabel label = gameObject.GetComponent<dfLabel>();
            label.gameObject.SetActive(true);
            label.Text = text;
            label.Color = colour;
            label.Opacity = opacity;
            label.TextAlignment = TextAlignment.Center;

            label.transform.position = point;
            Vector2 point2 = new Vector2(label.transform.position.x - (label.GetCenter().x - label.transform.position.x), point.y);
            label.transform.position = label.transform.position.QuantizeFloor(label.PixelsToUnits() / (Pixelator.Instance.ScaleTileScale / Pixelator.Instance.CurrentTileScale));
            label.StartCoroutine(HandleDamageNumberCR(point2, point2.y - heightOffGround, label));
        }
        private static IEnumerator HandleDamageNumberCR(Vector3 startWorldPosition, float worldFloorHeight, dfLabel damageLabel)
        {
            float elapsed = 0f;
            float duration = 1.5f;
            float holdTime = 0f;
            Camera mainCam = GameManager.Instance.MainCameraController.Camera;
            Vector3 worldPosition = startWorldPosition;
            Vector3 lastVelocity = new Vector3(Mathf.Lerp(-8f, 8f, UnityEngine.Random.value), UnityEngine.Random.Range(15f, 25f), 0f);
            while (elapsed < duration)
            {
                float dt = BraveTime.DeltaTime;
                elapsed += dt;
                if (GameManager.Instance.IsPaused)
                {
                    break;
                }
                if (elapsed > holdTime)
                {
                    lastVelocity += new Vector3(0f, -50f, 0f) * dt;
                    Vector3 vector = lastVelocity * dt + worldPosition;
                    if (vector.y < worldFloorHeight)
                    {
                        float num = worldFloorHeight - vector.y;
                        float num2 = worldFloorHeight + num;
                        vector.y = num2 * 0.5f;
                        lastVelocity.y *= -0.5f;
                    }
                    worldPosition = vector;
                    damageLabel.transform.position = dfFollowObject.ConvertWorldSpaces(worldPosition, mainCam, GameUIRoot.Instance.Manager.RenderCamera).WithZ(0f);
                }
                float t = elapsed / duration;
                damageLabel.Opacity = 1f - t;
                yield return null;
            }
            damageLabel.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(damageLabel.gameObject, 1);
            yield break;
        }
        public static GameObject CreateOverheadVFX(List<string> filepaths, string name, int fps)
        {


            //Setting up the Overhead Plague VFX
            GameObject overheadderVFX = SpriteBuilder.SpriteFromResource(filepaths[0], new GameObject(name));
            overheadderVFX.SetActive(false);
            tk2dBaseSprite plaguevfxSprite = overheadderVFX.GetComponent<tk2dBaseSprite>();
            plaguevfxSprite.GetCurrentSpriteDef().ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerCenter, plaguevfxSprite.GetCurrentSpriteDef().position3);
            FakePrefab.MarkAsFakePrefab(overheadderVFX);
            UnityEngine.Object.DontDestroyOnLoad(overheadderVFX);

            //Animating the overhead
            tk2dSpriteAnimator plagueanimator = overheadderVFX.AddComponent<tk2dSpriteAnimator>();
            plagueanimator.Library = overheadderVFX.AddComponent<tk2dSpriteAnimation>();
            plagueanimator.Library.clips = new tk2dSpriteAnimationClip[0];

            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip { name = "NewOverheadVFX", fps = fps, frames = new tk2dSpriteAnimationFrame[0] };
            foreach (string path in filepaths)
            {
                int spriteId = SpriteBuilder.AddSpriteToCollection(path, overheadderVFX.GetComponent<tk2dBaseSprite>().Collection);

                overheadderVFX.GetComponent<tk2dBaseSprite>().Collection.spriteDefinitions[spriteId].ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerCenter);

                tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame { spriteId = spriteId, spriteCollection = overheadderVFX.GetComponent<tk2dBaseSprite>().Collection };
                clip.frames = clip.frames.Concat(new tk2dSpriteAnimationFrame[] { frame }).ToArray();
            }
            plagueanimator.Library.clips = plagueanimator.Library.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            plagueanimator.playAutomatically = true;
            plagueanimator.DefaultClipId = plagueanimator.GetClipIdByName("NewOverheadVFX");
            return overheadderVFX;
        }
        public static GameObject CreateVFX(string name, List<string> spritePaths, int fps, IntVector2 Dimensions, tk2dBaseSprite.Anchor anchor, bool usesZHeight, float zHeightOffset, float emissivePower = -1, Color? emissiveColour = null, tk2dSpriteAnimationClip.WrapMode wrap = tk2dSpriteAnimationClip.WrapMode.Once, bool persist = false)
        {
            GameObject Obj = new GameObject(name);
            VFXObject vfObj = new VFXObject();
            Obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(Obj);
            UnityEngine.Object.DontDestroyOnLoad(Obj);

            tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(Obj, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(spritePaths[0], VFXSpriteCollection);

            tk2dSprite sprite = Obj.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(VFXSpriteCollection, spriteID);
            tk2dSpriteDefinition defaultDef = sprite.GetCurrentSpriteDef();
            defaultDef.colliderVertices = new Vector3[]{
                      new Vector3(0f, 0f, 0f),
                      new Vector3((Dimensions.x / 16), (Dimensions.y / 16), 0f)
                  };

            tk2dSpriteAnimator animator = Obj.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = Obj.GetOrAddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = "start", frames = new tk2dSpriteAnimationFrame[0], fps = fps };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Count; i++)
            {
                tk2dSpriteCollectionData collection = VFXSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(anchor);
                frameDef.colliderVertices = defaultDef.colliderVertices;
                if (emissivePower > 0) frameDef.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.material.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.material.SetColor("_EmissiveColor", (Color)emissiveColour);
                if (emissivePower > 0) frameDef.materialInst.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.materialInst.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.materialInst.SetColor("_EmissiveColor", (Color)emissiveColour);
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            if (emissivePower > 0) sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
            if (emissivePower > 0) sprite.renderer.material.SetFloat("_EmissiveColorPower", emissivePower);
            if (emissiveColour != null) sprite.renderer.material.SetColor("_EmissiveColor", (Color)emissiveColour);
            clip.frames = frames.ToArray();
            clip.wrapMode = wrap;
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            if (!persist)
            {
                SpriteAnimatorKiller kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
                kill.fadeTime = -1f;
                kill.animator = animator;
                kill.delayDestructionTime = -1f;
            }
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("start");
            vfObj.attached = true;
            vfObj.persistsOnDeath = false;
            vfObj.usesZHeight = usesZHeight;
            vfObj.zHeight = zHeightOffset;
            vfObj.alignment = VFXAlignment.NormalAligned;
            vfObj.destructible = false;
            vfObj.effect = Obj;
            return Obj;
        }
        public static VFXComplex CreateVFXComplex(string name, List<string> spritePaths, int fps, IntVector2 Dimensions, tk2dBaseSprite.Anchor anchor, bool usesZHeight, float zHeightOffset, bool persist = false, float emissivePower = -1, Color? emissiveColour = null)
        {
            GameObject Obj = new GameObject(name);
            VFXPool pool = new VFXPool
            {
                type = VFXPoolType.All
            };
            VFXComplex complex = new VFXComplex();
            VFXObject vfObj = new VFXObject();
            Obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(Obj);
            UnityEngine.Object.DontDestroyOnLoad(Obj);

            tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(Obj, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(spritePaths[0], VFXSpriteCollection);

            tk2dSprite sprite = Obj.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(VFXSpriteCollection, spriteID);
            tk2dSpriteDefinition defaultDef = sprite.GetCurrentSpriteDef();
            defaultDef.colliderVertices = new Vector3[]{
                      new Vector3(0f, 0f, 0f),
                      new Vector3((Dimensions.x / 16), (Dimensions.y / 16), 0f)
                  };

            tk2dSpriteAnimator animator = Obj.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = Obj.GetOrAddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = "start", frames = new tk2dSpriteAnimationFrame[0], fps = fps };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Count; i++)
            {
                tk2dSpriteCollectionData collection = VFXSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(anchor);
                frameDef.colliderVertices = defaultDef.colliderVertices;
                if (emissivePower > 0) frameDef.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.material.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.material.SetColor("_EmissiveColor", (Color)emissiveColour);
                if (emissivePower > 0) frameDef.materialInst.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.materialInst.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.materialInst.SetColor("_EmissiveColor", (Color)emissiveColour);
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            if (emissivePower > 0) sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
            if (emissivePower > 0) sprite.renderer.material.SetFloat("_EmissiveColorPower", emissivePower);
            if (emissiveColour != null) sprite.renderer.material.SetColor("_EmissiveColor", (Color)emissiveColour);
            clip.frames = frames.ToArray();
            clip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            SpriteAnimatorKiller kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
            kill.fadeTime = -1f;
            kill.animator = animator;
            kill.delayDestructionTime = -1f;
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("start");
            vfObj.attached = true;
            vfObj.persistsOnDeath = persist;
            vfObj.usesZHeight = usesZHeight;
            vfObj.zHeight = zHeightOffset;
            vfObj.alignment = VFXAlignment.NormalAligned;
            vfObj.destructible = false;
            vfObj.effect = Obj;
            complex.effects = new VFXObject[] { vfObj };
            pool.effects = new VFXComplex[] { complex };
            return complex;
        }
        public static VFXPool CreateVFXPool(string name, List<string> spritePaths, int fps, IntVector2 Dimensions, tk2dBaseSprite.Anchor anchor, bool usesZHeight, float zHeightOffset, bool persist = false, VFXAlignment alignment = VFXAlignment.NormalAligned, float emissivePower = -1, Color? emissiveColour = null)
        {
            GameObject Obj = new GameObject(name);
            VFXPool pool = new VFXPool
            {
                type = VFXPoolType.All
            };
            VFXComplex complex = new VFXComplex();
            VFXObject vfObj = new VFXObject();
            Obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(Obj);
            UnityEngine.Object.DontDestroyOnLoad(Obj);

            tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(Obj, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(spritePaths[0], VFXSpriteCollection);

            tk2dSprite sprite = Obj.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(VFXSpriteCollection, spriteID);
            tk2dSpriteDefinition defaultDef = sprite.GetCurrentSpriteDef();
            defaultDef.colliderVertices = new Vector3[]{
                      new Vector3(0f, 0f, 0f),
                      new Vector3((Dimensions.x / 16), (Dimensions.y / 16), 0f)
                  };

            tk2dSpriteAnimator animator = Obj.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = Obj.GetOrAddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = "start", frames = new tk2dSpriteAnimationFrame[0], fps = fps };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Count; i++)
            {
                tk2dSpriteCollectionData collection = VFXSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(anchor);
                frameDef.colliderVertices = defaultDef.colliderVertices;
                if (emissivePower > 0) frameDef.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.material.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.material.SetColor("_EmissiveColor", (Color)emissiveColour);
                if (emissivePower > 0) frameDef.materialInst.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.materialInst.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.materialInst.SetColor("_EmissiveColor", (Color)emissiveColour);
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            if (emissivePower > 0) sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
            if (emissivePower > 0) sprite.renderer.material.SetFloat("_EmissiveColorPower", emissivePower);
            if (emissiveColour != null) sprite.renderer.material.SetColor("_EmissiveColor", (Color)emissiveColour);
            clip.frames = frames.ToArray();
            clip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            if (!persist)
            {
                SpriteAnimatorKiller kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
                kill.fadeTime = -1f;
                kill.animator = animator;
                kill.delayDestructionTime = -1f;
            }
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("start");
            vfObj.attached = true;
            vfObj.persistsOnDeath = persist;
            vfObj.usesZHeight = usesZHeight;
            vfObj.zHeight = zHeightOffset;
            vfObj.alignment = alignment;
            vfObj.destructible = false;
            vfObj.effect = Obj;
            complex.effects = new VFXObject[] { vfObj };
            pool.effects = new VFXComplex[] { complex };
            return pool;
        }
        public static VFXPool CreateMuzzleflash(string name, List<string> spriteNames, int fps, List<IntVector2> spriteSizes, List<tk2dBaseSprite.Anchor> anchors, List<Vector2> manualOffsets, bool orphaned, bool attached, bool persistsOnDeath,
            bool usesZHeight, float zHeight, VFXAlignment alignment, bool destructible, List<float> emissivePowers, List<Color> emissiveColors)
        {
            VFXPool pool = new VFXPool
            {
                type = VFXPoolType.All
            };
            VFXComplex complex = new VFXComplex();
            VFXObject vfObj = new VFXObject();
            GameObject obj = new GameObject(name);
            obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(obj);
            UnityEngine.Object.DontDestroyOnLoad(obj);
            tk2dSprite sprite = obj.AddComponent<tk2dSprite>();
            tk2dSpriteAnimator animator = obj.AddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip
            {
                fps = fps,
                frames = new tk2dSpriteAnimationFrame[0]
            };
            for (int i = 0; i < spriteNames.Count; i++)
            {
                string spriteName = spriteNames[i];
                IntVector2 spriteSize = spriteSizes[i];
                tk2dBaseSprite.Anchor anchor = anchors[i];
                Vector2 manualOffset = manualOffsets[i];
                float emissivePower = emissivePowers[i];
                Color emissiveColor = emissiveColors[i];
                tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame
                {
                    spriteId = VFXCollection.GetSpriteIdByName(spriteName)
                };
                tk2dSpriteDefinition def = VFXToolbox.SetupDefinitionForShellSprite(spriteName, frame.spriteId, spriteSize.x, spriteSize.y);
                def.ConstructOffsetsFromAnchor(anchor, def.position3);
                def.MakeOffset(manualOffset);
                def.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                def.material.SetFloat("_EmissiveColorPower", emissivePower);
                def.material.SetColor("_EmissiveColor", emissiveColor);
                def.materialInst.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                def.materialInst.SetFloat("_EmissiveColorPower", emissivePower);
                def.materialInst.SetColor("_EmissiveColor", emissiveColor);
                frame.spriteCollection = VFXCollection;
                clip.frames = clip.frames.Concat(new tk2dSpriteAnimationFrame[] { frame }).ToArray();
            }
            sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
            sprite.renderer.material.SetFloat("_EmissiveColorPower", emissivePowers[0]);
            sprite.renderer.material.SetColor("_EmissiveColor", emissiveColors[0]);
            clip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            clip.name = "start";
            animator.spriteAnimator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
            animator.spriteAnimator.Library.clips = new tk2dSpriteAnimationClip[] { clip };
            animator.spriteAnimator.Library.enabled = true;
            SpriteAnimatorKiller kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
            kill.fadeTime = -1f;
            kill.animator = animator;
            kill.delayDestructionTime = -1f;
            vfObj.orphaned = orphaned;
            vfObj.attached = attached;
            vfObj.persistsOnDeath = persistsOnDeath;
            vfObj.usesZHeight = usesZHeight;
            vfObj.zHeight = zHeight;
            vfObj.alignment = alignment;
            vfObj.destructible = destructible;
            vfObj.effect = obj;
            complex.effects = new VFXObject[] { vfObj };
            pool.effects = new VFXComplex[] { complex };
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("start");
            return pool;
        }
        public static GameObject CreateCustomClip(string spriteName, int pixelWidth, int pixelHeight)
        {
            GameObject clip = UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(95) as Gun).clipObject);
            clip.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(clip.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(clip);
            clip.GetComponent<tk2dBaseSprite>().spriteId = VFXCollection.inst.GetSpriteIdByName(spriteName);
            VFXToolbox.SetupDefinitionForClipSprite(spriteName, clip.GetComponent<tk2dBaseSprite>().spriteId, pixelWidth, pixelHeight);
            return clip;
        }
        public static void SetupDefinitionForClipSprite(string name, int id, int pixelWidth, int pixelHeight)
        {
            float thing = 14;
            float trueWidth = (float)pixelWidth / thing;
            float trueHeight = (float)pixelHeight / thing;
            tk2dSpriteDefinition def = VFXCollection.inst.spriteDefinitions[(PickupObjectDatabase.GetById(47) as Gun).clipObject.GetComponent<tk2dBaseSprite>().spriteId].CopyDefinitionFrom();
            def.boundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.boundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.untrimmedBoundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.untrimmedBoundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.position0 = new Vector3(0f, 0f, 0f);
            def.position1 = new Vector3(0f + trueWidth, 0f, 0f);
            def.position2 = new Vector3(0f, 0f + trueHeight, 0f);
            def.position3 = new Vector3(0f + trueWidth, 0f + trueHeight, 0f);
            def.colliderVertices[1].x = trueWidth;
            def.colliderVertices[1].y = trueHeight;
            def.name = name;
            VFXCollection.spriteDefinitions[id] = def;
        }
        public static GameObject CreateCustomShellCasing(string spriteName, int pixelWidth, int pixelHeight)
        {
            GameObject casing = UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(202) as Gun).shellCasing);
            casing.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(casing.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(casing);
            casing.GetComponent<tk2dBaseSprite>().spriteId = VFXCollection.inst.GetSpriteIdByName(spriteName);
            VFXToolbox.SetupDefinitionForShellSprite(spriteName, casing.GetComponent<tk2dBaseSprite>().spriteId, pixelWidth, pixelHeight);
            return casing;
        }
        public static tk2dSpriteDefinition SetupDefinitionForShellSprite(string name, int id, int pixelWidth, int pixelHeight, tk2dSpriteDefinition overrideToCopyFrom = null)
        {
            float thing = 14;
            float trueWidth = (float)pixelWidth / thing;
            float trueHeight = (float)pixelHeight / thing;
            tk2dSpriteDefinition def = overrideToCopyFrom ?? VFXCollection.inst.spriteDefinitions[(PickupObjectDatabase.GetById(202) as Gun).shellCasing.GetComponent<tk2dBaseSprite>().spriteId].CopyDefinitionFrom();
            def.boundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.boundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.untrimmedBoundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.untrimmedBoundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.position0 = new Vector3(0f, 0f, 0f);
            def.position1 = new Vector3(0f + trueWidth, 0f, 0f);
            def.position2 = new Vector3(0f, 0f + trueHeight, 0f);
            def.position3 = new Vector3(0f + trueWidth, 0f + trueHeight, 0f);
            def.name = name;
            VFXCollection.spriteDefinitions[id] = def;
            return def;
        }
    }
    /*
    public class TiledSpriteConnector : MonoBehaviour
    {
        private void Start()
        {
            tiledSprite = base.GetComponent<tk2dTiledSprite>();
        }
        private void Update()
        {
            if (sourceRigidbody)
            {
                Vector2 unitCenter = sourceRigidbody.UnitCenter;
                Vector2 unitCenter2 = Vector2.zero;
                if (usesVector && targetVector != Vector2.zero) unitCenter2 = targetVector;
                else if (targetRigidbody) unitCenter2 = targetRigidbody.UnitCenter;
                if (unitCenter2 != Vector2.zero)
                {
                    tiledSprite.transform.position = unitCenter;
                    Vector2 vector = unitCenter2 - unitCenter;
                    float num = BraveMathCollege.Atan2Degrees(vector.normalized);
                    int num2 = Mathf.RoundToInt(vector.magnitude / 0.0625f);
                    tiledSprite.dimensions = new Vector2((float)num2, tiledSprite.dimensions.y);
                    tiledSprite.transform.rotation = Quaternion.Euler(0f, 0f, num);
                    tiledSprite.UpdateZDepth();

                }
                else
                {
                    if (eraseSpriteIfTargetOrSourceNull) UnityEngine.Object.Destroy(tiledSprite.gameObject);
                    else if (eraseComponentIfTargetOrSourceNull) UnityEngine.Object.Destroy(this);
                }
            }
            else
            {
                if (eraseSpriteIfTargetOrSourceNull) UnityEngine.Object.Destroy(tiledSprite.gameObject);
                else if (eraseComponentIfTargetOrSourceNull) UnityEngine.Object.Destroy(this);
            }
        }

        public SpeculativeRigidbody sourceRigidbody;
        public SpeculativeRigidbody targetRigidbody;
        public Vector2 targetVector;
        public bool usesVector;
        public bool eraseSpriteIfTargetOrSourceNull;
        public bool eraseComponentIfTargetOrSourceNull;
        private tk2dTiledSprite tiledSprite;
    }*/
    public class EasyVFXDatabase
    {
        //Basegame VFX Objects
        public static GameObject WeakenedStatusEffectOverheadVFX = ResourceCache.Acquire("Global VFX/VFX_Debuff_Status") as GameObject;
        public static GameObject SpiratTeleportVFX;
        public static GameObject TeleporterPrototypeTelefragVFX = PickupObjectDatabase.GetById(449).GetComponent<TeleporterPrototypeItem>().TelefragVFXPrefab.gameObject;
        public static GameObject BloodiedScarfPoofVFX = PickupObjectDatabase.GetById(436).GetComponent<BlinkPassiveItem>().BlinkpoofVfx.gameObject;
        public static GameObject ChestTeleporterTimeWarp = (PickupObjectDatabase.GetById(573) as ChestTeleporterItem).TeleportVFX;
        public static GameObject MachoBraceDustUpVFX = PickupObjectDatabase.GetById(665).GetComponent<MachoBraceItem>().DustUpVFX;
        public static GameObject MachoBraceBurstVFX = PickupObjectDatabase.GetById(665).GetComponent<MachoBraceItem>().BurstVFX;
        public static GameObject MachoBraceOverheadVFX = PickupObjectDatabase.GetById(665).GetComponent<MachoBraceItem>().OverheadVFX;
        public static GameObject LastBulletStandingX;
        //Projectile Death Effects
        public static GameObject GreenLaserCircleVFX = (PickupObjectDatabase.GetById(89) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject YellowLaserCircleVFX = (PickupObjectDatabase.GetById(651) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject RedLaserCircleVFX = (PickupObjectDatabase.GetById(32) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject BlueLaserCircleVFX = (PickupObjectDatabase.GetById(59) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject SmoothLightBlueLaserCircleVFX = (PickupObjectDatabase.GetById(576) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject SmoothLightGreenLaserCircleVFX = (PickupObjectDatabase.GetById(360) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject WhiteCircleVFX = (PickupObjectDatabase.GetById(330) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject BlueFrostBlastVFX = (PickupObjectDatabase.GetById(225) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject RedFireBlastVFX = (PickupObjectDatabase.GetById(125) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        public static GameObject SmallMagicPuffVFX = (PickupObjectDatabase.GetById(338) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
        //Basegame VFX Pools
        public static VFXPool SpiratTeleportVFXPool;

        //Custom VFX Objects
        public static GameObject JarateOverheadVFX() { return JarateEffectSetup.jarateVFXObject; }

        //Stat Up VFX
        public static GameObject DamageUpVFX;
        public static GameObject ShotSpeedUpVFX;
        public static GameObject SpeedUpVFX;
        public static GameObject FirerateUpVFX;
        public static GameObject AccuracyUpVFX;
        public static GameObject KnockbackUpVFX;
        public static GameObject ReloadSpeedUpVFX;
        public static void Init()
        {
            //Last Bullet Standing VFX
            GameObject ChallengeManagerReference = LoadHelper.LoadAssetFromAnywhere<GameObject>("_ChallengeManager");
            LastBulletStandingX = (ChallengeManagerReference.GetComponent<ChallengeManager>().PossibleChallenges[0].challenge as BestForLastChallengeModifier).KingVFX;
            //Spirat Teleportation VFX
            #region SpiratTP
            GameObject teleportBullet = EnemyDatabase.GetOrLoadByGuid("7ec3e8146f634c559a7d58b19191cd43").bulletBank.GetBullet("self").BulletObject;
            Projectile proj = teleportBullet.GetComponent<Projectile>();
            if (proj != null)
            {
                TeleportProjModifier tp = proj.GetComponent<TeleportProjModifier>();
                if (tp != null)
                {
                    SpiratTeleportVFXPool = tp.teleportVfx;
                    SpiratTeleportVFX = tp.teleportVfx.effects[0].effects[0].effect;
                }
            }
            #endregion
        }
    }
    public class ImprovedAfterImage : BraveBehaviour // credit to ski
    {

        public ImprovedAfterImage()
        {
            shaders = new List<Shader>
        {
            ShaderCache.Acquire("Brave/Internal/RainbowChestShader"),
            ShaderCache.Acquire("Brave/Internal/GlitterPassAdditive"),
            ShaderCache.Acquire("Brave/Internal/HologramShader"),
            ShaderCache.Acquire("Brave/Internal/HighPriestAfterImage")
        };
            //shaders.Add(ShaderCache.Acquire("Brave/ItemSpecific/MetalSkinShader"));
            this.IsRandomShader = false;
            this.spawnShadows = true;
            this.shadowTimeDelay = 0.1f;
            this.shadowLifetime = 0.6f;
            this.minTranslation = 0.2f;
            this.maxEmission = 800f;
            this.minEmission = 100f;
            this.targetHeight = -2f;
            this.dashColor = new Color(1f, 0f, 1f, 1f);
            this.m_activeShadows = new LinkedList<Shadow>();
            this.m_inactiveShadows = new LinkedList<Shadow>();
        }

        public void Start()
        {
            if (this.OptionalImageShader != null)
            {
                this.OverrideImageShader = this.OptionalImageShader;
            }
            if (base.transform.parent != null && base.transform.parent.GetComponent<Projectile>() != null)
            {
                base.transform.parent.GetComponent<Projectile>().OnDestruction += this.ProjectileDestruction;
            }
            this.m_lastSpawnPosition = base.transform.position;
        }

        private void ProjectileDestruction(Projectile source)
        {
            if (this.m_activeShadows.Count > 0)
            {
                GameManager.Instance.StartCoroutine(this.HandleDeathShadowCleanup());
            }
        }

        public void LateUpdate()
        {
            if (this.spawnShadows && !this.m_previousFrameSpawnShadows)
            {
                this.m_spawnTimer = this.shadowTimeDelay;
            }
            this.m_previousFrameSpawnShadows = this.spawnShadows;
            LinkedListNode<ImprovedAfterImage.Shadow> next;
            for (LinkedListNode<ImprovedAfterImage.Shadow> linkedListNode = this.m_activeShadows.First; linkedListNode != null; linkedListNode = next)
            {
                next = linkedListNode.Next;
                linkedListNode.Value.timer -= BraveTime.DeltaTime;
                if (linkedListNode.Value.timer <= 0f)
                {
                    this.m_activeShadows.Remove(linkedListNode);
                    this.m_inactiveShadows.AddLast(linkedListNode);
                    if (linkedListNode.Value.sprite)
                    {
                        linkedListNode.Value.sprite.renderer.enabled = false;
                    }
                }
                else if (linkedListNode.Value.sprite)
                {
                    float num = linkedListNode.Value.timer / this.shadowLifetime;
                    Material sharedMaterial = linkedListNode.Value.sprite.renderer.sharedMaterial;
                    sharedMaterial.SetFloat("_EmissivePower", Mathf.Lerp(this.maxEmission, this.minEmission, num));
                    sharedMaterial.SetFloat("_Opacity", num);
                }
            }
            if (this.spawnShadows)
            {
                if (this.m_spawnTimer > 0f)
                {
                    this.m_spawnTimer -= BraveTime.DeltaTime;
                }
                if (this.m_spawnTimer <= 0f && Vector2.Distance(this.m_lastSpawnPosition, base.transform.position) > this.minTranslation)
                {
                    this.SpawnNewShadow();
                    this.m_spawnTimer += this.shadowTimeDelay;
                    this.m_lastSpawnPosition = base.transform.position;
                }
            }
        }

        private IEnumerator HandleDeathShadowCleanup()
        {
            while (this.m_activeShadows.Count > 0)
            {
                LinkedListNode<ImprovedAfterImage.Shadow> next;
                for (LinkedListNode<ImprovedAfterImage.Shadow> node = this.m_activeShadows.First; node != null; node = next)
                {
                    next = node.Next;
                    node.Value.timer -= BraveTime.DeltaTime;
                    if (node.Value.timer <= 0f)
                    {
                        this.m_activeShadows.Remove(node);
                        this.m_inactiveShadows.AddLast(node);
                        if (node.Value.sprite)
                        {
                            node.Value.sprite.renderer.enabled = false;
                        }
                    }
                    else if (node.Value.sprite)
                    {
                        float num = node.Value.timer / this.shadowLifetime;
                        Material sharedMaterial = node.Value.sprite.renderer.sharedMaterial;
                        sharedMaterial.SetFloat("_EmissivePower", Mathf.Lerp(this.maxEmission, this.minEmission, num));
                        sharedMaterial.SetFloat("_Opacity", num);
                    }
                }
                yield return null;
            }
            yield break;
        }

        public override void OnDestroy()
        {
            GameManager.Instance.StartCoroutine(this.HandleDeathShadowCleanup());
            base.OnDestroy();
        }


        private void SpawnNewShadow()
        {
            if (this.m_inactiveShadows.Count == 0)
            {
                this.CreateInactiveShadow();
            }
            LinkedListNode<ImprovedAfterImage.Shadow> first = this.m_inactiveShadows.First;
            tk2dSprite sprite = first.Value.sprite;
            this.m_inactiveShadows.RemoveFirst();
            if (!sprite || !sprite.renderer)
            {
                return;
            }
            first.Value.timer = this.shadowLifetime;
            sprite.SetSprite(base.sprite.Collection, base.sprite.spriteId);
            sprite.transform.position = base.sprite.transform.position;
            sprite.transform.rotation = base.sprite.transform.rotation;
            sprite.scale = base.sprite.scale;
            sprite.usesOverrideMaterial = true;
            sprite.IsPerpendicular = true;
            if (sprite.renderer && IsRandomShader)
            {
                sprite.renderer.enabled = true;
                sprite.renderer.material.shader = shaders[(int)UnityEngine.Random.Range(0, shaders.Count)];

                if (sprite.renderer.material.shader == shaders[3])
                {
                    sprite.renderer.sharedMaterial.SetFloat("_EmissivePower", this.minEmission);
                    sprite.renderer.sharedMaterial.SetFloat("_Opacity", 1f);
                    sprite.renderer.sharedMaterial.SetColor("_DashColor", Color.HSVToRGB(UnityEngine.Random.value, 1.0f, 1.0f));
                }
                if (sprite.renderer.material.shader == shaders[0])
                {
                    sprite.renderer.sharedMaterial.SetFloat("_AllColorsToggle", 1f);
                }
            }

            else if (sprite.renderer)
            {

                sprite.renderer.enabled = true;
                sprite.renderer.material.shader = (this.OverrideImageShader ?? ShaderCache.Acquire("Brave/Internal/HighPriestAfterImage"));
                sprite.renderer.sharedMaterial.SetFloat("_EmissivePower", this.minEmission);
                sprite.renderer.sharedMaterial.SetFloat("_Opacity", 1f);
                sprite.renderer.sharedMaterial.SetColor("_DashColor", this.dashColor);
                sprite.renderer.sharedMaterial.SetFloat("_AllColorsToggle", 1f);
            }

            sprite.HeightOffGround = this.targetHeight;
            sprite.UpdateZDepth();
            this.m_activeShadows.AddLast(first);
        }

        public bool IsRandomShader;

        private void CreateInactiveShadow()
        {
            GameObject gameObject = new GameObject("after image");
            if (this.UseTargetLayer)
            {
                gameObject.layer = LayerMask.NameToLayer(this.TargetLayer);
            }
            tk2dSprite sprite = gameObject.AddComponent<tk2dSprite>();
            gameObject.transform.parent = SpawnManager.Instance.VFX;
            this.m_inactiveShadows.AddLast(new ImprovedAfterImage.Shadow
            {
                timer = this.shadowLifetime,
                sprite = sprite
            });
        }


        public bool spawnShadows;

        public float shadowTimeDelay;

        public float shadowLifetime;

        public float minTranslation;

        public float maxEmission;

        public float minEmission;

        public float targetHeight;

        public Color dashColor;

        public Shader OptionalImageShader;

        public bool UseTargetLayer;

        public string TargetLayer;

        [NonSerialized]
        public Shader OverrideImageShader;

        private readonly LinkedList<ImprovedAfterImage.Shadow> m_activeShadows;

        private readonly LinkedList<ImprovedAfterImage.Shadow> m_inactiveShadows;

        private readonly List<Shader> shaders;

        private float m_spawnTimer;

        private Vector2 m_lastSpawnPosition;

        private bool m_previousFrameSpawnShadows;

        private class Shadow
        {
            public float timer;

            public tk2dSprite sprite;
        }
    }
}
