struct Err
{
    private TextPosition _errorPosition;
    private byte _errorCode;

    public TextPosition ErrorPosition 
    {
        get;
        set;
    }

    public byte ErrorCode
    {
        get;
        set;
    }

    public Err(TextPosition _errorPosition, byte _errorCode)
    {
        ErrorPosition = _errorPosition;
        ErrorCode = _errorCode;
    }
}