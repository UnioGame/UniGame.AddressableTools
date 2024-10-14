namespace UniGame.AddressableTools.Runtime
{
    using System;
    using Object = UnityEngine.Object;

    [Serializable]
    public struct AddressableResourceResult 
    {
        public const string ResourceError = "Game Resource Not Found";
        
        public static AddressableResourceResult FailedResourceResult = new()
        {
            Complete = false,
            Error = ResourceError,
            Result = null
        };
        
        public Object Result;
        public bool Complete;
        public string Error;
        public Exception Exception;
    }
    
    [Serializable]
    public struct AddressableResourceResult<TAsset> 
    {
        public const string ResourceError = "Game Resource Not Found";
        
        public static readonly AddressableResourceResult<TAsset> FailedResourceResult = new()
        {
            Complete = false,
            Error = ResourceError,
            Result = default
        };
        
        public static readonly AddressableResourceResult<TAsset> CompleteResourceResult = new()
        {
            Complete = true,
            Error = string.Empty,
            Result = default,
            Exception = null,
        };
        
        public TAsset Result;
        public bool Complete;
        public string Error;
        public Exception Exception;
    }
}