namespace ZPViral.Player;

public partial class Controller : EntityComponent<PlayerPawn>, ISingletonComponent
{
	[ConVar.Replicated( "debug_controller" )]
	public static bool Debug { get; set; } = false;
	public Vector3 LastVelocity { get; set; }
	public Entity LastGroundEntity { get; set; }
	public Entity GroundEntity { get; set; }
	public Vector3 BaseVelocity { get; set; }
	public Vector3 GroundNormal { get; set; }
	public float CurrentGroundAngle { get; set; }
	public float StopSpeed => 150f;
	public float StepSize => 18.0f;
	public float GroundAngle => 46.0f;
	public float DefaultSpeed => 180;
	public float WalkSpeed => 140f;
	public float GroundFriction => 4.0f;
	public float MaxNonJumpVelocity => 140.0f;
	public float SurfaceFriction { get; set; } = 1f;
	public float Acceleration => 6f;
	public float DuckAcceleration => 5f;
	public float Gravity => 800.0f;
	public float AirControl => 30.0f;
	public float AirAcceleration => 35.0f;
	public float SprintSpeed => 320f;
	public float CrouchSpeed => 110f;
	public float WishCrouchSpeed => 120f;
	public float EyeCrouchHeight => 32f;
	public float StandingEyeHeight => 64.0f;
	public float JumpAmount => 350f;

	public bool IsNoclipping = false;

	public bool IsCrouching = false;
	[Net, Predicted] public float CurrentEyeHeight { get; set; } = 64f;
	public PlayerPawn Player => Entity;

	protected override void OnActivate()
	{
		base.OnActivate();
	}

	[ConCmd.Server("noclip")]
	public static void DoNoclip()
	{
		if ( !ZPVGame.Debugging ) return;

		var player = ConsoleSystem.Caller.Pawn as PlayerPawn;
		if ( player == null ) return;

		player.Controller.IsNoclipping = !player.Controller.IsNoclipping;
	}

	public Vector3 Position
	{
		get => Player.Position;
		set => Player.Position = value;
	}

	public Vector3 Velocity
	{
		get => Player.Velocity;
		set => Player.Velocity = value;
	}
	public float BodyGirth => 32f;

