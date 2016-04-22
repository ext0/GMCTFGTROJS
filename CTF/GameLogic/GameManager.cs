using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace CTF.GameLogic
{
    public class CTFWebSocketService : WebSocketBehavior
    {
        public Player me { get; set; }
        protected override void OnOpen()
        {
            if (!UserStore.activeSessions.Contains(this))
            {
                UserStore.activeSessions.Add(this);
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            if (UserStore.activeSessions.Contains(this))
            {
                UserStore.activeSessions.Remove(this);
            }
        }
        public void NotifyNewPlayer(Player player)
        {
            Send(new RequestMessage("PNEWREQ", player, true).serialize());
        }
        public void UpdatePlayerPosition(Player player)
        {
            Send(new RequestMessage("POMOVE", player, true).serialize());
        }
        public void UpdateDamage(int damage)
        {
            Send(new RequestMessage("PDAMAGE", damage, true).serialize());
        }
        public void UpdateProjectile(Vector3 origin, Vector3 target)
        {
            Send(new RequestMessage("PSHOT", new PlayerShot(origin, target), true).serialize());
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data.StartsWith("PREQ"))
            {
                String[] username = e.Data.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (username.Length != 2)
                {
                    Send(new RequestMessage("PREQ", null, false).serialize());
                    return;
                }
                if (!UserStore.playerExists(username[1]))
                {
                    Player player = new Player
                    {
                        deaths = 0,
                        kills = 0,
                        name = username[1],
                        team = Team.Red,
                        position = new Position(
                            new Vector3(0, 0, 0),
                            new Vector3(0, 0, 0),
                            new Vector2(0, 0),
                            new Vector2(0, 0),
                            new Vector2(0, 0),
                            new Vector2(0, 0),
                            0)
                    };

                    this.me = player;
                    UserStore.addPlayer(player);
                    Send(new RequestMessage("PREQ", player, true).serialize());
                    return;
                }
                else
                {
                    Send(new RequestMessage("PREQ", null, false).serialize());
                    return;
                }
            }
            else if (e.Data.StartsWith("PFETCH"))
            {
                Send(new RequestMessage("PFETCH", UserStore.getAllPlayers(), true).serialize());
                return;
            }
            else if (e.Data.StartsWith("PMOVE"))
            {
                String[] coords = e.Data.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (coords.Length != 16 || me == null)
                {
                    Send(new RequestMessage("PMOVE", null, false).serialize());
                    return;
                }
                double px = double.Parse(coords[1]);
                double py = double.Parse(coords[2]);
                double pz = double.Parse(coords[3]);
                double boxminx = double.Parse(coords[4]);
                double boxminy = double.Parse(coords[5]);
                double boxmaxx = double.Parse(coords[6]);
                double boxmaxy = double.Parse(coords[7]);
                double gridminx = double.Parse(coords[8]);
                double gridminy = double.Parse(coords[9]);
                double gridmaxx = double.Parse(coords[10]);
                double gridmaxy = double.Parse(coords[11]);
                double xangle = double.Parse(coords[12]);
                double directionx = double.Parse(coords[13]);
                double directiony = double.Parse(coords[14]);
                double directionz = double.Parse(coords[15]);
                me.position = new Position(
                                    new Vector3(px, py, pz),
                                    new Vector3(0, 0, 0),
                                    new Vector2(boxminx, boxminy),
                                    new Vector2(boxmaxx, boxmaxy),
                                    new Vector2(gridminx, gridminy),
                                    new Vector2(gridmaxx, gridmaxy),
                                    xangle);
                UserStore.updatePosition(me);
            }
            else if (e.Data.StartsWith("PHIT"))
            {
                String[] data = e.Data.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (data.Length != 3)
                {
                    Send(new RequestMessage("PHIT", null, false).serialize());
                    return;
                }
                String target = data[1];
                int damage = int.Parse(data[2]);
                UserStore.updateDamage(this.me, target, damage);
            }
            else if (e.Data.StartsWith("PSHOT"))
            {
                String[] data = e.Data.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (data.Length != 4)
                {
                    Send(new RequestMessage("PHIT", null, false).serialize());
                    return;
                }
                double px = double.Parse(data[1]);
                double py = double.Parse(data[2]);
                double pz = double.Parse(data[3]);
            }
        }
    }
}
