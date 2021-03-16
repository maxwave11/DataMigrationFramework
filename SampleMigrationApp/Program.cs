using SampleMigrationApp.PipelineCommands;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using XQ.DataMigration;
using XQ.DataMigration.Pipeline.Trace;
using XQ.EqDataMigrator.TargetProvider;

namespace SampleMigrationApp
{
    class Program
    {
        private static StreamWriter _file;
        private static StringBuilder logsChunk = new StringBuilder();
        private static Stopwatch logTimer = new Stopwatch();

        static void Main(string[] args)
        {
            Console.SetWindowSize(200, 50);
            Console.BufferHeight = 30000;


            var commands = new[] {
                // typeof(AssetTransition),
                typeof(SetCustomField),
                typeof(SetCashFlowValue),
                typeof(XqDataSource),
                typeof(SetAssumption),
                typeof(JsonDataSource),
                typeof(ToRecurrancePattern),
            };


            Console.WriteLine("Reading Map config file");

            var mapConfig = MapConfig.ReadFromFile("SampleBigConfig.yaml", commands);

            var migrator = new Migrator(mapConfig);
            migrator.Tracer.TransitValueStarted += (s, e) =>
            {
                CheckCommands();
            };

            migrator.Tracer.Trace += Migrator_Trace;

            migrator.Run();

            LogToFile("", true);
            /////////////////////
            Console.ReadLine();
        }

        private static void Migrator_Trace(object sender, TraceMessage e)
        {
            Console.ForegroundColor = e.Color;
            Console.Write(e.Text);
            Debug.Write(e.Text);
            LogToFile(e.Text);
            CheckCommands();
        }

        private static void LogToFile(string text, bool isLastCall = false)
        {
            if (_file == null)
            {
                System.IO.Directory.CreateDirectory("logs");
                _file = new StreamWriter($"logs\\logs_{ DateTime.Now.ToString("yyyy.MM.dd.hh.mm.ss")}.txt", false);
            }

            if (!logTimer.IsRunning)
                logTimer.Start();

            logsChunk.Append(text);

            //write to file every 20 secs (for perfomance)
            if (logTimer.Elapsed.Seconds > 10 || isLastCall)
            {
                _file.Write(logsChunk.ToString());
                logsChunk.Clear();

                logTimer.Restart();

                if (isLastCall)
                    _file.Dispose();
            }

        }


        private static bool CheckContinue()
        {
            Console.WriteLine("\n Продолжить импорт? (y/n)");

            return Console.ReadKey().Key == ConsoleKey.Y;
        }

        private static void CheckCommands()
        {
            if (!Console.KeyAvailable) return;

            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.P)
            {
                Console.WriteLine("Pause... Press P to continue");
                do
                {
                    Thread.Sleep(1000);
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.P)
                        return;

                } while (true);
            }
            // if (key == ConsoleKey.S)
            //   TransitLogger.IsConsoleSaveLogsEnabled = !TransitLogger.IsConsoleSaveLogsEnabled;
        }
    }
}
