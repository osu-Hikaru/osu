// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Users;
using osu.Framework;
using osu.Framework.Logging;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIScore : IScoreInfo
    {
        [JsonProperty(@"score")]
        public long TotalScore { get; set; }

        [JsonProperty(@"max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty(@"user")]
        public APIUser User { get; set; }

        [JsonProperty(@"id")]
        public long OnlineID { get; set; }

        [JsonProperty(@"replay")]
        public bool HasReplay { get; set; }

        [JsonProperty(@"created_at")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty(@"beatmap")]
        [CanBeNull]
        public APIBeatmap Beatmap { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty(@"beatmapset")]
        [CanBeNull]
        public APIBeatmapSet BeatmapSet
        {
            set
            {
                // in the deserialisation case we need to ferry this data across.
                // the order of properties returned by the API guarantees that the beatmap is populated by this point.
                if (!(Beatmap is APIBeatmap apiBeatmap))
                    throw new InvalidOperationException("Beatmap set metadata arrived before beatmap metadata in response");

                apiBeatmap.BeatmapSet = value;
            }
        }

        [JsonProperty("statistics")]
        public Dictionary<string, int> Statistics { get; set; }

        [JsonProperty(@"mode_int")]
        public int RulesetID { get; set; }

        [JsonProperty(@"mods")]
        private string[] mods { set => Mods = value.Select(acronym => new APIMod { Acronym = acronym }); }

        [NotNull]
        public IEnumerable<APIMod> Mods { get; set; } = Array.Empty<APIMod>();

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        /// <summary>
        /// Create a <see cref="ScoreInfo"/> from an API score instance.
        /// </summary>
        /// <param name="rulesets">A ruleset store, used to populate a ruleset instance in the returned score.</param>
        /// <param name="beatmap">An optional beatmap, copied into the returned score (for cases where the API does not populate the beatmap).</param>
        /// <returns></returns>
        public ScoreInfo CreateScoreInfo(RulesetStore rulesets, BeatmapInfo beatmap = null)
        {
            var ruleset = rulesets.GetRuleset(RulesetID) ?? throw new InvalidOperationException();

            var rulesetInstance = ruleset.CreateInstance();

            var modInstances = Mods.Select(apiMod => rulesetInstance.CreateModFromAcronym(apiMod.Acronym)).Where(m => m != null).ToArray();

            // all API scores provided by this class are considered to be legacy.
            //modInstances = modInstances.Append(rulesetInstance.CreateMod<ModClassic>()).ToArray();

            var scoreInfo = new ScoreInfo
            {
                TotalScore = TotalScore,
                MaxCombo = MaxCombo,
                BeatmapInfo = beatmap ?? new BeatmapInfo(),
                User = User,
                Accuracy = Accuracy,
                OnlineID = OnlineID,
                Date = Date,
                PP = PP,
                Hash = HasReplay ? "online" : string.Empty, // todo: temporary?
                Rank = Rank,
                Ruleset = ruleset,
                Mods = modInstances,
            };

            if (Statistics != null)
            {
                foreach (var kvp in Statistics)
                {
                    switch (kvp.Key)
                    {
                        case @"great":
                            scoreInfo.SetCountGreat(kvp.Value);
                            break;

                        case @"good":
                            scoreInfo.SetCountGood(kvp.Value);
                            break;

                        case @"ok":
                            scoreInfo.SetCountOk(kvp.Value);
                            break;

                        case @"meh":
                            scoreInfo.SetCountMeh(kvp.Value);
                            break;

                        case @"miss":
                            scoreInfo.SetCountMiss(kvp.Value);
                            break;

                        case @"small_bonus":
                            scoreInfo.SetCountSB(kvp.Value);
                            break;

                        case @"large_bonus":
                            scoreInfo.SetCountLB(kvp.Value);
                            break;

                        case @"small_tick_miss":
                            scoreInfo.SetCountSTM(kvp.Value);
                            break;

                        case @"small_tick_hit":
                            scoreInfo.SetCountSTH(kvp.Value);
                            break;

                        case @"large_tick_miss":
                            scoreInfo.SetCountLTM(kvp.Value);
                            break;

                        case @"large_tick_hit":
                            scoreInfo.SetCountLTH(kvp.Value);
                            break;

                        case @"ignore_miss":
                            scoreInfo.SetCountIM(kvp.Value);
                            break;

                        case @"ignore_hit":
                            scoreInfo.SetCountIH(kvp.Value);
                            break;
                    }
                }
            }

            return scoreInfo;
        }

        public IRulesetInfo Ruleset => new RulesetInfo { OnlineID = RulesetID };
        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => throw new NotImplementedException();

        #region Implementation of IScoreInfo

        IBeatmapInfo IScoreInfo.Beatmap => Beatmap;
        IUser IScoreInfo.User => User;

        #endregion
    }
}
