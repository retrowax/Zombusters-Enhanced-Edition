using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;
using ZombustersWindows.Subsystem_Managers;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    public class SelectPlayerScreen : BackgroundScreen
    {
        private const int LINE_X_OFFSET = 40;
        private const int MENU_TITLE_Y_OFFSET = 400;

        MyGame game;
        Rectangle uiBounds;
        Vector2 selectPos;

        Texture2D btnLT, btnRT;

        Texture2D logoMenu;
        Texture2D lineaMenu;
        Texture2D arrow;
        Texture2D myGroupBKG;
        Texture2D HTPHeaderImage;
        Texture2D kbUp;
        Texture2D kbDown;

        List<Texture2D> avatarTexture, avatarPixelatedTexture, cardBkgTexture;
        List<String> avatarNameList;

        NeutralInput[] playerInputList; 
        NeutralInput playerOneInput;
        NeutralInput playerTwoInput;
        NeutralInput playerThreeInput;
        NeutralInput playerFourInput;

        SpriteFont DigitBigFont, DigitLowFont;
        SpriteFont MenuHeaderFont;
        SpriteFont MenuInfoFont;
        SpriteFont MenuListFont;

        private MenuComponent menu;
        readonly Boolean isMatchmaking;
        Boolean canStartGame;
        Boolean isMMandHost;
        public byte levelSelected;
        Boolean isThereAPlayerWithKeyboard = false;

        public SelectPlayerScreen(Boolean ismatchmaking)
        {
            this.isMatchmaking = ismatchmaking;
        }

        public override void Initialize()
        {
            this.game = (MyGame)this.ScreenManager.Game;

            byte i;
            Viewport view = this.ScreenManager.GraphicsDevice.Viewport;
            int borderheight = (int)(view.Height * .05);

            // Deflate 10% to provide for title safe area on CRT TVs
            uiBounds = GetTitleSafeArea();

            //"Select" text position
            selectPos = new Vector2(uiBounds.X + 60, uiBounds.Bottom - 30);

            DigitBigFont = game.Content.Load<SpriteFont>(@"menu\DigitBig");
            DigitLowFont = game.Content.Load<SpriteFont>(@"menu\DigitLow");
            MenuHeaderFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuHeader");
            MenuInfoFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuInfo");
            MenuListFont = game.Content.Load<SpriteFont>(@"menu\ArialMenuList");

            HTPHeaderImage = this.ScreenManager.Game.Content.Load<Texture2D>(@"menu/lobby_header_image");

            menu = new MenuComponent(game, MenuListFont);
            menu.Initialize();
            menu.uiBounds = menu.Extents;

            //Offset de posicion del menu
            menu.uiBounds.Offset(uiBounds.X, 300);

            avatarTexture = new List<Texture2D>(4);
            avatarPixelatedTexture = new List<Texture2D>(4);
            cardBkgTexture = new List<Texture2D>(4);
            for (i = 0; i < 4; i++)
            {
                game.currentPlayers[i].character = 0;
            }
            avatarNameList = new List<string>(4);
            avatarNameList.Add("Tracy");
            avatarNameList.Add("Charles");
            avatarNameList.Add("Ryan");
            avatarNameList.Add("Peter");

            levelSelected = 1;
            canStartGame = false;
            isMMandHost = false;

            game.currentPlayers[0].Player.LoadSavedGame();

            // Nos aseguramos que no se queda marcado el flag de selecci�n de personaje
            foreach (Avatar player in game.currentPlayers)
            {
                if (game.currentPlayers[0].Equals(player))
                {
                    player.isReady = false;
                    player.status = ObjectStatus.Active;
                }
                else
                {
                    player.isReady = false;
                    player.status = ObjectStatus.Inactive;
                }
            }

            playerInputList = new NeutralInput[] { playerOneInput, playerTwoInput, playerThreeInput, playerFourInput };

            base.Initialize();

            this.isBackgroundOn = true;
        }

        public override void LoadContent()
        {
            byte i;

            // Key "Up"
            kbUp = game.Content.Load<Texture2D>(@"Keyboard/key_up");

            // Key "Down"
            kbDown = game.Content.Load<Texture2D>(@"Keyboard/key_down");

            // Button Left Trigger
            btnLT = game.Content.Load<Texture2D>("xboxControllerLeftTrigger");

            // Button Right Trigger
            btnRT = game.Content.Load<Texture2D>("xboxControllerRightTrigger");

            //Linea blanca separatoria
            lineaMenu = game.Content.Load<Texture2D>(@"menu/linea_menu");

            //Logo Menu
            logoMenu = game.Content.Load<Texture2D>(@"menu/logo_menu");

            //Arrow
            arrow = game.Content.Load<Texture2D>(@"InGame/SelectPlayer/arrowLeft");

            //Fondo negro fade menu
            myGroupBKG = game.Content.Load<Texture2D>(@"menu/mygroup_bkg");

            for (i=1; i <= 4; i++)
            {
                avatarTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/p" + i.ToString() + "Avatar"));
                avatarPixelatedTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/p" + i.ToString() + "Avatar_pixel"));
            }

            cardBkgTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_bkg"));
            cardBkgTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_border"));
            cardBkgTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_gradient"));
            cardBkgTexture.Add(game.Content.Load<Texture2D>(@"InGame/SelectPlayer/selpla_ready"));

            base.LoadContent();
        }

        void ToggleReady(Avatar avatar)
        {
            if (avatar.isReady == true)
            {
                avatar.isReady = false;
            }
            else
            {
                avatar.isReady = true;
            }
        }

        void ChangeCharacterSelected(Avatar avatar, Boolean directionToRight)
        {
            if (directionToRight == true)
            {
                if (avatar.isReady == false)
                {
                    avatar.character += 1;
                    if (avatar.character > 3)
                    {
                        avatar.character = 0;
                    }
                }
            }
            else
            {
                if (avatar.isReady == false)
                {
                    avatar.character -= 1;
                    if (avatar.character < 0 || avatar.character > 3)
                    {
                        avatar.character = 3;
                    }
                }
            }
        }

        public override void HandleInput(InputState input)
        {
            if (game.currentPlayers[0].Player.Controller == PlayerIndex.One)
                playerOneInput = ProcessPlayer(game.currentPlayers[0], input);
            if (game.currentPlayers[1].Player.Controller == PlayerIndex.Two)
                playerTwoInput = ProcessPlayer(game.currentPlayers[1], input);
            if (game.currentPlayers[2].Player.Controller == PlayerIndex.Three)
                playerThreeInput = ProcessPlayer(game.currentPlayers[2], input);
            if (game.currentPlayers[3].Player.Controller == PlayerIndex.Four)
                playerFourInput = ProcessPlayer(game.currentPlayers[3], input);

            base.HandleInput(input);
        }


        private NeutralInput ProcessPlayer(Avatar avatar, InputState input)
        {
            NeutralInput state = new NeutralInput
            {
                Fire = Vector2.Zero
            };
            Vector2 stickLeft = Vector2.Zero;
            Vector2 stickRight = Vector2.Zero;
            GamePadState gpState = input.GetCurrentGamePadStates()[(int)avatar.Player.Controller];

            if (avatar.IsPlaying)
            {
                if ((input.IsNewButtonPress(Buttons.A, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
                || ((input.IsNewKeyPress(Keys.Space, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard)))
                {
                    ToggleReady(avatar);
                    state.ButtonA = true;
                }
                else
                {
                    state.ButtonA = false;
                }

                if ((input.IsNewButtonPress(Buttons.DPadLeft, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
                    || ((input.IsNewButtonPress(Buttons.LeftThumbstickLeft, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
                    || ((input.IsNewKeyPress(Keys.Left, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard))))
                {
                    if ((gpState.DPad.Left == ButtonState.Pressed) || (gpState.ThumbSticks.Left.X < 0)
                    || input.IsNewKeyPress(Keys.Left, avatar.Player.Controller)                           
                        )
                    {
                        ChangeCharacterSelected(avatar, false);
                        state.DPadLeft = true;
                    }
                    else
                    {
                        state.DPadLeft = false;
                    }
                }
                else
                {
                    state.DPadLeft = false;
                }

                if ((input.IsNewButtonPress(Buttons.DPadRight, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
                    || ((input.IsNewButtonPress(Buttons.LeftThumbstickRight, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
                    || ((input.IsNewKeyPress(Keys.Right, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard))))
                {
                    if ((gpState.DPad.Right == ButtonState.Pressed) || (gpState.ThumbSticks.Left.X > 0)
                    || input.IsNewKeyPress(Keys.Right, avatar.Player.Controller)    
                        )
                    {
                        ChangeCharacterSelected(avatar, true);
                        state.DPadRight = true;
                    }
                    else
                    {
                        state.DPadRight = false;
                    }
                }
                else
                {
                    state.DPadRight = false;
                }

                if ((input.IsNewButtonPress(Buttons.DPadDown, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
                || ((input.IsNewKeyPress(Keys.Down, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard)
                || ((input.IsNewButtonPress(Buttons.LeftTrigger, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad))))
                {
#if !DEMO
                    if (levelSelected > 1)
                    {
                        levelSelected -= 1;
                    }
                    else
                    {
                        if (avatar.Player.levelsUnlocked != 0)
                        {
                            levelSelected = (byte)avatar.Player.levelsUnlocked;
                        }
                        else
                        {
                            levelSelected = 1;
                        }
                    }
#endif
                    state.ButtonLT = true;
                }
                else
                {
                    state.ButtonLT = false;
                }

                if ((input.IsNewButtonPress(Buttons.DPadUp, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
                || ((input.IsNewKeyPress(Keys.Up, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard)
                || ((input.IsNewButtonPress(Buttons.RightTrigger, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard))))
                {
#if !DEMO
                    if (levelSelected < avatar.Player.levelsUnlocked)
                    {
                        levelSelected += 1;
                    }
                    else
                    {
                        levelSelected = 1;
                    }
#endif
                    state.ButtonRT = true;
                }
                else
                {
                    state.ButtonRT = false;
                }

                stickLeft = gpState.ThumbSticks.Left;
                stickRight = gpState.ThumbSticks.Right;
                state.Fire = gpState.ThumbSticks.Right;
                state.StickLeftMovement = stickLeft;
                state.StickRightMovement = stickRight;
            } else
            {
                if (input.IsNewButtonPress(Buttons.Start, avatar.Player.Controller))
                {
                    canStartGame = false;
                    avatar.Player.inputMode = InputMode.GamePad;
                    avatar.Activate(avatar.Player);
                    avatar.isReady = false;
                }
                if (input.IsNewKeyPress(Keys.Enter, avatar.Player.Controller) && !isThereAPlayerWithKeyboard)
                {
                    canStartGame = false;
                    avatar.Player.inputMode = InputMode.Keyboard;
                    avatar.Activate(avatar.Player);
                    avatar.isReady = false;
                    isThereAPlayerWithKeyboard = true;
                }
            }

            if ((input.IsNewButtonPress(Buttons.B, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
                || ((input.IsNewKeyPress(Keys.Escape, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard)
                || ((input.IsNewKeyPress(Keys.Back, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard))))
            {
                MenuCanceled(this);
                state.ButtonB = true;
            }
            else
            {
                state.ButtonB = false;
            }

            if ((input.IsNewButtonPress(Buttons.Start, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.GamePad)
            || ((input.IsNewKeyPress(Keys.Enter, avatar.Player.Controller) && avatar.Player.inputMode == InputMode.Keyboard)))
            {
                if (canStartGame)
                {
                    List<int> ListPlayersAreGoingToPlay = new List<int>();
                    for (int i = 0; i < game.currentPlayers.Length; i++)
                    {
                        if (game.currentPlayers[i].status == ObjectStatus.Active)
                        {
                            ListPlayersAreGoingToPlay.Add(i);
                        }
                    }

                    switch (levelSelected)
                    {
                        case 1:
                            game.BeginLocalGame(LevelType.One, ListPlayersAreGoingToPlay);
                            break;
                        case 2:
                            game.BeginLocalGame(LevelType.Two, ListPlayersAreGoingToPlay);
                            break;
                        case 3:
                            game.BeginLocalGame(LevelType.Three, ListPlayersAreGoingToPlay);
                            break;
                        case 4:
                            game.BeginLocalGame(LevelType.Four, ListPlayersAreGoingToPlay);
                            break;
                        case 5:
                            game.BeginLocalGame(LevelType.Five, ListPlayersAreGoingToPlay);
                            break;
                        case 6:
                            game.BeginLocalGame(LevelType.Six, ListPlayersAreGoingToPlay);
                            break;
                        case 7:
                            game.BeginLocalGame(LevelType.Seven, ListPlayersAreGoingToPlay);
                            break;
                        case 8:
                            game.BeginLocalGame(LevelType.Eight, ListPlayersAreGoingToPlay);
                            break;
                        case 9:
                            game.BeginLocalGame(LevelType.Nine, ListPlayersAreGoingToPlay);
                            break;
                        case 10:
                            game.BeginLocalGame(LevelType.Ten, ListPlayersAreGoingToPlay);
                            break;
                        default:
                            game.BeginLocalGame(LevelType.One, ListPlayersAreGoingToPlay);
                            break;
                    }
                }
                state.ButtonStart = true;
            }
            else
            {
                state.ButtonStart = false;
            }


            // Read in our gestures
            foreach (GestureSample gesture in input.GetGestures())
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    // Toggle Ready
                    if ((gesture.Position.X >= 489 && gesture.Position.X <= 775) &&
                        (gesture.Position.Y >= 612 && gesture.Position.Y <= 648))
                    {
                        state.ButtonA = true;
                    }

                    // Toggle Ready CARD
                    if ((gesture.Position.X >= 198 && gesture.Position.X <= 336) &&
                        (gesture.Position.Y >= 288 && gesture.Position.Y <= 586))
                    {
                        state.ButtonA = true;
                    }

                    // Start Game
                    if ((gesture.Position.X >= 815 && gesture.Position.X <= 1104) &&
                        (gesture.Position.Y >= 612 && gesture.Position.Y <= 648))
                    {
                        state.ButtonStart = true;
                    }


                    // Select Player Right
                    if ((gesture.Position.X >= 341 && gesture.Position.X <= 368) &&
                        (gesture.Position.Y >= 412 && gesture.Position.Y <= 452))
                    {
                        state.DPadRight = true;
                    }

                    // Select Player Left
                    if ((gesture.Position.X >= 171 && gesture.Position.X <= 191) &&
                        (gesture.Position.Y >= 412 && gesture.Position.Y <= 452))
                    {
                        state.DPadLeft = true;
                    }

                    // Select Level Left
                    if ((gesture.Position.X >= 907 && gesture.Position.X <= 995) &&
                        (gesture.Position.Y >= 75 && gesture.Position.Y <= 176))
                    {
                        state.ButtonLT = true;
                    }

                    // Select Level Right
                    if ((gesture.Position.X >= 1099 && gesture.Position.X <= 1204) &&
                        (gesture.Position.Y >= 75 && gesture.Position.Y <= 176))
                    {
                        state.ButtonRT = true;
                    }

                    // Go Back Button
                    if ((gesture.Position.X >= 156 && gesture.Position.X <= 442) &&
                        (gesture.Position.Y >= 614 && gesture.Position.Y <= 650))
                    {
                        state.ButtonB = true;
                    }

                    // BUY NOW!!
                    //if ((gesture.Position.X >= 0 && gesture.Position.X <= 90) &&
                    //    (gesture.Position.Y >= 0 && gesture.Position.Y <= 90))
                    //{
                    //    if (Guide.IsTrialMode)
                    //    {
                    //        Guide.ShowMarketplace(PlayerIndex.One);
                    //    }
                    //}
                }
            }

            return state;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
            bool coveredByOtherScreen)
        {
            if (game.currentGameState != GameState.Paused)
            {
                CheckIfCanStartTheGame();
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public void UpdateCurrentPlayersList()
        {

        }

        private void CheckIfCanStartTheGame()
        {
            int playersActive = 0;
            int numPlayersReady = 0;

            foreach (Avatar player in game.currentPlayers)
            {
                if (player.status == ObjectStatus.Active)
                {
                    playersActive += 1;
                }

                if (player.isReady == true)
                {
                    numPlayersReady += 1;
                }
            }

            if (playersActive != 0)
            {
                if (playersActive == numPlayersReady)
                {
                    canStartGame = true;
                }
                else
                {
                    canStartGame = false;
                }
            }
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ExitScreen();
        }

        // Leave match
        void MenuCanceled(Object sender)
        {
            MessageBoxScreen confirmExitMessageBox =
                                    new MessageBoxScreen(Strings.CharacterSelectString, Strings.ConfirmReturnMainMenuString);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            if (!IsConfirmationScreenAlreadyShown())
            {
                ScreenManager.AddScreen(confirmExitMessageBox);
            }
        }

        private bool IsConfirmationScreenAlreadyShown()
        {
            bool hasConfirmationScreen = false;
            GameScreen[] screenList = ScreenManager.GetScreens();
            for (int i = screenList.Length - 1; i > 0; i--)
            {
                if (screenList[i] is MessageBoxScreen)
                {
                    hasConfirmationScreen = true;
                }
            }
            return hasConfirmationScreen
        }

        void Menu_ShowMarketPlace(Object sender, MenuSelection selection)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (game.currentGameState != GameState.Paused)
            {
#if !DEMO
                DrawLevelSelectionMenu(this.ScreenManager.SpriteBatch);
#endif
                DrawSelectCharacters(selectPos, this.ScreenManager.SpriteBatch);
                DrawContextMenu(menu, selectPos, this.ScreenManager.SpriteBatch);
                //DrawHowToPlay(this.ScreenManager.SpriteBatch, selectPos);
                menu.DrawLogoRetrowaxMenu(this.ScreenManager.SpriteBatch, new Vector2(uiBounds.Width, uiBounds.Height), MenuInfoFont);
#if DEMO
                menu.DrawDemoWIPDisclaimer(this.ScreenManager.SpriteBatch);
#endif
            }
            else
            {
                this.ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());
                this.ScreenManager.SpriteBatch.Draw(game.blackTexture, new Vector2(0, 0), Color.White);
                this.ScreenManager.SpriteBatch.DrawString(MenuInfoFont, "ZOMBUSTERS " + Strings.Paused.ToUpper(), new Vector2(5, 5), Color.White);
                this.ScreenManager.SpriteBatch.End();
            }
        }

        private void DrawSelectCharacters(Vector2 pos, SpriteBatch batch)
        {
            string[] lines;
            Avatar cplayer;
            byte index;
            Vector2 contextMenuPosition = new Vector2(uiBounds.X + 22, pos.Y - 100);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);
            Vector2 CharacterPosition = new Vector2(MenuTitlePosition.X + 50, MenuTitlePosition.Y + 70);
            Vector2 ConnectControlerStringPosition = new Vector2(CharacterPosition.X - 5 + cardBkgTexture[1].Width, CharacterPosition.Y + cardBkgTexture[1].Height / 2);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            //foreach (Avatar cplayer in game.currentPlayers)
            for (index = 0; index < game.currentPlayers.Length; index++)
            {
                cplayer = game.currentPlayers[index];

                if (cplayer.Player != null)
                {
                    if (cplayer.status == ObjectStatus.Active)
                    {
                        batch.Draw(cardBkgTexture[0], CharacterPosition, Color.White);
                        batch.Draw(cardBkgTexture[1], CharacterPosition, Color.White);

                        batch.Draw(avatarTexture[cplayer.character], CharacterPosition, Color.White);

                        batch.Draw(cardBkgTexture[2], CharacterPosition, Color.White);

                        if (cplayer.isReady == true)
                        {
                            batch.Draw(cardBkgTexture[3], CharacterPosition, Color.White);
                            batch.DrawString(DigitLowFont, Strings.ReadySelPlayerString,
                                new Vector2(CharacterPosition.X + cardBkgTexture[0].Width / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ReadySelPlayerString).X) / 2, CharacterPosition.Y + cardBkgTexture[0].Height / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ReadySelPlayerString).Y)),
                                Color.Black);
                        }

                        batch.Draw(avatarPixelatedTexture[cplayer.character], new Vector2(CharacterPosition.X + (avatarTexture[cplayer.character].Width / 2) - avatarPixelatedTexture[cplayer.character].Width / 2,
                            CharacterPosition.Y + (avatarTexture[cplayer.character].Height - avatarPixelatedTexture[cplayer.character].Height)), null, Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

                        if (cplayer.Player.Name != null)
                        {
                            batch.DrawString(MenuInfoFont, cplayer.Player.Name, new Vector2(CharacterPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(cplayer.Player.Name).Y), CharacterPosition.Y + 2 + cardBkgTexture[1].Height),
                                    Color.White, 4.70f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
                        }

                        batch.DrawString(MenuInfoFont, avatarNameList[cplayer.character], new Vector2(CharacterPosition.X - 5 + cardBkgTexture[1].Width - Convert.ToInt32(MenuInfoFont.MeasureString(avatarNameList[cplayer.character]).X), CharacterPosition.Y + 3),
                                Color.Black);

                        // Character NOT AVAILABLE
                        /*
                        if (cplayer.character != 0)
                        {
                            batch.DrawString(MenuInfoFont, "NOT", new Vector2(CharacterPosition.X + (pAvatar[cplayer.character].Width / 2) - pAvatarPixelated[cplayer.character].Width / 2,
                                CharacterPosition.Y + (pAvatar[cplayer.character].Height - pAvatarPixelated[cplayer.character].Height)), Color.Black);
                            batch.DrawString(MenuInfoFont, "AVAILABLE", new Vector2(CharacterPosition.X - 30 + (pAvatar[cplayer.character].Width / 2) - pAvatarPixelated[cplayer.character].Width / 2,
                                CharacterPosition.Y + 20 + (pAvatar[cplayer.character].Height - pAvatarPixelated[cplayer.character].Height)), Color.Black);
                        }*/

                        if (cplayer.isReady == false)
                        {
                            batch.Draw(arrow, new Vector2(CharacterPosition.X - arrow.Width, CharacterPosition.Y + cardBkgTexture[1].Height / 2 - arrow.Height / 2),
                                null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 1.0f);
                            batch.Draw(arrow, new Vector2(CharacterPosition.X + 8 + cardBkgTexture[1].Width, CharacterPosition.Y + cardBkgTexture[1].Height / 2 - arrow.Height / 2),
                                null, Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.FlipHorizontally, 1.0f);
                        }
                    }
                    else
                    {
                        if (GamePad.GetState(cplayer.Player.Controller).IsConnected && cplayer.Player.Name != null)
                        {
                            batch.Draw(cardBkgTexture[0], CharacterPosition, Color.White);
                            batch.Draw(cardBkgTexture[1], CharacterPosition, Color.White);

                            switch (index)
                            {
                                case 0:
                                    lines = Regex.Split(Strings.P1PressStartString, " ");
                                    break;
                                case 1:
                                    lines = Regex.Split(Strings.P2PressStartString, " ");
                                    break;
                                case 2:
                                    lines = Regex.Split(Strings.P3PressStartString, " ");
                                    break;
                                case 3:
                                    lines = Regex.Split(Strings.P4PressStartString, " ");
                                    break;
                                default:
                                    lines = Regex.Split(Strings.P1PressStartString, " ");
                                    break;
                            }

                            foreach (string line in lines)
                            {
                                batch.DrawString(MenuInfoFont, line.Replace("	", ""),
                                    new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkgTexture[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
                                    Color.Black);
                                ConnectControlerStringPosition.Y += 20;
                            }
                            // Connect Controller to play
                            //batch.DrawString(MenuInfoFont, Strings.ConnectControllerToJoinString,
                            //    new Vector2(CharacterPosition.X - 5 + cardBkg[1].Width - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ConnectControllerToJoinString).X), CharacterPosition.Y + cardBkg[1].Height / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ConnectControllerToJoinString).Y)),
                            //    Color.Black);
                        }
                        else
                        {
                            lines = Regex.Split(Strings.ConnectControllerToJoinString, " ");
                            foreach (string line in lines)
                            {
                                batch.DrawString(MenuInfoFont, line.Replace("	", ""),
                                    new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkgTexture[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
                                    Color.White);
                                ConnectControlerStringPosition.Y += 20;
                            }
                        }
                    }
                }
                else
                {
                    // Background Images
                    //batch.Draw(cardBkg[0], CharacterPosition, Color.White);
                    //batch.Draw(cardBkg[1], CharacterPosition, Color.White);

                    //Texto de contexto de menu
                    lines = Regex.Split(Strings.ConnectControllerToJoinString, " ");
                    foreach (string line in lines)
                    {
                        batch.DrawString(MenuInfoFont, line.Replace("	", ""),
                            new Vector2(ConnectControlerStringPosition.X - Convert.ToInt32(MenuInfoFont.MeasureString(line).X) / 2 - cardBkgTexture[0].Width / 2, ConnectControlerStringPosition.Y - Convert.ToInt32(MenuInfoFont.MeasureString(line).Y) - 25),
                            Color.White);
                        ConnectControlerStringPosition.Y += 20;
                    }
                    // Connect Controller to play
                    //batch.DrawString(MenuInfoFont, Strings.ConnectControllerToJoinString,
                    //    new Vector2(CharacterPosition.X - 5 + cardBkg[1].Width - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ConnectControllerToJoinString).X), CharacterPosition.Y + cardBkg[1].Height / 2 - Convert.ToInt32(MenuInfoFont.MeasureString(Strings.ConnectControllerToJoinString).Y)),
                    //    Color.Black);
                }

                CharacterPosition.X += 250;
                ConnectControlerStringPosition.X += 250;
                ConnectControlerStringPosition.Y = CharacterPosition.Y + cardBkgTexture[1].Height / 2;
            }

            batch.End();
        }

        private void DrawLevelSelectionMenu(SpriteBatch batch)
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            batch.Draw(myGroupBKG, new Vector2(uiBounds.X + uiBounds.Width - 260, 50), Color.White);
            batch.DrawString(MenuInfoFont, Strings.LevelSelectionString, new Vector2(uiBounds.X + uiBounds.Width - MenuInfoFont.MeasureString(Strings.LevelSelectionString).X / 2 - 100, 56), Color.White);
            batch.DrawString(DigitBigFont, String.Format("{0:00}", levelSelected), new Vector2(uiBounds.X + uiBounds.Width - 135, 90), Color.Salmon);
            batch.DrawString(DigitLowFont, "-", new Vector2(uiBounds.X + uiBounds.Width - 210, 80), Color.White);

            if (game.player1.inputMode == InputMode.Keyboard)
            {   
                batch.Draw(kbDown, new Vector2(uiBounds.X + uiBounds.Width - 217, 115), new Rectangle(0, 0, kbDown.Width, kbDown.Height), Color.White, 0.0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.0f);
            }
            else
            {
                batch.Draw(btnLT, new Vector2(uiBounds.X + uiBounds.Width - 217, 115), new Rectangle(0, 0, btnLT.Width, btnLT.Height), Color.White, 0.0f, Vector2.Zero, 0.28f, SpriteEffects.None, 0.0f);
            }

            batch.DrawString(DigitLowFont, "+", new Vector2(uiBounds.X + uiBounds.Width - 5, 80), Color.White);
            
            if (game.player1.inputMode == InputMode.Keyboard)
            {
                batch.Draw(kbUp, new Vector2(uiBounds.X + uiBounds.Width - 10, 115), new Rectangle(0, 0, kbUp.Width, kbUp.Height), Color.White, 0.0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.0f);
            }
            else
            {
                batch.Draw(btnRT, new Vector2(uiBounds.X + uiBounds.Width - 10, 115), new Rectangle(0, 0, btnRT.Width, btnRT.Height), Color.White, 0.0f, Vector2.Zero, 0.28f, SpriteEffects.None, 0.0f);
            }

            batch.End();
        }


        private void DrawContextMenu(MenuComponent menu, Vector2 position, SpriteBatch batch)
        {
            Vector2 MenuTitlePosition = new Vector2(position.X - LINE_X_OFFSET, position.Y - MENU_TITLE_Y_OFFSET);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            batch.Draw(logoMenu, new Vector2(MenuTitlePosition.X - 55, MenuTitlePosition.Y - 5), Color.White);
            batch.DrawString(MenuHeaderFont, Strings.CharacterSelectString, MenuTitlePosition, Color.White);

            if (this.isMatchmaking == true)
            {
                batch.DrawString(MenuHeaderFont, Strings.MatchmakingMenuString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                    new Color(255, 255, 255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);
            }
            else
            {
                batch.DrawString(MenuHeaderFont, Strings.LocalGameString, new Vector2(MenuTitlePosition.X - 10, MenuTitlePosition.Y + 70),
                    new Color(255, 255, 255, 40), 1.58f, Vector2.Zero, 0.8f, SpriteEffects.None, 1.0f);
            }

            batch.Draw(lineaMenu, new Vector2(position.X - LINE_X_OFFSET, position.Y - 15), Color.White);

            menu.DrawSelectPlayerButtons(batch, new Vector2(position.X - 30, position.Y - 5), MenuInfoFont, canStartGame, true);

            batch.End();
        }

        public void DrawHowToPlay(SpriteBatch batch, Vector2 position)
        {
            Vector2 contextMenuPosition = new Vector2(position.X + 260, position.Y - 105);
            Vector2 MenuTitlePosition = new Vector2(contextMenuPosition.X - 3, contextMenuPosition.Y - 300);

            string[] lines;

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            // Lobby Header Image
            batch.Draw(HTPHeaderImage, new Vector2(MenuTitlePosition.X, MenuTitlePosition.Y + 75), Color.White);

            //Linea divisoria
            position.X -= 40;
            position.Y -= 115;
            batch.Draw(lineaMenu, new Vector2(position.X + 300, position.Y - 40), Color.White);

            //Texto de contexto del How to Play
            contextMenuPosition = new Vector2(position.X + 300, position.Y - 30);
            if (game.player1.inputMode == InputMode.Keyboard)
            {
                lines = Regex.Split(Strings.PCExplanationString, "\r\n");
            }
            else
            {
                lines = Regex.Split(Strings.HTPExplanationString, "\r\n");
            }
            foreach (string line in lines)
            {
                batch.DrawString(MenuInfoFont, line.Replace("	", ""), contextMenuPosition, Color.White);
                contextMenuPosition.Y += 20;
            }

            batch.End();
        }
    }
}