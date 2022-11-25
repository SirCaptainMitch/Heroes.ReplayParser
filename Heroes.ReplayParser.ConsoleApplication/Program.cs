using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Heroes.ReplayParser;
using System.Collections.Generic;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var replayCache = @"C:\Users\haman\OneDrive\Documents\Heroes of the Storm\Accounts\67450286\1-Hero-1-9771889\Replays\Other\NGS";
            //var heroesAccountsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Accounts");
            var heroesAccountsFolder = "D:/source/hots/hptest/replays";
            var randomReplayFileName = Directory.GetFiles(replayCache, "*.StormReplay", SearchOption.AllDirectories).OrderBy(i => Guid.NewGuid()).First();

            // Attempt to parse the replay
            // Ignore errors can be set to true if you want to attempt to parse currently unsupported replays, such as 'VS AI' or 'PTR Region' replays
            var (replayParseResult, replay) = DataParser.ParseReplay(randomReplayFileName, deleteFile: false, ParseOptions.DefaultParsing);

            // If successful, the Replay object now has all currently available information
            if (replayParseResult == DataParser.ReplayParseResult.Success)
            {
                var x = DataParser.ReplayParseResult.Duplicate;
                //Console.WriteLine("Replay Build: " + replay.ReplayBuild);
                Console.WriteLine("Map: " + replay.Map);
                var options = new JsonSerializerOptions { 
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                //string jsonString = JsonSerializer.Serialize(replay.TeamLevels, options);
                //Console.WriteLine(jsonString);
                string fileName = "d:/replay.json";

                string jsonString = JsonSerializer.Serialize(replay.DraftOrder, options);
                File.WriteAllText(fileName, jsonString);
                //Console.WriteLine(File.ReadAllText(fileName));
                //Console.WriteLine("Random Seed: " + replay.RandomValue);

                var result = new List<Dictionary<string, string>>();

                foreach (var pick in replay.DraftOrder)
                {
                    Console.WriteLine("hero: " + pick.HeroSelected + " - Pick Type: " + pick.PickType + " - SlotId: " + pick.SelectedPlayerSlotId);

                    var output = new Dictionary<string, string>();
                    output.Add("hero", pick.HeroSelected);
                    output.Add("pickType", pick.PickType.ToString());

                    result.Add(output);
                }                    
                jsonString = JsonSerializer.Serialize(result, options);
                    
                File.WriteAllText(fileName, jsonString);
                //foreach (var player in replay.Players.OrderByDescending(i => i.IsWinner))
                //    Console.WriteLine("Player: " + player.Name + ", Win: " + player.IsWinner + ", Hero: " + player.Character + ", Lvl: " + player.CharacterLevel + ", Talents: " + string.Join(",", player.Talents.Select(i => i.TalentID + ":" + i.TalentName)));

                Console.WriteLine("Press Any Key to Close");
            }
            else
                Console.WriteLine("Failed to Parse Replay: " + replayParseResult);

            Console.Read();
        }
    }
}
