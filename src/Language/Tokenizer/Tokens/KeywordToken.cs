﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language
{
    public class KeywordToken : Token
    {
        public override Tokens Kind => Tokens.KEYWORD;
        public Keywords KeywordKind { get; set; }
    }
}
