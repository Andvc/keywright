using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Keywright.Core;
using Keywright.Phases;
using Keywright.Ui;

namespace Keywright;

public partial class Main
{
    /// <summary>--density-review --shots=DIR: every figure, shop/light combination, story backdrop and portrait UI.
    /// Uses in-memory fixtures and screenshots only; no trades, progression or saved game.</summary>
    async void DensityReview()
    {
        _demo = true;
        _saving = false;
        bool smoke = Arg(OS.GetCmdlineUserArgs(), "--density-review") == "smoke";
        var shots = new List<string>();
        try
        {
            if (_shotsDir == null) throw new InvalidOperationException("--density-review requires --shots=DIR");
            if (DisplayServer.GetName() == "headless") throw new InvalidOperationException("Density review needs a rendering window");
            GetWindow().Mode = Window.ModeEnum.Windowed;
            GetWindow().Borderless = true;
            GetWindow().Size = new Vector2I(2560, 1440);
            Settings.Shake = 0;
            Settings.Outlines = false;
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);
            async Task Wait(double seconds) => await ToSignal(GetTree().CreateTimer(seconds, true), SceneTreeTimer.SignalName.Timeout);
            // A phase is added once the fade is out; with a slow renderer one frame can outlast any fixed wait.
            async Task Show(Phase next)
            {
                SwitchTo(next);
                while (!next.IsNodeReady()) await Wait(0.05);
            }
            async Task Shot(string name)
            {
                await Wait(0.8);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                string path = $"{_shotsDir}/{name}.png";
                if (FileAccess.FileExists(path)) throw new InvalidOperationException($"Use a new review directory: {path} already exists");
                using var image = GetViewport().GetTexture().GetImage();
                if (image.GetSize() != new Vector2I(2560, 1440))
                    throw new InvalidOperationException($"Expected 2560x1440 review capture, got {image.GetSize()}");
                if (image.SavePng(path) != Error.Ok) throw new InvalidOperationException($"Cannot write {path}");
                shots.Add(name + ".png");
                GD.Print($"Density review {shots.Count}: {name}");
            }
            void Prepare(int tier, string seller = "hob", int served = 2)
            {
                State = GameState.NewRun(31000 + tier, Characters.Kit, 0, guided: false);
                State.SkipTo(tier, 2000);
                State.StartDay();
                State.KnowAll();
                State.Queue.Clear();
                State.Today.Passed = served;
                for (int i = served; i < 6; i++)
                    State.Queue.Add(Market.Price(new Random(100 + i), Sellers.Get(seller),
                        new Chest(new[] { new Item(Category.Coins, ShopTiers.Typical(tier)),
                            new Item(Category.Arms, ShopTiers.Typical(tier)) }, ShopTiers.Typical(tier)),
                        tier, 40, Trade.Fixed, "A sound chest, and a fair price. Have a listen before you decide."));
                State.NextCustomer();
                State.Display.Add(new Item(Category.Gems, Rarity.Rare));
                State.Display.Add(Heartsongs.Make(Heartsongs.Get(Heartsongs.Lullaby), Rarity.Rare));
                foreach (string tool in new[] { "scales", "lamp", "seal", "magnet", "patrons", "block", "picks" }) State.Grant(tool);
                Hud.Visible = true;
                Hud.SetDay(State.Day, tier);
                Hud.SetGold(State.Gold, animate: false);
                Hud.SetDue(State);
                Hud.SetEvent(State);
            }

            var figures = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                FileAccess.GetFileAsString("res://assets/figures/layout.json"));
            Prepare(4);
            foreach (string key in figures.Keys.Order().Take(smoke ? 1 : figures.Count))
            {
                var seller = Sellers.All.FirstOrDefault(s => s.Key == key) ?? new SellerDef { Key = key, Name = Story.Name(key) };
                var phase = new Phase();
                phase.AddChild(new ShopView(4, 1f, ShopHour.Midday, State));
                // Keep the review caption away from tall visitors' faces; the shop captures below use the normal UI.
                var person = new SellerView(seller) { BubbleAt = new Vector2(500, 410) };
                phase.AddChild(person);
                var panel = UiKit.MakePanel(Palette.PanelBorder).Place(500, 90, 720, 300);
                panel.AddChild(UiKit.MakeLabel(key, 32, Palette.Gold).Place(24, 18, 660, 40));
                panel.AddChild(UiKit.MakeLabel("Portrait / queue", 20, Palette.Text).Place(24, 72, 660, 30));
                panel.AddChild(UiKit.MakeSprite($"res://assets/sprites/sellers/{key}.png", new Vector2(120, 176), 1));
                panel.AddChild(UiKit.MakeSprite($"res://assets/sprites/sellers/faces/{key}.png", new Vector2(300, 176), 1));
                phase.AddChild(panel);
                await Show(phase);
                await Wait(0.25);
                person.Say("A closer look at the visitor and their portraits.");
                await Shot($"figure_{key}");
            }

            string[] visitors = { "tam", "hob", "orrin", "vexis" };
            for (int tier = 1; tier <= (smoke ? 1 : 4); tier++)
                for (int hour = 0; hour < (smoke ? 1 : 4); hour++)
                {
                    Prepare(tier, visitors[tier - 1], Math.Min(hour, 2) * 2);
                    await Show(hour == 3 ? new DuesPhase() : new CounterPhase());
                    await Shot($"shop_{tier}_{((ShopHour)hour).ToString().ToLowerInvariant()}");
                }

            Prepare(4);
            foreach (string place in new[] { "bram", "guild", "keyhold", "vault" }.Take(smoke ? 1 : 4))
            {
                await Show(new ScenePhase(new List<Line> {
                    new(Story.Hood, "Some things are worth more than the Guild can count."),
                    new(Story.Bram, "Then we had better keep them safe.") }, () => { }, "THE ALDRIC KEY", place));
                await Shot($"story_{place}");
            }
            await Show(new CounterPhase());
            await Wait(0.6);
            for (int tier = 1; tier <= (smoke ? 1 : 4); tier++)
            {
                var notebook = new NotebookView(State, NotebookView.RegularsPage,
                    current: Sellers.All.First(s => s.Tier == tier).Key) { ZIndex = 30 };
                AddChild(notebook);
                await Shot($"notebook_{tier}");
                notebook.Close();
                await Wait(0.25);
            }
            for (int i = 0; i < (smoke ? 1 : 3); i++)
            {
                var chest = new Chest(new[] { new Item(Category.Coins, Rarity.Rare) }, Rarity.Rare, lockTier: LockTiers.All[i + 1]);
                var visit = Market.Price(new Random(160 + i), Sellers.Get("hob"), chest, State.Tier, 40, Trade.Fixed, "A lock for the art review.");
                State.Opening = new Purchase(visit, new Appraisal(Rarity.Rare, 1, new[] { Category.Coins }));
                await Show(new LockpickPhase());
                await Shot($"lock_{i + 1}");
            }
            await Show(new DayEndPhase());
            await Shot("upgrade_cards");
            using var report = FileAccess.Open($"{_shotsDir}/capture-report.json", FileAccess.ModeFlags.Write);
            report.StoreString(JsonSerializer.Serialize(new { width = 2560, height = 1440, saves = false, screenshots = shots },
                new JsonSerializerOptions { WriteIndented = true }));
            GD.Print($"Density integration review complete: {shots.Count} screenshots; no saved game or simulated run.");
        }
        catch (Exception error)
        {
            GD.PrintErr(error);
            QuitGame(1);
            return;
        }
        QuitGame();
    }
}
