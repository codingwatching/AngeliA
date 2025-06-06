using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// General class for menu entity ui
/// </summary>
public abstract class MenuUI : EntityUI, IWindowEntityUI {




	#region --- VAR ---

	// Api
	/// <summary>
	/// Index of the current selecting item
	/// </summary>
	public int SelectionIndex { get; private set; } = 0;
	/// <summary>
	/// Text content of the message display on top. Set to empty means no message should be display.
	/// </summary>
	public string Message { get; set; } = "";
	/// <summary>
	/// Rect position of the background range in global space
	/// </summary>
	public IRect BackgroundRect { get; private set; }
	/// <summary>
	/// Forced horizontal size in global space 
	/// </summary>
	public int OverrideWindowWidth { get; set; } = -1;
	/// <summary>
	/// Length in frame for the appearing animation
	/// </summary>
	public int AnimationDuration { get; set; } = 8;
	/// <summary>
	/// GUI style instance of the background panel
	/// </summary>
	protected GUIStyle BackgroundStyle { get; set; }
	/// <summary>
	/// GUI style instance of the message box
	/// </summary>
	protected GUIStyle MessageStyle { get; set; }
	/// <summary>
	/// GUI style instance of the item label
	/// </summary>
	protected GUIStyle DefaultLabelStyle { get; init; } = null;
	/// <summary>
	/// GUI style instance of the item content
	/// </summary>
	protected GUIStyle DefaultContentStyle { get; init; } = null;
	protected override bool BlockEvent => true;

	// Config
	/// <summary>
	/// Artwork sprite for the background
	/// </summary>
	protected SpriteCode BackgroundCode = "Pixel";
	/// <summary>
	/// Artwork sprite for the selecting item mark
	/// </summary>
	protected SpriteCode SelectionMarkCode = BuiltInSprite.MENU_SELECTION_MARK;
	/// <summary>
	/// Artwork sprite for the mark displays on bottom when there's hiden item under the menu
	/// </summary>
	protected SpriteCode MoreItemMarkCode = BuiltInSprite.MENU_MORE_MARK;
	/// <summary>
	/// Artwork sprite for the adjusting arrows
	/// </summary>
	protected SpriteCode ArrowMarkCode = BuiltInSprite.MENU_ARROW_MARK;
	/// <summary>
	/// Unified width of the window
	/// </summary>
	protected int WindowWidth = 660;
	/// <summary>
	/// Unified height of a single item
	/// </summary>
	protected int ItemHeight = 32;
	/// <summary>
	/// Unified space between two items
	/// </summary>
	protected int ItemGap = 16;
	/// <summary>
	/// How many items can it display at the same time
	/// </summary>
	protected int MaxItemCount = 10;
	/// <summary>
	/// Unified padding gap for the content panel
	/// </summary>
	protected Int4 ContentPadding = new(32, 32, 18, 46);
	/// <summary>
	/// Unified size of the selection hand mark
	/// </summary>
	protected Int2 SelectionMarkSize = new(32, 32);
	/// <summary>
	/// Unified size of the item adjusting arrow
	/// </summary>
	protected Int2 SelectionArrowMarkSize = new(24, 24);
	/// <summary>
	/// Unified size of the hidden item indicator
	/// </summary>
	protected Int2 MoreMarkSize = new(28, 28);
	/// <summary>
	/// Color tint that blocks all screen behind
	/// </summary>
	protected Color32 ScreenTint = new(0, 0, 0, 0);
	/// <summary>
	/// Color tint of the background panel
	/// </summary>
	protected Color32 BackgroundTint = new(0, 0, 0, 255);
	/// <summary>
	/// Color tint of the selecting hand mark
	/// </summary>
	protected Color32 SelectionMarkTint = new(255, 255, 255, 255);
	/// <summary>
	/// Color tint of the hidden item indicator
	/// </summary>
	protected Color32 MoreMarkTint = new(220, 220, 220, 255);
	/// <summary>
	/// Color tint for the current hovering item
	/// </summary>
	protected Color32 MouseHighlightTint = new(255, 255, 255, 16);
	/// <summary>
	/// True if the menu react to player input currently
	/// </summary>
	protected bool Interactable = true;
	/// <summary>
	/// True if the menu react to mouse input
	/// </summary>
	protected bool AllowMouseClick = true;
	/// <summary>
	/// True if the menu close when player press "Start" button
	/// </summary>
	protected bool QuitOnPressStartOrEscKey = true;
	/// <summary>
	/// How many amount of appearing animation should apply on this menu
	/// </summary>
	protected int AnimationAmount = -32;

