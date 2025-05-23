﻿using AngeliA;

namespace AngeliA.Platformer;

public class PoseAttack_Shooting : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseAttack_Shooting).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Shooting();
	}
	public static void Shooting () {

		float ease01 =
			IsChargingAttack ? 1f - AttackEase :
			Attackness.LastAttackCharged ? 1f :
			AttackEase;
		if (!IsChargingAttack) {
			AttackHeadDown(ease01, 0, 200, 0);
			AttackLegShake(ease01);
		}
		ResetShoulderAndUpperArmPos();

		// Arm
		int rotUpperL = 0;
		int rotUpperR = 0;
		int rotLowerL = 0;
		int rotLowerR = 0;
		int grabRot = 0;
		int grabScl = 1000;
		switch (Attackness.AimingDirection) {


			case Direction8.Left: {
				// L
				rotUpperL = 90;
				rotUpperR = 90;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = 0;
				grabScl = -1000;
				break;
			}
			case Direction8.Right: {
				// R
				rotUpperL = -90;
				rotUpperR = -90;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = 0;
				grabScl = 1000;
				break;
			}
			case Direction8.Top: {
				// T
				rotUpperL = -20;
				rotUpperR = 20;
				rotLowerL = FacingRight ? 0 : -140;
				rotLowerR = FacingRight ? 140 : 0;
				grabRot = FacingRight ? 90 : -90;
				grabScl = FacingRight ? -1000 : 1000;
				break;
			}
			case Direction8.Bottom: {
				// B
				rotUpperL = -20;
				rotUpperR = 20;
				rotLowerL = FacingRight ? -140 : 0;
				rotLowerR = FacingRight ? 0 : 140;
				grabRot = 90;
				grabScl = 1000;
				break;
			}

			case Direction8.TopLeft: {
				// TL
				rotUpperL = 145;
				rotUpperR = 125;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = 45;
				grabScl = -1000;
				break;
			}
			case Direction8.TopRight: {
				// TR
				rotUpperL = -125;
				rotUpperR = -145;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = -45;
				grabScl = 1000;
				break;
			}
			case Direction8.BottomLeft: {
				// BL
				rotUpperL = 35;
				rotUpperR = 55;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = -45;
				grabScl = -1000;
				break;
			}
			case Direction8.BottomRight: {
				// BR
				rotUpperL = -55;
				rotUpperR = -35;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = 45;
				grabScl = 1000;
				break;
			}

		}

		// Arm Hand
		UpperArmL.LimbRotate((int)Util.LerpUnclamped(UpperArmL.Rotation, rotUpperL, ease01));
		UpperArmR.LimbRotate((int)Util.LerpUnclamped(UpperArmR.Rotation, rotUpperR, ease01));
		LowerArmL.LimbRotate((int)Util.LerpUnclamped(LowerArmL.Rotation, rotLowerL, ease01));
		LowerArmR.LimbRotate((int)Util.LerpUnclamped(LowerArmR.Rotation, rotLowerR, ease01));
		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Z
		ShoulderL.Z = ShoulderR.Z = FrontSign * (POSE_Z_HAND - 3);
		UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
		LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
		HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

		// Grab
		Rendering.HandGrabRotationL.Override(grabRot);
		Rendering.HandGrabRotationR.Override(grabRot);
		Rendering.HandGrabScaleL.Override(grabScl);
		Rendering.HandGrabScaleR.Override(grabScl);
		Rendering.HandGrabAttackTwistL.Override(1000);
		Rendering.HandGrabAttackTwistR.Override(1000);

		// Leg
		AttackLegShake(ease01);

	}
}
