namespace ImmotionAR.ImmotionRoom.Helpers
{
    using System;

    public static class Requires
    {
        public static void NotNull(object o, string propertyName)
        {
            if (o == null)
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}