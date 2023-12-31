﻿using System;

namespace Netryoshka.Services
{
    public interface ILogger
    {
        void Info(string message);
        void Error(string message);
        void Error(string message, Exception e);
        void Warn(string message);
    }
}
