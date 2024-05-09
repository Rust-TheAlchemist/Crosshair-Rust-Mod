using System.Collections.Generic;
using System.Globalization;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Crosshair", "MisterPixie & TheAlchemist", "3.1.1")]
    [Description("Allows the user to toggle a crosshair. Automatically comes on when connecting or spawning.")]

    class Crosshair : RustPlugin
    {
        private string mainUI = "UI_MAIN";
        private CuiElementContainer _ui;
        private HashSet<string> _crosshairSettings = new HashSet<string>();
        private const string _usePerm = "crosshair.use";
		
		#region Debug
		private bool DebugEnabled = false;

        private void PrintDebug(string message, bool warning = false, bool error = false)
        {
            if (DebugEnabled)
            {
                if (error)
                {
                    PrintError(message);
                }
                else if (warning)
                {
                    PrintWarning(message);
                }
                else
                {
                    Puts(message);
                }
            }
        }
        #endregion Debug
		
        #region Classes
        private class UI
        {
            public static CuiElementContainer CreateElementContainer(string panelName, string color, string aMin, string aMax, bool cursor = false)
            {
                var newElement = new CuiElementContainer
                {
                    {
                        new CuiPanel
                        {
                            Image = {Color = color},
                            RectTransform = {AnchorMin = aMin, AnchorMax = aMax},
                            CursorEnabled = cursor
                        },
                        new CuiElement().Parent = "Hud",
                        panelName
                    }
                };

                return newElement;
            }

            public static void CreateLabel(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleCenter, float fadein = 1f)
            {
                container.Add(new CuiLabel
                {
                    Text = { Color = color, FontSize = size, Align = align, FadeIn = fadein, Text = text },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax }
                }, panel, CuiHelper.GetGuid());
            }
        }
        #endregion

        #region Methods
        private void ToggleCrosshair(BasePlayer player, string command, string[] args)
        {
			PrintDebug("Method:: ToggleCrosshair");

            if (!permission.UserHasPermission(player.UserIDString, _usePerm))
            {
                PrintToChat(player, Lang("No Permission", player.UserIDString));
                return;
            }

            if (_crosshairSettings.Contains(player.UserIDString))
            {
				PrintDebug("Method:: ToggleCrosshair >> _crosshairSettings: " + _crosshairSettings);
                DestroyCrosshair(player);
                _crosshairSettings.Remove(player.UserIDString);
                player.ChatMessage(Lang("CrosshairOff", player.UserIDString));
                return;
            }

            CuiHelper.AddUi(player, _ui);
            _crosshairSettings.Add(player.UserIDString);
            player.ChatMessage(Lang("CrosshairOn", player.UserIDString));

        }

        private void DestroyCrosshair(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, mainUI);
        }

        private void DestroyAllCommand(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null) return;

            DestroyCrosshair(player);
        }

        private void CreateUI()
        {
			PrintDebug("Method:: CreateUI");

			// TheAlchemist - modified to true center, at least on my screen @ 2560 x 1440 Fullscreen -- TO-DO: move this to server config and/or per-player configurable.
            var container = UI.CreateElementContainer(mainUI, $"{HexToColor("000000")} 0", "0.4692 0.4672", "0.5292 0.5272");
            UI.CreateLabel(ref container, mainUI, $"{HexToColor(configData.CrosshairColor)} 0.9", configData.CrosshairText, 25, "0 0", "1 1");
            _ui = container;
        }
        private static string HexToColor(string hexColor)
        {
            if (hexColor.IndexOf('#') != -1) hexColor = hexColor.Replace("#", "");

            var red = 0;
            var green = 0;
            var blue = 0;

            if (hexColor.Length == 6)
            {
                red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hexColor.Length == 3)
            {
                red = int.Parse(hexColor[0] + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor[1] + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor[2] + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
            }

            return $"{(double)red / 255} {(double)green / 255} {(double)blue / 255}";
        }
		
		
		// TheAlchemist
        private void MaybeCreateButtonUi(BasePlayer player)
        {
			// Temporary patch to always enable during connection. Will add per player data file save in the future.
			bool ShouldDisplayGuiButton = true;
			
			PrintDebug("Method:: MaybeCreateButtonUi");

            if (player == null || player.IsNpc || !player.IsAlive() || player.IsSleeping())
                return;

            if (!permission.UserHasPermission(player.UserIDString, _usePerm))
                return;

			if (!ShouldDisplayGuiButton)
                return;

            if (_ui == null)
            {
                CreateUI();
            }

            CuiHelper.AddUi(player, _ui);
            _crosshairSettings.Add(player.UserIDString);
            player.ChatMessage(Lang("CrosshairOn", player.UserIDString));
        }		
			
        #endregion

        #region Hooks
        private void Init()
        {
            LoadVariables();
            permission.RegisterPermission("crosshair.use", this);
            cmd.AddChatCommand("crosshair", this, "ToggleCrosshair");
            cmd.AddConsoleCommand("ui_destroy", this, "DestroyAllCommand");
            CreateUI();
			
			// TheAlchemist - so that if the plugin gets reloaded/loaded it will enable for players with permission.
            foreach (var player in BasePlayer.activePlayerList)
            {
				MaybeCreateButtonUi(player);
            }
        }

        private void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyCrosshair(player);
				_crosshairSettings.Remove(player.UserIDString);
            }
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            DestroyCrosshair(player);
            _crosshairSettings.Remove(player.UserIDString);
        }
		
		// TheAlchemist - Start added Hooks
		private void OnPlayerConnected(BasePlayer player) => MaybeCreateButtonUi(player);
		private void OnPlayerRespawned(BasePlayer player) => MaybeCreateButtonUi(player);
        private void OnPlayerSleepEnded(BasePlayer player) => OnPlayerRespawned(player);
        private void OnPlayerSleep(BasePlayer player) 
		{ 
			DestroyCrosshair(player);
			_crosshairSettings.Remove(player.UserIDString);
		}

        private void OnEntityDeath(BasePlayer player, HitInfo info) => OnEntityKill(player);

        private void OnEntityKill(BasePlayer player)
        {
            if (player.IsNpc)
                return;

			PrintDebug("Hook:: OnEntityKill - Is Player");

			DestroyCrosshair(player);
			_crosshairSettings.Remove(player.UserIDString);
        }		
		// TheAlchemist - End added Hooks
		
        #endregion

        #region lang
        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"CrosshairOn", "Crosshair turned on."},
                {"CrosshairOff", "Crosshair turned off."},
                {"No Permission", "Error, you lack permission."}
            }, this);
        }
        #endregion

        #region Config
        private ConfigData configData;
        private class ConfigData
        {
            public string CrosshairColor;
            public string CrosshairText;
        }

        private void LoadVariables()
        {
            LoadConfigVariables();
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            var config = new ConfigData
            {
                CrosshairColor = "#008000",
                CrosshairText = "◎"
            };
            SaveConfig(config);
        }
        private void LoadConfigVariables() => configData = Config.ReadObject<ConfigData>();
        void SaveConfig(ConfigData config) => Config.WriteObject(config, true);
        #endregion
    }
}