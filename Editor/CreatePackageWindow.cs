using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CreatePackageWindow : EditorWindow
{
    private string m_packageName = "MyPackage";
    private string m_displayName = "My Package";
    private string m_fullPackageName = "com.srawls1.MyPackage";
    private string m_author = "Spencer Rawls";
    private string m_description = "Fill me in!";

    private static readonly Regex SPACE_OR_PUNCTUATION = new Regex("[\\s\\p{P}]+");

    private static readonly string[] allowedTLDs =
    {
        "com",
        "org",
        "net",
        "cool" // just cause
    };

    #region Properties

    public string packageName
    {
        get { return m_packageName; }
        set
        {
            m_packageName = SPACE_OR_PUNCTUATION.Replace(value, string.Empty);
        }
    }

    public string displayName
    {
        get { return m_displayName; }
        set
        {
            m_displayName = value;
        }
    }

    public string fullPackageName
    {
        get { return m_fullPackageName; }
        set
        {
            List<string> parts = new List<string>(value.Split('.'));
            while (parts.Count < 3)
            {
                parts.Add("temp");
            }

            if (!allowedTLDs.Contains(parts[0]))
            {
                parts[0] = allowedTLDs[0];
            }

            for (int i = 1; i < parts.Count - 1; ++i)
            {
                parts[i] = SPACE_OR_PUNCTUATION.Replace(parts[i], string.Empty);
            }

            if (!string.Equals(parts[parts.Count - 1], packageName))
            {
                parts[parts.Count - 1] = packageName;
            }

            m_fullPackageName = string.Join(".", parts);
        }
    }

    public string githubUser
    {
        get
        {
            string[] parts = fullPackageName.Split('.');
            return parts[1];
        }
    }

    public string author
    {
        get { return m_author; }
        set
        {
            m_author = value;
        }
    }

    public string description
    {
        get { return m_description; }
        set
        {
            m_description = value;
        }
    }

    #endregion // Properties

    [MenuItem("Window/My Package Manager/Create Package")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CreatePackageWindow));
    }

    private void OnGUI()
    {
        packageName = EditorGUILayout.TextField("Name", packageName);
        displayName = EditorGUILayout.TextField("Display Name", displayName);
        fullPackageName = EditorGUILayout.TextField("Full Package Name", fullPackageName);
        author = EditorGUILayout.TextField("Author", author);
        EditorGUILayout.LabelField("Description");
        description = EditorGUILayout.TextArea(description);
        if (GUILayout.Button("Create Package"))
        {
            Create();
        }
    }

    private void Create()
    {
        Debug.Log(string.Format("Creating package structure with name={0}, display name={1}, fully qualified name={2}, and author={3}",
            packageName, displayName, fullPackageName, author));
        // We could add additional fullPackageName validation here
        if (AssetDatabase.IsValidFolder(Path.Combine("Assets", packageName)))
        {
            Debug.LogWarning(string.Format("Folder named {0} already exists; not doing anything.", packageName));
            return;
        }

        string guid = AssetDatabase.CreateFolder("Assets", packageName);
        string baseFolderPath = AssetDatabase.GUIDToAssetPath(guid);

        WritePackageManifest(baseFolderPath);
        WriteReadmeFile(baseFolderPath);
        WriteChangelogFile(baseFolderPath);
        WriteLicenseFile(baseFolderPath);
        WriteGitignore(baseFolderPath);
        CreateRuntimeDirectory(baseFolderPath);
        CreateTestsDirectory(baseFolderPath);
        CreateEditorDirectory(baseFolderPath);

        AssetDatabase.Refresh();
    }

    #region Package Manifest

    [Serializable]
    public class PackageManifest
    {
        public string name;
        public string version;
        public string author;
        public string description;
        public string displayName;
        public string unity;
        public Dependencies dependencies;

        public PackageManifest(string packageName, string author, string description, string displayName)
        {
            this.name = packageName;
            this.version = "1.0.0";
            this.author = author;
            this.description = description;
            this.displayName = displayName;
            this.unity = Application.unityVersion;
            this.dependencies = new Dependencies();
        }
    }

    [Serializable]
    public class Dependencies
    { }

    private void WritePackageManifest(string baseFolderPath)
    {
        object manifest = new PackageManifest(packageName, author, description, displayName);

        using (StreamWriter writer = File.CreateText(Path.Combine(baseFolderPath, "package.json")))
        {
            writer.Write(JsonUtility.ToJson(manifest, true));
        }
    }

    #endregion // Package Manifest

    #region Readme File

    private void WriteReadmeFile(string baseFolderPath)
    {
        using (StreamWriter writer = File.CreateText(Path.Combine(baseFolderPath, "README.md")))
        {
            writer.WriteLine(string.Format("# {0}", displayName));
            writer.WriteLine();
            writer.WriteLine(description);
            writer.WriteLine();
            writer.WriteLine("This package template was generated by [MyPackageManager](https://github.com/srawls1/MyPackageManager)");
            writer.WriteLine();
            writer.WriteLine("You can import this package into your project by adding the following line into the `dependencies` section of your project manifest:");
            writer.WriteLine();
            writer.WriteLine(string.Format("```\"{0}\": \"https://github.com/{1}/{2}\"```", fullPackageName, githubUser, packageName));
        }
    }

    #endregion // Readme File

    #region Changelog File

    private void WriteChangelogFile(string baseFolderPath)
    {
        using (StreamWriter writer = File.CreateText(Path.Combine(baseFolderPath, "CHANGELOG.md")))
        {
            writer.WriteLine("# Changelog");
            writer.WriteLine("All notable changes to this project will be documented in this file.");
            writer.WriteLine();
            writer.WriteLine("The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0),");
            writer.WriteLine("and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).");
            writer.WriteLine();
            writer.WriteLine("## [Unreleased]");
            writer.WriteLine();
            writer.WriteLine(string.Format("## [1.0.0] - {0:yyyy-MM-dd}", DateTime.Now));
            writer.WriteLine("Initial template - generated by [MyPackageManager](https://github.com/srawls1/MyPackageManager)");
        }
    }

    #endregion // Changelog File

    #region License File

    private void WriteLicenseFile(string baseFolderPath)
    {
        using (StreamWriter writer = File.CreateText(Path.Combine(baseFolderPath, "LICENSE.md")))
        {
            writer.WriteLine(string.Format("Copyright {0:yyyy} {1}", DateTime.Now, author));
            writer.WriteLine("Creative Commons Attribution 4.0 International License (CC BY 4.0)");
            writer.WriteLine("https://creativecommons.org/licenses/by/4.0");
            writer.WriteLine("Unless expressly provided otherwise, the Software under this license is " +
                "made available strictly on an \"AS IS\" BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. " +
                "Please review the license for details on these and other terms and conditions.");
        }
    }

    #endregion // License File

    private void WriteGitignore(string baseFolderPath)
    {
        Debug.Log("Writing gitignore file not implemented yet");
    }

    private void CreateRuntimeDirectory(string baseFolderPath)
    {
        string folder = CreateDirectory(baseFolderPath, "Runtime");
        CreateAssemblyDefinition(folder, string.Format("{0}.asmdef", fullPackageName));
    }

    private void CreateTestsDirectory(string baseFolderPath)
    {
        string folder = CreateDirectory(baseFolderPath, "Tests");
        string editorFolder = CreateDirectory(folder, "Editor");
        string runtimeFolder = CreateDirectory(folder, "Runtime");
        CreateAssemblyDefinition(editorFolder, string.Format("{0}.Editor.Tests.asmdef", fullPackageName));
        CreateAssemblyDefinition(runtimeFolder, string.Format("{0}.Tests.asmdef", fullPackageName));
    }

    private void CreateEditorDirectory(string baseFolderPath)
    {
        string folder = CreateDirectory(baseFolderPath, "Editor");
        CreateAssemblyDefinition(folder, string.Format("{0}.Editor.asmdef", fullPackageName));
    }

    private string CreateDirectory(string parent, string name)
    {
        string guid = AssetDatabase.CreateFolder(parent, name);
        return AssetDatabase.GUIDToAssetPath(guid);
    }

    //[Serializable]
    //public class AssemblyDefinition
    //{

    //}

    private void CreateAssemblyDefinition(string folder, string assemblyName)
    {
       
    }
}
