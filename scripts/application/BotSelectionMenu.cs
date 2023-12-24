using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class BotSelectionMenu : Node
{
    [Export] Node buttonGridHolder;
    //[Export] Label botInfoLabel;
    [Export] RichTextLabel botInfoLabel;
    [Export] Color colBotName;
    [Export] Color colAuthorName;
    [Export] Color colDescription;
    [Export] Color colRating;
    [Export] string[] specials;
    [Export] Color wonCol;
    [Export] Color wonHoverCol;
    [Export] Color drawCol;
    [Export] Color drawHoverCol;

    [Export] Button resolution_720;
    [Export] Button resolution_1080;
    [Export] Button resolution_1440;
    [Export] Button toggleFullscreen;
    [Export] Button quitButton;
    [Export] Button vidButton;
    [Export] Button creditsButton;
    [Export] Node2D selectionMenuHolder;
    [Export] Node2D creditsMenuHolder;

    BotInfo[] bots;

    void SetUpBots()
    {
        bots = new BotInfo[]
        {

           new BotInfo()
           {
               name = "The CopyCat",
               nameFull = "The CopyCat",
               saveID = "CopyCat",
               author = "catur_",
               rating = 192,
               description = "Tries to mirror the moves of its opponent as closely as possible. \nFar more impressive to lose to than to defeat!",
               type = typeof(auto_Bot_17.Bot_17)
           },
            new BotInfo()
           {
               name = "BongcloudEnthusiast",
               nameFull = "Bongcloud Enthusiast",
               saveID = "BongcloudEnthusiast",
               author = "CreeperXD",
               rating = 307,
               description = "This bot will play the bongcloud no matter what. Beyond that, it moves randomly unless it can capture a piece, promote a pawn, or deliver checkmate!",
               type = typeof(auto_Bot_278.Bot_278)
           },
            new BotInfo()
            {
                name = "WhateverBot",
                nameFull = "WhateverBot",
                saveID = "WhateverBot",
                author = "Alec Holden",
                rating = 677,
                description = "This bot tries it's best to play a good game of chess. It's only capable of looking one move ahead though, so it doesn't often achieve that goal. Still, it appreciates the value of developing pieces to safe squares, and retreating them when under attack.",
                type = typeof(auto_Bot_153.Bot_153)
            },
            new BotInfo()
            {
                name = "applemethod",
                nameFull = "applemethod-orz",
                saveID = "applemethod",
                author = $"RedBlackTree",
                rating = 1085,
                description = "This bot cares only about material and tempo, and spends most of its time dreaming about capturing your pieces. Unlike the bots before it though, it's capable of looking ahead several moves into the future, so you'd best be on high alert!",
                type = typeof(auto_Bot_70.Bot_70)
            },
            new BotInfo()
            {
                name = "Squeedo",
                nameFull = "Squeedo",
                saveID = "Squeedo",
                author = "David Giesegh",
                rating = 1276,
                description = "This bot likes to advance its pawns, develop its pieces, and get its king to safety. It also prefers positions where it has lots of possible moves, and its opponent has very few. Struggles with tactical lines though, often thinking that hanging pieces are doomed, instead of fully calculating the outcome.",
                type = typeof(auto_Bot_253.Bot_253)
            },
            new BotInfo()
            {
                name = "Monstrosity 200",
                nameFull = "200 Tokens Monstrosity",
                saveID = "Monstrosity200",
                author = "ErwanF",
                rating = 1569,
                description = "This bot uses only 200 of the available 1024 tokens (a personal challenge by the author), but is surprisingly strong nonethless. It cares about material and piece mobility, and has a decent search algorithm squeezed into its tiny brain, allowing it to calculate far better than the bots before it.",
                type = typeof(auto_Bot_425.Bot_425)
            },
            new BotInfo()
            {
                name = "GameTechExplained",
                nameFull = "Game Tech Explained",
                saveID = "GameTechExplained",
                author = "Game Tech Explained",
                rating = 1713,
                description = "This bot has some idea of which squares its pieces will generally be good on, and is capable of looking ahead to figure out how to get them there. More importantly though, the code has been written to resemble a pawn. Some claim that this gives the bot superhuman chess powers, rendering it impossible for mere mortals to defeat.",
                type = typeof(auto_Bot_266.Bot_266)
            },
            new BotInfo()
            {
                name = "Electric Shockwave",
                nameFull = "Electric Shockwave Gambit",
                saveID = "ElectricShockwaveGambit",
                author = "Valentin",
                rating = 2001,
                description = "In the tiny chess bot tournament, this sneaky bot tried to exploit a loop-hole in the rules in order to stun its opponents while they were thinking. Beyond that, it values both the mobility and safety of its pieces (and the opposite for its enemy of course!), and is able to look quite far ahead to achieve this.",
                type = typeof(auto_Bot_303.Bot_303)
            },
            new BotInfo()
            {
                name = "King Gambot IV",
                nameFull = "King Gambot IV",
                saveID = "KingGambotIV",
                author = "ToTheAnd (aka toanth)",
                rating = 2172,
                description = "This bot uses many techniques to search deep into the position. It has a good understanding of where its pieces should be placed, but is hindered drastically by the king's (usually misplaced) faith in his ability to lead the attack. This entry was intended as a joke, but is nonetheless a force to be reckoned with.",
                type = typeof(auto_Bot_628.Bot_628)
            },
            new BotInfo()
            {
                name = "NNBot",
                nameFull = "NNBot",
                saveID = "NNBot",
                author = "Jamie Whiting",
                rating = 2246,
                description = "This bot uses a neural network for its positional evaluations, though due to the competition's size limit, the network had to be very small and cleverly compressed. This gives it a good understanding of tempo and piece placement, and it's capable of searching fairly far into the future.",
                type = typeof(auto_Bot_529.Bot_529)
            },
            new BotInfo()
            {
                name = "TinyHugeBot",
                nameFull = "TinyHugeBot",
                saveID = "TinyHugeBot",
                author = "Popax21 & atpx8",
                rating = 2513,
                description = "This team found a very clever exploit that allowed them to pack almost 5x as much code into their entry as anyone else, while still technically remaining within the limit. It has a good understanding of piece placement, and can quickly search quite deep into the position.",
                type = typeof(auto_Bot_610.Bot_610)
            },
            new BotInfo()
            {
                name = "Boychesser",
                nameFull = "Boychesser",
                saveID = "Boychesser",
                author = "MinusKelvin, Analog Hors, Anon, Algorhythm, and Daniel Ke",
                rating = 2772,
                description = "The undisputed winner of the tiny chess bot tournament. This bot has been painstakingly optimized to see further ahead than all the others, allowing it often to crush its foes with tactical blows. Beyond this, it has a good understanding of piece placement, mobility, and pawn structure.",
                type = typeof(auto_Bot_614.Bot_614)
            },
        };
    }

    struct BotInfo
    {
        public string name;
        public string nameFull;
        public string author;
        public string description;
        public int rating;
        public Type type;
        public string saveID;
        public int id => int.Parse(type.ToString().Split("_")[3], System.Globalization.CultureInfo.InvariantCulture);
    }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetUpBots();
        InitBottomMenuButtons();

        InitBotButtons();

        // Init other ui
        botInfoLabel.Text = "";
        selectionMenuHolder.Visible = true;
        creditsMenuHolder.Visible = false;

        void InitBottomMenuButtons()
        {
            resolution_720.Pressed += () => SetWindowSize(1280, 720);
            resolution_1080.Pressed += () => SetWindowSize(1920, 1080);
            resolution_1440.Pressed += () => SetWindowSize(2560, 1440);
            toggleFullscreen.Pressed += ToggleFullscreen;
            quitButton.Pressed += Quit;
            vidButton.Pressed += OpenVid;
            creditsButton.Pressed += ToggleCreditsMenu;
        }

        string FormatButtonName(string botName, int number)
        {
            int maxLen = 0;
            for (int i = 0; i < bots.Length; i++)
            {
                string n = (i + 1) + ". " + bots[i].name;
                maxLen = Math.Max(maxLen, n.Length);
            }

            string prefix = number + ". ";
            if (number < 10) prefix = " " + prefix;
            string formatted = prefix + botName;

            int numSpacesToAdd = maxLen - formatted.Length;

            int numPre = prefix.Length;
            int numPost = 0;

            for (int j = 0; j < numSpacesToAdd; j++)
            {
                if (numPost <= numPre)
                {
                    formatted += " ";
                    numPost++;
                }
                else
                {
                    numPre++;
                    formatted = formatted.Insert(formatted.IndexOf(".") + 1, " ");
                }
                // botNameFormatted = botNameFormatted + " ";
            }

            return formatted;
        }

        void InitBotButtons()
        {
            string[] ids = bots.Select(b => b.saveID).ToArray();
            BotSettings.Stats = PlayerStats.TryLoadBotStats(ids);
            PlayerStats.SaveBotStats(BotSettings.Stats);

            Button[] botButtons = GetAllBotButtons();

            for (int i = 0; i < botButtons.Length; i++)
            {
                Button button = botButtons[i];
                string botName = i < bots.Length ? bots[i].name : "?";

                button.Text = FormatButtonName(botName, i + 1);
                int botIndex = i;

                // Events
                button.Pressed += () => OnPressed(botIndex);
                button.MouseEntered += () => ButtonHover(botIndex);
                button.MouseExited += ButtonHoverExit;

                // Stats
                if (i < bots.Length)
                {
                    (int w, int d, int l) = BotSettings.Stats.GetWDL(bots[i].saveID);
                    button.GetChild<Label>(0).Text = $"Wins: {w} Draws: {d} Losses: {l}";

                    if (w > 0)
                    {
                        SetButtonCol(button, wonCol, wonHoverCol);
                    }
                    else if (d > 0)
                    {
                        SetButtonCol(button, drawCol, drawHoverCol);
                    }
                }
            }
        }
    }

    void ToggleCreditsMenu()
    {
        creditsMenuHolder.Visible = !creditsMenuHolder.Visible;
        selectionMenuHolder.Visible = !selectionMenuHolder.Visible;
    }

    void OpenVid()
    {
        Helpers.OpenUrl("https://youtu.be/Ne40a5LkK6A?si=rB0B0NGhlGqQi8TZ");
    }



    void SetButtonCol(Button button, Color colNormal, Color colHover)
    {
        var styleBox = (StyleBoxFlat)button.GetThemeStylebox("normal").Duplicate();
        var hoverWon = (StyleBoxFlat)button.GetThemeStylebox("hover").Duplicate();
        styleBox.BgColor = colNormal;
        hoverWon.BgColor = colHover;

        button.AddThemeStyleboxOverride("normal", styleBox);
        button.AddThemeStyleboxOverride("hover", hoverWon);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Quit();
        }
    }

    void ButtonHover(int botIndex)
    {
        string desc = botIndex < bots.Length ? bots[botIndex].description : "?";
        string botName = botIndex < bots.Length ? bots[botIndex].nameFull : "?";
        string authorName = botIndex < bots.Length ? bots[botIndex].author : "?";
        int rating = botIndex < bots.Length ? bots[botIndex].rating : -1;



        string text = $"[center]{ColToBBCode(colBotName)}{botName}{ColToBBCode(new Color(1, 1, 1, 0.5f))}";
        string eloDesc = botName == "Boychesser" ? "(CCRL blitz)" : "(estimated CCRL blitz)";
        text += $" by {ColToBBCode(colAuthorName)}{authorName}. {ColToBBCode(colRating)}ELO: {rating} {eloDesc}.\n{ColToBBCode(colDescription)}{desc}";
        botInfoLabel.Text = text;
    }

    void ButtonHoverExit()
    {
        botInfoLabel.Text = "";
    }

    void OnPressed(int botIndex)
    {
        BotSettings.ActiveBotName = bots[botIndex].nameFull;
        BotSettings.ActiveBotType = bots[botIndex].type;
        BotSettings.ActiveBotSaveID = bots[botIndex].saveID;
        BotSettings.ActiveBotID = bots[botIndex].id;
        BotSettings.ActiveBotIndex = botIndex;
        SceneHelper.SwitchScene(GetTree(), SceneHelper.chessScene);

    }

    Button[] GetAllBotButtons()
    {
        List<Button> allBotButtons = new();

        foreach (Node verticalChild in buttonGridHolder.GetChildren())
        {
            if (verticalChild is HBoxContainer)
            {
                foreach (Node horizontalChild in verticalChild.GetChildren())
                {
                    if (horizontalChild is Button)
                    {
                        allBotButtons.Add(horizontalChild as Button);
                    }
                }
            }
        }

        return allBotButtons.ToArray();
    }

    public static void ToggleFullscreen()
    {
        bool isFullscreen = DisplayServer.WindowGetMode() is DisplayServer.WindowMode.Fullscreen;
        var targetMode = isFullscreen ? DisplayServer.WindowMode.Windowed : DisplayServer.WindowMode.Fullscreen;
        DisplayServer.WindowSetMode(targetMode);
    }

    public static void SetWindowSize(int width, int height)
    {
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        Vector2I windowSize = new Vector2I(width, height);
        Vector2I windowCentre = screenSize / 2;
        Vector2I windowTopLeft = windowCentre - windowSize / 2;
        DisplayServer.WindowSetSize(windowSize);
        DisplayServer.WindowSetPosition(windowTopLeft);
    }

    void Quit()
    {
        GetTree().Quit();
    }

    public string ColToBBCode(Color color)
    {
        int r = (int)(color.R * 255);
        int g = (int)(color.G * 255);
        int b = (int)(color.B * 255);
        int a = (int)(color.A * 255);

        return $"[color=#{r:X2}{g:X2}{b:X2}{a:X2}]";
    }
}
