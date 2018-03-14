// -----------------------------------------------------------------------
// <copyright file="CommandRequestStatus.cs" company="ImmotionAR">
// Copyright (C) 2015 ImmotionAR. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    public enum CommandRequestStatus
    {
        Undefined = 0,
        Enqueued = 1,
        Completed = 2,
        Error = 3,
    }
}
