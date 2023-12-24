using Godot;

public partial class GameUI : Node
{
    [Export] Label botNameLabel;
    [Export] Button exitButton;
    //[Export] PackedScene selectionScene;


    public override void _Ready()
    {
        exitButton.Pressed += Exit;
        botNameLabel.Text = $"Opponent: {BotSettings.ActiveBotName}";
    }

    void Exit()
    {
        SceneHelper.SwitchScene(GetTree(), SceneHelper.botSelectionScene);
    }


}
