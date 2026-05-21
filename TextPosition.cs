struct TextPosition
{
    public uint _lineNumber;
    public byte _charNumber;

    public TextPosition(uint ln = 0, byte c = 0)
    {
        _lineNumber = ln;
        _charNumber = c;
    }
}