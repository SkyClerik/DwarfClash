using UnityEditor;

class GameBuilder
{
     static void PerformBuild ()
     {   
         BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
         buildPlayerOptions.scenes = new[] { "Assets/Scenes/AuthScence.unity", "Assets/Scenes/Main menu.unity", "Assets/Scenes/Game 3D.unity" };
         buildPlayerOptions.target = BuildTarget.WebGL;
         buildPlayerOptions.locationPathName = "%%%WebGLDwarfClashBuild%%%"; // сюда вместо %%%WebGLDwarfClashBuild%%% скриптом сборки будет вставлен путь к директории, куда будут сложены результаты сборки под WebGL
         buildPlayerOptions.options = BuildOptions.None;
         BuildPipeline.BuildPlayer(buildPlayerOptions);
     }
}
