using System;

[Serializable]
public class RoomState
{
    public bool wateredToday;
    public int burgerCount;
    public int failureCount;
    public bool lastServeSucceeded;

    public bool isCooking;
    public DateTime cookingEndsAtUtc;

    public string lastMessage;

    public void ResetDaily()
    {
        wateredToday = false;
    }

    /// <summary>게임 재시작 시 전체 초기화</summary>
    public void Reset()
    {
        wateredToday     = false;
        burgerCount      = 0;
        failureCount     = 0;
        lastServeSucceeded = false;
        isCooking        = false;
        cookingEndsAtUtc = default;
        lastMessage      = "같이 햄버거 만들어요!";
    }
}
