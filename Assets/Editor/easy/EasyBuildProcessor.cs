using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using easy;

public class EasyBuildProcessor : IPostprocessBuildWithReport
{
    private static readonly string FRAMEWORKS_BASE_PATH = "Plugins/iOS";

    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS)
        {
#if UNITY_EDITOR_OSX
            string projectPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projectPath));
            string targetId = proj.TargetGuidByName("Unity-iPhone");
            AddDynamicFrameworkToEmbed(report.summary.outputPath, proj, targetId, new DirectoryInfo(Path.Combine(Application.dataPath, FRAMEWORKS_BASE_PATH)).GetDirectories());
            proj.SetBuildProperty(targetId, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
            proj.WriteToFile(projectPath);
#endif
        }
    }

#if UNITY_EDITOR_OSX
    private void AddDynamicFrameworkToEmbed(string pathToBuiltProject, PBXProject proj, string targetId, DirectoryInfo[] dirs)
    {
        if (dirs != null)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                if (dir.Name.Contains(".framework"))
                {
                    var output = string.Format("file {0}/Frameworks/{1}/{2}", pathToBuiltProject, dir.Name, dir.Name.Split('.')[0]).Bash();
                    if (output.Contains("dynamically"))
                    {
                        PBXProjectExtensions.AddFileToEmbedFrameworks(proj, targetId, proj.FindFileGuidByProjectPath("Frameworks/" + dir.Name));
                    }
                }
                else
                {
                    AddDynamicFrameworkToEmbed(pathToBuiltProject, proj, targetId, new DirectoryInfo(dir.FullName).GetDirectories());
                }
            }
        }
    }
#endif
}
