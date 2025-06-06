﻿namespace AngeliA;

public class PoseAnimation_GrabSide : PoseAnimation {

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int loop = Util.Max((700 / Movement.GrabMoveSpeedY.FinalValue.Clamp(1, 1024)) / 4 * 4, 1);
		int arrFrame = (CurrentAnimationFrame.UMod(loop) / (loop / 4)) % 4; // 0123
		int pingpong = arrFrame == 3 ? 1 : arrFrame; // 0121

		Rendering.PoseRootY -= pingpong * A2G;
		Body.Rotation = FacingSign * 4 + pingpong * 3;

		// Arm
		UpperArmL.LimbRotate(FacingSign * (-77 + (pingpong - 1) * -12), 700);
		UpperArmR.LimbRotate(FacingSign * (-77 + (pingpong - 1) * 12), 700);

		LowerArmL.LimbRotate(FacingSign * (pingpong * -28 - 70) - UpperArmL.Rotation, 700);
		LowerArmR.LimbRotate(FacingSign * ((2 - pingpong) * -28 - 70) - UpperArmR.Rotation, 700);

		HandL.LimbRotate(-FacingSign);
		HandR.LimbRotate(-FacingSign);

		// Leg
		UpperLegL.X = Body.X - A2G - Body.Width / 6;
		UpperLegR.X = Body.X + A2G - Body.Width / 6;

		UpperLegL.LimbRotate(FacingSign * (-71 + pingpong * 18));
		UpperLegR.LimbRotate(FacingSign * (-71 + pingpong * -18));

		LowerLegL.LimbRotate(-UpperLegL.Rotation - pingpong * 5);
		LowerLegR.LimbRotate(-UpperLegR.Rotation - pingpong * -5);

		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Final
		Rendering.HandGrabRotationL.Override( LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override( LowerArmR.Rotation + FacingSign * 90);

		// Z
		HandL.Z = FacingFront ? POSE_Z_HAND : -POSE_Z_HAND;
		HandR.Z = FacingFront ? POSE_Z_HAND : -POSE_Z_HAND;
	}

}
