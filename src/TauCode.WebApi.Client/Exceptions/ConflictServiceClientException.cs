﻿using System;

namespace TauCode.WebApi.Client.Exceptions
{
    [Serializable]
    public class ConflictServiceClientException : ServiceClientException
    {
        public ConflictServiceClientException()
        {
        }

        public ConflictServiceClientException(string message)
            : base(message)
        {
        }

        // todo
        //public ConflictServiceClientException(string code, string message)
        //    : base(code, message)
        //{
        //}
    }
}