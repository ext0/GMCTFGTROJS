using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTF.GameLogic
{
    public static class UserStore
    {
        private static List<Player> players = new List<Player>();
        private static Object playerLock = new object();
        private static Object playerSearchLock = new object();
        private static bool allowTeamKilling = true;
        public static void addPlayer(Player player) //welcome!
        {
            lock (playerLock)
            {
                if (player == null || playerExists(player.name))
                {
                    return; //1 job
                }
                players.Add(player);
                foreach (CTFWebSocketService service in activeSessions)
                {
                    service.NotifyNewPlayer(player);
                }
            }
        }
        public static void updatePosition(Player player)
        {
            lock (playerLock)
            {
                if (player == null)
                {
                    return;
                }
                foreach (CTFWebSocketService service in activeSessions)
                {
                    service.UpdatePlayerPosition(player);
                }
            }
        }
        public static void updateProjectile(Vector3 origin, Vector3 target)
        {
            lock (playerLock)
            {
                foreach (CTFWebSocketService service in activeSessions)
                {
                    service.UpdateProjectile(origin, target);
                }
            }
        }
        public static void updateDamage(Player sender, String target, int damage)
        {
            lock (playerLock)
            {
                Player player = players.Where((x) =>
                {
                    return x.name.Equals(target);
                }).FirstOrDefault();
                if (player == null || (!allowTeamKilling && sender.team.Equals(player.team)))
                {
                    //come on now...
                    return;
                }
                Console.WriteLine("HIT REGISTER!");
                CTFWebSocketService service = activeSessions.Where((x) =>
                {
                    return x.me.Equals(player);
                }).FirstOrDefault();
                if (service == null) //what the fuck?
                {
                    return;
                }
                service.UpdateDamage(damage);
            }
        }
        public static bool playerExists(String username)
        {
            lock (playerSearchLock)
            {
                return players.Where((x) =>
                {
                    return x.name.Equals(username);
                }).Count() > 0;
            }
        }
        public static ReadOnlyCollection<Player> getAllPlayers()
        {
            return players.AsReadOnly();
        }
        public static List<CTFWebSocketService> activeSessions = new List<CTFWebSocketService>();
    }
}
