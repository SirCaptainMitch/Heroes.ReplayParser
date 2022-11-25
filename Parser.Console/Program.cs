using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using Parser.Shared;
using Heroes.ReplayParser;
using System.Dynamic;

namespace ConsoleApplication
{

    class Program
    {

        static void Main(string[] args)
        {

            string GetFingerprint(Replay replay)
            {
                var str = new StringBuilder();
                replay.Players.Select(p => p.BattleNetId).OrderBy(x => x).Map(x => str.Append(x.ToString()));
                str.Append(replay.RandomValue);
                var md5 = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str.ToString()));
                var result = new Guid(md5);
                return result.ToString();
            }

            var fullReplayCache = @"C:\Users\haman\OneDrive\Documents\Heroes of the Storm\Accounts\67450286\1-Hero-1-9771889\Replays\";
            var multiplayerReplayCache = @"C:\Users\haman\OneDrive\Documents\Heroes of the Storm\Accounts\67450286\1-Hero-1-9771889\Replays\Multiplayer";
            var alyReplayCache = @"C:\\Users\haman\OneDrive\Documents\Heroes of the Storm\Accounts\813804689\1-Hero-1-10958259\Replays\Multiplayer";
            var downloadsReplayCache = @"C:\Users\haman\OneDrive\Documents\Heroes of the Storm\Accounts\67450286\1-Hero-1-9771889\Replays\Other\Downloads\";
            var testReplayCache = @"C:\Users\haman\OneDrive\Documents\Heroes of the Storm\Accounts\67450286\1-Hero-1-9771889\Replays\Other\test";
            var allReplayCache = @"C:\Users\haman\OneDrive\Documents\Heroes of the Storm\Accounts\";


            var replayCache = allReplayCache;
            //var heroesAccountsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Accounts");

            string parseCache = "d:/parsed_replays/";
            string parserDatabase = "database.json";
            string folderName = @"d:\parser_file_cache";
            var parserDb = new List<ParserCachedFile>();
            var options = new JsonSerializerOptions { WriteIndented = true };
            string pathString = System.IO.Path.Combine(folderName, parserDatabase);
            var replayList = new List<string>();

            if (!File.Exists(pathString))
            {
                                
                System.IO.Directory.CreateDirectory(folderName);
                Console.WriteLine("Parser Cache DB does not exist.");
                //File.Create(pathString);
                var parserString = JsonSerializer.Serialize(parserDb, options);
                File.WriteAllText(pathString, parserString);
            }
            else
            {
                string parserDatabaseString = File.ReadAllText(pathString);
                parserDb = JsonSerializer.Deserialize<List<ParserCachedFile>>(parserDatabaseString)!;
            }

            var replayFiles = Directory.GetFiles(replayCache, "*.StormReplay", SearchOption.AllDirectories).OrderBy(i => Guid.NewGuid());

            foreach (var replayFile in replayFiles)
            {
                if (parserDb.Where((x) => x.ReplayFileName == replayFile).Any())
                {
                    var re = Path.GetFileName(replayFile);
                    var fileName = re.ToLower().Replace(".stormreplay", ".json");
                    //Console.WriteLine("File - " + fileName + " - seems to exist.");
                    re = null;
                } else
                {
                    replayList.Add(replayFile);
                }
            }

            Console.WriteLine("replay_count - " + replayFiles.Count().ToString());
            Console.WriteLine("replay_list_count - " + replayList.Count().ToString());
            Console.WriteLine("replay_database_count - " + parserDb.Count().ToString());
            
