﻿using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class OneUpMushroom : Mushroom {
	protected override bool Heal => false;
	protected override bool GiveExtraLife => true;
}


public class Mushroom : Rigidbody, IPingPongWalker, IAutoTrackWalker, IBumpable {

	// VAR
	private static readonly AudioCode ONE_UP_AC = "OneUp";
	private static readonly AudioCode MUSHROOM_AC = "GetPowerUp";
	public override int SelfCollisionMask => PhysicsMask.MAP;
	public override int PhysicalLayer => PhysicsLayer.ITEM;
	public override bool CarryOtherOnTop => false;
	protected virtual bool Heal => true;
	protected virtual bool GiveExtraLife => false;
	int IPingPongWalker.WalkSpeed => 16;
	bool IPingPongWalker.WalkOffEdge => true;
	int IPingPongWalker.LastTurnFrame { get; set; }
	bool IPingPongWalker.WalkingRight { get; set; }
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }
	int IBumpable.LastBumpedFrame { get; set; }
	Direction4 IBumpable.LastBumpFrom { get; set; }
	bool IBumpable.TransferBumpFromOther => true;
	bool IBumpable.TransferBumpToOther => false;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IPingPongWalker.OnActivated(this);
	}

	public override void Update () {
		base.Update();
		// Walk
		IPingPongWalker.PingPongWalk(this);
		// Collect Check
		var player = PlayerSystem.Selecting;
		if (player != null && player.Rect.Overlaps(Rect)) {
			if (Heal) {
				player.Health.Heal(1);
				player.Bounce();
				FrameworkUtil.PlaySoundAtPosition(MUSHROOM_AC, XY);
			}
			if (GiveExtraLife) {
				// ※ No Life Count System ※
				MarioUtil.GiveScore(500, CenterX, Y + Height);
				FrameworkUtil.PlaySoundAtPosition(ONE_UP_AC, XY);
			}
			Active = false;
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Draw();
	}

	void IBumpable.OnBumped (Entity entity, Damage damage) {
		var walker = this as IPingPongWalker;
		walker.WalkingRight = !walker.WalkingRight;
	}



}
