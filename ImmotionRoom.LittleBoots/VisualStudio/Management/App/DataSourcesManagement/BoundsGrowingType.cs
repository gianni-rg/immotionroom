namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Enumerates different possible growing edges for a Bounding Boxes
    /// </summary>
    public enum BoundsGrowingType
    {
        None,
        LeftLimit,
        FrontLimit,
        RightLimit,
        BackLimit
    }

}