﻿using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

internal partial class Engine {




	#region --- VAR ---


	// Const
	private static readonly LanguageCode LOG_ERROR_PROJECT_OBJECT_IS_NULL = ("Log.BuildError.ProjectObjectIsNull", "Build Error: Project object is Null");
	private static readonly LanguageCode LOG_ERROR_PROJECT_FOLDER_INVALID = ("Log.BuildError.ProjectFolderInvalid", "Build Error: Project folder path invalid");
	private static readonly LanguageCode LOG_ERROR_PUBLISH_DIR_INVALID = ("Log.BuildError.PublishDirInvalid", "Build Error: Publish folder path invalid");
	private static readonly LanguageCode LOG_ERROR_PROJECT_FOLDER_NOT_EXISTS = ("Log.BuildError.ProjectFolderNotExists", "Build Error: Project folder not exists");
	private static readonly LanguageCode LOG_ERROR_PRODUCT_NAME_INVALID = ("Log.BuildError.ProductNameInvalid", "Build Error: Product name invalid");
	private static readonly LanguageCode LOG_ERROR_DEV_NAME_INVALID = ("Log.BuildError.DevNameInvalid", "Build Error: Developer name invalid");
	private static readonly LanguageCode LOG_ERROR_RESULT_DLL_NOT_FOUND = ("Log.BuildError.ResultDllNotFound", "Build Error: Result dll file not found");
	private static readonly LanguageCode LOG_ERROR_RUNTIME_FILE_NOT_FOUND = ("Log.BuildError.RuntimeFileNotFound", "Build Error: Runtime file not found in the engine universe folder");
	private static readonly LanguageCode LOG_ERROR_UNIVERSE_FOLDER_NOT_FOUND = ("Log.BuildError.UniverseFolderNotFound", "Build Error: Universe folder not found");
	private static readonly LanguageCode LOG_ERROR_EXE_FOR_RUN_NOT_FOUND = ("Log.BuildError.ExeForRunNotFound", "Build Error: No Exe file to run");
	private static readonly LanguageCode LOG_ERROR_DOTNET_SDK_NOT_FOUND = ("Log.BuildError.DotnetSdkNotFound", "Build Error: Dotnet Sdk not found in the engine universe folder");
	private static readonly LanguageCode LOG_ERROR_ENTRY_PROJECT_NOT_FOUND = ("Log.BuildError.EntryProjectNotFound", "Build Error: Entry exe file for the project not found");
	private static readonly LanguageCode LOG_ERROR_ENTRY_RESULT_NOT_FOUND = ("Log.BuildError.EntryResultNotFound", "Build Error: Entry exe file result not found");
	private static readonly LanguageCode LOG_ERROR_CSPROJ_NOT_EXISTS = ("Log.BuildError.CsprojNotExists", "Csproj file not found");
	private static readonly LanguageCode LOG_BUILD_UNKNOWN = ("Log.BuildError.Unknown", "Unknown error on building project in background. Error code:{0}");
	private static readonly LanguageCode HINT_PUBLISHING = ("Hint.Publishing", "Publishing");

	// Data
	private readonly GUIStyle RigGameHintStyle = new(GUI.Skin.SmallCenterMessage) { LineSpace = 14 };
	private readonly RigTransceiver Transceiver = new(EngineUtil.RiggedExePath);
	private WindowUI CurrentWindow;
	private int RigGameFailToStartCount = 0;
	private int RigGameFailToStartFrame = int.MinValue;
	private long RequireBackgroundBuildDate = 0;
	private bool IgnoreInputForRig = false;
	private bool CurrentWindowRequireRigGame = false;
	private bool HasCompileError = false;
	private bool RequireBgmForRiggedGame = false;
	private int NoGameRunningFrameCount = 0;
	private int ForceRigGameRunInBackgroundFrame = -1;
	private int RenderingSheetIndex;


	#endregion




	#region --- MSG ---


