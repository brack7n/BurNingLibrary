using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FrameSet
{
    public static FrameSet Players;
    public int resMaxFrame;
    public Dictionary<MirAction, Frame> Frames = new Dictionary<MirAction, Frame>();

    static FrameSet()
    {
        Players = new FrameSet();

        #region Player Frames
        Players.Frames.Add(MirAction.Standing, new Frame(0, 4, 0, 32));
        Players.Frames.Add(MirAction.Walking, new Frame(32, 6, 0, 48));
        Players.Frames.Add(MirAction.Running, new Frame(80, 6, 0, 48));
        Players.Frames.Add(MirAction.BeiZhan, new Frame(128, 1, 0, 8));
        Players.Frames.Add(MirAction.Attack1, new Frame(136, 6, 0, 48));
        Players.Frames.Add(MirAction.Attack2, new Frame(184, 6, 0, 48));
        Players.Frames.Add(MirAction.Attack3, new Frame(232, 8, 0, 64));
        Players.Frames.Add(MirAction.ShiFa, new Frame(296, 6, 0, 48));
        Players.Frames.Add(MirAction.ShiQu, new Frame(344, 2, 0, 16));
        Players.Frames.Add(MirAction.BeiJi, new Frame(360, 3, 0, 24));
        Players.Frames.Add(MirAction.Die, new Frame(384, 4, 0, 32));
        Players.resMaxFrame = Players.Frames[MirAction.Die].FrameIndex;
        #endregion
    }
}

public class Frame
{
    public int Start, Count, Skip, MaxCount;

    public int OffSet
    {
        get { return Count + Skip; }
    }

    public int FrameIndex
    {
        get { return Start + MaxCount; }
    }

    public Frame(int start, int count, int skip, int maxCount)
    {
        Start = start;
        Count = count;
        Skip = skip;
        MaxCount = maxCount;
    }
}

