using System.Text;

namespace Language;

public class Tokenizer
{
    private int _cursor;
    private readonly byte[] _source;

    public Tokenizer(byte[] source)
    {
        _source = source;
        _cursor = 0;
    }

    public bool HasMoreTokens()
    {
        return _cursor < _source.Length;
    }

    public bool EOF()
    {
        return _cursor >= _source.Length;
    }

    public Token? GetNextToken()
    {
        if (!this.HasMoreTokens())
        {
            return new EOFToken()
            {
            };
        }
        var span = new ReadOnlySpan<byte>(_source);

        if (IsWhitespace(span[_cursor]))
        {
            var start = _cursor;
            while (HasMoreTokens() && IsWhitespace(span[_cursor]))
            {
                _cursor++;
            }

            return null;

            //return new WhitespaceToken()
            //{
            //    RawValue = span.Slice(start, _cursor - start).ToArray(),
            //};
        }

        if (IsIdentifierStart(span[_cursor]))
        {
            var start = _cursor;
            ReadIdentifer();
            var identifier = span.Slice(start, _cursor - start);

            if (IsKeyword(identifier))
            {
                var en = GetKeywordEnum(identifier);
                return new KeywordToken()
                {
                    RawValue = identifier.ToArray(),
                    KeywordKind = en
                };
            }
            else if (IsNullWord(identifier))
            {
                return new NullToken()
                {
                    RawValue = identifier.ToArray()
                };
            }
            else if (IsBooleanWord(identifier))
            {
                return new BooleanToken()
                {
                    RawValue = identifier.ToArray()
                };
            }

            return new IdentifierToken()
            {
                RawValue = identifier.ToArray()
            };
        }

        if (IsDigit(span[_cursor]))
        {
            var start = _cursor;
            while (HasMoreTokens() && IsDigit(span[_cursor]))
            {
                _cursor++;
            }

            return new IntegerToken()
            {
                RawValue = span.Slice(start, _cursor - start).ToArray(),
            };
        }

        if (IsDblQuote(span[_cursor]))
        {
            var start = _cursor;
            _cursor++;
            while (HasMoreTokens())
            {
                if (IsDblQuote(span[_cursor]))
                {
                    if (IsBackslash(span[_cursor - 1]))
                    {
                        _cursor++;
                        continue;
                    }
                    _cursor++;
                    break;
                }
                _cursor++;
            }

            return new StringToken()
            {
                RawValue = span.Slice(start, _cursor - start).ToArray()
            };
        }

        if (IsPunctuator(span[_cursor]))
        {
            return ReadPunctuator();
        }

        throw new Exception($"Unexpected token at {_cursor}, value: 0x{span[_cursor].ToString("X2")}: {Encoding.UTF8.GetString(span.Slice(_cursor).ToArray())}");
    }

    public bool IsValidPunctuator(ReadOnlySpan<byte> bytes)
    {
        switch (bytes.Length)
        {
            case 1:
                return IsValidSoloPunctuator(bytes[0])
                    ;
            case 2:
                return bytes.SequenceEqual(new byte[] { Bytes.Equal, Bytes.Equal })            // ==
                    || bytes.SequenceEqual(new byte[] { Bytes.Pipe, Bytes.Pipe })              // ||
                    || bytes.SequenceEqual(new byte[] { Bytes.Amp, Bytes.Amp })                // &&
                    || bytes.SequenceEqual(new byte[] { Bytes.Exclaim, Bytes.Equal })          // !=
                    || bytes.SequenceEqual(new byte[] { Bytes.Less, Bytes.Equal })             // <=
                    || bytes.SequenceEqual(new byte[] { Bytes.Greater, Bytes.Equal })          // >=                    
                    || bytes.SequenceEqual(new byte[] { Bytes.Less, Bytes.ForwardSlash })      // </     
                    || bytes.SequenceEqual(new byte[] { Bytes.ForwardSlash, Bytes.Greater })    // />     
                    || bytes.SequenceEqual(new byte[] { Bytes.LeftCurly, Bytes.Hash })         // {#
                    || bytes.SequenceEqual(new byte[] { Bytes.LeftCurly, Bytes.ForwardSlash }) // {/
                    ;
        }

        return false;
    }

    public Token ReadPunctuator()
    {
        var span = new ReadOnlySpan<byte>(_source);
        var start = _cursor;
        ReadAllPunctuators();
        var fullPunc = span.Slice(start, _cursor - start);

        if (!IsValidPunctuator(fullPunc))
        {
            var singlePunc = span.Slice(start, 1);

            if (IsValidSoloPunctuator(singlePunc[0]))
            {
                _cursor = start + 1;
                return new PunctuatorToken()
                {
                    RawValue = singlePunc.ToArray()
                };
            }


            throw new Exception($"Not Valid Punctuator {string.Join(", ", fullPunc.ToArray().Select(x => "0x" + x.ToString("X2")))}");
        }

        return new PunctuatorToken()
        {
            RawValue = fullPunc.ToArray()
        };
    }

    public void ReadAllPunctuators()
    {
        var span = new ReadOnlySpan<byte>(_source);
        while (!EOF())
        {
            if (!IsPunctuator(span[_cursor]))
            {
                break;
            }
            _cursor++;
        }
    }

    public bool IsPunctuator(byte ch)
    {
        return ch == Bytes.Pipe
            || ch == Bytes.Amp
            || IsValidSoloPunctuator(ch)
            ;
    }

