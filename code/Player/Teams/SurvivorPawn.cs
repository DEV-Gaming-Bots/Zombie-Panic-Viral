namespace ZPViral.Player;

public partial class SurvivorPawn : PlayerPawn
{
	[Net] public bool IsInfected { get; set; }
	TimeUntil timeInfection;
	[Net] public float Armor { get; set; }

	public enum InfectionEnum
	{
		None,
		Window,
		Symptom,
		Transform,
	}

	InfectionEnum infectStatus;

	public void Infect()
	{
		IsInfected = true;
		infectStatus = InfectionEnum.Window;
		timeInfection = 20.0f;
	}

	public override void SetSpawnPosition()
	{
		var spawnpoint = All.OfType<ZPVSpawn>().Where(x => x.TeamSpawnpoint == ZPVSpawn.TeamSpawnEnum.Survivor)
			.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( spawnpoint != null )
		{
			Transform = spawnpoint.Transform;
			SetViewAngles( spawnpoint.Rotation.Angles() );
			ResetInterpolation();
		}
	}

	public override void CreateHull()
	{
		//SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
		//SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
	
		base.CreateHull();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if( IsInfected && Game.IsServer )
		{
			if ( infectStatus != InfectionEnum.Window && infectStatus != InfectionEnum.None )
				DoInfectionEffects(To.Single(this), infectStatus);

			if ( timeInfection > 0.0f ) return;

			switch( infectStatus )
			{
				case InfectionEnum.Window:
					infectStatus = InfectionEnum.Symptom;
					PlaySoundClientside( To.Single(this), "infection_jolt" );
					timeInfection = 20.0f;
					break;
				case InfectionEnum.Symptom:
					infectStatus = InfectionEnum.Transform;
					PlaySoundClientside( To.Single( this ), "infection_transform" );
					timeInfection = 7.5f;
					break;
				case InfectionEnum.Transform:
					ZPVGame.UpdatePawnWithPosition( Client, TeamEnum.Zombie, Position, Rotation.Angles() );
					ZPVGame.Instance.AddZombieLive();
					ZPVGame.Instance.CheckRoundStatus();
					break;
			}
		}
	}



	public override void Spawn()
	{
		CleanUpCameraEffects( To.Single( this ) );

		infectStatus = InfectionEnum.None;

		RenderColor = Color.White;

		SetModel( "models/citizen/citizen.vmdl" );
		Tags.Add( "survivor" );

		LifeState = LifeState.Alive;
		Health = 100;
		Armor = 0;

		Components.Create<Controller>();
		Components.Create<Inventory>();
		Components.Create<PlayerAnimator>();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetSpawnPosition();
		CreateHull();
	}

	float saturation = 1.0f;
	float effectSpeed = 2.0f;
	float brightness = 1.0f;

	[ClientRpc]
	void DoInfectionEffects( InfectionEnum status = InfectionEnum.None )
	{
		var postProcess = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();

		if( status == InfectionEnum.Symptom )
		{
			saturation = MathX.Lerp( postProcess.Saturation, 0.1f, Time.Delta / effectSpeed );
			postProcess.Saturation = saturation;
		}

		if( status == InfectionEnum.Transform )
		{
			brightness = MathX.Lerp( postProcess.Brightness, 0.0075f, Time.Delta / effectSpeed * 2 );
			postProcess.Brightness = brightness;
		}
	}
	public override async void AsyncRespawn()
	{
		await GameTask.DelaySeconds( 7.5f );
		ZPVGame.UpdatePawn( Client, TeamEnum.Zombie );
	}

	public override void TakeDamage( DamageInfo info )
	{
		float damage = info.Damage;
		float absorb = 0;
		if ( Armor > 0 )
		{
			absorb = damage * 0.65f;
			Armor -= absorb.CeilToInt();
			Armor = Armor.Clamp( 0, 100 );
		}

		if ( absorb > 0.0f )
			info.Damage = damage - absorb;

		base.TakeDamage( info );
	}
	public override void OnKilled()
	{
		ZPVGame.Instance.AddZombieLive();
		base.OnKilled();
		ZPVGame.Instance.CheckRoundStatus();
	}
}

