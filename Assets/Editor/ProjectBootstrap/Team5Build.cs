using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Team5.qPCR.Editor
{
    public static class Team5Build
    {
        private const string ScenePath = "Assets/Team5/Scenes/Team5_qPCR_LoadAndRun.unity";
        private const string OpenXrSettingsKey = "com.unity.xr.openxr.settings4";
        private const string AndroidOpenXrLoaderType = "UnityEngine.XR.OpenXR.OpenXRLoader";

        public static void BuildWindows()
        {
            var output = GetCommandLineValue("-buildOutput") ??
                         Path.Combine(GetDeliverableRoot(), "Builds", "Windows", "Team5-qPCR.exe");
            PrepareDesktopOpenXrSettings();
            EditorBuildSettings.TryGetConfigObject(OpenXrSettingsKey, out UnityEngine.Object openXrPackageSettings);
            OverrideOpenXrSettingsLocator(openXrPackageSettings, disableRuntimeSettings: true);
            try
            {
                BuildPlayer(output, BuildTarget.StandaloneWindows64, BuildOptions.None);
            }
            finally
            {
                OverrideOpenXrSettingsLocator(openXrPackageSettings, disableRuntimeSettings: false);
            }
        }

        public static void BuildAndroid()
        {
            var output = GetCommandLineValue("-buildOutput") ??
                         Path.Combine(GetDeliverableRoot(), "Builds", "Android", "Team5-qPCR.apk");
            PrepareAndroidOpenXrSettings();
            EditorUserBuildSettings.buildAppBundle = false;
            BuildPlayer(output, BuildTarget.Android, BuildOptions.None);
        }

        private static void BuildPlayer(string output, BuildTarget target, BuildOptions options)
        {
            if (!File.Exists(Path.GetFullPath(ScenePath)))
            {
                throw new BuildFailedException($"Required scene does not exist: {ScenePath}");
            }

            output = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = target,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"{target} build failed: {report.summary.result}, {report.summary.totalErrors} errors.");
            }

            Debug.Log($"[Team 5 Build] {target} build completed: {output} ({report.summary.totalSize} bytes)");
        }

        private static void PrepareDesktopOpenXrSettings()
        {
            // The desktop MVP deliberately does not initialize an XR loader. Keeping the
            // Standalone feature array empty also prevents Android-only Meta feature data
            // from being deserialized by the Windows player. Android settings stay intact.
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
            if (settings == null)
            {
                return;
            }

            var serializedSettings = new SerializedObject(settings);
            var features = serializedSettings.FindProperty("features");
            if (features != null)
            {
                features.arraySize = 0;
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static void PrepareAndroidOpenXrSettings()
        {
            // XR Simulation moves its editor-only resources through this temporary folder
            // during a player build. Remove a copy left by an interrupted build so the
            // package can perform that move and restore cleanly.
            if (AssetDatabase.IsValidFolder("Assets/XR/Temp") &&
                !AssetDatabase.DeleteAsset("Assets/XR/Temp"))
            {
                throw new BuildFailedException("Could not clear the stale XR Simulation build folder.");
            }

            var perTargetSettings = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(
                "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
            if (perTargetSettings == null)
            {
                throw new BuildFailedException("XR General Settings asset is missing.");
            }

            if (!perTargetSettings.HasSettingsForBuildTarget(BuildTargetGroup.Android))
            {
                perTargetSettings.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
            }

            if (!perTargetSettings.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
            {
                perTargetSettings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            }

            var generalSettings = perTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            var managerSettings = perTargetSettings.ManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            if (generalSettings == null || managerSettings == null ||
                !XRPackageMetadataStore.AssignLoader(
                    managerSettings,
                    AndroidOpenXrLoaderType,
                    BuildTargetGroup.Android))
            {
                throw new BuildFailedException("Could not assign the OpenXR loader for Android.");
            }

            generalSettings.InitManagerOnStart = true;
            managerSettings.automaticLoading = true;
            managerSettings.automaticRunning = true;
            EditorUtility.SetDirty(generalSettings);
            EditorUtility.SetDirty(managerSettings);

            var openXrSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (openXrSettings == null)
            {
                throw new BuildFailedException("Android OpenXR settings are missing.");
            }

            openXrSettings.latencyOptimization = OpenXRSettings.LatencyOptimization.PrioritizeInputPolling;

            EnableOpenXrFeature(openXrSettings,
                "UnityEngine.XR.OpenXR.Features.MetaQuestSupport.MetaQuestFeature");
            EnableOpenXrFeature(openXrSettings,
                "UnityEngine.XR.OpenXR.Features.Interactions.OculusTouchControllerProfile");
            EnableOpenXrFeature(openXrSettings,
                "UnityEngine.XR.OpenXR.Features.Interactions.MetaQuestTouchPlusControllerProfile");
            EnableOpenXrFeature(openXrSettings,
                "UnityEngine.XR.OpenXR.Features.CompositionLayers.OpenXRCompositionLayersFeature");

            EditorUtility.SetDirty(openXrSettings);
            AssetDatabase.SaveAssets();
        }

        private static void EnableOpenXrFeature(OpenXRSettings settings, string featureTypeName)
        {
            var feature = settings.GetFeatures()
                .FirstOrDefault(candidate => candidate != null && candidate.GetType().FullName == featureTypeName);
            if (feature == null)
            {
                throw new BuildFailedException($"Required Android OpenXR feature is missing: {featureTypeName}");
            }

            feature.enabled = true;
            EditorUtility.SetDirty(feature);
        }

        private static void OverrideOpenXrSettingsLocator(UnityEngine.Object packageSettings, bool disableRuntimeSettings)
        {
            if (packageSettings == null)
            {
                return;
            }

            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var methodName = disableRuntimeSettings
                ? "UnityEngine.XR.OpenXR.IPackageSettings2.OverrideSettingsLocatorFunc"
                : "UnityEngine.XR.OpenXR.IPackageSettings2.RestoreDefaultSettingsLocatorFunc";
            var method = packageSettings.GetType().GetMethod(methodName, flags);
            if (method == null)
            {
                throw new BuildFailedException("Could not configure OpenXR settings isolation for the desktop build.");
            }

            if (disableRuntimeSettings)
            {
                Func<BuildTargetGroup, OpenXRSettings> noDesktopSettings = _ => null;
                method.Invoke(packageSettings, new object[] { noDesktopSettings });
            }
            else
            {
                method.Invoke(packageSettings, null);
            }
        }

        private static string GetDeliverableRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "deliverables", "Team5-qPCR"));
        }

        private static string GetCommandLineValue(string key)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(args[index], key, StringComparison.OrdinalIgnoreCase))
                {
                    return args[index + 1];
                }
            }

            return null;
        }
    }
}
