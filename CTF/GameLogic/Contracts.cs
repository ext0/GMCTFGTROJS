using SaneWeb.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTF.GameLogic
{
    public static class Constants
    {
        public readonly static Vector3 PlayerSize = new Vector3(3, 7, 3);
    }

    public enum Team
    {
        Red,
        Blue
    }
    public class PlayerShot
    {
        public Player player { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 target { get; set; }
        public PlayerShot(Player player, Vector3 origin, Vector3 target)
        {
            this.player = player;
            this.origin = origin;
            this.target = target;
        }
    }
    public class DamageEvent
    {
        public Player victim { get; set; }
        public Player origin { get; set; }
        public int damage { get; set; }
        public DamageEvent(Player victim, Player origin, int damage)
        {
            this.victim = victim;
            this.origin = origin;
            this.damage = damage;
        }
    }
    public class KillEvent
    {
        public Player victim { get; set; }
        public Player killer { get; set; }
        public KillEvent(Player victim, Player killer)
        {
            this.victim = victim;
            this.killer = killer;
        }
    }
    public class ServiceInstanceStorage
    {
        public DateTime lastUpdate { get; set; }
        public CTFWebSocketService source { get; set; }
        public List<DamageEvent> damageEvents { get; set; }
        public ServiceInstanceStorage(CTFWebSocketService source)
        {
            this.source = source;
            this.lastUpdate = DateTime.Now;
            this.damageEvents = new List<DamageEvent>();
        }
        public void update()
        {
            lastUpdate = DateTime.Now;
        }
        public void addEvent(Player origin, int damage)
        {
            damageEvents.Add(new DamageEvent(source.me, origin, damage));
        }
        public Player getMostRecentKiller()
        {
            if (damageEvents.Count > 0)
            {
                return damageEvents[damageEvents.Count - 1].origin;
            }
            else
            {
                return null;
            }
        }
    }
    public class Player
    {
        public int kills { get; set; }
        public int deaths { get; set; }
        public String name { get; set; }
        public Position position { get; set; }
        public Team team { get; set; }
    }
    public class Position
    {
        public Vector3 position { get; set; }
        public GridLocation gridLocation { get; set; }
        public Vector3 direction { get; set; }
        public double xAngle { get; set; }
        public Position(Vector3 position, Vector3 direction, Vector2 boxMin, Vector2 boxMax, Vector2 gridMin, Vector2 gridMax, double xAngle)
        {
            this.position = position;
            this.direction = direction;
            this.xAngle = xAngle;
            this.gridLocation = new GridLocation(new RelativePairedCoord(boxMin, boxMax), new RelativePairedCoord(gridMin, gridMax));
        }
    }
    public class GridLocation
    {
        public RelativePairedCoord box { get; set; }
        public RelativePairedCoord grid { get; set; }
        public GridLocation(RelativePairedCoord box, RelativePairedCoord grid)
        {
            this.box = box;
            this.grid = grid;
        }
    }
    public class RelativePairedCoord
    {
        public Vector2 min { get; set; }
        public Vector2 max { get; set; }
        public RelativePairedCoord(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }
        public override string ToString()
        {
            return "{(" + min + "),(" + max + ")}";
        }
    }
    public class Vector2
    {
        public double x;
        public double y;
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString()
        {
            return x + "," + y;
        }
    }
    public class Vector3
    {
        public double x;
        public double y;
        public double z;
        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public override string ToString()
        {
            return x + "," + y + "," + z;
        }
    }
    public class RequestMessage
    {
        public String type { get; set; }
        public bool success { get; set; }
        public Object data { get; set; }
        public RequestMessage(String type, Object data, bool success)
        {
            this.type = type;
            this.data = data;
            this.success = success;
        }
        public String serialize()
        {
            return Utility.serializeObjectToJSON(this);
        }
    }
}
