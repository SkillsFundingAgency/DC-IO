﻿using System;

namespace ESFA.DC.IO.SqlServer.Model
{
    public sealed class SqlKey
    {
        public SqlKey(string key)
        {
            string[] tokens = key.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(key), "Key should be formatted as JobId_Item_ActorId e.g. 1089_2_4");
            }

            JobId = int.Parse(tokens[0]);
            Item = int.Parse(tokens[1]);
            Actor = int.Parse(tokens[2]);
        }

        public SqlKey(int jobId, int item, int actor)
        {
            JobId = jobId;
            Item = item;
            Actor = actor;
        }

        public int JobId { get; }

        public int Item { get; }

        public int Actor { get; }

        public override string ToString()
        {
            return $"{JobId}_{Item}_{Actor}";
        }
    }
}
