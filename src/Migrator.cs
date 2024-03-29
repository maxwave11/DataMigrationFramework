﻿using System;
using System.Diagnostics;
using System.Linq;
using DataMigration.Pipeline;
using DataMigration.Pipeline.Commands;
using DataMigration.Pipeline.Trace;

namespace DataMigration
{
    public class Migrator
    {
        public MigrationTracer Tracer { get; }

        public static Migrator Current  {get; private set; }
        public bool ThrowExceptionOnError { get; }

        private MapConfig _mapConfig;

        public Migrator(MapConfig mapConfig, bool throwExeptionOnError = false)
        {
            if (mapConfig == null)
                throw new ArgumentNullException(nameof(mapConfig));

            _mapConfig = mapConfig;
            ThrowExceptionOnError = throwExeptionOnError;
            Current = this;
            Tracer = new MigrationTracer();
        }

        public void Run()
        {
            var migrationTimeCounter = new Stopwatch();
            migrationTimeCounter.Start();

            Tracer.TraceLine(" - Migration start...");

            try
            {
                InitializeVariables();
                InitializePipeline();
            }
            catch (DataMigrationException e)
            {
                Tracer.TraceMigrationException("Error occured while pipeline processing", e);
                if (ThrowExceptionOnError)
                    throw;
            }
            catch (Exception e)
            {
                Tracer.TraceLine(e.ToString());
                if (ThrowExceptionOnError)
                    throw;
            }
                
            migrationTimeCounter.Stop();

            Tracer.SaveLogs();
            Tracer.TraceLine($"\n - Migration end {migrationTimeCounter.Elapsed.TotalMinutes} mins");
        }

        private void InitializeVariables() 
        {
            if (_mapConfig.Variables == null)
                return;

            //Calculate variable values from appropriate YAML expressions
            foreach (string varName in _mapConfig.Variables.Keys)
            {
                if (_mapConfig.Variables[varName] is CommandBase command)
                {
                    var ctx = new ValueTransitContext(null, null);
                    ctx.Execute(command);
                    _mapConfig.Variables[varName] = ctx.TransitValue;
                }
            }
        }

        private void InitializePipeline() 
        {
            foreach (var pipeline in _mapConfig.Pipeline.Where(i => i.Enabled))
            {
                pipeline.Run();
            }
        }
    }
}


