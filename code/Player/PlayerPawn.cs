namespace ZPViral.Player;

public partial class PlayerPawn : AnimatedEntity
{
	[BindComponent] public Controller Controller { get; }
	[ClientInput] public Vector2 MoveInput { get; protected set; }
	[ClientInput] public Angles LookInput { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

	ClothingContainer clothing = new();

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
	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public PlayerPawn()
	{

	}

	public PlayerPawn(IClient client) : this()
	{
		clothing.LoadFromClient(client);
	}

	public void SetSpawnPosition()
	{
		var spawnpoint = All.OfType<SpawnPoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( spawnpoint != null )
		{
			var tx = spawnpoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 25.0f;
			spawnpoint.Transform = tx;
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		Components.Create<Controller>();

		clothing.DressEntity( this );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public override void BuildInput()
	{
		//Inventory?.BuildInput();

		MoveInput = Input.AnalogMove;
		var lookInput = (LookInput + Input.AnalogLook).Normal;

		// Since we're a FPS game, let's clamp the player's pitch between -90, and 90.
		LookInput = lookInput.WithPitch( lookInput.pitch.Clamp( -90f, 90f ) );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		TickUse();
		Controller?.Simulate(cl);
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Rotation = ViewAngles.ToRotation();
		Controller?.FrameSimulate(cl);
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
	public void CameraSimulate()
	{
		Camera.Position = EyePosition;
		Camera.Rotation = EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		Camera.FirstPersonViewer = this;
	}
}
