using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Resources;
using Microsoft.SqlServer.Server;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;
using Protoplasm.Utils.Graph;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public class Scheduler<TAmount>
            where TAmount : IComparable<TAmount>
        {
            static Scheduler()
            {
                DataAdapter<TAmount>.Add = (a, b) => AddAmount(a, b);
                DataAdapter<TAmount>.Subst = (a, b) => SubstAmount(a, b);
            }

            public static Func<TAmount, TAmount, TAmount> AddAmount;
            public static Func<TAmount, TAmount, TAmount> SubstAmount;

            private RequestInstructionsDelegate _requestInstructions;
            private RequestAmountDelegate _requestAmount;
            private RequestDurationByAmountDelegate _requestDurationByAmount;

            public static TDuration ToDuration(Interval<TTime> interval)
            {
                return ToDuration(interval.Left, interval.Right);
            }
            public static TDuration ToDuration(Point<TTime> left, Point<TTime> right)
            {
                var duration = Calendars<TTime, TDuration>.ToDuration(left, right);
                if (!duration.HasValue)
                    throw new NotSupportedException("не удалось вычислить длительность");
                return duration.Value;
            }

            //public static TAmount Amount(TDuration duration, TData data, TAmount requiredAmount)
            //{
            //    return CalculateAmount(duration, data, requiredAmount);
            //}


            ISchedule _schedule;


            public Scheduler(ISchedule schedule, RequestInstructionsDelegate requestInstructions, RequestAmountDelegate requestAmount, RequestDurationByAmountDelegate requestDurationByAmount)
            {
                _schedule = schedule;
                _requestInstructions = requestInstructions;
                _requestAmount = requestAmount;
                _requestDurationByAmount = requestDurationByAmount;
            }

            public void FindAllocation(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> totalDuration, Interval<TDuration> duration, TAmount amount, SchedulerKind kind)
            {
                start = start ?? new Interval<TTime>();
                finish = finish ?? new Interval<TTime>();
                totalDuration = totalDuration ?? new Interval<TDuration>();
                duration = duration ?? new Interval<TDuration>();

                // Допустимы комплекты данных:
                // 1. полнотью определен диапазон начал или окончаний, определена максимальная длительность - второй диапазон определится по первому и длительности
                // 2. заданы правый и левый горизонты, длительность любая

                if (start.Left.IsUndefined && finish.Right.IsUndefined)
                {
                    throw new ArgumentException($"не определен ни один горизонт (start.Left.IsUndefined && finish.Right.IsUndefined): start - {start}, finsh - {finish}");
                }

                if (start.Left > finish.Right)
                {
                    throw new ArgumentException($"[минимальное начало] правее [максимального окончания] (start.Left > finish.Right): {start.Left} > {finish.Right}");
                }

                duration = PreProcessDuration(duration, totalDuration);

                var maxDurationDefined = !duration.Right.IsUndefined;

                var leftHorizontIsDefined = !start.Left.IsUndefined;
                var rightHorizontIsDefined = !finish.Right.IsUndefined;

                var byHorizonts = leftHorizontIsDefined && rightHorizontIsDefined;
                var byDefinedIntervalAndMaxDuration = maxDurationDefined && (start.IsDefined || finish.IsDefined);

                if (!byHorizonts && !byDefinedIntervalAndMaxDuration)
                {
                    var message =
                        @"Допустимы комплекты данных:
1. полнотью определен диапазон начал или окончаний, определена максимальная длительность - второй диапазон определится по первому и длительности.
2. заданы правый и левый горизонты, длительность любая.
";
                    throw new ArgumentException(message);
                }

                /*
                Обозначения.
                   - длительность:                         
						Ø═══□◄═ D ═►□ 
                           D.L     D.R
                   - диапазон начал / окончаний:
						□◄═══ X ═══►□
					   X.L         X.R
                   - "≡" какая-то часть, не играет роли 
                       например
                       □◄═ X ≡ 
                       значит, что нас интересует леваая часть, а правая роли не играет
                   - "□═>■" - приемлимы сдвиг. ■ - новое, приемлимое, значение
                   - "□─X●" - неприемлимый сдвиг. ● - новое, неприемлимое, значение
                   - "E" - минимальная допустимая длительность

               Корректировка длительности:

                  Мнимальная длительность (D.L):

                  ⁞◄ E ►⁞                ⁞◄ E ►⁞
                  Ø═════●X─□◄═ D ≡       Ø══□═>■◄─ D ≡
                           D.L              D.L

                  D.L = Max(D.L, E)

                  Максимальная длительность

                  □◄═ S ≡ ≡ F ═►□               □◄═ S ≡ ≡ F ═►□
                  ↓             ↓               ↓             ↓
                  Ø════ ≡ D ────■<══□ D.R       Ø═ ≡ D ═►□───X● D.R
                  ⁞             ↓   ⁞           ⁞        ↓    ⁞
                 ═╪═════════════╪═══╪════      ═╪════════╪════╪════

                  D.R = Min(D.L, Duration(F.R, S.L))

               Диапазоны начал и окончаний:
                 Первым корректируется диапазон, для которого выплнено требование. 
                 Если для обоих выполнено - порядок без разницы.
                 
                Диапазон начал (если полностью определен диапазон окончаний)

							  □◄═════ F ═════►□
							  ↓               ↓
				   Ø═ ≡ D.R ═►□      Ø════════□◄═ D.L ≡
				   ↓          ⁞      ↓        ⁞  
				   ●X──□◄═ S ───────►■<══□    ⁞
				   ⁞   ↓      ⁞      ↓   ⁞    ⁞
				  ═╪═══╪══════╪══════╪═══╪════╪════════				 
				   ⁞   ↓     F.L     ↓   ⁞   F.R    
				   ●X─S.L            ■<═S.R

				   S.L = Max(S.L, F.L.OffsetLeft(D.R))	
				   S.R = Min(S.R, F.R.OffsetLeft(D.L))	

               Диапазон окончаний  (если полностью определен диапазон начал)

                   □◄═════ S ═════►□
                   ↓               ↓      
                   ⁞               Ø═ ≡ D.R ═►□
                   ↓               ⁞          ↓
                   Ø═══════□◄═ D.L ≡          ⁞
                   ⁞       ↓       ⁞          ↓          
                   ⁞   □══>■◄── F ═════►□────X●    
                   ⁞   ⁞   ↓       ⁞    ↓     ⁞
                  ═╪═══╪═══╪═══════╪════╪═════╪═
                  S.L  ⁞   ⁞      S.R   ⁞     ⁞
                      F.L═>■           F.R───X●


                   F.L = Max(F.L, S.L.OffsetRight(D.L))	
                   F.R = Min(F.R, S.R.OffsetRight(D.R))

               */

                Interval<TTime> s = start;
                Interval<TTime> f = finish;
                Interval<TDuration> d = duration;

                if (byHorizonts)
                {
                    s = StartByFinish(s, f, d);
                    f = FinishByStart(s, f, d);
                    d = DurationByHorizonts(s, f, d);
                }
                else
                {
                    d = DurationByHorizonts(s, f, d);

                    if (start.IsDefined)
                    {
                        f = FinishByStart(s, f, d);
                        s = StartByFinish(s, f, d);
                    }
                    else
                    {
                        s = StartByFinish(s, f, d);
                        f = FinishByStart(s, f, d);
                    }

                    d = DurationByHorizonts(s, f, d);
                }

                var restrictions = new Restrictions(s, f, totalDuration, d, amount);

                var iterator = new Iterator(_schedule, restrictions, kind, _requestInstructions, _requestAmount, _requestDurationByAmount, AddAmount, SubstAmount);
                iterator.Init();
                iterator.Process();
            }

            private Interval<TDuration> PreProcessDuration(Interval<TDuration> duration, Interval<TDuration> totalDuration)
            {
                var left = Point<TDuration>.Left(_schedule.MinDuration);
                left = Point<TDuration>.Max(duration.Left, left);

                var right = Point<TDuration>.Min(duration.Right, totalDuration.Right);
                duration = new Interval<TDuration>(left, right);

                return duration;
            }

            private Interval<TTime> StartByFinish(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> duration)
            {
                var left = Calendars<TTime, TDuration>.OffsetPointToLeft(finish.Left, duration.Right.PointValue.Value);
                left = Point<TTime>.Max(start.Left, left);

                var right = Calendars<TTime, TDuration>.OffsetPointToLeft(finish.Right, duration.Left.PointValue.Value);
                right = Point<TTime>.Min(start.Right, right);

                return new Interval<TTime>(left, right);
            }

            private Interval<TTime> FinishByStart(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> duration)
            {
                var left = Calendars<TTime, TDuration>.OffsetPointToRight(start.Left, duration.Left.PointValue.Value);
                left = Point<TTime>.Max(finish.Left, left);

                var right = Calendars<TTime, TDuration>.OffsetPointToRight(start.Right, duration.Right.PointValue.Value);
                right = Point<TTime>.Min(finish.Right, right);

                return new Interval<TTime>(left, right);
            }

            private Interval<TDuration> DurationByHorizonts(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> duration)
            {
                var left = Point<TDuration>.Left(_schedule.MinDuration);
                left = Point<TDuration>.Max(duration.Left, left);
                var right = duration.Right;

                if (!start.Left.IsUndefined && !finish.Right.IsUndefined)
                {
                    right = Point<TDuration>.Right(ToDuration(start.Left, finish.Right), start.Left.Included && finish.Right.Included);
                    right = Point<TDuration>.Min(right, duration.Right);
                }

                duration = new Interval<TDuration>(left, right);
                return duration;
            }

            struct Restrictions
            {
                public readonly Interval<TTime> Start;
                public readonly Interval<TTime> Finish;
                public readonly Interval<TDuration> TotalDuration;
                public readonly Interval<TDuration> Duration;
                public readonly TAmount RequiredAmount;

                public Restrictions(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> totalDuration, Interval<TDuration> duration, TAmount requiredAmount)
                {
                    Start = start;
                    Finish = finish;
                    TotalDuration = totalDuration;
                    Duration = duration;
                    RequiredAmount = requiredAmount;
                }
            }

            class Iterator
            {
                class IteratorIntervalData
                {
                    public readonly TData OriginalData;
                    public readonly IteratorInstruction Instruction;
                    public readonly DataAdapter<TAmount> MinDurationAmount;
                    public readonly DataAdapter<TAmount> Amount;

                    public IteratorIntervalData(TData originalData, IteratorInstruction instruction, TAmount amount = default(TAmount), TAmount minDurationAmount = default(TAmount))
                    {
                        OriginalData = originalData;
                        Instruction = instruction;
                        Amount = amount;
                        MinDurationAmount = minDurationAmount;
                    }
                }
                class IteratorInterval : PointedInterval<TTime, IteratorIntervalData>
                {
                    protected internal IteratorInterval(Interval<TTime> interval, IteratorIntervalData data = null)
                        : this(interval.Left, interval.Right, data ?? new IteratorIntervalData(default(TData), default(IteratorInstruction)))
                    {

                    }

                    protected internal IteratorInterval(TTime? left, bool leftIncluded, TTime? right, bool rightIncluded, IteratorIntervalData data)
                        : base(left, leftIncluded, right, rightIncluded, data)
                    {
                    }

                    protected internal IteratorInterval(Point<TTime> left = null, Point<TTime> right = null, IteratorIntervalData data = null)
                        : base(left, right, data)
                    {
                    }

                    public new IteratorInterval Intersect(Interval<TTime> interval)
                    {
                        var intersect = base.Intersect(interval);
                        return intersect == null ? null : new IteratorInterval(intersect.Left, intersect.Right, Data);
                    }

                    public static Interval<TTime> StartFinishInterval(IteratorInterval start, IteratorInterval finish, SchedulerKind kind)
                    {
                        if (kind == SchedulerKind.LeftToRight)
                        {
                            return new Interval<TTime>(start.Right.AsLeft(), finish.Right);
                        }
                        else
                        {
                            return new Interval<TTime>(finish.Left, start.Left.AsRight());
                        }
                    }
                }

                private ISchedule _schedule;
                private readonly Restrictions _restrictions;
                private readonly SchedulerKind _kind;

                //public INode<ICalendarItem> Current => _nodes.LastOrDefault();

                //public Point<TTime> Start => _getStartPoint();
                //public Point<TTime> Finish => _getFinishPoint();

                //public TDuration? TotalDuration => Calendars<TTime, TDuration>.ToDuration(Finish, Start);
                //public TDuration? Duration => Calendars<TTime, TDuration>.ToDuration(Finish, Start) ?? default(TDuration);

                public TAmount Amount;

                private Func<INode<IteratorInterval>, INode<IteratorInterval>> _getNextNode;
                private Func<INode<IteratorInterval>, INode<IteratorInterval>> _getPrevNode;

                private RequestInstructionsDelegate _requestInstructions;
                private RequestAmountDelegate _requestAmount;
                private readonly RequestDurationByAmountDelegate _requestDurationByAmount;
                private readonly Func<TAmount, TAmount, TAmount> _add;
                private readonly Func<TAmount, TAmount, TAmount> _subst;


                //private Interval<TTime> _max;
                private Interval<TTime> _startArea;
                private Interval<TTime> _finishArea;
                private Func<Point<TTime>, TDuration, Point<TTime>> _offsetNext;
                private Func<Point<TTime>, TDuration, Point<TTime>> _offsetPrev;


                public Iterator(ISchedule schedule, Restrictions restrictions, SchedulerKind kind, RequestInstructionsDelegate requestInstructions, RequestAmountDelegate requestAmount, RequestDurationByAmountDelegate requestDurationByAmount, Func<TAmount, TAmount, TAmount> add, Func<TAmount, TAmount, TAmount> subst)
                {
                    _schedule = schedule;
                    _restrictions = restrictions;
                    _kind = kind;
                    _requestInstructions = requestInstructions;
                    _requestAmount = requestAmount;
                    _requestDurationByAmount = requestDurationByAmount;
                    _add = add;
                    _subst = subst;


                    Amount = default(TAmount);

                    if (_kind == SchedulerKind.LeftToRight)
                    {
                        _getNextNode = node => node?.Next;
                        _getPrevNode = node => node?.Previous;
                        _startArea = _restrictions.Start;
                        _finishArea = _restrictions.Finish;
                        _offsetNext = Calendars<TTime, TDuration>.OffsetPointToLeft;
                        _offsetPrev = Calendars<TTime, TDuration>.OffsetPointToLeft;
                    }
                    else
                    {
                        _getNextNode = node => node?.Previous;
                        _getPrevNode = node => node?.Next;
                        _startArea = _restrictions.Finish;
                        _finishArea = _restrictions.Start;
                        _offsetPrev = Calendars<TTime, TDuration>.OffsetPointToLeft;
                        _offsetNext = Calendars<TTime, TDuration>.OffsetPointToLeft;
                    }
                }

                class DurationInterval : Interval<TDuration>
                {
                    public DataAdapter<TDuration> Duration => Right.PointValue.Value;
                    public TDuration Value => Duration.Value;

                    public DurationInterval() : base(Point<TDuration>.Left(default(TDuration)), Point<TDuration>.Right(default(TDuration)))
                    {
                    }

                    internal DurationInterval(TDuration duration) : base(Point<TDuration>.Left(default(TDuration)), Point<TDuration>.Right(duration))
                    {
                    }

                    internal DurationInterval(DataAdapter<TDuration> duration) : this(duration.Value)
                    {
                    }

                    public static DurationInterval operator +(DurationInterval interval, DataAdapter<TDuration> duration)
                    {
                        return new DurationInterval(interval.Duration + duration);
                    }
                    public static DurationInterval operator -(DurationInterval interval, DataAdapter<TDuration> duration)
                    {
                        return new DurationInterval(interval.Duration - duration);
                    }
                }

                public void Init()
                {
                    //Func<ICalendarItem, Point<TTime>, TAmount, AmountedInterval> f;
                    //if (_kind == SchedulerKind.LeftToRight)
                    //    f = (ci, p, a) => new AmountedInterval(p, ci.Right, a);
                    //else
                    //    f = (ci, p, a) => new AmountedInterval(ci.Left, p, a);


                    var container = new PointedIntervalsContainer<IteratorInterval, TTime, IteratorIntervalData>
                        (
                        (left, right, dt) => new IteratorInterval(left, right, dt),
                        (a, b) => b,
                        (a, b) => null
                        );


                    var items = _schedule.Available.Get(_restrictions.Start.Left, _restrictions.Finish.Right);
                    foreach (var item in items)
                    {
                        var instructions = RequestInstructions(item);

                        var minDurationAmount = instructions == IteratorInstruction.Accept
                            ? _requestAmount(MinDuration, item.Data, _restrictions.RequiredAmount)
                            : default(TAmount);

                        var intervalAmount = instructions == IteratorInstruction.Accept
                            ? _requestAmount(item.Duration.Value, item.Data, _restrictions.RequiredAmount)
                            : default(TAmount);

                        container.Include
                            (
                                item.Left,
                                item.Right,
                                new IteratorIntervalData(item.Data, instructions, intervalAmount, minDurationAmount)
                            );
                    }


                    var getPoint = _kind == SchedulerKind.LeftToRight
                            ? (Func<Interval<TTime>, Point<TTime>>)(i => i.Left)
                            : (i => i.Right);


                    var point = getPoint(_startArea);


                    var i1 = container.Get(_startArea);
                    var i2 = container.Get(_finishArea);

                    var nextpoint = _offsetNext(point, _restrictions.Duration.Left.PointValue.Value);


                    var finishhNode = container.FindNode(x => x.Contains(point));
                    var startNode = _getPrevNode(finishhNode);

                    DataAdapter<TAmount> amount = default(TAmount);
                    var duration = new DurationInterval();
                    var totalDuration = new DurationInterval();

                    IteratorInterval finishIntersection = null;
                    var skipped = false;
                    var accepted = false;

                    while (finishhNode.Value.Data != null)
                    {

                        DataAdapter<TDuration> nodeDuration = ToDuration(finishhNode.Value);

                        var intervalData = finishhNode.Value.Data;
                        var data = intervalData.OriginalData;
                        var instructions = intervalData.Instruction;

                        switch (instructions)
                        {
                            case IteratorInstruction.Skip:
                                if (skipped)
                                    ObstructByRejected(ref startNode, ref finishhNode, container);
                                else
                                    totalDuration += nodeDuration;

                                skipped = true;
                                break;
                            case IteratorInstruction.Reject:
                                accepted = false;
                                skipped = true;

                                ObstructByRejected(ref startNode, ref finishhNode, container);

                                amount = default(TAmount);
                                duration = new DurationInterval();
                                totalDuration = new DurationInterval();
                                break;
                            case IteratorInstruction.Accept:
                                var nodeAmount = intervalData.Amount;

                                var d = duration + nodeDuration;
                                var td = totalDuration + nodeDuration;
                                var a = amount + nodeAmount;


                                // длительности недобор до минимума
                                var lower1 = d.Right < _restrictions.Duration.Left;
                                var lower2 = td.Right < _restrictions.TotalDuration.Left;

                                // длительности в рамках требований
                                var in1 = _restrictions.Duration.Contains(d.Right);
                                var in2 = _restrictions.TotalDuration.Contains(td.Right);

                                // недобор объёма
                                var lowerA = a <= _restrictions.RequiredAmount;

                                // берем всё, если:
                                if (
                                    // два недобора
                                    lower1 && lower2
                                    // один недобор и один в рамках
                                    || (lower1 && in2) || (in1 && lower2)
                                    // два в рамках и amount не больше требуемого
                                    || (in1 && in2 && lowerA)
                                    )
                                {
                                    duration = d;
                                    totalDuration = td;
                                    amount = a;
                                    break;
                                }




                                // если недобор объёма, 
                                // то надо двигать начало на максимальное отклонение по длительности 
                                // и идти дальше
                                if (lowerA)
                                {
                                    ObstructStartByDurations(ref startNode, ref finishhNode, container, ref d, ref td, ref a);

                                    duration = d;
                                    totalDuration = td;
                                    amount = a;
                                    break;
                                }


                                var minDuratonAmount = intervalData.MinDurationAmount;



                                // учесть минимальный кусок
                                if (!accepted)
                                {

                                    totalDuration += MinDuration;
                                    duration += MinDuration;

                                    amount += minDuratonAmount;

                                    nodeAmount -= minDuratonAmount;
                                    nodeDuration -= MinDuration;
                                }

                                accepted = true;
                                skipped = false;

                                // оценить и, если надо, подтянуть хвост

                                duration += nodeDuration;
                                amount += nodeAmount;

                                totalDuration += nodeDuration;

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        finishhNode = _getNextNode(finishhNode);
                    }

                    var finishNode = finishhNode;

                    var startIntersection = _startArea.Intersect(startNode.Value);

                    var interval = new Interval<TTime>(Point<TTime>.Min(startIntersection.Left, finishIntersection.Left), Point<TTime>.Min(startIntersection.Right, finishIntersection.Right));
                    var dur = ToDuration(interval.Left, interval.Right);

                    //TAmount amount;
                    //INode<ICalendarItem> node = MoveNode(null, _startArea);
                    //if(node == null)
                    //    return;
                }

                private static void Obstruct(ref INode<IteratorInterval> startNode, PointedIntervalsContainer<IteratorInterval, TTime, IteratorIntervalData> container, Interval<TTime> interval)
                {
                    container.Exclude(new IteratorInterval(interval));
                    if (!startNode.Alive)
                    {
                        var point = startNode.Value.Left;
                        startNode = container.FindNode(x => x.Contains(point));
                    }
                }

                private void ObstructByRejected(ref INode<IteratorInterval> startNode, ref INode<IteratorInterval> finishhNode, PointedIntervalsContainer<IteratorInterval, TTime, IteratorIntervalData> container)
                {
                    var startFinishInterval = IteratorInterval.StartFinishInterval(startNode.Value, finishhNode.Value, _kind);
                    Obstruct(ref startNode, container, startFinishInterval);

                    finishhNode = startNode;
                }

                private void ObstructStartByDurations(ref INode<IteratorInterval> startNode, ref INode<IteratorInterval> finishhNode, PointedIntervalsContainer<IteratorInterval, TTime, IteratorIntervalData> container, ref DurationInterval duration, ref DurationInterval totalDuration, ref DataAdapter<TAmount> amount)
                {
                    var requiredAmount = _restrictions.RequiredAmount;
                    var zero = default(TDuration);

                    // превышения длительности
                    var delta = duration.Duration - (_restrictions.Duration.Right.PointValue ?? duration.Duration);
                    var totalDelta = totalDuration.Duration - (_restrictions.TotalDuration.Right.PointValue ?? totalDuration.Duration);


                    while (delta > zero || totalDelta > zero)
                    {
                        var currentNode = _getNextNode(startNode);

                        DataAdapter<TDuration> nodeDuration = ToDuration(currentNode.Value);
                        var interval = finishhNode.Value;
                        var intervalData = interval.Data;
                        var data = intervalData.OriginalData;


                        DataAdapter<TAmount> nodeAmount = _requestAmount(nodeDuration.Value, data, requiredAmount);

                        switch (intervalData.Instruction)
                        {
                            case IteratorInstruction.Skip:
                                totalDelta -= nodeDuration;
                                totalDuration = new DurationInterval(totalDuration.Duration - nodeDuration);
                                Obstruct(ref startNode, container, interval);
                                break;
                            case IteratorInstruction.Reject:
                                throw new NotSupportedException("не должно такого быть, от слова 'никогда'");
                            case IteratorInstruction.Accept:

                                // если длительность не меньше, чем какая-нить из дельт...
                                var takeFullNodeDuration = nodeDuration <= delta || nodeDuration <= totalDelta;

                                // ... то ...
                                var obstructDuration = takeFullNodeDuration
                                    ? nodeDuration // ... режем целиком
                                    : delta.Max(totalDelta); // ... иначе будем резать по максимальной дельте

                                // если режем не целиком
                                if (!takeFullNodeDuration)
                                {
                                    // и остаток длительносит меньше минимальной длительности
                                    if (nodeDuration - obstructDuration < MinDuration)
                                    {
                                        // то режем целиком
                                        obstructDuration = nodeDuration;
                                        takeFullNodeDuration = true;
                                    }
                                }
                                var obstrucAmount = takeFullNodeDuration
                                    ? nodeAmount // режем целиком
                                    : _requestAmount(obstructDuration.Value, data, requiredAmount); // иначе будем резать по максимальной дельте

                                delta -= obstructDuration;
                                totalDelta -= obstructDuration;

                                amount -= obstrucAmount;
                                duration = new DurationInterval(duration.Duration - obstructDuration);
                                totalDuration = new DurationInterval(totalDuration.Duration - obstructDuration);

                                if (takeFullNodeDuration)
                                {
                                    Obstruct(ref startNode, container, interval);
                                }
                                else
                                {
                                    var right = interval.Left.AsRight();
                                    right = Calendars<TTime, TDuration>.OffsetPointToRight(right, obstructDuration.Value);
                                    Obstruct(ref startNode, container, new Interval<TTime>(interval.Left, right));
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                    }
                }

                private TDuration MinDuration
                {
                    get { return _schedule.MinDuration; }
                }

                //INode<ICalendarItem> MoveNode(INode<ICalendarItem> node, Interval<TTime> interval)
                //{
                //    if (node == null)
                //    {
                //        Point<TTime> point = null;
                //        node = _schedule.Available.Find(point);
                //    }
                //    else
                //    {
                //        node = _getNextNode(node);
                //    }

                //    if (node == null)
                //        return null;

                //    var intersection = node.Value.Intersect(interval);
                //    // проверим выход за пределы
                //    if (intersection == null)
                //        return null; // всё, отпрыгались...

                //    var duration = ToDuration(intersection.Left, intersection.Right);
                //    var instruction = RequestInstructions(duration, node.Value.OriginalData);

                //    if (instruction == IteratorInstruction.Accept)
                //        return node;

                //    if (instruction == IteratorInstruction.Reject)
                //    {
                //        // reset
                //    }

                //    return MoveNode(node, interval);
                //}

                //void AAA()
                //{
                //    INode<ICalendarItem> node = null;
                //    // пока не набрали сколько нужно
                //    while (Amount?.CompareTo(_restrictions.RequiredAmount) < 0)
                //    {
                //        // если выскочили за ограничения
                //        if(!MeetTheConstraints())
                //        {
                //            // сброс и выход
                //            Reset();
                //            return;
                //        }

                //        //берем следующую ноду в обработку
                //        node = _getNextNode(node);

                //        //запрос наружу на объем
                //        TAmount nodeAmount;
                //        var instruction = RequestInstructions(node.Value, out nodeAmount);
                //        switch (instruction)
                //        {
                //            case IteratorInstruction.Skip:
                //                break;
                //            case IteratorInstruction.Reject:
                //                Reset();
                //                break;
                //            case IteratorInstruction.Accept:
                //                Amount = Add(Amount, nodeAmount);
                //                _nodes.Add(node);
                //                break;
                //            default:
                //                throw new ArgumentOutOfRangeException();
                //        }
                //    }

                //    bool result = Amount?.CompareTo(_restrictions.RequiredAmount) >= 0;
                //}

                //private void Reset()
                //{
                //    _nodes.Clear();
                //    Amount = default(TAmount);
                //}

                private IteratorInstruction RequestInstructions(ICalendarItem item)
                {
                    var left = item.Left;
                    var right = Point<TTime>.Min(item.Right, _restrictions.Start.Right);
                    var duration = ToDuration(left, right);
                    var data = item.Data;

                    return RequestInstructions(data);
                }


                private IteratorInstruction RequestInstructions(TData data)
                {
                    return _requestInstructions(data, _restrictions.RequiredAmount);
                }

                private bool? Check(ICalendarItem calendarItem, TAmount requiredAmount)
                {
                    return null;
                }

                void Initialize()
                {
                }


                //public bool MoveNext()
                //{
                //    var node = _schedule.Available.Find(Start);

                //    if (Current == null)
                //    {
                //        node = node ?? _schedule.Available.Find(Start);

                //        while (node != null)
                //        {
                //            var checkResult = Check(node.Value, _restrictions.RequiredAmount);
                //            if (checkResult == null)
                //            {
                //                node = _getNextNode(node);
                //                continue;
                //            }

                //            var nodeData = node.Value.OriginalData;
                //            var nodeDuration = ToDuration(node.Value.Left, node.Value.Right);
                //            var nodeAmount = Amount(nodeDuration, nodeData, _restrictions.RequiredAmount);


                //            Point<TTime>[] points;
                //            bool splitted = node.Value.TrySplit(Start, out points);


                //            if (splitted && points.Length == 4)
                //            {
                //                var leftDur = ToDuration(points[0], points[1]);
                //                nodeAmount = Subst(nodeAmount, Amount(leftDur, nodeData, _restrictions.RequiredAmount));

                //                var rightDur = ToDuration(points[2], points[3]);
                //                nodeAmount = Add(nodeAmount, Amount(rightDur, nodeData, _restrictions.RequiredAmount));
                //            }
                //        }

                //        if (node == null)
                //            return false;
                //    }

                //    return false;
                //}
                public void Process()
                {
                }
            }


            //void Tmp(Point<TTime> start, Point<TTime> finish, Point<TTime> point, Restrictions restrictions, SchedulerKind kind)
            //{
            //    TDuration allocatedDuration;

            //    var node = _schedule.Available.Find(point);

            //    //var checkResult = Check(node.Value, restrictions.RequiredAmount);

            //    var nodeData = node.Value.Data;
            //    var nodeDuration = ToDuration(node.Value.Left, node.Value.Right);
            //    var nodeAmount = Amount(nodeDuration, nodeData, restrictions.RequiredAmount);

            //    if (kind == SchedulerKind.LeftToRight)
            //    {
            //        Point<TTime>[] points;
            //        bool splitted = node.Value.TrySplit(point, out points);


            //        if (splitted && points.Length == 4)
            //        {
            //            var leftDur = ToDuration(points[0], points[1]);
            //            nodeAmount = Subst(nodeAmount, Amount(leftDur, nodeData, restrictions.RequiredAmount));

            //            var rightDur = ToDuration(points[2], points[3]);
            //            nodeAmount = Add(nodeAmount, Amount(rightDur, nodeData, restrictions.RequiredAmount));
            //        }
            //    }

            //    //if (nodeAm)
            //}

            public delegate IteratorInstruction RequestInstructionsDelegate(TData data, TAmount requiredAmount);

            public delegate TAmount RequestAmountDelegate(TDuration duration, TData data, TAmount requiredAmount);

            public delegate TDuration RequestDurationByAmountDelegate(TDuration fullDuration, TAmount fullAmount, TAmount amount);
        }
    }

    public enum SchedulerKind
    {
        LeftToRight,
        RightToLeft
    }
}