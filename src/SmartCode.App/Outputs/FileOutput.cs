﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HandlebarsDotNet;

namespace SmartCode.App.Outputs
{
    public class FileOutput : IOutput
    {
        private readonly ILogger<FileOutput> _logger;

        public FileOutput(ILogger<FileOutput> logger)
        {
            _logger = logger;
        }

        public bool Initialized { get; private set; }

        public string Name { get; private set; } = "File";

        public void Initialize(IDictionary<string, object> paramters)
        {
            if (paramters == null) { return; }
            if (paramters.Value("Name", out string name))
            {
                Name = name;
            }
            Initialized = true;
        }

        public async Task Output(BuildContext context)
        {
            var output = context.Output;
            _logger.LogInformation($"------ Mode:{output.Mode},Build:{context.BuildKey} Start! ------");
            
            var outputPath = Handlebars.Compile(output.Path)(context);
            outputPath = Path.Combine(context.Project.OutputPath, outputPath);
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                _logger.LogWarning($"------ Directory:{outputPath} is not Exists,Created! ------");
            }
            var fileName = Handlebars.Compile(output.Name)(context) + output.Extension;
            var filePath = Path.Combine(outputPath, fileName);
            var fileExists = File.Exists(filePath);
            if (fileExists)
            {
                switch (output.Mode)
                {
                    case Configuration.CreateMode.Incre:
                        {
                            _logger.LogWarning($"------ Mode:{output.Mode},Build:{context.BuildKey},FilePath:{filePath} Exists ignore output End! ------");
                            return;
                        }
                    case Configuration.CreateMode.Full:
                        {
                            File.Delete(filePath);
                            _logger.LogWarning($"------ Mode:{output.Mode},FilePath:{filePath} Exists Deleted ! ------");
                            break;
                        }
                }
            }
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                await streamWriter.WriteAsync(context.Result);
            }
            _logger.LogInformation($"------ Mode:{output.Mode},Build:{context.BuildKey} -> {filePath} End! ------");
        }


    }
}
