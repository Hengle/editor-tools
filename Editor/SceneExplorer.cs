using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.IMGUI.Controls;

class SceneExplorer : EditorWindow
{
	[MenuItem("Window/Scene Explorer")]
	static void MenuInitSceneExplorer()
	{
		GetWindow<SceneExplorer>(true, "Scene Explorer", true).Show();
	}

	void OnEnable()
	{
		m_SearchField = new SearchField();
		OnHierarchyChange();
	}

	class ObjectInfo
	{
		public string name;
		public bool expanded;
		public Object[] objects;

		public ObjectInfo()
		{
		}

		public ObjectInfo(string name, bool expanded, Object[] objects)
		{
			this.name = name;
			this.expanded = expanded;
			this.objects = objects;
		}
	}

	ObjectInfo[] m_Objects = new ObjectInfo[]
	{
		new ObjectInfo("textures: ", false, null),
		new ObjectInfo("audioclips: ", false, null),
		new ObjectInfo("meshes: ", false, null),
		new ObjectInfo("materials: ", false, null),
		new ObjectInfo("gameobjects: ", false, null),
		new ObjectInfo("components: ", false, null)
	};

	Vector2 m_Scroll = Vector2.zero;
	SearchField m_SearchField;
	[SerializeField]
	string m_SearchPattern;
	int m_Count = 0;

	void OnGUI()
	{
		if(EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
			return;

		if(GUILayout.Button("Clean Unused Assets"))
		{
			EditorUtility.UnloadUnusedAssetsImmediate();
			OnHierarchyChange();
		}

		Rect searchRect = GUILayoutUtility.GetRect(new GUIContent(""), EditorStyles.textField, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));

		EditorGUI.BeginChangeCheck();
		m_SearchPattern = m_SearchField.OnGUI(searchRect, m_SearchPattern);
		if(EditorGUI.EndChangeCheck())
			OnHierarchyChange();

		m_Scroll = GUILayout.BeginScrollView(m_Scroll);

		GUILayout.Label("All: " + m_Count, EditorStyles.boldLabel);

		GUILayout.Space(12);

		GUI.skin.label.richText = true;

		bool doReload = false;

		EditorGUI.BeginChangeCheck();

		for (int i = 0; i < m_Objects.Length; i++)
		{
			m_Objects[i].expanded = EditorGUILayout.Foldout(m_Objects[i].expanded, m_Objects[i].name + m_Objects[i].objects.Length);

			if(m_Objects[i].expanded)
				DrawObjectArray(m_Objects[i].objects);
		}

		if (EditorGUI.EndChangeCheck())
			doReload = true;

		GUILayout.EndScrollView();

		if (doReload)
		{
			OnHierarchyChange();
			EditorGUIUtility.ExitGUI();
		}
	}

	void DrawObjectArray(Object[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null)
				continue;

			GUILayout.BeginHorizontal();
			GUILayout.Label(string.Format("<color=#808080ff>\u2022</color> {0}", array[i] != null ? array[i].ToString() : "null"));
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("x", EditorStyles.miniButtonRight))
				DestroyImmediate(array[i]);
			GUILayout.EndHorizontal();
		}
	}

	void OnHierarchyChange()
	{
		Object[] textures = Resources.FindObjectsOfTypeAll(typeof(Texture));
		Object[] audioclips = Resources.FindObjectsOfTypeAll(typeof(AudioClip));
		Object[] meshes = Resources.FindObjectsOfTypeAll(typeof(Mesh));
		Object[] materials = Resources.FindObjectsOfTypeAll(typeof(Material));
		Object[] gameobjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
		Object[] components = Resources.FindObjectsOfTypeAll(typeof(Component));

		if (m_SearchPattern == null)
			m_SearchPattern = "";

		m_Objects[0].objects = textures.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[1].objects = audioclips.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[2].objects = meshes.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[3].objects = materials.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[4].objects = gameobjects.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();
		m_Objects[5].objects = components.Where(x => Regex.Match(x.name, m_SearchPattern).Success).ToArray();

		m_Count = m_Objects[0].objects.Length +
		          m_Objects[1].objects.Length +
		          m_Objects[2].objects.Length +
		          m_Objects[3].objects.Length +
		          m_Objects[4].objects.Length +
		          m_Objects[5].objects.Length;

		Repaint();
	}
}
