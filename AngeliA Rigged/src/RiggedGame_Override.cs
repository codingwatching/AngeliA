﻿using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliaRaylib;

namespace AngeliaRigged;

public partial class RiggedGame {


	// VAR
	private readonly Dictionary<Int2, CharSprite> CharPool = [];
	private readonly int[] KeyboardHoldingFrames;
	private readonly int[] GamepadHoldingFrames;
	private readonly int RiggedFontCount;
	private int CurrentPressedCharIndex;
	private int CurrentPressedKeyIndex;
	private int CurrentBgmID = 0;


	// System
	protected override void _SetFullscreen (bool fullScreen) { }

	protected override int _GetScreenWidth () => CallingMessage.ScreenWidth;

	protected override int _GetScreenHeight () => CallingMessage.ScreenHeight;

	protected override void _QuitApplication () { }

	protected override void _OpenUrl (string url) { }


	// Window
	protected override bool _IsWindowFocused () => true;

	protected override void _MakeWindowFocused () { }

	protected override void _SetWindowPosition (int x, int y) { }

	protected override Int2 _GetWindowPosition () => Int2.Zero;

	protected override void _SetWindowSize (int x, int y) { }

	protected override int _GetCurrentMonitor () => 0;

	protected override int _GetMonitorWidth (int monitor) => CallingMessage.MonitorWidth;

	protected override int _GetMonitorHeight (int monitor) => CallingMessage.MonitorHeight;

	protected override bool _GetWindowDecorated () => true;
	protected override void _SetWindowDecorated (bool decorated) { }

	protected override bool _GetWindowResizable () => true;
	protected override void _SetWindowResizable (bool resizable) { }

	protected override bool _GetWindowTopmost () => false;
	protected override void _SetWindowTopmost (bool topmost) { }

	protected override bool _GetWindowMaximized () => false;

	protected override void _SetWindowMaximized (bool maximized) { }

	protected override bool _GetWindowMinimized () => false;

	protected override void _SetWindowMinimized (bool minimized) { }

	protected override void _SetWindowTitle (string title) { }

	protected override void _SetWindowIcon (int spriteID) { }

	protected override void _SetWindowMinSize (int size) { }

	protected override void _SetEventWaiting (bool enable) { }


	// Render
	protected override void _BeforeAllLayersUpdate () { }

	protected override void _AfterAllLayersUpdate () { }

	protected override void _OnLayerUpdate (int layerIndex, Cell[] cells, int cellCount) { }

	protected override bool _GetEffectEnable (int effectIndex) => CallingMessage.EffectEnable.GetBit(effectIndex);

	protected override void _SetEffectEnable (int effectIndex, bool enable) => RespondMessage.EffectEnable.SetBit(effectIndex, enable);

	protected override void _Effect_SetDarkenParams (float amount, float step) {
		RespondMessage.HasEffectParams.SetBit(Const.SCREEN_EFFECT_RETRO_DARKEN, true);
		RespondMessage.e_DarkenAmount = amount;
		RespondMessage.e_DarkenStep = step;
	}

