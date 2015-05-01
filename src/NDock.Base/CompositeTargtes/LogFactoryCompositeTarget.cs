﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyLog;
using NDock.Base.Config;

namespace NDock.Base.CompositeTargets
{
    class LogFactoryCompositeTarget : SingleResultCompositeTargetCore<ILogFactory, ILogFactoryMetadata>
    {
        public LogFactoryCompositeTarget(Action<ILogFactory> callback)
            : base((config) => config.LogFactory, callback, true)
        {

        }

        protected override bool MetadataNameEqual(ILogFactoryMetadata metadata, string name)
        {
            return metadata.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        protected override bool PrepareResult(ILogFactory result, IAppServer appServer, ILogFactoryMetadata metadata)
        {
            var currentAppDomain = AppDomain.CurrentDomain;
            var isolation = IsolationMode.None;

            var isolationValue = currentAppDomain.GetData(typeof(IsolationMode).Name);

            if (isolationValue != null)
                isolation = (IsolationMode)isolationValue;

            var configFileName = metadata.ConfigFileName;

            if (Path.DirectorySeparatorChar != '\\')
            {
                configFileName = Path.GetFileNameWithoutExtension(configFileName) + ".unix" + Path.GetExtension(configFileName);
            }

            var configFiles = new List<string>();

            if (isolation == IsolationMode.None)
            {
                configFiles.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName));
                configFiles.Add(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), configFileName));
            }
            else //The running AppServer is in isolated appdomain
            {
                configFiles.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName));
                configFiles.Add(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), configFileName));

                //go to the application's root
                //the appdomain's root is /WorkingDir/DomainName, so get parent path twice to reach the application root
                var rootDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;

                configFiles.Add(Path.Combine(rootDir, AppDomain.CurrentDomain.FriendlyName + "." + configFileName));
                configFiles.Add(Path.Combine(Path.Combine(rootDir, "Config"), AppDomain.CurrentDomain.FriendlyName + "." + configFileName));
                configFiles.Add(Path.Combine(rootDir, configFileName));
                configFiles.Add(Path.Combine(Path.Combine(rootDir, "Config"), configFileName));
            }

            if (!result.Initialize(configFiles.ToArray()))
            {
                appServer.Logger.Error("Failed to initialize the logfactory:" + metadata.Name);
                return false;
            }

            return true;
        }
    }
}
