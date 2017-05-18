#define CONS

using System;
using System.Data;
using System.Linq;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public partial class Scheduler<TAmount>
            where TAmount : IComparable<TAmount>
        {
            internal class Iterator
            {
                class NodeAdapter
                {
                    public override string ToString()
                    {
                        return $"{Interval}";
                    }

                    private Restrictions _restrictions;
                    private INode<IteratorInterval> _node;
                    private readonly Func<INode<IteratorInterval>, INode<IteratorInterval>> _getNode;
                    private readonly Func<INode<IteratorInterval>, INode<IteratorInterval>> _getNextNode;
                    private readonly Func<INode<IteratorInterval>, INode<IteratorInterval>> _getPrevNode;
                    private readonly RequestAmountDelegate _requestAmount;
                    private readonly RequestDurationByAmountDelegate _requestDurationByAmount;

                    public NodeAdapter(Restrictions restrictions, INode<IteratorInterval> node, RequestAmountDelegate requestAmount, RequestDurationByAmountDelegate requestDurationByAmount, Func<INode<IteratorInterval>, INode<IteratorInterval>> getNextNode, Func<INode<IteratorInterval>, INode<IteratorInterval>> getPrevNode, Func<INode<IteratorInterval>, INode<IteratorInterval>> getNode)
                    {
                        _node = node;
                        _requestAmount = requestAmount;
                        _requestDurationByAmount = requestDurationByAmount;
                        _getNextNode = getNextNode;
                        _getPrevNode = getPrevNode;
                        _getNode = getNode;
                        _restrictions = restrictions;
                    }

                    public INode<IteratorInterval> Node => _node.Alive ? _node : (_node = _getNode(_node));

                    public IteratorInterval Interval => Node.Value;
                    public IteratorIntervalData IntervalData => Interval.Data;
                    public IteratorInstruction Instruction => IntervalData.Instruction;

                    private TDuration? _duration;
                    public ArithmeticAdapter<TDuration> Duration => _duration ?? (_duration = ToDuration(Interval)).Value;

                    public ArithmeticAdapter<TAmount> Amount => _requestAmount(Duration.Value, OriginalData, _restrictions.RequiredAmount);
                    public TData OriginalData => IntervalData.OriginalData;

                    public ArithmeticAdapter<TDuration> DurationByAmount(ArithmeticAdapter<TAmount> amount)
                    {
                        return _requestDurationByAmount(Duration.Value, Amount.Value, amount.Value);
                    }

                    public NodeAdapter Next()
                    {
                        var nextNode = _node.Alive ? _getNextNode(Node) : Node;
                        return new NodeAdapter(_restrictions, nextNode, _requestAmount, _requestDurationByAmount, _getNextNode, _getPrevNode, _getNode);
                    }
                    //public void MoveNext()
                    //{
                    //    _node = _node.Alive ? _getNextNode(Node) : Node;
                    //}
                    public void Reset()
                    {
                        _node = _getNode(_node);
                    }

                    public ArithmeticAdapter<TAmount> AmountByDuration(ArithmeticAdapter<TDuration> duration)
                    {
                        return duration == Duration
                            ? Amount
                            : _requestAmount(duration.Value, OriginalData, _restrictions.RequiredAmount);
                    }
                }


                private ISchedule _schedule;
                private readonly Restrictions _restrictions;
                private readonly SchedulerKind _kind;


                private Func<INode<IteratorInterval>, INode<IteratorInterval>> _getNextNode;
                private Func<INode<IteratorInterval>, INode<IteratorInterval>> _getPrevNode;

                private readonly RequestInstructionsDelegate _requestInstructions;
                private readonly RequestAmountDelegate _requestAmount;
                private readonly RequestDurationByAmountDelegate _requestDurationByAmount;
                private readonly RequestDataForAllocateDelegate _requestDataForAllocateDelegate;
                //private INode<IteratorInterval> _finishNode;

                //                private INode<IteratorInterval> _rootNode;
                private NodeAdapter Root;
                private ArithmeticAdapter<TAmount> Amount;
                private Point<TDuration> Duration;
                private Point<TDuration> TotalDuration;
                private PointedIntervalsContainer<IteratorInterval, TTime, IteratorIntervalData> Items;

                public TAmount RequiredAmount => _restrictions.RequiredAmount;
                private TDuration ScheduleMinDuration => _schedule.MinDuration;

                private ArithmeticAdapter<TAmount> AmountΔ => RequiredAmount - Amount;
                private ArithmeticAdapter<TDuration> MaxDuration => _restrictions.MaxDuration;
                private ArithmeticAdapter<TDuration> MinDuration => _restrictions.MinDuration;

                private ArithmeticAdapter<TDuration> MaxTotalDuration => _restrictions.MaxTotalDuration;
                private ArithmeticAdapter<TDuration> MinTotalDuration => _restrictions.MinTotalDuration;

                private ArithmeticAdapter<TDuration> MinDurationΔ => MinDuration - Duration;
                private ArithmeticAdapter<TDuration> MaxDurationΔ => MaxDuration - Duration;
                private ArithmeticAdapter<TDuration> MinTotalDurationΔ => MinTotalDuration - TotalDuration;
                private ArithmeticAdapter<TDuration> MaxTotalDurationΔ => MaxTotalDuration - TotalDuration;

                //INode<IteratorInterval> _firstNode => _getNextNode(_rootNode);

                private NodeAdapter _first;
                private NodeAdapter First => _first ?? (_first = GetNodeAdapter(_getNextNode(Root.Node), x => _getNextNode(Root.Node)));

                private NodeAdapter _current;
                private NodeAdapter Current
                {
                    get { return _current ?? (_current = GetNodeAdapter(_getNextNode(Root.Node), x => Root.Node)); }
                    set { _current = value; }
                }

                public Iterator(ISchedule schedule, Restrictions restrictions, SchedulerKind kind, RequestInstructionsDelegate requestInstructions, RequestAmountDelegate requestAmount,
                    RequestDurationByAmountDelegate requestDurationByAmount, RequestDataForAllocateDelegate requestDataForAllocateDelegate)
                {
                    _schedule = schedule;
                    _restrictions = restrictions;
                    _kind = kind;
                    _requestInstructions = requestInstructions;
                    _requestAmount = requestAmount;
                    _requestDurationByAmount = requestDurationByAmount;
                    _requestDataForAllocateDelegate = requestDataForAllocateDelegate;
                }

                NodeAdapter GetNodeAdapter(INode<IteratorInterval> node, Func<INode<IteratorInterval>, INode<IteratorInterval>> getRootNode)
                {
                    return new NodeAdapter(_restrictions, node, _requestAmount, _requestDurationByAmount, _getNextNode, _getPrevNode, getRootNode);
                }

                public void Init()
                {

                    if (_kind == SchedulerKind.LeftToRight)
                    {
                        _getNextNode = node => node?.Next;
                        _getPrevNode = node => node?.Previous;
                    }
                    else
                    {
                        _getNextNode = node => node?.Previous;
                        _getPrevNode = node => node?.Next;
                    }

                    var point = _kind == SchedulerKind.LeftToRight
                        ? _restrictions.MinStart
                        : _restrictions.MaxFinish;

                    Amount = ArithmeticAdapter<TAmount>.Zero;
                    Duration = default(TDuration).Right();
                    TotalDuration = default(TDuration).Right();


                    Items = new PointedIntervalsContainer<IteratorInterval, TTime, IteratorIntervalData>(
                        (left, right, dt) => new IteratorInterval(left, right, dt),
                        (a, b) => b,
                        (a, b) => null
                        );


                    var items = _schedule.Available.Get(_restrictions.MinStart, _restrictions.MaxFinish);

                    foreach (var item in items)
                    {
                        var instructions = _requestInstructions(((ICalendarItem) item).Data, _restrictions.RequiredAmount);

                        var minDurationAmount = instructions == IteratorInstruction.Accept
                            ? _requestAmount(ScheduleMinDuration, item.Data, RequiredAmount)
                            : default(TAmount);

                        Items.Include
                            (
                                item.Left,
                                item.Right,
                                new IteratorIntervalData(item.Data, instructions, minDurationAmount)
                            );
                    }

                    var fn = Items.FindNode(x => x.Contains(point));
                    Root = GetNodeAdapter(_getPrevNode(fn), x => Items.FindNode(y => y.Contains(x.Value.Left)));
                }

                public IteratorInterval[] Process()
                { 
                    var skipped = false;
                    var accepted = false;

                    ConsoleWriteLine($"-= Process =-  ***********************************************");


                    while (Current.OriginalData != null)
                    {
                        ConsoleWriteLine($"========================");
                        ConsoleWriteLine($"-> {Current.Interval}");
                        ConsoleWriteLine($"   amount:{Amount.Value}, duration:{Duration}");
                        ConsoleWriteLine($"   {Current.Instruction}", GetConsoleColor(Current.Instruction));

                        switch (Current.Instruction)
                        {
                            case IteratorInstruction.Skip:
                                if (skipped)
                                    ObstructByRejected();
                                else
                                    TotalDuration += Current.Duration;


                                if (ObstructBeginingByDurations())
                                {
                                    ConsoleWriteLine($"  -> обрезали начало по длительностям");
                                    ConsoleWriteLine($"     результат: amount:{Amount.Value}, duration:{Duration}, totalDuration:{TotalDuration}");
                                }


                                skipped = true;
                                break;
                            case IteratorInstruction.Reject:
                                accepted = false;
                                skipped = true;

                                ObstructByRejected();

                                break;
                            case IteratorInstruction.Accept:

                                skipped = false;
                                accepted = true;


                                // если недостающий объем > объема текущего куска, то берем кусок целиком
                                if (AmountΔ > Current.Amount)
                                {
                                    TakeCurrent();
                                    break;
                                }

                                // объема потенциально достаточно, либо уже набрали
                                // что бы попасть в диапазон длительностей, расчитываем минимальную и максимальную недостающие длительности
                                var minDurationΔ = MinDurationΔ.Max(MinTotalDurationΔ).Max(ScheduleMinDuration);
                                var maxDurationΔ = MaxDurationΔ.Min(MaxTotalDurationΔ).Max(ScheduleMinDuration);

                                // если рачетная минимальная недостающая длительность > длительности текущего куска, то берем кусок целиком
                                if (minDurationΔ >= Current.Duration)
                                {
                                    TakeCurrent();
                                    break;
                                }


                                var durationΔByAmountΔ = Current.DurationByAmount(AmountΔ);

                                var resultDurationΔ = durationΔByAmountΔ.Max(minDurationΔ);
                                if (resultDurationΔ <= maxDurationΔ)
                                {
                                    Duration += resultDurationΔ;
                                    TotalDuration += resultDurationΔ;
                                    Amount += Current.AmountByDuration(resultDurationΔ);

                                    //нашли
                                    Terminate(resultDurationΔ);
                                    break;
                                }

                                throw new RowNotInTableException();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (Completed())
                            break;

                        Current = Current.Next();
                    }

                    if (!Completed())
                    {
                        
                    }

                    var result = Items.Where(x => x.Data?.Instruction == IteratorInstruction.Accept)
                        .ToArray();

                    ConsoleWriteLine($"-= Completed, [{result.Length}] items for allocate =-");

                    return result;
                }

                private void Terminate(ArithmeticAdapter<TDuration> durationΔ)
                {
                    var obstructDuration = durationΔ;
                    Point<TTime> left;
                    Point<TTime> right;
                    if (_kind == SchedulerKind.LeftToRight)
                    {
                        left = Current.Interval.Left.OffsetToRight(obstructDuration);
                        right = _restrictions.MaxStart.Max(_restrictions.MaxFinish);
                    }
                    else
                    {
                        right = Current.Interval.Right.OffsetToLeft(obstructDuration);
                        left = _restrictions.MinStart.Min(_restrictions.MinFinish);
                    }

                    Items.Exclude(new IteratorInterval(left.Interval(right)));
                }

                private ConsoleColor GetConsoleColor(IteratorInstruction instruction)
                {
                    switch (instruction)
                    {
                        case IteratorInstruction.Skip:
                            return ConsoleColor.Yellow;
                        case IteratorInstruction.Reject:
                            return ConsoleColor.Red;
                        case IteratorInstruction.Accept:
                            return ConsoleColor.Green;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(instruction), instruction, null);
                    }
                }

                private void TakeCurrent()
                {
                    Duration += Current.Duration;
                    TotalDuration += Current.Duration;
                    Amount += Current.Amount;
                }

                bool Completed()
                {
                    var result = Amount >= RequiredAmount && _restrictions.Duration.Contains(Duration) && _restrictions.TotalDuration.Contains(TotalDuration);

                    return result;
                }

                private void ConsoleWriteDetails(ArithmeticAdapter<TAmount> deltaAmount, ArithmeticAdapter<TDuration> deltaDuration)
                {
                    ConsoleWriteLine("---------------");
                    if (First.Interval == Current.Interval)
                    {
                        ConsoleWriteLine($"    first == current");
                        ConsoleWriteLine($"    ---");
                    }
                    ConsoleWriteLine($"    first: {First.Interval}");
                    ConsoleWriteLine($"        min: {First.IntervalData.MinDurationAmount}");
                    if (First.Interval != Current.Interval)
                    {
                        ConsoleWriteLine($"    current: {Current.Interval}");
                        ConsoleWriteLine($"        min: {Current.IntervalData.MinDurationAmount}");
                    }
                    //ConsoleWriteLine($"    i: {Current.Interval}");
                    //ConsoleWriteLine($"        min: {Current.IntervalData.MinDurationAmount}");
                    ConsoleWriteLine($"    deltaAmount: {deltaAmount}");
                    ConsoleWriteLine($"    deltaDuration: {deltaDuration}");
                    ConsoleWriteLine("---------------");
                }

                private void ObstructFromRootTo(NodeAdapter to)
                {
                    var a = Root.Interval;
                    var b = to.Interval;
                    var left = a.Left.Min(b.Left);
                    var right = a.Right.Max(b.Right);
                    var interval = left.Interval(right);
                    Items.Exclude(new IteratorInterval(interval));
                }
                private void Obstruct(NodeAdapter node, ArithmeticAdapter<TDuration>? duration = null)
                {
                    var obstructDuration = duration ?? node.Duration;
                    Point<TTime> left;
                    Point<TTime> right;
                    if (_kind == SchedulerKind.LeftToRight)
                    {
                        left = node.Interval.Left;
                        right = left.OffsetToRight(obstructDuration).AsRight();
                    }
                    else
                    {
                        right = node.Interval.Right;
                        left = right.OffsetToLeft(obstructDuration).AsLeft();
                    }

                    Items.Exclude(new IteratorInterval(left.Interval(right)));
                }

                private void ObstructByRejected()
                {
                    IteratorInterval a = Root.Interval;
                    IteratorInterval b = Current.Interval;
                    var left = a.Left.Min(b.Left);
                    var right = a.Right.Max(b.Right);
                    var startFinishInterval = new Interval<TTime>(left, right);
                    ObstructFromRootTo(Current);

                    Current.Reset();
                    Amount = ArithmeticAdapter<TAmount>.Zero;
                    Duration = default(TDuration).Right();
                    TotalDuration = default(TDuration).Right();
                }

                private bool ObstructBeginingByDurations()
                {
                    var obstructed = false;
                    var zero = default(ArithmeticAdapter<TDuration>);

                    // превышения длительности
                    var delta = _restrictions.MaxDuration.IsUndefined ? zero : Duration - _restrictions.MaxDuration;

                    var totalDelta = _restrictions.MaxTotalDuration.IsUndefined ? zero : TotalDuration - _restrictions.MaxTotalDuration;

                    var instruction = IteratorInstruction.Accept;

                    while (delta > zero || totalDelta > zero || instruction != IteratorInstruction.Accept)
                    {
                        obstructed = true;
                        // если длительность не меньше, чем какая-нить из дельт...
                        var obstructDuration = First.Duration <= delta || First.Duration <= totalDelta ? First.Duration // ... режем целиком
                            : delta.Max(totalDelta).Value; // ... иначе будем резать по максимальной дельте

                        obstructDuration = ObstructNode(obstructDuration);

                        totalDelta -= obstructDuration;

                        if (instruction == IteratorInstruction.Accept)
                            delta -= obstructDuration;

                        instruction = First.Instruction;
                    }

                    return obstructed;
                }


                private void ObstructStartByInterval(ArithmeticAdapter<TDuration> intervalDuration)
                {
                    var zero = default(TDuration);

                    IteratorInstruction instruction = IteratorInstruction.Accept;
                    while (intervalDuration > zero || instruction != IteratorInstruction.Accept)
                    {
                        var obstructDuration = intervalDuration.Min(First.Duration);

                        obstructDuration = ObstructNode(obstructDuration);

                        intervalDuration -= obstructDuration;

                        instruction = First.Instruction;
                    }
                }

                private ArithmeticAdapter<TDuration> ObstructNode(ArithmeticAdapter<TDuration> obstructDuration)
                {
                    switch (First.Instruction)
                    {
                        case IteratorInstruction.Skip:
                        {
                            TotalDuration -= First.Duration;

                            //var left = Root.Interval.Right.AsLeft();
                            //var right = First.Interval.Right;
                            //var obstructionInterval = left.Interval(right);
                            Obstruct(First);
                            break;
                        }
                        case IteratorInstruction.Reject:
                            throw new NotSupportedException("не должно такого быть, от слова 'никогда'");
                        case IteratorInstruction.Accept:
                        {
                            // если остаток длительносит меньше минимальной длительности
                            if (First.Duration - obstructDuration < ScheduleMinDuration)
                            {
                                // то режем целиком
                                obstructDuration = First.Duration;
                            }

                            var obstrucAmount = First.AmountByDuration(obstructDuration.Value);

                            Amount -= obstrucAmount;
                            Duration -= obstructDuration;
                            TotalDuration -= obstructDuration;

                            Obstruct(First, obstructDuration);

                            //var left = Root.Interval.Right.AsLeft();
                            //var right = Root.Interval.Right.OffsetToRight(obstructDuration);
                            //var obstructionInterval = left.Interval(right);

                            //Obstruct(obstructionInterval);

                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return obstructDuration;
                }
            }

            internal class IteratorInterval : PointedInterval<TTime, IteratorIntervalData>
            {
                private TDuration? _duration;
                public TDuration Duration => _duration ?? (_duration = ToDuration(Left, Right)).Value;

                protected override string CustomToStringBeforeData()
                {
                    return Left.IsUndefined || Right.IsUndefined ? null : $", Duration: {Duration}";
                }

                protected internal IteratorInterval(Interval<TTime> interval, IteratorIntervalData data = null) : this(interval.Left, interval.Right, data ?? new IteratorIntervalData(default(TData), default(IteratorInstruction)))
                {
                }

                protected internal IteratorInterval(Point<TTime> left = null, Point<TTime> right = null, IteratorIntervalData data = null) : base(left, right, data)
                {
                }
            }

            internal class IteratorIntervalData
            {
                public override string ToString()
                {
                    return Instruction == IteratorInstruction.Accept ? $"{Instruction}, Data:[{OriginalData}], MinDurationAmount:[{MinDurationAmount}]" : $"{Instruction}";
                }

                public readonly TData OriginalData;
                public readonly IteratorInstruction Instruction;
                public readonly ArithmeticAdapter<TAmount> MinDurationAmount;

                public IteratorIntervalData(TData originalData, IteratorInstruction instruction, TAmount minDurationAmount = default(TAmount))
                {
                    OriginalData = originalData;
                    Instruction = instruction;
                    MinDurationAmount = minDurationAmount;
                }
            }

            private static void ConsoleWriteLine(object value, ConsoleColor? color = null)
            {
#if CONS
                var original = Console.ForegroundColor;
                if (color.HasValue)
                {
                    Console.ForegroundColor = color.Value;
                }
                try
                {
                    Console.WriteLine(value);
                }
                finally
                {
                    if (color.HasValue)
                        Console.ForegroundColor = original;
                }
#endif
            }
        }
    }
}