using System.Threading.Tasks;
using Godot;
using Keywright.Ui;

namespace Keywright;

public partial class Main
{
    /// <summary>
    /// --font-sheet --shots=DIR: every language's name and a line in each script the fonts cover, at the sizes the UI
    /// uses, as the game draws them (Kenney Mini, and what it falls back to for what it hasn't got), to see that a
    /// translation's letters are there and sit well with the rest.
    /// </summary>
    async void FontSheet()
    {
        _demo = true;
        _saving = false;
        var layer = new CanvasLayer { Layer = 50 };
        AddChild(layer);
        layer.AddChild(new ColorRect { Color = Palette.Panel, Size = new Vector2(1280, 720) });
        string[] lines =
        {
            "English: Buy 12g, Pass. A jingle: coin.",
            "Deutsch: Kaufen für 12g. Münze, Größe, Ärger",
            "Français : Acheter 12 po. Épée, maître, cœur, sœur, ça !",
            "Polski: Kup za 12g. Złota moneta, ąęńśźż",
            "Русский: Купить за 12g. Монета",
            "日本語: 12gで買う。金貨、剣、宝石",
            "한국어: 12g에 사기. 동전, 검, 보석",
            "简体中文：用 12 金币买下。金币、剑、宝石、钥匙",
            "繁體中文：用 12 金幣買下。金幣、劍、寶石、鑰匙",
        };
        float y = 8;
        foreach (int size in new[] { 16, 24 })
        {
            foreach (string line in lines)
            {
                var label = UiKit.MakeLabel(line, size, Palette.Text).Place(16, y, 1240, size + 8);
                layer.AddChild(label);
                y += size + 10;
            }
            y += 8;
        }
        await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
        Moment("font_sheet");
        await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
        QuitGame();
    }
}
