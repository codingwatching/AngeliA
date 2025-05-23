﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AngeliA;

public static partial class Util {


	// VAR
	private const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct MathfInternal {
		public static volatile float FloatMinNormal = 1.17549435E-38f;
		public static volatile float FloatMinDenormal = float.Epsilon;
		public static bool IsFlushToZeroEnabled = FloatMinDenormal == 0f;
	}

	/// <summary>
	/// Rad angle * Rad2Deg = Deg angle
	/// </summary>
	public const float Rad2Deg = 57.29578f;
	/// <summary>
	/// Deg angle * Deg2Rad = Rad angle
	/// </summary>
	public const float Deg2Rad = PI / 180f;
	/// <summary>
	/// π = 3.14159274f
	/// </summary>
	public const float PI = 3.14159274f;
	internal static readonly float Epsilon = MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinNormal : MathfInternal.FloatMinDenormal;
	private static int QuickRandomSeed = 73633632;


	// Language
	private static readonly Dictionary<string, string> IsoToDisplayName = new() { { "af", "Afrikaans" }, { "am", "Amharic" }, { "ar", /**/"العربية" }, { "as",/**/ "অসমীয়া" }, { "az", /**/"azərbaycan" }, { "be", "беларуская" }, { "bg", "български" }, { "bn",/**/ "বাংলা" }, { "bo", "藏語" }, { "br", "brezhoneg" }, { "bs", "bosanski" }, { "ca", "català" }, { "cs", /**/"čeština" }, { "cy", "Cymraeg" }, { "da", "dansk" }, { "de", "Deutsch" }, { "el", /**/"Ελληνικά" }, { "en", "English" }, { "es", "español" }, { "et", "eesti" }, { "eu", "euskara" }, { "fa",/**/ "فارسی" }, { "ff", "Pulaar" }, { "fi", "suomi" }, { "fo", "føroyskt" }, { "fr", "français" }, { "fy", "West-Frysk" }, { "ga", "Gaeilge" }, { "gd", "Gàidhlig" }, { "gl", "galego" }, { "gu",/**/ "ગુજરાતી" }, { "ha", "Hausa" }, { "he", /**/"עברית" }, { "hi", /**/"हिन्दी" }, { "hr", "hrvatski" }, { "hu", "magyar" }, { "hy", /**/"հայերեն" }, { "id", "Indonesia" }, { "ig", "Igbo" }, { "ii", "彜語" }, { "is", "íslenska" }, { "it", "italiano" }, { "iv", "Invariant" }, { "ja", "日本語" }, { "ka",/**/ "ქართული" }, { "kk",/**/ "қазақ тілі" }, { "kl", "kalaallisut" }, { "km", "Kambodschanisch" }, { "kn",/**/ "ಕನ್ನಡ" }, { "ko", /**/"한국어" }, { "ky", "кыргызча" }, { "lb", "Lëtzebuergesch" }, { "lo", "Laotisch" }, { "lt", /**/"lietuvių" }, { "lv", /**/"latviešu" }, { "mk", "македонски" }, { "ml",/**/ "മലയാളം" }, { "mn", "монгол" }, { "mr", /**/"मराठी" }, { "ms", "Bahasa Melayu" }, { "mt", "Malti" }, { "my", "Birmanisch" }, { "nb", "norsk" }, { "ne", /**/"नेपाली" }, { "nl", "Nederlands" }, { "nn", "nynorsk" }, { "om", "Oromoo" }, { "or", /**/"ଓଡ଼ିଆ" }, { "pa", /**/"ਪੰਜਾਬੀ" }, { "pl", "polski" }, { "ps", /**/"پښتو" }, { "pt", "português" }, { "rm", "rumantsch" }, { "ro", "română" }, { "ru", "русский" }, { "rw", "Kinyarwanda" }, { "se", "davvisámegiella" }, { "si",/**/ "සිංහල" }, { "sk",/**/ "slovenčina" }, { "sl", /**/"slovenščina" }, { "so", "Soomaali" }, { "sq", "shqip" }, { "sr", "српски" }, { "st", "Sesotho" }, { "sv", "svenska" }, { "sw", "Kiswahili" }, { "ta",/**/ "தமிழ்" }, { "te", /**/"తెలుగు" }, { "tg",/**/ "Тоҷикӣ" }, { "th",/**/ "ไทย" }, { "ti", "Tigrinja-Sprache" }, { "tk", "türkmençe" }, { "tn", "Setswana" }, { "tr", "Türkçe" }, { "ts", "Xitsonga" }, { "tt", "татар" }, { "ug", /**/"ئۇيغۇرچە" }, { "uk",/**/ "українська" }, { "ur", /**/"اردو" }, { "uz", "o‘zbek" }, { "vi", "Tiếng Việt" }, { "xh", "isiXhosa" }, { "yo", "Èdè Yorùbá" }, { "zhs", "简体中文" }, { "zht", "正體中文" }, { "zu", "isiZulu" }, };
	private static readonly Dictionary<string, string> IsoToEnglishDisplayName = new()
	{
		{ "af", "Afrikaans" }, { "am", "Amharic" }, { "ar", "Arabic" }, { "as", "Assamese" },
		{ "az", "Azerbaijani" }, { "be", "Belarusian" }, { "bg", "Bulgarian" }, { "bn", "Bengali" },
		{ "bo", "Tibetan" }, { "br", "Breton" }, { "bs", "Bosnian" }, { "ca", "Catalan" },
		{ "cs", "Czech" }, { "cy", "Welsh" }, { "da", "Danish" }, { "de", "German" },
		{ "el", "Greek" }, { "en", "English" }, { "es", "Spanish" }, { "et", "Estonian" },
		{ "eu", "Basque" }, { "fa", "Persian" }, { "ff", "Pulaar" }, { "fi", "Finnish" },
		{ "fo", "Faroese" }, { "fr", "French" }, { "fy", "West Frisian" }, { "ga", "Irish" },
		{ "gd", "Scottish Gaelic" }, { "gl", "Galician" }, { "gu", "Gujarati" }, { "ha", "Hausa" },
		{ "he", "Hebrew" }, { "hi", "Hindi" }, { "hr", "Croatian" }, { "hu", "Hungarian" },
		{ "hy", "Armenian" }, { "id", "Indonesian" }, { "ig", "Igbo" }, { "ii", "Yi" },
		{ "is", "Icelandic" }, { "it", "Italian" }, { "iv", "Invariant" }, { "ja", "Japanese" },
		{ "ka", "Georgian" }, { "kk", "Kazakh" }, { "kl", "Greenlandic" }, { "km", "Khmer" },
		{ "kn", "Kannada" }, { "ko", "Korean" }, { "ky", "Kyrgyz" }, { "lb", "Luxembourgish" },
		{ "lo", "Lao" }, { "lt", "Lithuanian" }, { "lv", "Latvian" }, { "mk", "Macedonian" },
		{ "ml", "Malayalam" }, { "mn", "Mongolian" }, { "mr", "Marathi" }, { "ms", "Malay" },
		{ "mt", "Maltese" }, { "my", "Burmese" }, { "nb", "Norwegian" }, { "ne", "Nepali" },
		{ "nl", "Dutch" }, { "nn", "Nynorsk" }, { "om", "Oromo" }, { "or", "Odia" },
		{ "pa", "Punjabi" }, { "pl", "Polish" }, { "ps", "Pashto" }, { "pt", "Portuguese" },
		{ "rm", "Romansh" }, { "ro", "Romanian" }, { "ru", "Russian" }, { "rw", "Kinyarwanda" },
		{ "se", "Northern Sami" }, { "si", "Sinhala" }, { "sk", "Slovak" }, { "sl", "Slovenian" },
		{ "so", "Somali" }, { "sq", "Albanian" }, { "sr", "Serbian" }, { "st", "Sesotho" },
		{ "sv", "Swedish" }, { "sw", "Swahili" }, { "ta", "Tamil" }, { "te", "Telugu" },
		{ "tg", "Tajik" }, { "th", "Thai" }, { "ti", "Tigrinya" }, { "tk", "Turkmen" },
		{ "tn", "Tswana" }, { "tr", "Turkish" }, { "ts", "Tsonga" }, { "tt", "Tatar" },
		{ "ug", "Uyghur" }, { "uk", "Ukrainian" }, { "ur", "Urdu" }, { "uz", "Uzbek" },
		{ "vi", "Vietnamese" }, { "xh", "Xhosa" }, { "yo", "Yoruba" }, { "zhs", "Chinese Simplified" },
		{ "zht", "Chinese Traditional" }, { "zu", "Zulu" }
	};

