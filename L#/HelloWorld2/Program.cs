using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.IO;
//using FireSharp;
//using FireSharp.Config;
//using FireSharp.Interfaces;

namespace HelloWorld2
{
    class Program
    {
        public static Spell E;
        public static Spell W;
        public static Spell Q;
        public static Spell R;
        public static Obj_AI_Hero Player;

        public static int[] abilitySequence;

        public static ItemId[] itemSequence;
        public static int[] itemSequenceCost;
        public static int itemindex = 0;

        public static String serverip = "https://leaguesharp.firebaseio.com/.json";

        public static Vector3 target = new Vector3(0,0,0);

        public static int qOff = 0, wOff = 0, eOff = 0, rOff = 0;

        public static bool disabled = false;

        public static Vector2 movedir;

        public static List<int> positionsx = new List<int>();
        public static List<int> positionsy = new List<int>();


        static void Main(string[] args)
        {
            Game.PrintChat("heyo");
            Game.Say("heyo2");

            Player = ObjectManager.Player;

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;

            Q = new Spell(SpellSlot.Q,625);
            W = new Spell(SpellSlot.W,625);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 650);

            Game.Say("prep2");

            abilitySequence = new int[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
            itemSequence = new ItemId[] { ItemId.Dorans_Ring, ItemId.Ruby_Crystal, ItemId.Sapphire_Crystal, ItemId.Catalyst_the_Protector, ItemId.Blasting_Wand, ItemId.Rod_of_Ages};
            itemSequenceCost = new int[] { 400, 400, 400, 400, 860, 740};

           

            //IFirebaseConfig config = new FirebaseConfig
            //{
            //    //AuthSecret = "your_firebase_secret",
            //    BasePath = "https://leaguesharp.firebaseio.com/"
            //};
            //IFirebaseClient client = new FirebaseClient(config);

            //client.OnAsync("chat", (sender, args1) =>
            //{
            //    System.Console.WriteLine(args1.Data);
            //    Game.Say("there's been an update, mate!");
            //});

        }

        static void CustomEvents_OnSpawn(Obj_AI_Hero sender, EventArgs args)
        {
            if (sender.NetworkId == ObjectManager.Player.NetworkId)
                BuyItems();
        }

        private static void BuyItems()
        {

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(0, 0, System.Drawing.Color.White, "hi kevin is here at codeday yay");

            if (Q.IsReady())
            {
                // draw Aqua circle around the player
                Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Aqua);
                Utility.DrawCircle(ObjectManager.Player.Position, 2000, System.Drawing.Color.Yellow);

            }

            if(R.IsReady())
            {
                Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Aqua);
            }

            for(int i = 0; i < positionsx.Count; i++)
            {
                Vector2 start = Drawing.WorldToScreen(new Vector3(Player.Position.X,Player.Position.Y,Player.Position.Z));
                Vector2 end = Drawing.WorldToScreen(new Vector3(Player.Position.X + positionsx[i], Player.Position.Y + positionsy[i],Player.Position.Z));
                Drawing.DrawLine(start,end,2,System.Drawing.Color.Red);
            }

            Vector2 start2 = Drawing.WorldToScreen(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z));
            Vector2 end2 = Drawing.WorldToScreen(new Vector3(Player.Position.X + movedir.X, Player.Position.Y + movedir.Y, Player.Position.Z));
            Drawing.DrawLine(start2, end2, 10, System.Drawing.Color.Red);

        }

        public static void Game_OnGameUpdate(EventArgs args)
        {

            if(Player.Gold > itemSequenceCost[itemindex] && Player.InShop())
            {
                Player.BuyItem(itemSequence[itemindex]);
                itemindex += 1;
            }
            

            string fileloc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data.txt");

            string[] text = System.IO.File.ReadAllLines(fileloc);

            int controllers = int.Parse(text[0]);

            //Game.Say(text.Length.ToString());

            positionsx = new List<int>();
            positionsy = new List<int>();

            Vector2 total = new Vector2(0, 0);

            for(int i = 0; i < (text.Length-1)/2; i++)
            {
                int x = (int)Math.Round(float.Parse(text[2*(i+1)-1]));
                int y = -1 * (int)Math.Round(float.Parse(text[2*(i+1)]));
                positionsx.Add(x);
                positionsy.Add(y);
                Vector2 dir = new Vector2(x, y);
                dir.Normalize();
                total += dir;
            }
            total.Normalize();
            movedir = total* 500;

            //Game.Say(movedir.X + " " + movedir.Y);


            int cursorx = (int)Math.Round(movedir.X);
            int cursory = (int)Math.Round(movedir.Y);

            target = ObjectManager.Player.Position + new Vector3(cursorx,cursory,0);

            if (Math.Abs(cursorx) + Math.Abs(cursory) > 100)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                disabled = false;
            }
            else
            {
                disabled = true;
            }

            Obj_AI_Hero targetE = TargetSelector.GetTarget(750, TargetSelector.DamageType.Magical);

            if (Q.IsReady())
            {
                // check if we found a valid target in range
                if (targetE.IsValidTarget(Q.Range))
                {
                    
                    // blast him
                    Q.CastOnUnit(targetE);
                }


                List<Obj_AI_Base> minionarray = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly);


                foreach (Obj_AI_Base minion in minionarray)
                {
                    // check if we found a minion to consume
                    if (minion.IsValidTarget())
                    {
                        Obj_AI_Hero targetE2 = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);
                        if (ObjectManager.Player.GetSpellDamage(minion,SpellSlot.Q) > minion.Health)
                        {
                            var buffs = ObjectManager.Player.Buffs.Where(buff => (buff.Name.ToLower() == "pyromania") || buff.Name.ToLower() == "pyromania_particle");
                            if (buffs.Any())
                            {
                                if (buffs.First().Name.ToLower() == "pyromania_particle") {
                                
                                    if(!targetE2.IsValidTarget())
                                    {
                                        Q.CastOnUnit(minion);
                                    }
                                }
                                else Q.CastOnUnit(minion);
                            }
                            else { Q.CastOnUnit(minion); }
                           
                        }
                        // nom nom nom
                    }
                }
            }
            if (W.IsReady())
            {
                // check if we found a valid target in range
                if (targetE.IsValidTarget(W.Range) && targetE.HasBuffOfType(BuffType.Stun))
                {
                    // blast him
                    W.CastOnUnit(targetE);
                }
            }
            if (E.IsReady() && !disabled)
            {
                E.Cast();
            }
            if (R.IsReady())
            {
                // check if we found a valid target in range
                if (targetE.IsValidTarget(R.Range))
                {
                    // blast him
                   R.Cast(targetE);
                   W.Cast(targetE);
                }
            }


            if(ObjectManager.Player.Health < 500)
            {
                Items.UseItem(3157);
            }

            int qL = Player.Spellbook.GetSpell(SpellSlot.Q).Level + qOff;
            int wL = Player.Spellbook.GetSpell(SpellSlot.W).Level + wOff;
            int eL = Player.Spellbook.GetSpell(SpellSlot.E).Level + eOff;
            int rL = Player.Spellbook.GetSpell(SpellSlot.R).Level + rOff;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = new int[] { 0, 0, 0, 0 };
                for (int i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[abilitySequence[i] - 1] = level[abilitySequence[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }

            

            //ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }

    }
}
