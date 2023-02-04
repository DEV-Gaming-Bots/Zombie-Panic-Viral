using ZPViral.Weapons.FireArms;

namespace ZPViral.Weapons;

public partial class ArmVM: AnimatedEntity
{
	Vector3 SmoothedVelocity;
	Vector3 velocity;
	Vector3 acceleration;
	float VelocityClamp => 20f;
	float walkBob = 0;
	float upDownOffset = 0;
	float avoidance = 0;
	float sprintLerp = 0;
	float aimLerp = 0;
	float crouchLerp = 0;
	float airLerp = 0;
	float sideLerp = 0;

	protected float MouseDeltaLerpX;
	protected float MouseDeltaLerpY;

	Vector3 positionOffsetTarget = Vector3.Zero;
	Rotation rotationOffsetTarget = Rotation.Identity;

	Vector3 realPositionOffset;
	Rotation realRotationOffset;

	public static ArmVM Current;

	protected Weapon Weapon { get; init; }
	public ArmVM( Weapon weapon)
	{
		if ( Current.IsValid() )
		{
			Current.Delete();
		}

		Current = this;
		EnableShadowCasting = false;
		EnableViewmodelRendering = true;
		Weapon = weapon;
	}

	[ClientRpc]
	public void SetArmAnimations()
	{
		SetAnimParameter( "weapon_enum", GetWeaponAnimEnum() );
	}

	//CURRENT WEAPON ENUMS
	//0 = None
	//1 = Pistol
	//2 = Rifle
	protected int GetWeaponAnimEnum()
	{
		if ( Weapon is USP )
			return 1;

		if ( Weapon is AK47 )
			return 2;

		return 0;
	}

	protected override void OnDestroy()
	{
		Current = null;
	}

	[Event.Client.PostCamera]
	public void PlaceViewmodel()
	{
		if ( Game.IsRunningInVR )
			return;

		Camera.Main.SetViewModelCamera( 80f, 1, 500 );
		AddEffects();
	}

