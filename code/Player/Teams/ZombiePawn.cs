namespace ZPViral.Player;

public partial class ZombiePawn : PlayerPawn
{
	public override void SetSpawnPosition()
	{
		var spawnpoint = All.OfType<ZPVSpawn>().Where( x => x.TeamSpawnpoint == ZPVSpawn.TeamSpawnEnum.Zombie )
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

	public override void Spawn()
	{
		CleanUpCameraEffects( To.Single(this) );

		SetModel( "models/citizen/citizen.vmdl" );
		Tags.Add( "zombie" );

		RenderColor = Color.Red;
		LifeState = LifeState.Alive;
		
		if ( ZombieType == ZombieEnum.Standard )
			Health = 200;
		else
			Health = 250;

		Components.Create<Controller>();
		Components.Create<Inventory>();
		Components.Create<PlayerAnimator>();

		Inventory.AddWeapon( new ZombieArms() );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetSpawnPosition();
		CreateHull();
	}

	public override void TakeDamage( DamageInfo info )
	{
		info.Damage *= ScaleHitDamage( info.Hitbox );

		base.TakeDamage( info );
	}
	public override async void AsyncRespawn()
	{
		await GameTask.DelaySeconds( 7.5f );

		if ( ZPVGame.Instance.ZombieLives > 0 )
			Spawn();
		else
			ZPVGame.Instance.CheckRoundStatus();
	}

	public override void OnKilled()
	{
		ZPVGame.Instance.TakeZombieLive();
		base.OnKilled();
	}
}
