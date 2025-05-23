﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;


/// <summary>
/// Body cloth of a pose-style character. Include body, shoulder and arms
/// </summary>
public abstract class BodyCloth : Cloth {




	#region --- VAR ---


	// Api
	public sealed override ClothType ClothType => ClothType.Body;
	public override bool SpriteLoaded => SpriteBody.IsValid;
	/// <summary>
	/// How much does it shift left and right with character's body movement (0 means 0%, 1000 means 100%)
	/// </summary>
	protected virtual int TwistShiftTopAmount => 300;
	/// <summary>
	/// Z value for sorting rendering cells
	/// </summary>
	protected virtual int LocalZ => 7;

	// Data
	private OrientedSprite SpriteBody;
	private OrientedSprite SpriteCape;
	private OrientedSprite SpriteShoulderLeft;
	private OrientedSprite SpriteShoulderRight;
	private OrientedSprite SpriteUpperArmLeft;
	private OrientedSprite SpriteUpperArmRight;
	private OrientedSprite SpriteLowerArmLeft;
	private OrientedSprite SpriteLowerArmRight;


	#endregion




	#region --- API ---


	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteBody = new OrientedSprite(name, "BodySuit");
		SpriteCape = new OrientedSprite(name, "CapeSuit");
		SpriteShoulderLeft = new OrientedSprite(name, "ShoulderSuitLeft", "ShoulderSuit");
		SpriteShoulderRight = new OrientedSprite(name, "ShoulderSuitRight", "ShoulderSuit");
		SpriteUpperArmLeft = new OrientedSprite(name, "UpperArmSuitLeft", "UpperArmSuit");
		SpriteUpperArmRight = new OrientedSprite(name, "UpperArmSuitRight", "UpperArmSuit");
		SpriteLowerArmLeft = new OrientedSprite(name, "LowerArmSuitLeft", "LowerArmSuit");
		SpriteLowerArmRight = new OrientedSprite(name, "LowerArmSuitRight", "LowerArmSuit");
		return SpriteLoaded;
	}

	public override void DrawCloth (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		var body = renderer.Body;
		using var _ = new SheetIndexScope(SheetIndex);
		using (new RotateCellScope(body.Rotation, body.GlobalX, body.GlobalY)) {
			DrawClothForBody(renderer, SpriteBody, LocalZ, TwistShiftTopAmount);
			DrawClothForShoulder(renderer, SpriteShoulderLeft, SpriteShoulderRight);
			DrawClothForUpperArm(renderer, SpriteUpperArmLeft, SpriteUpperArmRight);
			DrawClothForLowerArm(renderer, SpriteLowerArmLeft, SpriteLowerArmRight);
		}
		DrawCape(renderer, SpriteCape);
	}

	public override void DrawClothGizmos (IRect rect, Color32 tint, int z) {

		int limbSize = rect.width / 8;
		int limbShift = limbSize / 4;

		// Body
		var bodyRect = rect.CornerInside(Alignment.TopMid, rect.width - limbSize * 2, rect.height);
		if (SpriteBody.TryGetSpriteForGizmos(out var bodySP)) {
			Renderer.Draw(bodySP, bodyRect, tint, z);
			bodyRect.y -= bodyRect.height * Const.ART_SCALE / bodySP.GlobalHeight;
		}

		// Shoulder
		int shoulderHeight = rect.height / 3;
		var shoulderRectL = bodyRect.CornerOutside(Alignment.TopLeft, limbSize, shoulderHeight).Shift(0, -shoulderHeight);
		var shoulderRectR = bodyRect.CornerOutside(Alignment.TopRight, limbSize, shoulderHeight).Shift(0, -shoulderHeight);
		if (SpriteShoulderLeft.TryGetSpriteForGizmos(out var shoulderSpL)) {
			Renderer.Draw(shoulderSpL, shoulderRectL, tint, z);
		}
		if (SpriteShoulderRight.TryGetSpriteForGizmos(out var shoulderSpR)) {
			Renderer.Draw(shoulderSpR, shoulderRectR.GetFlipHorizontal(), tint, z);
		}
		shoulderRectL.yMin += shoulderRectL.height / 2;
		shoulderRectR.yMin += shoulderRectR.height / 2;

		// Upper Arm
		int armHeight = rect.height / 3;
		var uArmRectL = shoulderRectL.EdgeOutsideDown(armHeight);
		var uArmRectR = shoulderRectR.EdgeOutsideDown(armHeight);
		uArmRectL.x -= limbShift;
		uArmRectR.x += limbShift;
		if (SpriteUpperArmLeft.TryGetSpriteForGizmos(out var uArmSpL)) {
			Renderer.Draw(uArmSpL, uArmRectL, tint, z);
		}
		if (SpriteUpperArmRight.TryGetSpriteForGizmos(out var uArmSpR)) {
			Renderer.Draw(uArmSpR, uArmRectR.GetFlipHorizontal(), tint, z);
		}

		// Lower Arm
		var lArmRectL = uArmRectL.EdgeOutsideDown(armHeight);
		var lArmRectR = uArmRectR.EdgeOutsideDown(armHeight);
		lArmRectL.x -= limbShift * 2;
		lArmRectR.x += limbShift * 2;
		if (SpriteLowerArmLeft.TryGetSpriteForGizmos(out var lArmSpL)) {
			Renderer.Draw(lArmSpL, lArmRectL, tint, z);
		}
		if (SpriteLowerArmRight.TryGetSpriteForGizmos(out var lArmSpR)) {
			Renderer.Draw(lArmSpR, lArmRectR.GetFlipHorizontal(), tint, z);
		}

	}

	// Static Draw
	/// <summary>
	/// Draw all body cloths for given character from system pool
	/// </summary>
	public static void DrawClothFromPool (PoseCharacterRenderer renderer) {
		if (renderer.SuitBody != 0 && TryGetCloth(renderer.SuitBody, out var cloth)) {
			cloth.DrawCloth(renderer);
		}
	}

	/// <summary>
	/// Draw artwork sprite as cloth for only "Body" of the given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="clothSprite">Artwork sprite</param>
	/// <param name="localZ">Z value for sorting rendering cells</param>
	/// <param name="twistShiftTopAmount">How much does it shift left and right with character's body movement</param>
	public static void DrawClothForBody (PoseCharacterRenderer renderer, OrientedSprite clothSprite, int localZ, int twistShiftTopAmount) {

		if (!clothSprite.IsValid) return;
		if (renderer.Body.IsFullCovered) return;

		var body = renderer.Body;
		bool facingRight = body.Width > 0;
		bool facingFront = body.FrontSide;
		if (!clothSprite.TryGetSprite(
			facingFront, facingRight, renderer.CurrentAnimationFrame, out var suitSprite
		)) return;

		bool separatedSprite = clothSprite.SpriteID_FL != clothSprite.SpriteID_FR;
		var hip = renderer.Hip;
		int poseTwist = renderer.BodyTwist;
		var rect = new IRect(
			body.GlobalX - body.Width / 2,
			hip.GlobalY,
			body.Width,
			body.Height + hip.Height
		);

		// Border
		if (!suitSprite.GlobalBorder.IsZero) {
			if (rect.width > 0) {
				rect = rect.Expand(suitSprite.GlobalBorder);
			} else {
				rect = rect.Expand(
					-suitSprite.GlobalBorder.left,
					-suitSprite.GlobalBorder.right,
					suitSprite.GlobalBorder.down,
					suitSprite.GlobalBorder.up
				);
			}
		}

		// Flip
		bool flipX = separatedSprite && !facingRight;
		if (flipX) rect.FlipHorizontal();

		// Draw
		var cell = Renderer.Draw(suitSprite, rect, body.Z + localZ);

		// Twist
		if ((poseTwist != 0 || body.Rotation != 0) && body.FrontSide && body.Height > 0) {
			if (flipX) poseTwist = -poseTwist;
			int shiftTop = body.Height * twistShiftTopAmount / 1000;
			int shiftX = poseTwist * cell.Width / 2500;

			var cellL = Renderer.DrawPixel(default);
			cellL.CopyFrom(cell);
			cellL.Shift.up = shiftTop;
			cellL.Width += body.Width.Sign() * shiftX;
			cellL.Shift.right = cellL.Width.Abs() / 2;

			var cellR = Renderer.DrawPixel(default);
			cellR.CopyFrom(cell);
			cellR.Shift.up = shiftTop;
			cellR.Width -= body.Width.Sign() * shiftX;
			cellR.X = cellL.X + cellL.Width / 2 - cellR.Width / 2;
			cellR.Shift.left = cellR.Width.Abs() / 2;

			cell.Shift.down = cell.Height - shiftTop;

			if (body.Rotation != 0) {
				cellR.Shift.down = Util.Remap(0, 90, 0, cellR.Height * 2 / 3, body.Rotation.Abs());
			}

		}

		// Hide Limb
		body.Covered = suitSprite.Tag.HasAll(Tag.HideLimb) ? CoverMode.FullCovered : CoverMode.Covered;

	}

	/// <summary>
	/// Draw artwork sprite as cloth for shoulders of the given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="spriteLeft">Artwork sprite for left shoulder</param>
	/// <param name="spriteRight">Artwork sprite for right shoulder</param>
	/// <param name="localZ">Z value for sorting rendering cells</param>
	public static void DrawClothForShoulder (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		bool facingRight = renderer.Body.Width > 0;
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(
				renderer.ShoulderL.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.ShoulderL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(
				renderer.ShoulderR.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.ShoulderR, sprite, localZ);
		}
	}

	/// <summary>
	/// Draw artwork sprite as cloth for upper-arms of the given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="spriteLeft">Artwork sprite for left upper-arm</param>
	/// <param name="spriteRight">Artwork sprite for right upper-arm</param>
	/// <param name="localZ">Z value for sorting rendering cells</param>
	public static void DrawClothForUpperArm (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		bool facingRight = renderer.Body.Width > 0;
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(
				renderer.UpperArmL.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.UpperArmL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(
				renderer.UpperArmR.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.UpperArmR, sprite, localZ);
		}
	}

	/// <summary>
	/// Draw artwork sprite as cloth for lower-arms of the given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="spriteLeft">Artwork sprite for left lower-arm</param>
	/// <param name="spriteRight">Artwork sprite for right lower-arm</param>
	/// <param name="localZ">Z value for sorting rendering cells</param>
	public static void DrawClothForLowerArm (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		bool facingRight = renderer.Body.Width > 0;
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(
				renderer.LowerArmL.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.LowerArmL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(
				renderer.LowerArmR.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.LowerArmR, sprite, localZ);
		}
	}

	/// <summary>
	/// Draw artwork sprite as cape for given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="clothSprite">Artwork sprite</param>
	/// <param name="motionAmount">How much does the cape flow with character's movement (0 means 0%, 1000 means 100%)</param>
	public static void DrawCape (PoseCharacterRenderer renderer, OrientedSprite clothSprite, int motionAmount = 1000) {

		if (!clothSprite.IsValid) return;
		var body = renderer.Body;
		bool facingRight = body.Width > 0;
		bool facingFront = body.FrontSide;
		if (!clothSprite.TryGetSprite(facingFront, facingRight, renderer.CurrentAnimationFrame, out var sprite)) return;

		var target = renderer.TargetCharacter;
		var animatedPoseType = target.AnimationType;

		if (
			animatedPoseType == CharacterAnimationType.Rolling ||
			animatedPoseType == CharacterAnimationType.Spin ||
			animatedPoseType == CharacterAnimationType.Fly ||
			animatedPoseType.IsLyingDown()
		) return;

		// Draw
		int height = body.Height.Sign() * (sprite.GlobalHeight + body.Height.Abs() - body.SizeY);
		if (
			animatedPoseType == CharacterAnimationType.SquatIdle ||
			animatedPoseType == CharacterAnimationType.SquatMove ||
			animatedPoseType == CharacterAnimationType.Dash
		) {
			int lastSqFrame = target.Movement.LastSquatStartFrame;
			if (lastSqFrame >= 0) {
				height -= ((Game.GlobalFrame - lastSqFrame) * 10).Clamp(0, height / 2);
			}
		} else {
			int lastSqFrame = target.Movement.LastSquattingFrame;
			if (lastSqFrame >= 0) {
				height -= ((lastSqFrame - Game.GlobalFrame) * 10 + height / 2).Clamp(0, height / 2);
			}
		}
		var cells = Renderer.DrawSlice(
			sprite,
			target.X + renderer.PoseRootX,
			target.Y + renderer.PoseRootY + body.Height,
			500, 1000, body.Rotation / 3,
			sprite.GlobalWidth,
			height,
			z: body.FrontSide ? -31 : 31
		);

		// Flow Motion
		if (motionAmount != 0) {
			// X
			int maxX = 30 * motionAmount / 1000;
			int offsetX = (-target.DeltaPositionX * motionAmount / 1000).Clamp(-maxX, maxX);
			cells[3].X += offsetX / 2;
			cells[4].X += offsetX / 2;
			cells[5].X += offsetX / 2;
			cells[6].X += offsetX;
			cells[7].X += offsetX;
			cells[8].X += offsetX;
			// Y
			int maxY = 20 * motionAmount / 1000;
			int offsetAmountY = 1000 + (target.DeltaPositionY * motionAmount / 10000).Clamp(-maxY, maxY) * 1000 / 20;
			offsetAmountY = offsetAmountY.Clamp(800, 1200);
			cells[0].Height = cells[0].Height * offsetAmountY / 1000;
			cells[1].Height = cells[1].Height * offsetAmountY / 1000;
			cells[2].Height = cells[2].Height * offsetAmountY / 1000;
			cells[3].Height = cells[3].Height * offsetAmountY / 1000;
			cells[4].Height = cells[4].Height * offsetAmountY / 1000;
			cells[5].Height = cells[5].Height * offsetAmountY / 1000;
			cells[6].Height = cells[6].Height * offsetAmountY / 1000;
			cells[7].Height = cells[7].Height * offsetAmountY / 1000;
			cells[8].Height = cells[8].Height * offsetAmountY / 1000;
		}

	}


	#endregion




}