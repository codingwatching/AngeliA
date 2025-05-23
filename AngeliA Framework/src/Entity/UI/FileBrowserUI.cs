using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Built-in file brower entity for PC only
/// </summary>
[EntityAttribute.StageOrder(4095)]
[EntityAttribute.Capacity(1, 1)]
public sealed class FileBrowserUI : EntityUI, IWindowEntityUI {




	#region --- SUB ---


	private enum BrowserActionType { Open, Save, }
	private enum BrowserTargetType { File, Folder, }


	private class ItemData {
		public string Path;
		public string DisplayName;
		public bool IsFolder;
		public AngeSprite Icon;
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly int TYPE_ID = typeof(FileBrowserUI).AngeHash();
	private static readonly LanguageCode FILE_NAME = ("FileBrowser.FileName", "File Name");
	private static readonly LanguageCode FOLDER_NAME = ("FileBrowser.FolderName", "Folder Name");
	private static readonly LanguageCode ERROR_NAME_EMPTY = ("FileBrowser.Error.NameEmpty", "Name can't be empty");
	private static readonly LanguageCode ERROR_NAME_INVALID = ("FileBrowser.Error.NameInValid", "Name contains invalid char");
	private static readonly LanguageCode FAV_DESKTOP = ("FileBrowser.Fav.Desktop", "Desktop");
	private static readonly LanguageCode FAV_PIC = ("FileBrowser.Fav.Pic", "My Picture");
	private static readonly Dictionary<int, SpriteCode> FileIconPool = new() {
		{ ".mp3".AngeHash(), BuiltInSprite.FILE_ICON_AUDIO},
		{ ".wav".AngeHash(), BuiltInSprite.FILE_ICON_AUDIO},
		{ ".ogg".AngeHash(), BuiltInSprite.FILE_ICON_AUDIO},
		{ ".xm".AngeHash(), BuiltInSprite.FILE_ICON_AUDIO},
		{ ".mod".AngeHash(), BuiltInSprite.FILE_ICON_AUDIO},
		{ ".txt".AngeHash(), BuiltInSprite.FILE_ICON_TEXT},
		{ ".json".AngeHash(), BuiltInSprite.FILE_ICON_TEXT},
		{ ".png".AngeHash(), BuiltInSprite.FILE_ICON_IMAGE},
		{ ".jpg".AngeHash(), BuiltInSprite.FILE_ICON_IMAGE},
		{ ".ttf".AngeHash(), BuiltInSprite.FILE_ICON_FONT},
	};

	// Api
	/// <summary>
	/// Global single instance for this class
	/// </summary>
	public static FileBrowserUI Instance { get; private set; }
	/// <summary>
	/// True if the entity is currently displaying
	/// </summary>
	public static bool ShowingBrowser => Instance != null && Instance.Active;
	/// <summary>
	/// Rect position for background range in global space
	/// </summary>
	public IRect BackgroundRect { get; private set; }
	protected override bool BlockEvent => true;
	/// <summary>
	/// Path of the current browsing folder
	/// </summary>
	public string CurrentFolder { get; set; } = "";
	/// <summary>
	/// Name of the current selecting file/folder
	/// </summary>
	public string CurrentName { get; set; } = "";
	/// <summary>
	/// Title of the current window
	/// </summary>
	public string Title { get; set; } = "";
	/// <summary>
	/// Files that will be display inside the content panel
	/// </summary>
	public string[] SearchPatterns { get; private set; }

	// Data
	private static readonly List<string> Disks = [];
	private readonly List<ItemData> Items = [];
	private System.Action<string> OnPathPicked = null;
	private BrowserActionType ActionType;
	private BrowserTargetType TargetType;
	private string ErrorMessage = "";
	private string NavbarText = "";
	private string FileTypeName = "";
	private int ScrollY = 0;
	private int LastSelectFrame = -1;
	private int SelectingIndex = -1;


	#endregion




	#region --- MSG ---


	public FileBrowserUI () => Instance = this;


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Disks.Clear();
		for (int i = 2; i < 26; i++) {
			string path = $"{(char)('A' + i)}:\\";
			if (Util.FolderExists(path)) {
				Disks.Add(path);
			}
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		SearchPatterns = null;
		ErrorMessage = "";
		NavbarText = "";
		FileTypeName = "";
		ScrollY = 0;
		SelectingIndex = -1;
		Items.Clear();
		Width = Unify(800);
		Height = Unify(618);
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Cursor.RequireCursor();
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Input.IgnoreMouseToActionJump();
	}


