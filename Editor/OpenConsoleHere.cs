﻿using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Karl.Editor
{
	sealed class OpenConsoleHere : UnityEditor.Editor
	{
		private const string CONSOLE_PATH = "C:\\Program Files\\ConEmu\\ConEmu64.exe";

		[MenuItem("Assets/Open Console Here", false, 0)]
		private static void MenuOpenConsole()
		{
			string dir = GetDirectory();
			System.Diagnostics.Process.Start(CONSOLE_PATH, string.Format("-Dir {0} -run {{Bash::msys2}} -new_console", dir));
		}

		static string GetDirectory()
		{
			Object o = Selection.activeObject;

			if (o != null)
			{
				string path = AssetDatabase.GetAssetPath(o.GetInstanceID());

				if (!string.IsNullOrEmpty(path))
				{
					if (Directory.Exists(path))
						return Path.GetFullPath(path);

					string res = Path.GetDirectoryName(path);

					if (!string.IsNullOrEmpty(res) && System.IO.Directory.Exists(res))
						return Path.GetFullPath(res);
				}
			}

			return Path.GetFullPath("Assets");
		}
	}
}