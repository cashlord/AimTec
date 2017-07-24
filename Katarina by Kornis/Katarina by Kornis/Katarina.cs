using System.Net.Configuration;
using System.Resources;
using System.Security.Authentication.ExtendedProtection;

namespace Katarina_By_Kornis
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

    internal class Katarina
    {
        public static Menu Menu = new Menu("Katarina By Kornis", "Katarina by Kornis", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        public static Spell Q, W, E, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 725);
            R = new Spell(SpellSlot.R, 550);

        }

        public Katarina()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuList("combomode", "Combo Mode:", new[] {"Q E", "E Q"}, 0));
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("usee", "Use E "));
                ComboMenu.Add(new MenuBool("savee", "^- Save E if no Daggers", false));
            }
            var RSet = new Menu("rset", "R Settings");
            {
                RSet.Add(new MenuBool("user", "Use R"));
                RSet.Add(new MenuList("rmode", "R Mode", new[] {"Always", "Only if Killable"}, 1));
                RSet.Add(new MenuSlider("dagger", "X R Daggers for Damage Check", 8, 1, 16));
                RSet.Add(new MenuSlider("rhit", "R If Hits", 1, 1, 5));
                RSet.Add(new MenuBool("cancelen", "Cancel R if no Enemies"));
                RSet.Add(new MenuBool("cancelks", "Cancel R for Killsteal"));
                RSet.Add(new MenuSlider("waster", "Don't waste R if Enemy HP lower than", 100, 0, 500));
            }
            Menu.Add(ComboMenu);
            ComboMenu.Add(RSet);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuList("harassmode", "Harass Mode:", new[] {"Q E", "E Q"}, 0));
                HarassMenu.Add(new MenuBool("useq", "Use Q"));
                HarassMenu.Add(new MenuBool("usew", "Use W"));
                HarassMenu.Add(new MenuBool("usee", "Use E"));

            }
            Menu.Add(HarassMenu);
            var FarmMenu = new Menu("farming", "Farming");
            {
                FarmMenu.Add(new MenuBool("useq", "Use Q to Farm"));
                FarmMenu.Add(new MenuBool("lastq", "^- Only for Last Hit"));
                FarmMenu.Add(new MenuBool("lastaa", "Don't Last Hit in AA Range"));
                FarmMenu.Add(new MenuBool("usew", "Use W to Farm"));
                FarmMenu.Add(new MenuSlider("hitw", "^- if Hits", 3, 1, 6));
                FarmMenu.Add(new MenuBool("usee", "Use E to Farm"));
                FarmMenu.Add(new MenuSlider("hite", "^- if Dagger Hits", 3, 1, 6));
                FarmMenu.Add(new MenuBool("turret", "Don't E Under the Turret"));
            }
            Menu.Add(FarmMenu);
            var LastMenu = new Menu("lasthit", "Last Hit");
            {
                LastMenu.Add(new MenuBool("lastq", "Use Q to Last Hit"));
                LastMenu.Add(new MenuBool("lastaa", "Don't Last Hit in AA Range"));
            }
            Menu.Add(LastMenu);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("ksq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kse", "Killsteal with E"));
                KSMenu.Add(new MenuBool("ksdagger", "^- Killsteal with E Dagger"));
                KSMenu.Add(new MenuBool("ksegap", "Gap with E for Q Killsteal"));

            }
            Menu.Add(KSMenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawdagger", "Draw Daggers"));
                DrawMenu.Add(new MenuBool("drawdamage", "Draw Damage"));
                DrawMenu.Add(new MenuBool("drawflee", "Draw Circle on Flee"));
            }
            Menu.Add(DrawMenu);
            var FleeMenu = new Menu("flee", "Flee");
            {
                FleeMenu.Add(new MenuBool("fleew", "Use W to Flee"));
                FleeMenu.Add(new MenuBool("fleee", "Use E to Flee"));
                FleeMenu.Add(new MenuBool("dagger", "^- Use E on Daggers"));
                FleeMenu.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.G, KeybindType.Press));
            }
            Menu.Add(FleeMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            SpellBook.OnCastSpell += OnCastSpell;
            LoadSpells();
            Console.WriteLine("Katarina by Kornis - Loaded");
        }

        public static readonly List<string> SpecialChampions = new List<string> {"Annie", "Jhin"};

        public static int SxOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 1 : 10;
        }

        public static int SyOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 3 : 20;
        }

        private int timeW;
        private bool meowmeowRthing = false;
        public void OnCastSpell(Obj_AI_Base sender, SpellBookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.W)
            {
                timeW = Game.TickCount + 1200;
            }
            if (sender.IsMe)
            {
                if (args.Slot != SpellSlot.R && Player.HasBuff("katarinarsound"))
                {
                    
                    args.Process = false;
                }
            }
        }

        static double GetR(Obj_AI_Base target)
        {
            double meow = 0;
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 1)
            {
                meow = 25;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 2)
            {
                meow = 37.5;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 3)
            {
                meow = 50;
            }
            double ap = Player.TotalAbilityDamage * 0.19;
            double ad = (Player.TotalAttackDamage - Player.BaseAttackDamage) * 0.22;
            double full = ap + ad + meow;
            double damage = Player.CalculateDamage(target, DamageType.Magical, full);
            return damage;

        }
                public static double Passive(Obj_AI_Base target)
        {
            double dmg = 0;
            double yay = 0;
            foreach (var daggers in GameObjects.AllGameObjects)
            {
                if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                {
                    if (daggers.Distance(target) < 450)
                    {


                        if (Player.Level >= 1 && Player.Level < 6)
                        {
                            dmg = 0.55;
                        }
                        if (Player.Level >= 6 && Player.Level < 11)
                        {
                            dmg = 0.7;
                        }
                        if (Player.Level >= 11 && Player.Level < 16)
                        {
                            dmg = 0.85;
                        }
                        if (Player.Level >= 16)
                        {
                            dmg = 1;
                        }
                        double psv = 35 + (Player.Level * 12);
                        double psvdmg = Player.TotalAbilityDamage * dmg;
                        double full = psvdmg + psv + (Player.TotalAttackDamage - Player.BaseAttackDamage);
                        double damage = Player.CalculateDamage(target, DamageType.Magical, full);
                        yay = damage;
                    }

                }
            }
            return yay;
        }



        private void Render_OnPresent()
        {
            
            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.CornflowerBlue);
            }

            if (Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.Crimson);
            }
            if (Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Crimson);
            }
            if (Menu["drawings"]["drawdagger"].Enabled)
            {


                foreach (var daggers in GameObjects.AllGameObjects)
                {
                    if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                    {

                        if (daggers.CountEnemyHeroesInRange(450) != 0)
                        {
                            Render.Circle(daggers.ServerPosition, 450, 40, Color.LawnGreen);
                            Render.Circle(daggers.ServerPosition, 150, 40, Color.LawnGreen);
                        }
                        if (daggers.CountEnemyHeroesInRange(450) == 0)
                        {
                            Render.Circle(daggers.ServerPosition, 450, 40, Color.Red);
                            Render.Circle(daggers.ServerPosition, 150, 40, Color.Red);
                        }
                    }
                }
            }




            if (Menu["flee"]["key"].Enabled && Menu["drawings"]["drawflee"].Enabled)
            {
                Render.Circle(Game.CursorPos, 200, 50, Color.Yellow);
            }

            if (Menu["drawings"]["drawdamage"].Enabled)
            {

                ObjectManager.Get<Obj_AI_Base>()
                    .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(Q.Range + R.Range))
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
                                                     (GetR(unit) *
                                                      Menu["combo"]["rset"]["dagger"].As<MenuSlider>().Value) +
                                                     Passive(unit)
                                             ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) +
                                                         Player.GetSpellDamage(unit, SpellSlot.E) +
                                                         (GetR(unit) *
                                                          Menu["combo"]["rset"]["dagger"].As<MenuSlider>().Value) +
                                                         Passive(unit))) / unit.MaxHealth * 100 / 100)
                                             : 0));

                            Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 8, true,
                                unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) +
                                (GetR(unit) * Menu["combo"]["rset"]["dagger"].As<MenuSlider>().Value) +
                                Player.GetSpellDamage(unit, SpellSlot.E) + Passive(unit)
                                    ? Color.GreenYellow
                                    : Color.Orange);

                        });
            }
        }

        private void Game_OnUpdate()
        {

            if (Player.IsDead || MenuGUI.IsChatOpen())
            {
                return;
            }
            if (Player.HasBuff("katarinarsound"))
            {
                Orbwalker.MovingEnabled = false;
                Orbwalker.AttackingEnabled = false;

            }
            else
            {

                Orbwalker.MovingEnabled = true;
                Orbwalker.AttackingEnabled = true;
            }
            switch (Orbwalker.Mode)
                {
                    case OrbwalkingMode.Combo:
                        OnCombo();
                        break;
                    case OrbwalkingMode.Mixed:
                        OnHarass();
                        break;
                    case OrbwalkingMode.Lasthit:
                        Lasthit();
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
            Killsteal();
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
            bool useQ = Menu["farming"]["useq"].Enabled;
            bool useW = Menu["farming"]["usew"].Enabled;
            bool useE = Menu["farming"]["usee"].Enabled;
            float hitW = Menu["farming"]["hitw"].As<MenuSlider>().Value;
            float hitE = Menu["farming"]["hite"].As<MenuSlider>().Value;
            if (useQ)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {


                    if (minion.IsValidTarget(Q.Range) && minion != null && !Menu["farming"]["lastq"].Enabled)
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }
            if (Menu["farming"]["lastq"].Enabled)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {
                    if (minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        if (Menu["farming"]["lastaa"].Enabled)
                        {
                            if (minion.Distance(Player) > 250)
                            {
                                Q.CastOnUnit(minion);
                            }
                        }
                        if (!Menu["farming"]["lastaa"].Enabled)
                        {
                            Q.CastOnUnit(minion);
                        }

                    }
                }
            }
            if (useE)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
                {

                    if (minion.IsValidTarget(E.Range) && minion != null)
                    {
                        {
                            foreach (var daggers in GameObjects.AllGameObjects)
                            {
                                if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                                {
                                    if (GameObjects.EnemyMinions.Count(h => h.IsValidTarget(450, false, false,
                                            daggers.ServerPosition)) >= hitE)
                                    {
                                        if (timeW < Game.TickCount)
                                        {
                                            if (Menu["farming"]["turret"].Enabled)
                                            {
                                                if (!daggers.IsUnderEnemyTurret())
                                                {
                                                    E.Cast(daggers.ServerPosition.Extend(minion.ServerPosition, 200));
                                                    
                                                }
                                            }
                                            if (!Menu["farming"]["turret"].Enabled)
                                            {

                                                E.Cast(daggers.ServerPosition.Extend(minion.ServerPosition, 200));

                                            }

                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                if (useW)
                {

                    foreach (var minion in GetEnemyLaneMinionsTargetsInRange(300))
                    {

                        if (minion.IsValidTarget(W.Range) && minion != null &&
                            GetEnemyLaneMinionsTargetsInRange(300).Count >= hitW)
                        {

                            W.Cast();

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
          return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range)).ToList();
        }

        private void Jungle()
        {
            foreach (var jungleTarget in GetGenericJungleMinionsTargetsInRange(Q.Range))
            {
                bool useQ = Menu["farming"]["useq"].Enabled;
                bool useW = Menu["farming"]["usew"].Enabled;

                if (useQ && jungleTarget.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(jungleTarget);
                }
                if (useW && jungleTarget.IsValidTarget(250f))
                {
                    W.Cast();
                }
            }
        }


        private void Lasthit()
        {
            if (Menu["lasthit"]["lastq"].Enabled)
            {

                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {

                    if (minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        if (Menu["lasthit"]["lastaa"].Enabled)
                        {
                            if (minion.Distance(Player) > 250)
                            {
                                Q.CastOnUnit(minion);
                            }
                        }
                        if (!Menu["lasthit"]["lastaa"].Enabled)
                        {
                            Q.CastOnUnit(minion);
                        }

                    }
                }
            }
        }

        private void Flee()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            bool usew = Menu["flee"]["fleew"].Enabled;
            bool usee = Menu["flee"]["fleee"].Enabled;
            bool meow = Menu["flee"]["dagger"].Enabled;
            if (usew)
            {

                W.Cast();
            }
            if (usee)
            {

                foreach (var en in ObjectManager.Get<Obj_AI_Base>())
                {
                    if (!en.IsDead)
                    {
                        if (en.Distance(Game.CursorPos) < 200)
                        {
                            E.Cast(en.ServerPosition);
                        }

                    }
                }
                if (meow)
                {
                    foreach (var daggers in GameObjects.AllGameObjects)
                    {
                        if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                        {
                            if (daggers.Distance(Game.CursorPos) < 200)
                            {
                                E.Cast(daggers.ServerPosition);
                            }
                        }
                    }
                }
            }
        }

        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }

        public static Obj_AI_Hero GetEGAP(DamageType damageType = DamageType.True, bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(Q.Range + E.Range)
                .FirstOrDefault(t => t.IsValidTarget());
        }

        private void Killsteal()
        {
            if (Menu["killsteal"]["ksdagger"].Enabled)
            {
                foreach (var daggers in GameObjects.AllGameObjects)

                {
                    if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                    {
                        var bestTarget = GetEGAP(DamageType.Magical, false);
                        if (bestTarget != null &&
                            Passive(bestTarget) >= bestTarget.Health && bestTarget.Distance(daggers) < 450 &&
                            bestTarget.IsValidTarget(E.Range))
                        {
                            if (Player.HasBuff("katarinarsound"))
                            {
                                Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                            }
                            E.Cast(bestTarget.ServerPosition.Extend(daggers.ServerPosition, 200));


                        }
                    }
                }
            }

            if (Q.Ready &&
                Menu["killsteal"]["ksq"].Enabled)
            {
                var bestTarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health && bestTarget.IsValidTarget(Q.Range))
                {
                    if (Player.HasBuff("katarinarsound"))
                    {
                        Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                    }
                    Q.CastOnUnit(bestTarget);
                }
            }
            if (E.Ready &&
                Menu["killsteal"]["kse"].Enabled)
            {
                var bestTarget = GetBestKillableHero(E, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.E) >= bestTarget.Health && bestTarget.IsValidTarget(E.Range))
                {
                    if (Player.HasBuff("katarinarsound"))
                    {
                        Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                    }
                    E.CastOnUnit(bestTarget);
                }
            }

            if (Q.Ready &&
                Menu["killsteal"]["ksegap"].Enabled)
            {
                var bestTarget = GetEGAP(DamageType.Magical, false);
                if (bestTarget != null &&
                    bestTarget.Distance(Player) > Q.Range &&
                    bestTarget.Health <= Player.GetSpellDamage(bestTarget, SpellSlot.Q))
                {
                    foreach (var en in ObjectManager.Get<Obj_AI_Base>())
                    {
                        if (!en.IsDead &&
                            en.Distance(bestTarget) < Q.Range && en.Distance(Player) < E.Range)
                        {
                            if (Player.HasBuff("katarinarsoudn"))
                            {
                                Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                            }
                            E.Cast(en.ServerPosition);


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

            var firstTarget = ts.GetOrderedTargets(range).FirstOrDefault(t => t.IsValidTarget() && !Invulnerable.Check(t));
            if (firstTarget != null)
            {
                return firstTarget;
            }

            return null;
        }

        private void OnCombo()
        {

            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            bool SaveE = Menu["combo"]["savee"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useR = Menu["combo"]["rset"]["user"].Enabled;
            bool cancel = Menu["combo"]["rset"]["cancelen"].Enabled;
            bool ksR = Menu["combo"]["rset"]["cancelks"].Enabled;
            float hitR = Menu["combo"]["rset"]["rhit"].As<MenuSlider>().Value;
            float dagggggggers = Menu["combo"]["rset"]["dagger"].As<MenuSlider>().Value;
            float meow = Menu["combo"]["rset"]["waster"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            if (cancel)
            {
                if (Player.HasBuff("katarinarsound"))
                {
                    if (Player.CountEnemyHeroesInRange(R.Range) == 0)
                    {
                      
                        Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                    }
                }
            }
            if (ksR)
            {
                if (Player.HasBuff("katarinarsound"))
                {
                    if (target.Distance(Player) >= R.Range - 100 && target != null && E.Ready)
                    {
                        Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                        var dagger = ObjectManager.Get<Obj_AI_Base>()
                            .Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                        foreach (var daggers in GameObjects.AllGameObjects)

                        {
                            if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                            {
                                if (target.Distance(daggers) < 450 &&
                                    target.IsValidTarget(E.Range) && E.Ready)

                                {

                                    E.Cast(daggers.ServerPosition.Extend(target.ServerPosition, 200));


                                }
                                if (daggers.Distance(Player) > E.Range)
                                {
                                    E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                }
                                if (daggers.Distance(target) > 450)
                                {

                                    E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                }
                            }
                            if (dagger.Count() == 0)
                            {

                                E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                            }
                        }
                    }
                    if (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) >=
                        target.Health)
                    {
                        
                        var dagger = ObjectManager.Get<Obj_AI_Base>()
                            .Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                        foreach (var daggers in GameObjects.AllGameObjects)

                        {
                            if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid && E.Ready)
                            {
                                Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                                if (target.Distance(daggers) < 450 &&
                                    target.IsValidTarget(E.Range) && E.Ready)

                                {

                                    E.Cast(daggers.ServerPosition.Extend(target.ServerPosition, 200));


                                }
                                if (daggers.Distance(Player) > E.Range)
                                {
                                    E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                }
                                if (daggers.Distance(target) > 450)
                                {

                                    E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                }
                            }
                            if (dagger.Count() == 0 && E.Ready)
                            {
                                Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                                E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                            }

                            if (target.IsValidTarget(Q.Range) && Q.Ready)
                            {
                                Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                                Q.CastOnUnit(target);
                            }

                        }

                    }
                }
            }
            if (!target.IsValidTarget())
            {
                return;
            }
            
            switch (Menu["combo"]["combomode"].As<MenuList>().Value)
            {
                case 0:
                    if (!target.IsValidTarget())
                    {
                        return;
                    }
                    if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
                    {
                        if (target != null)
                        {
                            Q.CastOnUnit(target);
                        }
                    }

                    if (E.Ready && useE && target.IsValidTarget(E.Range) && !Q.Ready)
                    {
                        if (target != null)
                        {
                            var dagger = ObjectManager.Get<Obj_AI_Base>()
                                .Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                            foreach (var daggers in GameObjects.AllGameObjects)

                            {
                                if (!SaveE)
                                {
                                    if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                                    {
                                        if (target.Distance(daggers) < 450 &&
                                            target.IsValidTarget(E.Range))
                                        {

                                            E.Cast(daggers.ServerPosition.Extend(target.ServerPosition, 200));


                                        }
                                        if (daggers.Distance(Player) > E.Range)
                                        {
                                            E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                        }
                                        if (daggers.Distance(target) > 450)
                                        {

                                            E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                        }
                                    }
                                    if (dagger.Count() == 0)
                                    {

                                        E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                    }

                                }
                                if (SaveE)
                                {
                                    if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                                    {
                                        if (target.Distance(daggers) < 450 &&
                                            target.IsValidTarget(E.Range))
                                        {

                                            E.Cast(daggers.ServerPosition.Extend(target.ServerPosition, 200));


                                        }
                                    }
                                }
                            }

                        }
                    }
                    if (W.Ready && useW && target.IsValidTarget(W.Range))
                    {
                        if (target != null)
                        {
                            W.Cast();
                        }
                    }
                    if (useR)
                    {
                        switch (Menu["combo"]["rset"]["rmode"].As<MenuList>().Value)
                        {
                            case 0:
                                if (R.Ready && target.IsValidTarget(R.Range - 50))
                                {
                                    if (target != null && Player.CountEnemyHeroesInRange(R.Range - 150) >= hitR)
                                    {
                                        if (target.Health > meow && !Q.Ready)
                                        {
                                            R.Cast();
                                        }
                                    }
                                }
                                break;
                            case 1:
                                if (R.Ready && target.IsValidTarget(R.Range - 150))
                                {
                                    if (target != null && target.Health <=
                                        Player.GetSpellDamage(target, SpellSlot.Q) +
                                        Player.GetSpellDamage(target, SpellSlot.E) + Passive(target) +
                                        GetR(target) * dagggggggers)
                                    {
                                        if (target.Health > meow && !Q.Ready)
                                        {
                                            R.Cast();
                                        }
                                    }
                                }
                                break;
                        }

                    }

                    break;
                case 1:
                    if (!target.IsValidTarget())
                    {
                        return;
                    }
                    if (E.Ready && useE && target.IsValidTarget(E.Range))
                    {
                        if (target != null)
                        {
                            var dagger = ObjectManager.Get<Obj_AI_Base>()
                                .Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                            foreach (var daggers in GameObjects.AllGameObjects)

                            {
                                if (!SaveE)
                                {
                                    if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                                    {
                                        if (target.Distance(daggers) < 450 &&
                                            target.IsValidTarget(E.Range))
                                        {

                                            E.Cast(daggers.ServerPosition.Extend(target.ServerPosition, 200));


                                        }
                                        if (daggers.Distance(Player) > E.Range)
                                        {
                                            E.Cast(target.ServerPosition.Extend(Player.ServerPosition, 50));
                                        }
                                        if (daggers.Distance(target) > 450)
                                        {

                                            E.Cast(target.ServerPosition.Extend(Player.ServerPosition, 50));
                                        }
                                    }
                                    if (dagger.Count() == 0)
                                    {

                                        E.Cast(target.ServerPosition.Extend(Player.ServerPosition, 50));
                                    }

                                }
                                if (SaveE)
                                {
                                    if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                                    {
                                        if (target.Distance(daggers) < 450 &&
                                            target.IsValidTarget(E.Range))
                                        {

                                            E.Cast(daggers.ServerPosition.Extend(target.ServerPosition, 200));


                                        }
                                    }
                                }

                            }
                        }
                    }
                    if (W.Ready && useW && target.IsValidTarget(W.Range))
                    {
                        if (target != null)
                        {
                            W.Cast();
                        }
                    }
                    if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
                    {
                        if (target != null)
                        {
                            Q.CastOnUnit(target);
                        }
                    }
                    if (useR)
                    {
                        switch (Menu["combo"]["rset"]["rmode"].As<MenuList>().Value)
                        {
                            case 0:
                                if (R.Ready && target.IsValidTarget(R.Range - 150))
                                {
                                    if (target != null && Player.CountEnemyHeroesInRange(R.Range - 50) >= hitR)
                                    {
                                        if (target.Health > meow && !Q.Ready)
                                        {
                                            R.Cast();
                                        }
                                    }
                                }
                                break;
                            case 1:
                                if (R.Ready && target.IsValidTarget(R.Range - 150))
                                {
                                    if (target != null && target.Health <=
                                        Player.GetSpellDamage(target, SpellSlot.Q) +
                                        Player.GetSpellDamage(target, SpellSlot.E) + Passive(target) +
                                        GetR(target) * dagggggggers)
                                    {
                                        if (target.Health > meow && !Q.Ready)
                                        {
                                            R.Cast();
                                        }
                                    }
                                }
                                break;
                        }

                    }
                    break;
            }
        }




        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool useW = Menu["harass"]["usew"].Enabled;
            bool useE = Menu["harass"]["usee"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            switch (Menu["harass"]["harassmode"].As<MenuList>().Value)
            {
                case 0:
                    if (!target.IsValidTarget())
                    {
                        return;
                    }
                    if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
                    {
                        if (target != null)
                        {
                            Q.CastOnUnit(target);
                        }
                    }

                    if (E.Ready && useE && target.IsValidTarget(E.Range) && !Q.Ready)
                    {
                        if (target != null)
                        {
                            var dagger = ObjectManager.Get<Obj_AI_Base>().Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                            foreach (var daggers in GameObjects.AllGameObjects)

                            {
                                if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                                {
                                    if (target.Distance(daggers) < 450 &&
                                        target.IsValidTarget(E.Range))
                                    {

                                        E.Cast(daggers.ServerPosition.Extend(target.ServerPosition, 200));


                                    }
                                    if (daggers.Distance(Player) > E.Range)
                                    {
                                      E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                    }
                                    if (daggers.Distance(target) > 450)
                                    {
                                        
                                        E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                    }
                                }
                                if (dagger.Count() == 0)
                                {
                                  
                                    E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -50));
                                }
    
                            }

                        }
                    }
                    if (W.Ready && useW && target.IsValidTarget(W.Range))
                    {
                        if (target != null)
                        {
                            W.Cast();
                        }
                    }

                    break;
                case 1:
                    if (!target.IsValidTarget())
                    {
                        return;
                    }
                    if (E.Ready && useE && target.IsValidTarget(E.Range))
                    {
                        if (target != null)
                        {
                            var dagger = ObjectManager.Get<Obj_AI_Base>().Where(a => a.Name == "HiddenMinion" && a.IsValid && !a.IsDead);
                            foreach (var daggers in GameObjects.AllGameObjects)

                            {
                                if (daggers.Name == "HiddenMinion" && !daggers.IsDead && daggers.IsValid)
                                {
                                    if (target.Distance(daggers) < 450 &&
                                        target.IsValidTarget(E.Range))
                                    {

                                        E.Cast(daggers.ServerPosition.Extend(target.ServerPosition, 200));


                                    }
                                    if (daggers.Distance(Player) > E.Range)
                                    {
                                        E.Cast(target.ServerPosition.Extend(Player.ServerPosition, 50));
                                    }
                                    if (daggers.Distance(target) > 450)
                                    {

                                        E.Cast(target.ServerPosition.Extend(Player.ServerPosition, 50));
                                    }
                                }
                                if (dagger.Count() == 0)
                                {

                                    E.Cast(target.ServerPosition.Extend(Player.ServerPosition, 50));
                                }

                            }

                        }
                    }

                    if (W.Ready && useW && target.IsValidTarget(W.Range))
                    {
                        if (target != null)
                        {
                            W.Cast();
                        }
                    }
                    if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
                    {
                        if (target != null)
                        {
                            Q.CastOnUnit(target);
                        }
                    }              
                    break;
            }
        }
    }
}
