using System.Net.Configuration;
using System.Resources;
using System.Security.Authentication.ExtendedProtection;

namespace Cassiopeia_By_Kornis
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
    using Aimtec.SDK.Prediction.Health;

    internal class Cassiopeia
    {
        public static Menu Menu = new Menu("Cassiopeia By Kornis", "Cassiopeia by Kornis", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        public static Spell Q, W, E, R, Flash;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 825);
            Q.SetSkillshot(0.83f, 100, float.MaxValue, false, SkillshotType.Circle, false, HitChance.None);
            W.SetSkillshot(1.2f, 200, float.MaxValue, false, SkillshotType.Circle, false, HitChance.None);
            R.SetSkillshot(0.4f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.Cone);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner1, 425);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner2, 425);

        }

        public Cassiopeia()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            var QSet = new Menu("qset", "Q Settings");
            {
                QSet.Add(new MenuBool("useq", "Use Q in Combo"));
                QSet.Add(new MenuBool("qpoison", "^- Only if NOT POISONED", false));
                QSet.Add(new MenuBool("autoq", "Auto Q on Dash", true));
            }
            var WSet = new Menu("wset", "W Settings");
            {
                WSet.Add(new MenuBool("usew", "Use W in Combo"));
                WSet.Add(new MenuBool("startw", "Start Combo with W", false));
                WSet.Add(new MenuSlider("rangew", "W Max Range", 800, 400, 800));

            }
            var ESet = new Menu("eset", "E Settings");
            {
                ESet.Add(new MenuBool("usee", "Use E in Combo"));
                ESet.Add(new MenuBool("epoison", "^- Only if POISONED", false));
            }
            var RSet = new Menu("rset", "R Settings");
            {
                RSet.Add(new MenuBool("user", "Use R in Combo"));
                RSet.Add(new MenuList("rmode", "R Mode", new[] { "At X Health", "If Killable" }, 0));
                RSet.Add(new MenuSlider("waster", "Don't waste R if Enemy HP lower than", 100, 0, 500));
                RSet.Add(new MenuSlider("hpr", "R if Target has Health Percent", 60, 1, 100));
                RSet.Add(new MenuSlider("hitr", "Min. Enemies to Hit", 1, 1, 5));
                //RSet.Add(new MenuSlider("stunr", "Min. Enemies to Stun", 1, 1, 5));
                RSet.Add(new MenuSlider("ranger", "R Range", 750, 125, 825));
                RSet.Add(new MenuBool("facer", "Only R if Facing", false));
                RSet.Add(new MenuKeyBind("key", "R Flash:", KeyCode.T, KeybindType.Press));
                RSet.Add(new MenuKeyBind("semi", "Semi-Manual R:", KeyCode.G, KeybindType.Press));

            }
            Menu.Add(ComboMenu);
            ComboMenu.Add(QSet);
            ComboMenu.Add(WSet);
            ComboMenu.Add(ESet);
            ComboMenu.Add(RSet);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                HarassMenu.Add(new MenuBool("useq", "Use Q to Harass"));
                HarassMenu.Add(new MenuBool("usee", "Use E to Harass"));
                HarassMenu.Add(new MenuBool("epoison", "^- Only if POISONED"));

            }
            Menu.Add(HarassMenu);
            var FarmMenu = new Menu("farming", "Farming");
            FarmMenu.Add(new MenuKeyBind("mode", "Mode Change", KeyCode.Z, KeybindType.Toggle));
            var Pushing = new Menu("push", "Pushing");
            {
                Pushing.Add(new MenuSlider("mana", "Mana Manager", 50));
                Pushing.Add(new MenuBool("useq", "Use Q to Farm"));
                Pushing.Add(new MenuBool("usee", "Use E to Farm"));
                Pushing.Add(new MenuBool("epoison", "^- Only if POISONED"));
                Pushing.Add(new MenuBool("disable", "Disable AA"));
            }
            var Passive = new Menu("passive", "Passive");
            {
                Passive.Add(new MenuBool("usee", "Use E"));
            }
            var JungleClear = new Menu("jungle", "Jungle Clear");
            {
                JungleClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                JungleClear.Add(new MenuBool("useq", "Use Q in Jungle"));
                JungleClear.Add(new MenuBool("usee", "Use E in Jungle"));
            }
            var lasthit = new Menu("lasthit", "Last Hit");
            {
                lasthit.Add(new MenuBool("laste", "Use E"));
            }
            Menu.Add(lasthit);
            Menu.Add(FarmMenu);
            FarmMenu.Add(Pushing);
            FarmMenu.Add(Passive);
            FarmMenu.Add(JungleClear);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("ksq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kse", "Killsteal with E"));
                KSMenu.Add(new MenuBool("ksr", "Killsteal with R"));
                KSMenu.Add(new MenuSlider("waster", "Don't waste R if Enemy HP lower than", 100, 0, 500));
            }
            Menu.Add(KSMenu);
            var miscmenu = new Menu("misc", "Misc.");
            {
                miscmenu.Add(new MenuBool("disable", "Disable AA"));
                miscmenu.Add(new MenuSlider("level", "At what Level disable AA", 6, 1, 18));
                miscmenu.Add(new MenuKeyBind("aakey", "AA Disable Key: (Combo)", KeyCode.Space, KeybindType.Press));
            }
            Menu.Add(miscmenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawflash", "Draw R-Flash Range"));
                DrawMenu.Add(new MenuBool("drawdamage", "Draw Damage"));
                DrawMenu.Add(new MenuBool("drawkill", "Draw Killable with Minions with E"));
                DrawMenu.Add(new MenuBool("drawtoggle", "Draw Farm Toggle"));
            }
            Menu.Add(DrawMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            LoadSpells();
            Console.WriteLine("Cassiopeia by Kornis - Loaded");
        }

        public static readonly List<string> SpecialChampions = new List<string> { "Annie", "Jhin" };
        public static int SxOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 1 : 10;
        }

        public static int SyOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 3 : 20;
        }


        static double GetE(Obj_AI_Base target)
        {
            int meow = 0;
            int hello = 48 + (4 * Player.Level);
            double test = 0;
            if (Player.SpellBook.GetSpell(SpellSlot.E).Level == 1)
            {
                meow = 10;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.E).Level == 2)
            {
                meow = 40;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.E).Level == 3)
            {
                meow = 70;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.E).Level == 4)
            {
                meow = 100;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.E).Level == 5)
            {
                meow = 130;
            }

            if (!target.HasBuffOfType(BuffType.Poison))
            {
                double ap = Player.TotalAbilityDamage * 0.1;
                double full = ap + hello;
                double damage = Player.CalculateDamage(target, DamageType.Magical, full);
                test = damage;
            }
            if (target.HasBuffOfType(BuffType.Poison))
            {
                double ap = Player.TotalAbilityDamage * 0.1;
                double ap2 = Player.TotalAbilityDamage * 0.35;
                double full = ap + hello + meow + ap2;
                double damage = Player.CalculateDamage(target, DamageType.Magical, full);
                test = damage;
            }
            return test;

        }
        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;
            if (Menu["drawings"]["drawtoggle"].Enabled)
            {
                if (Menu["farming"]["mode"].Enabled)
                {
                    Render.Text(xaOffset - 50, yaOffset + 10, Color.HotPink, "Farm Mode: Pushing",
                        RenderTextFlags.VerticalCenter);
                }
                if (!Menu["farming"]["mode"].Enabled)
                {
                    Render.Text(xaOffset - 50, yaOffset + 10, Color.HotPink, "Farm Mode: Passive",
                        RenderTextFlags.VerticalCenter);
                }
            }
            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.CornflowerBlue);
            }
            if (Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, Menu["combo"]["wset"]["rangew"].As<MenuSlider>().Value, 40, Color.Crimson);
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
                Render.Circle(Player.Position, Menu["combo"]["rset"]["ranger"].As<MenuSlider>().Value + 410, 40, Color.HotPink);
            }
            if (Menu["drawings"]["drawkill"].Enabled)
            {
                var minion = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range));
                foreach (var m in minion)
                {
                    if (m.IsValidTarget() && !m.IsDead)
                    {

                        if (E.Ready)
                        {
                            if (HealthPrediction.Implementation.GetPrediction(m, (int)(0.2 * 1000f)) <= GetE(m))
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
                                (float)(barPos.X + (unit.Health >
                                                     Player.GetSpellDamage(unit, SpellSlot.Q) +
                                                     GetE(unit) * 3 +
                                                     Player.GetSpellDamage(unit, SpellSlot.R)
                                             ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) +
                                                         GetE(unit) * 3 +
                                                         Player.GetSpellDamage(unit, SpellSlot.R))) /
                                                        unit.MaxHealth * 100 / 100)
                                             : 0));

                            Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 8, true,
                                unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) +
                                GetE(unit) * 3 +
                                Player.GetSpellDamage(unit, SpellSlot.R)
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
            if (Menu["combo"]["qset"]["autoq"].Enabled)
            {
                var bestTarget = GetBestEnemyHeroTargetInRange(Q.Range);
                if (bestTarget != null &&
                    bestTarget.IsValidTarget(Q.Range))
                {

                    var pred = Q.GetPrediction(bestTarget);
                    if (pred.HitChance == HitChance.Impossible)
                    {

                        Q.Cast(pred.CastPosition);
                    }
                }
            }
            if (Orbwalker.Mode.Equals(OrbwalkingMode.Lasthit))
            {
                if (E.Ready && Player.Mana > Player.SpellBook.GetSpell(SpellSlot.E).Cost)
                {
                    Orbwalker.AttackingEnabled = false;
                }
                else Orbwalker.AttackingEnabled = true;
            }
            if (Orbwalker.Mode.Equals(OrbwalkingMode.Laneclear))
            {
                if (Menu["farming"]["mode"].Enabled && Menu["farming"]["push"]["disable"].Enabled)
                {
                    Orbwalker.AttackingEnabled = false;
                }
                else Orbwalker.AttackingEnabled = true;
            }
            if (Menu["misc"]["aakey"].Enabled)
            {
                if (Player.Mana > 100 && Menu["misc"]["disable"].Enabled && Menu["misc"]["level"].As<MenuSlider>().Value <= Player.Level)
                {
                    Orbwalker.AttackingEnabled = false;
                }
            }
            if (!Menu["misc"]["disable"].Enabled || Player.Mana < 100 || !Menu["misc"]["aakey"].Enabled && (!Orbwalker.Mode.Equals(OrbwalkingMode.Lasthit) && !Menu["misc"]["aakey"].Enabled && !Orbwalker.Mode.Equals(OrbwalkingMode.Laneclear)))
            {
                Orbwalker.AttackingEnabled = true;
            }
            if (Menu["combo"]["rset"]["key"].Enabled)
            {
                FlashR();
            }

            if (Menu["combo"]["rset"]["semi"].Enabled)
            {
                SemiR();
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

        private void FlashR()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            var target = GetBestEnemyHeroTargetInRange(Menu["combo"]["rset"]["ranger"].As<MenuSlider>().Value + 410);
            if (R.Ready)
            {
                if (Flash.Ready && Flash != null && target.IsValidTarget())
                {
                    if (target.IsValidTarget(Menu["combo"]["rset"]["ranger"].As<MenuSlider>().Value + 410))
                    {
                        if (target.Distance(Player) > R.Range)
                        {
                            if (R.Cast(target.ServerPosition))
                            {
                                DelayAction.Queue(200 + Game.Ping, () =>
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

        private void Lasthit()
        {
            if (Menu["lasthit"]["laste"].Enabled)
            {


                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
                {
                    if (HealthPrediction.Implementation.GetPrediction(minion, (int)(0.2 * 1000f)) <= GetE(minion))
                    {

                        E.CastOnUnit(minion);

                    }
                }
            }
        }
        private void Clearing()
        {
            if (Menu["farming"]["mode"].Enabled)
            {
                bool useQ = Menu["farming"]["push"]["useq"].Enabled;
                bool useE = Menu["farming"]["push"]["usee"].Enabled;
                bool poisonE = Menu["farming"]["push"]["epoison"].Enabled;
                float manapercent = Menu["farming"]["push"]["mana"].As<MenuSlider>().Value;
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
                    if (useE)
                    {
                        foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
                        {

                            if (minion.IsValidTarget(E.Range) && minion != null)
                            {
                                if (poisonE && minion.HasBuffOfType(BuffType.Poison))
                                {
                                    E.CastOnUnit(minion);
                                }
                                if (!poisonE)
                                {
                                    E.CastOnUnit(minion);
                                }

                            }
                        }
                    }
                }
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
                {
                    if (HealthPrediction.Implementation.GetPrediction(minion, (int)(0.2 * 1000f)) <= GetE(minion))
                    {

                        E.CastOnUnit(minion);

                    }
                }
            }
        
            if (!Menu["farming"]["mode"].Enabled)
            {
                bool useE = Menu["farming"]["passive"]["usee"].Enabled;

 
                    if (useE)
                    {
                        foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
                        {
                            if (HealthPrediction.Implementation.GetPrediction(minion, (int)(0.2 * 1000f)) <= GetE(minion))
                            {

                                E.CastOnUnit(minion);

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
            foreach (var jungleTarget in GameObjects.Jungle.Where(m => m.IsValidTarget(Q.Range)).ToList())
            {
                if (!jungleTarget.IsValidTarget() || !jungleTarget.IsValidSpellTarget())
                {
                    return;
                }
                bool useQ = Menu["farming"]["jungle"]["useq"].Enabled;
                bool useW = Menu["farming"]["jungle"]["usee"].Enabled;
                float manapercent = Menu["farming"]["jungle"]["mana"].As<MenuSlider>().Value;
                if (manapercent < Player.ManaPercent())
                {
                    if (useQ && jungleTarget.IsValidTarget(Q.Range))
                    {
                        Q.Cast(jungleTarget);
                    }
                    if (useW && jungleTarget.IsValidTarget(E.Range))
                    {
                        E.CastOnUnit(jungleTarget);
                    }
                }

            }
        }
        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }

      
        public Vector3 End = Vector3.Zero;
        public static float GetAngleByDegrees(float degrees)
        {
            return (float)(degrees * Math.PI / 180);
        }
        public Geometry.Sector UltimateCone(Vector2 pos)
        {
            return new Geometry.Sector(
                (Vector2)Player.Position.Extend(End, -Player.BoundingRadius),
                pos,
                GetAngleByDegrees(80f),
                R.Range);
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
            if (R.Ready &&
    Menu["killsteal"]["ksr"].Enabled)
            {
                var bestTarget = GetBestKillableHero(R, DamageType.Magical, false);
                if (bestTarget != null && Player.GetSpellDamage(bestTarget, SpellSlot.R) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(Menu["combo"]["rset"]["ranger"].As<MenuSlider>().Value) && bestTarget.Health >= Menu["killsteal"]["waster"].As<MenuSlider>().Value)
                {
                    Geometry.Sector cone;
                    foreach (var enemy in GetBestEnemyHeroesTargetsInRange(R.Range))
                    {

                        cone = UltimateCone((Vector2)enemy.ServerPosition);
                        if (GameObjects.EnemyHeroes.Count(t2 => t2.IsValidTarget() && cone.IsInside((Vector2)t2.ServerPosition)) >= Menu["combo"]["rset"]["hitr"].As<MenuSlider>().Value)
                        {
                            R.Cast(enemy);
                        }
                    }
                }
            }
            


        }
        
        public static List<Obj_AI_Hero> GetBestEnemyHeroesTargets()
        {
            return GetBestEnemyHeroesTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Hero> GetBestEnemyHeroesTargetsInRange(float range)
        {
            return TargetSelector.Implementation.GetOrderedTargets(range);
        }
        private void SemiR()
        {
            if (R.Ready)
            {
                Geometry.Sector cone;
                foreach (var enemy in GetBestEnemyHeroesTargetsInRange(R.Range))
                {

                    cone = UltimateCone((Vector2)enemy.ServerPosition);
                    if (GameObjects.EnemyHeroes.Count(t2 => t2.IsValidTarget() && cone.IsInside((Vector2)t2.ServerPosition)) >= Menu["combo"]["rset"]["hitr"].As<MenuSlider>().Value)
                    {
                        R.Cast(enemy);  
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

            bool useQ = Menu["combo"]["qset"]["useq"].Enabled;
            bool poisonQ = Menu["combo"]["qset"]["qpoison"].Enabled;
            bool useW = Menu["combo"]["wset"]["usew"].Enabled;
            bool useE = Menu["combo"]["eset"]["usee"].Enabled;
            bool poisonE = Menu["combo"]["eset"]["epoison"].Enabled;
            bool useR = Menu["combo"]["rset"]["user"].Enabled;
            float waste = Menu["combo"]["rset"]["waster"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (R.Ready && useR && target.IsValidTarget(Menu["combo"]["rset"]["ranger"].As<MenuSlider>().Value))
            {

                if (target != null)
                {
                    switch (Menu["combo"]["rset"]["rmode"].As<MenuList>().Value)
                    {
                        case 0:
                            if (target.HealthPercent() <= Menu["combo"]["rset"]["hpr"].As<MenuSlider>().Value && target.Health >= Menu["combo"]["rset"]["waster"].As<MenuSlider>().Value)
                            {
                                if (Menu["combo"]["rset"]["facer"].Enabled)
                                {
                                    if (target.IsFacing(Player))
                                    {
                                        Geometry.Sector cone;
                                        foreach (var enemy in GetBestEnemyHeroesTargetsInRange(R.Range))
                                        {

                                            cone = UltimateCone((Vector2)enemy.ServerPosition);
                                            if (GameObjects.EnemyHeroes.Count(t2 => t2.IsValidTarget() && cone.IsInside((Vector2)t2.ServerPosition)) >= Menu["combo"]["rset"]["hitr"].As<MenuSlider>().Value)
                                            {
                                                R.Cast(enemy);
                                            }
                                        }
                                    }

                                }
                                if (!Menu["combo"]["rset"]["facer"].Enabled)
                                {
                                    Geometry.Sector cone;
                                    foreach (var enemy in GetBestEnemyHeroesTargetsInRange(R.Range))
                                    {

                                        cone = UltimateCone((Vector2)enemy.ServerPosition);
                                        if (GameObjects.EnemyHeroes.Count(t2 => t2.IsValidTarget() && cone.IsInside((Vector2)t2.ServerPosition)) >= Menu["combo"]["rset"]["hitr"].As<MenuSlider>().Value)
                                        {
                                            R.Cast(enemy);
                                        }
                                    }
                                }
                            }
                            break;
                        case 1:
                            if (target.Health <= GetE(target) * 3 + Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.R) && target.Health >= Menu["combo"]["rset"]["waster"].As<MenuSlider>().Value)
                            {

                                if (Menu["combo"]["rset"]["facer"].Enabled)
                                {
                                    if (target.IsFacing(Player))
                                    {
                                        Geometry.Sector cone;
                                        foreach (var enemy in GetBestEnemyHeroesTargetsInRange(R.Range))
                                        {

                                            cone = UltimateCone((Vector2)enemy.ServerPosition);
                                            if (GameObjects.EnemyHeroes.Count(t2 => t2.IsValidTarget() && cone.IsInside((Vector2)t2.ServerPosition)) >= Menu["combo"]["rset"]["hitr"].As<MenuSlider>().Value)
                                            {
                                                R.Cast(enemy);
                                            }
                                        }
                                    }

                                }
                                if (!Menu["combo"]["rset"]["facer"].Enabled)
                                {
                                    Geometry.Sector cone;
                                    foreach (var enemy in GetBestEnemyHeroesTargetsInRange(R.Range))
                                    {

                                        cone = UltimateCone((Vector2)enemy.ServerPosition);
                                        if (GameObjects.EnemyHeroes.Count(t2 => t2.IsValidTarget() && cone.IsInside((Vector2)t2.ServerPosition)) >= Menu["combo"]["rset"]["hitr"].As<MenuSlider>().Value)
                                        {
                                            R.Cast(enemy);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            if (!Menu["combo"]["wset"]["startw"].Enabled)
            {
                if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
                {

                    if (target != null)
                    {
                        if (!poisonQ)
                        {
                            Q.Cast(target);
                        }
                        if (poisonQ && !target.HasBuffOfType(BuffType.Poison))
                        {
                            Q.Cast(target);
                        }
                    }
                }
                if (E.Ready && useE && target.IsValidTarget(E.Range))
                {

                    if (target != null)
                    {
                        if (!poisonE)
                        {
                            E.CastOnUnit(target);
                        }
                        if (poisonE && target.HasBuffOfType(BuffType.Poison))
                        {
                            E.CastOnUnit(target);
                        }
                    }
                }
                if (W.Ready && useW && target.IsValidTarget(Menu["combo"]["wset"]["rangew"].As<MenuSlider>().Value))
                {

                    if (target != null && target.Distance(Player) >= 400)
                    {
                        W.Cast(target);
                    }
                }
            }
            if (Menu["combo"]["wset"]["startw"].Enabled)
            {

                if (W.Ready && useW && target.IsValidTarget(Menu["combo"]["wset"]["rangew"].As<MenuSlider>().Value))
                {

                    if (target != null && target.Distance(Player) >= 400)
                    {
                        W.Cast(target);
                    }
                }
                if (E.Ready && useE && target.IsValidTarget(E.Range))
                {

                    if (target != null)
                    {
                        if (!poisonE)
                        {
                            E.CastOnUnit(target);
                        }
                        if (poisonE && target.HasBuffOfType(BuffType.Poison))
                        {
                            E.CastOnUnit(target);
                        }
                    }
                }
                if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
                {

                    if (target != null)
                    {
                        if (!poisonQ)
                        {
                            Q.Cast(target);
                        }
                        if (poisonQ && !target.HasBuffOfType(BuffType.Poison))
                        {
                            Q.Cast(target);
                        }
                    }
                }

            }
            
        }
        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool useE = Menu["harass"]["usee"].Enabled;
            bool poisonE = Menu["harass"]["epoison"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);
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
                if (E.Ready && useE && target.IsValidTarget(E.Range))
                {
                    if (target != null)
                    {
                        if (!poisonE)
                        {
                            E.CastOnUnit(target);
                        }

                        if (poisonE && target.HasBuffOfType(BuffType.Poison))
                            {
                            E.CastOnUnit(target);
                        }
                    }
                }
            }
        }
    }
}
// にゃにゃ