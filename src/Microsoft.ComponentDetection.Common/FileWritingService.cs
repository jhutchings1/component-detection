﻿using System;
using System.Composition;
using System.IO;
using Microsoft.ComponentDetection.Common.Exceptions;

namespace Microsoft.ComponentDetection.Common
{
    [Export(typeof(IFileWritingService))]
    [Export(typeof(FileWritingService))]
    [Shared]
    public class FileWritingService : IFileWritingService
    {
        private object lockObject = new object();
        private string timestamp = DateTime.Now.ToString(TimestampFormatString);
        public const string TimestampFormatString = "yyyyMMddHHmmss";

        public string BasePath { get; private set; }

        public void Init(string basePath)
        {
            if (!string.IsNullOrEmpty(basePath) && !Directory.Exists(basePath))
            {
                throw new InvalidUserInputException($"The path {basePath} does not exist.", new DirectoryNotFoundException());
            }

            BasePath = string.IsNullOrEmpty(basePath) ? Path.GetTempPath() : basePath;
        }

        public void AppendToFile(string relativeFilePath, string text)
        {
            relativeFilePath = ResolveFilePath(relativeFilePath);

            lock (lockObject)
            {
                File.AppendAllText(relativeFilePath, text);
            }
        }

        public void WriteFile(string relativeFilePath, string text)
        {
            relativeFilePath = ResolveFilePath(relativeFilePath);

            lock (lockObject)
            {
                File.WriteAllText(relativeFilePath, text);
            }
        }

        public void WriteFile(FileInfo absolutePath, string text)
        {
            File.WriteAllText(absolutePath.FullName, text);
        }

        public string ResolveFilePath(string relativeFilePath)
        {
            EnsureInit();
            if (relativeFilePath.Contains("{timestamp}"))
            {
                relativeFilePath = relativeFilePath.Replace("{timestamp}", timestamp);
            }

            relativeFilePath = Path.Combine(BasePath, relativeFilePath);
            return relativeFilePath;
        }

        private void EnsureInit()
        {
            if (string.IsNullOrEmpty(BasePath))
            {
                throw new InvalidOperationException("Base path has not yet been initialized in File Writing Service!");
            }
        }
    }
}
