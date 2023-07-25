using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language
{
    public class IdentifierToken : Token
    {
        public override Tokens Kind => Tokens.IDENTIFIER;

    }
}