	/// <summary>
	/// Get display name of the given language in it's native language
	/// </summary>
	/// <param name="iso">ISO of the language (eg. "en" for English, "zht" for Chinese Traditional, "de" for German)</param>
	/// <param name="displayName">Result name</param>
	/// <returns>True if the iso is valid</returns>
	public static bool TryGetLanguageDisplayName (string iso, out string displayName) => IsoToDisplayName.TryGetValue(iso, out displayName);
	/// <summary>
	/// Get display name of the given language in English
	/// </summary>
	/// <param name="iso">ISO of the language (eg. "en" for English, "zht" for Chinese Traditional, "de" for German)</param>
	/// <returns>Result name. Return empty string when iso not valid.</returns>
	public static string GetLanguageDisplayNameInEnglish (string iso) => IsoToEnglishDisplayName.TryGetValue(iso, out var displayName) ? displayName : "";
	/// <summary>
	/// Get display name of the given language in it's native language
	/// </summary>
	/// <param name="iso">ISO of the language (eg. "en" for English, "zht" for Chinese Traditional, "de" for German)</param>
	/// <returns>Result name. Return empty string when iso not valid.</returns>
	public static string GetLanguageDisplayName (string iso) => IsoToDisplayName.TryGetValue(iso, out var displayName) ? displayName : "";
	/// <summary>
	/// True if the given iso is valid
	/// </summary>
	public static bool IsSupportedLanguageISO (string iso) => IsoToDisplayName.ContainsKey(iso);
	/// <summary>
	/// Iterate through iso of all language AngeliA support
	/// </summary>
	public static IEnumerable<string> ForAllSystemLanguages () => IsoToDisplayName.Keys;