            foreach (var replayFile in replayList)
            {

                var re = Path.GetFileName(replayFile);
                var fileName = re.ToLower().Replace(".stormreplay", ".json");
                var parse_dt = DateTime.UtcNow.ToString();                

                var (replayParseResult, replay) = DataParser.ParseReplay(replayFile, deleteFile: false, new ParseOptions
                    {
                        ShouldParseUnits = false,
                        ShouldParseMouseEvents = false,
                        ShouldParseDetailedBattleLobby = true,
                        ShouldParseEvents = true,
                        ShouldParseMessageEvents = true,
                        ShouldParseStatistics = true
                });
                try
                {
                    Console.WriteLine("Parsing file - " + fileName);
                    if (replayParseResult == DataParser.ReplayParseResult.Success)
                    {
                        var fingerprint = GetFingerprint(replay);
                        List<string> parseModes = new List<string> { "StormLeague", "QuickMatch", "HeroLeague", "TeamLeague", "UnrankedDraft", "Custom" };              
                        if ( !parseModes.Contains(replay.GameMode.ToString()) )
                        {
                            Console.WriteLine("Skipping " + replay.GameMode.ToString() +  " file - " + fileName);
                            replay = null;
                            re = null;
                            //return;                            
                            var parsedFile = new ParserCachedFile
                            {
                                DocumentFileName = parseCache + fileName,
                                ReplayFileName = replayFile,
                                RandomValue = "ARAM files not being parsed",
                                ReplayFingerPrint = fingerprint,
                                ParsedDateTime = parse_dt,
                                IsError = true,
                                ReplayParseResult = "ARAM files not being parsed",
                            };

                            parserDb.Add(parsedFile);
                            var aramFile = JsonSerializer.Serialize(parserDb, options);
                            File.WriteAllText(pathString, aramFile);
                            continue;
                        }

                        var x = DataParser.ReplayParseResult.Duplicate;

                        var replayObj = new ReplayFile
                        {
                            RandomValue = replay.RandomValue.ToString(),
                            ReplayFingerPrint= fingerprint,
                            BattlegroundName = replay.Map,
                            ShortName = replay.MapAlternativeName,
                            GameType = replay.GameMode.ToString(),
                            GameLength = replay.ReplayLength.ToString(),
                            GameLengthTimestamp = replay.ReplayLength.TotalSeconds,
                            VersionBuild = replay.ReplayBuild.ToString(),
                            VersionMajor = replay.ReplayVersionMajor.ToString(),
                            Version = replay.ReplayVersion.ToString(),
                            GameDate = replay.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                            GameDateFormatted = replay.Timestamp.Date.ToShortDateString(),
                            Draft = new List<object>(),
                            FirstToTen = Convert.ToInt32(TeamSide.none),
                            DraftFirstTeam = Convert.ToInt32(TeamSide.left),
                            ReplayFileName = re,
                            ReplayFileNameFormatted = re.ToLower().Replace(".stormreplay", ""),
                            ReplayFilePath = Path.GetFullPath(replayFile) //replayCache,
                        };
                        //Console.WriteLine("... Object Successfully Created");
                        replayObj.WinningTeam = replay.Players.Select(x => replay.Players.Where((x) => x.IsWinner).FirstOrDefault()).FirstOrDefault().Team;

                        var leftTeamTen = replay.TeamLevels[0].ContainsKey(10) ? replay.TeamLevels[0].GetValueOrDefault(10) : new TimeSpan();
                        var rightTeamTen = replay.TeamLevels[0].ContainsKey(1) ? replay.TeamLevels[1].GetValueOrDefault(10) : new TimeSpan();

                        if (leftTeamTen < rightTeamTen)
                        {
                            replayObj.FirstToTen = Convert.ToInt32(TeamSide.left);
                        }
                        else if (leftTeamTen > rightTeamTen)
                        {
                            replayObj.FirstToTen = Convert.ToInt32(TeamSide.right);
                        }

                        // team 0 
                        var leftTeam = new ReplayTeam
                        {
                            RandomValue = replay.RandomValue.ToString(),
                            ReplayFingerPrint= fingerprint,
                            TeamId = Convert.ToInt32(TeamSide.left),
                            Players = new List<object>()
                        };

                        //leftTeam.TeamLevels.Add(replay.TeamLevels[0]);
                        replay.TeamPeriodicXPBreakdown[0].ForEach(xp =>
                        {
                            var xpObj = new ReplayTeamPeriodicXPBreakdown();

                            xpObj.TeamLevel = xp.TeamLevel;
                            xpObj.TimeSpan = xp.TimeSpan.TotalSeconds;
                            xpObj.MinionXP = xp.MinionXP;
                            xpObj.CreepXP = xp.CreepXP;
                            xpObj.StructureXP = xp.StructureXP;
                            xpObj.HeroXP = xp.HeroXP;
                            xpObj.TrickleXP = xp.TrickleXP;

                            leftTeam.TeamPeriodicXPBreakdown.Add(xpObj);
                        });


                        // team 1 
                        var rightTeam = new ReplayTeam
                        {
                            RandomValue = replay.RandomValue.ToString(),
                            ReplayFingerPrint= fingerprint,
                            TeamId = Convert.ToInt32(TeamSide.right),
                            Players = new List<object>()
                        };
                        //Console.WriteLine("...... Teams Successfully Created");
                        //rightTeam.TeamLevels.Add(replay.TeamLevels[1]);
                        replay.TeamPeriodicXPBreakdown[1].ForEach(xp =>
                        {
                            var xpObj = new ReplayTeamPeriodicXPBreakdown();

                            xpObj.TeamLevel = xp.TeamLevel;
                            xpObj.TimeSpan = xp.TimeSpan.TotalSeconds;
                            xpObj.MinionXP = xp.MinionXP;
                            xpObj.CreepXP = xp.CreepXP;
                            xpObj.StructureXP = xp.StructureXP;
                            xpObj.HeroXP = xp.HeroXP;
                            xpObj.TrickleXP = xp.TrickleXP;

                            rightTeam.TeamPeriodicXPBreakdown.Add(xpObj);
                        });


                        foreach (var p in replay.Players)
                        {
                            var playerfirstToTen = replayObj.FirstToTen == p.Team ? true : false;

                            var scoreResult = new PlayerScoreResult();

                            scoreResult.Level = p.ScoreResult.Level;
                            scoreResult.Takedowns = p.ScoreResult.Takedowns;
                            scoreResult.SoloKills = p.ScoreResult.SoloKills;
                            scoreResult.Assists = p.ScoreResult.Assists;
                            scoreResult.Deaths = p.ScoreResult.Deaths;
                            scoreResult.HeroDamage = p.ScoreResult.HeroDamage;
                            scoreResult.SiegeDamage = p.ScoreResult.SiegeDamage;
                            scoreResult.StructureDamage = p.ScoreResult.StructureDamage;
                            scoreResult.MinionDamage = p.ScoreResult.MinionDamage;
                            scoreResult.CreepDamage = p.ScoreResult.CreepDamage;
                            scoreResult.SummonDamage = p.ScoreResult.SummonDamage;
                            scoreResult.TimeCCdEnemyHeroes = p.ScoreResult.TimeCCdEnemyHeroes.GetValueOrDefault().TotalSeconds;
                            scoreResult.Healing = p.ScoreResult.Healing;
                            scoreResult.SelfHealing = p.ScoreResult.SelfHealing;
                            scoreResult.RegenGlobes = p.ScoreResult.RegenGlobes;
                            scoreResult.DamageTaken = p.ScoreResult.DamageTaken;
                            scoreResult.DamageSoaked = p.ScoreResult.DamageSoaked;
                            scoreResult.ExperienceContribution = p.ScoreResult.ExperienceContribution;
                            scoreResult.TownKills = p.ScoreResult.TownKills;
                            scoreResult.TimeSpentDead = p.ScoreResult.TimeSpentDead.TotalSeconds;
                            scoreResult.MercCampCaptures = p.ScoreResult.MercCampCaptures;
                            scoreResult.WatchTowerCaptures = p.ScoreResult.WatchTowerCaptures;
                            scoreResult.MetaExperience = p.ScoreResult.MetaExperience;
                            scoreResult.HighestKillStreak = p.ScoreResult.HighestKillStreak;
                            scoreResult.ProtectionGivenToAllies = p.ScoreResult.ProtectionGivenToAllies;
                            scoreResult.TimeSilencingEnemyHeroes = p.ScoreResult.TimeSilencingEnemyHeroes;
                            scoreResult.TimeRootingEnemyHeroes = p.ScoreResult.TimeRootingEnemyHeroes;
                            scoreResult.TimeStunningEnemyHeroes = p.ScoreResult.TimeStunningEnemyHeroes;
                            scoreResult.ClutchHealsPerformed = p.ScoreResult.ClutchHealsPerformed;
                            scoreResult.EscapesPerformed = p.ScoreResult.EscapesPerformed;
                            scoreResult.VengeancesPerformed = p.ScoreResult.VengeancesPerformed;
                            scoreResult.OutnumberedDeaths = p.ScoreResult.OutnumberedDeaths;
                            scoreResult.TeamfightEscapesPerformed = p.ScoreResult.TeamfightEscapesPerformed;
                            scoreResult.TeamfightHealingDone = p.ScoreResult.TeamfightHealingDone;
                            scoreResult.TeamfightDamageTaken = p.ScoreResult.TeamfightDamageTaken;
                            scoreResult.TeamfightHeroDamage = p.ScoreResult.TeamfightHeroDamage;
                            scoreResult.Multikill = p.ScoreResult.Multikill;
                            scoreResult.PhysicalDamage = p.ScoreResult.PhysicalDamage;
                            scoreResult.SpellDamage = p.ScoreResult.SpellDamage;
                            scoreResult.OnFireTimeonFire = p.ScoreResult.OnFireTimeonFire.GetValueOrDefault().TotalSeconds;

                            foreach (var award in p.ScoreResult.MatchAwards)
                            {
                                scoreResult.MatchAwards.Add(award.ToString());
                            }

                            var output = new ReplayPlayer
                            {
                                RandomValue = replay.RandomValue.ToString(),
                                ReplayFingerPrint= fingerprint,
                                Battletag = p.Name + "#" + p.BattleTag,
                                BlizzardId = p.BattleNetId,
                                PlayerType = p.PlayerType.ToString(),
                                Region = p.BattleNetRegionId,
                                Team = p.Team,
                                IsWinner = p.IsWinner,
                                Character = p.Character,
                                HeroId = p.HeroId,
                                AttributeId = p.HeroAttributeId,
                                CharacterLevel = p.CharacterLevel,
                                Party = p.PartyValue,
                                FirstToTen = playerfirstToTen,
                                ScoreResult = scoreResult,
                                AccountLevel = p.AccountLevel
                            }; 

                            List<int> list = new List<int>(p.Talents.Select(x => x.TalentID));
                            List<string> tierList = new List<string>() { "One", "Four", "Seven", "Ten", "Thirteen", "Sixteen", "Twenty" };

                            foreach (var tal in p.Talents)
                            {
                                //Console.WriteLine(tal.TalentName);
                                var talIndex = list.IndexOf(tal.TalentID);
                                var talTier = tierList[talIndex];

                                var talent = new ReplayPlayerTalent
                                {
                                    RandomValue = replay.RandomValue.ToString(),
                                    ReplayFingerPrint= fingerprint,
                                    Battletag = output.Battletag,
                                    TalentIndex = talIndex,
                                    TalentTier = talTier,
                                    TalentId = tal.TalentID,
                                    TalentName = tal.TalentName,
                                    TimeSpanSelectedString = tal.TimeSpanSelected.ToString(),
                                    TimeSpanSelected = tal.TimeSpanSelected.TotalSeconds
                                };

                                output.Talents.Add(talent);
                            }

                            if (p.Team == 0)
                            {
                                leftTeam.Players.Add(output);
                            }
                            else
                            {
                                rightTeam.Players.Add(output);
                            }
                        }

                        replayObj.Teams.Add(leftTeam);
                        replayObj.Teams.Add(rightTeam);

                        var draftModes = new List<string>();
                        draftModes.Add("HeroLeague");
                        draftModes.Add("TeamLeague");
                        draftModes.Add("UnrankedDraft");
                        draftModes.Add("StormLeague");
                        draftModes.Add("Custom");


                        if (replay.DraftOrder.Count > 0 && draftModes.Contains(replay.GameMode.ToString()))
                        {
                            foreach (var pick in replay.DraftOrder)
                            {
                                List<string> draftList = new List<string>(replay.DraftOrder.Select(d => d.HeroSelected));
                                var DraftIndex = draftList.LastIndexOf(pick.HeroSelected);

                                var output = new ReplayDraftPick
                                {
                                    DraftIndex = DraftIndex,
                                    ReplayFingerPrint= fingerprint,
                                    Team = Convert.ToInt32(TeamSide.none),
                                    RandomValue = replay.RandomValue.ToString(),
                                    AltName = pick.HeroSelected,
                                    PickType = pick.PickType.ToString(),
                                    SelectedPlayerSlotId = pick.SelectedPlayerSlotId,
                                };

                                if (replay.Players.Select((x) => x.HeroId).Contains(pick.HeroSelected))
                                {
                                    var he = replay.Players.Where(x => x.HeroId == pick.HeroSelected).FirstOrDefault();

                                    output.Team = he.Team;
                                    output.Battletag = he.Name + "#" + he.BattleTag;
                                }

                                replayObj.Draft.Add(output);

                            }

                            var firstPick = replay.DraftOrder.FirstOrDefault((x) => x.PickType.ToString() == "Picked").HeroSelected;
                            replayObj.DraftFirstTeam = replay.Players.FirstOrDefault((x) => x.HeroId == firstPick).Team;
                        }
                        

                        var parsedFileSuccess = new ParserCachedFile
                        {
                            DocumentFileName = parseCache + fileName,
                            ReplayFileName = replayFile,
                            RandomValue = replayObj.RandomValue.ToString(),
                            ParsedDateTime = parse_dt,
                            ReplayFingerPrint= fingerprint,
                            IsError = false, 
                            ReplayParseResult = null

                        };

                        parserDb.Add(parsedFileSuccess);

                        string jsonString = JsonSerializer.Serialize(replayObj, options);
                        File.WriteAllText(parseCache + fileName, jsonString);
                        replay = null;
                        var s = JsonSerializer.Serialize(parserDb, options);
                        File.WriteAllText(pathString, s);
                    }
                    else
                    {
                        var parsedFileFail = new ParserCachedFile
                        {
                            DocumentFileName = parseCache + fileName,
                            ReplayFileName = replayFile,
                            RandomValue = "-1",
                            ParsedDateTime = parse_dt,
                            IsError = true,
                            ReplayParseResult = replayParseResult.ToString()
                        };

                        parserDb.Add(parsedFileFail);
                        var s = JsonSerializer.Serialize(parserDb, options);
                        File.WriteAllText(pathString, s);
                        Console.WriteLine("Failed to Parse Replay: " + replayParseResult);
                    }

                } catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            };

            //Console.WriteLine("exit");

            //Console.Read();
        }

    }
}
