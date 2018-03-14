﻿namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;
    using System.Collections.Generic;

    public class Command
    {
        public IDictionary<string, object> Data { get; private set; }

        public string RequestId { get; set; }
        public DateTime Timestamp { get; set; }

        public CommandType CommandType { get; set; }

        public Command()
        {
            Data = new Dictionary<string, object>();
        }
    }
}