	// Input
	private static readonly Dictionary<KeyboardKey, string> KeyDisplayName = new() { { KeyboardKey.NumpadDivide, "num /" }, { KeyboardKey.NumpadEquals, "num =" }, { KeyboardKey.NumpadMinus, "num -" }, { KeyboardKey.NumpadMultiply, "num *" }, { KeyboardKey.NumpadPeriod, "num ." }, { KeyboardKey.NumpadPlus, "num +" }, { KeyboardKey.F1, "F1" }, { KeyboardKey.F2, "F2" }, { KeyboardKey.F3, "F3" }, { KeyboardKey.F4, "F4" }, { KeyboardKey.F5, "F5" }, { KeyboardKey.F6, "F6" }, { KeyboardKey.F7, "F7" }, { KeyboardKey.F8, "F8" }, { KeyboardKey.F9, "F9" }, { KeyboardKey.F10, "F10" }, { KeyboardKey.F11, "F11" }, { KeyboardKey.F12, "F12" }, { KeyboardKey.Space, "Space" }, { KeyboardKey.Enter, "Enter" }, { KeyboardKey.Tab, "Tab" }, { KeyboardKey.Backquote, "`" }, { KeyboardKey.Quote, "'" }, { KeyboardKey.Semicolon, ";" }, { KeyboardKey.Comma, "," }, { KeyboardKey.Period, "." }, { KeyboardKey.Slash, "/" }, { KeyboardKey.Backslash, "\\" }, { KeyboardKey.LeftBracket, "[" }, { KeyboardKey.RightBracket, "]" }, { KeyboardKey.Minus, "-" }, { KeyboardKey.Equals, "=" }, { KeyboardKey.A, "A" }, { KeyboardKey.B, "B" }, { KeyboardKey.C, "C" }, { KeyboardKey.D, "D" }, { KeyboardKey.E, "E" }, { KeyboardKey.F, "F" }, { KeyboardKey.G, "G" }, { KeyboardKey.H, "H" }, { KeyboardKey.I, "I" }, { KeyboardKey.J, "J" }, { KeyboardKey.K, "K" }, { KeyboardKey.L, "L" }, { KeyboardKey.M, "M" }, { KeyboardKey.N, "N" }, { KeyboardKey.O, "O" }, { KeyboardKey.P, "P" }, { KeyboardKey.Q, "Q" }, { KeyboardKey.R, "R" }, { KeyboardKey.S, "S" }, { KeyboardKey.T, "T" }, { KeyboardKey.U, "U" }, { KeyboardKey.V, "V" }, { KeyboardKey.W, "W" }, { KeyboardKey.X, "X" }, { KeyboardKey.Y, "Y" }, { KeyboardKey.Z, "Z" }, { KeyboardKey.Digit1, "1" }, { KeyboardKey.Digit2, "2" }, { KeyboardKey.Digit3, "3" }, { KeyboardKey.Digit4, "4" }, { KeyboardKey.Digit5, "5" }, { KeyboardKey.Digit6, "6" }, { KeyboardKey.Digit7, "7" }, { KeyboardKey.Digit8, "8" }, { KeyboardKey.Digit9, "9" }, { KeyboardKey.Digit0, "0" }, { KeyboardKey.LeftShift, "Shift" }, { KeyboardKey.RightShift, "Shift" }, { KeyboardKey.LeftAlt, "Alt" }, { KeyboardKey.RightAlt, "Alt" }, { KeyboardKey.LeftCtrl, "Ctrl" }, { KeyboardKey.RightCtrl, "Ctrl" }, { KeyboardKey.Escape, "ESC" }, { KeyboardKey.LeftArrow, "←" }, { KeyboardKey.RightArrow, "→" }, { KeyboardKey.UpArrow, "↑" }, { KeyboardKey.DownArrow, "↓" }, { KeyboardKey.Backspace, "Backspace" }, { KeyboardKey.PageDown, "PageDown" }, { KeyboardKey.PageUp, "PageUp" }, { KeyboardKey.Home, "Home" }, { KeyboardKey.End, "End" }, { KeyboardKey.Insert, "Insert" }, { KeyboardKey.Delete, "Delete" }, { KeyboardKey.CapsLock, "CapsLock" }, { KeyboardKey.NumLock, "NumLock" }, { KeyboardKey.PrintScreen, "PrintScreen" }, { KeyboardKey.ScrollLock, "ScrollLock" }, { KeyboardKey.Numpad0, "Num 0" }, { KeyboardKey.Numpad1, "Num 1" }, { KeyboardKey.Numpad2, "Num 2" }, { KeyboardKey.Numpad3, "Num 3" }, { KeyboardKey.Numpad4, "Num 4" }, { KeyboardKey.Numpad5, "Num 5" }, { KeyboardKey.Numpad6, "Num 6" }, { KeyboardKey.Numpad7, "Num 7" }, { KeyboardKey.Numpad8, "Num 8" }, { KeyboardKey.Numpad9, "Num 9" }, };
	/// <summary>
	/// Get display name in English for given keyboard key
	/// </summary>
	public static string GetKeyDisplayName (KeyboardKey key) => KeyDisplayName.TryGetValue(key, out var value) ? value : string.Empty;


	// Misc
	public static int GetAngeHashForClassName (string className) {
		if (string.IsNullOrEmpty(className)) return 0;
		if (char.IsLower(className[0])) className = className[1..];
		return className.AngeHash();
	}


	/// <summary>
	/// Add given key and value into system Environment Variable
	/// </summary>
	public static void AddEnvironmentVariable (string key, string value) {
		string oldPath = System.Environment.GetEnvironmentVariable(
			key, System.EnvironmentVariableTarget.Process
		) ?? "";
		System.Environment.SetEnvironmentVariable(
			key, oldPath.Insert(0, $"{value};"), System.EnvironmentVariableTarget.Process
		);
	}


	/// <summary>
	/// Make the given string (in English) looks better for user to read.
	/// 1. If the string start with "m_". Remove the "m_";
	/// 2. Replace all "_" into " "
	/// 3. Add space before capital letters that behind a lowercase letter
	/// </summary>
	public static string GetDisplayName (string name) {

		// Remove "m_" at Start
		if (name.Length > 2 && name[0] == 'm' && name[1] == '_') {
			name = name[2..];
		}

		// Replace "_" to " "
		name = name.Replace('_', ' ');

		// Add " " Space Between "a Aa"
		for (int i = 0; i < name.Length - 1; i++) {
			char a = name[i];
			char b = name[i + 1];
			if (
				char.IsLetter(a) &&
				(char.IsLetter(b) || char.IsNumber(b)) &&
				!char.IsUpper(a) &&
				(char.IsUpper(b) || char.IsNumber(b))
			) {
				name = name.Insert(i + 1, " ");
				i++;
			}
		}

		return name;
	}


	public static bool TryGetIntFromString (string str, int startIndex, out int value, out int endIndex) {
		value = 0;
		for (endIndex = startIndex; endIndex < str.Length; endIndex++) {
			char c = str[endIndex];
			if (!char.IsNumber(c)) break;
			value = value * 10 + (c - '0');
		}
		return startIndex != endIndex;
	}


	/// <inheritdoc cref="QuickSort{T}(Span{T}, int, int, IComparer{T})"/>
	public static void QuickSort<T> (T[] array, int min, int max, IComparer<T> comparer) {
		int lo = min;
		int hi = max;
		T pvt = array[(min + max) / 2];
		while (lo <= hi) {
			while (comparer.Compare(array[lo], pvt) < 0) lo++;
			while (comparer.Compare(array[hi], pvt) > 0) hi--;
			if (lo > hi) break;
			(array[lo], array[hi]) = (array[hi], array[lo]);
			lo++;
			hi--;
		}
		if (min < hi) QuickSort(array, min, hi, comparer);
		if (lo < max) QuickSort(array, lo, max, comparer);
	}
	/// <summary>
	/// Sort the given array with QuickSort algorithm.
	/// </summary>
	/// <typeparam name="T">Type of the array</typeparam>
	/// <param name="array">The array data</param>
	/// <param name="min">Index range left (included)</param>
	/// <param name="max">Index range right (included)</param>
	/// <param name="comparer">Comparer instance to comparer the array element</param>
	public static void QuickSort<T> (Span<T> array, int min, int max, IComparer<T> comparer) {
		int lo = min;
		int hi = max;
		T pvt = array[(min + max) / 2];
		while (lo <= hi) {
			while (comparer.Compare(array[lo], pvt) < 0) lo++;
			while (comparer.Compare(array[hi], pvt) > 0) hi--;
			if (lo > hi) break;
			(array[lo], array[hi]) = (array[hi], array[lo]);
			lo++;
			hi--;
		}
		if (min < hi) QuickSort(array, min, hi, comparer);
		if (lo < max) QuickSort(array, lo, max, comparer);
	}


