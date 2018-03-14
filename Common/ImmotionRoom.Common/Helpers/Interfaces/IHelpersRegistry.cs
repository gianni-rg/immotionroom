namespace ImmotionAR.ImmotionRoom.Helpers.Interfaces
{
    public interface IHelpersRegistry
    {
        string ReadSetting(string subKey, string keyName);
        bool WriteSetting(string subKey, string keyName, string value);
    }
}