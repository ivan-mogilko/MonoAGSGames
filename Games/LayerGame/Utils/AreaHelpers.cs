using AGS.API;

namespace LayerGame
{
    public static class AreaHelpers
    {
        public static void AllowEntity(this IArea a, IEntity e)
        {
            var res = a.GetComponent<IAreaRestriction>();
            if (res == null)
                return;
            if (res.RestrictionType == RestrictionListType.BlackList)
                res.RestrictionList.Remove(e.ID);
            else
                res.RestrictionList.Add(e.ID);
        }

        public static void DisallowEntity(this IArea a, IEntity e)
        {
            var res = a.GetComponent<IAreaRestriction>();
            if (res == null)
                return;
            if (res.RestrictionType == RestrictionListType.BlackList)
                res.RestrictionList.Add(e.ID);
            else
                res.RestrictionList.Remove(e.ID);
        }
    }
}
