namespace ClarkChatProtocol
{

    // Byte Enum up to 256 only
    public enum PayloadCommand
    {
        None,
        REGISTER,
        LOGIN,
        LOGOUT,
        WHISPER
    }
}