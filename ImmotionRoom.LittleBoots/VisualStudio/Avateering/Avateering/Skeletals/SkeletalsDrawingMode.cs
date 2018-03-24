namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Skeletals
{
    /// <summary>
    /// Different modes of visualization of a group of skeletal avatars
    /// </summary>
    public enum SkeletalsDrawingMode
    {
        /// <summary>
        /// Draw all skeletons with green positive color and red negative color
        /// </summary>
        Standard,

        /// <summary>
        /// Draw all skeletons with user defined positive color and user defined negative color
        /// </summary>
        FixedColors,

        /// <summary>
        /// Draw each skeleton with a random extracted set of predefined positive-negative colors pair
        /// </summary>
        RandomPresetsColor,

        /// <summary>
        /// Draw each skeleton with a random extracted set of positive and negative colors pair
        /// </summary>
        RandomColor
    }
}