	/// <summary>
	/// Get a random color without spending too much CPU resources
	/// </summary>
	/// <param name="minH">(from 0 to 360)</param>
	/// <param name="maxH">(from 0 to 360)</param>
	/// <param name="minS">(from 0 to 100)</param>
	/// <param name="maxS">(from 0 to 100)</param>
	/// <param name="minV">(from 0 to 100)</param>
	/// <param name="maxV">(from 0 to 100)</param>
	/// <param name="minA">(from 0 to 255)</param>
	/// <param name="maxA">(from 0 to 255)</param>
	public static Color32 QuickRandomColor (int minH = 0, int maxH = 360, int minS = 0, int maxS = 100, int minV = 0, int maxV = 100, int minA = 0, int maxA = 255) {
		var result = HsvToRgb(
			QuickRandom(minH, maxH) / 360f,
			QuickRandom(minS, maxS) / 100f,
			QuickRandom(minV, maxV) / 100f
		);
		result.a = (byte)QuickRandom(minA, maxA);
		return result;
	}


	/// <summary>
	/// Get a random int between min and max without spending too much CPU resources. ("min" is Included, "max" is Excluded)
	/// </summary>
	public static int QuickRandom (int min, int max) => (QuickRandomSeed = QuickRandomWithSeed(QuickRandomSeed)).UMod((max - min).GreaterOrEquel(1)) + min;
	/// <summary>
	/// Get a random int between int.MinValue and int.MaxValue without spending too much CPU resources.
	/// </summary>
	public static int QuickRandom () => QuickRandomSeed = QuickRandomWithSeed(QuickRandomSeed);
	/// <summary>
	/// Get a random int which is -1 or 1 without spending too much CPU resources
	/// </summary>
	public static int QuickRandomSign () => QuickRandom() % 2 == 0 ? 1 : -1;
	/// <summary>
	/// Get a random int between min and max without spending too much CPU resources. ("min" is Included, "max" is Excluded)
	/// </summary>
	/// <param name="seed">Same seed always return same result</param>
	/// <param name="min"></param>
	/// <param name="max"></param>
	public static int QuickRandomWithSeed (int seed, int min, int max) => QuickRandomWithSeed(seed).UMod((max - min).GreaterOrEquel(1)) + min;
	/// <summary>
	/// Get a random int between int.MinValue and int.MaxValue without spending too much CPU resources.
	/// </summary>
	/// <param name="seed">Same seed always return same result</param>
	public static int QuickRandomWithSeed (int seed) {
		seed = (seed * 1103515245 + 12345) % 23456789;
		seed = (seed * 16807) % 2147483647;
		seed = (seed ^ (seed >> 16)) % 2147483647;
		seed = (seed * 2127912213) % 2147483647;
		return seed;
	}
	/// <summary>
	/// Get a random int between min and max without spending too much CPU resources. ("min" is Included, "max" is Excluded)
	/// </summary>
	/// <param name="seed">Same seed always return same result</param>
	/// <param name="min"></param>
	/// <param name="max"></param>
	public static int QuickRandomWithSeed (long seed, int min, int max) => QuickRandomWithSeed(seed).UMod((max - min).GreaterOrEquel(1)) + min;
	/// <summary>
	/// Get a random int between int.MinValue and int.MaxValue without spending too much CPU resources. ("min" is Included, "max" is Excluded)
	/// </summary>
	/// <param name="seed">Same seed always return same result</param>
	public static int QuickRandomWithSeed (long seed) {
		seed = (seed * 12234503515245 + 72456224) % 2223423456789;
		seed = (seed * 168689307) % 21470543323483647;
		seed = (seed ^ (seed >> 23)) % 4243214724483647;
		seed = (seed * 212791213672213) % 214748223573647;
		return (int)seed;
	}


	/// <summary>
	/// Perform a command with the terminal
	/// </summary>
	/// <param name="workingDirectory">Path of the working directory</param>
	/// <param name="arguments"></param>
	/// <param name="logID">(-1 means ignore log. 0 means log with Debug.Log. >=1 means log with Debug.LogInternal)</param>
	/// <param name="wait">True if wait the terminal process to exit</param>
	/// <returns>Return code</returns>
	public static int ExecuteCommand (string workingDirectory, string arguments, int logID = -1, bool wait = true) {
		try {
			var info = new ProcessStartInfo {
				Verb = "runas",
				FileName = "cmd.exe",
				Arguments = $"/C {arguments}",
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = workingDirectory,
			};
			var process = Process.Start(info);
			if (logID >= 0) {
				string line;
				while ((line = process.StandardOutput.ReadLine()) != null) {
					if (logID == 0) {
						Debug.Log(line);
					} else {
						Debug.LogInternal(logID, line);
					}
				}
				while ((line = process.StandardError.ReadLine()) != null) {
					if (logID == 0) {
						Debug.LogError(line);
					} else {
						Debug.LogErrorInternal(logID, line);
					}
				}
			}
			if (wait) {
				process.WaitForExit(30_000);
				return process.ExitCode;
			} else {
				return 0;
			}
		} catch (Exception ex) {
			Debug.LogException(ex);
			return -1;
		}
	}


	/// <summary>
	/// Get index of the nearby segment of the given string.
	/// (eg. for "Test Content String 123" if the start index is 5 which is start of "Content", toRight is true, return value will be 12 which is the end of "Content".)
	/// </summary>
	/// <param name="content"></param>
	/// <param name="start"></param>
	/// <param name="toRight">True if move the cursor to right side</param>
	/// <returns>Index of the next step segment</returns>
	public static int FindNextStringStep (string content, int start, bool toRight) {
		int result = start;
		int delta = toRight ? 1 : -1;
		bool flag = false;
		if (string.IsNullOrEmpty(content)) return -1;
		start = start.Clamp(0, content.Length - 1);
		for (int i = start; i < content.Length && i >= 0; i += delta) {
			result = i;
			char c = content[i];
			if (char.IsWhiteSpace(c) || (!char.IsLetter(c) && !char.IsNumber(c) && c != '_')) {
				if (flag) return i;
			} else {
				flag = true;
			}
		}
		return result + (toRight ? 1 : 0);
	}


