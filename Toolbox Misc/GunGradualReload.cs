using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;

namespace TF2Items
{
	public class GunGradualReload : MonoBehaviour
	{
		// new attributes
		public bool hasRepeatingAnimation;
		public int frameToRepeatAnimationFrom;
		public string[] additionalAudioEventsPerReload;
		//

		public GunGradualReload()
		{
			reloadnumber = 1;
			emptyclipPunishtime = .5f;

			hasRepeatingAnimation = true;
			frameToRepeatAnimationFrom = 0;
			additionalAudioEventsPerReload = new string[0];
		}


		private void Start()
		{
			gun = base.GetComponent<Gun>();
			player = GunPlayerOwner(gun);
			origiReloadtime = gun.reloadTime;
			reloadrate = (gun.reloadTime / gun.ClipCapacity);
			
		}

		private void Update()
		{
			if(Time.timeScale > 0)
            {
				if (player.CurrentGun == gun) // my gun?
				{

					if (gun.ClipShotsRemaining > 0) // has ammo?
					{
						if (gun.IsReloading)
						{
							if (this.gun.IsReloading && Key(GungeonActions.GungeonActionType.Shoot, this.gun.CurrentOwner as PlayerController) && this.gun.ClipShotsRemaining > 0) //shot durring reload with ammo
							{
								maintaincount = gun.ClipShotsRemaining;
                                this.gun.ForceImmediateReload(false); // force reload to finish and reset back to number we know was loaded beforehand.
								GameUIRoot.Instance.ForceClearReload(player.PlayerIDX);
                                gun.ClipShotsRemaining = maintaincount;
							}

						}
					}
					else
					{
						if (gun.ClipShotsRemaining <= 0 && Key(GungeonActions.GungeonActionType.Shoot, this.gun.CurrentOwner as PlayerController)) // anti jank with the below clip shooting idk how you can shoot with negative ammo but you sure can :)
						{
							
							this.gun.ClipShotsRemaining = 0;

							gun.reloadTime = ((this.gun.ClipCapacity - this.gun.ClipShotsRemaining) * reloadrate);

							timer = reloadrate + emptyclipPunishtime; // adds extra time to the last bullet to punish spam shooting at the bottom of the clip.
							gun.Reload();
							
						}
					}

					if (!this.gun.IsReloading)
					{
						gun.reloadTime = ((this.gun.ClipCapacity - this.gun.ClipShotsRemaining) * reloadrate);
						//sets the guns reload time to the time it will take for all missing rounds to be loaded
					}
					
					if(this.gun.IsReloading)
					{
						reloadrate = (gun.reloadTime / gun.ClipCapacity);
						gun.reloadTime = origiReloadtime;
						if (timer > 0)
						{
							timer -= BraveTime.DeltaTime; // timer loop while gun is reloading
						}
						else
						{
							gun.MoveBulletsIntoClip(reloadnumber);

							// NEW LOGIC
                            if (hasRepeatingAnimation) gun.spriteAnimator.PlayFromFrame(gun.reloadAnimation, frameToRepeatAnimationFrom); // repeats animation
                            foreach (string audio in additionalAudioEventsPerReload) // plays each audio sound if needed
                            {
                                AkSoundEngine.PostEvent(audio, base.gameObject);
                            }
							//

                            timer = reloadrate * player.stats.GetStatModifier(PlayerStats.StatType.ReloadSpeed);
							if(gun.ClipCapacity == gun.ClipShotsRemaining)
                            {
								maintaincount = gun.ClipShotsRemaining;
								this.gun.ForceImmediateReload(false); // force reload to finish and reset back to number we know was loaded beforehand.
								GameUIRoot.Instance.ForceClearReload(player.PlayerIDX);
								gun.ClipShotsRemaining = maintaincount;
							}
						}
						
					}
				}
		
			}
		}

		
		public float emptyclipPunishtime;
		public int maintaincount;
		private float timer;
		private float reloadrate;
		private float origiReloadtime;
		public int reloadnumber;
		public Gun gun;
		public PlayerController player;

		public bool Key(GungeonActions.GungeonActionType action, PlayerController user)
		{
			return BraveInput.GetInstanceForPlayer(user.PlayerIDX).ActiveActions.GetActionFromType(action).IsPressed;
		}


		public PlayerController GunPlayerOwner(Gun bullet)
		{
			bool flag = bullet && bullet.CurrentOwner && bullet.CurrentOwner is PlayerController;
			PlayerController result;
			if (flag)
			{
				result = (bullet.CurrentOwner as PlayerController);
			}
			else
			{
				result = null;
			}
			return result;
		}

		
		
		
	}


}