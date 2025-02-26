﻿using System.Collections.Generic;
using System.Composition;
using System.Linq;
using CommandLine;
using Microsoft.ComponentDetection.Orchestrator.ArgumentSets;

namespace Microsoft.ComponentDetection.Orchestrator
{
    [Export(typeof(IArgumentHelper))]
    public class ArgumentHelper : IArgumentHelper
    {
        [ImportMany]
        public IEnumerable<IScanArguments> ArgumentSets { get; set; }

        public ArgumentHelper()
        {
            ArgumentSets = Enumerable.Empty<IScanArguments>();
        }

        public ParserResult<object> ParseArguments(string[] args)
        {
            return Parser.Default.ParseArguments(args, ArgumentSets.Select(x => x.GetType()).ToArray());
        }

        public ParserResult<T> ParseArguments<T>(string[] args, bool ignoreInvalidArgs = false)
        {
            Parser p = new Parser(x =>
            {
                x.IgnoreUnknownArguments = ignoreInvalidArgs;

                // This is not the main argument dispatch, so we don't want console output.
                x.HelpWriter = null;
            });

            return p.ParseArguments<T>(args);
        }

        public static IDictionary<string, string> GetDetectorArgs(IEnumerable<string> detectorArgsList)
        {
            var detectorArgs = new Dictionary<string, string>();

            foreach (var arg in detectorArgsList)
            {
                var keyValue = arg.Split('=');

                if (keyValue.Length != 2)
                {
                    continue;
                }

                detectorArgs.Add(keyValue[0], keyValue[1]);
            }

            return detectorArgs;
        }
    }
}
