namespace UniGame.AddressableTools.Runtime
{
    using System;
    using Object = UnityEngine.Object;

    [Serializable]
    public struct AddressableResourceResult 
    {
        public const string ResourceError = "Game Resource Not Found";
        
        public static AddressableResourceResult FailedResourceResult = new AddressableResourceResult()
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
    public struct AddressableResourceResult<TAsset> where  TAsset : Object
    {
        public const string ResourceError = "Game Resource Not Found";
        
        public static readonly AddressableResourceResult<TAsset> FailedResourceResult = new AddressableResourceResult<TAsset>()
        {
            Complete = false,
            Error = ResourceError,
            Result = null
        };
        
        public static readonly AddressableResourceResult<TAsset> CompleteResourceResult = new AddressableResourceResult<TAsset>()
        {
            Complete = true,
            Error = string.Empty,
            Result = null,
            Exception = null,
        };
        
        public TAsset Result;
        public bool Complete;
        public string Error;
        public Exception Exception;
    }
}