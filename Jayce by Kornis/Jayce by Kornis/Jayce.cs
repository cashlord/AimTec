using System.Net.Configuration;
using System.Resources;
using System.Security.Authentication.ExtendedProtection;

namespace Jayce_By_Kornis
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Aimtec;
    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Orbwalking;
    using Aimtec.SDK.TargetSelector;
    using Aimtec.SDK.Util.Cache;
    using Aimtec.SDK.Prediction.Skillshots;
    using Aimtec.SDK.Util;


    using Spell = Aimtec.SDK.Spell;

    internal class Jayce
    {
        public static Menu Menu = new Menu("Jayce By Kornis", "Jayce by Kornis", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        public static Spell Q2, W2, E2, R, Q, W, E, QE, Flash;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 285);
            E = new Spell(SpellSlot.E, 385);
            Q2 = new Spell(SpellSlot.Q, 1050);
            QE = new Spell(SpellSlot.Q, 1550);
            W2 = new Spell(SpellSlot.W, 600);
            E2 = new Spell(SpellSlot.E, 660);
            R = new Spell(SpellSlot.R, 0);
            Q2.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.Line, false, HitChance.None);
            QE.SetSkillshot(0.25f, 70f, 2000, true, SkillshotType.Line, false, HitChance.None);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner1, 425);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner2, 425);

        }

        public Jayce()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            var QSet = new Menu("qset", "Q Settings");
            {
                QSet.Add(new MenuBool("melee", "Use Q Melee"));
                QSet.Add(new MenuBool("ranged", "Use Q Ranged"));
                QSet.Add(new MenuBool("qerange", "QE Only if out of Q Range", false));
            }
            var WSet = new Menu("wset", "W Settings");
            {
                WSet.Add(new MenuBool("melee", "Use W Melee"));
                WSet.Add(new MenuBool("ranged", "Use W Ranged"));
                WSet.Add(new MenuBool("wait", "Wait for Ranged W to End"));
            }
            var ESet = new Menu("eset", "E Settings");
            {
                ESet.Add(new MenuBool("melee", "Use E Melee"));
                ESet.Add(new MenuList("emode", "E Mode",
                    new[] {"Use E Melee Only for KS", "Use E Melee only if Killable with Combo", "Always"}, 2));
                ESet.Add(new MenuBool("eq", "Use E on Q"));
            }
            ComboMenu.Add(new MenuKeyBind("key", "QE to Mouse:", KeyCode.T, KeybindType.Press));

            Menu.Add(ComboMenu);
            ComboMenu.Add(QSet);
            ComboMenu.Add(WSet);
            ComboMenu.Add(ESet);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                HarassMenu.Add(new MenuBool("ranged", "Use Q Ranged"));
                HarassMenu.Add(new MenuBool("qe", "Use Q E"));

            }
            Menu.Add(HarassMenu);
            var FarmMenu = new Menu("farming", "Farming");
            var LaneClear = new Menu("lane", "Lane Clear");
            {
                LaneClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                LaneClear.Add(new MenuBool("useq", "Use Q Melee"));
                LaneClear.Add(new MenuSlider("hitq", "^- if Hits", 3, 1, 6));
                LaneClear.Add(new MenuBool("usew", "Use W Melee"));
                LaneClear.Add(new MenuSlider("hitw", "^- if in W Range", 3, 1, 6));
            }
            var JungleClear = new Menu("jungle", "Jungle Clear");
            {
                JungleClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                JungleClear.Add(new MenuBool("qranged", "Use Q Ranged"));
                JungleClear.Add(new MenuBool("wranged", "Use W Ranged"));
                JungleClear.Add(new MenuBool("qmelee", "Use Q Melee"));
                JungleClear.Add(new MenuBool("wmelee", "Use W Melee"));
                JungleClear.Add(new MenuBool("emelee", "Use E Melee"));
            }
            Menu.Add(FarmMenu);
            FarmMenu.Add(LaneClear);
            FarmMenu.Add(JungleClear);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("qranged", "Killsteal with Q Ranged"));
                KSMenu.Add(new MenuBool("qmelee", "Killsteal with Q Melee"));
                KSMenu.Add(new MenuBool("qe", "Killsteal with QE"));
                KSMenu.Add(new MenuBool("emelee", "Killsteal with E Melee"));
            }
            Menu.Add(KSMenu);
            var inscmenu = new Menu("insec", "Insec");
            {
                inscmenu.Add(new MenuBool("gapq", "Use Q to Gap"));
                inscmenu.Add(new MenuBool("procq", "Wait for Q Proc. Damage"));
                inscmenu.Add(new MenuKeyBind("key", "Insec Key:", KeyCode.Z, KeybindType.Press));

            }
            Menu.Add(inscmenu);

            var miscmenu = new Menu("misc", "Misc.");
            {

                miscmenu.Add(new MenuBool("autoe", "Auto E on Manual Q"));
            }
            Menu.Add(miscmenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("qmelee", "Draw Q Melee"));
                DrawMenu.Add(new MenuBool("wmelee", "Draw W Melee"));
                DrawMenu.Add(new MenuBool("emelee", "Draw E Melee"));
                DrawMenu.Add(new MenuBool("qranged", "Draw Q Ranged"));
                DrawMenu.Add(new MenuBool("eranged", "Draw E Ranged"));
                DrawMenu.Add(new MenuBool("qe", "Draw Q+E Range"));
                DrawMenu.Add(new MenuBool("drawdamage", "Draw Damage"));
                DrawMenu.Add(new MenuBool("drawtime", "Draw Q Timer"));
            }
            Menu.Add(DrawMenu);
            var FleeMenu = new Menu("flee", "Flee");
            {
                FleeMenu.Add(new MenuBool("usee", "Use E to Flee"));
                FleeMenu.Add(new MenuBool("user", "Use R to Flee"));
                FleeMenu.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.G, KeybindType.Press));
            }
            Menu.Add(FleeMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
           Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;

            LoadSpells();
            Console.WriteLine("Jayce by Kornis - Loaded");
        }

        public static readonly List<string> SpecialChampions = new List<string> {"Annie", "Jhin"};
        private float timer;
        private int helalmoney;

        public void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender.IsMe)
            {
                if (Orbwalker.Mode.Equals(OrbwalkingMode.Combo) || Orbwalker.Mode.Equals(OrbwalkingMode.Mixed) ||
                    Menu["combo"]["key"].Enabled)

                {
                    return;
                }
                    var spellName = args.SpellData.Name;
                if (spellName.Equals("JayceShockBlast") && Menu["misc"]["autoe"].Enabled)
                {
                    DelayAction.Queue(200, () =>
                    {
                        E2.Cast(Player.ServerPosition.Extend(args.End, 150));
                    });

                }
            }
        }

        public static int SxOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 1 : 10;
        }

        public static int SyOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 3 : 20;
        }

        static double GetQ(Obj_AI_Base target)
        {
            double meow = 0;
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 1)
            {
                meow = 70;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 2)
            {
                meow = 120;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 3)
            {
                meow = 170;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 4)
            {
                meow = 220;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 5)
            {
                meow = 270;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 6)
            {
                meow = 320;
            }
            double calc = (Player.TotalAttackDamage - Player.BaseAttackDamage) * 1.2;
            double full = calc + meow;
            double damage = Player.CalculateDamage(target, DamageType.Physical, full);
            return damage;

        }

        static double GetEQ(Obj_AI_Base target)
        {
            double meow = 0;
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 1)
            {
                meow = 98;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 2)
            {
                meow = 168;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 3)
            {
                meow = 238;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 4)
            {
                meow = 308;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 5)
            {
                meow = 374;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 6)
            {
                meow = 448;
            }
            double calc = (Player.TotalAttackDamage - Player.BaseAttackDamage) * 1.68;
            double full = calc + meow;
            double damage = Player.CalculateDamage(target, DamageType.Physical, full);
            return damage;

        }

        private void SemiQ()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            if (Player.HasBuff("jaycestancehammer"))
            {
                R.Cast();
            }
            if (!Player.HasBuff("jaycestancehammer"))
            {
                if (E2.Cast(Player.ServerPosition.Extend(Game.CursorPos, 150)))
                {
                    {
                        QE.Cast(Game.CursorPos);

                    }
                }
            }
        }
        private void Render_OnPresent()
        {


            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int) maybeworks.X;
            var yaOffset = (int) maybeworks.Y;
            if (Menu["drawings"]["drawtime"].Enabled)
            {
                if (!Player.HasBuff("jaycestancehammer") && timer - Game.ClockTime > 0)
                {
                    Render.Text(xaOffset - 50, yaOffset + 10, Color.HotPink, "Q Melee CD: " + (timer - Game.ClockTime),
                        RenderTextFlags.VerticalCenter);
                }

            }
            if (Player.HasBuff("jaycestancehammer"))
            {
                if (Menu["drawings"]["qmelee"].Enabled)
                {
                    Render.Circle(Player.Position, Q.Range, 40, Color.CornflowerBlue);
                }
                if (Menu["drawings"]["wmelee"].Enabled)
                {
                    Render.Circle(Player.Position, W.Range, 40, Color.Crimson);
                }
                if (Menu["drawings"]["emelee"].Enabled)
                {
                    Render.Circle(Player.Position, E.Range, 40, Color.CornflowerBlue);
                }

            }
            if (!Player.HasBuff("jaycestancehammer"))
            {
                if (Menu["drawings"]["qranged"].Enabled)
                {
                    Render.Circle(Player.Position, Q2.Range, 40, Color.Crimson);
                }
                if (Menu["drawings"]["eranged"].Enabled)
                {
                    Render.Circle(Player.Position, E2.Range, 40, Color.Crimson);
                }
                if (Menu["drawings"]["qe"].Enabled)
                {
                    Render.Circle(Player.Position, QE.Range, 40, Color.Crimson);
                }
            }
            if (Menu["drawings"]["drawdamage"].Enabled)
            {
                if (Player.HasBuff("jaycestancehammer"))
                {
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(Q2.Range * 2))
                        .ToList()
                        .ForEach(
                            unit =>
                            {

                                var heroUnit = unit as Obj_AI_Hero;
                                int width = 103;
                                int height = 8;
                                int xOffset = SxOffset(heroUnit);
                                int yOffset = SyOffset(heroUnit);
                                var barPos = unit.FloatingHealthBarPosition;
                                barPos.X += xOffset;
                                barPos.Y += yOffset;
                                var drawEndXPos = barPos.X + width * (unit.HealthPercent() / 100);
                                var drawStartXPos =
                                    (float) (barPos.X + (unit.Health >
                                                         Player.GetSpellDamage(unit, SpellSlot.Q) +
                                                         Player.GetSpellDamage(unit, SpellSlot.E) +
                                                         Player.GetSpellDamage(unit, SpellSlot.W)
                                                 ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) +
                                                                            Player.GetSpellDamage(unit, SpellSlot.E) +
                                                                            Player.GetSpellDamage(unit, SpellSlot.W))) /
                                                            unit.MaxHealth * 100 / 100)
                                                 : 0));

                                Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 8, true,
                                    unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) +
                                    Player.GetSpellDamage(unit, SpellSlot.W) +
                                    Player.GetSpellDamage(unit, SpellSlot.E)
                                        ? Color.GreenYellow
                                        : Color.Orange);

                            });
                }
                if (!Player.HasBuff("jaycestancehammer"))
                {
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(Q2.Range * 2))
                        .ToList()
                        .ForEach(
                            unit =>
                            {

                                var heroUnit = unit as Obj_AI_Hero;
                                int width = 103;
                                int height = 8;
                                int xOffset = SxOffset(heroUnit);
                                int yOffset = SyOffset(heroUnit);
                                var barPos = unit.FloatingHealthBarPosition;
                                barPos.X += xOffset;
                                barPos.Y += yOffset;
                                var drawEndXPos = barPos.X + width * (unit.HealthPercent() / 100);
                                var drawStartXPos =
                                    (float) (barPos.X + (unit.Health >
                                                         Player.GetSpellDamage(unit, SpellSlot.E)
                                                         + GetEQ(unit)
                                                 ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.E)
                                                                            + GetEQ(unit))) /
                                                            unit.MaxHealth * 100 / 100)
                                                 : 0));

                                Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 8, true,
                                    unit.Health < Player.GetSpellDamage(unit, SpellSlot.E)
                                    + GetEQ(unit)
                                        ? Color.GreenYellow
                                        : Color.Orange);

                            });
                }

            }
        }

        private void Game_OnUpdate()
        {

            if (Player.IsDead || MenuGUI.IsChatOpen())
            {
                return;
            }
            if (Player.HasBuff("jaycestancehammer"))
            {
                timer = Player.SpellBook.GetSpell(SpellSlot.Q).CooldownEnd;
            }

            switch (Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                         OnCombo();
                    break;
                case OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
                case OrbwalkingMode.Laneclear:
                    Clearing();
                    Jungle();
                    break;

            }
            if (Menu["flee"]["key"].Enabled)
            {
                Flee();
            }
              if (Menu["insec"]["key"].Enabled)
            {
                  FlashE();
            }
            if (Menu["combo"]["key"].Enabled)
            {
                SemiQ();
            }
            Killsteal();
        }

        public static List<Obj_AI_Minion> GetAllGenericMinionsTargets()
        {
            return GetAllGenericMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetAllGenericMinionsTargetsInRange(float range)
        {
            return GetEnemyLaneMinionsTargetsInRange(range).Concat(GetGenericJungleMinionsTargetsInRange(range))
                .ToList();
        }

        public static List<Obj_AI_Base> GetAllGenericUnitTargets()
        {
            return GetAllGenericUnitTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Base> GetAllGenericUnitTargetsInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(range))
                .Concat<Obj_AI_Base>(GetAllGenericMinionsTargetsInRange(range)).ToList();
        }

        private void FlashE()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            if (!Player.HasBuff("jaycestancehammer"))
            {
                R.Cast();
            }
            if (Player.HasBuff("jaycestancehammer"))
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (E.Ready)
                {
                    if (Flash.Ready && Flash != null && target.IsValidTarget())
                    {
                        if (target.IsValidTarget(380))
                        {

                            foreach (var ally in GameObjects.AllyHeroes)
                            {
                                if (ally != null && ally.Distance(Player) < 1000 && !ally.IsMe &&
                                    helalmoney < Game.TickCount)
                                {
                                    if (Flash.Cast(target.ServerPosition.Extend(ally.ServerPosition, -100)))
                                    {
                                        E.CastOnUnit(target);
                                        helalmoney = 0;
                                    }
                                }

                            }
                            if (Player.CountAllyHeroesInRange(1000) == 0)
                            {
                                if (helalmoney < Game.TickCount)
                                {
                                    if (Flash.Cast(target.ServerPosition.Extend(Player.ServerPosition, -100)))
                                    {
                                        E.CastOnUnit(target);
                                        helalmoney = 0;
                                    }
                                }
                            }
                        }
                        if (Menu["insec"]["gapq"].Enabled)
                        {
                            if (Menu["insec"]["procq"].Enabled)
                            {
                                if (target.Distance(Player) > 380 && target.IsValidTarget(Q.Range))
                                {
                                    if (Q.CastOnUnit(target))
                                    {
                                        helalmoney = Game.TickCount + 600;
                                    }
                                }
                            }
                            if (!Menu["insec"]["procq"].Enabled)
                            {
                                if (target.Distance(Player) > 380 && target.IsValidTarget(Q.Range))
                                {
                                    if (Q.CastOnUnit(target))
                                    {
                                        helalmoney = Game.TickCount + 0;
                                    }
                                }
                            }
                        }
                    }
                }


            }
        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }


        private void Clearing()
        {
            bool useQ = Menu["farming"]["lane"]["useq"].Enabled;
            bool useW = Menu["farming"]["lane"]["usew"].Enabled;
            float hitQ = Menu["farming"]["lane"]["hitq"].As<MenuSlider>().Value;
            float hitW = Menu["farming"]["lane"]["hitw"].As<MenuSlider>().Value;
            float manapercent = Menu["farming"]["lane"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (Player.HasBuff("jaycestancehammer"))
                {
                    if (useQ)
                    {
                        foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                        {


                            if (minion.IsValidTarget(Q.Range) && minion != null)
                            {
                                if (GameObjects.EnemyMinions.Count(h => h.IsValidTarget(300, false, false,
                                        minion.ServerPosition)) >= hitQ)
                                {
                                    Q.CastOnUnit(minion);
                                }
                            }
                        }
                    }
                    if (useW)
                    {
                        foreach (var minion in GetEnemyLaneMinionsTargetsInRange(W.Range))
                        {


                            if (minion.IsValidTarget(W.Range) && minion != null)
                            {
                                if (GameObjects.EnemyMinions.Count(h => h.IsValidTarget(W.Range, false, false,
                                        minion.ServerPosition)) >= hitW)

                                {
                                    W.Cast();
                                }
                            }
                        }
                    }
                }

            }
        }

        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range))
                .ToList();
        }

        private void Jungle()
        {
            foreach (var jungleTarget in GameObjects.Jungle.Where(m => m.IsValidTarget(Q2.Range)).ToList())
            {
                if (!jungleTarget.IsValidTarget() || !jungleTarget.IsValidSpellTarget())
                {
                    return;
                }
                bool meleeq = Menu["farming"]["jungle"]["qmelee"].Enabled;
                bool meleew = Menu["farming"]["jungle"]["wmelee"].Enabled;
                bool meleee = Menu["farming"]["jungle"]["emelee"].Enabled;
                bool rangedq = Menu["farming"]["jungle"]["qranged"].Enabled;
                bool rangedw = Menu["farming"]["jungle"]["wranged"].Enabled;
                float manapercent = Menu["farming"]["jungle"]["mana"].As<MenuSlider>().Value;
                if (manapercent < Player.ManaPercent())
                {
                    if (Player.HasBuff("jaycestancehammer"))
                    {
                        if (meleeq && jungleTarget.IsValidTarget(Q.Range))
                        {
                            Q.CastOnUnit(jungleTarget);
                        }
                        if (meleew && jungleTarget.IsValidTarget(W.Range))
                        {
                            W.Cast();
                        }
                        if (meleee && jungleTarget.IsValidTarget(E.Range))
                        {
                            E.CastOnUnit(jungleTarget);
                        }
                    }
                    if (!Player.HasBuff("jaycestancehammer"))
                    {
                        if (rangedq && jungleTarget.IsValidTarget(Q2.Range))
                        {
                            Q2.Cast(jungleTarget);
                        }
                        if (rangedw && jungleTarget.IsValidTarget(W2.Range))
                        {
                            W2.Cast();
                        }

                    }
                    if (!Player.HasBuff("jaycestancehammer"))
                    {
                        if (!Q2.Ready && !W2.Ready && !Player.HasBuff("JayceHyperCharge"))
                        {
                            R.Cast();
                        }
                    }
                    if (Player.HasBuff("jaycestancehammer"))
                    {
                        if (!Q.Ready && !W.Ready)
                        {
                            R.Cast();
                        }
                    }
                }

            }
        }

        private void Flee()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            bool useE = Menu["flee"]["usee"].Enabled;
            bool useR = Menu["flee"]["user"].Enabled;
            if (useE && !Player.HasBuff("jaycestancehammer"))
            {

                E.Cast(Player.ServerPosition.Extend(Game.CursorPos, 200));
            }
            if (useR)
            {

                R.Cast();
            }

        }

        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }

        private void Killsteal()
        {
            var enemies = GetBestKillableHero(QE, DamageType.Physical, false);
            if (enemies != null)
            {

                if (GetEQ(enemies) > enemies.Health || Player.GetSpellDamage(enemies, SpellSlot.Q) > enemies.Health ||
                    GetQ(enemies) > enemies.Health || Player.GetSpellDamage(enemies, SpellSlot.E) > enemies.Health)
                {
                    if (!Player.HasBuff("jaycestancehammer") && Player.Mana > Player.SpellBook.GetSpell(SpellSlot.E).Cost + 20)
                    {
                        if (Player.GetSpellDamage(enemies, SpellSlot.Q) > GetQ(enemies) ||
                             Player.GetSpellDamage(enemies, SpellSlot.Q) > GetEQ(enemies) ||
                            Player.GetSpellDamage(enemies, SpellSlot.E) > GetQ(enemies) ||
                            Player.GetSpellDamage(enemies, SpellSlot.E) > GetEQ(enemies))
                        {
                            R.Cast();
                        }
                    }
                    if (Player.HasBuff("jaycestancehammer") && Player.Mana > Player.SpellBook.GetSpell(SpellSlot.E).Cost + 20)
                    {
                        if ((Player.GetSpellDamage(enemies, SpellSlot.Q) < GetQ(enemies) ||
                             Player.GetSpellDamage(enemies, SpellSlot.Q) < GetEQ(enemies)) ||
                            Player.GetSpellDamage(enemies, SpellSlot.E) < GetQ(enemies) ||
                            Player.GetSpellDamage(enemies, SpellSlot.E) < GetEQ(enemies))
                        {
                            R.Cast();
                        }
                    }
                    if (Player.HasBuff("jaycestancehammer") && Menu["killsteal"]["qmelee"].Enabled &&
                        Player.GetSpellDamage(enemies, SpellSlot.Q) > enemies.Health && enemies.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(enemies);
                    }
                    if (Player.HasBuff("jaycestancehammer") && Menu["killsteal"]["emelee"].Enabled &&
                        Player.GetSpellDamage(enemies, SpellSlot.E) > enemies.Health && enemies.IsValidTarget(E.Range))
                    {
                        E.CastOnUnit(enemies);
                    }
                    if (!Player.HasBuff("jaycestancehammer") && Menu["killsteal"]["qranged"].Enabled &&
                        GetQ(enemies) > enemies.Health && enemies.IsValidTarget(Q2.Range))
                    {
                        Q2.Cast(enemies);
                    }
                    if (!Player.HasBuff("jaycestancehammer") && Menu["killsteal"]["qe"].Enabled &&
                        GetEQ(enemies) > enemies.Health && enemies.IsValidTarget(QE.Range) && Player.Mana > Player.SpellBook.GetSpell(SpellSlot.E).Cost + Player.SpellBook.GetSpell(SpellSlot.Q).Cost + 30)
                    {
                        if (E2.Cast(Player.ServerPosition.Extend(enemies.ServerPosition, 150)))
                        {
                            {
                                QE.Cast(enemies);

                            }
                        }
                    }

                }
            }
        }
    




    public static Obj_AI_Hero GetBestEnemyHeroTarget()
        {
            return GetBestEnemyHeroTargetInRange(float.MaxValue);
        }

        public static Obj_AI_Hero GetBestEnemyHeroTargetInRange(float range)
        {
            var ts = TargetSelector.Implementation;
            var target = ts.GetTarget(range);
            if (target != null && target.IsValidTarget() && !Invulnerable.Check(target))
            {
                return target;
            }

            var firstTarget = ts.GetOrderedTargets(range)
                .FirstOrDefault(t => t.IsValidTarget() && !Invulnerable.Check(t));
            if (firstTarget != null)
            {
                return firstTarget;
            }

            return null;
        }


        private void OnCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(QE.Range);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (!Menu["combo"]["qset"]["qerange"].Enabled && Menu["combo"]["eset"]["eq"].Enabled && target.IsValidTarget(QE.Range))
            {
                if (!Player.HasBuff("jaycestancehammer") && Q2.Ready && E2.Ready &&
                    Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.E).Cost + 30 <
                    Player.Mana)
                {
                    if (E2.Cast(Player.ServerPosition.Extend(target.ServerPosition, 150)))
                    {

                        QE.Cast(target);


                    }
                }
            }
            if (Menu["combo"]["qset"]["qerange"].Enabled && Menu["combo"]["eset"]["eq"].Enabled &&
                target.Distance(Player) > Q.Range && target.IsValidTarget(QE.Range))
                
            {
                if (!Player.HasBuff("jaycestancehammer") && Q2.Ready && E2.Ready &&
                    Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.E).Cost  + 30<
                    Player.Mana)
                {
                    if (E2.Cast(Player.ServerPosition.Extend(target.ServerPosition, 150)))
                    {

                        QE.Cast(target);


                    }
                }
            }
            if (Menu["combo"]["qset"]["qerange"].Enabled && Menu["combo"]["qset"]["ranged"].Enabled &&
                target.IsValidTarget(Q.Range))
            {
                if (!Player.HasBuff("jaycestancehammer") && Q2.Ready)
                {

                    Q2.Cast(target);



                }

            }
            if (!Menu["combo"]["qset"]["qerange"].Enabled && Q2.Ready && !E2.Ready ||
                (Q2.Ready && !Menu["combo"]["qset"]["qerange"].Enabled && Player.SpellBook.GetSpell(SpellSlot.Q).Cost +
                 Player.SpellBook.GetSpell(SpellSlot.E).Cost >
                 Player.Mana) || Q2.Ready && Menu["combo"]["qset"]["qerange"].Enabled &&
                !Menu["combo"]["eset"]["eq"].Enabled)
            {
                if (!Player.HasBuff("jaycestancehammer"))
                {

                    if (target.IsValidTarget(Q2.Range) && Menu["combo"]["qset"]["ranged"].Enabled)
                    {
                        Q2.Cast(target);
                    }



                }

            }
            if (!Player.HasBuff("jaycestancehammer"))
            {
                if (W.Ready && Menu["combo"]["wset"]["ranged"].Enabled && target.IsValidTarget(W2.Range))
                {

                    if (target != null)
                    {
                        W2.Cast();
                    }
                }

            }
            if (R.Ready)
            {
                if (target.IsValidTarget(1200))
                {
                    if (target.IsValidTarget(Q.Range) && !Player.HasBuff("jaycestancehammer"))
                    {
                        if (Menu["combo"]["wset"]["wait"].Enabled && !Player.HasBuff("JayceHyperCharge"))
                        {
                            if (!W.Ready)
                            {
                                if (timer - Game.ClockTime < 1)
                                {
                                    R.Cast();
                                }
                                if (timer - Game.ClockTime > 1 && target.IsValidTarget(200))
                                {
                                    R.Cast();
                                }
                            }
                        }
                        if (!Menu["combo"]["wset"]["wait"].Enabled && Player.HasBuff("JayceHyperCharge"))
                        {
                            if (!W.Ready)
                            {
                                if (timer - Game.ClockTime < 1)
                                {
                                    R.Cast();
                                }
                                if (timer - Game.ClockTime > 1 && target.IsValidTarget(200))
                                {
                                    R.Cast();
                                }
                            }
                        }
                    }
                    if (!Q.Ready && !W.Ready && Player.HasBuff("jaycestancehammer"))
                    {
                        R.Cast();
                    }
                    if (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) >
                        target.Health && Player.HasBuff("jaycestancehammer") && target.IsValidTarget(QE.Range))
                    {
                        R.Cast();
                    }
                    if (target.Distance(Player) > Q.Range + 200 && target.Distance(Player) < 1200 &&
                        Player.HasBuff("jaycestancehammer"))
                    {

                        var collisions =
                            (IList<Obj_AI_Base>) QE.GetPrediction(target).CollisionObjects;
                        if (collisions.Any())
                        {
                            if (collisions.All(c => GetAllGenericUnitTargets().Contains(c)))
                            {
                                return;
                            }
                        }
                        R.Cast();
                    }


                }
            }
            switch (Menu["combo"]["eset"]["emode"].As<MenuList>().Value)
            {
                case 0:
                    if (Player.HasBuff("jaycestancehammer"))
                    {
                        if (E.Ready)
                        {
                            if (Menu["combo"]["eset"]["melee"].Enabled && target.IsValidTarget(E.Range) &&
                                Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
                            {
                                E.CastOnUnit(target);
                            }
                        }
                    }
                    break;
                case 1:
                    if (Player.HasBuff("jaycestancehammer"))
                    {
                        if (E.Ready)
                        {
                            if (Menu["combo"]["eset"]["melee"].Enabled && target.IsValidTarget(E.Range))
                            {
                                if (GetEQ(target) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health &&
                                    Player.SpellBook.GetSpell(SpellSlot.R).CooldownEnd - Game.ClockTime < 4 ||
                                    Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    if (Player.HasBuff("jaycestancehammer"))
                    {
                        if (E.Ready)
                        {
                            if (Menu["combo"]["eset"]["melee"].Enabled && target.IsValidTarget(E.Range))
                            {
                                if (Player.SpellBook.GetSpell(SpellSlot.R).CooldownEnd - Game.ClockTime < 0.5)
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                        }
                    }
                    break;
            }
            if (Player.HasBuff("jaycestancehammer"))
            {
                if (Q.Ready)
                {
                    if (Menu["combo"]["qset"]["melee"].Enabled && target.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(target);
                    }
                }
            }
            if (Player.HasBuff("jaycestancehammer"))
            {
                if (W.Ready)
                {
                    if (Menu["combo"]["wset"]["melee"].Enabled && target.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }
                }
            }
        }

        private void OnHarass()
        {
            bool useQ = Menu["harass"]["ranged"].Enabled;
            bool useE = Menu["harass"]["qe"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(QE.Range);
            float manapercent = Menu["harass"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (!target.IsValidTarget())
                {
                    return;
                }

                if (useQ && target.IsValidTarget(Q2.Range))
                {
                    if (target != null)
                    {
                        Q2.Cast(target);
                    }
                }
                if (useE && target.IsValidTarget(QE.Range) && Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.E).Cost)
                {
                    if (target != null)
                    {
                        if (E2.Cast(Player.ServerPosition.Extend(target.ServerPosition, 150)))
                        {
                            {
                                QE.Cast(target);

                            }
                        }
                    }
                }
            }
        }
    }
}
// Hello :>