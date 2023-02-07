
public partial class Inventory : EntityComponent<PlayerPawn>, ISingletonComponent
{
	[Net] public int PistolAmmo { get; set; } = 0;
	[Net] public int ShotgunAmmo { get; set; } = 0;
	[Net] public int RifleAmmo { get; set; } = 0;
	[Net] public int MagnumAmmo { get; set; } = 0;
	[Net] protected IList<Weapon> Weapons { get; set; }
	[Net, Predicted] public Weapon ActiveWeapon { get; set; }
	[Net] public float Weight { get; set; }

	public int GetWeaponSlotUsage()
	{
		int filled = 0;

		foreach ( var slot in Weapons )
			filled += slot.SlotUsage;

		return filled;
	}

	public enum AmmoEnum
	{
		None,
		Pistol,
		Shotgun,
		Rifle,
		Magnum,
	}

	float rifleWeight = 0.22f;
	float shotgunWeight = 1.26f;

	public int TakeAmmo( AmmoEnum type, int take = 1 )
	{
		if ( Game.IsClient ) return 0;

		int available = 0;

		switch( type )
		{
			case AmmoEnum.Pistol: available = PistolAmmo; break;
			case AmmoEnum.Shotgun: available = ShotgunAmmo; break;
			case AmmoEnum.Rifle: available = RifleAmmo; break;
			case AmmoEnum.Magnum: available = MagnumAmmo; break;
		}

		take = Math.Min( available, take );
		UpdateWeight( type, take, false );

		switch ( type )
		{
			case AmmoEnum.Pistol: PistolAmmo -= take; break;
			case AmmoEnum.Shotgun: ShotgunAmmo -= take; break;
			case AmmoEnum.Rifle: RifleAmmo -= take; break;
			case AmmoEnum.Magnum: MagnumAmmo -= take; break;
		}

		return available;
	}

	public void UpdateWeight( AmmoEnum type, int amtMulti, bool adding = true)
	{
		float amount = 0.0f;

		switch(type)
		{
			case AmmoEnum.Pistol: amount = 0.25f; break;

			case AmmoEnum.Shotgun: amount = shotgunWeight; break;

			case AmmoEnum.Rifle: amount = rifleWeight; break;

			case AmmoEnum.Magnum: amount = 1.5f; break;
		}

		if ( !adding )
			amount = -amount;

		switch ( type )
		{
			case AmmoEnum.Pistol: Weight += amount * amtMulti; break;

			case AmmoEnum.Shotgun: Weight += amount * amtMulti; break;

			case AmmoEnum.Rifle: Weight += amount * amtMulti; break;

			case AmmoEnum.Magnum: Weight += amount * amtMulti; break;
		}

		Weight = Weight.Clamp( 0, 30.25f );
	}

	public bool CanAdd()
	{
		return Weight < 30.25f;
	}

	public int GetAmmo( Weapon.TypeEnum weapon )
	{
		return weapon switch
		{
			Weapon.TypeEnum.Pistol => PistolAmmo,
			Weapon.TypeEnum.Shotgun => ShotgunAmmo,
			Weapon.TypeEnum.Rifle => RifleAmmo,
			Weapon.TypeEnum.Magnum => MagnumAmmo,
			_ => 0
		};
	}

	public int GetAmmo(AmmoEnum ammoType)
	{
		return ammoType switch
		{
			AmmoEnum.Pistol => PistolAmmo,
			AmmoEnum.Shotgun => ShotgunAmmo,
			AmmoEnum.Rifle => RifleAmmo,
			AmmoEnum.Magnum => MagnumAmmo,
			_ => 0
		};
	}

	public bool CheckWeight( AmmoEnum ammoType, int amount = 1 )
	{
		float check = Weight;

		switch ( ammoType )
		{
			case AmmoEnum.Pistol: check += 0.25f * amount; break;
			case AmmoEnum.Shotgun: check += shotgunWeight * amount; break;
			case AmmoEnum.Rifle: check += rifleWeight * amount; break;
			case AmmoEnum.Magnum: check += 1.5f * amount; break;
		}

		return check < 30.25f;
	}

	public int CalculateAmmo( AmmoEnum ammoType, int amt )
	{
		int loopTimes = 0;

		switch ( ammoType )
		{
			case AmmoEnum.Pistol: loopTimes = 15; break;
			case AmmoEnum.Shotgun: loopTimes = 8; break;
			case AmmoEnum.Rifle: loopTimes = 30; break;
			case AmmoEnum.Magnum: loopTimes = 6; break;
		}

		for ( int i = 1; i <= loopTimes; i++ )
		{
			int check = amt - i;

			if ( check < amt)
			{
				amt = check;
				break;
			}
		}

		return amt;
	}