	public override void UpdateUI () {
		base.UpdateUI();

		int bgPadding = Unify(12);
		int navBarHeight = Unify(32);
		int titleHeight = Unify(32);
		int favPanelWidth = Unify(196);
		int controlPanelHeight = Unify(86);
		int lineSize = Unify(5);

		// Curtain
		Renderer.DrawPixel(Renderer.CameraRect, Color32.BLACK_128, int.MinValue);

		// BG
		X = Renderer.CameraRect.CenterX() - Width / 2;
		Y = Renderer.CameraRect.CenterY() - Height / 2;
		BackgroundRect = Rect.Expand(bgPadding);
		Renderer.DrawPixel(BackgroundRect, Color32.BLACK, 0);

		// Lines
		Renderer.Draw(BuiltInSprite.SOFT_LINE_H, new IRect(X, Y + Height - navBarHeight - lineSize / 2, Width, lineSize), Color32.GREY_20, z: 1);
		Renderer.Draw(BuiltInSprite.SOFT_LINE_H, new IRect(X, Y + controlPanelHeight - lineSize / 2, Width, lineSize), Color32.GREY_20, z: 1);
		Renderer.Draw(BuiltInSprite.SOFT_LINE_V, new IRect(X + favPanelWidth - lineSize / 2, Y + controlPanelHeight, lineSize, Height - controlPanelHeight - navBarHeight), Color32.GREY_20, z: 1);

		// Title
		GUI.SmallLabel(Rect.EdgeInside(Direction4.Up, titleHeight).Shrink(Unify(6), 0, 0, 0), Title);

		// Panels
		Update_NavigationBar(Rect.EdgeInside(Direction4.Up, navBarHeight).Shift(0, -titleHeight));
		Update_Favorite(Rect.Shrink(0, Width - favPanelWidth, controlPanelHeight, navBarHeight + titleHeight));
		Update_ControlPanel(Rect.EdgeInside(Direction4.Down, controlPanelHeight));
		Update_Explorer(Rect.Shrink(favPanelWidth, 0, controlPanelHeight, navBarHeight + titleHeight));

		// Hotkey
		if (!GUI.IsTyping) {
			if (Input.KeyboardDown(KeyboardKey.Escape)) {
				Input.UseKeyboardKey(KeyboardKey.Escape);
				Input.UseGameKey(Gamekey.Start);
				OnPathPicked = null;
				Active = false;
				Input.UseAllMouseKey();
			}
		}

		// Final
		Input.UseGameKey(Gamekey.Action);
		Input.UseGameKey(Gamekey.Jump);

	}


	private void Update_NavigationBar (IRect barRect) {

		int buttonSize = barRect.height;
		int buttonPadding = Unify(2);
		var rect = barRect.EdgeInside(Direction4.Left, buttonSize);

		// Parent
		rect.x += buttonPadding;
		if (GUI.Button(rect.Shrink(Unify(6)), BuiltInSprite.ICON_TRIANGLE_LEFT, GUI.Skin.IconButton) || (!GUI.IsTyping && Input.KeyboardDown(KeyboardKey.Backspace))) {
			string parentPath = Util.GetParentPath(CurrentFolder);
			if (!string.IsNullOrEmpty(parentPath)) {
				CurrentName = ActionType == BrowserActionType.Open ? Util.GetNameWithExtension(CurrentFolder) : CurrentName;
				Explore(parentPath);
			}
		}

		// Address
		rect.x += buttonSize + buttonPadding;
		rect.width = barRect.xMax - rect.x;
		NavbarText = GUI.SmallInputField(12124, rect, NavbarText, out _, out bool confirm);
		if (confirm) Explore(NavbarText);

	}


