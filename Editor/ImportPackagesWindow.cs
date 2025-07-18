using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

public class ImportPackagesWindow : EditorWindow
{
    private class MyPackageInfo
    {
        public readonly string displayName;
        public readonly string packageName;
        public readonly string githubUrl;

        private AddRequest addRequest;
        private RemoveRequest removeRequest;
        private static ListRequest listRequest;

        public bool isLoaded { get; private set; }
        public bool isLoading { get; private set; }

        public MyPackageInfo(string displayName, string packageName, string githubUrl)
        {
            this.displayName = displayName;
            this.packageName = packageName;
            this.githubUrl = githubUrl;
            isLoaded = false;
            isLoading = false;
        }

        public void Add()
        {
            if (isLoading || isLoaded)
            {
                return;
            }

            addRequest = Client.Add(githubUrl);
            EditorApplication.update += AddProgress;
            isLoading = true;
        }

        public void Remove()
        {
            if (isLoading || !isLoaded)
            {
                return;
            }

            removeRequest = Client.Remove(packageName);
            EditorApplication.update += RemoveProgress;
            isLoading = true;
        }

        public void CheckIfAdded()
        {
            if (isLoading)
            {
                return;
            }

            isLoading = true;
            if (listRequest == null)
            {
                listRequest = Client.List();
            }

            EditorApplication.update += ListProgress;
        }

        private void AddProgress()
        {
            if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    isLoading = false;
                    isLoaded = true;
                }
                else
                {
                    Debug.LogError(addRequest.Error.message);
                }

                EditorApplication.update -= AddProgress;
            }
        }

        private void RemoveProgress()
        {
            if (removeRequest.IsCompleted)
            {
                if (removeRequest.Status == StatusCode.Success)
                {
                    isLoading = false;
                    isLoaded = false;
                }
                else
                {
                    Debug.LogError(removeRequest.Error.message);
                }

                EditorApplication.update -= RemoveProgress;
            }
        }

        private void ListProgress()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    isLoading = false;
                    isLoaded = listRequest.Result.Any((package) => package.name.Equals(packageName));
                }
                else
                {
                    Debug.LogError(listRequest.Error.message);
                }
            }
        }
    }

    private static MyPackageInfo[] packages =
    {
        new MyPackageInfo("Core Library", "com.srawls1.core", "https://github.com/srawls1/core.git"),
        new MyPackageInfo("Hitboxes", "com.srawls1.hitboxes", "https://github.com/srawls1/HitBoxes.git"),
        new MyPackageInfo("Character Controllers", "com.srawls1.character-controller", "https://github.com/srawls1/character-controllers.git")
    };

    private ListRequest listRequest;

    [MenuItem("Window/My Package Manager/Import Packages")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ImportPackagesWindow));
        for (int i = 0; i < packages.Length; ++i)
        {
            packages[i].CheckIfAdded();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < packages.Length; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(packages[i].displayName);
            if (packages[i].isLoading)
            {
                EditorGUILayout.LabelField("Loading...");
            }
            if (packages[i].isLoaded)
			{
                EditorGUILayout.LabelField("Loaded!");
			}

            if (GUILayout.Button("Add"))
            {
                packages[i].Add();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}
