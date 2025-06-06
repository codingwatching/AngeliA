using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Entity that spawn a file entity repeately
/// </summary>
/// <typeparam name="F">Type of the fire entity</typeparam>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class Burner<F> : Entity, IBlockEntity where F : Fire {




	#region --- VAR ---


	// Api
	/// <summary>
	/// How many frame does it take to spawn another file
	/// </summary>
	protected virtual int FireFrequency => 480;
	/// <summary>
	/// How long does a single file stay
	/// </summary>
	protected virtual int FireDuration => FireFrequency / 4;
	/// <summary>
	/// Direction of the file
	/// </summary>
	protected virtual Direction4 Direction => Direction4.Up;
	/// <summary>
	/// Read fire type from overlaping map element block
	/// </summary>
	protected virtual bool AllowFireFromMapElement => true;

	// Data
	private int FireTypeID = 0;
	private int FireFrameOffset = 0;
	private int NextFireSpawnedFrame = int.MinValue;
	private bool Burning = false;
	private Color32 FireTint = Color32.WHITE;


	#endregion




	#region --- MSG ---


	public Burner () => FireTypeID = typeof(F).AngeHash();


	public override void OnActivated () {

		base.OnActivated();

		var dirNormal = Direction.Normal();

		NextFireSpawnedFrame = int.MinValue;
		Burning = false;

		// Get Fire Type from Map
		if (AllowFireFromMapElement) {
			int mapFireID = WorldSquad.Front.GetBlockAt(
				(X + 1).ToUnit() + dirNormal.x,
				(Y + 1).ToUnit() + dirNormal.y,
				BlockType.Entity
			);
			if (mapFireID != 0) {
				var fireType = Stage.GetEntityType(mapFireID);
				if (fireType != null && fireType.IsSubclassOf(typeof(Fire))) {
					FireTypeID = mapFireID;
				}
			}
		}

		// Get Fire Tint
		if (Renderer.TryGetSprite(FireTypeID, out var fSprite, false)) {
			FireTint = fSprite.SummaryTint;
		} else {
			FireTint = Color32.WHITE;
		}
		FireTint.a = 96;

		// Fire Offset
		FireFrameOffset = 0;
		if (FrameworkUtil.TryGetSingleSystemNumber(
			WorldSquad.Front, (X + 1).ToUnit() - dirNormal.x, (Y + 1).ToUnit() - dirNormal.y, Stage.ViewZ, out int fireOffset
		)) {
			FireFrameOffset = FireFrequency * fireOffset.Clamp(0, 9) / 10;
		}
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}


	public override void Update () {
		base.Update();
		int localFrame = (Game.SettleFrame - FireFrameOffset).UMod(FireFrequency);
		Burning = localFrame < FireDuration;
		// Spawn Fire
		if (
			localFrame < FireDuration &&
			Game.GlobalFrame >= NextFireSpawnedFrame &&
			Stage.TrySpawnEntity(FireTypeID, X, Y, out var entity) &&
			entity is Fire fire
		) {
			fire.Setup(FireDuration - localFrame, Direction, Width, Height, damageImmediately: true);
			fire.X =
				Direction == Direction4.Left ? X - Const.CEL :
				Direction == Direction4.Right ? X + Const.CEL :
				X;
			fire.Y =
				Direction == Direction4.Up ? Y + Const.CEL :
				Direction == Direction4.Down ? Y - Const.CEL :
				Y;
			NextFireSpawnedFrame = Game.GlobalFrame + FireFrequency - localFrame;
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		// Body
		Draw();
		// Fire
		if (Burning && Game.GlobalFrame % 6 < 3) {
			Renderer.SetLayerToAdditive();
			Renderer.Draw(TypeID, Rect, FireTint);
			Renderer.SetLayerToDefault();
		}
	}


	#endregion




}