	/// <summary>
	/// Trim the clear pixels for the given pixels and create a new instance for the result
	/// </summary>
	/// <param name="pixels">Source pixel data. (will not be changed after this operation)</param>
	/// <param name="width">Width in pixel</param>
	/// <param name="height">Height in pixel</param>
	/// <param name="left">Left position that first found a pixel in whole column</param>
	/// <param name="right">Right position that first found a pixel in whole column</param>
	/// <param name="down">Bottom position that first found a pixel in whole row</param>
	/// <param name="up">Top position that first found a pixel in whole row</param>
	/// <returns>New instance of pixel data created</returns>
	public static Color32[] GetTrimedPixels (in Color32[] pixels, int width, int height, out int left, out int right, out int down, out int up) {

		left = 0;
		right = width - 1;
		down = 0;
		up = height - 1;

		// L
		for (int i = 0; i < width; i++) {
			left = i;
			for (int j = 0; j < height; j++) {
				if (pixels[j * width + i].a > 0) goto _L_FOUND_;
			}
		}
		_L_FOUND_:;

		// R
		for (int i = width - 1; i >= left; i--) {
			right = i;
			for (int j = 0; j < height; j++) {
				if (pixels[j * width + i].a > 0) goto _R_FOUND_;
			}
		}
		_R_FOUND_:;

		// D
		for (int j = 0; j < height; j++) {
			down = j;
			for (int i = 0; i < width; i++) {
				if (pixels[j * width + i].a > 0) goto _D_FOUND_;
			}
		}
		_D_FOUND_:;

		// U
		for (int j = height - 1; j >= down; j--) {
			up = j;
			for (int i = 0; i < width; i++) {
				if (pixels[j * width + i].a > 0) goto _U_FOUND_;
			}
		}
		_U_FOUND_:;

		int newW = right - left + 1;
		int newH = up - down + 1;
		var result = new Color32[newW * newH];
		int index = 0;
		for (int j = down; j <= up; j++) {
			for (int i = left; i <= right; i++) {
				result[index] = pixels[j * width + i];
				index++;
			}
		}
		return result;
	}



	/// <summary>
	/// Value from one range to another, while maintaining it's relative position within the original range.
	/// When "t" is "l", return "newL". 
	/// When "t" is "r", return "newR". 
	/// When "t" is something in the middle, it find a point between "newL" and "newR".
	/// </summary>
	/// <param name="l">Left range for t</param>
	/// <param name="r">Right range for t</param>
	/// <param name="newL">Left range for the result</param>
	/// <param name="newR">Right range for the result</param>
	/// <param name="t">Original value</param>
	/// <returns>Remaped value</returns>
	[MethodImpl(INLINE)]
	public static float Remap (float l, float r, float newL, float newR, float t) => l == r ? newL : Lerp(newL, newR, (t - l) / (r - l));

	/// <inheritdoc cref="Remap(float, float, float, float, float)"/>
	[MethodImpl(INLINE)]
	public static float RemapUnclamped (float l, float r, float newL, float newR, float t) => l == r ? newL : newL + (newR - newL) * ((t - l) / (r - l));

	/// <inheritdoc cref="Remap(float, float, float, float, float)"/>
	[MethodImpl(INLINE)]
	public static int Remap (int l, int r, int newL, int newR, int t) => RemapUnclamped(l, r, newL, newR, t.ClampDisorder(l, r));

	/// <inheritdoc cref="Remap(float, float, float, float, float)"/>
	public static int RemapUnclamped (int l, int r, int newL, int newR, int t) {
		if (l == r) return newL;
		int deltaNew = newR - newL;
		int deltaT = t - l;
		int deltaR = r - l;
		try {
			return checked(newL + deltaNew * deltaT / deltaR);
		} catch {
			if (deltaNew.Abs() > deltaT.Abs()) {
				return newL + deltaNew / deltaR * deltaT;
			} else {
				return newL + deltaT / deltaR * deltaNew;
			}
		}
	}


	/// <summary>
	/// Remap "value" between ("from" and "to") into (0 and 1)
	/// </summary>
	[MethodImpl(INLINE)]
	public static float InverseLerp (float from, float to, float value) {
		if (from != to) {
			return ((value - from) / (to - from)).Clamp01();
		}
		return 0f;
	}

	/// <inheritdoc cref="InverseLerp(float, float, float)"/>
	[MethodImpl(INLINE)]
	public static float InverseLerpUnclamped (float from, float to, float value) {
		if (from != to) {
			return (value - from) / (to - from);
		}
		return 0f;
	}

	/// <summary>
	/// Make value "t" liner oscillate between 0 and "length"
	/// </summary>
	[MethodImpl(INLINE)]
	public static float PingPong (float t, float length) {
		t = Repeat(t, length * 2f);
		return length - (t - length).Abs();
	}

	/// <summary>
	/// Make value "t" liner loop between 0 and "length"
	/// </summary>
	[MethodImpl(INLINE)] public static float Repeat (float t, float length) => (t - (t / length).FloorToInt() * length).Clamp(0, length);

	/// <summary>
	/// Remap value "t" between (0 and 1) into ("a" and "b")
	/// </summary>
	[MethodImpl(INLINE)] public static float Lerp (float a, float b, float t) => a + (b - a) * t.Clamp01();

	/// <inheritdoc cref="Lerp(float, float, float)"/>
	[MethodImpl(INLINE)] public static float LerpUnclamped (float a, float b, float t) => a + (b - a) * t;

	/// <summary>
	/// Remap angle value "t" between (0 and 1) into ("a" and "b") 
	/// </summary>
	[MethodImpl(INLINE)]
	public static float LerpAngle (float a, float b, float t) {
		float delta = Repeat(b - a, 360);
		if (delta > 180)
			delta -= 360;
		return a + delta * Clamp01(t);
	}