	[OnProjectBuiltInBackground]
	internal static void RiggedGameRebuild (int code) {

		if (Instance == null) return;

		GameEditor.Instance.CleanDirty();
		Instance.HasCompileError = code != 0;

		switch (code) {

			case 0:
				Instance.RigGameFailToStartCount = 0;
				Instance.RigGameFailToStartFrame = int.MinValue;
				ConsoleWindow.Instance.RemoveAllCompileErrors();
				break;

			default:
				Debug.LogError(string.Format(LOG_BUILD_UNKNOWN, code));
				break;

			case EngineUtil.ERROR_USER_CODE_COMPILE_ERROR:
				ConsoleWindow.Instance.BeginCompileError();
				try {
					while (EngineUtil.BackgroundBuildMessages.Count > 0) {
						Debug.LogError(EngineUtil.BackgroundBuildMessages.Dequeue());
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
				ConsoleWindow.Instance.EndCompileError();
				break;

			case EngineUtil.ERROR_PROJECT_OBJECT_IS_NULL:
				Debug.LogError(LOG_ERROR_PROJECT_OBJECT_IS_NULL);
				break;

			case EngineUtil.ERROR_PROJECT_FOLDER_INVALID:
				Debug.LogError(LOG_ERROR_PROJECT_FOLDER_INVALID);
				break;

			case EngineUtil.ERROR_PUBLISH_DIR_INVALID:
				Debug.LogError(LOG_ERROR_PUBLISH_DIR_INVALID);
				break;

			case EngineUtil.ERROR_PROJECT_FOLDER_NOT_EXISTS:
				Debug.LogError(LOG_ERROR_PROJECT_FOLDER_NOT_EXISTS);
				break;

			case EngineUtil.ERROR_PRODUCT_NAME_INVALID:
				Debug.LogError(LOG_ERROR_PRODUCT_NAME_INVALID);
				break;

			case EngineUtil.ERROR_DEV_NAME_INVALID:
				Debug.LogError(LOG_ERROR_DEV_NAME_INVALID);
				break;

			case EngineUtil.ERROR_RESULT_DLL_NOT_FOUND:
				Debug.LogError(LOG_ERROR_RESULT_DLL_NOT_FOUND);
				break;

			case EngineUtil.ERROR_RUNTIME_FILE_NOT_FOUND:
				Debug.LogError(LOG_ERROR_RUNTIME_FILE_NOT_FOUND);
				break;

			case EngineUtil.ERROR_UNIVERSE_FOLDER_NOT_FOUND:
				Debug.LogError(LOG_ERROR_UNIVERSE_FOLDER_NOT_FOUND);
				break;

			case EngineUtil.ERROR_EXE_FOR_RUN_NOT_FOUND:
				Debug.LogError(LOG_ERROR_EXE_FOR_RUN_NOT_FOUND);
				break;

			case EngineUtil.ERROR_DOTNET_SDK_NOT_FOUND:
				Debug.LogError(LOG_ERROR_DOTNET_SDK_NOT_FOUND);
				break;

			case EngineUtil.ERROR_ENTRY_PROJECT_NOT_FOUND:
				Debug.LogError(LOG_ERROR_ENTRY_PROJECT_NOT_FOUND);
				break;

			case EngineUtil.ERROR_ENTRY_RESULT_NOT_FOUND:
				Debug.LogError(LOG_ERROR_ENTRY_RESULT_NOT_FOUND);
				break;

			case EngineUtil.ERROR_CSPROJ_NOT_EXISTS:
				Debug.LogError(LOG_ERROR_CSPROJ_NOT_EXISTS);
				break;
		}

	}


	[OnGameQuitting(1)]
	internal static void OnEngineQuittingRig () => Instance.Transceiver.Quit();


	private void OnGUI_RiggedGame () {

		bool openingGameEditor = CurrentWindow is GameEditor;
		bool currentWindowRequireRigGame = openingGameEditor || Game.GlobalFrame <= ForceRigGameRunInBackgroundFrame;

		// Quit if Not Game Project
		if (CurrentProject != null && CurrentProject.Universe.Info.ProjectType != ProjectType.Game) {
			if (Transceiver.RigProcessRunning) {
				Transceiver.Abort();
			}
			return;
		}

		ConsoleWindow.Instance.HaveRunningRigGame = Transceiver.RigProcessRunning;

		if (HasCompileError) return;

		bool requireRigGameRender = openingGameEditor;

		var gameEDT = GameEditor.Instance;
		var resp = Transceiver.RespondMessage;
		var console = ConsoleWindow.Instance;
		var lanEditor = LanguageEditor.Instance;
		var currentUniverse = CurrentProject?.Universe;
		bool runningGame = !gameEDT.FrameDebugging || gameEDT.RequireNextFrame;
		int sheetIndex = RenderingSheetIndex;
		bool ignoreInGameGizmos = gameEDT.IgnoreInGameGizmos;

		if (console.RequireCodeAnalysis != 0 || lanEditor.RequireAddKeysForAllLanguageCode) {
			ForceRigGameRunInBackgroundFrame = Game.GlobalFrame + 2;
		}
		gameEDT.ThumbnailSheetIndex = sheetIndex;

		Transceiver.LogWithPrefix = EngineSetting.AddPrefixMarkForMessageFromGame.Value;

		// Call
		bool called = OnGUI_Calling();

		// Respond
		bool buildingProjectInBackground = EngineUtil.BuildingProjectInBackground;
		NoGameRunningFrameCount = Transceiver.RigProcessRunning || buildingProjectInBackground ? 0 : NoGameRunningFrameCount + 1;

		if (CurrentProject == null) {
			if (Transceiver.RigProcessRunning) {
				Transceiver.Abort();
			}
			return;
		}

		// Abort when Building
		if (Transceiver.RigProcessRunning && buildingProjectInBackground) {
			Transceiver.Abort();
		}

		if (buildingProjectInBackground) {
			// Building in Background
			if (currentWindowRequireRigGame && requireRigGameRender) {
				Transceiver.UpdateLastRespondedRender(currentUniverse, sheetIndex, true, ignoreInGameGizmos, true);
			}
		} else if (Transceiver.RigProcessRunning) {
			// Rig Running
			if (called) {
				if (runningGame) {
					// Get Respond
					Transceiver.Respond(
						currentUniverse, sheetIndex,
						openingGameEditor,
						!requireRigGameRender,
						ignoreInGameGizmos
					);
					gameEDT.UpdateUsageData(
						resp.FrameDurationMilliSecond, resp.RenderUsages, resp.RenderCapacities, resp.EntityUsages, resp.EntityCapacities
					);
					gameEDT.SetRiggedGameInfo(resp.GlobalFrame, GetEngineLeftBarWidth(out _));
					gameEDT.HavingGamePlay = resp.GamePlaying;
					if (openingGameEditor) {
						Sky.ForceSkyboxTint(resp.SkyTop, resp.SkyBottom, 3);
					}
					RequireBgmForRiggedGame = Game.IsMusicPlaying;
				} else if (requireRigGameRender) {
					Transceiver.UpdateLastRespondedRender(currentUniverse, sheetIndex, false, ignoreInGameGizmos, false);
				}
			}
		} else if (
			(RigGameFailToStartCount < 16 && Game.GlobalFrame > RigGameFailToStartFrame + 30) ||
			Game.GlobalFrame > RigGameFailToStartFrame + 6000
		) {
			// No Rig Game Running
			int code = Transceiver.Start(
				CurrentProject.BuildPath,
				CurrentProject.UniversePath
			);
			if (code == 0) {
				// Start
				RigGameFailToStartCount = 0;
				RigGameFailToStartFrame = int.MinValue;
				SettingWindow.Instance.MapSettingChanged = true;
				GameEditor.Instance.LightMapSettingChanged = true;
			} else {
				// Fail to Start
				RigGameFailToStartFrame = Game.GlobalFrame;
				RigGameFailToStartCount++;
			}
			if (currentWindowRequireRigGame && requireRigGameRender) {
				// Still Render Last Image
				Transceiver.UpdateLastRespondedRender(currentUniverse, sheetIndex, true, ignoreInGameGizmos, true);
			}
		}

	}


	private bool OnGUI_Calling () {

		var gameEDT = GameEditor.Instance;
		bool runningGame = !gameEDT.FrameDebugging || gameEDT.RequireNextFrame;
		gameEDT.RequireNextFrame = false;

		if (
			CurrentProject == null ||
			EngineUtil.BuildingProjectInBackground ||
			!Transceiver.RigProcessRunning ||
			(CurrentWindow is not GameEditor && Game.GlobalFrame > ForceRigGameRunInBackgroundFrame)
		) return false;

		var console = ConsoleWindow.Instance;
		var lanEditor = LanguageEditor.Instance;
		var currentUniverse = CurrentProject?.Universe;
		var currentInfo = currentUniverse?.Info;
		bool requireRigInput = CurrentWindow is GameEditor;
		var calling = Transceiver.CallingMessage;

		// Ignore Input Check
		bool mouseInputIgnoreFromPanel = !WindowUI.WindowRect.MouseInside() ||
				gameEDT.PanelRect.MouseInside() ||
				gameEDT.ToolbarRect.MouseInside();
		if (Input.AnyMouseButtonDown || Input.MouseWheelDelta != 0) {
			IgnoreInputForRig = IgnoreInputForRig || mouseInputIgnoreFromPanel;
		}
		if (!Input.AnyMouseButtonHolding && Input.MouseWheelDelta == 0) {
			IgnoreInputForRig = false;
		}
		if (Input.IgnoringMouseInput) {
			IgnoreInputForRig = true;
		}
		if (!Input.AnyMouseButtonHolding && mouseInputIgnoreFromPanel) {
			IgnoreInputForRig = true;
		}

		// Game Editor Toolbox
		if (gameEDT.DrawCollider) {
			calling.RequireDrawColliderGizmos();
		}
		if (gameEDT.EntityClickerOn) {
			calling.RequireEntityClicker();
		}
		if (gameEDT.RequireOpenOrCloseMovementPanel.HasValue) {
			calling.RequireRemoteSetting(MovementEditor.SETTING_PANEL, gameEDT.RequireOpenOrCloseMovementPanel.Value);
			gameEDT.RequireOpenOrCloseMovementPanel = null;
		}
		if (gameEDT.RequireSetViewPos.HasValue) {
			var pos = gameEDT.RequireSetViewPos.Value;
			calling.RequireRemoteSetting(Stage.SETTING_SET_VIEW_X, pos.x);
			calling.RequireRemoteSetting(Stage.SETTING_SET_VIEW_Y, pos.y);
			calling.RequireRemoteSetting(Stage.SETTING_SET_VIEW_Z, pos.z);
			calling.RequireRemoteSetting(Stage.SETTING_SET_VIEW_H, pos.w);
			gameEDT.RequireSetViewPos = null;
		}

		// Tool Command
		if (console.RequireCodeAnalysis != 0) {
			ForceRigGameRunInBackgroundFrame = Game.GlobalFrame + 2;
			calling.RequireRemoteSetting(console.RequireCodeAnalysis == 1 ?
				FrameworkUtil.RUN_CODE_ANALYSIS_SETTING_ID :
				FrameworkUtil.RUN_CODE_ANALYSIS_SETTING_SILENTLY_ID,
				0
			);
			console.RequireCodeAnalysis = 0;
		}
		if (lanEditor.RequireAddKeysForAllLanguageCode) {
			ForceRigGameRunInBackgroundFrame = Game.GlobalFrame + 2;
			calling.RequireRemoteSetting(LanguageUtil.ADD_KEYS_FOR_ALL_LANGUAGE_CODE_SETTING_ID, 0);
			lanEditor.RequireAddKeysForAllLanguageCode = false;
		}

		// Map Editor Setting Changed
		if (SettingWindow.Instance.MapSettingChanged) {
			SettingWindow.Instance.MapSettingChanged = false;
			calling.RequireRemoteSetting(MapEditor.SETTING_QUICK_PLAYER_DROP, EngineSetting.MapEditor_QuickPlayerDrop.Value);
			calling.RequireRemoteSetting(MapEditor.SETTING_SHOW_BEHIND, EngineSetting.MapEditor_ShowBehind.Value);
			calling.RequireRemoteSetting(MapEditor.SETTING_SHOW_GRID_GIZMOS, EngineSetting.MapEditor_ShowGizmos.Value);
			calling.RequireRemoteSetting(MapEditor.SETTING_SHOW_STATE, EngineSetting.MapEditor_ShowState.Value);
		}

		// Lighting Map Setting Changed
		if (gameEDT.LightMapSettingChanged) {
			gameEDT.LightMapSettingChanged = false;
			if (gameEDT.ForcingInGameDaytime >= 0f) {
				calling.RequireRemoteSetting(
					LightingSystem.SETTING_IN_GAME_DAYTIME, (int)(gameEDT.ForcingInGameDaytime * 1000)
				);
			} else if (gameEDT.ForcingInGameDaytime < -1.001f) {
				gameEDT.ForcingInGameDaytime = -1f;
				calling.RequireRemoteSetting(
					LightingSystem.SETTING_FREE_IN_GAME_DAYTIME, 0
				);
			}
			calling.RequireRemoteSetting(
				LightingSystem.SETTING_PIXEL_STYLE, currentInfo.LightMap_PixelStyle
			);
			calling.RequireRemoteSetting(
				LightingSystem.SETTING_SELF_LERP, (int)(currentInfo.LightMap_SelfLerp * 1000)
			);
			calling.RequireRemoteSetting(
				LightingSystem.SETTING_AIR_ILLUMINANCE_DAY, (int)(currentInfo.LightMap_AirIlluminanceDay * 1000)
			);
			calling.RequireRemoteSetting(
				LightingSystem.SETTING_AIR_ILLUMINANCE_NIGHT, (int)(currentInfo.LightMap_AirIlluminanceNight * 1000)
			);
			calling.RequireRemoteSetting(
				LightingSystem.SETTING_BACKGROUND_TINT, (int)(currentInfo.LightMap_BackgroundTint * 1000)
			);
			calling.RequireRemoteSetting(
				LightingSystem.SETTING_SOLID_ILLUMINANCE, (int)(currentInfo.LightMap_SolidIlluminance * 1000)
			);
			calling.RequireRemoteSetting(
				LightingSystem.SETTING_LEVEL_ILLUMINATE_REMAIN, (int)(currentInfo.LightMap_LevelIlluminateRemain * 1000)
			);
			// Save Uni-Info to File
			JsonUtil.SaveJsonToPath(currentInfo, currentUniverse.InfoPath, prettyPrint: true);
		}

		// Make the Call
		if (runningGame) {
			Transceiver.Call(
				ignoreMouseInput: !requireRigInput || IgnoreInputForRig || Game.PauselessFrame < LastNotInteractableFrame + 6,
				ignoreKeyInput: !requireRigInput,
				paddingLeft: GetEngineLeftBarWidth(out _),
				paddingRight: gameEDT.ToolbarWidth,
				requiringWindowIndex: 0
			);
		}

		return true;
	}


	#endregion




}