	public void AddEffects()
	{
		var player = Weapon.Player;
		var controller = player?.Controller;

		if ( controller == null )
			return;

		SmoothedVelocity += (controller.Velocity - SmoothedVelocity) * 5f * Time.Delta;

		var isGrounded = controller.GroundEntity != null;
		var speed = controller.Velocity.Length.LerpInverse( 0, 750 );
		var bobSpeed = SmoothedVelocity.Length.LerpInverse( -250, 700 );
		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;
		var forward = Camera.Rotation.Forward;
		var isCrouching = controller.IsCrouching;

		LerpTowards( ref crouchLerp, isCrouching ? 1f : 0f, 7f );
		LerpTowards( ref airLerp, isGrounded ? 0 : 1, 10f );

		var leftAmt = left.WithZ( 0 ).Normal.Dot( controller.Velocity.Normal );
		LerpTowards( ref sideLerp, leftAmt, 5f );

		if ( isGrounded )
			walkBob += Time.Delta * 30.0f * bobSpeed;

		walkBob %= 360;

		var mouseDeltaX = -Input.MouseDelta.x * Time.Delta * Weapon.OverallWeight;
		var mouseDeltaY = -Input.MouseDelta.y * Time.Delta * Weapon.OverallWeight;

		acceleration += Vector3.Left * mouseDeltaX;
		acceleration += Vector3.Up * mouseDeltaY;
		acceleration += -velocity * Weapon.WeightReturnForce * Time.Delta;

		// Apply horizontal offsets based on walking direction
		var horizontalForwardBob = WalkCycle( 0.5f, 3f ) * speed * Weapon.WalkCycleOffset.x * Time.Delta;

		acceleration += forward.WithZ( 0 ).Normal.Dot( controller.Velocity.Normal ) * Vector3.Forward * Weapon.BobAmount.x * horizontalForwardBob;

		// Apply left bobbing and up/down bobbing
		acceleration += Vector3.Left * WalkCycle( 0.5f, 2f ) * speed * Weapon.WalkCycleOffset.y * Time.Delta;
		acceleration += Vector3.Up * WalkCycle( 0.5f, 2f, true ) * speed * Weapon.WalkCycleOffset.z * Time.Delta;
		acceleration += left.WithZ( 0 ).Normal.Dot( controller.Velocity.Normal ) * Vector3.Left * speed * Weapon.BobAmount.y * Time.Delta;

		velocity += acceleration * Time.Delta;

		ApplyDamping( ref acceleration, Weapon.AccelerationDamping );
		ApplyDamping( ref velocity, Weapon.WeightDamping );
		velocity = velocity.Normal * Math.Clamp( velocity.Length, 0, VelocityClamp );

		var avoidanceTrace = Trace.Ray( Camera.Position, Camera.Position + forward * 50f )
			.WorldAndEntities()
			.WithoutTags( "trigger" )
			.Ignore( Weapon )
			.Ignore( this )
			.Run();

		var avoidanceVal = avoidanceTrace.Hit ? (1f - avoidanceTrace.Fraction) : 0;
		avoidanceVal *= 1 - (aimLerp * 0.8f);

		LerpTowards( ref avoidance, avoidanceVal, 10f );

		Position = Camera.Position;
		Rotation = Camera.Rotation;

		positionOffsetTarget = Vector3.Zero;
		rotationOffsetTarget = Rotation.Identity;

		{
			// Global
			rotationOffsetTarget *= Rotation.From( Weapon.GlobalAngleOffset );
			positionOffsetTarget += forward * (velocity.x * Weapon.VelocityScale + Weapon.GlobalPositionOffset.x);
			positionOffsetTarget += left * (velocity.y * Weapon.VelocityScale + Weapon.GlobalPositionOffset.y);
			positionOffsetTarget += up * (velocity.z * Weapon.VelocityScale + Weapon.GlobalPositionOffset.z + upDownOffset);

			float cycle = Time.Now * 10.0f;

			// Crouching
			rotationOffsetTarget *= Rotation.From( Weapon.CrouchAngleOffset * crouchLerp );
			ApplyPositionOffset( Weapon.CrouchPositionOffset, crouchLerp );

			// Air
			ApplyPositionOffset( new( 0, 0, 1 ), airLerp );

			// Avoidance
			rotationOffsetTarget *= Rotation.From( Weapon.AvoidanceAngleOffset * avoidance );
			ApplyPositionOffset( Weapon.AvoidancePositionOffset, avoidance );
		}

		realRotationOffset = rotationOffsetTarget;
		realPositionOffset = positionOffsetTarget;

		Rotation *= realRotationOffset;
		Position += realPositionOffset;

		Camera.FieldOfView -= 10f * aimLerp;
		Camera.Main.SetViewModelCamera( 85f, 1, 2048 );
	}

	protected void ApplyPositionOffset( Vector3 offset, float delta )
	{
		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;
		var forward = Camera.Rotation.Forward;

		positionOffsetTarget += forward * offset.x * delta;
		positionOffsetTarget += left * offset.y * delta;
		positionOffsetTarget += up * offset.z * delta;
	}

	private float WalkCycle( float speed, float power, bool abs = false )
	{
		var sin = MathF.Sin( walkBob * speed );
		var sign = Math.Sign( sin );

		if ( abs )
		{
			sign = 1;
		}

		return MathF.Pow( sin, power ) * sign;
	}

	private void LerpTowards( ref float value, float desired, float speed )
	{
		var delta = (desired - value) * speed * Time.Delta;
		var deltaAbs = MathF.Min( MathF.Abs( delta ), MathF.Abs( desired - value ) ) * MathF.Sign( delta );

		if ( MathF.Abs( desired - value ) < 0.001f )
		{
			value = desired;

			return;
		}

		value += deltaAbs;
	}

	private void ApplyDamping( ref Vector3 value, float damping )
	{
		var magnitude = value.Length;

		if ( magnitude != 0 )
		{
			var drop = magnitude * damping * Time.Delta;
			value *= Math.Max( magnitude - drop, 0 ) / magnitude;
		}
	}
}