	/// <inheritdoc cref="LerpAngle(float, float, float)"/>
	[MethodImpl(INLINE)]
	public static float LerpAngleUnclamped (float a, float b, float t) {
		float delta = Repeat(b - a, 360);
		if (delta > 180)
			delta -= 360;
		return a + delta * t;
	}


	/// <summary>
	/// Return the arctangent of (x,y)
	/// </summary>
	[MethodImpl(INLINE)] public static float Atan (float x, float y) => (float)Math.Atan2(y, x) * Rad2Deg;

	/// <summary>
	/// Return the smaller value 
	/// </summary>
	[MethodImpl(INLINE)] public static int Min (int a, int b) => (a < b) ? a : b;

	/// <summary>
	/// Return the smallest value
	/// </summary>
	[MethodImpl(INLINE)]
	public static int Min (int a, int b, int c) {
		int ab = (a < b) ? a : b;
		return (ab < c) ? ab : c;
	}

	/// <summary>
	/// Return the bigger value
	/// </summary>
	[MethodImpl(INLINE)] public static int Max (int a, int b) => (a > b) ? a : b;

	/// <summary>
	/// Return the biggest value
	/// </summary>
	[MethodImpl(INLINE)]
	public static int Max (int a, int b, int c) {
		int ab = (a > b) ? a : b;
		return (ab > c) ? ab : c;
	}

	/// <summary>
	/// Return the smaller value 
	/// </summary>
	[MethodImpl(INLINE)] public static float Min (float a, float b) => (a < b) ? a : b;

	/// <summary>
	/// Return the bigger value
	/// </summary>
	[MethodImpl(INLINE)] public static float Max (float a, float b) => (a > b) ? a : b;

	/// <summary>
	/// Return sin value for given angle in RAD. Use (degAngle * Util.Deg2Rad) to get the Rad angle.
	/// </summary>
	[MethodImpl(INLINE)] public static float Sin (float radAngle) => (float)Math.Sin(radAngle);

	/// <summary>
	/// Return cos value for given angle in RAD. Use (degAngle * Util.Deg2Rad) to get the Rad angle.
	/// </summary>
	[MethodImpl(INLINE)] public static float Cos (float radAngle) => (float)Math.Cos(radAngle);

	/// <summary>
	/// Return the absolute value
	/// </summary>
	[MethodImpl(INLINE)] public static int Abs (int value) => value > 0 ? value : -value;

	/// <summary>
	/// Return the absolute value
	/// </summary>
	[MethodImpl(INLINE)] public static float Abs (float value) => value > 0f ? value : -value;

	/// <summary>
	/// True if the given two float is almost same
	/// </summary>
	[MethodImpl(INLINE)] public static bool Approximately (float a, float b) => Abs(b - a) < Max(1E-06f * Max(Abs(a), Abs(b)), Epsilon * 8f);

	/// <summary>
	/// Limit value "a" between "min" and "max"
	/// </summary>
	[MethodImpl(INLINE)] public static int Clamp (int a, int min, int max) => a < min ? min : a > max ? max : a;

	/// <summary>
	/// Limit value "a" between "min" and "max"
	/// </summary>
	[MethodImpl(INLINE)] public static float Clamp (float a, float min, float max) => a < min ? min : a > max ? max : a;

	/// <summary>
	/// Limit value "a" between 0 and 1
	/// </summary>
	[MethodImpl(INLINE)] public static float Clamp01 (float value) => value < 0f ? 0f : value > 1f ? 1f : value;

	/// <summary>
	/// Return "f" to the power of "p"
	/// </summary>
	[MethodImpl(INLINE)] public static float Pow (float f, float p) => (float)Math.Pow(f, p);

	/// <summary>
	/// Return square root of "f"
	/// </summary>
	[MethodImpl(INLINE)] public static float Sqrt (float f) => (float)Math.Sqrt(f);

	/// <summary>
	/// Round a float to the nearest integer
	/// </summary>
	[MethodImpl(INLINE)] public static int RoundToInt (float value) => (int)Math.Round(value);

	/// <summary>
	/// Get the smallest intager that larger than "value"
	/// </summary>
	[MethodImpl(INLINE)] public static int CeilToInt (float value) => (int)Math.Ceiling(value);

	/// <summary>
	/// Get the largest intager that smaller than "value"
	/// </summary>
	[MethodImpl(INLINE)] public static int FloorToInt (float value) => (int)Math.Floor(value);


	/// <summary>
	/// Convert Hue-Saturation-Value into Red-Green-Blue.
	/// </summary>
	/// <param name="h">Range: 0-1</param>
	/// <param name="s">Range: 0-1</param>
	/// <param name="v">Range: 0-1</param>
	public static Color32 HsvToRgb (float h, float s, float v) => HsvToRgbF(h, s, v).ToColor32();

	/// <inheritdoc cref="HsvToRgb"/>
	public static ColorF HsvToRgbF (float h, float s, float v) {
		ColorF result = new(1f, 1f, 1f);
		if (s == 0f) {
			result.r = v;
			result.g = v;
			result.b = v;
		} else if (v == 0f) {
			result.r = 0f;
			result.g = 0f;
			result.b = 0f;
		} else {
			result.r = 0f;
			result.g = 0f;
			result.b = 0f;
			float num = h * 6f;
			int num2 = FloorToInt(num);
			float num3 = num - num2;
			float num4 = v * (1f - s);
			float num5 = v * (1f - s * num3);
			float num6 = v * (1f - s * (1f - num3));
			switch (num2) {
				case 0:
					result.r = v;
					result.g = num6;
					result.b = num4;
					break;
				case 1:
					result.r = num5;
					result.g = v;
					result.b = num4;
					break;
				case 2:
					result.r = num4;
					result.g = v;
					result.b = num6;
					break;
				case 3:
					result.r = num4;
					result.g = num5;
					result.b = v;
					break;
				case 4:
					result.r = num6;
					result.g = num4;
					result.b = v;
					break;
				case 5:
					result.r = v;
					result.g = num4;
					result.b = num5;
					break;
				case 6:
					result.r = v;
					result.g = num6;
					result.b = num4;
					break;
				case -1:
					result.r = v;
					result.g = num4;
					result.b = num5;
					break;
			}

			result.r = Clamp01(result.r);
			result.g = Clamp01(result.g);
			result.b = Clamp01(result.b);

		}

		return result;
	}