	private void Update_Explorer (IRect panelRect) {

		if (!Util.FolderExists(CurrentFolder)) return;

		int scrollBarWidth = GUI.ScrollbarSize;
		panelRect.width -= scrollBarWidth;
		var paddedPanelRect = panelRect.Shrink(Unify(12));
		int itemWidth = Unify(184);
		int itemHeight = Unify(56);
		int iconSize = Unify(40);
		int column = paddedPanelRect.width / itemWidth;
		int pageRow = paddedPanelRect.height / itemHeight;
		int totalRow = Items.Count.CeilDivide(column);
		int extendedRow = totalRow + 2;

		// Scroll
		if (extendedRow > pageRow) {
			ScrollY = (ScrollY - Input.MouseWheelDelta * 2).Clamp(0, extendedRow - pageRow);
			ScrollY = GUI.ScrollBar(
				2376, paddedPanelRect.EdgeOutsideRight(scrollBarWidth),
				ScrollY, extendedRow, pageRow
			);
		} else {
			ScrollY = 0;
		}

		// Content
		bool mouseInsideItem = false;
		int itemPadding = Unify(4);
		var rect = new IRect(0, 0, itemWidth, itemHeight);
		for (int row = ScrollY; row < ScrollY + pageRow; row++) {
			rect.y = paddedPanelRect.yMax - ((row - ScrollY + 1) * itemHeight);
			for (int x = 0; x < column; x++) {
				int index = row * column + x;
				if (index >= Items.Count) goto _END_;
				var item = Items[index];
				rect.x = paddedPanelRect.x + x * itemWidth;
				var paddedRect = rect.Shrink(itemPadding);

				// Hover Highlight
				if (rect.MouseInside()) {
					Renderer.DrawPixel(rect, Color32.GREY_20, z: 1);
					mouseInsideItem = true;
				}

				// Selecting Highlight
				if (SelectingIndex == index) {
					Renderer.DrawPixel(rect, Color32.GREY_56, z: 1);
				}

				// Icon
				if (item.Icon != null) {
					Renderer.Draw(
						item.Icon,
						new IRect(paddedRect.x, paddedRect.yMax - iconSize, iconSize, iconSize),
						z: 2
					);
				}

				// Name Label
				GUI.SmallLabel(paddedRect.Shrink(iconSize + itemPadding, 0, 0, itemPadding), item.DisplayName);

				// Click
				if (rect.MouseInside() && Input.MouseLeftButtonDown) {
					if (SelectingIndex == index && Game.PauselessFrame < LastSelectFrame + 30) {
						// Double Click
						LastSelectFrame = -1;
						if (item.IsFolder) {
							Explore(item.Path);
						} else if (ActionType == BrowserActionType.Open && TargetType == BrowserTargetType.File) {
							PerformPick();
						}
					} else {
						// Single Click
						LastSelectFrame = Game.PauselessFrame;
						SelectingIndex = index;
						if (ActionType == BrowserActionType.Open || !item.IsFolder) {
							CurrentName = item.IsFolder ? Util.GetNameWithExtension(item.Path) : Util.GetNameWithoutExtension(item.Path);
						}
					}
					Input.UseMouseKey(0);
				}

			}
		}
		_END_:;

		// Cancel Selection
		if (SelectingIndex >= 0 && Input.MouseLeftButtonDown && !mouseInsideItem) {
			SelectingIndex = -1;
		}

	}


	private void Update_Favorite (IRect panelRect) {

		panelRect = panelRect.Shrink(Unify(6));
		int buttonSize = Unify(32);
		int iconShrink = Unify(6);
		int padding = Unify(4);
		var rect = panelRect.EdgeInside(Direction4.Up, buttonSize);
		rect = rect.Shrink(buttonSize + padding, 0, 0, 0);

		// Buttons
		DrawButton(
			FAV_DESKTOP,
			System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
			BuiltInSprite.FILE_ICON_FOLDER
		);
		DrawButton(
			FAV_PIC,
			System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures),
			BuiltInSprite.FILE_ICON_FOLDER
		);

		// Disks
		foreach (var diskPath in Disks) {
			DrawButton(diskPath, diskPath, BuiltInSprite.FILE_ICON_DISK);
		}

		// Func
		void DrawButton (string label, string path, int icon) {
			if (GUI.Button(rect, label, GUI.Skin.SmallLabelButton)) Explore(path);
			Renderer.Draw(icon, new IRect(rect.x - buttonSize - padding, rect.y, buttonSize, buttonSize).Shrink(iconShrink));
			rect.y -= rect.height;
		}

	}