    public bool IsValidSoloPunctuator(byte ch)
    {
        return ch == Bytes.Period
           || ch == Bytes.Equal
           || ch == Bytes.Less
           || ch == Bytes.Greater
           || ch == Bytes.Exclaim
           //|| ch == Bytes.Pipe
           //|| ch == Bytes.Amp
           || ch == Bytes.Astrix
           || ch == Bytes.Plus
           || ch == Bytes.Minus
           || ch == Bytes.LeftParen
           || ch == Bytes.RightParen
           || ch == Bytes.LeftCurly
           || ch == Bytes.RightCurly
           || ch == Bytes.SemiColon
           || ch == Bytes.Colon
           || ch == Bytes.ForwardSlash
           || ch == Bytes.Hash;
    }



    public void ReadIdentifer()
    {
        var span = new ReadOnlySpan<byte>(_source);
        while (!EOF())
        {
            if (IsIdentifierPart(span[_cursor]))
            {
                _cursor++;
            }
            else
            {
                break;
            }
        }
    }

    public bool IsKeyword(ReadOnlySpan<byte> bytes)
    {
        switch (bytes.Length)
        {
            case 2:
                return bytes.SequenceEqual(new byte[] { Bytes.LittleI, Bytes.LittleF })  // if
                    || bytes.SequenceEqual(new byte[] { Bytes.LittleI, Bytes.LittleN })  // in
                    || bytes.SequenceEqual(new byte[] { Bytes.LittleA, Bytes.LittleS }); // as
            case 3:
                return bytes.SequenceEqual(new byte[] { Bytes.LittleN, Bytes.LittleE, Bytes.LittleW })  // new
                    || bytes.SequenceEqual(new byte[] { Bytes.LittleF, Bytes.LittleO, Bytes.LittleR })  // for
                    || bytes.SequenceEqual(new byte[] { Bytes.LittleV, Bytes.LittleA, Bytes.LittleR }); // var
        }


        return false;
    }

    public Keywords GetKeywordEnum(ReadOnlySpan<byte> bytes)
    {
        if (bytes.SequenceEqual(new byte[] { Bytes.LittleI, Bytes.LittleF }))
        {
            return Keywords.IF;
        }

        if (bytes.SequenceEqual(new byte[] { Bytes.LittleI, Bytes.LittleN }))
        {
            return Keywords.IN;
        }

        if (bytes.SequenceEqual(new byte[] { Bytes.LittleA, Bytes.LittleS }))
        {
            return Keywords.AS;
        }

        if (bytes.SequenceEqual(new byte[] { Bytes.LittleN, Bytes.LittleE, Bytes.LittleW }))
        {
            return Keywords.NEW;
        }

        if (bytes.SequenceEqual(new byte[] { Bytes.LittleF, Bytes.LittleO, Bytes.LittleR }))
        {
            return Keywords.FOR;
        }

        if (bytes.SequenceEqual(new byte[] { Bytes.LittleV, Bytes.LittleA, Bytes.LittleR }))
        {
            return Keywords.VAR;
        }

        throw new Exception($"That keyword isn't supported {Encoding.UTF8.GetString(bytes)}");
    }

    public bool IsBooleanWord(ReadOnlySpan<byte> bytes)
    {
        return bytes.SequenceEqual(new byte[] { Bytes.LittleT, Bytes.LittleR, Bytes.LittleU, Bytes.LittleE }) // true
            || bytes.SequenceEqual(new byte[] { Bytes.LittleF, Bytes.LittleA, Bytes.LittleL, Bytes.LittleS, Bytes.LittleE });  //  false
    }

    public bool IsNullWord(ReadOnlySpan<byte> bytes)
    {
        return bytes.SequenceEqual(new byte[] { Bytes.LittleN, Bytes.LittleU, Bytes.LittleL, Bytes.LittleL });  //  null
    }


    public bool IsIdentifierStart(byte ch)
    {
        return (ch >= 0x41 && ch <= 0x5A)  // A..Z
            || (ch >= 0x61 && ch <= 0x7A); // a..z
    }

    public bool IsIdentifierPart(byte ch)
    {
        return (ch == 0x5F)                // _ (underscore)
            || IsIdentifierStart(ch)
            || (ch >= 0x30 && ch <= 0x39); // 0..9
    }



    public bool IsWhitespace(byte ch)
    {
        return ch == Bytes.Space
            || ch == 0x09
            || ch == 0x0B
            || ch == 0x0C
            || ch == 0xA0
            || IsLineTerminator(ch);
    }

    public bool IsLineTerminator(byte ch)
    {
        return ch == Bytes.Return
            || ch == Bytes.CarriageReturn;
    }

    public bool IsDigit(byte ch)
    {
        return ch == Bytes.Zero
            || ch == Bytes.One
            || ch == Bytes.Two
            || ch == Bytes.Three
            || ch == Bytes.Four
            || ch == Bytes.Five
            || ch == Bytes.Six
            || ch == Bytes.Seven
            || ch == Bytes.Eight
            || ch == Bytes.Nine;
    }

    public bool IsBackslash(byte ch)
    {
        return ch == Bytes.BackSlash;
    }

    public bool IsDblQuote(byte ch)
    {
        return ch == Bytes.DblQuote;
    }
}