	/// <summary>
	/// Convert Red-Green-Blue into Hue-Saturation-Value.
	/// </summary>
	/// <param name="rgbColor"></param>
	/// <param name="h">Range: 0-1</param>
	/// <param name="s">Range: 0-1</param>
	/// <param name="v">Range: 0-1</param>
	public static void RgbToHsv (Color32 rgbColor, out float h, out float s, out float v) => RgbToHsvF(rgbColor.ToColorF(), out h, out s, out v);

	/// <inheritdoc cref="RgbToHsv"/>
	public static void RgbToHsvF (ColorF rgbColor, out float h, out float s, out float v) {
		float r = rgbColor.r;
		float g = rgbColor.g;
		float b = rgbColor.b;
		if (b > g && b > r) {
			Helper(4f, b, r, g, out h, out s, out v);
		} else if (g > r) {
			Helper(2f, g, b, r, out h, out s, out v);
		} else {
			Helper(0f, r, g, b, out h, out s, out v);
		}
		// Func
		static void Helper (float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V) {
			V = dominantcolor;
			if (V != 0f) {
				float num;
				num = ((!(colorone > colortwo)) ? colorone : colortwo);
				float num2 = V - num;
				if (num2 != 0f) {
					S = num2 / V;
					H = offset + (colorone - colortwo) / num2;
				} else {
					S = 0f;
					H = offset + (colorone - colortwo);
				}

				H /= 6f;
				if (H < 0f) {
					H += 1f;
				}
			} else {
				S = 0f;
				H = 0f;
			}
		}
	}


	/// <inheritdoc cref="RgbToGreyF"/>
	public static float RgbToGrey (Color32 rgbColor) => RgbToGreyF(rgbColor.ToColorF());
	/// <summary>
	/// Get grey scale of given color. (return between 0 and 1)
	/// </summary>
	public static float RgbToGreyF (ColorF rgbColor) => rgbColor.r * 0.299f + rgbColor.g * 0.587f + rgbColor.b * 0.114f;


	/// <summary>
	/// Draw color "top" onto "back" with blending overlay
	/// </summary>
	public static ColorF MergeColor_Overlay (ColorF top, ColorF back) {
		float alpha = top.a + back.a * (1f - top.a);
		return new ColorF(
			(top.r * top.a + back.r * back.a * (1f - top.a)) / alpha,
			(top.g * top.a + back.g * back.a * (1f - top.a)) / alpha,
			(top.b * top.a + back.b * back.a * (1f - top.a)) / alpha,
			alpha
		);
	}

	/// <summary>
	/// Draw color "top" onto "back" with blending lerp
	/// </summary>
	public static ColorF MergeColor_Lerp (ColorF top, ColorF back) {
		if (back.a.AlmostZero()) return top;
		float lerp = Min(top.a / back.a, 1f);
		return new ColorF(
			back.r + (top.r - back.r) * lerp,
			back.g + (top.g - back.g) * lerp,
			back.b + (top.b - back.b) * lerp,
			top.a + back.a * (1f - top.a)
		);
	}

	/// <summary>
	/// Draw color "top" onto "back" with no special blending
	/// </summary>
	public static ColorF MergeColor (ColorF top, ColorF back) => new(
		top.r * top.a + back.r * (1f - top.a),
		top.g * top.a + back.g * (1f - top.a),
		top.b * top.a + back.b * (1f - top.a),
		top.a + back.a * (1f - top.a)
	);

	/// <inheritdoc cref="MergeColor_Overlay(ColorF, ColorF)"/>
	public static Color32 MergeColor_Overlay (Color32 top, Color32 back) => MergeColor_Overlay(top.ToColorF(), back.ToColorF()).ToColor32();

	/// <inheritdoc cref="MergeColor_Lerp(ColorF, ColorF)"/>
	public static Color32 MergeColor_Lerp (Color32 top, Color32 back) => MergeColor_Lerp(top.ToColorF(), back.ToColorF()).ToColor32();

	/// <inheritdoc cref="MergeColor(ColorF, ColorF)"/>
	public static Color32 MergeColor (Color32 top, Color32 back) => MergeColor(top.ToColorF(), back.ToColorF()).ToColor32();


	/// <summary>
	/// Draw color "top" onto "back" with blending for sprite editing
	/// </summary>
	public static Color32 MergeColor_Editor (Color32 top, Color32 back) {
		if (back.a == 0) return top;
		const int AMOUNT = 10240;
		int lerp = top.a * AMOUNT / 255;
		return new Color32(
			(byte)(back.r + (top.r - back.r) * lerp / AMOUNT).Clamp(0, 255),
			(byte)(back.g + (top.g - back.g) * lerp / AMOUNT).Clamp(0, 255),
			(byte)(back.b + (top.b - back.b) * lerp / AMOUNT).Clamp(0, 255),
			(byte)(top.a + back.a * (255 - top.a) / 255).Clamp(0, 255)
		);
	}


	public static void WritePixelsToConsole (Color32[] pixels, int width) {

		int height = pixels.Length / width;

		for (int y = height - 1; y >= 0; y--) {
			System.Console.ResetColor();
			System.Console.WriteLine();
			for (int x = 0; x < width; x++) {
				var p = pixels[(y).Clamp(0, height - 1) * width + (x).Clamp(0, width - 1)];
				RgbToHsv(p, out float h, out float s, out float v);
				System.Console.BackgroundColor = (v * s < 0.2f) ?
					(v < 0.33f ? System.ConsoleColor.Black : v > 0.66f ? System.ConsoleColor.White : System.ConsoleColor.Gray) :
					(h < 0.08f ? (v > 0.5f ? System.ConsoleColor.Red : System.ConsoleColor.DarkRed) :
					h < 0.25f ? (v > 0.5f ? System.ConsoleColor.Yellow : System.ConsoleColor.DarkYellow) :
					h < 0.42f ? (v > 0.5f ? System.ConsoleColor.Green : System.ConsoleColor.DarkGreen) :
					h < 0.58f ? (v > 0.5f ? System.ConsoleColor.Cyan : System.ConsoleColor.DarkCyan) :
					h < 0.75f ? (v > 0.5f ? System.ConsoleColor.Blue : System.ConsoleColor.DarkBlue) :
					h < 0.92f ? (v > 0.5f ? System.ConsoleColor.Magenta : System.ConsoleColor.DarkMagenta) :
					(v > 0.6f ? System.ConsoleColor.Red : System.ConsoleColor.DarkRed));
				System.Console.Write(" ");
			}
		}
		System.Console.ResetColor();
		System.Console.WriteLine();
	}


