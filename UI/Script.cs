﻿using System.Drawing;
using System.IO;
using System;
using BotFramework;
using System.Diagnostics;
using ImgXml;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UI
{
    public class VCBotScript : ScriptInterface
    {
        public static Stopwatch stop = new Stopwatch();
        public static bool RuneBoss, Stuck, EnterWitchGate, Archwitch_Repeat, DisableAutoCheckEvent, CloseEmu = false, pushed = false;
        public static readonly string game = "com.nubee.valkyriecrusade", activity = "/.GameActivity";
        public static int runes, energy;
        public static double Archwitch_Stage, Weapon_Stage;
        public static int TreasureHuntIndex = -1;
        private static int Retry = 0, error = 0;
        public static string Tower_Floor = "", Tower_Rank = "";
        public static ScreenshotData image = default;
        public static DateTime nextOnline;
        private static bool Collected;
        //Try Locate MainScreen
        public static void LocateMainScreen()
        {
            Debug_.WriteLine();
            BotCore.Delay(1000, false);
            PrivateVariable.Instance.InMainScreen = false;
            BotCore.Delay(100, 200);
            bool StartGame = false;
            for (int x = 0; x < 30; x++)
            {
                if (!Muted_Device && StartGame)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        BotCore.SendEvent(25);
                    }
                    Muted_Device = true;
                }
                image = Screenshot.ImageCapture();
                while (BotCore.RGBComparer(new Point(520, 355), Color.Black, 1))
                {
                    BotCore.Delay(1000, true);
                    image = Screenshot.ImageCapture();
                    if (!BotCore.GameIsForeground(game))
                    {
                        BotCore.StartGame(game + activity);
                    }
                }
                if (!ScriptErrorHandler.ErrorHandle())
                {
                    var crop = Screenshot.CropImage(image, new Point(315, 150), new Point(1005, 700));
                    Point? point = BotCore.FindImage(crop, Img.GreenButton, false, 0.9);
                    if (point != null && !BotCore.RGBComparer(new Point(116, 37), Color.FromArgb(36, 8, 39), 15, image))
                    {
                        BotCore.SendTap(point.Value.X + 315, point.Value.Y + 150);
                        x--;
                    }
                    // We are inside draw card page!!
                    else if(point != null)
                    {
                        Variables.ScriptLog("Unexpected UI found! Restarting game!", Color.Red);
                        BotCore.KillGame(game);
                        return;
                    }
                    if (BotCore.FindImage(image, Img.MainScreen, true, 0.9) == null)
                    {
                        if (!BotCore.GameIsForeground(game))
                        {
                            return;
                        }
                        Variables.ScriptLog("Main Screen not visible", Color.White);
                        point = BotCore.FindImage(Screenshot.CropImage(image, new Point(319, 525), new Point(588, 591)), Img.Start_Game, false, 0.8);
                        if (point != null && BotCore.RGBComparer(new Point(379, 642), Color.FromArgb(37, 37, 37), 15, image))
                        {
                            Variables.ScriptLog("Start Game Button Located!", Color.Lime);
                            BotCore.SendTap(438, 558);
                            error = 0;
                            return;
                        }
                        point = BotCore.FindImage(image, Img.Update_Complete, true, 0.8);
                        if (point != null)
                        {
                            BotCore.SendTap(point.Value);
                            return;
                        }
                        point = BotCore.FindImage(image, Img.Close2, true, 0.9);
                        if (point != null)
                        {
                            BotCore.SendTap(point.Value);
                            return;
                        }
                        point = BotCore.FindImage(image, Img.Close, true, 0.9);
                        if (point != null)
                        {
                            BotCore.SendTap(point.Value);
                            return;
                        }
                        point = BotCore.FindImage(image, Img.Login_Reward, true, 0.9);
                        if (point != null)
                        {
                            for (int y = 0; y < 4; y++)
                            {
                                BotCore.SendTap(600, 350);
                                BotCore.Delay(1000, false);
                            }
                            return;
                        }
                        point = BotCore.FindImage(crop, Img.Red_Button, false, 0.88);
                        if (BotCore.RGBComparer(new Point(282, 285), Color.FromArgb(63, 110, 114), 10, image) && BotCore.RGBComparer(new Point(169, 52), Color.FromArgb(31, 56, 74), 10, image) && point != null)
                        {
                            BotCore.SendTap(point.Value);
                            Variables.ScriptLog("Found Red Button!", Color.Lime);
                        }
                        point = BotCore.FindImage(image, Img.Back_to_Village, true, 0.9);
                        if (point != null)
                        {
                            Variables.ScriptLog("Going back to Main screen", Color.Lime);
                            BotCore.SendTap(point.Value);
                            PrivateVariable.Instance.InMainScreen = true;
                            Variables.ScriptLog("Screen Located", Color.Lime);
                        }
                        point = BotCore.FindImage(image, Img.Menu, true, 0.9);
                        if (point == null)
                        {
                            if (error < 30)
                            {
                                if (error == 0)
                                {
                                    Variables.ScriptLog("Waiting for Main screen", Color.White);
                                }
                                BotCore.Delay(5000, false);
                                error++;
                            }
                            else
                            {
                                BotCore.KillGame(game);
                                ScriptErrorHandler.Reset("Unable to locate main screen. Restarting Game!");
                                error = 0;
                                return;
                            }
                        }
                        else
                        {
                            BotCore.SendTap(point.Value);
                            BotCore.Delay(1000, false);
                            Variables.ScriptLog("Returning main screen", Color.Lime);
                            BotCore.SendTap(942, 630);
                            BotCore.Delay(5000, false);
                        }
                        point = BotCore.FindImage(image, Img.GreenButton, false, 0.88);
                        if (point != null)
                        {
                            BotCore.SendTap(point.Value);
                            Variables.ScriptLog("Green Button Found!", Color.Lime);
                            BotCore.Delay(200);
                        }
                    }
                    else
                    {
                        Retry++;
                        if (Retry > 5)
                        {
                            PrivateVariable.Instance.InMainScreen = true;
                            Variables.ScriptLog("Screen Located", Color.White);
                            if (PrivateVariable.Instance.VCevent != PrivateVariable.EventType.GuildWar)
                            {
                                Collect();
                            }
                            Retry = 0;
                            break;
                        }
                        else
                        {
                            BotCore.Delay(1000, false);
                            if (Retry == 1)
                            {
                                Variables.ScriptLog("Waiting for Login Bonus", Color.DeepPink);
                            }
                        }
                    }
                    if (x > 25)
                    {
                        BotCore.KillGame(game);
                    }
                }
                else
                {
                    x = 0;
                }
            }
        }
        //Collect
        private static void Collect()
        {
            if (!Collected)
            {
                Variables.ScriptLog("Collecting Resources", Color.Lime);
                for (int x = 0; x < 4; x++)
                {
                    switch (x)
                    {
                        case 0:
                            BotCore.SendSwipe(new Point(925, 576), new Point(614, 26), 1000);
                            break;
                        case 1:
                            BotCore.SendSwipe(new Point(320, 550), new Point(877, 127), 1000);
                            break;
                        case 2:
                            BotCore.SendSwipe(new Point(226, 175), new Point(997, 591), 1000);
                            break;
                        case 3:
                            BotCore.SendSwipe(new Point(969, 128), new Point(260, 545), 1000);
                            break;
                    }
                    image = Screenshot.ImageCapture();
                    var crop = Screenshot.CropImage(image, new Point(395, 0), new Point(1020, 720));
                    var p = BotCore.FindImage(crop, Img.Red_Button, false, 0.8);
                    if(p != null)
                    {
                        BotCore.SendTap(new Point(p.Value.X + 395, p.Value.Y));
                    }
                    p = BotCore.FindImage(crop, Img.GreenButton, false, 0.8);
                    if(p != null)
                    {
                        BotCore.SendTap(new Point(p.Value.X + 395, p.Value.Y));
                    }
                    //Find image and collect
                    if(!BotCore.RGBComparer(new Point(1259, 219), Color.FromArgb(75, 87, 254), 65, image))
                    {
                        p = BotCore.FindImage(crop, Img.Resource_1, false, 0.9);
                        if (p != null)
                        {
                            BotCore.SendTap(new Point(p.Value.X + 395, p.Value.Y));
                            BotCore.Delay(100, 200);
                        }
                    }
                    if (!BotCore.RGBComparer(new Point(1261, 101), Color.FromArgb(70, 130, 10), 65, image))
                    {
                        p = BotCore.FindImage(crop, Img.Resource_2, false, 0.9);
                        if (p != null)
                        {
                            BotCore.SendTap(new Point(p.Value.X + 395, p.Value.Y));
                            BotCore.Delay(100, 200);
                            
                        }
                    }
                    if (!BotCore.RGBComparer(new Point(1260, 42), Color.FromArgb(249, 173, 46), 10, image))
                    {
                        p = BotCore.FindImage(crop, Img.Resource_3, false, 0.9);
                        if (p != null)
                        {
                            BotCore.SendTap(new Point(p.Value.X + 395, p.Value.Y));
                            BotCore.Delay(100, 200);
                            
                        }
                    }
                    if(!BotCore.RGBComparer(new Point(1260, 160), Color.FromArgb(125, 125, 125), 10, image))
                    {
                        p = BotCore.FindImage(crop, Img.Resource_4, false, 0.9);
                        if (p != null)
                        {
                            BotCore.SendTap(new Point(p.Value.X + 395, p.Value.Y));
                            BotCore.Delay(100, 200);
                            
                        }
                    }
                    BotCore.Delay(800, 1200);
                }
            }
            Collected = true;
        }

        //Check Event
        private static Point[] eventlocations = new Point[] { new Point(795, 80), new Point(795, 240), new Point(795, 400)  };
        private static int selectedeventlocations = 0;
        private static void CheckEvent()
        {
            if (selectedeventlocations >= eventlocations.Length)
            {
                selectedeventlocations = 0;
            }
            Point eventlocation = eventlocations[selectedeventlocations];
            Debug_.WriteLine();
            Point? point = null;
            int error = 0;
            if (Variables.FindConfig("General","Double_Event", out string Special))
            {
                if (Special == "true")
                {
                    BotCore.SendTap(170, 655);
                    BotCore.Delay(5000, false);
                    for (int x = 0; x < 5; x++)
                    {
                        image = Screenshot.ImageCapture();
                        Point? located = BotCore.FindImage(image, "Img\\LocateEventSwitch.png", true, 0.8);
                        if (located == null)
                        {
                            x -= 1;
                            BotCore.Delay(1000, false);
                            if (error > 10)
                            {
                                ScriptErrorHandler.Reset("Unable to locate Event Switch screen! Returning main screen!");
                                error = 0;
                                return;
                            }
                            error++;
                            ScriptErrorHandler.ErrorHandle();
                            continue;
                        }
                        else if (File.Exists("Img\\Event.png"))
                        {
                            Variables.ScriptLog("Finding Event.png on screen", Color.White);
                            point = BotCore.FindImage(image, Environment.CurrentDirectory + "\\Img\\Event.png", true, 0.8);
                            if (point != null)
                            {
                                Variables.ScriptLog("Image matched", Color.Lime);
                                BotCore.SendTap(point.Value);
                                break;
                            }
                            ScriptErrorHandler.ErrorHandle();
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (point == null)
                    {
                        Variables.ScriptLog("Event.png not found, force enter event and check what event...", Color.Red);
                        BotCore.SendTap(eventlocation);
                        Variables.ScriptLog("Updating Event.png...", Color.White);
                        var save = Screenshot.CropImage(image, eventlocation, new Point(eventlocation.X + 60, eventlocation.Y + 30));
                        if (File.Exists("Img\\Event.png"))
                        {
                            File.Delete(("Img\\Event.png"));
                        }
                        Screenshot.Decompress(save).Save(Environment.CurrentDirectory + "\\Img\\Event.png");
                    }
                }
                else
                {
                    BotCore.SendTap(130, 520);
                }
            }
            else
            {
                BotCore.SendTap(130, 520);
            }
            BotCore.Delay(9000, 12000);
            Variables.ScriptLog("Detecting Events", Color.Green);
            error = 0;
            point = null;
            var tutorial_charac = 0;
            do
            {
                if (!BotCore.GameIsForeground(game))
                {
                    return;
                }
                image = Screenshot.ImageCapture();
                var crop = Screenshot.CropImage(image, new Point(60, 133), new Point(255, 160));
                point = BotCore.FindImage(crop, Img.GreenButton, false, 0.8);
                if (point != null)
                {
                    BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                    continue;
                }
                if (BotCore.FindImage(image, Img.Demon_Tutorial, true, 0.8) != null || BotCore.FindImage(image, Img.Tower_Tutorial, true, 0.9) != null)
                {
                    for(int x = 0; x < 10; x++)
                    {
                        Random rnd = new Random();
                        BotCore.SendTap(rnd.Next(290, 310), rnd.Next(290, 310));
                        BotCore.Delay(rnd.Next(90, 110));
                    }
                    //check if it is stucked with this booby. Maybe the event is ended!
                    tutorial_charac++;
                    if(tutorial_charac > 10)
                    {
                        //yes, the event sure ended, no more stucking loop here!! Lets get the fxxk out!
                        File.Delete("Img\\Event.png");
                        selectedeventlocations++;
                        ScriptErrorHandler.Reset("Previous Event is ended! Lets get a new event!");
                        return;
                    }
                    continue;
                }
                if (BotCore.FindImage(image, Img.ArchwitchHunt, false, 0.85) != null || BotCore.FindImage(image, Img.SoulArrow, false, 0.8) != null)
                {
                    
                    //Is SoulWeapon Event
                    PrivateVariable.Instance.VCevent = PrivateVariable.EventType.SoulWeapon;
                    PrivateVariable.Instance.InEventScreen = true;
                    return;
                }
                if (BotCore.FindImage(image, Img.Locate_Tower, true, 0.85) != null)
                {
                    //Is Tower Event
                    PrivateVariable.Instance.VCevent = PrivateVariable.EventType.Tower;
                    PrivateVariable.Instance.InEventScreen = true;
                    return;
                }
                if (BotCore.RGBComparer(new Point(824, 651), Color.FromArgb(80, 1, 9), 15) && BotCore.RGBComparer(new Point(466, 673), Color.FromArgb(77, 75, 84), 15))
                {
                    //Is Demon Event
                    PrivateVariable.Instance.VCevent = PrivateVariable.EventType.DemonRealm;
                    PrivateVariable.Instance.InEventScreen = true;
                    return;
                }
                if (BotCore.FindImage(image, Img.HellLoc, true, 0.8) != null)
                {
                    //Is Demon Event
                    PrivateVariable.Instance.VCevent = PrivateVariable.EventType.DemonRealm;
                    PrivateVariable.Instance.InEventScreen = true;
                    return;
                }
                if (BotCore.FindImage(image, Img.Archwitch, true, 0.8) != null)
                {
                    if (File.Exists("Img\\ArchEvent.png"))
                    {
                        File.Delete("Img\\ArchEvent.png");
                    }
                    File.Move("Img\\Event.png", "Img\\ArchEvent.png");
                    selectedeventlocations++;
                    ScriptErrorHandler.Reset("Archwitch Event found, but we don't want to run this!");
                    return;
                }
                if (BotCore.FindImage(image, Img.MainScreen, true, 0.8) != null)
                {
                    Variables.ScriptLog("Rare error happens, still in main screen!", Color.Red);
                    PrivateVariable.Instance.InMainScreen = false;
                    return;
                }
                crop = Screenshot.CropImage(image, new Point(140, 0), new Point(1160, 720));
                point = BotCore.FindImage(crop, Img.Red_Button, false, 0.8);
                if (point != null)
                {
                    Variables.ScriptLog("Battle Screen found. Starting battle!", Color.Lime);
                    BotCore.SendTap(point.Value.X + 140, point.Value.Y);
                    PrivateVariable.Instance.Battling = true;
                    PrivateVariable.Instance.InEventScreen = true;
                    return;
                }
                if (error > 5)
                {
                    //The event is not readable, try to click another event!
                    if(selectedeventlocations < eventlocations.Length)
                    {
                        selectedeventlocations++;
                    }
                    else
                    {
                        MessageBox.Show("Current all event is not supported by VCBot!");
                    }
                    if (File.Exists("Img\\Event.png"))
                    {
                        File.Delete("Img\\Event.png");
                    }
                    ScriptErrorHandler.Reset("Event unable to detected, restarting...");
                    error = 0;
                    return;
                }
                ScriptErrorHandler.ErrorHandle();
                error++;
            }
            while (true);
        }
        //Demon Event
        
        static int locateUIError = 0;
        //Fighting and locate UI
        private static void LocateUI()
        {
            BotCore.Delay(1000);
            var crop = Screenshot.CropImage(image, new Point(1100, 0), new Point(1280, 200));
            var check = BotCore.FindImage(crop, Img.BattleScreen, true, 0.7);
            if (check == null)
            {
                Variables.ScriptLog("HP bar not found. Finding UIs", Color.Yellow);
                Attackable = false;
                if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.SoulWeapon)
                {
                    if (BotCore.FindImage(image, Img.ArchwitchHunt, true, 0.8) != null)
                    {
                        PrivateVariable.Instance.Battling = false;
                        Variables.ScriptLog("Battle Ended!", Color.Lime);
                        stop.Stop();
                        Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                        stop.Reset();
                        BotCore.SendTap(1076, 106);
                        locateUIError = 0;
                        return;
                    }
                    if (BotCore.FindImage(image, Img.GodArch, true, 0.8) != null)
                    {
                        //Ignore it
                        BotCore.SendTap(1067, 54);
                        BotCore.Delay(800);
                        BotCore.SendTap(1224, 42);
                        PrivateVariable.Instance.Battling = false;
                        Variables.ScriptLog("Battle Ended!", Color.Lime);
                        stop.Stop();
                        Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                        stop.Reset();
                        return;
                    }
                }
                if(PrivateVariable.Instance.VCevent == PrivateVariable.EventType.SoulWeapon || PrivateVariable.Instance.VCevent == PrivateVariable.EventType.ArchWitch)
                {
                    BotCore.SendTap(640, 156);
                }
                else
                {
                    BotCore.SendTap(10, 10);
                }
                var point = BotCore.FindImage(image, Img.Close2, true, 0.9);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    BotCore.Delay(1000, 1200);
                    locateUIError = 0;
                    image = Screenshot.ImageCapture();
                }
                if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.SoulWeapon || PrivateVariable.Instance.VCevent == PrivateVariable.EventType.ArchWitch)
                {
                    BotCore.SendTap(640, 156);
                }
                else
                {
                    BotCore.SendTap(10, 10);
                }
                if (BotCore.FindImage(image, Img.NoEnergy, true, 0.85) != null)
                {
                    ScriptErrorHandler.Reset("No Energy Left!");
                    NoEnergy();
                    locateUIError = 0;
                    return;
                }
                if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.SoulWeapon || PrivateVariable.Instance.VCevent == PrivateVariable.EventType.ArchWitch)
                {
                    BotCore.SendTap(640, 156);
                }
                else
                {
                    BotCore.SendTap(10, 10);
                }
                if (!PrivateVariable.Instance.Battling)
                {
                    return;
                }
                if (BotCore.RGBComparer(new Point(824, 651), Color.FromArgb(80, 1, 9), 15) && BotCore.RGBComparer(new Point(466, 673), Color.FromArgb(77, 75, 84), 15))
                {
                    PrivateVariable.Instance.Battling = false;
                    Variables.ScriptLog("Battle Ended!", Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                    stop.Reset();
                    BotCore.SendTap(1076, 106);
                    locateUIError = 0;
                    return;
                }
                if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.SoulWeapon || PrivateVariable.Instance.VCevent == PrivateVariable.EventType.ArchWitch)
                {
                    BotCore.SendTap(640, 156);
                }
                else
                {
                    BotCore.SendTap(10, 10);
                }
                if (!PrivateVariable.Instance.Battling)
                {
                    return;
                }
                if (BotCore.FindImage(image, Img.Locate_Tower, true, 0.8) != null)
                {
                    PrivateVariable.Instance.Battling = false;
                    Variables.ScriptLog("Battle Ended!", Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                    stop.Reset();
                    locateUIError = 0;
                    return;
                }
                if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.SoulWeapon || PrivateVariable.Instance.VCevent == PrivateVariable.EventType.ArchWitch)
                {
                    BotCore.SendTap(640, 156);
                }
                else
                {
                    BotCore.SendTap(10, 10);
                }
                if (!PrivateVariable.Instance.Battling)
                {
                    return;
                }
                if (BotCore.RGBComparer(new Point(824, 651), Color.FromArgb(80, 1, 9), 10) && BotCore.RGBComparer(new Point(466, 673), Color.FromArgb(77, 75, 84), 10))
                {
                    //Is Demon Event
                    PrivateVariable.Instance.Battling = false;
                    Variables.ScriptLog("Battle Ended!", Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                    stop.Reset();
                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Instance.Battling)
                {
                    return;
                }
                if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.SoulWeapon || PrivateVariable.Instance.VCevent == PrivateVariable.EventType.ArchWitch)
                {
                    BotCore.SendTap(640, 156);
                }
                else
                {
                    BotCore.SendTap(10, 10);
                }
                crop = Screenshot.CropImage(image, new Point(125, 0), new Point(1280, 720));
                point = BotCore.FindImage(crop, Img.GreenButton, false, 0.85);
                if (point != null)
                {
                    Variables.ScriptLog("Green Button Found!", Color.Lime);
                    if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.Tower)
                    {
                        if (RuneBoss && runes >= 3 && runes != 5 && BotCore.FindImage(image, Img.TowerFinished, true, 0.8) != null)
                        {
                            PrivateVariable.Instance.InEventScreen = false;
                            PrivateVariable.Instance.InMainScreen = false;
                            PrivateVariable.Instance.Battling = false;
                            Stuck = true;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            locateUIError = 0;
                            RuneBoss = false;
                            return;
                        }
                        else
                        {
                            if (BotCore.FindImage(image, Img.Locate_Tower, true, 0.85) != null)
                            {
                                PrivateVariable.Instance.Battling = false;
                                Variables.ScriptLog("Battle Ended!", Color.Lime);
                                stop.Stop();
                                Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                                stop.Reset();

                                locateUIError = 0;
                            }
                            else
                            {
                                BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                                BotCore.Delay(1000);
                                locateUIError = 0;
                                return;
                            }
                        }
                    }
                    else if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.DemonRealm)
                    {
                        crop = Screenshot.CropImage(image, new Point(147, 234), new Point(613, 299));
                        if (BotCore.FindImage(crop, Img.DemonEnd, true, 0.85) != null)
                        {
                            PrivateVariable.Instance.Battling = false;
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();

                            locateUIError = 0;
                            return;
                        }
                        else
                        {
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);

                            locateUIError = 0;
                            BotCore.Delay(1000);
                            return;
                        }
                    }
                    else
                    {
                        if (BotCore.FindImages(image, new Bitmap[] { Img.SoulArrow, Img.PT, Img.ArchwitchHunt }, true, 0.9, true) != null)
                        {
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            PrivateVariable.Instance.Battling = false;
                            return;
                        }
                        else
                        {
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            locateUIError = 0;
                            return;
                        }
                    }
                }
                crop = Screenshot.CropImage(image, new Point(125, 0), new Point(1280, 720));
                point = BotCore.FindImage(crop, Img.Red_Button, false, 0.9);
                if (point != null)
                {
                    if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.DemonRealm)
                    {
                        if (BotCore.RGBComparer(new Point(133, 35), Color.FromArgb(30, 30, 30), 50, image))
                        {
                            PrivateVariable.Instance.Battling = false;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            locateUIError = 0;
                            return;
                        }
                    }
                    else if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.GuildWar)
                    {
                        if (BotCore.FindImage(image, "Img\\GuildWar\\Locate.png", false, 0.8) != null)
                        {
                            PrivateVariable.Instance.Battling = false;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            locateUIError = 0;
                            return;
                        }
                    }
                    Variables.ScriptLog("Starting Battle", Color.Lime);
                    BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                    PrivateVariable.Instance.Battling = true;
                    BotCore.Delay(900, 1000);
                    crop = Screenshot.CropImage(image, new Point(682, 544), new Point(905, 589));
                    if (BotCore.RGBComparer(crop, Color.FromArgb(29, 98, 24), 30))
                    {
                        BotCore.SendTap(793, 565);
                        BotCore.Delay(1000, false);
                    }

                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Instance.Battling)
                {
                    return;
                }
                if (BotCore.RGBComparer(new Point(1125, 185), Color.Black, 0, image) && BotCore.RGBComparer(new Point(1195, 610), Color.Black, 0, image))
                {
                    point = BotCore.FindImage(Screenshot.CropImage(image, new Point(340, 535), new Point(385, 585)), Img.Start_Game, false, 0.8);
                    if (point != null && BotCore.RGBComparer(new Point(379, 642), Color.FromArgb(37, 37, 37), 15, image))
                    {
                        PrivateVariable.Instance.Battling = false;
                        ScriptErrorHandler.Reset("Start Game Button Located!");
                        error = 0;
                        return;
                    }
                }
                if (BotCore.RGBComparer(new Point(959, 656), 31, 102, 26, 34, image))
                {
                    Variables.ScriptLog("Start battle", Color.Lime);
                    BotCore.SendTap(959, 656);
                    PrivateVariable.Instance.Battling = true;
                    locateUIError = 0;
                    return;
                }
                /*if (BotCore.RGBComparer(new Point(415, 678), Color.FromArgb(223, 192, 63), 40, image))
                {
                    PrivateVariable.Instance.Battling = false;
                    BotCore.Delay(9000, 12000);
                    return;
                }*/
                locateUIError++;
                if (locateUIError > 30)
                {
                    //Something wrong, we need to reset
                    BotCore.KillGame(game);
                    ScriptErrorHandler.Reset("Error on finding UIs, no UI found and no battle screen found!");
                    locateUIError = 0;
                }
                else if(locateUIError / 2 == 0)
                {
                    ScriptErrorHandler.ErrorHandle();
                }
            }
        }
        static bool Attackable = true;


        /// <summary>
        /// Check is there any HP bar in game
        /// </summary>
        private static void CheckEnemy()
        {
            if (Attackable)
            {
                Debug_.WriteLine();
                var enemy = Screenshot.CropImage(image, new Point(585, 260), new Point(715, 305));
                if (BotCore.RGBComparer(enemy, Color.FromArgb(33, 106, 159), 5) || BotCore.RGBComparer(enemy, Color.FromArgb(171, 0, 21), 5))
                {
                    BotCore.SendTap(640, 156); //Boss在中间，打Boss
                    Variables.ScriptLog("Found Boss at center!", Color.LightPink);
                }
                else
                {
                    if (PrivateVariable.Instance.Battling == true && PrivateVariable.Instance.VCevent == PrivateVariable.EventType.DemonRealm)
                    {
                        var point = BotCore.FindImage(image, Img.HellLoc, false, 0.85);
                        if (point != null)
                        {
                            PrivateVariable.Instance.Battling = false;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            return;
                        }
                        BotCore.Delay(100, 500);
                    }
                    //找不到Boss血量条，可能剩下一丝血，所以先打中间试试水，再打小怪
                    BotCore.SendTap(640, 156);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(462, 176);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(820, 187);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(330, 202);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(952, 193);
                    Variables.ScriptLog("Boss not found, trying to hit others", Color.LightPink);
                }
            }
            Attackable = true;
        }
        public static void Battle()
        {
            do
            {
                Attackable = true;
                BotCore.Delay(500);
                image = Screenshot.ImageCapture();
                LocateUI();
                Debug_.WriteLine();
                if (Attackable)
                {
                    Variables.ScriptLog("Locating Skills and enemies", Color.Gold);
                    if (PrivateVariable.Instance.BattleScript.Count > 0)
                    {
                        PrivateVariable.Instance.BattleScript[PrivateVariable.Instance.Selected_Script].Attack();
                        BotCore.Delay(300);
                    }
                    CheckEnemy();
                    Stopwatch delay = Stopwatch.StartNew();
                    for (int x = 0; x < 10; x++)
                    {
                        BotCore.Delay(100, 300);
                        if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.DemonRealm || PrivateVariable.Instance.VCevent == PrivateVariable.EventType.Tower)
                        {
                            Random rnd = new Random();
                            BotCore.SendTap(rnd.Next(5, 15), rnd.Next(5, 15));
                        }
                        else
                        {
                            BotCore.SendTap(643, 167);
                        }
                        if (delay.ElapsedMilliseconds > 3000)
                        {
                            break;
                        }
                    }
                    delay.Stop();
                    BotCore.SendTap(640, 650);
                }
                if (!BotCore.GameIsForeground(game))
                {
                    ScriptErrorHandler.Reset("Game is closed! Restarting all!");
                    return;
                }
            }
            while (PrivateVariable.Instance.Battling);
        }
        //Get energy
        public static int GetEnergy()
        {
            Debug_.WriteLine();
            image = Screenshot.ImageCapture();
            int num = 0;
            if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.Tower)
            {
                    Color energy = Color.FromArgb(50, 233, 34);
                    if (BotCore.RGBComparer(new Point(417, 535), energy, 30, image))
                    {
                        num++;
                    }
                    if (BotCore.RGBComparer(new Point(481, 535), energy, 30, image))
                    {
                        num++;
                    }
                    if (BotCore.RGBComparer(new Point(546, 535), energy, 30, image))
                    {
                        num++;
                    }
                    if (BotCore.RGBComparer(new Point(613, 535), energy, 30, image))
                    {
                        num++;
                    }
                    if (BotCore.RGBComparer(new Point(677, 535), energy, 30, image))
                    {
                        num++;
                    }
                
            }
            else if(PrivateVariable.Instance.VCevent == PrivateVariable.EventType.DemonRealm)
            {
                Color energy = Color.FromArgb(104, 45, 22);
                if (BotCore.RGBComparer(new Point(208, 445), energy, 30, image))
                {
                    num++;
                }
                if (BotCore.RGBComparer(new Point(253, 441), energy, 30, image))
                {
                    num++;
                }
                if (BotCore.RGBComparer(new Point(315, 445), energy, 30, image))
                {
                    num++;
                }
                if (BotCore.RGBComparer(new Point(351, 449), energy, 30, image))
                {
                    num++;
                }
                if (!BotCore.RGBComparer(new Point(410, 458), Color.FromArgb(27, 24, 29), 10, image))
                {
                    num++;
                }
            }
            else
            {
                Color energy = Color.FromArgb(111,111,111);
                if (!BotCore.RGBComparer(new Point(876, 560), energy, 30, image))
                {
                    num++;
                }
                if (!BotCore.RGBComparer(new Point(941, 560), energy, 30, image))
                {
                    num++;
                }
                if(!BotCore.RGBComparer(new Point(1006, 559), energy, 30, image))
                {
                    num++;
                }
                if(!BotCore.RGBComparer(new Point(1068, 560), energy, 30, image))
                {
                    num++;
                }
                if(!BotCore.RGBComparer(new Point(1133, 560), energy, 30, image))
                {
                    num++;
                }
                return num;
            }
            if(nextOnline < DateTime.Now)
            {
                var temp = 5 - num;
                nextOnline = DateTime.Now.AddMinutes(temp * 45);
            }
            return num;
        }
        //Get runes
        public static int GetRune()
        {
            Debug_.WriteLine();
            if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.Tower)
            {
                int num = 5;
                if (BotCore.RGBComparer(new Point(945, 207), 118, 117, 118, 30, image))
                {
                    num--;
                }
                if (BotCore.RGBComparer(new Point(979, 308), 114, 114, 114, 30, image))
                {
                    num--;
                }
                if (BotCore.RGBComparer(new Point(1088, 309), 118, 117, 118, 30, image))
                {
                    num--;
                }
                if (BotCore.RGBComparer(new Point(1121, 204), 113, 113, 113, 30, image))
                {
                    num--;
                }
                if (BotCore.RGBComparer(new Point(1033, 140), 116, 115, 115, 30, image))
                {
                    num--;
                }
                return num;
            }
            else if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.DemonRealm)
            {
                int num = 0;
                
                if (BotCore.RGBComparer(new Point(980, 163), 117, 95, 80, 30, image))
                {
                    num++;
                }
                else
                {
                    return num;
                }
                if (BotCore.RGBComparer(new Point(1113, 140), 145, 118, 94, 30, image))
                {
                    num++;
                }
                else
                {
                    return num;
                }
                if (BotCore.RGBComparer(new Point(980, 239), 148, 128, 105, 30, image))
                {
                    num++;
                }
                else
                {
                    return num;
                }
                if (BotCore.RGBComparer(new Point(1113, 256), 150, 126, 100, 30, image))
                {
                    num++;
                }
                else
                {
                    return num;
                }
                return num;
            }
            else
            {
                int num = 0;
                Color star = Color.FromArgb(69, 47, 17);
                if(!BotCore.RGBComparer(new Point(877, 234), star, 30, image))
                {
                    num++;
                }
                if(!BotCore.RGBComparer(new Point(929, 237), star, 30, image))
                {
                    num++;
                }
                if(!BotCore.RGBComparer(new Point(986, 232), star, 30, image))
                {
                    num++;
                }
                if(!BotCore.RGBComparer(new Point(1032, 235), star, 30, image))
                {
                    num++;
                }
                if(!BotCore.RGBComparer(new Point(1080, 236), star, 30, image))
                {
                    num++;
                }
                if(!BotCore.RGBComparer(new Point(1131, 235), star, 30, image))
                {
                    num++;
                }
                return num;
            }
        }
        //Close Game and wait for energy in tower event for stucking rune time
        public static void StuckRune()
        {
            Debug_.WriteLine();
            Variables.ScriptLog("Close game and stuck rune!", Color.DarkGreen);
            Variables.ScriptLog("Estimate online time is " + nextOnline, Color.Lime);
            string filename = Encryption.SHA256(Variables.AdbIpPort);
            if (!Directory.Exists("C:\\ProgramData\\" + filename))
            {
                Directory.CreateDirectory("C:\\ProgramData\\" + filename);
            }
            if (!BotCore.Pull("/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", "C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
            {
                Variables.ScriptLog("Pull files failed", Color.Red);
            }
            else
            {
                Variables.ScriptLog("Backup saved", Color.Lime);
            }
            BotCore.KillGame(game);
            Variables.FindConfig("General","Manual_Rune", out string output);
            if (output == "true")
            {
                Console.Beep();
                BotCore.Delay(1000);
                Console.Beep();
                BotCore.Delay(1000);
                Console.Beep();
                EmulatorLoader.CloseEmulator();
                MessageBox.Show("正在卡符文！下次上线时间为" + nextOnline + "!");
                Environment.Exit(0);
            }
            if (nextOnline < DateTime.Now)
            {
                //We are now need continue our event!
                return;
            }
            //We can now go to archwitch event
            if (Variables.FindConfig("General", "SoWeEv", out string conf))
            {
                if (bool.Parse(conf))
                {
                    Variables.ScriptLog("Entering SoulWeapon Event", Color.AliceBlue);
                    if (!BotCore.GameIsForeground(game))
                    {
                        for (int e = 0; e < 10; e++)
                        {
                            Variables.ScriptLog("Starting Game", Color.Lime);
                            if (PrivateVariable.Instance.biubiu)
                            {
                                BotCore.StartGame("com.njh.biubiu/com.njh.ping.core.business.LauncherActivity");
                                do
                                {
                                    Variables.WinApiCapt = false;
                                    image = Screenshot.ImageCapture();
                                    var p = BotCore.FindImage(image, Img.biubiu, false, 0.8);
                                    if (p != null)
                                    {
                                        BotCore.SendTap(p.Value.X + 25, p.Value.Y - 600);
                                        BotCore.Delay(10000);
                                        BotCore.SendTap(620, 320);
                                        break;
                                    }
                                    else
                                    {
                                        p = BotCore.FindImage(image, Img.biubiu2, false, 0.8);
                                        if (p != null)
                                        {
                                            BotCore.SendTap(p.Value);
                                            break;
                                        }
                                    }
                                    BotCore.Delay(10000);
                                    Variables.WinApiCapt = true;
                                }
                                while (true);
                            }
                            BotCore.StartGame(game + activity);
                            BotCore.Delay(3000);
                            if (BotCore.GameIsForeground(game))
                            {
                                //nextOnline = DateTime.Now;
                                //Collected = false;
                                break;
                            }
                            else if (e == 9)
                            {
                                EmulatorLoader.RestartEmulator();
                            }
                        }
                    }
                    var temp = PrivateVariable.Instance.VCevent;
                    PrivateVariable.Instance.VCevent = PrivateVariable.EventType.SoulWeapon;
                    //Enter ArchwitchStage
                    SoulWeapon.SoulWeaponEnter();
                    PrivateVariable.Instance.VCevent = temp;
                }
            }
            if (Variables.FindConfig("General", "ArWiEv", out conf))
            {
                if (bool.Parse(conf))
                {
                    Variables.ScriptLog("Entering Archwitch Event", Color.AliceBlue);
                    if (!BotCore.GameIsForeground(game))
                    {
                        for (int e = 0; e < 10; e++)
                        {
                            Variables.ScriptLog("Starting Game", Color.Lime);
                            if (PrivateVariable.Instance.biubiu)
                            {
                                BotCore.StartGame("com.njh.biubiu/com.njh.ping.core.business.LauncherActivity");
                                do
                                {
                                    Variables.WinApiCapt = false;
                                    image = Screenshot.ImageCapture();
                                    var p = BotCore.FindImage(image, Img.biubiu, false, 0.8);
                                    if (p != null)
                                    {
                                        BotCore.SendTap(p.Value.X + 25, p.Value.Y - 600);
                                        BotCore.Delay(10000);
                                        BotCore.SendTap(620, 320);
                                        break;
                                    }
                                    else
                                    {
                                        p = BotCore.FindImage(image, Img.biubiu2, false, 0.8);
                                        if (p != null)
                                        {
                                            BotCore.SendTap(p.Value);
                                            break;
                                        }
                                    }
                                    BotCore.Delay(10000);
                                    Variables.WinApiCapt = true;
                                }
                                while (true);
                            }
                            BotCore.StartGame("com.nubee.valkyriecrusade/.GameActivity");
                            BotCore.Delay(3000);
                            if (BotCore.GameIsForeground(game))
                            {
                                //nextOnline = DateTime.Now;
                                //Collected = false;
                                break;
                            }
                            if (e == 9)
                            {
                                EmulatorLoader.RestartEmulator();
                            }
                        }
                    }
                    var temp = PrivateVariable.Instance.VCevent;
                    PrivateVariable.Instance.VCevent = PrivateVariable.EventType.ArchWitch;
                    //Enter ArchwitchStage
                    ArchwitchEvent.ArchwitchEnter();
                    PrivateVariable.Instance.VCevent = temp;
                }
            }
            if(DateTime.Now < nextOnline.AddMinutes(-5))
            {
                if (Variables.FindConfig("General", "Suspend_PC", out string suspend))
                {
                    if (suspend == "true")
                    {
                        SleepWake.SetWakeTimer(nextOnline.AddMinutes(-15));
                    }
                    else
                    {
                        do
                        {
                            BotCore.Delay(2000);
                            Guildwar.Enter();
                        }
                        while (DateTime.Now <= nextOnline.AddMinutes(-10));
                    }
                }
                else
                {
                    do
                    {
                        BotCore.Delay(2000);
                        if(Variables.FindConfig("GuildWar", "Manual", out string boolean))
                        {
                            if(boolean == "false")
                            {
                                Guildwar.Enter();
                            }
                        }
                    }
                    while (DateTime.Now <= nextOnline.AddMinutes(-10));
                }
            }
            energy = 5;
        }
        
        //No energy left so close game
        public static void NoEnergy()
        {
            Debug_.WriteLine();
            if (PrivateVariable.Instance.VCevent == PrivateVariable.EventType.Tower || PrivateVariable.Instance.VCevent == PrivateVariable.EventType.DemonRealm)
            {
                if(Variables.FindConfig("General", "SoWeEv", out string conf))
                {
                    if (bool.Parse(conf))
                    {
                        Variables.ScriptLog("Entering Soul Weapon Event", Color.AliceBlue);
                        var temp = PrivateVariable.Instance.VCevent;
                        PrivateVariable.Instance.VCevent = PrivateVariable.EventType.SoulWeapon;
                        SoulWeapon.SoulWeaponEnter();
                        PrivateVariable.Instance.VCevent = temp;
                    }
                }
                if (Variables.FindConfig("General", "ArWiEv", out conf))
                {
                    if (bool.Parse(conf))
                    {
                        Variables.ScriptLog("Entering Archwitch Event", Color.AliceBlue);
                        var temp = PrivateVariable.Instance.VCevent;
                        PrivateVariable.Instance.VCevent = PrivateVariable.EventType.ArchWitch;
                        //Enter ArchwitchStage
                        ArchwitchEvent.ArchwitchEnter();
                        PrivateVariable.Instance.VCevent = temp;
                    }
                }
            }
            Variables.ScriptLog("Estimate online time is " + nextOnline, Color.Lime);
            BotCore.KillGame(game);
            string filename = Encryption.SHA256(Variables.AdbIpPort);
            if (!Directory.Exists("C:\\ProgramData\\" + filename))
            {
                Directory.CreateDirectory("C:\\ProgramData\\" + filename);
            }
            if (!BotCore.Pull("/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", "C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
            {
                Variables.ScriptLog("Pull files failed", Color.Red);
            }
            else
            {
                Variables.ScriptLog("Backup saved", Color.Lime);
            }
            if (DateTime.Now < nextOnline.AddMinutes(-5))
            {
                if (Variables.FindConfig("General", "Suspend_PC", out string suspend))
                {
                    if (suspend == "true")
                    {
                        SleepWake.SetWakeTimer(nextOnline.AddMinutes(-15));
                    }
                    else
                    {
                        do
                        {
                            BotCore.Delay(2000);
                            Guildwar.Enter();
                        }
                        while (DateTime.Now <= nextOnline.AddMinutes(-10));
                    }
                }
                else
                {
                    do
                    {
                        BotCore.Delay(2000);
                        if (Variables.FindConfig("GuildWar", "Manual", out string boolean))
                        {
                            if (boolean == "false")
                            {
                                Guildwar.Enter();
                            }
                        }
                    }
                    while (DateTime.Now <= nextOnline.AddMinutes(-10));
                }
            }
        }
        //Read battle script plugins
        public static void Read_Plugins()
        {
            Debug_.WriteLine();
            if (!Directory.Exists("Battle_Script"))
            {
                Directory.CreateDirectory("Battle_Script");
            }
            string[] files = Directory.GetFiles("Battle_Script", "*.dll");
            if (files.Length > 0)
            {
                foreach (var f in files)
                {
                    var a = Assembly.LoadFrom(f);
                    foreach (var t in a.GetTypes())
                    {
                        if (t.GetInterface("BattleScript") != null)
                        {
                            PrivateVariable.Instance.BattleScript.Add(Activator.CreateInstance(t) as BattleScript);
                        }
                    }
                }
                foreach (var s in PrivateVariable.Instance.BattleScript)
                {
                    s.ReadConfig();
                }
            }
        }

        static bool Muted_Device = false;

        void ScriptInterface.Script()
        {
            if (Variables.FindConfig("General", "Lang", out string lang))
            {
                if (File.Exists("Profiles\\Verify.vcb"))
                {
                    FileInfo f = new FileInfo("Profiles\\Verify.vcb");
                    if((DateTime.Now.ToUniversalTime() - f.LastWriteTimeUtc) > new TimeSpan(1, 0, 0))
                    {
                        try
                        {
                            if (lang.Contains("cn"))
                            {
                                lang = "scn";
                            }
                            else
                            {
                                lang = "en";
                            }
                            WebClientOverride wc = new WebClientOverride();
                            var url = "https://d2n1d3zrlbtx8o.cloudfront.net/news/info/" + lang + "/index.html";
                            MainScreen.html = wc.DownloadString(new Uri(url));
                            MainScreen.html = MainScreen.html.Replace("bgcolor=\"#000000\"　text color=\"#FFFFFF\"", "style=\"background - color:#303030; color:white\"");
                            MainScreen.html = MainScreen.html.Remove(MainScreen.html.IndexOf("<table width=\"200\">"), MainScreen.html.IndexOf("</table>") - MainScreen.html.IndexOf("<table width=\"200\">"));
                            MainScreen.html = Regex.Replace(MainScreen.html, "(\\<span class\\=\"iro4\"\\>.*</span>)", "");
                        }
                        catch
                        {
                            Variables.ScriptLog("Verification failed! Server not reachable! Request again next round!", Color.Red);
                            MainScreen.html = Img.index;
                        }
                        if (MainScreen.html != Img.index)
                        {
                            if (File.ReadAllText("Profiles\\Verify.vcb") != Encryption.SHA256(MainScreen.html))
                            {
                                Variables.ScriptLog("Verification failed, regenerate new Verify.vcb", Color.Red);
                                //Event is updated
                                if (File.Exists("Img\\Event.png"))
                                {
                                    File.Delete("Img\\Event.png");
                                }
                                File.WriteAllText("Profiles\\Verify.vcb", Encryption.SHA256(MainScreen.html));
                            }
                            else
                            {
                                Variables.ScriptLog("VCBot verified", Color.White);
                                File.WriteAllText("Profiles\\Verify.vcb", Encryption.SHA256(MainScreen.html));
                            }
                        }
                    }
                   
                }
                else
                {
                    if(MainScreen.html != Img.index)
                    {
                        Variables.ScriptLog("VCBot verified", Color.White);
                        File.WriteAllText("Profiles\\Verify.vcb", Encryption.SHA256(MainScreen.html));
                    }
                }
            }
            Debug_.WriteLine();
            if (!CloseEmu)
            {
                BotCore.Delay(10, true);
                if (Variables.Controlled_Device != null) //The Emulator is running
                {
                    while (Variables.Proc == null)//But not registred on our Proc value
                    {
                        Debug_.WriteLine("Variables.Proc is null");
                        //so go on and find the emulator!
                        EmulatorLoader.ConnectAndroidEmulator();
                        //Emulator found!
                        if (Variables.Proc != null)
                        {
                            break;
                        }
                        //Maybe something is wrong, no process is connected!
                        EmulatorLoader.StartEmulator();
                    }
                }
                else //The Emulator is not exist!
                {
                    EmulatorLoader.StartEmulator(); //Start our fxxking Emulator!!
                    EmulatorLoader.ConnectAndroidEmulator();
                    return;
                }
                if (Variables.Proc.HasExited)
                {
                    Variables.Proc = null;
                    return;
                }
                int error = 0;
                BotCore.Delay(10, true);
                do
                {
                    BotCore.Delay(2200, false);
                    image = Screenshot.ImageCapture();
                    if (Variables.Controlled_Device == null) //Emulator not started, awaiting...
                    {
                        EmulatorLoader.ConnectAndroidEmulator();
                        return;
                    }
                    if (Variables.Proc == null)
                    {
                        EmulatorLoader.StartEmulator();
                        EmulatorLoader.ConnectAndroidEmulator();
                        return;
                    }
                    error++;
                    if (error > 60)
                    {
                        EmulatorLoader.RestartEmulator();
                        error = 0;
                    }
                } while (image == null);
                BotCore.Delay(10, true);
                string filename = Encryption.SHA256(Variables.AdbIpPort);
                if (!Directory.Exists("C:\\ProgramData\\" + filename))
                {
                    Directory.CreateDirectory("C:\\ProgramData\\" + filename);
                }

                if (!File.Exists("C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
                {
                    if (!BotCore.Pull("/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", "C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
                    {
                        Variables.ScriptLog("Pull files failed", Color.Red);
                    }
                    else
                    {
                        Variables.ScriptLog("Backup saved", Color.Lime);
                    }
                }
                else
                {
                    if (!pushed)
                    {
                        BotCore.Push("C:\\ProgramData\\" + filename + "\\" + filename + ".xml", "/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", 660);
                        pushed = true;
                        Variables.ScriptLog("Restored backup xml", Color.Lime);
                        BotCore.Delay(1000, false);
                    }
                }
            }
            if (Stuck)
            {
                StuckRune();
                Stuck = false;
                return;
            }
            BotCore.Delay(10, true);
            if (!BotCore.GameIsForeground(game))
            {
                for (int e = 0; e < 10; e++)
                {
                    Variables.ScriptLog("Starting Game", Color.Lime);
                    if (PrivateVariable.Instance.biubiu)
                    {
                        BotCore.StartGame("com.njh.biubiu/com.njh.ping.core.business.LauncherActivity");
                        do
                        {
                            Variables.WinApiCapt = false;
                            image = Screenshot.ImageCapture();
                            var p = BotCore.FindImage(image, Img.biubiu, false, 0.8);
                            if (p != null)
                            {
                                BotCore.SendTap(p.Value.X + 25, p.Value.Y - 600);
                                BotCore.Delay(10000);
                                BotCore.SendTap(620,320);
                                break;
                            }
                            else
                            {
                                p = BotCore.FindImage(image, Img.biubiu2, false, 0.8);
                                if (p != null)
                                {
                                    BotCore.SendTap(p.Value);
                                    break;
                                }
                            }
                            BotCore.Delay(10000);
                            Variables.WinApiCapt = true;
                        }
                        while (true);
                    }
                    BotCore.StartGame("com.nubee.valkyriecrusade/.GameActivity");
                    BotCore.Delay(3000);
                    if (BotCore.GameIsForeground(game))
                    {
                        nextOnline = DateTime.Now;
                        Collected = false;
                        break;
                    }
                    if (e == 9)
                    {
                        EmulatorLoader.RestartEmulator();
                    }
                }
            }
            else
            {
                var Japan = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                var time = TimeZoneInfo.ConvertTime(DateTime.Now, Japan).TimeOfDay;
                if (time.Hours == 12)//Event change time
                {
                    GetEventXML.LoadXMLEvent();
                }
                if (DateTime.Now > GetEventXML.guildwar && DateTime.Now < GetEventXML.guildwar.AddDays(9))
                {
                    Variables.ScriptLog("Guildwar is now running! ", Color.LightCyan);
                    if (Variables.FindConfig("GuildWar", "Manual", out string output))
                    {
                        if (bool.Parse(output))
                        {
                            WaitGuildWar(time);
                        }
                        else
                        {
                            //Direct attack guildwar
                            Guildwar.Enter();
                        }
                    }
                    else
                    {
                        WaitGuildWar(time);
                    }
                }
                if (!PrivateVariable.Instance.InMainScreen && !PrivateVariable.Instance.InEventScreen && !PrivateVariable.Instance.Battling)
                {
                    LocateMainScreen();
                }
                else
                {
                    if (!PrivateVariable.Instance.InEventScreen)
                    {
                        CheckEvent();
                    }
                    else
                    {
                        if (!PrivateVariable.Instance.Battling)
                        {
                            switch (PrivateVariable.Instance.VCevent)
                            {
                                case PrivateVariable.EventType.Tower:
                                    TowerEvent.Tower();
                                    break;
                                case PrivateVariable.EventType.ArchWitch:
                                    break;
                                case PrivateVariable.EventType.SoulWeapon:
                                    SoulWeapon.SoulWeaponEnter();
                                    break;
                                case PrivateVariable.EventType.DemonRealm:
                                    DemonRealm.Demon_Realm();
                                    break;
                                default:
                                    Variables.ScriptLog("Unknown error occur, unable to detect event type.", Color.Red);
                                    PrivateVariable.Instance.InEventScreen = false;
                                    break;
                            }
                        }
                        else
                        {
                            Battle();
                        }
                    }
                }
            }
        }

        private static void WaitGuildWar(TimeSpan time)
        {
            var hour = time.Hours;
            if (hour == 8 || hour == 12 || hour == 19 || hour == 22)
            {
                Console.Beep();
                ScriptErrorHandler.Reset("Guild War is running, waiting for end...");
                double seconds = 0;
                Console.Beep();
                switch (hour)
                {
                    case 8:
                        seconds = (TimeSpan.Parse("8:59:59") - time).TotalMilliseconds;
                        Variables.ScriptLog("Will start game at Japan time 8:59:59", Color.YellowGreen);
                        break;
                    case 12:
                        seconds = (TimeSpan.Parse("12:59:59") - time).TotalMilliseconds;
                        Variables.ScriptLog("Will start game at Japan Time 12:59:59", Color.YellowGreen);
                        break;
                    case 19:
                        seconds = (TimeSpan.Parse("19:59:59") - time).TotalMilliseconds;
                        Variables.ScriptLog("Will start game at Japan Time 19:59:59", Color.YellowGreen);
                        break;
                    case 22:
                        seconds = (TimeSpan.Parse("23:59:59") - time).TotalMilliseconds;
                        Variables.ScriptLog("Will start game at Japan Time 23:59:59", Color.YellowGreen);
                        break;
                }
                BotCore.KillGame(game);
                BotCore.Delay(Convert.ToInt32(seconds), true);
            }
        }
        public void ResetScript()
        {
            ScriptErrorHandler.Reset("Reset script!");
        }
    }
}

