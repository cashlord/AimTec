namespace Kassadin_By_Kornis
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

    internal class Kassadin
    {
        public static Menu Menu = new Menu("Kassadin By Kornis", "Kassadin by Kornis", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, W, E, R, Flash;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 290f);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 500);
           
            E.SetSkillshot(0.3f, (float)(80f + Math.PI / 180f), 1000f, false, SkillshotType.Cone);
            R.SetSkillshot(0.25f, 150, 1500, false, SkillshotType.Circle);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner1, 450);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner2, 450);
        }

        public Kassadin()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("waa", "^- Only for AA Reset"));
                ComboMenu.Add(new MenuBool("usee", "Use E "));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuBool("turret", "Don't R Under the Turret"));
                ComboMenu.Add(new MenuSlider("hp", "Don't use R if my HP lower than", 20));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                HarassMenu.Add(new MenuBool("useq", "Use Q"));
                HarassMenu.Add(new MenuBool("usew", "Use W"));
                HarassMenu.Add(new MenuBool("waa", "^- Only for AA Reset"));
                HarassMenu.Add(new MenuBool("usee", "Use E "));

            }
            Menu.Add(HarassMenu);
            var FarmMenu = new Menu("farming", "Farming");
            {
                FarmMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                FarmMenu.Add(new MenuBool("useq", "Use Q"));
                FarmMenu.Add(new MenuBool("usew", "Use W"));
                FarmMenu.Add(new MenuBool("usee", "Use E"));
 
         
            }
            Menu.Add(FarmMenu);
            var LastMenu = new Menu("lasthit", "Last Hit");
            {
                LastMenu.Add(new MenuBool("lastq", "Use Q to Last Hit"));
                LastMenu.Add(new MenuBool("qaa", "^- Don't use if in AA Range"));
                LastMenu.Add(new MenuBool("lastw", "Use W to Last Hit"));
            }
            Menu.Add(LastMenu);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("ksq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kse", "Killsteal with E"));
                KSMenu.Add(new MenuBool("ksr", "Killsteal with R"));
                KSMenu.Add(new MenuBool("ksrgap", "Gapclose with R for Q"));
            }
            Menu.Add(KSMenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawburst", "Draw Burst Range"));
               DrawMenu.Add(new MenuBool("drawDamage", "Draw Damage"));
            }
            Menu.Add(DrawMenu);
            var FleeMenu = new Menu("flee", "Flee");
            {
                FleeMenu.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.G, KeybindType.Press));

                FleeMenu.Add(new MenuBool("useR", "Use R to Flee"));
            }
            Menu.Add(FleeMenu);
            var MiscMenu = new Menu("misc", "Misc.");
            {
                MiscMenu.Add(new MenuBool("InterruptQ", "Interrupt with Q"));
            }
            //Menu.Add(MiscMenu);
            var BurstMenu = new Menu("burst", "Burst");
            {
                BurstMenu.Add(new MenuKeyBind("key", "Burst Key:", KeyCode.T, KeybindType.Press));
                BurstMenu.Add(new MenuBool("waitE", "Wait for E"));
            }
            Menu.Add(BurstMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            //Events.OnInterruptableTarget += OnInterruptableTarget;

            Orbwalker.PostAttack += OnPostAttack;
            LoadSpells();
            Console.WriteLine("Kassadin by Kornis - Loaded");
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
        private void Render_OnPresent()
        {

            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 50, Color.Crimson);
            }

            if (Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 50, Color.LightGreen);
            }
            if (Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 50, Color.Crimson);
            }
            if (Menu["drawings"]["drawburst"].Enabled)
            {
                Render.Circle(Player.Position, R.Range + 500, 50, Color.LightGreen);
            }
            if (Menu["drawings"]["drawDamage"].Enabled)
            {
                
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(Q.Range+R.Range))
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
                            var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R) + Player.GetSpellDamage(unit, SpellSlot.W)
                                                            ? width * ((unit.Health - Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R) + Player.GetSpellDamage(unit, SpellSlot.W)) / unit.MaxHealth * 100 / 100)
                                                            : 0));

                            Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R) + Player.GetSpellDamage(unit, SpellSlot.W) ? Color.GreenYellow : Color.Orange);
                            
                        });
            }
        }

      /* public void OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs args)
        {
            if (Player.IsDead || !args.Sender.IsValidTarget())
            {
                return;
            }
            if (args.Sender.IsValidTarget(Q.Range)
                && Menu["misc"]["InterruptQ"].Enabled)
            {
                Q.CastOnUnit(args.Sender);
            }
        }*/
        public void OnPostAttack(object sender, PostAttackEventArgs args)
        {
             var heroTarget = args.Target as Obj_AI_Hero;
            if (Orbwalker.Mode.Equals(OrbwalkingMode.Combo))
            {
                if (!Menu["combo"]["waa"].Enabled)
                {
                    return;
                }
                Obj_AI_Hero hero = args.Target as Obj_AI_Hero;
                if (hero == null || !hero.IsValid || !hero.IsEnemy)
                {
                    return;
                }
                W.Cast();
            }
            if (Orbwalker.Mode.Equals(OrbwalkingMode.Mixed))
            {
                if (!Menu["harass"]["waa"].Enabled)
                {
                    return;
                }
                Obj_AI_Hero hero = args.Target as Obj_AI_Hero;
                if (hero == null || !hero.IsValid || !hero.IsEnemy)
                {
                    return;
                }
                W.Cast();
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
            if (Menu["flee"]["key"].Enabled)
            {
                Flee();
            }
            if (Menu["burst"]["key"].Enabled)
            {
                Burst();
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
            float manapercent = Menu["farming"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (useQ)
                {
                    foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                    {

                        if (minion.IsValidTarget(Q.Range) && minion != null)
                        {
                            Q.CastOnUnit(minion);
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
                            Orbwalker.ForceTarget(minion);
                            
                        }
                    }
                }
                if (useE)
                {
                    foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
                    {

                        if (minion.IsValidTarget(E.Range) && minion != null)
                        {
                            E.Cast(minion);
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
            return GameObjects.Jungle.Concat(GameObjects.JungleSmall).Where(m => m.IsValidTarget(range)).ToList();
        }

        private void Jungle()
        {
            foreach (var jungleTarget in GameObjects.Jungle.Where(m => m.IsValidTarget(Q.Range)).ToList())
            { 
                if (!jungleTarget.IsValidTarget() ||
                    !GetGenericJungleMinionsTargets().Contains(jungleTarget))
                {
                    return;
                }
                bool useQ = Menu["farming"]["useq"].Enabled;
                bool useW = Menu["farming"]["usew"].Enabled;
                bool useE = Menu["farming"]["usee"].Enabled;
                float manapercent = Menu["farming"]["mana"].As<MenuSlider>().Value;
                if (manapercent < Player.ManaPercent())
                {
                    if (useQ && jungleTarget.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(jungleTarget);
                    }
                    if (useW && jungleTarget.IsValidTarget(W.Range))
                    {
                        W.Cast();

                        Orbwalker.ForceTarget(jungleTarget);
                    }
                    if (useE && jungleTarget.IsValidTarget(E.Range))
                    {
                        E.Cast(jungleTarget);
                    }
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
                        if (!Menu["lasthit"]["qaa"].Enabled)
                        {
                            
                            Q.CastOnUnit(minion);
                        }
                        if (Menu["lasthit"]["qaa"].Enabled)
                        {
                            if (minion.Distance(Player) > 300)
                            {

                                Q.CastOnUnit(minion);
                            }
                        }
                    }
                }
            }
            if (Menu["lasthit"]["lastw"].Enabled)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(250))
                {
                    if (minion.Health <= Player.GetSpellDamage(minion, SpellSlot.W) && minion.IsValidTarget(250))
                    {
                        W.Cast();
                        Orbwalker.ForceTarget(minion);
                    }
                }
            }

        }


        private void Flee()
        {
            bool user = Menu["flee"]["useR"].Enabled;
            if (user)
            {
                Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                R.Cast(Game.CursorPos);
            }
        }
        private void Burst()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            var target = GetBestEnemyHeroTargetInRange(R.Range+ 550);
            bool usee = Menu["burst"]["waitE"].Enabled;
            if (usee && E.Ready)
            {
                if (R.Ready && Flash.Ready && Flash != null && target.IsValidTarget())
                {
                    if (target.IsValidTarget(R.Range + 500))
                    {
                        if (target.Distance(Player) > R.Range)
                        {
                            if (R.Cast(target.Position))
                            {
                                Flash.Cast(target.Position);
                            }
                        }
                    }
                }
            }
            if (!usee)
            {
                if (R.Ready && Flash.Ready && Flash != null && target.IsValidTarget())
                {
                    if (target.IsValidTarget(R.Range + 500))
                    {
                        if (target.Distance(Player) > R.Range)
                        {
                            if (R.Cast(target.Position))
                            {
                                Flash.Cast(target.Position);
                            }
                        }
                    }
                }
            }
            if (target.IsValidTarget(W.Range) && target.IsValidTarget() && W.Ready)
            {
                if (W.Cast())
                {
                    Player.IssueOrder(OrderType.AttackUnit, target);
                }
            }
            if (target.IsValidTarget(E.Range) && target.IsValidTarget())
            {
                E.Cast(target);
            }
            if (target.IsValidTarget(Q.Range) && target.IsValidTarget())
            {
                Q.CastOnUnit(target);
            }
            if (target.IsValidTarget(R.Range) && target.IsValidTarget())
            {
                R.Cast(target);
            }
           
        }


        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True, bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }
        public static Obj_AI_Hero GetRGAP(DamageType damageType = DamageType.True, bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(Q.Range + R.Range).FirstOrDefault(t => t.IsValidTarget());
        }
        private void Killsteal()
        {
            if (Q.Ready &&
                Menu["killsteal"]["ksq"].Enabled)
            {
                var bestTarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health)
                {
                    Q.CastOnUnit(bestTarget);
                }
            }
            if (E.Ready &&
                Menu["killsteal"]["kse"].Enabled)
            {
                var bestTarget = GetBestKillableHero(E, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.E) >= bestTarget.Health)
                {
                    E.Cast(bestTarget);
                }
            }
            if (R.Ready &&
                Menu["killsteal"]["ksr"].Enabled)
            {
                var bestTarget = GetBestKillableHero(R, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.R) >= bestTarget.Health)
                {
                    R.Cast(bestTarget);
                }
            }
            if (Q.Ready &&
                Menu["killsteal"]["ksrgap"].Enabled)
            {
                var bestTarget = GetRGAP(DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health && bestTarget.Distance(Player) > Q.Range)
                {
                    R.Cast(bestTarget.Position);
                    
                }
                if (bestTarget != null && bestTarget.Distance(Player) <= Q.Range && bestTarget != null && Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health)
                {
                    Q.CastOnUnit(bestTarget);
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
            if (target != null && target.IsValidTarget())
            {
                return target;
            }

            return ts.GetOrderedTargets(range).FirstOrDefault(t => target.IsValidTarget());
        }
        private void OnCombo()
        {
            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            bool WAA = Menu["combo"]["waa"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            bool Turret = Menu["combo"]["turret"].Enabled;
            float HP = Menu["combo"]["hp"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);
            if (!target.IsValidTarget())
            {
                return;
            }


            if (R.Ready && useR && target.IsValidTarget(R.Range) && Player.HealthPercent() > HP)
            {
                if (target != null)
                {
                    if (!Turret)
                    {
                        R.Cast(target);
                    }
                    if (Turret && !target.IsUnderEnemyTurret())
                    {
                        R.Cast(target);
                    }
                }
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {
                if (target != null)
                {
                    Q.CastOnUnit(target);
                }
            }
            if (W.Ready && useW && !WAA && target.IsValidTarget(W.Range))
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
                    E.Cast(target);
                }
            }
        }

        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool useW = Menu["harass"]["usew"].Enabled;
            bool WAA = Menu["harass"]["waa"].Enabled;
            bool useE = Menu["harass"]["usee"].Enabled;
            float manapercent = Menu["harass"]["mana"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);
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
                        Q.CastOnUnit(target);
                    }
                }
                if (W.Ready && useW && !WAA && target.IsValidTarget(W.Range))
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
                        E.Cast(target);
                    }
                }
            }
        }
    }
}
// Hi :>