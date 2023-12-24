using Godot;

public static class SceneHelper
{
    public const string botSelectionScene = "res://scenes/bot-selection.tscn";
    public const string chessScene = "res://scenes/chess-play.tscn";

    public static void SwitchScene(SceneTree activeScene, string newScenePath)
    {
        activeScene.ChangeSceneToFile(newScenePath);
    }
}
