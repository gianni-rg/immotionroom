namespace ImmotionAR.ImmotionRoom.AutoDiscovery.Model
{
    using System;
    using System.Collections.Generic;

    public class DataSourceDiscoveryResult
    {
        public Dictionary<string, DataSourceItem> DataSources { get; private set; }

        public DataSourceDiscoveryResult()
        {
            DataSources = new Dictionary<string, DataSourceItem>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
