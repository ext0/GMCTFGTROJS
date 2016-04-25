using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTF.GameLogic
{
    public static class UserStore
    {
        private static List<Player> players = new List<Player>();
        private static Object playerLock = new object();
        private static Object playerSearchLock = new object();
        private static Object updateLock = new object();
        private static bool allowTeamKilling = true;
        private static int disconnectTime = 1000;
        private static Dictionary<CTFWebSocketService, ServiceInstanceStorage> latestUpdate = new Dictionary<CTFWebSocketService, ServiceInstanceStorage>();
        public static void update(CTFWebSocketService source)
        {
            lock (updateLock)
            {
                if (!latestUpdate.ContainsKey(source))
                {
                    latestUpdate.Add(source, new ServiceInstanceStorage(source));
                }
                else
                {
                    latestUpdate[source].update();
                }
            }
        }
        public static bool isDisconnected(CTFWebSocketService source, int dcTimeMs)
        {
            lock (updateLock)
            {
                if (latestUpdate.ContainsKey(source))
                {
                    return (latestUpdate[source].lastUpdate.AddMilliseconds(dcTimeMs)) < DateTime.Now;
                }
                else
                {
                    return true;
                }
            }
        }
        public static void addPlayer(CTFWebSocketService source, Player player) //welcome!
        {
            lock (playerLock)
            {
                if (player == null || playerExists(player.name))
                {
                    return; //1 job
                }
                players.Add(player);
                update(source);
                ThreadPool.QueueUserWorkItem((s)=>
                {
                    while (!isDisconnected(s as CTFWebSocketService, disconnectTime))
                    {
                        Thread.Sleep(disconnectTime);
                    }
                    UserStore.playerDisconnect(s as CTFWebSocketService);
                }, source);
                foreach (CTFWebSocketService service in activeSessions)
                {
                    service.NotifyNewPlayer(player);
                }
            }
        }
        public static void updatePosition(CTFWebSocketService source)
        {
            lock (playerLock)
            {
                if (source == null || source.me == null)
                {
                    return;
                }
                update(source);
                foreach (CTFWebSocketService service in activeSessions)
                {
                    if (service.Equals(source))
                    {
                        continue;
                    }
                    service.UpdatePlayerPosition(source.me);
                }
            }
        }
        public static void updateProjectile(CTFWebSocketService source, Vector3 origin, Vector3 target)
        {
            lock (playerLock)
            {
                if (source == null || source.me == null)
                {
                    return;
                }
                update(source);
                foreach (CTFWebSocketService service in activeSessions)
                {
                    if (service.Equals(source))
                    {
                        continue;
                    }
                    service.UpdateProjectile(source.me, origin, target);
                }
            }
        }
        public static void updateDied(CTFWebSocketService source)
        {
            lock (playerLock)
            {
                if (source == null || source.me == null)
                {
                    return;
                }
                update(source);
                Player killer = latestUpdate[source].getMostRecentKiller();
                foreach (CTFWebSocketService service in activeSessions)
                {
                    if (service.Equals(source))
                    {
                        service.me.deaths++;
                        continue;
                    }
                    service.NotifyDied(killer, source.me);
                }
            }
        }
        public static void updateDamage(CTFWebSocketService source, String origin, int damage)
        {
            lock (playerLock)
            {
                update(source);
                Player player = players.Where((x) =>
                {
                    return x.name.Equals(origin);
                }).FirstOrDefault();
                if (player == null)
                {
                    return;
                }
                player.kills++;
                latestUpdate[source].addEvent(player, damage);
            }
        }
        public static void playerDisconnect(CTFWebSocketService source)
        {
            lock (playerLock)
            {
                foreach (CTFWebSocketService service in activeSessions)
                {
                    if (service.Equals(source))
                    {
                        continue;
                    }
                    service.NotifyDisconnect(source.me);
                }
                players.Remove(source.me); //cya
                activeSessions.Remove(source);
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
