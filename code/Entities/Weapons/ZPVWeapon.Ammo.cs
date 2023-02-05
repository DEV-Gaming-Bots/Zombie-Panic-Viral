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

	public void Fill()
	{
		if ( AmmoCount > 0 )
			AmmoCount = MaximumAmmo + 1;
		else
			AmmoCount = MaximumAmmo;
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

	/////
	/// Reloading
	/////
	protected bool CanReload( PlayerPawn player )
	{
		if ( TimeSinceActivated < TimeToEquip ) return false;

		if ( ReloadLock ) return false;

		return Input.Pressed( InputButton.Reload );
	}

	protected void DoReload( PlayerPawn player )
	{
		if ( IsFull )
			return;

		TimeUntilReloaded = ReloadTime;
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
	public static void FinishReloadEffects()
	{
		WeaponViewModel.Current?.SetAnimParameter( "empty", false );
		ArmVM.Current?.SetAnimParameter( "empty", false );
	}
}
