﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using AngeliA;
using Task = System.Threading.Tasks.Task;

namespace AngeliaEngine;


internal static class EngineUtil {



	#region --- VAR ---


	// Const
	public const int ERROR_PROJECT_OBJECT_IS_NULL = -100;
	public const int ERROR_PROJECT_FOLDER_INVALID = -101;
	public const int ERROR_PUBLISH_DIR_INVALID = -102;
	public const int ERROR_PROJECT_FOLDER_NOT_EXISTS = -103;
	public const int ERROR_PRODUCT_NAME_INVALID = -104;
	public const int ERROR_DEV_NAME_INVALID = -105;
	public const int ERROR_RESULT_DLL_NOT_FOUND = -106;
	public const int ERROR_RUNTIME_FILE_NOT_FOUND = -107;
	public const int ERROR_UNIVERSE_FOLDER_NOT_FOUND = -108;
	public const int ERROR_EXE_FOR_RUN_NOT_FOUND = -109;
	public const int ERROR_DOTNET_SDK_NOT_FOUND = -110;
	public const int ERROR_ENTRY_PROJECT_NOT_FOUND = -111;
	public const int ERROR_ENTRY_RESULT_NOT_FOUND = -112;
	public const int ERROR_USER_CODE_COMPILE_ERROR = -113;
	public const int ERROR_CSPROJ_NOT_EXISTS = -114;
	private const int BACK_GROUND_BUILD_LOG_ID = 102735648;
	private static readonly int[] ICON_SIZES = [64, 128, 256, 512, 1024, 2048];

