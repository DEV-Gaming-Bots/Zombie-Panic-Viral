namespace ZPViral.Player;

public partial class PlayerPawn : AnimatedEntity
{
	[BindComponent] public Controller Controller { get; }
	[ClientInput] public Vector2 MoveInput { get; protected set; }
	[ClientInput] public Angles LookInput { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }
	[ClientInput] public Entity ActiveWeaponInput { get; set; }
	[BindComponent] public Inventory Inventory { get; }
	[BindComponent] public PlayerAnimator Animator { get; }
	public DamageInfo LastDamage { get; protected set; }
	public Weapon ActiveWeapon => Inventory?.ActiveWeapon;
	public bool ShouldPlayMusic { get; set; } = true;
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	[Net, Predicted] public Vector3 EyeLocalPosition { get; set; }
	[Net, Predicted] public Rotation EyeLocalRotation { get; set; }
	[Net] public bool FreezeMovement { get; set; }
	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public enum SurvivorType
	{
		Random,
		Eugene,
		Jessica,
	}

	[Net] public SurvivorType Survivor { get; set; }

	public SurvivorType ServerSurvivor { get; set; } = SurvivorType.Random;

	bool setView;
	Angles setAngles;

	public PlayerPawn()
	{

	}

	[ClientRpc]
	public void SetViewAngles( Angles angle )
	{
		setView = true;
		setAngles = angle;
	}

	public bool HasAmmo( Weapon.AmmoEnum wepType, int amount = 1 )
	{
		switch ( wepType )
		{
			case Weapon.AmmoEnum.Pistol: return Inventory.PistolAmmo >= amount;
			case Weapon.AmmoEnum.Shotgun: return Inventory.ShotgunAmmo >= amount;
			case Weapon.AmmoEnum.Rifle: return Inventory.RifleAmmo >= amount;
			case Weapon.AmmoEnum.Magnum: return Inventory.MagnumAmmo >= amount;
		}

		return true;
	}

	public int UpdateAmmo( Weapon.TypeEnum wepType, int amount, bool add = true )
	{
		if ( add )
		{
			switch ( wepType )
			{
				case Weapon.TypeEnum.Pistol: Inventory.AddAmmo( Inventory.AmmoEnum.Pistol, amount ); break;
				case Weapon.TypeEnum.Shotgun: Inventory.AddAmmo( Inventory.AmmoEnum.Shotgun, amount ); break;
				case Weapon.TypeEnum.Rifle: Inventory.AddAmmo( Inventory.AmmoEnum.Rifle, amount ); break;
				case Weapon.TypeEnum.Magnum: Inventory.AddAmmo( Inventory.AmmoEnum.Magnum, amount ); break;
			}
		}
		else
		{
			switch ( wepType )
			{
				case Weapon.TypeEnum.Pistol: return Inventory.TakeAmmo(Inventory.AmmoEnum.Pistol, amount);
				case Weapon.TypeEnum.Shotgun: return Inventory.TakeAmmo( Inventory.AmmoEnum.Shotgun, amount );
				case Weapon.TypeEnum.Rifle: return Inventory.TakeAmmo( Inventory.AmmoEnum.Rifle, amount );
				case Weapon.TypeEnum.Magnum: return Inventory.TakeAmmo( Inventory.AmmoEnum.Magnum, amount );
			}
		}


		return 0;
	}


	public virtual void SetSpawnPosition()
	{
		var spawnpoint = All.OfType<SpawnPoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( spawnpoint != null )
		{
			Transform = spawnpoint.Transform;
			SetViewAngles( spawnpoint.Rotation.Angles() );
			ResetInterpolation();
		}
	}

	public virtual void CreateHull()
	{
		Tags.Add( "zpvpawn" );

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		EnableHitboxes = true;
		EnableLagCompensation = true;
		EnableAllCollisions = true;
	}

	public override void Spawn()
	{
		//TEMPORARY
		SetModel( "models/citizen/citizen.vmdl" );

		FreezeMovement = false;

		Components.Create<Controller>();
		Components.Create<Inventory>();
		Components.Create<PlayerAnimator>();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetSpawnPosition();
		CreateHull();
	}

	public override void BuildInput()
	{
		if ( setView )
		{
			LookInput = setAngles;
			setView = false;
		}

		if ( FreezeMovement ) return;

		Inventory?.BuildInput();

		MoveInput = Input.AnalogMove;
		var lookInput = (LookInput + Input.AnalogLook).Normal;

		LookInput = lookInput.WithPitch( lookInput.pitch.Clamp( -90f, 90f ) );
	}

	public override void Simulate( IClient cl )
	{
		TickUse();
		Controller?.Simulate( cl );
		Animator?.Simulate( cl );
		Inventory?.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		Rotation = LookInput.WithPitch( 0f ).ToRotation();

		Controller?.FrameSimulate(cl);
		Animator?.FrameSimulate(cl);

		CameraSimulate();
	}

	public TraceResult GetEyeTraceResult(float dist, float size = 1.0f)
	{
		TraceResult tr;

		tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * dist )
			.UseHitboxes()
			.Ignore( this )
			.Run();

		return tr;
	}

	TimeSince timeSinceLastFootstep;
	public virtual float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
	}
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !Game.IsClient )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	public int ScaleHitDamage(Hitbox hitbox)
	{
		if ( hitbox.HasTag( "head" ) )
			return 3;

		return 1;
	}

	public virtual async void AsyncRespawn()
	{
		await GameTask.DelaySeconds( 7.5f );
	}

	[ClientRpc]
	public void PlaySoundClientside(string path)
	{
		Sound.FromScreen( path );
	}

	public override void OnKilled()
	{
		if ( LifeState == LifeState.Alive )
		{
			CreateRagdoll( Controller.Velocity, LastDamage.Position, LastDamage.Force,
				LastDamage.BoneIndex, LastDamage.HasTag( "bullet" ), LastDamage.HasTag( "blast" ) );
			
			PhysicsClear();

			LifeState = LifeState.Dead;

			EnableAllCollisions = false;
			EnableHitboxes = false;
			EnableLagCompensation = false;
			EnableDrawing = false;

			Controller.Remove();
			Animator.Remove();
			Inventory.Remove();

			// Disable all children as well.
			Children.OfType<ModelEntity>()
				.ToList()
				.ForEach( x => x.EnableDrawing = false );

			AsyncRespawn();
		}
	}

	[ClientRpc]
	public void CleanUpCameraEffects()
	{
		var postProcess = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();

		postProcess.Saturation = 1.0f;
		postProcess.Brightness = 1.0f;
	}

	public virtual void UpdatePostProcess()
	{

	}

	public void CameraSimulate()
	{
		Camera.Position = EyePosition;
		Camera.Rotation = EyeRotation;
		Camera.FieldOfView = Game.Preferences.FieldOfView;
		Camera.FirstPersonViewer = this;
		Camera.ZNear = 0.5f;

		UpdatePostProcess();
	}
}