	public BBox Hull
	{
		get
		{
			var girth = BodyGirth * 0.5f;
			var baseHeight = CurrentEyeHeight;

			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, baseHeight * 1.1f );

			return new BBox( mins, maxs );
		}
	}

	protected void SimulateEyes()
	{
		Player.EyeRotation = Player.LookInput.ToRotation();
		Player.EyeLocalPosition = Vector3.Up * CurrentEyeHeight;
	}

	public virtual float? WishSpeed => 200f;

	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		if ( liftHead > 0 )
		{
			end += Vector3.Up * liftHead;
		}

		string[] tags = new string[3];

		tags[0] = "solid";
		tags[1] = "playerclip";

		if ( Player is SurvivorPawn )
			tags[2] = "zombie";
		else if ( Player is ZombiePawn )
			tags[2] = "survivor";

		var tr = Trace.Ray( start, end )
			.Size( mins, maxs )
			.WithAnyTags( tags )
			.Ignore( Player )
			.Run();

		return tr;
	}

	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		var hull = Hull;
		return TraceBBox( start, end, hull.Mins, hull.Maxs, liftFeet, liftHead );
	}

	public Vector3 GetWishVelocity( bool zeroPitch = false )
	{
		var result = new Vector3( Player.MoveInput.x, Player.MoveInput.y, 0 );
		result *= Vector3.One;

		var inSpeed = result.Length.Clamp( 0, 1 );
		result *= Player.LookInput.WithPitch( 0f ).ToRotation();

		if ( zeroPitch )
			result.z = 0;

		float speed = GetWishSpeed();

		result = result.Normal * inSpeed;
		result *= speed;

		var ang = CurrentGroundAngle.Remap( 0, 45, 1, 0.6f );
		result *= ang;

		return result;
	}

	public void NoclipSimulate()
	{
		var fwd = Player.MoveInput.x.Clamp( -1f, 1f );
		var left = Player.MoveInput.y.Clamp( -1f, 1f );
		var rotation = Player.LookInput.ToRotation();

		var vel = (rotation.Forward * fwd) + (rotation.Left * left);

		if ( Input.Down( InputButton.Jump ) )
			vel += Vector3.Up * 1;

		vel = vel.Normal * 2000;

		if ( Input.Down( InputButton.Run ) )
			vel *= 5.0f;

		if ( Input.Down( InputButton.Duck ) )
			vel *= 0.2f;

		Velocity += vel * Time.Delta;

		if ( Velocity.LengthSquared > 0.01f )
			Position += Velocity * Time.Delta;

		Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

		GroundEntity = null;
		BaseVelocity = Vector3.Zero;
	}

	public virtual float GetWishSpeed()
	{
		if ( Input.Down( InputButton.Duck ) )
			return CrouchSpeed;

		if ( Input.Down( InputButton.Walk ) )
			return WalkSpeed;

		//TEMPORARY COMMENT, will uncomment later
		//if ( Input.Down(InputButton.Run) )
		//	return SprintSpeed;

		return DefaultSpeed;
	}

	public void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		var currentspeed = Velocity.Dot( wishdir );
		var addspeed = wishspeed - currentspeed;

		if ( addspeed <= 0 )
			return;

		var accelspeed = acceleration * Time.Delta * wishspeed;

		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Velocity += wishdir * accelspeed;
	}

	public void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
	{
		var speed = Velocity.Length;
		if ( speed.AlmostEqual( 0f ) ) return;

		var control = (speed < stopSpeed) ? stopSpeed : speed;
		var drop = control * Time.Delta * frictionAmount;

		// Scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}
	}

	public void StepMove( float groundAngle = 46f, float stepSize = 18f )
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Hull )
			.Ignore( Player );

		//Log.Info( Hull );

		mover.MaxStandableAngle = groundAngle;

		mover.TryMoveWithStep( Time.Delta, stepSize );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	public void Move( float groundAngle = 46f )
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Hull )
			.Ignore( Player );

		mover.MaxStandableAngle = groundAngle;

		mover.TryMove( Time.Delta );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	void Jump()
	{
		float flGroundFactor = 1.0f;
		float startz = Velocity.z;

		Velocity = Velocity.WithZ( startz + JumpAmount * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		ClearGroundEntity();
	}

	void AirMove()
	{
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;
		BaseVelocity = BaseVelocity.WithZ( 0 );

		var groundedAtStart = GroundEntity.IsValid() ;
		if ( groundedAtStart )
			return;

		var wishVel = GetWishVelocity( true );
		var wishdir = wishVel.Normal;
		var wishspeed = wishVel.Length;

		Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );
		Velocity += BaseVelocity;

		Move();

		Velocity -= BaseVelocity;
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
	}

	float crouchSpeed = 5.0f;
	float curEyeHeight = 64.0f;

	public void DoCrouching(bool should)
	{
		IsCrouching = should;

		if ( should )
		{
			curEyeHeight = MathX.Lerp( curEyeHeight, EyeCrouchHeight, Time.Delta * crouchSpeed );
		}
		else
		{
			curEyeHeight = MathX.Lerp( curEyeHeight, StandingEyeHeight, Time.Delta * crouchSpeed );

			if ( curEyeHeight > 63.99f )
				curEyeHeight = 64.0f;
		}

		CurrentEyeHeight = curEyeHeight;
	}

	public virtual void Simulate( IClient cl )
	{
		if ( Player.FreezeMovement ) return;

		SimulateEyes();

		if( IsNoclipping )
		{
			NoclipSimulate();
			return;
		}

		if ( Input.Pressed( InputButton.Jump ) && GroundEntity != null )
			Jump();

		if ( GroundEntity != null )
			WalkMove();

		AirMove();

		DoCrouching( Input.Down(InputButton.Duck) );

		CategorizePosition( GroundEntity != null );

		if ( Debug )
		{
			var hull = Hull;
			DebugOverlay.Box( Position, hull.Mins, hull.Maxs, Color.Red );
			DebugOverlay.Box( Position, hull.Mins, hull.Maxs, Color.Blue );

			var lineOffset = 0;

			DebugOverlay.ScreenText( $"Player Controller", ++lineOffset );
			DebugOverlay.ScreenText( $"       Position: {Position}", ++lineOffset );
			DebugOverlay.ScreenText( $"        Velocity: {Velocity}", ++lineOffset );
			DebugOverlay.ScreenText( $"    BaseVelocity: {BaseVelocity}", ++lineOffset );
			DebugOverlay.ScreenText( $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]", ++lineOffset );
			DebugOverlay.ScreenText( $"           Speed: {Velocity.Length}", ++lineOffset );
		}
	}

	public void ClearGroundEntity()
	{
		if ( GroundEntity == null ) return;

		LastGroundEntity = GroundEntity;
		GroundEntity = null;
		SurfaceFriction = 1.0f;
	}

	public void SetGroundEntity( Entity entity )
	{
		LastGroundEntity = GroundEntity;
		LastVelocity = Velocity;

		GroundEntity = entity;

		if ( GroundEntity != null )
		{
			Velocity = Velocity.WithZ( 0 );
			BaseVelocity = GroundEntity.Velocity;
		}
	}

	private void UpdateGroundEntity( TraceResult tr )
	{
		GroundNormal = tr.Normal;

		SurfaceFriction = tr.Surface.Friction * 1.25f;
		if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

		SetGroundEntity( tr.Entity );
	}

	public void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		var point = Position - Vector3.Up * 2;
		var vBumpOrigin = Position;
		bool bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
		bool bMoveToEndPos = false;

		if ( GroundEntity != null )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}

		if ( bMovingUpRapidly )
		{
			ClearGroundEntity();
			return;
		}

		var pm = TraceBBox( vBumpOrigin, point, 4.0f );

		var angle = Vector3.GetAngle( Vector3.Up, pm.Normal );
		CurrentGroundAngle = angle;

		if ( pm.Entity != null && pm.Entity.GetType() != Player.GetType() )
		{
			bMoveToEndPos = false;
			UpdateGroundEntity( pm );
			return;
		}

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Position = pm.EndPosition;
		}
	}

	public virtual void FrameSimulate( IClient cl )
	{
		SimulateEyes();
	}

	private void StayOnGround()
	{
		var start = Position + Vector3.Up * 2;
		var end = Position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = TraceBBox( Position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

		Position = trace.EndPosition;
	}

	private void WalkMove()
	{
		var wishVel = GetWishVelocity( true );
		var wishdir = wishVel.Normal;
		var wishspeed = wishVel.Length;
		var friction = GroundFriction * SurfaceFriction;

		Velocity = Velocity.WithZ( 0 );
		ApplyFriction( StopSpeed, friction );

		var accel = Acceleration;

		Velocity = Velocity.WithZ( 0 );
		Accelerate( wishdir, wishspeed, 0, accel );
		Velocity = Velocity.WithZ( 0 );

		// Add in any base velocity to the current velocity.
		Velocity += BaseVelocity;

		try
		{
			if ( Velocity.Length < 1.0f )
			{
				Velocity = Vector3.Zero;
				return;
			}

			var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );
			var pm = TraceBBox( Position, dest );

			if ( pm.Fraction == 1 )
			{
				Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			StepMove();
		}
		finally
		{
			Velocity -= BaseVelocity;
		}

		StayOnGround();
	}
}

