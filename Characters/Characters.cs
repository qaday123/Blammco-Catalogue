using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Alexandria.cAPI.Hatabase;
using static Alexandria.cAPI.HatUtility;

namespace TF2Stuff
{
    public static class TF2Characters
    {
        public static PlayableCharacters Scout;
        public static PlayableCharacters Soldier;

        public const string scoutObjectName = "PlayerScout(Clone)";
        public const string soldierObjectName = "PlayerSoldier(Clone)";
    }
    public static class CharacterHatOffsets
    {
        public static void AddAllHatOffsets()
        {
            AddHatOffsetForCharacter(TF2Characters.scoutObjectName);
            AddEyeOffsetForCharacter(TF2Characters.scoutObjectName);

            AddHatOffsetForCharacter(TF2Characters.soldierObjectName);
            AddEyeOffsetForCharacter(TF2Characters.soldierObjectName);
        }
        static void AddHatOffsetForCharacter(string objectName)
        {
            var HeadOffset = Alexandria.cAPI.Hatabase.ModdedHeadFrameOffsets[objectName];
            var HeadOffsetToAdd = objectName switch
            {
                TF2Characters.scoutObjectName => ScoutHeadFrameOffsets,
                TF2Characters.soldierObjectName => SoldierHeadFrameOffsets
            };
            foreach (KeyValuePair<string, FrameOffset> Entry in HeadOffsetToAdd)
            {
                HeadOffset[Entry.Key] = Entry.Value;
            }
        }
        static void AddEyeOffsetForCharacter(string objectName)
        {
            var EyeOffset = Alexandria.cAPI.Hatabase.ModdedEyeFrameOffsets[objectName];
            var EyeOffsetToAdd = objectName switch
            {
                TF2Characters.scoutObjectName => ScoutEyeFrameOffsets, 
                TF2Characters.soldierObjectName => SoldierEyeFrameOffsets,
            };
            foreach (KeyValuePair<string, FrameOffset> Entry in EyeOffsetToAdd)
            {
                EyeOffset[Entry.Key] = Entry.Value;
            }
        }
        public static readonly Dictionary<string, FrameOffset> ScoutHeadFrameOffsets = new()
        {
            {"Scout_idle1",                 new FrameOffset( 1, -1)},
            {"Scout_idle2",                 new FrameOffset( 1, -1)},
            {"Scout_idle3",                 new FrameOffset( 1,  0)},
            {"Scout_idle0",                 new FrameOffset( 1,  0)},
            {"pilot_idle_back_right_001",            new FrameOffset( 2,  0)},
            {"pilot_idle_back_right_002",            new FrameOffset( 2, -1)},
            {"pilot_idle_back_right_003",            new FrameOffset( 2, -1)},
            {"pilot_idle_back_right_004",            new FrameOffset( 2,  0)},
            {"Scout_idle_backward1",                  new FrameOffset( 0, -1)},
            {"Scout_idle_backward2",                  new FrameOffset( 0, -1)},
            {"Scout_idle_backward3",                 new FrameOffset( 0,  0)},
            {"Scout_idle_backward4",                 new FrameOffset( 0,  0)},
            {"Scout_idle_forward1",                 new FrameOffset( 0,  -1)},
            {"Scout_idle_forward2",                 new FrameOffset( 0,  -1)},
            {"Scout_run_right1",                 new FrameOffset( 0, 1)},
            {"Scout_run_right2",                 new FrameOffset( 0, 0)},
            {"Scout_run_right3",                 new FrameOffset( 0, -1)},
            {"Scout_run_right4",                 new FrameOffset( 1, 1)},
            {"Scout_run_right5",                 new FrameOffset( 1, 0)},
            {"Scout_run_right0",                 new FrameOffset( 1, 1)},
            {"Scout_run_right_bw1",             new FrameOffset( 0, -1)},
            {"Scout_run_right_bw2",             new FrameOffset(-1,  1)},
            {"Scout_run_right_bw3",             new FrameOffset(-1,  0)},
            {"Scout_run_right_bw4",             new FrameOffset( 0, -1)},
            {"Scout_run_right_bw5",             new FrameOffset( 1,  2)},
            {"Scout_run_right_bw0",             new FrameOffset( 1,  0)},
            {"Scout_run_up0",                 new FrameOffset( 0,  3)},
            {"Scout_run_up1",                 new FrameOffset(0,  2)},
            {"Scout_run_up2",                 new FrameOffset( 0,  0)},
            {"Scout_run_up3",                 new FrameOffset( 0,  3)},
            {"Scout_run_up4",                 new FrameOffset( 0,  2)},
            {"Scout_run_up5",                 new FrameOffset( 0,  0)},
            {"Scout_run_down0",                  new FrameOffset( 0, 3)},
            {"Scout_run_down1",                  new FrameOffset( 0, 2)},
            {"Scout_run_down2",                  new FrameOffset( 0, 0)},
            {"Scout_run_down3",                  new FrameOffset( 0, 3)},
            {"Scout_run_down4",                  new FrameOffset(0, 2)},
            {"Scout_run_down5",                  new FrameOffset(0, 0)},
        };
        public static readonly Dictionary<string, FrameOffset> ScoutEyeFrameOffsets = new()
        {
            {"Scout_idle1",                 new FrameOffset( 0, 0)},
            {"Scout_idle2",                 new FrameOffset( 0, 0)},
            {"Scout_idle3",                 new FrameOffset( 0,  1)},
            {"Scout_idle0",                 new FrameOffset( 0,  1)},
            {"pilot_idle_back_right_001",            new FrameOffset( 2,  0)},
            {"pilot_idle_back_right_002",            new FrameOffset( 2, -1)},
            {"pilot_idle_back_right_003",            new FrameOffset( 2, -1)},
            {"pilot_idle_back_right_004",            new FrameOffset( 2,  0)},
            {"Scout_idle_backward1",                  new FrameOffset( 0, -1)},
            {"Scout_idle_backward2",                  new FrameOffset( 0, -1)},
            {"Scout_idle_backward3",                 new FrameOffset( 0,  0)},
            {"Scout_idle_backward4",                 new FrameOffset( 0,  0)},
            {"Scout_idle_forward1",                 new FrameOffset( 0,  -1)},
            {"Scout_idle_forward2",                 new FrameOffset( 0,  -1)},
            {"Scout_run_right0",                 new FrameOffset( 0, 0)},
            {"Scout_run_right1",                 new FrameOffset( 0, 3)},
            {"Scout_run_right2",                 new FrameOffset( 0, 2)},
            {"Scout_run_right3",                 new FrameOffset( 0, 0)},
            {"Scout_run_right4",                 new FrameOffset( 0, 2)},
            {"Scout_run_right5",                 new FrameOffset( 0, 1)},
            {"Scout_run_right_bw0",             new FrameOffset( 0, 0)},
            {"Scout_run_right_bw1",             new FrameOffset(-1, 3)},
            {"Scout_run_right_bw2",             new FrameOffset(-1, 2)},
            {"Scout_run_right_bw3",             new FrameOffset( 0, 0)},
            {"Scout_run_right_bw4",             new FrameOffset( 1, 3)},
            {"Scout_run_right_bw5",             new FrameOffset( 1, 2)},
            {"Scout_run_up0",                 new FrameOffset( 0,  3)},
            {"Scout_run_up1",                 new FrameOffset(0,  2)},
            {"Scout_run_up2",                 new FrameOffset( 0,  0)},
            {"Scout_run_up3",                 new FrameOffset( -1,  3)},
            {"Scout_run_up4",                 new FrameOffset( -1,  2)},
            {"Scout_run_up5",                 new FrameOffset( 0,  0)},
            {"Scout_run_down0",                  new FrameOffset( -2, 4)},
            {"Scout_run_down1",                  new FrameOffset( -2, 3)},
            {"Scout_run_down2",                  new FrameOffset( -2, 0)},
            {"Scout_run_down3",                  new FrameOffset( -3, 4)},
            {"Scout_run_down4",                  new FrameOffset( -3, 3)},
            {"Scout_run_down5",                  new FrameOffset( -2, 0)},
        };
        public static readonly Dictionary<string, FrameOffset> SoldierHeadFrameOffsets = new()
        {
            {"Soldier_idle1",          new FrameOffset( 0, -1)},
            {"Soldier_idle2",          new FrameOffset( 0, -2)},
            {"Soldier_idle3",          new FrameOffset( 0, -1)},
            {"Soldier_idle_bw1",           new FrameOffset( 0, -1)},
            {"Soldier_idle_bw2",           new FrameOffset( 0, -2)},
            {"Soldier_idle_bw3",           new FrameOffset( 0, -1)},
            {"Soldier_idle_backward1",                 new FrameOffset( 0, -1)},
            {"Soldier_idle_backward2",                 new FrameOffset( 0, -2)},
            {"Soldier_idle_backward3",                 new FrameOffset( 0, -1)},
            {"Soldier_idle_forward1",                new FrameOffset( 0, -1)},
            {"Soldier_idle_forward2",                new FrameOffset( 0, -2)},
            {"Soldier_idle_forward3",                new FrameOffset( 0, -1)},
            {"Soldier_run_right0",           new FrameOffset( 0,  4)},
            {"Soldier_run_right1",           new FrameOffset( 1,  2)},
            {"Soldier_run_right2",           new FrameOffset( 1, -1)},
            {"Soldier_run_right3",           new FrameOffset( 0,  4)},
            {"Soldier_run_right4",           new FrameOffset( 1,  2)},
            {"Soldier_run_right5",           new FrameOffset( 1, -1)},
            {"Soldier_run_right_bw0",            new FrameOffset( 1,  5)},
            {"Soldier_run_right_bw1",            new FrameOffset( 1,  2)},
            {"Soldier_run_right_bw2",            new FrameOffset( 1, -1)},
            {"Soldier_run_right_bw3",            new FrameOffset( 1,  4)},
            {"Soldier_run_right_bw4",            new FrameOffset( 1,  2)},
            {"Soldier_run_right_bw5",            new FrameOffset( 1, -1)},
            {"Soldier_run_down0",                 new FrameOffset( 0,  3)},
            {"Soldier_run_down1",                 new FrameOffset( 0,  1)},
            {"Soldier_run_down2",                 new FrameOffset( 0, 0)},
            {"Soldier_run_down3",                 new FrameOffset( 0,  3)},
            {"Soldier_run_down4",                 new FrameOffset( 0,  1)},
            {"Soldier_run_down5",                 new FrameOffset( 0, 0)},
            {"Soldier_run_up0",                  new FrameOffset( 0,  4)},
            {"Soldier_run_up1",                  new FrameOffset( 0,  2)},
            {"Soldier_run_up2",                  new FrameOffset( 0, 0)},
            {"Soldier_run_up3",                  new FrameOffset( 0,  4)},
            {"Soldier_run_up4",                  new FrameOffset( 0,  2)},
            {"Soldier_run_up5",                  new FrameOffset( 0, 0)},
        };
        public static readonly Dictionary<string, FrameOffset> SoldierEyeFrameOffsets = new()
        {
            {"Soldier_idle1",          new FrameOffset( 0, -1)},
            {"Soldier_idle2",          new FrameOffset( 0, -2)},
            {"Soldier_idle3",          new FrameOffset( 0, -1)},
            {"Soldier_idle_bw1",           new FrameOffset( 0, -1)},
            {"Soldier_idle_bw2",           new FrameOffset( 0, -2)},
            {"Soldier_idle_bw3",           new FrameOffset( 0, -1)},
            {"Soldier_idle_backward0",                 new FrameOffset( 0, 0)},
            {"Soldier_idle_backward1",                 new FrameOffset( 0, -1)},
            {"Soldier_idle_backward2",                 new FrameOffset( 0, -2)},
            {"Soldier_idle_backward3",                 new FrameOffset( 0, -1)},
            {"Soldier_idle_forward0",                new FrameOffset( 0, 0)},
            {"Soldier_idle_forward1",                new FrameOffset( 0, -1)},
            {"Soldier_idle_forward2",                new FrameOffset( 0, -2)},
            {"Soldier_idle_forward3",                new FrameOffset( 0, -1)},
            {"Soldier_run_right0",           new FrameOffset( 0,  3)},
            {"Soldier_run_right1",           new FrameOffset( 1,  2)},
            {"Soldier_run_right2",           new FrameOffset( 1, -1)},
            {"Soldier_run_right3",           new FrameOffset( 0,  3)},
            {"Soldier_run_right4",           new FrameOffset( 1,  2)},
            {"Soldier_run_right5",           new FrameOffset( 1, -1)},
            {"Soldier_run_right_bw0",            new FrameOffset( 1,  4)},
            {"Soldier_run_right_bw1",            new FrameOffset( 1,  2)},  
            {"Soldier_run_right_bw2",            new FrameOffset( 1, -1)},
            {"Soldier_run_right_bw3",            new FrameOffset( 1,  4)},
            {"Soldier_run_right_bw4",            new FrameOffset( 1,  2)},
            {"Soldier_run_right_bw5",            new FrameOffset( 1, -1)},
            {"Soldier_run_down0",                 new FrameOffset( 0,  3)},
            {"Soldier_run_down1",                 new FrameOffset( 0,  1)},
            {"Soldier_run_down2",                 new FrameOffset( 0, 0)},
            {"Soldier_run_down3",                 new FrameOffset( 0,  3)},
            {"Soldier_run_down4",                 new FrameOffset( 0,  1)},
            {"Soldier_run_down5",                 new FrameOffset( 0, 0)},
            {"Soldier_run_up0",                  new FrameOffset( -1,  4)},
            {"Soldier_run_up1",                  new FrameOffset( -1,  2)},
            {"Soldier_run_up2",                  new FrameOffset( -1, 0)},
            {"Soldier_run_up3",                  new FrameOffset( -1,  4)},
            {"Soldier_run_up4",                  new FrameOffset( -1,  2)},
            {"Soldier_run_up5",                  new FrameOffset( -1, 0)},
        };
    }
}