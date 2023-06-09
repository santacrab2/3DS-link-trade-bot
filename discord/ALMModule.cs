﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PKHeX.Core.AutoMod;
using PKHeX.Core;
using Discord;
using Discord.Interactions;

namespace _3DS_link_trade_bot
{
    [EnabledInDm(false)]
 
    public class ALMModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("convert", "Makes you a pokemon file from showdown text")]
        public async Task convert(string PokemonText)
        {
            await DeferAsync();
            ShowdownSet set = TradeModule.ConvertToShowdown(PokemonText);
            RegenTemplate rset = new(set);
            var trainer = NTR.game switch
            {
                4 => TrainerSettings.GetSavedTrainerData(GameVersion.USUM, 7),
                3 => TrainerSettings.GetSavedTrainerData(GameVersion.SM, 7),
                2 => TrainerSettings.GetSavedTrainerData(GameVersion.ORAS, 6),
                1 => TrainerSettings.GetSavedTrainerData(GameVersion.XY, 6)
            };
            var sav = SaveUtil.GetBlankSAV((GameVersion)trainer.Game, "pip");

            var pkm = sav.GetLegalFromSet(rset, out var res);
            //pkm = EntityConverter.ConvertToType(pkm, sav.PKMType, out var result);
 
            if (pkm is PK7)
            {
                
               PK7 pk7 = (PK7)pkm;
                pk7.SetDefaultRegionOrigins();
                pkm = pk7;
            }
               
            
         
            var correctfile = (NTR.game > 2 && pkm is PK7) ? true : pkm is PK6 ? true : false;

            if (!new LegalityAnalysis(pkm).Valid)
            {
                var reason = $"I wasn't able to create a {(Species)set.Species} from that set.";
                var imsg = $"Oops! {reason}";
                var tempfile2 = $"{Directory.GetCurrentDirectory()}//{pkm.FileName}";
                File.WriteAllBytes(tempfile2, pkm.DecryptedBoxData);
                imsg += $"\n{set.SetAnalysis(sav, pkm)} Attempted Save: {sav.Version}";
            
                await FollowupWithFileAsync(tempfile2,text:imsg, ephemeral: true).ConfigureAwait(false);
                File.Delete(tempfile2);
                return;
            }
            var tempfile = $"{Directory.GetCurrentDirectory()}//{pkm.FileName}";
            File.WriteAllBytes(tempfile, pkm.DecryptedBoxData);
            await FollowupWithFileAsync(tempfile, text:"Here is your legalized pk file");
            File.Delete(tempfile);
        }
    }
}
