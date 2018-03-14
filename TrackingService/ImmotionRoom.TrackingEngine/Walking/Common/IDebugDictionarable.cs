namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    /// <summary>
    ///     Interface for a class serializable in a dictionary for debug purposes
    /// </summary>
    public interface IDebugDictionarable<out TDictionaryType>
    {
        /// <summary>
        ///     Serialize object info into a dictionary, for debugging purposes
        /// </summary>
        /// <returns>Object serialization into a dictionary of dictionaries (infos are subdivided into groups)</returns>
        TDictionaryType DictionarizeInfo();
    }
}