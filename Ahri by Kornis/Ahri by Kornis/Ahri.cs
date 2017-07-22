using System.Net.Configuration;
using System.Resources;
using System.Security.Authentication.ExtendedProtection;

namespace Ahri_By_Kornis
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

    internal class Ahri
    {
        public static Menu Menu = new Menu("Ahri By Kornis", "Ahri by Kornis", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, W, E, R, Flash;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 880);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 600);
            Q.SetSkillshot(0.25f, 70f, 1500f, false, SkillshotType.Line);
            E.SetSkillshot(0.25f, 50f, 1000f, true, SkillshotType.Line, false, HitChance.High);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner1, 425);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner2, 425);

        }

        public Ahri()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuSlider("rangew", "^- Range", 650, 1, 700));
                ComboMenu.Add(new MenuBool("usee", "Use E "));
                ComboMenu.Add(new MenuKeyBind("flashe", "E - Flash", KeyCode.T, KeybindType.Press));
                ComboMenu.Add(new MenuSlider("rangee", "^- Range", 1100, 950, 1250));
            }
            var RSet = new Menu("rset", "R Settings");
            {
                RSet.Add(new MenuBool("user", "Use R"));
                RSet.Add(new MenuList("rusage", "R Usage", new[] {"Always", "Only if Killable"}, 1));
                RSet.Add(new MenuList("rmode", "R Mode", new[] {"To Side", "To Mouse", "To Target"}, 0));
                RSet.Add(new MenuBool("burstr", "Auto R Burst Logic if Killable"));
                RSet.Add(new MenuKeyBind("under", "R Under-Turret Toggle", KeyCode.Z, KeybindType.Toggle));
                RSet.Add(new MenuSlider("waster", "Don't Jump in X Enemies", 3, 2, 5));
            }
            Menu.Add(ComboMenu);
            ComboMenu.Add(RSet);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                HarassMenu.Add(new MenuBool("useq", "Use Q to Harass"));
                HarassMenu.Add(new MenuBool("usew", "Use W to Harass"));
                HarassMenu.Add(new MenuBool("usee", "Use E to Harass"));

            }
            Menu.Add(HarassMenu);
            var FarmMenu = new Menu("farming", "Farming");
            var LaneClear = new Menu("lane", "Lane Clear");
            {
                LaneClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                LaneClear.Add(new MenuBool("useq", "Use Q to Farm"));
                //LaneClear.Add(new MenuSlider("hitw", "^- if Hits", 3, 1, 6));
            }
            var JungleClear = new Menu("jungle", "Jungle Clear");
            {
                JungleClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                JungleClear.Add(new MenuBool("useq", "Use Q to Farm"));
                JungleClear.Add(new MenuBool("usew", "Use W to Farm"));
            }
            Menu.Add(FarmMenu);
            FarmMenu.Add(LaneClear);
            FarmMenu.Add(JungleClear);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("ksq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("ksw", "Killsteal with W"));
                KSMenu.Add(new MenuBool("kse", "Killsteal with E"));
            }
            Menu.Add(KSMenu);
            var miscmenu = new Menu("misc", "Misc.");
            {
                //  miscmenu.Add(new MenuBool("interrupte", "Interrupt with E"));
                miscmenu.Add(new MenuBool("autoe", "Auto E on CC"));
            }
            Menu.Add(miscmenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawflash", "Draw E-Flash Range"));
                DrawMenu.Add(new MenuBool("drawdamage", "Draw Damage"));
                DrawMenu.Add(new MenuBool("drawtoggle", "Draw Toggle"));
            }
            Menu.Add(DrawMenu);
            var FleeMenu = new Menu("flee", "Flee");
            {
                FleeMenu.Add(new MenuBool("useq", "Use Q to Flee"));
                FleeMenu.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.G, KeybindType.Press));
            }
            Menu.Add(FleeMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            LoadSpells();
            Console.WriteLine("Ahri by Kornis - Loaded");
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

        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;
            if (Menu["drawings"]["drawtoggle"].Enabled)
            {
                if (Menu["combo"]["rset"]["under"].Enabled)
                {
                    Render.Text(xaOffset - 50, yaOffset + 10, Color.HotPink, "Under-Turret: ON",
                        RenderTextFlags.VerticalCenter);
                }
                if (!Menu["combo"]["rset"]["under"].Enabled)
                {
                    Render.Text(xaOffset - 50, yaOffset + 10, Color.HotPink, "Under-Turret: OFF",
                        RenderTextFlags.VerticalCenter);
                }
            }
            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.CornflowerBlue);
            }
            if (Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, Menu["combo"]["rangew"].As<MenuSlider>().Value, 40, Color.Crimson);
            }
            if (Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.CornflowerBlue);
            }
            if (Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Crimson);
            }
            if (Menu["drawings"]["drawflash"].Enabled)
            {
                Render.Circle(Player.Position, Menu["combo"]["rangee"].As<MenuSlider>().Value, 40, Color.HotPink);
            }
            if (Menu["drawings"]["drawdamage"].Enabled)
            {

                ObjectManager.Get<Obj_AI_Base>()
                    .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(Q.Range*2))
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
                                                     Player.GetSpellDamage(unit, SpellSlot.W) +
                                                     Player.GetSpellDamage(unit, SpellSlot.R) * 3
                                             ? width * ((unit.Health - Player.GetSpellDamage(unit, SpellSlot.Q) +
                                                         Player.GetSpellDamage(unit, SpellSlot.E) +
                                                         Player.GetSpellDamage(unit, SpellSlot.W) +
                                                         Player.GetSpellDamage(unit, SpellSlot.R) * 3) /
                                                        unit.MaxHealth * 100 / 100)
                                             : 0));

                            Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 8, true,
                                unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) +
                                Player.GetSpellDamage(unit, SpellSlot.W) +
                                Player.GetSpellDamage(unit, SpellSlot.E) +
                                Player.GetSpellDamage(unit, SpellSlot.R) * 3
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
            if (Menu["combo"]["flashe"].Enabled)
            {
                FlashE();
            }
            Killsteal();
            if (Menu["misc"]["autoe"].Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(E.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {

                    E.Cast(target);
                }

            }

        }

        public static List<Obj_AI_Minion> GetAllGenericMinionsTargets()
        {
            return GetAllGenericMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetAllGenericMinionsTargetsInRange(float range)
        {
            return GetEnemyLaneMinionsTargetsInRange(range).Concat(GetGenericJungleMinionsTargetsInRange(range)).ToList();
        }

        public static List<Obj_AI_Base> GetAllGenericUnitTargets()
        {
            return GetAllGenericUnitTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Base> GetAllGenericUnitTargetsInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(range)).Concat<Obj_AI_Base>(GetAllGenericMinionsTargetsInRange(range)).ToList();
        }

        private void FlashE()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            var target = GetBestEnemyHeroTargetInRange(Menu["combo"]["rangee"].As<MenuSlider>().Value);
            if (E.Ready)
            {
                if (Flash.Ready && Flash != null && target.IsValidTarget())
                {
                    if (target.IsValidTarget(Menu["combo"]["rangee"].As<MenuSlider>().Value))
                    {
                        if (target.Distance(Player) > E.Range - 100)
                        {
                            var meow = E.GetPrediction(target);
                            var collisions =
                                (IList<Obj_AI_Base>) E.GetPrediction(target).CollisionObjects;
                            if (collisions.Any())
                            {
                                if (collisions.All(c => GetAllGenericUnitTargets().Contains(c)))
                                {
                                    return;
                                }
                            }
                            if (E.Cast(meow.CastPosition))
                            {
                                DelayAction.Queue(200, () =>
                                {
                                    Flash.Cast(target.ServerPosition);
                                });
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
            float manapercent = Menu["farming"]["lane"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (useQ)
                {
                    foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                    {


                        if (minion.IsValidTarget(Q.Range) && minion != null)
                        {
                            Q.Cast(minion);
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
                bool useQ = Menu["farming"]["jungle"]["useq"].Enabled;
                bool useW = Menu["farming"]["jungle"]["usew"].Enabled;
                float manapercent = Menu["farming"]["jungle"]["mana"].As<MenuSlider>().Value;
                if (manapercent < Player.ManaPercent())
                {
                    if (useQ && jungleTarget.IsValidTarget(Q.Range))
                    {
                        Q.Cast(jungleTarget);
                    }
                    if (useW && jungleTarget.IsValidTarget(500))
                    {
                        W.Cast();
                    }
                }

            }
        }

        private void Flee()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            bool useQ = Menu["flee"]["useq"].Enabled;
            if (useQ)
            {

                Q.Cast(Player.ServerPosition.Extend(Game.CursorPos, -500));
            }

        }

        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }

        private void Killsteal()
        {
            if (Q.Ready &&
                Menu["killsteal"]["ksq"].Enabled)
            {
                var bestTarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(Q.Range))
                {
                    Q.Cast(bestTarget);
                }
            }
            if (W.Ready &&
                Menu["killsteal"]["ksw"].Enabled)
            {
                var bestTarget = GetBestKillableHero(W, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.W) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
            if (E.Ready &&
                Menu["killsteal"]["kse"].Enabled)
            {
                var bestTarget = GetBestKillableHero(E, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.E) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(bestTarget);
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
        public static bool AnyWallInBetween(Vector3 startPos, Vector2 endPos)
        {
            for (var i = 0; i < startPos.Distance(endPos); i++)
            {
                var point = NavMesh.WorldToCell(startPos.Extend(endPos, i));
                if (point.Flags.HasFlag(NavCellFlags.Wall | NavCellFlags.Building))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnCombo()
        {

            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useR = Menu["combo"]["rset"]["user"].Enabled;
            bool bursting = Menu["combo"]["rset"]["burstr"].Enabled;
            float enemies = Menu["combo"]["rset"]["waster"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(E.Range);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (useE && target.IsValidTarget(E.Range))
            {
                if (target != null)
                {
                    if (!bursting)
                    {
                        E.Cast(target);
                    }
                    if (bursting && (!R.Ready || Player.SpellBook.GetSpell(SpellSlot.R).Level == 0 || target.Health >
                                     Player.GetSpellDamage(target, SpellSlot.Q) +
                                     Player.GetSpellDamage(target, SpellSlot.W) +
                                     Player.GetSpellDamage(target, SpellSlot.E) +
                                     Player.GetSpellDamage(target, SpellSlot.R) * 2))
                    {

                        E.Cast(target);

                    }
                }
            }
            if (bursting)
            {

                if (target.Health <
                    Player.GetSpellDamage(target, SpellSlot.Q) +
                    Player.GetSpellDamage(target, SpellSlot.W) +
                    Player.GetSpellDamage(target, SpellSlot.E) +
                    Player.GetSpellDamage(target, SpellSlot.R) * 2)
                {
                    if (enemies >= target.CountEnemyHeroesInRange(600))
                    {
                        if (R.Ready && E.Ready)
                        {
                            if (E.Cast())
                            {
                                R.Cast(target.ServerPosition.Extend(Player.ServerPosition, -200));

                            }
                        }
                    }

                    if (Player.HasBuff("AhriTumble"))
                    {
                        switch (Menu["combo"]["rset"]["rmode"].As<MenuList>().Value)
                        {
                            case 0:

                                var targetpos = Prediction.GetPrediction(target, 0.15f).UnitPosition.To2D();
                                Vector2 intersec = new Vector2();
                                for (int i = 450; i >= 0; i = i - 50)
                                {
                                    for (int j = 50; j <= 450; j = j + 50)
                                    {
                                        var vectors =
                                            Vector2Extensions.CircleCircleIntersection(Player.Position.To2D(),
                                                targetpos, i, j);
                                        foreach (var x in vectors)
                                        {
                                            if (!AnyWallInBetween(Player.Position, x))
                                            {
                                                intersec = x;
                                                goto ABC;
                                            }
                                        }
                                    }
                                }
                                ABC:
                                R.Cast(intersec.To3D());
                                break;
                            case 1:
                                if (Menu["combo"]["rset"]["under"].Enabled)
                                {
                                    R.Cast(Game.CursorPos);
                                }
                                if (!Menu["combo"]["rset"]["under"].Enabled)
                                {
                                    if (!target.IsUnderEnemyTurret())
                                    {
                                        R.Cast(Game.CursorPos);
                                    }
                                }
                                break;
                            case 2:
                                if (Menu["combo"]["rset"]["under"].Enabled)
                                {
                                    R.Cast(target.ServerPosition.Extend(Player.ServerPosition, -200));
                                }
                                if (!Menu["combo"]["rset"]["under"].Enabled)
                                {
                                    if (!target.IsUnderEnemyTurret())
                                    {
                                        R.Cast(target.ServerPosition.Extend(Player.ServerPosition, -200));
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            if (useR)
            {
                switch (Menu["combo"]["rset"]["rusage"].As<MenuList>().Value)
                {
                    case 0:
                        switch (Menu["combo"]["rset"]["rmode"].As<MenuList>().Value)
                        {
                            case 0:

                                var targetpos = Prediction.GetPrediction(target, 0.15f).UnitPosition.To2D();
                                Vector2 intersec = new Vector2();
                                for (int i = 450; i >= 0; i = i - 50)
                                {
                                    for (int j = 50; j <= 450; j = j + 50)
                                    {
                                        var vectors =
                                            Vector2Extensions.CircleCircleIntersection(Player.Position.To2D(),
                                                targetpos, i, j);
                                        foreach (var x in vectors)
                                        {
                                            if (!AnyWallInBetween(Player.Position, x))
                                            {
                                                intersec = x;
                                                goto ABC;
                                            }
                                        }
                                    }
                                }
                                ABC:
                                R.Cast(intersec.To3D());
                                break;
                            case 1:
                                if (Menu["combo"]["rset"]["under"].Enabled)
                                {
                                    R.Cast(Game.CursorPos);
                                }
                                if (!Menu["combo"]["rset"]["under"].Enabled)
                                {
                                    if (!target.IsUnderEnemyTurret())
                                    {
                                        R.Cast(Game.CursorPos);
                                    }
                                }
                                break;
                            case 2:
                                if (Menu["combo"]["rset"]["under"].Enabled)
                                {
                                    R.Cast(target.ServerPosition.Extend(Player.ServerPosition, -200));
                                }
                                if (!Menu["combo"]["rset"]["under"].Enabled)
                                {
                                    if (!target.IsUnderEnemyTurret())
                                    {
                                        R.Cast(target.ServerPosition.Extend(Player.ServerPosition, -200));
                                    }
                                }
                                break;
                        }
                        break;
                    case 1:
                        if (target.Health <
                            Player.GetSpellDamage(target, SpellSlot.Q) +
                            Player.GetSpellDamage(target, SpellSlot.W) +
                            Player.GetSpellDamage(target, SpellSlot.E) +
                            Player.GetSpellDamage(target, SpellSlot.R) * 3)
                        {
                            switch (Menu["combo"]["rset"]["rmode"].As<MenuList>().Value)
                            {
                                case 0:

                                    var targetpos = Prediction.GetPrediction(target, 0.15f).UnitPosition.To2D();
                                    Vector2 intersec = new Vector2();
                                    for (int i = 450; i >= 0; i = i - 50)
                                    {
                                        for (int j = 50; j <= 450; j = j + 50)
                                        {
                                            var vectors =
                                                Vector2Extensions.CircleCircleIntersection(Player.Position.To2D(),
                                                    targetpos, i, j);
                                            foreach (var x in vectors)
                                            {
                                                if (!AnyWallInBetween(Player.Position, x))
                                                {
                                                    intersec = x;
                                                    goto ABC;
                                                }
                                            }
                                        }
                                    }
                                    ABC:
                                    R.Cast(intersec.To3D());
                                    break;
                                case 1:
                                    if (Menu["combo"]["rset"]["under"].Enabled)
                                    {
                                        R.Cast(Game.CursorPos);
                                    }
                                    if (!Menu["combo"]["rset"]["under"].Enabled)
                                    {
                                        if (!target.IsUnderEnemyTurret())
                                        {
                                            R.Cast(Game.CursorPos);
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Menu["combo"]["rset"]["under"].Enabled)
                                    {
                                        R.Cast(target.ServerPosition.Extend(Player.ServerPosition, -200));
                                    }
                                    if (!Menu["combo"]["rset"]["under"].Enabled)
                                    {
                                        if (!target.IsUnderEnemyTurret())
                                        {
                                            R.Cast(target.ServerPosition.Extend(Player.ServerPosition, -200));
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {

                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            if (W.Ready && useW && target.IsValidTarget(Menu["combo"]["rangew"].As<MenuSlider>().Value))
            {

                if (target != null)
                {
                    W.Cast(target);
                }
            }
        }

        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool useW = Menu["harass"]["usew"].Enabled;
            bool useE = Menu["harass"]["usee"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            float manapercent = Menu["harass"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (!target.IsValidTarget())
                {
                    return;
                }


                if (E.Ready && useE && target.IsValidTarget(E.Range))
                {
                    if (target != null)
                    {
                        E.Cast(target);
                    }
                }
                if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
                {

                    if (target != null)
                    {
                        Q.Cast(target);
                    }
                }
                if (W.Ready && useW && target.IsValidTarget(W.Range-100))

                {
                    if (target != null)
                    {
                        W.Cast();
                    }
                }
            }
        }
    }
}
