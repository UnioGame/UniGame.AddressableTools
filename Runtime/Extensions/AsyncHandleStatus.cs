﻿using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UniModules.UniGame.SerializableContext.Runtime.Addressables
{
    public struct HandleStatus
    {
        public AsyncOperationStatus Status;
        public long TotalBytes;
        public long DownloadedBytes;
        public bool IsDone;
        public Exception OperationException;
        
        public float Percent => (TotalBytes > 0) ? ((float)DownloadedBytes / (float)TotalBytes) : (IsDone ? 1.0f : 0f);
    }

}