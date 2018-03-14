namespace ImmotionAR.ImmotionRoom.Helpers
{
    using System;
    using Interfaces;
    using Microsoft.Win32;

    public class HelpersRegistry : IHelpersRegistry
    {
        // See: http://www.codeproject.com/Articles/3389/Read-write-and-delete-from-registry-with-C

        public string ReadSetting(string subKey, string keyName)
        {
            // Opening the registry key
            var rk = Registry.CurrentUser;

            // Open a subKey as read-only
            var sk1 = rk.OpenSubKey(subKey);

            if (sk1 == null)
            {
                return null;
            }

            try
            {
                return (string) sk1.GetValue(keyName);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (sk1 != null)
                {
                    sk1.Close();
                }
            }
        }

        public bool WriteSetting(string subKey, string keyName, string value)
        {
            try
            {
                // Setting
                var rk = Registry.CurrentUser;

                // I have to use CreateSubKey (create or open it if already exits), because OpenSubKey open a subKey as read-only
                var sk1 = rk.CreateSubKey(subKey);

                // Save the value
                sk1.SetValue(keyName, value);

                sk1.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}