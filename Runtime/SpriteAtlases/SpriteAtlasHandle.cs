using System;
using Cysharp.Threading.Tasks;
using UniModules.UniCore.Runtime.DataFlow;
using UniModules.UniGame.AddressableTools.Runtime.Extensions;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UniModules.UniGame.Core.Runtime.Interfaces;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public class SpriteAtlasHandle
    {
        public string tag;
        public string guid;
        public SpriteAtlas spriteAtlas;
    }
}