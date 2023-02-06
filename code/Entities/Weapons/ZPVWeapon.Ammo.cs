using Sandbox;
using System.Reflection.Emit;

namespace ZPViral.Weapons;

public partial class Weapon
{
	[Net] public int AmmoCount { get; set; }

	protected bool ReloadLock { get; set; } = false;
	public TimeUntil TimeUntilReloaded { get; set; }

	public bool IsFull
	{
		get => AmmoCount >= (AllowChamber ? MaximumAmmo + 1 : MaximumAmmo);
	}

	public bool IsEmpty
	{
		get => AmmoCount == 0;
	}

	public void Fill()
	{
		if ( AmmoCount > 0 )
			AmmoCount = MaximumAmmo + 1;
		else
			AmmoCount = MaximumAmmo;
	}

	public void Empty()
	{
		AmmoCount = 0;
	}

	public bool HasEnoughAmmo( int amount = 1 )
	{
		return AmmoCount >= amount;
	}

	public bool TakeAmmo( int amount = 1 )
	{
		if ( AmmoCount >= amount )
		{
			AmmoCount -= amount;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Unloading
	/// </summary>
	protected void DoUnload(PlayerPawn player)
	{
		player?.SetAnimParameter( "b_reload", true );
		Tags.Set( "unloading", true );

		TimeUntilReloaded = UnloadTime;
		ReloadLock = true;

		bool interrupt = Input.Down( InputButton.PrimaryAttack );

		if ( WeaponType == TypeEnum.Shotgun )
		{
			if(interrupt)
			{
				Tags.Set( "unloading", false );
				ReloadLock = false;

				using ( Prediction.Off() )
					FinishUnloadEffects( To.Single( player.Client ) );

				return;
			}

			if ( AmmoCount < 0 )
			{
				Tags.Set( "unloading", false );
				ReloadLock = false;

				AmmoCount = 0;

				using ( Prediction.Off() )
					FinishUnloadEffects( To.Single( player.Client ) );
			}
			else if ( AmmoCount >= 0 )
			{
				AmmoCount--;

				using ( Prediction.Off() )
					StartUnloadingEffects( To.Single( player.Client ) );
			}
		} 
		else
		{
			using ( Prediction.Off() )
				StartUnloadingEffects( To.Single( player.Client ) );
		}
	}

	protected void FinishUnloading( PlayerPawn player )
	{
		Tags.Set( "unloading", false );
		
		if ( WeaponType != TypeEnum.Shotgun )
			Empty();

		using ( Prediction.Off() )
			FinishUnloadEffects( To.Single( Player.Client ) );
	}


	[ClientRpc]
	public static void StartUnloadingEffects()
	{
		WeaponViewModel.Current?.SetAnimParameter( "unload", true );
		ArmVM.Current?.SetAnimParameter( "unload", true );
	}

	/////
	/// Reloading
	/////
	protected bool CanReload( PlayerPawn player )
	{
		if ( TimeSinceActivated < TimeToEquip ) return false;

		if ( ReloadLock ) return false;

		if ( IsFull ) return false;

		return true;
	}

	protected bool CanUnload( PlayerPawn player )
	{
		if ( TimeSinceActivated < TimeToEquip ) return false;

		if ( ReloadLock ) return false;

		if ( AmmoCount <= 0 ) return false;

		if ( IsMelee ) return false;

		return Input.Down( InputButton.Reload );
	}

	protected void DoReload( PlayerPawn player )
	{
		TimeUntilReloaded = IsEmpty ? EmptyReloadTime : ReloadTime;
		ReloadLock = true;

		if ( WeaponType == TypeEnum.Shotgun )
		{
			bool interrupt = Input.Down( InputButton.PrimaryAttack );

			if(interrupt)
			{
				Tags.Set( "reloading", false );
				ReloadLock = false;

				using ( Prediction.Off() )
					FinishReloadEffects( To.Single( player.Client ) );

				return;
			}

			if( !Tags.Has("reloading") )
				StartReloadShotgun();
			else
			{
				if( AmmoCount >= MaximumAmmo )
				{
					ReloadLock = false;
					FinishReloading(player);
				} 
				else if ( AmmoCount < MaximumAmmo )
				{
					AmmoCount++;
					ShotgunReloadEffects();
				}

			}
		}
		else 
			StartReloading();
	}

	protected void StartReloadShotgun()
	{
		Player?.SetAnimParameter( "b_reload", true );
		Tags.Set( "reloading", true );

		using ( Prediction.Off() )
			StartReloadEffects( To.Single( Player.Client ), IsEmpty );
	}

	protected void StartReloading()
	{
		Player?.SetAnimParameter( "b_reload", true );
		Tags.Set( "reloading", true );

		using ( Prediction.Off() )
			StartReloadEffects( To.Single( Player.Client ), IsEmpty );
	}

	[ClientRpc]
	public static void ShotgunReloadEffects()
	{
		WeaponViewModel.Current?.SetAnimParameter( "reload", true );
		ArmVM.Current?.SetAnimParameter( "reload", true );
	}

	[ClientRpc]
	public static void StartReloadEffects(bool isEmpty)
	{
		WeaponViewModel.Current?.SetAnimParameter( "reload", true );
		ArmVM.Current?.SetAnimParameter( "reload", true );

		WeaponViewModel.Current?.SetAnimParameter( "empty", isEmpty );
		ArmVM.Current?.SetAnimParameter( "empty", isEmpty );
	}

	protected void FinishReloading( PlayerPawn player )
	{
		Tags.Set( "reloading", false );
		
		if(WeaponType != TypeEnum.Shotgun)
			Fill();

		using ( Prediction.Off() )
			FinishReloadEffects( To.Single( player.Client ) );
	}

	[ClientRpc]
	public static void FinishUnloadEffects()
	{
		//WeaponViewModel.Current?.SetAnimParameter( "fire", true );
		//ArmVM.Current?.SetAnimParameter( "fire", true );

		WeaponViewModel.Current?.SetAnimParameter( "empty", true );
		ArmVM.Current?.SetAnimParameter( "empty", true );

		WeaponViewModel.Current?.SetAnimParameter( "finishunload", true );
		ArmVM.Current?.SetAnimParameter( "finishunload", true );
	}

	[ClientRpc]
	public static void FinishReloadEffects()
	{
		WeaponViewModel.Current?.SetAnimParameter( "empty", false );
		ArmVM.Current?.SetAnimParameter( "empty", false );

		WeaponViewModel.Current?.SetAnimParameter( "finishreload", true );
		ArmVM.Current?.SetAnimParameter( "finishreload", true );
	}
}