	/// <summary>
	/// Get closest direction for vector (deltaX, deltaY)
	/// </summary>
	public static Direction8 GetDirection (int deltaX, int deltaY) {
		if (deltaX == 0 && deltaY == 0) return Direction8.Right;
		var normal = new Float2(deltaX, deltaY);
		float angle = Float2.SignedAngle(Float2.Up, normal).UMod(360f);
		return (Direction8)((angle / 22.5f).RoundToInt().UMod(8));
	}


	// Pointer
	public static unsafe byte ReadByte (ref byte* p, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		return ReadByte(ref p);
	}
	public static unsafe sbyte ReadSByte (ref byte* p, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		return ReadSByte(ref p);
	}
	public static unsafe bool ReadBool (ref byte* p, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		return ReadBool(ref p);
	}
	public static unsafe char ReadChar (ref byte* p, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		return ReadChar(ref p);
	}
	public static unsafe short ReadShort (ref byte* p, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		return ReadShort(ref p);
	}
	public static unsafe ushort ReadUShort (ref byte* p, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		return ReadUShort(ref p);
	}
	public static unsafe int ReadInt (ref byte* p, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		return ReadInt(ref p);
	}
	public static unsafe uint ReadUInt (ref byte* p, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		return ReadUInt(ref p);
	}
	public static unsafe float ReadFloat (ref byte* p, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		return ReadFloat(ref p);
	}
	public static unsafe long ReadLong (ref byte* p, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		return ReadLong(ref p);
	}
	public static unsafe ulong ReadULong (ref byte* p, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		return ReadULong(ref p);
	}
	public static unsafe double ReadDouble (ref byte* p, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		return ReadDouble(ref p);
	}
	public static unsafe byte[] ReadBytes (ref byte* p, int length, byte* end) {
		if (p > end - length + 1) throw new IndexOutOfRangeException();
		return ReadBytes(ref p, length);
	}

	public static unsafe byte ReadByte (ref byte* p) {
		byte result = *p;
		p++;
		return result;
	}
	public static unsafe sbyte ReadSByte (ref byte* p) {
		sbyte result = *(sbyte*)p;
		p++;
		return result;
	}
	public static unsafe bool ReadBool (ref byte* p) {
		bool result = *p == 1;
		p++;
		return result;
	}
	public static unsafe char ReadChar (ref byte* p) {
		char result = *(char*)p;
		p += 2;
		return result;
	}
	public static unsafe short ReadShort (ref byte* p) {
		short result = *(short*)p;
		p += 2;
		return result;
	}
	public static unsafe ushort ReadUShort (ref byte* p) {
		ushort result = *(ushort*)p;
		p += 2;
		return result;
	}
	public static unsafe int ReadInt (ref byte* p) {
		int result = *(int*)p;
		p += 4;
		return result;
	}
	public static unsafe uint ReadUInt (ref byte* p) {
		uint result = *(uint*)p;
		p += 4;
		return result;
	}
	public static unsafe float ReadFloat (ref byte* p) {
		float result = *(float*)p;
		p += 4;
		return result;
	}
	public static unsafe long ReadLong (ref byte* p) {
		long result = *(long*)p;
		p += 8;
		return result;
	}
	public static unsafe ulong ReadULong (ref byte* p) {
		ulong result = *(ulong*)p;
		p += 8;
		return result;
	}
	public static unsafe double ReadDouble (ref byte* p) {
		double result = *(double*)p;
		p += 8;
		return result;
	}
	public static unsafe byte[] ReadBytes (ref byte* p, int length) {
		var result = new byte[length];
		for (int i = 0; i < length; i++) {
			result[i] = *p;
			p++;
		}
		return result;
	}


	public static unsafe void Write (ref byte* p, byte value, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, sbyte value, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, bool value, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, char value, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, short value, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, ushort value, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, int value, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, uint value, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, float value, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, long value, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, ulong value, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, double value, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, byte[] bytes, int length, byte* end) {
		if (p > end - length + 1) throw new IndexOutOfRangeException();
		Write(ref p, bytes, length);
	}


	public static unsafe void Write (ref byte* p, byte value) {
		*p = value;
		p++;
	}
	public static unsafe void Write (ref byte* p, sbyte value) {
		var _p = (sbyte*)p;
		*_p = value;
		p++;
	}
	public static unsafe void Write (ref byte* p, bool value) {
		*p = (byte)(value ? 1 : 0);
		p++;
	}
	public static unsafe void Write (ref byte* p, char value) {
		var _p = (char*)p;
		*_p = value;
		p += 2;
	}
	public static unsafe void Write (ref byte* p, short value) {
		var _p = (short*)p;
		*_p = value;
		p += 2;
	}
	public static unsafe void Write (ref byte* p, ushort value) {
		var _p = (ushort*)p;
		*_p = value;
		p += 2;
	}
	public static unsafe void Write (ref byte* p, int value) {
		var _p = (int*)p;
		*_p = value;
		p += 4;
	}
	public static unsafe void Write (ref byte* p, uint value) {
		var _p = (uint*)p;
		*_p = value;
		p += 4;
	}
	public static unsafe void Write (ref byte* p, float value) {
		var _p = (float*)p;
		*_p = value;
		p += 4;
	}
	public static unsafe void Write (ref byte* p, long value) {
		var _p = (long*)p;
		*_p = value;
		p += 8;
	}
	public static unsafe void Write (ref byte* p, ulong value) {
		var _p = (ulong*)p;
		*_p = value;
		p += 8;
	}
	public static unsafe void Write (ref byte* p, double value) {
		var _p = (double*)p;
		*_p = value;
		p += 8;
	}
	public static unsafe void Write (ref byte* p, byte[] bytes, int length) {
		length = length < 0 ? bytes.Length : length;
		for (int i = 0; i < length; i++) {
			*p = bytes[i];
			p++;
		}
	}



}