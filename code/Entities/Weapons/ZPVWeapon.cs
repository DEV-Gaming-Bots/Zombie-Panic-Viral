namespace ZPViral.Weapons;

public partial class Weapon : AnimatedEntity, IUse
{
	public AnimatedEntity EffectEntity => ViewModelEntity.IsValid() ? ViewModelEntity : this;
	public WeaponViewModel ViewModelEntity { get; protected set; }
	public ArmVM ArmsVMEntity { get; protected set; }
	public PlayerPawn Player => Owner as PlayerPawn;
	[Net, Predicted] public bool IsActive { get; protected set; }
	[Net, Predicted] public TimeSince TimeSinceActivated { get; protected set; }

	public override void Spawn()
	{
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = true;

		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		Model = WorldModel;

		Tags.Add( "zpvitem" );

		AmmoCount = DefaultAmmo;
	}

	/// <summary>
	/// Can we holster the weapon right now? Reasons to reject this could be that we're reloading the weapon..
	/// </summary>
	/// <returns></returns>
	public bool CanHolster( PlayerPawn player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets holstered.
	/// </summary>
	public void OnHolster( PlayerPawn player )
	{
		EnableDrawing = false;
		if ( ReloadLock )
		{
			ReloadLock = false;
			FinishReloading( Player, true );
			FinishUnloading( Player, true );
		}
	}

	public void Cleanup()
	{
		if ( Game.IsClient ) return;

		DestroyViewModel( To.Single( Player.Client ) );
	}

	public void OnDrop(PlayerPawn player)
	{
		if ( Game.IsClient ) return;

		DestroyViewModel( To.Single( Player.Client ) );

		if(ReloadLock)
		{
			ReloadLock = false;
			FinishReloading( Player, true );
			FinishUnloading( Player, true );
		}

		SetParent( null );
		Owner = null;
		EnableDrawing = true;
		EnableAllCollisions = true;
	}

	/// <summary>
	/// Can we deploy this weapon? Reasons to reject this could be that we're performing an action.
	/// </summary>
	/// <returns></returns>
	public bool CanDeploy( PlayerPawn player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets deployed.
	/// </summary>
	public void OnDeploy( PlayerPawn player )
	{
		SetParent( player, true );
		Owner = player;

		EnableDrawing = true;

		TimeSinceActivated = 0;

		Player.PlaySound( DrawSound );

		if ( Game.IsServer )
		{
			CreateViewModel( To.Single( player ) );
			AmmoStateAnim( To.Single( Player.Client ), AmmoCount <= 0 );
		}
	}

	[ClientRpc]
	public void AmmoStateAnim(bool isEmpty)
	{
		Game.AssertClient();

		WeaponViewModel.Current?.SetAnimParameter( "empty", isEmpty );
		ArmVM.Current?.SetAnimParameter( "empty", isEmpty );
	}

	PlayerPawn.SurvivorType setType = PlayerPawn.SurvivorType.Random;
	Model curArms = null;

	Model GetPreferredArmModel()
	{
		if(Player.Survivor != PlayerPawn.SurvivorType.Random)
		{
			setType = Player.Survivor;

			switch ( setType )
			{
				case PlayerPawn.SurvivorType.Eugene: return Model.Load( "models/arms/c_arms_eugene.vmdl" );
				case PlayerPawn.SurvivorType.Jessica: return Model.Load( "models/arms/c_arms_jessica.vmdl" );
			}

		} 
		else
		{
			if ( curArms != null )
				return curArms;

			int random = Game.Random.Int( 1, 2 );

			switch( random )
			{
				case 1: setType = PlayerPawn.SurvivorType.Eugene; break;
				case 2: setType = PlayerPawn.SurvivorType.Jessica; break;
			}

			switch ( setType )
			{
				case PlayerPawn.SurvivorType.Eugene: return Model.Load( "models/arms/c_arms_eugene.vmdl" );
				case PlayerPawn.SurvivorType.Jessica: return Model.Load( "models/arms/c_arms_jessica.vmdl" );
			}
		}


		return null;
	}

	[ClientRpc]
	public void CreateViewModel()
	{
		var vm = new WeaponViewModel( this );
		vm.Model = ViewModel;
		ViewModelEntity = vm;

		if ( Player is ZombiePawn ) return;

		var arms = new ArmVM( this );

		if ( curArms == null && Player.Survivor == PlayerPawn.SurvivorType.Random )
		{
			curArms = GetPreferredArmModel();
			arms.Model = curArms;
		}
		else
			arms.Model = GetPreferredArmModel();

		ArmsVMEntity = arms;

		ArmsVMEntity.SetArmAnimations();
	}

	[ClientRpc]
	public void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ArmsVMEntity?.Delete();

		ViewModelEntity = null;
		ArmsVMEntity = null;
	}

	TimeUntil timeToUnload;

	public override void Simulate( IClient cl )
	{
		SimulateRecoil(cl, Player);

		if ( CanUnload( Player ) )
		{
			if ( timeToUnload < -.1f )
				timeToUnload = 0.5f;

			if ( timeToUnload <= 0.0f )
				DoUnload( Player );

			return;
		} 
		else if ( Input.Released(InputButton.Reload) )
		{
			if( CanReload( Player ) )
				DoReload( Player );

			timeToUnload = -.1f;
		}

		if( ReloadLock && TimeUntilReloaded <= 0.0f )
		{
			if( WeaponType == TypeEnum.Shotgun )
			{
				if ( Tags.Has( "reloading" ) )
					DoReload( Player );

				if ( Tags.Has( "unloading" ) )
					DoUnload(Player);
			}
			else
			{
				if ( Tags.Has( "reloading" ) )
					FinishReloading( Player );
				else if ( Tags.Has( "unloading" ) )
					FinishUnloading( Player );

				ReloadLock = false;
			}
		}

		if( CanFire(Player) )
		{
			using ( LagCompensation() )
			{
				Fire(Player);
			}
		}
	}

	protected override void OnDestroy()
	{
		ViewModelEntity?.Delete();
		ArmsVMEntity?.Delete();
	}

	public bool OnUse( Entity user )
	{
		if(user is PlayerPawn player)
		{
			//TODO, check if its a survivor

			player.Inventory.AddWeapon( this, false );
				//Delete();
		}

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}
}
