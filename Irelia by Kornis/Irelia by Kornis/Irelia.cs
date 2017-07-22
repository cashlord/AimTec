using System.Net.Configuration;
using System.Resources;
using System.Security.Authentication.ExtendedProtection;

namespace Irelia_By_Kornis
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

    internal class Irelia
    {
        public static Menu Menu = new Menu("Irelia By Kornis", "Irelia by Kornis", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, W, E, R, Flash;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 1000);
            R.SetSkillshot(0.5f, 120, 1600, false, SkillshotType.Line);

        }

        public Irelia()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q in Combo"));
                ComboMenu.Add(new MenuSlider("minq", "^- Min. Q Range", 250, 20, 400));
                ComboMenu.Add(new MenuBool("gapq", "Use Q for Gapclose on Minion"));
                ComboMenu.Add(new MenuBool("usew", "Use W in Combo"));
                ComboMenu.Add(new MenuBool("usee", "Use E in Combo"));
                ComboMenu.Add(new MenuBool("stune", "^- Only if Stuns"));
                ComboMenu.Add(new MenuBool("user", "Use R in Combo "));
                ComboMenu.Add(new MenuList("rusage", "R Usage", new[] {"Always", "Only if Killable"}, 1));
                ComboMenu.Add(new MenuBool("gapr", "Use R on Minions for Q Gapclose"));
                ComboMenu.Add(new MenuBool("sheen", "Sheen Proc."));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                HarassMenu.Add(new MenuBool("useq", "Use Q to Harass"));
                HarassMenu.Add(new MenuBool("gapq", "^- Use Q for Gapclose on Minion"));
                HarassMenu.Add(new MenuBool("usew", "Use W to Harass"));
                HarassMenu.Add(new MenuBool("usee", "Use E to Harass"));

            }
            Menu.Add(HarassMenu);
            var FarmMenu = new Menu("farming", "Farming");
            {
                FarmMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                FarmMenu.Add(new MenuBool("useq", "Use Q to Farm"));
                FarmMenu.Add(new MenuBool("lastq", "^- Only for Last Hit"));
                FarmMenu.Add(new MenuBool("turret", "Don't use Q Under the Turret"));
                FarmMenu.Add(new MenuBool("usew", "Use W to Farm"));
            }
            Menu.Add(FarmMenu);
            var LastMenu = new Menu("lasthit", "Last Hit");
            {
                LastMenu.Add(new MenuSlider("mana", "Mana Manager", 30));
                LastMenu.Add(new MenuBool("useq", "Use Q to Last Hit"));
                LastMenu.Add(new MenuBool("qaa", "^- Don't use Q in AA Range"));
                LastMenu.Add(new MenuBool("turret", "Don't use Q Under the Turret"));
            }
            Menu.Add(LastMenu);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("ksq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kse", "Killsteal with E"));
                KSMenu.Add(new MenuBool("ksr", "Killsteal with R"));
            }
            Menu.Add(KSMenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawdamage", "Draw Damage"));
                DrawMenu.Add(new MenuBool("drawkill", "Draw Minions Killable with Q"));
            }
            Menu.Add(DrawMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            LoadSpells();
            Console.WriteLine("Irelia by Kornis - Loaded");
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
        static double GetQ(Obj_AI_Base target)
        {
            double meow = 0;
            
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 1)
            {
                meow = 20;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 2)
            {
                meow = 50;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 3)
            {
                meow = 80;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 4)
            {
                meow = 110;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 5)
            {
                meow = 140;
            }
            double ad = Player.TotalAttackDamage * 1.2;
            double full = ad + meow;
            double damage = Player.CalculateDamage(target, DamageType.Physical, full);
            return damage;

        }

        private void Render_OnPresent()
        {
            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Crimson);
            }
            if (Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.CornflowerBlue);
            }
            if (Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Crimson);
            }
            if (Menu["drawings"]["drawkill"].Enabled)
            {
                var minion = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range));
                foreach (var m in minion)
                {
                    if (m.IsValidTarget() && !m.IsDead)
                    {
                      
                        if (Q.Ready)
                        {
                            if (GetQ(m) >= m.Health)
                            {
                                Render.Circle(m.Position, 100, 40, Color.GreenYellow);
                            }
                        }
                    }

                }
            }
            if (Menu["drawings"]["drawdamage"].Enabled)
            {

                ObjectManager.Get<Obj_AI_Base>()
                    .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(Q.Range * 2))
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

                            Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 5, true,
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
                case OrbwalkingMode.Lasthit:
                    Lasthit();
                    break;
                case OrbwalkingMode.Laneclear:
                    Clearing();
                    Jungle();
                    break;

            }
            Killsteal();
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

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }


        private void Lasthit()
        {
            float manapercent = Menu["lasthit"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (Menu["lasthit"]["useq"].Enabled)
            {

                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {

                    if (minion.Health <= GetQ(minion))
                    {
                        if (!Menu["lasthit"]["qaa"].Enabled)
                        {
                            if (Menu["lasthit"]["turret"].Enabled)
                            {
                                if (!minion.IsUnderEnemyTurret())
                                {
                                    Q.CastOnUnit(minion);
                                }

                            }
                            if (!Menu["lasthit"]["turret"].Enabled)
                            {

                                Q.CastOnUnit(minion);


                            }
                        }
                        if (Menu["lasthit"]["qaa"].Enabled && minion.Distance(Player) > 200)
                        {
                            if (Menu["lasthit"]["turret"].Enabled)
                            {
                                if (!minion.IsUnderEnemyTurret())
                                {
                                    Q.CastOnUnit(minion);
                                }

                            }
                            if (!Menu["lasthit"]["turret"].Enabled)
                            {

                                Q.CastOnUnit(minion);


                            }
                        }
                    }

                }
            }
            }
        }

        private void Clearing()
        {
            bool useQ = Menu["farming"]["useq"].Enabled;
            bool useW = Menu["farming"]["usew"].Enabled;

            float manapercent = Menu["farming"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (useQ)
                {
                    foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                    {

                        if (minion.IsValidTarget(Q.Range) && minion != null)
                        {
                            if (!Menu["farming"]["turret"].Enabled)
                            {
                                if (!Menu["farming"]["lastq"].Enabled)
                                {
                                    Q.Cast(minion);
                                }
                                if (Menu["farming"]["lastq"].Enabled)
                                {
                                    if (minion.Health <= GetQ(minion))
                                    {
                                        Q.Cast(minion);
                                    }
                                }

                            }
                            if (Menu["farming"]["turret"].Enabled && !minion.IsUnderEnemyTurret())
                            {
                                if (!Menu["farming"]["lastq"].Enabled)
                                {
                                    Q.Cast(minion);
                                }
                                if (Menu["farming"]["lastq"].Enabled)
                                {
                                    if (minion.Health <= GetQ(minion))
                                    {
                                        Q.Cast(minion);
                                    }
                                }

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
                float manapercent = Menu["farming"]["mana"].As<MenuSlider>().Value;
                if (manapercent < Player.ManaPercent())
                {
                    if (useQ && jungleTarget.IsValidTarget(Q.Range))
                    {
                        Q.Cast(jungleTarget);
                    }
                    if (useW && jungleTarget.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }
                }

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
                var bestTarget = GetBestKillableHero(Q, DamageType.Physical, false);
                if (bestTarget != null &&
                    GetQ(bestTarget) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(bestTarget);
                }
            }
            if (E.Ready &&
                Menu["killsteal"]["kse"].Enabled)
            {
                var bestTarget = GetBestKillableHero(E, DamageType.Physical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.E) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(bestTarget);
                }
            }
            if (R.Ready &&
                Menu["killsteal"]["ksr"].Enabled)
            {
                var bestTarget = GetBestKillableHero(R, DamageType.Physical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.R) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(R.Range))
                {
                    R.Cast(bestTarget);
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
            bool stunE = Menu["combo"]["stune"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;

            bool gapQ = Menu["combo"]["gapq"].Enabled;
            bool gapR = Menu["combo"]["gapr"].Enabled;

            float minq = Menu["combo"]["minq"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q.Range * 2);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {

                if (target != null && minq < target.Distance(Player))
                {
                    Q.Cast(target);
                }
            }
            if (gapQ)
            {
                if (target.Distance(Player) > Q.Range)
                {
                    
                    foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                    {
 
                        if (minion.IsValidTarget(Q.Range) && minion != null)
                        {
                            if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost * 2 &&
                                GetQ(minion) >= minion.Health &&
                                minion.Distance(Player) <= Q.Range && minion.Distance(target) <= Q.Range &&
                                target.Distance(Player) > Q.Range)
                            {
                                Q.CastOnUnit(minion);
                            }
                            if (gapR)
                            {
                                var minionR =
                                    ObjectManager.Get<Obj_AI_Minion>()
                                        .Where(
                                            x =>
                                                x.IsValidTarget() && x.Distance(Player) < Q.Range && x.CountEnemyHeroesInRange(Q.Range) >= 1)
                                        .FirstOrDefault(
                                            x =>
                                                x.Health - Player.GetSpellDamage(x, SpellSlot.R) * 2 < Player.GetSpellDamage(x, SpellSlot.Q));
                                switch (Menu["combo"]["rusage"].As<MenuList>().Value)
                                {
                                    case 0:
                                        if (minionR.Health > GetQ(minionR) &&
                                            Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost * 2 +
                                            Player.SpellBook.GetSpell(SpellSlot.R).Cost &&
                                            Player.Distance(target) > Q.Range && minionR.Distance(Player) <= Q.Range &&
                                            minionR.Distance(target) <= Q.Range)
                                        {
                                            R.CastOnUnit(minionR);

                                        }
                                        break;
                                    case 1:
                                        if (minionR.Distance(Player) <= Q.Range &&
                                            minionR.Distance(target) <= Q.Range &&
                                            target.Distance(Player) > Q.Range)
                                        {
                                            if (minionR.Health > GetQ(minionR) &&
                                                Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost * 2 +
                                                Player.SpellBook.GetSpell(SpellSlot.R).Cost &&
                                                Player.Distance(target) > Q.Range)
                                            {

                                                R.CastOnUnit(minionR);

                                            }
                                        }
                                        break;

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
            if (E.Ready && useE && target.IsValidTarget(E.Range))
            {

                if (target != null)
                {
                    if (!stunE)
                    {
                        E.Cast(target);
                    }
                    if (stunE && target.Health > Player.Health)
                    {
                        E.Cast(target);
                    }
                }
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range - 50))
            {

                if (target != null)
                {
                    switch (Menu["combo"]["rusage"].As<MenuList>().Value)
                    {
                        case 0:
                            if (Menu["combo"]["sheen"].Enabled)
                            {
                                if (!Player.HasBuff("Sheen") && Player.Distance(target) < 250)
                                {
                                    R.Cast(target);
                                }
                            }
                            if (!Menu["combo"]["sheen"].Enabled)
                            {

                                R.Cast(target);
                            }
                            break;
                        case 1:
                            if (Player.GetSpellDamage(target, SpellSlot.R) * 3 +
                                Player.GetSpellDamage(target, SpellSlot.E)
                                + GetQ(target) > target.Health)
                            {
                                if (Menu["combo"]["sheen"].Enabled)
                                {
                                    if (!Player.HasBuff("Sheen") && Player.Distance(target) < 250)
                                    {
                                        R.Cast(target);
                                    }
                                }
                                if (!Menu["combo"]["sheen"].Enabled)
                                {

                                    R.Cast(target);
                                }
                                

                            }
                            break;
                    }
                }
            }
        }

        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool gapQ = Menu["harass"]["gapq"].Enabled;
            bool useW = Menu["harass"]["usew"].Enabled;
            bool useE = Menu["harass"]["usee"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(Q.Range*2);
            float manapercent = Menu["harass"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (!target.IsValidTarget())
                {
                    return;
                }

                if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
                {

                    if (target != null)
                    {
                        Q.Cast(target);
                    }
                }
                if (gapQ)
                {
                    if (target.Distance(Player) > Q.Range)
                    {
                        foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                        {

                            if (minion.IsValidTarget(Q.Range) && minion != null)
                            {
                                if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost * 2 &&
                                    GetQ(minion) >= minion.Health &&
                                    minion.Distance(Player) <= Q.Range && minion.Distance(target) <= Q.Range &&
                                    target.Distance(Player) > Q.Range)
                                {
                                    Q.CastOnUnit(minion);
                                }

                            }
                        }
                    }
                }
                if (E.Ready && useE && target.IsValidTarget(E.Range))
                {
                    if (target != null)
                    {
                        E.Cast(target);
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