	// Api
	public static string DotnetSdkPath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "dotnet", "dotnet.exe");
	public static string ProjectTemplatePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate");
	public static string TemplateFrameworkDll_Debug => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate", "lib", "Debug", "AngeliA Framework.dll");
	public static string TemplateFrameworkDll_Release => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate", "lib", "Release", "AngeliA Framework.dll");
	public static string EntryExePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Entry.exe");
	public static string RiggedExePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Rigged.exe");
	public static string AngeRaylibDllPath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Raylib.dll");
	public static string EntryProjectFolder => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Release");
	public static string EntryProjectCsproj => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Release", "AngeliA Entry for Publish.csproj");
	public static string ThemeRoot => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Theme");
	public static string PackagesRoot => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Packages");
	public static string CustomPackagesRoot => Util.CombinePaths(AngePath.PersistentDataPath, "Packages");
	public static bool BuildingProjectInBackground => BuildProjectTask != null && BuildProjectTask.Status == TaskStatus.Running;
	public static long LastBackgroundBuildModifyDate { get; private set; }
	public static int LastBackgroundBuildReturnCode { get; private set; }
	public static Queue<string> BackgroundBuildMessages { get; } = new(capacity: 32);

	// Cache
	[OnProjectBuiltInBackground] internal static System.Action<int> OnProjectBuiltInBackgroundHandler;

	// Data
	private static readonly StringBuilder CacheBuilder = new();
	private static Task BuildProjectTask = null;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		Debug.OnLogInternal += OnLogMessage;
		Debug.OnLogErrorInternal += OnLogMessage;
	}


	#endregion




	#region --- API ---


	// AngeliA
	public static int BuildAngeliaProject (Project project) {
		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;
		return BuildAngeliaProjectLogic(project, "", publish: false, logID: 0);
	}


	public static int PublishAngeliaProject (Project project, string publishDir) {
		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;
		return BuildAngeliaProjectLogic(
			project, publishDir, publish: true, logID: 0
		);
	}


	public static bool BuildAngeliaProjectInBackground (Project project, long srcModifyDate) {

		// Gate
		if (project == null) return false;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return false;
		if (BuildingProjectInBackground) return false;

		// Project >> Cache
		LastBackgroundBuildReturnCode = int.MinValue;
		LastBackgroundBuildModifyDate = srcModifyDate;

		// Task
		BuildProjectTask = Task.Factory.StartNew(BuildFromCache, project);

		return true;

		// Func
		static void BuildFromCache (object projectObj) {
			try {
				if (projectObj is not Project project) return;
				BackgroundBuildMessages.Clear();
				LastBackgroundBuildReturnCode = BuildAngeliaProjectLogic(project, "", publish: false);
				OnProjectBuiltInBackgroundHandler?.Invoke(LastBackgroundBuildReturnCode);
			} catch (System.Exception ex) {
				System.Console.WriteLine(ex.Message + "\n" + ex.Source);
			}
		}
	}


	public static void RunAngeliaBuild (Project project) {
		string entryPath = EntryExePath;
		if (!Util.FileExists(entryPath)) return;
		var process = new Process();
		process.StartInfo.FileName = entryPath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = false;
		process.StartInfo.Arguments = $"DontCloseCmd -uni:{Util.Path_to_ArgPath(project.UniversePath)} -lib:{Util.Path_to_ArgPath(project.BuildPath)}";
		process.StartInfo.WorkingDirectory = Util.GetParentPath(entryPath);
		process.Start();
	}


	public static string GetGameLibraryDllNameWithoutExtension (string productName) => $"lib.{productName}";


	// Modyfy Date
	public static long GetScriptModifyDate (Project project) {
		if (project == null || !Util.FolderExists(project.SourceCodePath)) return 0;
		long result = 0;
		foreach (var path in Util.EnumerateFiles(project.SourceCodePath, false, "*.cs")) {
			result = System.Math.Max(result, Util.GetFileModifyDate(path));
		}
		return result;
	}


	public static long GetBuildLibraryModifyDate (Project project) {
		if (project == null || !Util.FolderExists(project.BuildPath)) return 0;
		long result = 0;
		foreach (string path in Util.EnumerateFiles(project.BuildPath, true, "*.dll")) {
			result = System.Math.Max(result, Util.GetFileModifyDate(path));
		}
		return result;
	}


	// Res
	public static object[] LoadTexturesFromIco (string icoPath, bool firstOneOnly = false) {

		using var stream = File.OpenRead(icoPath);
		using var reader = new BinaryReader(stream);

		reader.ReadUInt16(); // ignore. Should be 0

		ushort type = reader.ReadUInt16();
		if (type != 1) {
			throw new System.Exception("Invalid type. The stream is not an icon file");
		}
		ushort num_of_images = reader.ReadUInt16();

		if (num_of_images == 0) return [];

		if (firstOneOnly) num_of_images = 1;
		var results = new object[num_of_images];
		var cache = new (uint offset, uint size, int bpp)[num_of_images];

		for (var i = 0; i < num_of_images; i++) {
			var width = reader.ReadByte();
			var height = reader.ReadByte();
			var colors = reader.ReadByte();
			reader.ReadByte(); // ignore. Should be 0
			var color_planes = reader.ReadUInt16(); // should be 0 or 1
			ushort bits_per_pixel = reader.ReadUInt16();
			uint size = reader.ReadUInt32();
			uint offset = reader.ReadUInt32();
			cache[i] = (offset, size, bits_per_pixel / 8);
		}

		for (int imgIndex = 0; imgIndex < num_of_images; imgIndex++) {
			var (offset, size, bpp) = cache[imgIndex];
			if (reader.BaseStream.Position < offset) {
				var dummy_bytes_to_read = (int)(offset - reader.BaseStream.Position);
				if (stream.CanSeek) {
					stream.Seek(dummy_bytes_to_read, SeekOrigin.Current);
				} else {
					reader.ReadBytes(dummy_bytes_to_read);
				}
			}
			var bytes = reader.ReadBytes((int)size);

			if (bytes == null || bytes.Length <= 4) continue;

			if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4e && bytes[3] == 0x47) {
				// PNG
				results[imgIndex] = Game.PngBytesToTexture(bytes);
			} else {
				// BMP
				int dataOffset = bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
				int width = bytes[4] | (bytes[5] << 8) | (bytes[6] << 16) | (bytes[7] << 24);
				int height = (bytes[8] | (bytes[9] << 8) | (bytes[10] << 16) | (bytes[11] << 24)).Abs() / 2;
				if (width != height) {
					Debug.LogWarning($"Icon with must be same with height ({width} x {height})");
					continue;
				}
				if (width > 4096 || height > 4096) continue;
				var colors = new Color32[width * height];
				int index = 0;
				if (bpp == 4) {
					// BGRA
					for (int i = dataOffset; i < bytes.Length - 3 && index < colors.Length; i += 4, index++) {
						colors[index].b = bytes[i + 0];
						colors[index].g = bytes[i + 1];
						colors[index].r = bytes[i + 2];
						colors[index].a = bytes[i + 3];
					}
				} else if (bpp == 3) {
					// BGR
					for (int i = dataOffset; i < bytes.Length - 2 && index < colors.Length; i += 3, index++) {
						colors[index].b = bytes[i + 0];
						colors[index].g = bytes[i + 1];
						colors[index].r = bytes[i + 2];
						colors[index].a = 255;
					}
				} else if (bpp == 1) {
					// Grey
					for (int i = dataOffset; i < bytes.Length && index < colors.Length; i++, index++) {
						byte value = bytes[i];
						colors[index].a = 255;
						colors[index].r = value;
						colors[index].g = value;
						colors[index].b = value;
					}
				}

				results[imgIndex] = Game.GetTextureFromPixels(colors, width, height);

			}

		}

		return results;
	}


	public static bool CreateIcoFromPng (string pngPath, string icoPath) {
		if (
			!Util.FileExists(pngPath) ||
			Game.PngBytesToTexture(Util.FileToBytes(pngPath)) is not object texture ||
			!Game.IsTextureReady(texture)
		) return false;
		var pngBytes = new byte[ICON_SIZES.Length][];
		for (int i = 0; i < ICON_SIZES.Length; i++) {
			int size = ICON_SIZES[i];
			var newTexture = Game.GetResizedTexture(texture, size, size);
			if (Game.IsTextureReady(newTexture)) {
				pngBytes[i] = Game.TextureToPngBytes(newTexture);
			} else {
				return false;
			}
			Game.UnloadTexture(newTexture);
		}
		Game.UnloadTexture(texture);
		return CreateIcoFromPng(pngBytes, icoPath);
	}


	public static bool CreateIcoFromPng (byte[][] pngBytes, string icoPath) {
		if (pngBytes == null || pngBytes.Length == 0) return false;

		using var stream = File.OpenWrite(icoPath);
		using var writer = new BinaryWriter(stream);

		int numOfImage = pngBytes.Length;

		writer.Write((ushort)0); // (ignore)

		writer.Write((ushort)1); // type
		writer.Write((ushort)numOfImage); // num_of_images

		const int INFO_SIZE = 16;
		int offsetHead = (int)writer.BaseStream.Position + INFO_SIZE * numOfImage;
		int currentOffset = offsetHead;

		for (int i = 0; i < numOfImage; i++) {
			writer.Write((byte)0); // w
			writer.Write((byte)0); // h
			writer.Write((byte)0); // colors
			writer.Write((byte)0); // (ignore)
			writer.Write((ushort)1); // color_planes
			writer.Write((ushort)32); // bits_per_pixel
			writer.Write((uint)pngBytes[i].Length);   // size
			writer.Write((uint)currentOffset);   // offset
			currentOffset += pngBytes[i].Length;
		}

		for (int i = 0; i < numOfImage; i++) {
			writer.Write(pngBytes[i]); // data
		}

		writer.Flush();
		return true;
	}


	// Resources
	public static void ImportMusicFile (Project project, string filePath) {
		if (project == null) return;
		Util.CopyFile(filePath, Util.CombinePaths(
			project.Universe.MusicRoot,
			Util.GetNameWithExtension(filePath)
		));
		Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, project.UniversePath);
	}


	public static void ImportSoundFile (Project project, string filePath) {
		if (project == null) return;
		Util.CopyFile(filePath, Util.CombinePaths(
			project.Universe.SoundRoot,
			Util.GetNameWithExtension(filePath)
		));
		Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, project.UniversePath);
	}


	public static void ImportFontFile (Project project, string filePath) {
		if (project == null) return;
		string newName = Util.GetNameWithExtension(filePath);
		Util.CopyFile(filePath, Util.CombinePaths(project.Universe.FontRoot, newName));
		Game.SyncFontsWithPool(project.Universe.FontRoot);
	}


	public static bool ImportIconFile (Project project, string filePath) {
		if (project == null) return false;
		return CreateIcoFromPng(filePath, project.IconPath);
	}


	#endregion




	#region --- LGC ---


	private static int BuildAngeliaProjectLogic (Project project, string publishDir, bool publish, int logID = BACK_GROUND_BUILD_LOG_ID) {

		var info = project.Universe.Info;
		string projectPath = project.ProjectPath;
		string csprojPath = project.CsprojPath;
		string productName = info.ProductName;
		string buildPath = project.BuildPath;
		string tempBuildPath = project.TempBuildPath;
		string tempPublishPath = project.TempPublishPath;
		string tempRoot = project.TempRoot;
		string iconPath = project.IconPath;
		string universePath = project.UniversePath;
		string versionStr = $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}";
		string libAssemblyName = GetGameLibraryDllNameWithoutExtension(productName);

