using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.Core
{
    public class PauseService : IPauseService
    {
        private readonly HashSet<object> _tokens = new();
        public bool IsPaused => _tokens.Count > 0;

        public object Acquire(string reason)
        {
            var token = new PauseToken(reason);
            _tokens.Add(token);
            Apply();
            return token;
        }

        public void Release(object token)
        {
            if (token == null) return;
            _tokens.Remove(token);
            Apply();
        }

        private void Apply()
        {
            Time.timeScale = IsPaused ? 0f : 1f;
        }

        private class PauseToken
        {
            public readonly string Reason;
            public PauseToken(string reason) => Reason = reason;
        }
    }
}