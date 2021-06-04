namespace Laser
{
    public enum CommandEnum
    {
        Invalid = -1,

        Init = 0b0111_0001,
        Ack = 0b0111_0010,
        Detach = 0b0111_1000,

        LaserOff = 0b0001_0000,
        LaserOn = 0b0001_0001,

        Image00 = 0b0010_0000,
        Image01 = 0b0010_0001,
        Image02 = 0b0010_0010,
        Image03 = 0b0010_0011,
        Image04 = 0b0010_0100,
        Image05 = 0b0010_0101,
        Image06 = 0b0010_0110,
        Image07 = 0b0010_0111,
        Image08 = 0b0010_1000,
        Image09 = 0b0010_1001,
        Image10 = 0b0010_1010,
        Image11 = 0b0010_1011,
        Image12 = 0b0010_1100,
        Image13 = 0b0010_1101,
        Image14 = 0b0010_1110,
        Image15 = 0b0010_1111,

        ExecStart = 0b0011_0000,
        ExecStop = 0b0011_1111,

        MoveUp = 0b0100_1000,
        MoveDown = 0b0100_0100,
        MoveLeft = 0b0100_0010,
        MoveRight = 0b0100_0001,
    }
}
