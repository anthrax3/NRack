﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base.Config
{
    public interface IServerConfig
    {
        string Name { get; }

        string Group { get; }

        string Type { get; }
    }
}