	// Data
	private bool SelectionAdjustable = false;
	private int ItemCount;
	private int ScrollY = 0;
	private int MarkPingPongFrame = 0;
	private int RequireSetSelection = -1;
	private int ActiveFrame = int.MinValue;
	private int TargetItemCount;
	private int AnimationFrame = 0;
	private bool Layout;
	private int MessageHeight = 0;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		SelectionIndex = 0;
		ScrollY = 0;
		ActiveFrame = Game.GlobalFrame;
		AnimationFrame = 0;
		Input.UseAllHoldingKeys();
		MessageStyle = GUI.Skin.CenterMessageGrey;
		BackgroundStyle = null;
		OverrideWindowWidth = -1;
		AnimationDuration = 8;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		if (Input.AnyMouseButtonHolding && !BackgroundRect.MouseInside()) {
			Input.UseMouseKey(0);
			Input.UseMouseKey(1);
			Input.UseMouseKey(2);
		}
	}


	public override void UpdateUI () {

		Input.IgnoreMouseToActionJump();
		Cursor.RequireCursor();
		int msgPadding = Unify(24);
		string msg = Message;
		bool hasMsg = !string.IsNullOrWhiteSpace(msg);

		// Layout
		TargetItemCount = 0;
		Layout = true;
		DrawMenu();
		Layout = false;
		TargetItemCount = TargetItemCount.Clamp(0, MaxItemCount);
		ItemCount = 0;

		// Window Rect
		var windowRect = GetWindowRect();
		X = windowRect.x;
		Width = windowRect.width;
		Height = windowRect.height;
		Y = windowRect.y;

		// Paint
		var windowBounds = windowRect.Expand(
			Unify(ContentPadding.left),
			Unify(ContentPadding.right),
			Unify(ContentPadding.down),
			hasMsg ? msgPadding : Unify(ContentPadding.up)
		);
		var moreMarkSize = new Int2(
			Unify(MoreMarkSize.x),
			Unify(MoreMarkSize.y)
		);
		var contentPadding = new Int4(
			Unify(ContentPadding.x),
			Unify(ContentPadding.y),
			Unify(ContentPadding.z),
			Unify(ContentPadding.w)
		);

		// Ani
		bool animating = AnimationDuration > 0 && AnimationFrame < AnimationDuration;
		byte alpha = animating ? (byte)Util.RemapUnclamped(0, AnimationDuration, 0, 255, AnimationFrame) : (byte)255;
		using var _ = new GUIColorScope(new Color32(255, 255, 255, alpha));

		// BG
		var bgRect = windowBounds.Expand(0, 0, 0, hasMsg ? MessageHeight + contentPadding.up : 0);
		if (animating) {
			bgRect = bgRect.Expand(Util.RemapUnclamped(
				0, AnimationDuration * AnimationDuration,
				Unify(AnimationAmount), 0,
				(AnimationDuration * AnimationDuration) - (AnimationDuration - AnimationFrame) * (AnimationDuration - AnimationFrame)
			));
		}
		BackgroundRect = bgRect;
		if (ScreenTint.a > 0) {
			Renderer.DrawPixel(Renderer.CameraRect, ScreenTint, int.MaxValue - 6);
		}
		if (BackgroundStyle == null) {
			using var __ = new GUIColorScope(BackgroundTint);
			GUI.DrawSlice(BackgroundCode, bgRect);
		} else {
			GUI.DrawStyleBody(bgRect, BackgroundStyle, GUIState.Normal);
		}

		// Message
		if (hasMsg) {
			GUI.Label(new IRect(
				windowBounds.x + contentPadding.left,
				windowBounds.yMax,
				windowBounds.width - contentPadding.horizontal,
				MessageHeight
			), msg, out var msgBounds, MessageStyle);
			MessageHeight = msgBounds.height + msgPadding;
		}

		// Scroll Y
		if (SelectionIndex - ScrollY >= TargetItemCount) {
			ScrollY = SelectionIndex - TargetItemCount + 1;
		}
		if (SelectionIndex - ScrollY <= 0) {
			ScrollY = SelectionIndex;
		}

		ItemCount = 0;

		// Hint
		if (SelectionAdjustable) {
			ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_ADJUST);
		} else {
			ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_USE);
		}
		ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, BuiltInText.HINT_MOVE);


		// --- Draw all Menu Items ---

		DrawMenu();

		// ---------------------------


		// Scroll Wheel
		int wheel = -Input.MouseWheelDelta;
		if (wheel != 0) {
			if (wheel > 0) {
				SelectionIndex = TargetItemCount + ScrollY + wheel - 1;
			} else {
				SelectionIndex = ScrollY + wheel;
			}
		}
		if (ItemCount > 0) SelectionIndex = SelectionIndex.Clamp(0, ItemCount - 1);

		// Set Selection
		if (RequireSetSelection >= 0) {
			SelectionIndex = RequireSetSelection;
			RequireSetSelection = -1;
			OnSelectionChanged();
		}

		// More Mark
		if (ItemCount > TargetItemCount) {
			ScrollY = ScrollY.Clamp(0, ItemCount - TargetItemCount);
		} else {
			ScrollY = 0;
		}
		IRect markRectD = default;
		IRect markRectU = default;
		if (ScrollY > 0) {
			// U
			markRectU = new IRect(
				windowRect.x + (windowRect.width - moreMarkSize.x) / 2,
				windowRect.yMax + contentPadding.up - moreMarkSize.y,
				moreMarkSize.x, moreMarkSize.y
			).Shift(0, MarkPingPongFrame.PingPong(46));
			Renderer.Draw(MoreItemMarkCode, markRectU, MoreMarkTint, int.MaxValue - 4);
			Cursor.SetCursorAsHand(markRectU);
		}
		if (ScrollY < ItemCount - TargetItemCount) {
			// D
			markRectD = new IRect(
				windowRect.x + (windowRect.width - moreMarkSize.x) / 2,
				windowRect.yMin - contentPadding.down + moreMarkSize.y,
				moreMarkSize.x, -moreMarkSize.y
			).Shift(0, MarkPingPongFrame.PingPong(46));
			Renderer.Draw(MoreItemMarkCode, markRectD, MoreMarkTint, int.MaxValue - 4);
			Cursor.SetCursorAsHand(markRectD);
		}

		// Click on Mark
		if (Input.MouseLeftButtonDown) {
			markRectD.FlipNegative();
			markRectU.FlipNegative();
			if (markRectD.Expand(Unify(12)).MouseInside()) {
				ScrollY++;
			}
			if (markRectU.Expand(Unify(12)).MouseInside()) {
				ScrollY--;
			}
		}

		// Use Action
		if (Interactable) {
			if (Input.GameKeyDownGUI(Gamekey.Up) || Input.KeyboardDownGUI(KeyboardKey.UpArrow)) {
				SelectionIndex = (SelectionIndex - 1).Clamp(0, ItemCount - 1);
				OnSelectionChanged();
			}
			if (Input.GameKeyDownGUI(Gamekey.Down) || Input.KeyboardDownGUI(KeyboardKey.DownArrow)) {
				SelectionIndex = (SelectionIndex + 1).Clamp(0, ItemCount - 1);
				OnSelectionChanged();
			}
			if (QuitOnPressStartOrEscKey && Game.GlobalFrame != ActiveFrame && (Input.GameKeyUp(Gamekey.Start) || Input.KeyboardUp(KeyboardKey.Escape))) {
				Active = false;
				Input.UseAllHoldingKeys();
			}
		}

		MarkPingPongFrame++;
		AnimationFrame++;
	}


	protected abstract void DrawMenu ();


	/// <summary>
	/// This function is called when item selection is changed
	/// </summary>
	protected virtual void OnSelectionChanged () { }


	#endregion




	#region --- API ---


	/// <summary>
	/// Get the rect position of the panel window in global space
	/// </summary>
	protected virtual IRect GetWindowRect () {
		int w = OverrideWindowWidth >= 0 ? OverrideWindowWidth : Unify(WindowWidth);
		int h = Unify(TargetItemCount * ItemHeight + (TargetItemCount - 1) * ItemGap);
		int x = (int)(Renderer.CameraRect.x + Renderer.CameraRect.width / 2 - w / 2);
		int y = Renderer.CameraRect.y + Renderer.CameraRect.height / 2 - h / 2;
		return new IRect(x, y, w, h);
	}


	// Draw Item
	/// <inheritdoc cref="DrawItemLogic"/>
	protected bool DrawItem (string label, int icon = 0, GUIStyle labelStyle = null, GUIStyle contentStyle = null, bool drawStyleBody = false) => DrawItemLogic(label, "", null, icon, false, false, out _, labelStyle, contentStyle, drawStyleBody);
	/// <inheritdoc cref="DrawItemLogic"/>
	protected bool DrawItem (string label, string content, int icon = 0, GUIStyle labelStyle = null, GUIStyle contentStyle = null, bool drawStyleBody = false) => DrawItemLogic(label, content, null, icon, false, false, out _, labelStyle, contentStyle, drawStyleBody);
	/// <inheritdoc cref="DrawItemLogic"/>
	protected bool DrawArrowItem (string label, string content, bool leftArrow, bool rightArrow, out int delta, int icon = 0, GUIStyle labelStyle = null, GUIStyle contentStyle = null, bool drawStyleBody = false) => DrawItemLogic(label, content, null, icon, leftArrow, rightArrow, out delta, labelStyle, contentStyle, drawStyleBody);
	/// <inheritdoc cref="DrawItemLogic"/>
	protected bool DrawItem (string label, char[] chars, int icon = 0, GUIStyle labelStyle = null, GUIStyle contentStyle = null, bool drawStyleBody = false) => DrawItemLogic(label, "", chars, icon, false, false, out _, labelStyle, contentStyle, drawStyleBody);
	/// <inheritdoc cref="DrawItemLogic"/>
	protected bool DrawArrowItem (string label, char[] chars, bool leftArrow, bool rightArrow, out int delta, int icon = 0, GUIStyle labelStyle = null, GUIStyle contentStyle = null, bool drawStyleBody = false) => DrawItemLogic(label, "", chars, icon, leftArrow, rightArrow, out delta, labelStyle, contentStyle, drawStyleBody);
	/// <summary>
	/// Draw an item inside the menu
	/// </summary>
	/// <param name="label">Text displays on left side of this item</param>
	/// <param name="content">Text displays on right side of this item</param>
	/// <param name="chars">Text displays on right side of this item</param>
	/// <param name="icon">Artwork sprite</param>
	/// <param name="useLeftArrow">True if there should be an arrow at left side</param>
	/// <param name="useRightArrow">True if there should be an arrow at right side</param>
	/// <param name="delta">Adjusted value from the user at current frame</param>
	/// <param name="labelStyle">GUI style of the label part</param>
	/// <param name="contentStyle">GUI style of the content part</param>
	/// <param name="drawStyleBody">True if the body of GUI style should be display</param>
	/// <returns>True if the item is pressed</returns>
	private bool DrawItemLogic (
		string label, string content, char[] chars, int icon,
		bool useLeftArrow, bool useRightArrow, out int delta,
		GUIStyle labelStyle = null, GUIStyle contentStyle = null, bool drawStyleBody = false
	) {

		labelStyle ??= DefaultLabelStyle ?? GUI.Skin.Label;
		contentStyle ??= DefaultContentStyle ?? GUI.Skin.CenterLabel;

		delta = 0;
		if (Layout) {
			TargetItemCount++;
			return false;
		}

		bool invoke = false;
		if (ItemCount < ScrollY || ItemCount >= ScrollY + TargetItemCount) {
			ItemCount++;
			return invoke;
		}

		bool useStringContent = chars == null;
		bool hasContent = useStringContent ? !string.IsNullOrEmpty(content) : chars.Length > 0;
		bool useArrows = useLeftArrow || useRightArrow;
		int itemHeight = Unify(ItemHeight);
		int itemGap = Unify(ItemGap);
		var markSize = new Int2(
			Unify(SelectionMarkSize.x),
			Unify(SelectionMarkSize.y)
		);
		var selectionMarkSize = new Int2(
			Unify(SelectionArrowMarkSize.x),
			Unify(SelectionArrowMarkSize.y)
		);
		var windowRect = Rect;

		int itemY = windowRect.yMax - itemHeight;
		itemY -= ItemCount * (itemHeight + itemGap);
		itemY += ScrollY * (itemHeight + itemGap);
		bool mouseHoverLabel = false;
		bool mouseHoverArrowL = false;
		bool mouseHoverArrowR = false;

		var itemRect_Old = new IRect(windowRect.x, itemY, windowRect.width, itemHeight);
		var itemRect = itemRect_Old.Shrink(markSize.x, markSize.x, 0, 0);
		itemRect_Old = itemRect_Old.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
		var hoverCheckingRect = itemRect;
		if (itemRect.Overlaps(windowRect)) {

			var labelRect = itemRect;
			IRect bounds;

			// Labels
			if (!hasContent && icon == 0) {

				// Mouse Highlight
				hoverCheckingRect = labelRect.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
				bounds = hoverCheckingRect.Shrink(markSize.x * 2, markSize.x * 2, 0, 0);
				if (!useArrows) {
					mouseHoverLabel = AllowMouseClick && Interactable && hoverCheckingRect.MouseInside();
					if (!drawStyleBody && mouseHoverLabel && Input.LastActionFromMouse) {
						Renderer.DrawPixel(hoverCheckingRect, MouseHighlightTint);
					}
				}

				// Single Label
				if (drawStyleBody) {
					GUI.DrawStyleBody(labelRect, contentStyle, mouseHoverLabel ? GUIState.Hover : GUIState.Normal);
				}

				GUI.Label(labelRect, label, contentStyle);

			} else {

				var secLabelRect = labelRect.Shrink(labelRect.width / 2, 0, 0, 0);
				hoverCheckingRect = secLabelRect.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
				bounds = hoverCheckingRect.Shrink(markSize.x * 2, markSize.x * 2, 0, 0);

				// Mouse Highlight
				if (!useArrows) {
					mouseHoverLabel = AllowMouseClick && Interactable && hoverCheckingRect.MouseInside();
					if (!drawStyleBody && mouseHoverLabel && Input.LastActionFromMouse) {
						Renderer.DrawPixel(hoverCheckingRect, MouseHighlightTint, int.MaxValue - 3);
					}
				}

				// Double Labels
				var labelBounds = secLabelRect;
				GUI.Label(labelRect.Shrink(selectionMarkSize.x, labelRect.width / 2, 0, 0), label, labelStyle);

				// Content Label
				if (hasContent) {
					if (drawStyleBody) {
						GUI.DrawStyleBody(secLabelRect, contentStyle, mouseHoverLabel ? GUIState.Hover : GUIState.Normal);
					}
					if (useStringContent) {
						GUI.Label(secLabelRect, content, out labelBounds, contentStyle);
					} else {
						GUI.Label(secLabelRect, chars, out labelBounds, contentStyle);
					}
				}

				// Icon
				if (icon != 0 && Renderer.TryGetSprite(icon, out var iconSprite)) {
					var iconRect = labelBounds.Fit(iconSprite, hasContent ? 0 : 500, 500);
					if (hasContent) iconRect.x -= iconRect.height;
					Renderer.Draw(icon, iconRect);
				}

			}

			// Arrow
			if (useArrows) {

				const int HOVER_EXP = 32;

				var rectL = new IRect(
					bounds.xMin - selectionMarkSize.x,
					labelRect.y + labelRect.height / 2 - selectionMarkSize.y / 2,
					-selectionMarkSize.x,
					selectionMarkSize.y
				);
				var rectL_H = new IRect(
					rectL.x + rectL.width, rectL.y, rectL.width.Abs(), rectL.height
				).Expand(HOVER_EXP);

				var rectR = new IRect(
					bounds.xMax + selectionMarkSize.x,
					labelRect.y + labelRect.height / 2 - selectionMarkSize.y / 2,
					selectionMarkSize.x,
					selectionMarkSize.y
				);
				var rectR_H = rectR.Expand(HOVER_EXP);

				// Mouse Hover and Highlight
				if (AllowMouseClick && AllowMouseClick && Interactable) {
					mouseHoverArrowL = useLeftArrow && rectL_H.MouseInside();
					mouseHoverArrowR = useRightArrow && rectR_H.MouseInside();
				}

				// Draw Hover
				if (Input.LastActionFromMouse) {
					if (mouseHoverArrowL && useLeftArrow) {
						Renderer.DrawPixel(rectL_H, MouseHighlightTint, int.MaxValue - 3);
					}
					if (mouseHoverArrowR && useRightArrow) {
						Renderer.DrawPixel(rectR_H, MouseHighlightTint, int.MaxValue - 3);
					}
				}

				// L Arrow
				if (useLeftArrow) Renderer.Draw(ArrowMarkCode, rectL, int.MaxValue - 2);

				// R Arrow
				if (useRightArrow) Renderer.Draw(ArrowMarkCode, rectR, int.MaxValue - 2);

			}

		}

		// Selection
		if (SelectionIndex == ItemCount) {
			if (!Input.LastActionFromMouse) {
				// Highlight
				Renderer.DrawPixel(hoverCheckingRect, MouseHighlightTint);
				// Hand
				var handRect = new IRect(
					itemRect.x - markSize.x + MarkPingPongFrame.PingPong(60),
					itemRect.y + (itemRect.height - markSize.y) / 2,
					markSize.x,
					markSize.y
				);
				Renderer.Draw(SelectionMarkCode, handRect, SelectionMarkTint);
			}
			// Invoke
			if (Interactable) {
				if (Input.GameKeyDown(Gamekey.Action) || Input.KeyboardDown(KeyboardKey.Enter)) {
					invoke = true;
				}
				if (useLeftArrow && (Input.GameKeyDownGUI(Gamekey.Left) || Input.KeyboardDownGUI(KeyboardKey.LeftArrow))) {
					delta = -1;
				}
				if (useRightArrow && (Input.GameKeyDownGUI(Gamekey.Right) || Input.KeyboardDownGUI(KeyboardKey.RightArrow))) {
					delta = 1;
				}
			}
			// Api
			SelectionAdjustable = useArrows;
		}

		// Mouse
		if (Input.MouseLeftButtonDown) {
			if (mouseHoverArrowL) {
				delta = -1;
			} else if (mouseHoverArrowR) {
				delta = 1;
			} else if (mouseHoverLabel) {
				invoke = true;
			}
		}
		if (itemRect_Old.MouseInside() && Input.LastActionFromMouse && SelectionIndex != ItemCount) {
			SetSelection(ItemCount);
		}
		if (mouseHoverLabel) {
			Cursor.SetCursorAsHand();
		}

		// Final
		ItemCount++;
		return useArrows ? delta != 0 : invoke;
	}


	// Misc
	/// <summary>
	/// Set current selecting item
	/// </summary>
	protected void SetSelection (int index) => RequireSetSelection = index;


	protected void RefreshAnimation () => AnimationFrame = 0;


	#endregion




}