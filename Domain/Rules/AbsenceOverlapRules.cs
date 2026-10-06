namespace TeamAvailability.Domain.Rules;

public static class AbsenceOverlapRules
{
    public static bool Overlaps(DateOnly aStart, DateOnly aEnd, DateOnly bStart, DateOnly bEnd) =>
        aStart <= bEnd && bStart <= aEnd;
}
