using System.Collections.Generic;

namespace TGC.Group.Model.Entidades
{
    static class Entities
    {
        private static List<Entity> entities = new List<Entity>();

        public static void Add(Entity entity) { entities.Add(entity); }
        public static void Remove(Entity entity) { entities.Remove(entity); }
        public static List<Entity> GetEntities() { return entities; }
    }
}