	private void Update_ControlPanel (IRect panelRect) {

		int padding = Unify(6);
		int buttonHeight = Unify(32);
		int buttonWidth = Unify(108);
		int typeFieldWidth = Unify(256);
		int labelWidth = Unify(128);
		int frameBorder = Unify(1);
		int fieldHeight = Unify(32);
		var fieldRect = new IRect(
			panelRect.x + labelWidth + padding * 2,
			panelRect.yMax - fieldHeight - padding,
			panelRect.width - labelWidth - typeFieldWidth - padding * 4,
			fieldHeight
		);
		var typeRect = fieldRect.EdgeOutsideRight(typeFieldWidth);
		typeRect.x += padding;

		if (ActionType == BrowserActionType.Save) {
			// Name Field
			CurrentName = GUI.SmallInputField(091253, fieldRect, CurrentName);

			// Name Label
			GUI.SmallLabel(
				fieldRect.EdgeOutsideLeft(labelWidth).Shift(-padding * 2, 0),
				TargetType == BrowserTargetType.Folder ? FOLDER_NAME : FILE_NAME
			);
		}

		// Type Field
		if (TargetType == BrowserTargetType.File) {
			GUI.Label(typeRect, FileTypeName, GUI.Skin.SmallCenterLabel);
			Renderer.DrawSlice(
				BuiltInSprite.FRAME_16, typeRect, frameBorder, frameBorder, frameBorder, frameBorder, Color32.GREY_32, z: 1
			);
		}

		// Error Msg
		if (!string.IsNullOrEmpty(ErrorMessage)) {
			GUI.SmallLabel(panelRect.EdgeInside(Direction4.Down, buttonHeight), ErrorMessage);
		}

		// Cancel Button
		var buttonRect = new IRect(panelRect.xMax, panelRect.y, buttonWidth, buttonHeight);
		buttonRect.x -= buttonRect.width + padding;
		if (GUI.Button(buttonRect, BuiltInText.UI_CANCEL, GUI.Skin.SmallDarkButton)) {
			Input.UseAllMouseKey();
			ErrorMessage = string.Empty;
			OnPathPicked = null;
			Active = false;
		}

		// OK Button
		buttonRect.x -= buttonRect.width + padding;
		using (new GUIEnableScope(
			ActionType != BrowserActionType.Open ||
			TargetType != BrowserTargetType.File ||
			(SelectingIndex >= 0 && !Items[SelectingIndex].IsFolder)
		)) {
			if (GUI.Button(
				buttonRect, ActionType == BrowserActionType.Open ? BuiltInText.UI_OPEN : BuiltInText.UI_SAVE,
				GUI.Skin.SmallDarkButton
			)) {
				PerformPick();
			}
		}

	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Start file browser for open an existing folder
	/// </summary>
	/// <param name="title">Title of the window</param>
	/// <param name="onFolderOpen">Callback function for a folder being open</param>
	public static void OpenFolder (string title, System.Action<string> onFolderOpen) => SpawnBrowserLogic(title, "", null, BrowserActionType.Open, BrowserTargetType.Folder, onFolderOpen);
	/// <summary>
	/// Start file browser for open an existing file
	/// </summary>
	/// <param name="title">Title of the window</param>
	/// <param name="onFileOpen">Callback function for a file being open</param>
	/// <param name="searchPatterns">Which file should be display. eg."*.png", "*.jpg"</param>
	public static void OpenFile (string title, System.Action<string> onFileOpen, params string[] searchPatterns) => SpawnBrowserLogic(title, "", searchPatterns, BrowserActionType.Open, BrowserTargetType.File, onFileOpen);
	/// <summary>
	/// Start file browser for create/save a folder
	/// </summary>
	/// <param name="title">Title of the window</param>
	/// <param name="defaultFolderName">Default name shown in the input field</param>
	/// <param name="onFolderSaved">Callback function for the folder being create/save</param>
	public static void SaveFolder (string title, string defaultFolderName, System.Action<string> onFolderSaved) => SpawnBrowserLogic(title, defaultFolderName, null, BrowserActionType.Save, BrowserTargetType.Folder, onFolderSaved);
	/// <summary>
	/// Start file browser for create/save a file
	/// </summary>
	/// <param name="title">Title of the window</param>
	/// <param name="defaultFileNameWithExt">Default name shown in the input field. eg. "name.png"</param>
	/// <param name="onFileSaved">Callback function for the file being create/save</param>
	/// <param name="searchPatterns">Which type of file is allowed be create/save. eg."*.png", "*.jpg"</param>
	public static void SaveFile (string title, string defaultFileNameWithExt, System.Action<string> onFileSaved, params string[] searchPatterns) => SpawnBrowserLogic(title, defaultFileNameWithExt, searchPatterns, BrowserActionType.Save, BrowserTargetType.File, onFileSaved);


	#endregion




	#region --- LGC ---


	private static void SpawnBrowserLogic (string title, string defaultName, string[] searchPatterns, BrowserActionType actionType, BrowserTargetType targetType, System.Action<string> callback) {
		FileBrowserUI browser;
		if (Stage.Enable) {
			if (Stage.FindEntity(TYPE_ID) != null) return;
			browser = Stage.SpawnEntity(TYPE_ID, 0, 0) as FileBrowserUI;
			if (browser == null) return;
		} else {
			browser = Instance;
			browser.Active = true;
			browser.SpawnFrame = Game.GlobalFrame;
			browser.OnActivated();
		}
		if (string.IsNullOrEmpty(browser.CurrentFolder) || !Util.FolderExists(browser.CurrentFolder)) {
			browser.CurrentFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
		}
		browser.Title = title;
		browser.CurrentName = defaultName;
		browser.ActionType = actionType;
		browser.SearchPatterns = searchPatterns;
		browser.FileTypeName =
			targetType == BrowserTargetType.Folder ? "" :
			searchPatterns == null || searchPatterns.Length == 0 ? "*" :
			string.Join('|', searchPatterns);
		browser.OnPathPicked = callback;
		browser.TargetType = targetType;
		browser.Explore(browser.CurrentFolder);
	}


	private void Explore (string path) {
		const int MAX_NAME_LEN = 16;
		ScrollY = 0;
		LastSelectFrame = -1;
		SelectingIndex = -1;
		if (!Util.FolderExists(path)) {
			NavbarText = CurrentFolder;
			return;
		}
		Items.Clear();
		Renderer.TryGetSprite(BuiltInSprite.FILE_ICON_FOLDER, out var folderSprite);
		Renderer.TryGetSprite(BuiltInSprite.FILE_ICON_FILE, out var commenFileSprite);
		foreach (string folderPath in Util.EnumerateFolders(path, true)) {
			if (Util.IsHidden(folderPath)) continue;
			string name = Util.GetNameWithExtension(folderPath);
			Items.Add(new ItemData() {
				DisplayName = name.Length <= MAX_NAME_LEN ? name : $"{name[..(MAX_NAME_LEN - 2)]}..",
				Icon = folderSprite,
				IsFolder = true,
				Path = folderPath,
			});
			if (Util.GetNameWithExtension(name) == CurrentName) {
				SelectingIndex = Items.Count - 1;
			}
		}
		foreach (string filePath in Util.EnumerateFiles(path, true, SearchPatterns)) {
			if (Util.IsHidden(filePath)) continue;
			string name = Util.GetNameWithExtension(filePath);
			int fileSpriteID = FileIconPool.TryGetValue(
				Util.GetExtensionWithDot(name).AngeHash(), out var fileSpriteCode
			) ? fileSpriteCode : 0;
			Items.Add(new ItemData() {
				DisplayName = name.Length <= MAX_NAME_LEN ? name : $"{name[..(MAX_NAME_LEN - 2)]}..",
				Icon = fileSpriteID != 0 && Renderer.TryGetSprite(fileSpriteID, out var fileSprite) ? fileSprite : commenFileSprite,
				IsFolder = false,
				Path = filePath,
			});
		}
		// Final
		NavbarText = CurrentFolder = path;
	}


	private void PerformPick () {

		ErrorMessage = string.Empty;
		bool isSaving = ActionType == BrowserActionType.Save;
		bool forFolder = TargetType == BrowserTargetType.Folder;

		if (isSaving) {
			// Name Empty Check
			if (string.IsNullOrWhiteSpace(CurrentName)) {
				ErrorMessage = ERROR_NAME_EMPTY;
				return;
			}
			// Name Valid Check
			if (CurrentName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0) {
				ErrorMessage = ERROR_NAME_INVALID;
				return;
			}
		}

		// Get Target Path
		string targetPath;
		if (forFolder) {
			// For Folder
			if (isSaving) {
				targetPath = Util.CombinePaths(CurrentFolder, CurrentName);
			} else {
				if (SelectingIndex < 0) {
					targetPath = CurrentFolder;
				} else {
					targetPath = Util.CombinePaths(CurrentFolder, CurrentName);
				}
			}
		} else {
			// For File
			if (isSaving) {
				// Save File
				targetPath = Util.CombinePaths(CurrentFolder, CurrentName);
			} else {
				// Open File
				if (SelectingIndex >= 0 && SelectingIndex < Items.Count) {
					var selectingItem = Items[SelectingIndex];
					if (selectingItem.IsFolder) {
						Explore(selectingItem.Path);
						return;
					}
					string ext = Util.GetExtensionWithDot(selectingItem.Path);
					targetPath = Util.CombinePaths(CurrentFolder, $"{CurrentName}{ext}");
				} else {
					return;
				}
			}
		}
		targetPath = Util.FixPath(targetPath);

		// Perform
		if (isSaving || (forFolder ? Util.FolderExists(targetPath) : Util.FileExists(targetPath))) {
			try {
				OnPathPicked?.Invoke(targetPath);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
		OnPathPicked = null;
		Active = false;
		Input.UseAllMouseKey();
	}


	#endregion




}