namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tools
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Contains utilities for c# language miscellaneus problems
    /// </summary>
    internal static class LanguageUtilities
    {
        #region Collections Utilities

        /// <summary>
        ///     Checks if two containers are equals, independently from items orders (e.g. if two lists contains same elements)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            //code from http://stackoverflow.com/questions/3669970/compare-two-listt-objects-for-equality-ignoring-order
            var deletedItems = list1.Except(list2).ToList().Any();
            var newItems = list2.Except(list1).ToList().Any();
            return !newItems && !deletedItems;
        }

        #endregion
    }
}