	public void AddAmmo( AmmoEnum ammoType, int amount = 1 )
	{
		if ( !CheckWeight( ammoType, amount ) )
			amount = CalculateAmmo( ammoType, amount );

		UpdateWeight(ammoType, amount);

		switch ( ammoType )
		{
			case AmmoEnum.Pistol: PistolAmmo += amount; break;
			case AmmoEnum.Shotgun: ShotgunAmmo += amount; break;
			case AmmoEnum.Rifle: RifleAmmo += amount; break;
			case AmmoEnum.Magnum: MagnumAmmo += amount; break;
		}

		PistolAmmo = PistolAmmo.Clamp( 0, 142 );
		ShotgunAmmo = ShotgunAmmo.Clamp( 0, 24 );
		RifleAmmo = RifleAmmo.Clamp( 0, 142 );
		MagnumAmmo = MagnumAmmo.Clamp( 0, 46 );
	}

	public bool AddWeapon( Weapon weapon, bool makeActive = true )
	{
		if ( GetWeaponSlotUsage() >= 5 ) return false;

		Weapons.Add( weapon );

		weapon.EnableAllCollisions = false;
		weapon.Position = Entity.Position;
		weapon.SetParent( Entity );
		weapon.EnableDrawing = false;

		if ( makeActive )
			SetActiveWeapon( weapon );

		return true;
	}

	public bool RemoveWeapon( Weapon weapon, bool drop = false )
	{
		var success = Weapons.Remove( weapon );

		if ( success && drop )
		{
			weapon.OnDrop( Entity );
			weapon.Position = Entity.EyePosition + Entity.EyeRotation.Forward * 5;
			weapon.Velocity = Entity.Velocity + (Entity.EyeRotation.Forward + Entity.EyeRotation.Up) * 150;

			ActiveWeapon = null;

			if( Weapons.Count() > 0)
				SetActiveWeapon( Weapons.OrderBy( x => Guid.NewGuid() ).FirstOrDefault() );
		}

		return success;
	}

	public bool Clear()
	{
		foreach ( var weapon in Weapons.ToArray() )
		{
			if ( weapon == ActiveWeapon )
				ActiveWeapon.Cleanup();

			weapon?.Delete();
		}

		Weapons.Clear();
		return true;
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		if ( weapon == null )
		{
			ActiveWeapon = null;
			return;
		}
		var currentWeapon = ActiveWeapon;
		if ( currentWeapon.IsValid() )
		{
			// Can reject holster if we're doing an action already
			if ( !currentWeapon.CanHolster( Entity ) )
			{
				return;
			}

			currentWeapon.OnHolster( Entity );
			ActiveWeapon = null;
		}

		// Can reject deploy if we're doing an action already
		if ( !weapon.CanDeploy( Entity ) )
		{
			return;
		}

		ActiveWeapon = weapon;

		weapon?.OnDeploy( Entity );
	}

	protected override void OnDeactivate()
	{
		if ( Game.IsServer )
		{
			Weapons.ToList()
				.ForEach( x => x.Delete() );
		}
	}

	public Weapon GetSlot( int slot )
	{
		return Weapons.ElementAtOrDefault( slot ) ?? null;
	}

	protected int GetSlotIndexFromInput( InputButton slot )
	{
		return slot switch
		{
			InputButton.Slot1 => 0,
			InputButton.Slot2 => 1,
			InputButton.Slot3 => 2,
			InputButton.Slot4 => 3,
			InputButton.Slot5 => 4,
			_ => -1
		};
	}

	protected void TrySlotFromInput( InputButton slot )
	{
		if ( Input.Pressed( slot ) )
		{
			Input.SuppressButton( slot );

			if ( GetSlot( GetSlotIndexFromInput( slot ) ) is Weapon weapon )
			{
				Entity.ActiveWeaponInput = weapon;
			}
		}
	}

	public void BuildInput()
	{
		TrySlotFromInput( InputButton.Slot1 );
		TrySlotFromInput( InputButton.Slot2 );
		TrySlotFromInput( InputButton.Slot3 );
		TrySlotFromInput( InputButton.Slot4 );
		TrySlotFromInput( InputButton.Slot5 );

		ActiveWeapon?.BuildInput();
	}

	public void Simulate( IClient cl )
	{
		if ( Input.Pressed( InputButton.Drop ) )
			RemoveWeapon( ActiveWeapon, true );

		if ( Entity.ActiveWeaponInput != null && ActiveWeapon != Entity.ActiveWeaponInput )
		{
			var equipWep = Entity.ActiveWeaponInput as Weapon;

			SetActiveWeapon( equipWep );
			equipWep.OnDeploy(Entity);
			Entity.ActiveWeaponInput = null;
		}

		if( ActiveWeapon != null )
			ActiveWeapon?.Simulate( cl );
	}

	public void FrameSimulate( IClient cl )
	{
		ActiveWeapon?.FrameSimulate( cl );
	}
}
