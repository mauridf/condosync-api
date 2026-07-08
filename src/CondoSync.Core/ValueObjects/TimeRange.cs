namespace CondoSync.Core.ValueObjects;

public class TimeRange : IEquatable<TimeRange>
{
    public TimeOnly Start { get; }
    public TimeOnly End { get; }
    public TimeSpan Duration => End - Start;

    public TimeRange(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
            throw new ArgumentException("Horário de início deve ser anterior ao término");

        Start = start;
        End = end;
    }

    public bool Overlaps(TimeRange other)
    {
        return Start < other.End && End > other.Start;
    }

    public bool Contains(TimeOnly time)
    {
        return time >= Start && time <= End;
    }

    public override string ToString() => $"{Start:HH:mm} - {End:HH:mm}";

    public override bool Equals(object? obj) => obj is TimeRange other && Equals(other);

    public bool Equals(TimeRange? other) =>
        other is not null && Start == other.Start && End == other.End;

    public override int GetHashCode() => HashCode.Combine(Start, End);

    public static bool operator ==(TimeRange left, TimeRange right) => left.Equals(right);

    public static bool operator !=(TimeRange left, TimeRange right) => !left.Equals(right);
}