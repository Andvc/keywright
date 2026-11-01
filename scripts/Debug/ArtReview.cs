using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Keywright.Core;
using Keywright.Phases;
using Keywright.Ui;

namespace Keywright;

public partial class Main
{
    /// <summary>--art-review --shots=DIR: a short visual review of the round-2 integration, without playing or saving a run.</summary>
    async void ArtReview()
    {
        _demo = true;
        _saving = false;
        try
        {
            async Task Wait(double seconds) => await ToSignal(GetTree().CreateTimer(seconds, true), SceneTreeTimer.SignalName.Timeout);
            async Task Shot(string name)
            {
                await Wait(1.2);
                Moment(name);
                await Wait(0.3);
            }
            void Shop(int tier, string seller, int served)
            {
                State = GameState.NewRun(25025 + tier, Characters.Kit, 0, guided: false);
                State.SkipTo(tier, 2000);
                State.StartDay();
                State.KnowAll();
                State.Queue.Clear();
                State.Today.Passed = served;
                // Six customers total give stable first, middle and final thirds without simulating trades.
                for (int i = served; i < 6; i++)
                    State.Queue.Add(Market.Price(new Random(100 + i), Sellers.Get(seller),
                        new Chest(new[] { new Item(Category.Coins, ShopTiers.Typical(tier)), new Item(Category.Arms, ShopTiers.Typical(tier)) }, ShopTiers.Typical(tier)),
                        tier, 40, Trade.Fixed, "A sound chest, and a fair price. Have a listen before you decide."));
                State.NextCustomer();
                State.Display.Add(new Item(Category.Gems, Rarity.Rare));
                foreach (var song in tier == 4 ? Heartsongs.All : new[] { Heartsongs.Get(Heartsongs.Lullaby) })
                    State.Display.Add(Heartsongs.Make(song, Rarity.Rare));
                if (tier == 4)
                    foreach (string tool in new[] { "scales", "lamp", "seal", "magnet", "patrons", "block", "picks" }) State.Grant(tool);
                Hud.Visible = true;
                Hud.SetDay(State.Day, tier);
                Hud.SetGold(State.Gold, animate: false);
                Hud.SetDue(State);
                Hud.SetEvent(State);
                SwitchTo(new CounterPhase());
            }
            Shop(1, "pip", 0);
            await Shot("art_01_stall_morning_short");
            Shop(2, "garrick", 2);
            await Shot("art_02_thornwick_midday");
            Shop(3, "orme", 4);
            await Shot("art_03_crownhold_evening_tall");
            Shop(4, "tick", 4);
            await Wait(0.6);
            var counter = (CounterPhase)_phase;
            counter.SetMark(Category.Coins, 2, false);
            counter.SetMark(Category.Gems, 1, true);
            counter.SetHidden(true);
            counter.Mentor.Say("Listen first, then mark the slip. A good eye helps, but a Keywright trusts their ears.");
            await Shot("art_04_emporium_imp_tools");
            State.PeddlerStock.Clear();
            foreach (string key in new[] { "monocle", "watch", "ring" }) State.PeddlerStock.Add(Curios.Get(key));
            SwitchTo(new PeddlerPhase());
            await Shot("art_05_nim_morning");
            // In-memory fixture: show partial key progress beside week-two dues without writing a save.
            var guildReview = State.Save();
            guildReview.Day = 10;
            guildReview.BramPiece = true;
            guildReview.HoodPieces = 2;
            State = GameState.Load(guildReview);
            Hud.SetDay(State.Day, State.Tier);
            Hud.SetDue(State);
            SwitchTo(new DuesPhase());
            await Shot("art_06_vane_night");
            SwitchTo(new ScenePhase(new List<Line> {
                new(Story.Hood, "Some things are worth more than the Guild can count."),
                new(Story.Bram, "Then we had better keep them safe.") }, () => { }, "THE ALDRIC KEY", "keyhold"));
            await Shot("art_07_story_figures");
            var notebook = new NotebookView(State, NotebookView.RegularsPage, current: "orme") { ZIndex = 30 };
            AddChild(notebook);
            await Shot("art_08_notebook_heads");
            notebook.Close();
            await Wait(0.4);
            Shop(2, "garrick", 2);
            await Wait(0.4);
            for (int i = 0; i < 3; i++)
            {
                var chest = new Chest(new[] { new Item(Category.Coins, Rarity.Rare) }, Rarity.Rare, lockTier: LockTiers.All[i + 1]);
                var visit = Market.Price(new Random(160 + i), Sellers.Get("garrick"), chest, State.Tier, 40, Trade.Fixed, "A lock for the art review.");
                State.Opening = new Purchase(visit, new Appraisal(Rarity.Rare, 1, new[] { Category.Coins }));
                SwitchTo(new LockpickPhase());
                await Wait(0.4);
                ((LockpickPhase)_phase).Mentor.Hide();
                await Shot($"art_{9 + i:00}_lock_{i + 1}");
            }
            SwitchTo(new DayEndPhase());
            await Wait(0.4);
            ((DayEndPhase)_phase).Mentor.Hide();
            await Shot("art_12_upgrade_cards");
            GD.Print("Art integration review complete: twelve screenshots; controlled visual fixtures, no saves or simulated run.");
        }
        catch (Exception error)
        {
            GD.PrintErr(error);
            GetTree().Quit(1);
            return;
        }
        GetTree().Quit();
    }
}
