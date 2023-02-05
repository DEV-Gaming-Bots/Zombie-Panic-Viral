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

		Input.SuppressButton( InputButton.Reload );

		TimeUntilReloaded = UnloadTime;
		ReloadLock = true;

		using ( Prediction.Off() )
			StartUnloadingEffects( To.Single( player.Client ) );
	}

	protected void FinishUnloading( PlayerPawn player )
	{
		Tags.Set( "unloading", false );
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
		if ( IsFull )
			return;

		TimeUntilReloaded = IsEmpty ? EmptyReloadTime : ReloadTime;
		ReloadLock = true;

		StartReloading();
	}

	protected void StartReloading()
	{
		Player?.SetAnimParameter( "b_reload", true );
		Tags.Set( "reloading", true );

		using ( Prediction.Off() )
			StartReloadEffects( To.Single( Player.Client ) );
	}

	[ClientRpc]
	public static void StartReloadEffects()
	{
		WeaponViewModel.Current?.SetAnimParameter( "reload", true );
		ArmVM.Current?.SetAnimParameter( "reload", true );
	}

	protected void FinishReloading( PlayerPawn player )
	{
		Tags.Set( "reloading", false );
		Fill();

		using ( Prediction.Off() )
			FinishReloadEffects( To.Single( Player.Client ) );
	}

	[ClientRpc]
	public static void FinishUnloadEffects()
	{
		//Fake firing for weapons such as the pistols slide action
		//This is so it doesn't slide in forwards
		WeaponViewModel.Current?.SetAnimParameter( "fire", true );
		ArmVM.Current?.SetAnimParameter( "fire", true );

		WeaponViewModel.Current?.SetAnimParameter( "empty", true );
		ArmVM.Current?.SetAnimParameter( "empty", true );
	}

	[ClientRpc]
	public static void FinishReloadEffects()
	{
		WeaponViewModel.Current?.SetAnimParameter( "empty", false );
		ArmVM.Current?.SetAnimParameter( "empty", false );
	}
}