	protected override void _Effect_SetLightenParams (float amount, float step) {
		RespondMessage.HasEffectParams.SetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN, true);
		RespondMessage.e_LightenAmount = amount;
		RespondMessage.e_LightenStep = step;
	}

	protected override void _Effect_SetTintParams (Color32 color) {
		RespondMessage.HasEffectParams.SetBit(Const.SCREEN_EFFECT_TINT, true);
		RespondMessage.e_TintColor = color;
	}

	protected override void _Effect_SetVignetteParams (float radius, float feather, float offsetX, float offsetY, float round) {
		RespondMessage.HasEffectParams.SetBit(Const.SCREEN_EFFECT_VIGNETTE, true);
		RespondMessage.e_VigRadius = radius;
		RespondMessage.e_VigFeather = feather;
		RespondMessage.e_VigOffsetX = offsetX;
		RespondMessage.e_VigOffsetY = offsetY;
		RespondMessage.e_VigRound = round;
	}


	// Texture
	protected override object _GetTextureFromPixels (Color32[] pixels, int width, int height) => RayUtil.GetTextureFromPixels(pixels, width, height);

	protected override Color32[] _GetPixelsFromTexture (object texture) => RayUtil.GetPixelsFromTexture(texture);

	protected override void _FillPixelsIntoTexture (Color32[] pixels, object texture) => RayUtil.FillPixelsIntoTexture(pixels, texture);

	protected override Int2 _GetTextureSize (object texture) => RayUtil.GetTextureSize(texture);

	protected override object _PngBytesToTexture (byte[] bytes) => RayUtil.PngBytesToTexture(bytes);

	protected override byte[] _TextureToPngBytes (object texture) => RayUtil.TextureToPngBytes(texture);

	protected override void _UnloadTexture (object texture) => RayUtil.UnloadTexture(texture);

	protected override uint? _GetTextureID (object texture) => RayUtil.GetTextureID(texture);

	protected override bool _IsTextureReady (object texture) => RayUtil.IsTextureReady(texture);

	protected override object _GetResizedTexture (object texture, int newWidth, int newHeight, bool nearestNeighbor = true) => RayUtil.GetResizedTexture(texture, newWidth, newHeight, nearestNeighbor);

	protected override void _FillResizedTexture (object sourceTexture, object targetTexture, bool nearestNeighbor = true) { }

	protected override object _GetScreenRenderingTexture () => RayUtil.EMPTY_TEXTURE_OBJ;


	// Gizmos
	protected override void _DrawGizmosRect (IRect rect, Color32 color) => _DrawGizmosRect(rect, color, color, color, color);

	protected override void _DrawGizmosRect (IRect rect, Color32 colorT, Color32 colorB) => _DrawGizmosRect(rect, colorT, colorT, colorB, colorB);

	protected override void _DrawGizmosRect (IRect rect, Color32 colorTL, Color32 colorTR, Color32 colorBL, Color32 colorBR) {
		if (RespondMessage.RequireGizmosRectCount >= RespondMessage.RequireGizmosRects.Length) return;
		RespondMessage.RequireGizmosRects[RespondMessage.RequireGizmosRectCount] = new RigRespondMessage.GizmosRectData() {
			Rect = rect,
			ColorTL = colorTL,
			ColorTR = colorTR,
			ColorBL = colorBL,
			ColorBR = colorBR,
		};
		RespondMessage.RequireGizmosRectCount++;
	}

	protected override void _DrawGizmosLine (int startX, int startY, int endX, int endY, int thickness, Color32 color) {
		if (RespondMessage.RequireGizmosLineCount >= RespondMessage.RequireGizmosLines.Length) return;
		RespondMessage.RequireGizmosLines[RespondMessage.RequireGizmosLineCount] = new RigRespondMessage.GizmosLineData() {
			Start = new Int2(startX, startY),
			End = new Int2(endX, endY),
			Thickness = thickness,
			Color = color,
		};
		RespondMessage.RequireGizmosLineCount++;
	}

	protected override void _DrawGizmosTexture (IRect rect, FRect uv, object texture, Color32 tint, bool inverse, bool flipX, bool flipY) { }

	protected override void _IgnoreGizmos (int duration = 0) { }


	// Doodle
	protected override void _ResetDoodle (Color32 color) => RespondMessage.RequireResetDoodle = true;

	protected override void _SetDoodleOffset (Float2 screenOffset) => RespondMessage.RequireDoodleRenderingOffset = screenOffset;

	protected override void _SetDoodleZoom (float zoom) => RespondMessage.RequireDoodleRenderingZoom = zoom;

	protected override void _DoodleRect (FRect screenRect, Color32 color) {
		if (RespondMessage.RequireDoodleRectCount >= RespondMessage.RequireDoodleRects.Length) return;
		RespondMessage.RequireDoodleRects[RespondMessage.RequireDoodleRectCount] = new RigRespondMessage.DoodleRectData() {
			Rect = screenRect,
			Color = color,
		};
		RespondMessage.RequireDoodleRectCount++;
	}

	protected override void _DoodleWorld (IBlockSquad squad, FRect screenRect, IRect worldUnitRange, int z, bool ignoreLevel = false, bool ignoreBG = false, bool ignoreEntity = false, bool ignoreElement = true) {
		if (RespondMessage.RequireDoodleWorldCount >= RespondMessage.RequireDoodleWorlds.Length) return;
		byte mask = 0;
		mask.SetBit(0, ignoreLevel);
		mask.SetBit(1, ignoreBG);
		mask.SetBit(2, ignoreEntity);
		mask.SetBit(3, ignoreElement);
		RespondMessage.RequireDoodleWorlds[RespondMessage.RequireDoodleWorldCount] = new RigRespondMessage.DoodleWorldData() {
			ScreenRect = screenRect,
			WorldUnitRange = worldUnitRange,
			Z = z,
			IgnoreMask = mask,
		};
		RespondMessage.RequireDoodleWorldCount++;
	}


	// Text
	protected override int _GetFontCount () => RiggedFontCount;

	protected override string _GetClipboardText () => RayUtil.GetClipboardText();

	protected override void _SetClipboardText (string text) => RayUtil.SetClipboardText(text);

	protected override bool _GetCharSprite (int fontIndex, char c, out CharSprite result) {
		result = null;
		int reqCount = RespondMessage.CharRequiringCount;
		if (reqCount >= RigRespondMessage.REQUIRE_CHAR_MAX_COUNT) return false;
		// Get From Pool
		if (CharPool.TryGetValue(new(c, fontIndex), out result)) return true;
		// Require
		RespondMessage.RequireChars[reqCount] = c;
		RespondMessage.RequireCharsFontIndex[reqCount] = fontIndex;
		RespondMessage.CharRequiringCount++;
		return false;
	}

	protected override FontData CreateNewFontData () => null;


	// Music
	protected override void _UnloadMusic (object music) { }

	protected override void _PlayMusic (int id, bool fromStart) {
		RespondMessage.RequirePlayMusicID = id;
		RespondMessage.RequirePlayMusicFromStart = fromStart;
		CurrentBgmID = id;
	}

	protected override void _StopMusic () => RespondMessage.AudioActionRequirement.SetBit(0, true);

	protected override void _PauseMusic () => RespondMessage.AudioActionRequirement.SetBit(1, true);

	protected override void _UnPauseMusic () => RespondMessage.AudioActionRequirement.SetBit(2, true);

	protected override void _SetMusicVolume (int volume) => RespondMessage.RequireSetMusicVolume = volume;

	protected override bool _IsMusicPlaying () => CallingMessage.IsMusicPlaying;

	protected override int _GetCurrentMusicID () => CurrentBgmID;


	// Sounds
	protected override object _LoadSound (string filePath) => null;
	protected override object _LoadSoundAlias (object source) => null;
	protected override void _UnloadSound (SoundData sound) { }
	protected override void _PlaySound (int id, float volume, float pitch, float pan) {
		if (RespondMessage.RequirePlaySoundCount >= RespondMessage.PlaySoundRequirements.Length) return;
		var req = RespondMessage.PlaySoundRequirements[RespondMessage.RequirePlaySoundCount];
		req.ID = id;
		req.Volume = volume;
		req.Pitch = pitch;
		req.Pan = pan;
		RespondMessage.RequirePlaySoundCount++;
	}
	protected override void _StopAllSounds () => RespondMessage.AudioActionRequirement.SetBit(3, true);
	protected override void _SetSoundVolume (int volume) => RespondMessage.RequireSetSoundVolume = volume;


	// Cursor
	protected override void _ShowCursor () { }
	protected override void _HideCursor () { }
	protected override void _CenterCursor () { }
	protected override bool _CursorVisible () => true;
	protected override void _SetCursor (int index) => RespondMessage.RequireSetCursorIndex = index;
	protected override void _SetCursorToNormal () => RespondMessage.RequireSetCursorIndex = -3;
	protected override bool _CursorInScreen () => CallingMessage.CursorInScreen;


	// Mouse
	protected override bool _IsMouseAvailable () => CallingMessage.DeviceData.GetBit(0);
	protected override bool _IsMouseLeftHolding () => CallingMessage.DeviceData.GetBit(1);
	protected override bool _IsMouseRightHolding () => CallingMessage.DeviceData.GetBit(2);
	protected override bool _IsMouseMidHolding () => CallingMessage.DeviceData.GetBit(3);
	protected override int _GetMouseScrollDelta () => CallingMessage.MouseScrollDelta;
	protected override Int2 _GetMouseScreenPosition () => new(CallingMessage.MousePosX, CallingMessage.MousePosY);


	// Keyboard
	protected override bool _IsKeyboardAvailable () => CallingMessage.DeviceData.GetBit(5);
	protected override bool _IsKeyboardKeyHolding (KeyboardKey key) => KeyboardHoldingFrames[(int)key] == PauselessFrame;
	protected override char GetCharPressed () {
		if (CurrentPressedCharIndex < CallingMessage.PressedCharCount) {
			char result = CallingMessage.PressedChars[CurrentPressedCharIndex];
			CurrentPressedCharIndex++;
			return result;
		}
		return '\0';
	}
	protected override KeyboardKey? GetKeyPressed () {
		if (CurrentPressedKeyIndex < CallingMessage.PressedKeyCount) {
			var result = CallingMessage.PressedGuiKeys[CurrentPressedKeyIndex];
			CurrentPressedKeyIndex++;
			return (KeyboardKey)result;
		}
		return null;
	}


	// Gamepad
	protected override bool _IsGamepadAvailable () => CallingMessage.DeviceData.GetBit(6);
	protected override bool _IsGamepadKeyHolding (GamepadKey key) => GamepadHoldingFrames[(int)key] == PauselessFrame;
	protected override bool _IsGamepadLeftStickHolding (Direction4 direction) => CallingMessage.GamepadStickHolding.GetBit(direction switch {
		Direction4.Left => 0,
		Direction4.Right => 1,
		Direction4.Down => 2,
		Direction4.Up => 3,
		_ => 0,
	});
	protected override bool _IsGamepadRightStickHolding (Direction4 direction) => CallingMessage.GamepadStickHolding.GetBit(direction switch {
		Direction4.Left => 4,
		Direction4.Right => 5,
		Direction4.Down => 6,
		Direction4.Up => 7,
		_ => 4,
	});
	protected override Float2 _GetGamepadLeftStickDirection () => new(CallingMessage.GamepadLeftStickDirectionX, CallingMessage.GamepadLeftStickDirectionY);
	protected override Float2 _GetGamepadRightStickDirection () => new(CallingMessage.GamepadRightStickDirectionX, CallingMessage.GamepadRightStickDirectionY);



}