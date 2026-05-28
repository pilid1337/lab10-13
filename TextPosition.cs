struct TextPosition
{
    private uint _lineNumber;
    private byte _charNumber;

    public uint LineNumber 
    {
        get;
        set;
    }

    public byte CharNumber
    {
        get;
        set;
    }

    public TextPosition(uint ln = 0, byte c = 0)
    {
        LineNumber = ln;
        CharNumber = c;
    }
}