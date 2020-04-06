namespace ClarkChatProtocol
{

    // Byte Enum up to 256 only
    public enum PayloadCommand
    {
        NONE,
        REGISTER,
        LOGIN,
        LOGOUT,
        WHISPER
    }
}
