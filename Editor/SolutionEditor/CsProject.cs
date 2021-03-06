using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Unity.Karl.Editor
{
    [Serializable]
	class CsProject
	{
	    [SerializeField]
		string m_Path;

	    string m_Guid;

		public CsProject(string path)
		{
		    if(!File.Exists(path))
		        throw new ArgumentException("path is not a valid cs project");

			m_Path = path;
		}

	    public string path
	    {
	        get { return m_Path; }
	    }

	    public string guid
	    {
	        get
	        {
	            if (string.IsNullOrEmpty(m_Guid))
	                m_Guid = FindGuid();
	            return m_Guid;
	        }
	    }

		public void RemoveReferences(IEnumerable<string> patterns)
		{
			string csproj = File.ReadAllText(m_Path);

			var sr = new StringReader(csproj);
			var sb = new StringBuilder();

			while (sr.Peek() > -1)
			{
				var line = sr.ReadLine();

				var trim = line.Trim();

				// Remove HintPath references
				if (trim.StartsWith("<Reference Include=\""))
				{
					var name = trim.Replace("<Reference Include=\"", "").Replace("\">", "");

					// If a match is found, advance the reader beyond this reference
					if (patterns.Any(x => Regex.IsMatch(name, x)))
					{
						ReadToLine(sr, "</Reference>");
						continue;
					}
				}

				sb.AppendLine(line);
			}

			File.WriteAllText(m_Path, sb.ToString());
			sr.Dispose();
		}

		public void AddProjectReferences(IEnumerable<CsProject> projects)
		{
			var sb = new StringBuilder();

			sb.AppendLine("  <ItemGroup>");

			foreach(var prj in projects)
			{
				sb.AppendLine("    <ProjectReference Include=\"" + prj.path + "\">");
				sb.AppendLine("      <Project>{" + prj.guid + "}</Project>");
				sb.AppendLine("      <Name>" + Path.GetFileNameWithoutExtension(prj.path) + "</Name>");
				sb.AppendLine("    </ProjectReference>");

			}

			sb.AppendLine("  </ItemGroup>");

			AppendItemGroup(sb.ToString());
		}

		void AppendItemGroup(string itemGroup)
		{
			string csproj = File.ReadAllText(m_Path);

			var sr = new StringReader(csproj);
			var sb = new StringBuilder();

			while (sr.Peek() > -1)
			{
				var line = sr.ReadLine();
				var trim = line.Trim();

				if (trim.StartsWith("</Project>"))
					sb.AppendLine(itemGroup);

				sb.AppendLine(line);
			}

			File.WriteAllText(m_Path, sb.ToString());
			sr.Dispose();
		}

		static void ReadToLine(StringReader sr, string match)
		{
			while (sr.Peek() > -1)
			{
				var line = sr.ReadLine();
				var trim = line.Trim();
				if (trim.StartsWith(match))
					return;
			}
		}

	    string FindGuid()
	    {
	        string csproj = File.ReadAllText(m_Path);

	        using (var sr = new StringReader(csproj))
	        {
	            while (sr.Peek() > -1)
	            {
	                var line = sr.ReadLine().Trim();

	                if (line.StartsWith("<ProjectGuid>"))
	                {
	                    return line.Replace("<ProjectGuid>{", "").Replace("}</ProjectGuid>", "");
	                }
	            }
	        }

	        return "";
	    }
	}
}
