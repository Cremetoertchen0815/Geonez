﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Nez;

internal class MessageDBReader : ContentTypeReader<MessageDB>
{
    protected override MessageDB Read(ContentReader input, MessageDB existingInstance)
    {
        //Generate data holding lists
        var res = existingInstance ?? new MessageDB();
        var sets = new List<List<Message>>();
        var names = new List<string>();

        //Read DB
        if (Message.cVersion != input.ReadString())
            throw new ContentLoadException("MessageDB version differs from the currently loading one!");
        var cntA = input.ReadInt32();
        for (var i = 0; i < cntA; i++)
        {
            //Read set
            names.Add(input.ReadString());
            var elA = new List<Message>();
            var cntB = input.ReadInt32();
            for (var j = 0; j < cntB; j++)
            {
                //Read message
                var elB = new Message(input.ReadString(), null, null); //Read text
                //Read section formatting
                var elC = new List<MessageSectionFormat>();
                var cntC = input.ReadInt32();
                for (var k = 0; k < cntC; k++)
                    elC.Add(new MessageSectionFormat(input.ReadBoolean(), input.ReadSingle(), input.ReadColor()));
                elB.SectionFormatting = elC.ToArray();
                elB.Speaker = (input.ReadString(), input.ReadString()); //Read speaker and emote
                //Read Answers
                var elD = new List<(string, Telegram)>();
                var cntD = input.ReadInt32();
                for (var k = 0; k < cntD; k++)
                    elD.Add((input.ReadString(),
                        new Telegram("dialman", input.ReadString(), input.ReadString(), input.ReadString())));
                elB.Answers = elD.ToArray();
                //Read confirm reaction
                elB.ConfirmReaction = input.ReadBoolean()
                    ? new Telegram("dialman", input.ReadString(), input.ReadString(), input.ReadString())
                    : null;
                elB.AutoContinueDelay = input.ReadSingle(); //Read delay
                elB.DisplayFlags = (MessageFlags)input.ReadInt32(); //Read display flags
                //Add to set
                elA.Add(elB);
            }

            sets.Add(elA);
        }

        //Return data
        res._setList = sets.ToArray();
        res._setNames = names;
        return res;
    }
}