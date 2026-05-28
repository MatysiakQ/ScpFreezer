using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;

namespace ScpFreezer
{
    public class ScpFreezerPlugin : Plugin<Config>
    {
        public override string Name => "ScpStartFreezer";
        public override string Author => "JastrzabDev";
        public override Version Version => new Version(2, 0, 0);
        public override string Prefix => "scp_freezer";

        private Dictionary<Player, DateTime> _freezeEnds = new Dictionary<Player, DateTime>();

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            base.OnDisabled();
        }

        private void OnRoundStarted()
        {
            // Czyścimy pamięć
            _freezeEnds.Clear();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            // Sprawdzamy z configu czy postac ma dostac freeze
            if (Config.ScpFreezeSettings.TryGetValue(ev.Player.Role.Type, out bool shouldFreeze) && shouldFreeze)
            {
                float timeToFreeze = Config.FreezeDuration;

                if (_freezeEnds.TryGetValue(ev.Player, out DateTime endTime))
                {
                    timeToFreeze = (float)(endTime - DateTime.Now).TotalSeconds;
                }
                else
                {
                    _freezeEnds[ev.Player] = DateTime.Now.AddSeconds(timeToFreeze);
                }

                if (timeToFreeze > 0)
                {
                    Timing.CallDelayed(0.1f, () =>
                    {
                        if (ev.Player.IsConnected)
                        {
                            ev.Player.EnableEffect(EffectType.Ensnared, timeToFreeze);

                            if (!string.IsNullOrEmpty(Config.FreezeMessage))
                            {
                                int displayTime = (int)Math.Ceiling(timeToFreeze);
                                string msg = Config.FreezeMessage.Replace("%time%", displayTime.ToString());

                                ev.Player.Broadcast((ushort)displayTime, msg);
                            }
                        }
                    });
                }
            }
        }
    }
}
