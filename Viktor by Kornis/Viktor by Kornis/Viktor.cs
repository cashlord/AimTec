using System.Net.Configuration;
using System.Resources;
using System.Security.Authentication.ExtendedProtection;

namespace Viktor_By_Kornis
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

    internal class Viktor
    {
        public static Menu Menu = new Menu("Viktor By Kornis", "Viktor by Kornis", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        public static Spell Q, W, E, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 730);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 700);
            E.SetSkillshot(0.25f, 85, 1700, false, SkillshotType.Line, false, HitChance.High);
            W.SetSkillshot(0.9f, 50, float.MaxValue, false, SkillshotType.Circle);

        }

        public Viktor()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            var QSet = new Menu("qset", "Q Settings");
            {
                QSet.Add(new MenuBool("useq", "Use Q in Combo"));
              //  QSet.Add(new MenuBool("qaa", "^- Only for Q Resets(Waits for AA)", false));
                
            }
            var WSet = new Menu("wset", "W Settings");
            {
                WSet.Add(new MenuBool("usew", "Use W in Combo"));
                WSet.Add(new MenuList("wmode", "W Mode", new[] { "Always", "Only on Slowed/CC/Immobile" }, 1));

            }
            var ESet = new Menu("eset", "E Settings");
            {
                ESet.Add(new MenuBool("usee", "Use E in Combo"));
            }
            var RSet = new Menu("rset", "R Settings");
            {
                RSet.Add(new MenuBool("user", "Use R in Combo"));
                RSet.Add(new MenuList("rmode", "R Mode", new[] { "Always", "If Killable" }, 0));
                RSet.Add(new MenuSlider("rtick", "Include X R Ticks", 1, 1, 4));
                RSet.Add(new MenuSlider("hitr", "Use R only if Hits X", 1, 1, 5));
                RSet.Add(new MenuSlider("waster", "Don't waste R if Enemy HP lower than", 100, 0, 500));
                RSet.Add(new MenuBool("follow", "Auto R Follow", true));
    
                RSet.Add(new MenuBool("forcer", "Force R in TeamFights", true));
                RSet.Add(new MenuSlider("forcehit", "^- Min. Enemies", 3, 2, 5));

            }
            Menu.Add(ComboMenu);
            ComboMenu.Add(QSet);
            ComboMenu.Add(WSet);
            ComboMenu.Add(ESet);
            ComboMenu.Add(RSet);
            
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                HarassMenu.Add(new MenuBool("useq", "Use  Q to Harass"));
                HarassMenu.Add(new MenuBool("usee", "Use E to Harass"));

            }
            Menu.Add(HarassMenu);
            var FarmMenu = new Menu("farming", "Farming");
            var LaneClear = new Menu("lane", "Lane Clear");
            {
                LaneClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                LaneClear.Add(new MenuBool("useq", "Use Q to Farm"));
                LaneClear.Add(new MenuBool("lastq", "^- Use for Last Hit"));
                LaneClear.Add(new MenuBool("usee", "Use E to Farm"));
                LaneClear.Add(new MenuSlider("hite", "^- if Hits", 3, 1, 6));
            }
            var JungleClear = new Menu("jungle", "Jungle Clear");
            {
                JungleClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                JungleClear.Add(new MenuBool("useq", "Use Q to Farm"));
                JungleClear.Add(new MenuBool("usee", "Use E to Farm"));

            }
            Menu.Add(FarmMenu);
            FarmMenu.Add(LaneClear);
            FarmMenu.Add(JungleClear);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("ksq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kse", "Killsteal with E"));
            }
            Menu.Add(KSMenu);
            var miscmenu = new Menu("misc", "Misc.");
            {
                miscmenu.Add(new MenuBool("autow", "Auto W on CC"));
            }
            Menu.Add(miscmenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawdamage", "Draw Damage"));
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
            //Orbwalker.PreAttack += Orbwalker_PreAttack;

            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            LoadSpells();
            Console.WriteLine("Viktor by Kornis - Loaded");
        }

      /*  private void Orbwalker_PreAttack(object sender, PreAttackEventArgs e)
        {
            if (Orbwalker.Mode.Equals(OrbwalkingMode.Combo))
            {
                if (Menu["combo"]["qset"]["qaa"].Enabled)
                    {
                    var target = GetBestEnemyHeroTargetInRange(Player.AttackRange);

                    if (!target.IsValidTarget())
                    {
                        return;
                    }

                    if (Q.Ready && target.IsValidTarget(Q.Range))
                    {
                        if (target != null)
                        {

                            Q.CastOnUnit(target);
                        }
                    }

                }
            }
        }*/

            public void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SpellSlot == SpellSlot.Q)
                {

                    Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                    
                     Orbwalker.ResetAutoAttackTimer();
                }
            }
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

        public class LaneclearResult
        {
            public LaneclearResult(int hit, Vector3 cp)
            {
                this.numberOfMinionsHit = hit;
                this.CastPosition = cp;
            }

            public int numberOfMinionsHit = 0;
            public Vector3 CastPosition;
        }
        public LaneclearResult GetLineClearLocation(float range, float width)
        {
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidTarget(E.Range) && x.IsValidSpellTarget());

            var positions = minions.Select(x => x.ServerPosition).ToList();

            var locations = new List<Vector3>();

            locations.AddRange(positions);

            var max = positions.Count();

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (positions[j] != positions[i])
                    {
                        locations.Add((positions[j] + positions[i]) / 2);
                    }
                }
            }


            HashSet<LaneclearResult> results = new HashSet<LaneclearResult>();

            foreach (var p in locations)
            {
                var rect = new Rectangle(Player.Position, p, width);

                var count = 0;

                foreach (var m in minions)
                {
                    if (rect.Contains(m.Position))
                    {
                        count++;
                    }
                }

                results.Add(new LaneclearResult(count, p));
            }

            var maxhit = results.MaxBy(x => x.numberOfMinionsHit);

            return maxhit;
        }

        private void Render_OnPresent()
        {

            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.CornflowerBlue);
            }
            if (Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Crimson);
            }
            if (Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.CornflowerBlue);
            }
            if (Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Crimson);
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
                                                     Player.GetSpellDamage(unit, SpellSlot.E) +
                                                     Player.GetSpellDamage(unit, SpellSlot.R) * Menu["combo"]["rset"]["rtick"].As<MenuSlider>().Value
                                             ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) +
                                                         Player.GetSpellDamage(unit, SpellSlot.E) +

                                                         Player.GetSpellDamage(unit, SpellSlot.R) * Menu["combo"]["rset"]["rtick"].As<MenuSlider>().Value)) /
                                                        unit.MaxHealth * 100 / 100)
                                             : 0));

                            Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 8, true,
                                unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) +
                                Player.GetSpellDamage(unit, SpellSlot.E) +
                                Player.GetSpellDamage(unit, SpellSlot.R) * Menu["combo"]["rset"]["rtick"].As<MenuSlider>().Value
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
            if (Menu["combo"]["rset"]["follow"].Enabled)
            {
                RFollow();
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
            if (Menu["misc"]["autow"].Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(W.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {

                    W.Cast(target);
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


        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }

        private void RFollow()
        {
            if (Player.HasBuff("viktorchaosstormtimer"))
            {
                var target = GetBestEnemyHeroTargetInRange(2000);
                if (!target.IsValidTarget())
                {
                    return;
                }
                if (target.IsValidTarget(2000) && target != null)
                {
                    R.Cast(target.Position);
                }
            }
        }


            private void Clearing()
        {
            bool useQ = Menu["farming"]["lane"]["useq"].Enabled;
            bool useE = Menu["farming"]["lane"]["usee"].Enabled;
            float hitE = Menu["farming"]["lane"]["hite"].As<MenuSlider>().Value;
            float manapercent = Menu["farming"]["lane"]["mana"].As<MenuSlider>().Value;
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
                if (useE)
                {
                    foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
                    {


                        if (minion.IsValidTarget(E.Range) && minion != null && E.Ready)
                        {
                            var result = GetLineClearLocation(E.Range, 100);
                            if (result != null && result.numberOfMinionsHit >= hitE)
                            {

                                E.Cast(result.CastPosition, result.CastPosition.Extend(minion.ServerPosition, 500));
                            }
    
                        }
                    }
                }
            }
        }

        private void Lasthit()
        {

            if (Menu["farming"]["lane"]["lastq"].Enabled)
            {

                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {
                    
                    if (minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);

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
            foreach (var jungleTarget in GameObjects.Jungle.Where(m => m.IsValidTarget(E.Range)).ToList())
            {
                if (!jungleTarget.IsValidTarget() || !jungleTarget.IsValidSpellTarget())
                {
                    return;
                }
                bool useQ = Menu["farming"]["jungle"]["useq"].Enabled;
                bool useE = Menu["farming"]["jungle"]["usee"].Enabled;
                float manapercent = Menu["farming"]["jungle"]["mana"].As<MenuSlider>().Value;
                if (manapercent < Player.ManaPercent())
                {
                    if (useQ && jungleTarget.IsValidTarget(Q.Range))
                    {
                    
                        Q.CastOnUnit(jungleTarget);
                    }
                    if (useE && jungleTarget.IsValidTarget(E.Range) && E.Ready)
                    {


                        if (jungleTarget.IsValidTarget(E.Range) && jungleTarget != null)
                        {
                            var result = GetLineClearLocation(E.Range, 100);
                            if (result != null && result.numberOfMinionsHit >= 1)
                            {

                                E.Cast(result.CastPosition, result.CastPosition.Extend(jungleTarget.ServerPosition, 500));
                            }

                        }
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
                foreach (var en in GameObjects.EnemyHeroes)
                {
                    if (!en.IsDead && en.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(en);
                    }
                }
                foreach (var en in GameObjects.EnemyMinions)
                {
                    if (!en.IsDead && en.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(en);
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
                    if (bestTarget != null)
                    {
                        if (bestTarget.Distance(Player) > 500)
                        {
                            var startpos = Player.ServerPosition.Extend(bestTarget.ServerPosition, 500);
                            var pred = E.GetPredictionInput(bestTarget);
                            pred.RangeCheckFrom = startpos;
                            pred.From = startpos;
                            var output = Prediction.GetPrediction(pred);
                            if (output.HitChance >= HitChance.High)
                            {
                                E.Cast(output.CastPosition, startpos);
                            }

                        }
                        if (bestTarget.Distance(Player) < 500)
                        {
                            var pred = E.GetPredictionInput(bestTarget);
                            pred.RangeCheckFrom = bestTarget.ServerPosition;
                            pred.From = bestTarget.ServerPosition;
                            var output = Prediction.GetPrediction(pred);
                            if (output.HitChance >= HitChance.High)
                            {
                                E.Cast(output.UnitPosition, output.CastPosition);
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
            bool useW = Menu["combo"]["wset"]["usew"].Enabled;

            bool useE = Menu["combo"]["eset"]["usee"].Enabled;
            bool useR = Menu["combo"]["rset"]["user"].Enabled;
            float meow = Menu["combo"]["rset"]["waster"].As<MenuSlider>().Value;
            float hits = Menu["combo"]["rset"]["hitr"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(E.Range);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range))
            {
                if (Menu["combo"]["rset"]["forcer"].Enabled)
                {
                    if (Menu["combo"]["rset"]["forcehit"].As<MenuSlider>().Value <= target.CountEnemyHeroesInRange(300))
                    {


                        R.Cast(target);


                    }
                }
                switch (Menu["combo"]["rset"]["rmode"].As<MenuList>().Value)
                {
                    case 0:
                        if (target.Health > meow)
                        {
                            if (hits == 1)
                            {

                                R.Cast(target);

                            }
                            if (hits > 1)
                            {
                                if (hits <= target.CountEnemyHeroesInRange(300))
                                {
                                    R.Cast(target);
                                }
                            }
                        }
                        break;
                    case 1:
                        if (target.Health > meow && target.Health <= Player.GetSpellDamage(target, SpellSlot.Q) +
                                                     Player.GetSpellDamage(target, SpellSlot.E) +
                                                     Player.GetSpellDamage(target, SpellSlot.R) * Menu["combo"]["rset"]["rtick"].As<MenuSlider>().Value)
                        {
                            if (hits == 1)
                            {

                                R.Cast(target);

                            }
                            if (hits > 1)
                            {
                                if (hits <= target.CountEnemyHeroesInRange(300))
                                {
                                    R.Cast(target);
                                }
                            }
                        }
                        break;

                }
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {
                if (target != null)
                {
                  //  if (!Menu["combo"]["qset"]["qaa"].Enabled)
                    {
                        Q.CastOnUnit(target);
                    }
                   // if (Menu["combo"]["qset"]["qaa"].Enabled && target.Distance(Player) > Player.AttackRange)
                    {
                       // Q.CastOnUnit(target);
                    }
                }
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range))
            {

                if (target != null)
                {
                    if (target.Distance(Player) > 500)
                    {
                        var startpos = Player.ServerPosition.Extend(target.ServerPosition, 500);
                        var pred = E.GetPredictionInput(target);
                        pred.RangeCheckFrom = startpos;
                        pred.From = startpos;
                        var output = Prediction.GetPrediction(pred);
                        if (output.HitChance >= HitChance.High)
                        {
                            E.Cast(output.CastPosition, startpos);
                        }

                    }
                    if (target.Distance(Player) < 500)
                    {
                        var pred = E.GetPredictionInput(target);
                        pred.RangeCheckFrom = target.ServerPosition;
                        pred.From = target.ServerPosition;
                        var output = Prediction.GetPrediction(pred);
                        if (output.HitChance >= HitChance.High)
                        {
                            E.Cast(output.UnitPosition, output.CastPosition);
                        }
                    }
                }

            }
            
            switch (Menu["combo"]["wset"]["wmode"].As<MenuList>().Value)
            {
                case 0:
                    if (W.Ready && useW && target.IsValidTarget(W.Range) && !E.Ready)
                    {
                        if (target != null)
                        {
                            W.Cast(target);
                        }
                    }
                    break;
                case 1:
                    if (Q.Ready && useQ && target.IsValidTarget(W.Range) && !E.Ready)
                    {
                        if (target != null)
                        {
                            foreach (var starget in GameObjects.EnemyHeroes.Where(
    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) || t.HasBuffOfType(BuffType.Slow) || 
          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(W.Range) &&
         !Invulnerable.Check(t, DamageType.Magical)))
                            {

                                W.Cast(target);
                            }

                        }
                    }
                    break;
            }
           
            
        }

        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool useE = Menu["harass"]["usee"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(E.Range);
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
                        Q.CastOnUnit(target);
                    }
                }
                    if (E.Ready && useE && target.IsValidTarget(E.Range))
                {

                    if (target != null)
                    {
                        if (target.Distance(Player) > 500)
                        {
                            var startpos = Player.ServerPosition.Extend(target.ServerPosition, 500);
                            var pred = E.GetPredictionInput(target);
                            pred.RangeCheckFrom = startpos;
                            pred.From = startpos;
                            var output = Prediction.GetPrediction(pred);
                            if (output.HitChance >= HitChance.High)
                            {
                                E.Cast(output.CastPosition, startpos);
                            }

                        }
                        if (target.Distance(Player) < 500)
                        {
                            var pred = E.GetPredictionInput(target);
                            pred.RangeCheckFrom = target.ServerPosition;
                            pred.From = target.ServerPosition;
                            var output = Prediction.GetPrediction(pred);
                            if (output.HitChance >= HitChance.High)
                            {
                                E.Cast(output.UnitPosition, output.CastPosition);
                            }
                        }
                    }

                }
            }
        }
    }
}