#if DEBUG
		//var watch = Stopwatch.StartNew();
		//Debug.Log("Start to Build AngeliA Project");
#endif
		if (!Util.IsPathValid(projectPath)) return ERROR_PROJECT_FOLDER_INVALID;
		if (publish && !Util.IsPathValid(publishDir)) return ERROR_PUBLISH_DIR_INVALID;
		if (publish && !Util.FolderExists(EntryProjectFolder)) return ERROR_ENTRY_PROJECT_NOT_FOUND;
		if (!Util.FolderExists(projectPath)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;
		if (!Util.FileExists(csprojPath)) return ERROR_CSPROJ_NOT_EXISTS;
		if (!Util.FileExists(DotnetSdkPath)) return ERROR_DOTNET_SDK_NOT_FOUND;
		if (!Util.IsValidForFileName(productName)) return ERROR_PRODUCT_NAME_INVALID;


		// ===== Build =====


		// Delete Build Library Folder
		Util.DeleteFolder(buildPath);

		// Build Dotnet Project
		int returnCode = BuildDotnetProject(
			projectPath,
			DotnetSdkPath,
			csprojPath,
			publish: false,
			debug: !publish,
			assemblyName: libAssemblyName,
			version: versionStr,
			outputPath: tempBuildPath,
			logID: logID
		);

		if (returnCode != 0) return ERROR_USER_CODE_COMPILE_ERROR;

		string resultDllPath = Util.CombinePaths(tempBuildPath, $"{libAssemblyName}.dll");
		if (!Util.FileExists(resultDllPath)) return ERROR_RESULT_DLL_NOT_FOUND;

		// Copy Result Dll to Lib Folder
		string gameLibDllName = Util.GetNameWithExtension(resultDllPath);
		string gameLibBuildPath = Util.CombinePaths(buildPath, gameLibDllName);
		Util.CreateFolder(buildPath);
		Util.CopyFile(resultDllPath, gameLibBuildPath);

		// Copy Package Dlls to Lib Folder
		string debugLibFolder = Project.GetLibraryFolderPath(projectPath, true);
		foreach (var packagDllPath in Util.EnumerateFiles(debugLibFolder, true, "*.dll")) {
			string dllName = Util.GetNameWithExtension(packagDllPath);
			if (dllName == "AngeliA Framework.dll") continue;
			Util.CopyFile(packagDllPath, Util.CombinePaths(buildPath, dllName));
		}


		// ===== Publish =====


		if (publish) {

			// Build Entry Exe for Publish
			int pubReturnCode = BuildDotnetProject(
				EntryProjectFolder,
				DotnetSdkPath,
				EntryProjectCsproj,
				publish: true,
				debug: false,
				assemblyName: productName,
				version: versionStr,
				outputPath: tempBuildPath,
				publishDir: tempPublishPath,
				iconPath: iconPath,
				logID: -1
			);
			if (pubReturnCode != 0) return returnCode;

			// Move Result Exe to Publish Folder
			string resultExePath = Util.CombinePaths(tempPublishPath, $"{productName}.exe");
			if (!Util.FileExists(resultExePath)) return ERROR_ENTRY_RESULT_NOT_FOUND;
			string pubResultExePath = Util.CombinePaths(publishDir, $"{productName}.exe");
			Util.CopyFile(resultExePath, pubResultExePath);

			// Copy Game Libs to Publish Folder
			if (!Util.FileExists(gameLibBuildPath)) return ERROR_RESULT_DLL_NOT_FOUND;
			string publishLibRoot = Util.CombinePaths(publishDir, "Library");
			Util.CopyFile(gameLibBuildPath, Util.CombinePaths(publishLibRoot, gameLibDllName));

			// Copy Package Dlls to Lib Folder
			string releaseLibFolder = Project.GetLibraryFolderPath(projectPath, false);
			foreach (var packagDllPath in Util.EnumerateFiles(releaseLibFolder, true, "*.dll")) {
				string dllName = Util.GetNameWithExtension(packagDllPath);
				if (dllName == "AngeliA Framework.dll") continue;
				Util.CopyFile(packagDllPath, Util.CombinePaths(publishLibRoot, dllName));
			}

			// Copy Universe to Publish Folder
			if (!Util.FolderExists(universePath)) return ERROR_UNIVERSE_FOLDER_NOT_FOUND;
			Util.CreateFolder(publishDir);
			string universePublishFolderPath = Util.CombinePaths(publishDir, "Universe");
			Util.CopyFolder(universePath, universePublishFolderPath, true, true);

		}

		// Delete Temp Folder
		Util.DeleteFolder(tempRoot);
#if DEBUG
		//watch.Stop();
		//Debug.Log($"{watch.ElapsedMilliseconds / 1000f:0.00}s AngeliA Project Built Finish");
#endif
		return 0;
	}


	private static int BuildDotnetProject (
		string projectFolder, string sdkPath, string csprojPath, bool publish, bool debug, int logID,
		string assemblyName = "", string version = "", string outputPath = "",
		string publishDir = "", string iconPath = ""
	) {

		if (!Util.FolderExists(projectFolder)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;
		if (!Util.FileExists(csprojPath)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;

		CacheBuilder.Clear();

		// SDK
		CacheBuilder.AppendWithDoubleQuotes(sdkPath);

		// Build
		CacheBuilder.Append(publish ? " publish " : " build ");

		// Project Path
		CacheBuilder.AppendWithDoubleQuotes(csprojPath);

		// Config
		CacheBuilder.Append(debug ? " -c debug" : " -c release");

		// Dependencies
		CacheBuilder.Append(" --no-dependencies");

		// Prop
		if (!string.IsNullOrWhiteSpace(assemblyName)) {
			CacheBuilder.Append($" -p:AssemblyName=\"{assemblyName}\"");
		}
		if (!string.IsNullOrWhiteSpace(version)) {
			CacheBuilder.Append($" -p:Version={version}");
		}
		if (!string.IsNullOrWhiteSpace(outputPath)) {
			CacheBuilder.Append($" -p:OutputPath=\"{outputPath}\"");
		}
		if (!string.IsNullOrWhiteSpace(publishDir)) {
			CacheBuilder.Append($" -p:PublishDir=\"{publishDir}\"");
		}
		if (!string.IsNullOrWhiteSpace(iconPath) && Util.FileExists(iconPath)) {
			CacheBuilder.Append($" -p:ApplicationIcon=\"{iconPath}\"");
		}

		int resultID = Util.ExecuteCommand(projectFolder, CacheBuilder.ToStringWithDoubleQuotes(), logID: logID);

		return resultID;
	}


	private static void OnLogMessage (int id, string message) {
		if (id != BACK_GROUND_BUILD_LOG_ID) return;
		BackgroundBuildMessages.Enqueue(message);
	}


	#endregion




}