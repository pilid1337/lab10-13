struct Err
{
    public TextPosition _errorPosition;
    public byte _errorCode;
    
    public Err(TextPosition _errorPosition, byte _errorCode)
    {
        this._errorPosition = _errorPosition;
        this._errorCode = _errorCode;
    }
}