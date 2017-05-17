#define CONS

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public partial class Scheduler<TAmount>
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
                var duration = Calendars<TTime, TDuration>.Duration(left, right);
                if (!duration.HasValue)
                    throw new NotSupportedException("не удалось вычислить длительность");
                return duration.Value;
            }

            //public static TAmount Amount(TDuration duration, TData data, TAmount RequiredAmount)
            //{
            //    return CalculateAmount(duration, data, RequiredAmount);
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

                var iterator = new Iterator(_schedule, restrictions, kind, _requestInstructions, _requestAmount, _requestDurationByAmount);
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

            internal struct Restrictions
            {
                public readonly Interval<TTime> Start;
                public readonly Interval<TTime> Finish;
                public readonly Interval<TDuration> TotalDuration;
                public readonly Interval<TDuration> Duration;
                public readonly TAmount RequiredAmount;

                public Point<TTime> MinStart => Start.Left;
                public Point<TTime> MaxStart => Start.Right;

                public Point<TTime> MinFinish => Finish.Left;
                public Point<TTime> MaxFinish => Finish.Right;


                public Point<TDuration> MinDuration => Duration.Left;
                public Point<TDuration> MaxDuration => Duration.Right;

                public Point<TDuration> MinTotalDuration => TotalDuration.Left;
                public Point<TDuration> MaxTotalDuration => TotalDuration.Right;


                public Restrictions(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> totalDuration, Interval<TDuration> duration, TAmount requiredAmount)
                {
                    Start = start;
                    Finish = finish;
                    TotalDuration = totalDuration;
                    Duration = duration;
                    RequiredAmount = requiredAmount;
                }
            }

            internal partial class Iterator
            {
                class IteratorIntervalData
                {
                    public override string ToString()
                    {
                        return Instruction == IteratorInstruction.Accept
                            ? $"{Instruction}, Data:[{OriginalData}], MinDurationAmount:[{MinDurationAmount}]"
                            : $"{Instruction}";
                    }

                    public readonly TData OriginalData;
                    public readonly IteratorInstruction Instruction;
                    public readonly DataAdapter<TAmount> MinDurationAmount;

                    public IteratorIntervalData(TData originalData, IteratorInstruction instruction, TAmount minDurationAmount = default(TAmount))
                    {
                        OriginalData = originalData;
                        Instruction = instruction;
                        MinDurationAmount = minDurationAmount;
                    }
                }

                class IteratorInterval : PointedInterval<TTime, IteratorIntervalData>
                {
                    private TDuration? _duration;
                    public TDuration Duration => _duration ?? (_duration = ToDuration(Left, Right)).Value;
                    protected override string CustomToStringBeforeData()
                    {
                        return Left.IsUndefined || Right.IsUndefined
                            ? null
                            : $", Duration: {Duration}";
                    }

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
                    public TDuration Duration => _duration ?? (_duration = ToDuration(Interval)).Value;

                    public TAmount Amount => _requestAmount(Duration, OriginalData, _restrictions.RequiredAmount);
                    public TData OriginalData => IntervalData.OriginalData;

                    public TDuration DurationByAmount(TAmount amount)
                    {
                        return _requestDurationByAmount(Duration, Amount, amount);
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
                }


                private ISchedule _schedule;
                private readonly Restrictions _restrictions;
                private readonly SchedulerKind _kind;


                private Func<INode<IteratorInterval>, INode<IteratorInterval>> _getNextNode;
                private Func<INode<IteratorInterval>, INode<IteratorInterval>> _getPrevNode;

                private readonly RequestInstructionsDelegate _requestInstructions;
                private readonly RequestAmountDelegate _requestAmount;
                private readonly RequestDurationByAmountDelegate _requestDurationByAmount;
                //private INode<IteratorInterval> _finishNode;

                //                private INode<IteratorInterval> _rootNode;
                private NodeAdapter Root;
                private DataAdapter<TAmount> _amount;
                private DurationInterval _duration;
                private DurationInterval _totalDuration;
                private PointedIntervalsContainer<IteratorInterval, TTime, IteratorIntervalData> _container;

                public TAmount RequiredAmount => _restrictions.RequiredAmount;
                private TDuration ScheduleMinDuration => _schedule.MinDuration;
                private DataAdapter<TDuration> MaxDuration => _restrictions.MaxDuration.PointValue.Value;
                private DataAdapter<TDuration> MinDuration => _restrictions.MinDuration.PointValue.Value;


                //INode<IteratorInterval> _firstNode => _getNextNode(_rootNode);

                private NodeAdapter _first;
                private NodeAdapter First => _first ?? (_first = GetNodeAdapter(_getNextNode(Root.Node), x => _getNextNode(Root.Node)));

                private NodeAdapter _current;
                private NodeAdapter Current
                {
                    get { return _current ?? (_current = GetNodeAdapter(_getNextNode(Root.Node), x => _getNextNode(Root.Node))); }
                    set { _current = value; }
                }

                public Iterator(ISchedule schedule, Restrictions restrictions, SchedulerKind kind, RequestInstructionsDelegate requestInstructions, RequestAmountDelegate requestAmount,
                    RequestDurationByAmountDelegate requestDurationByAmount)
                {
                    _schedule = schedule;
                    _restrictions = restrictions;
                    _kind = kind;
                    _requestInstructions = requestInstructions;
                    _requestAmount = requestAmount;
                    _requestDurationByAmount = requestDurationByAmount;
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

                    _amount = default(TAmount);
                    _duration = new DurationInterval();
                    _totalDuration = new DurationInterval();


                    _container = new PointedIntervalsContainer<IteratorInterval, TTime, IteratorIntervalData>
                        (
                        (left, right, dt) => new IteratorInterval(left, right, dt),
                        (a, b) => b,
                        (a, b) => null
                        );


                    var items = _schedule.Available.Get(_restrictions.MinStart, _restrictions.MaxFinish);

                    foreach (var item in items)
                    {
                        var instructions = RequestInstructions(item);

                        var minDurationAmount = instructions == IteratorInstruction.Accept
                            ? _requestAmount(ScheduleMinDuration, item.Data, RequiredAmount)
                            : default(TAmount);

                        _container.Include
                            (
                                item.Left,
                                item.Right,
                                new IteratorIntervalData(item.Data, instructions, minDurationAmount)
                            );
                    }

                    var fn = _container.FindNode(x => x.Contains(point));
                    Root = GetNodeAdapter(_getPrevNode(fn), x => _container.FindNode(y => y.Contains(x.Value.Left)));


                    var skipped = false;
                    var accepted = false;

                    while (Current.OriginalData != null)
                    {


#if CONS
                        Console.WriteLine($"======================================================================");
                        Console.WriteLine($"-> {Current.Interval}");
                        Console.WriteLine($"   amount:{_amount.Value}, duration:{_duration.Right}");
#endif
                        switch (Current.Instruction)
                        {
                            case IteratorInstruction.Skip:
                                if (skipped)
                                    ObstructByRejected();
                                else
                                    _totalDuration += Current.Duration;

#if CONS
                                DataAdapter<TDuration> ddd = _duration.Value;
                                DataAdapter<TAmount> aaa = _amount.Value;
#endif

                                ObstructStartByDurations();
#if CONS
                                if (_duration.Value != ddd || aaa != _amount)
                                {
                                    Console.WriteLine($"  -> обрезали начало по длительностям");
                                    Console.WriteLine($"     результат: amount:{_amount.Value}, duration:{_duration.Right}, totalDuration:{_totalDuration.Right}");
                                }
#endif

                                skipped = true;
                                break;
                            case IteratorInstruction.Reject:
                                accepted = false;
                                skipped = true;

                                ObstructByRejected();

                                break;
                            case IteratorInstruction.Accept:

                                var d = _duration + Current.Duration;
                                var td = _totalDuration + Current.Duration;
                                var a = _amount + Current.Amount;


                                // длительности недобор до минимума
                                var lower1 = d.Right < _restrictions.MinDuration;
                                var lower2 = td.Right < _restrictions.MinTotalDuration;

                                // длительности в рамках требований
                                var in1 = _restrictions.Duration.Contains(d.Right);
                                var in2 = _restrictions.TotalDuration.Contains(td.Right);

                                // недобор объёма
                                var lowerA = a <= RequiredAmount;

                                // берем всё, если:
                                if (
                                    // два недобора
                                    lower1 && lower2
                                    // один недобор и один в рамках
                                    || (lower1 && in2) || (in1 && lower2)
                                    // два в рамках и _amount не больше требуемого
                                    || (in1 && in2 && lowerA)
                                    )
                                {
#if CONS
                                    Console.WriteLine("\tберем всё (1)");
#endif
                                    skipped = false;
                                    accepted = true;

                                    _duration = d;
                                    _totalDuration = td;
                                    _amount = a;
                                    break;
                                }




                                // если недобор объёма, 
                                // то надо двигать начало на максимальное отклонение по длительности 
                                // и идти дальше
                                if (lowerA)
                                {
#if CONS
                                    Console.WriteLine("\tобрезаем начало и берем всё (2)");
#endif
                                    _duration = d;
                                    _totalDuration = td;
                                    _amount = a;

                                    ObstructStartByDurations();

                                    skipped = false;
                                    accepted = true;

#if CONS
                                    Console.WriteLine($"\tрезультат: amount:{_amount.Value}, duration:{_duration.Right}, totalDuration:{_totalDuration.Right}");
#endif
                                    break;
                                }


                                var deltaAmount = RequiredAmount - _amount;
                                DataAdapter<TDuration> deltaDuration = Current.DurationByAmount(deltaAmount.Value);


                                var firstInterval = First.Interval;


#if CONS
                                Console.WriteLine("---------------");
                                if (First.Interval == Current.Interval)
                                {
                                    Console.WriteLine($"    first == current");
                                    Console.WriteLine($"    ---");
                                }
                                Console.WriteLine($"    first: {First.Interval}");
                                Console.WriteLine($"        min: {First.IntervalData.MinDurationAmount}");
                                if (First.Interval != Current.Interval)
                                {
                                    Console.WriteLine($"    current: {Current.Interval}");
                                    Console.WriteLine($"        min: {Current.IntervalData.MinDurationAmount}");

                                }   
                                //Console.WriteLine($"    i: {Current.Interval}");
                                //Console.WriteLine($"        min: {Current.IntervalData.MinDurationAmount}");
                                Console.WriteLine($"    deltaAmount: {deltaAmount}");
                                Console.WriteLine($"    deltaDuration: {deltaDuration}");
                                Console.WriteLine("---------------");

#endif
                                if (firstInterval == Current.Interval)
                                {
#if CONS
                                    Console.WriteLine($"    всё начало брезано, проверяем текущий");
#endif
                                    if (deltaDuration > MaxDuration)
                                    {
#if CONS
                                        Console.WriteLine($"        в текущем не удовлетворимся, режем чтоб остался кусок по максимальной длительности и переходим к следующему");
#endif
                                        var obstractDuration = Current.Duration - MaxDuration;

                                        _totalDuration = new DurationInterval(Current.Duration);
                                        _duration = new DurationInterval(Current.Duration);
                                        _amount = Current.Amount;

                                        ObstructStartByInterval(obstractDuration);

#if CONS
                                        Console.WriteLine($"     результат: amount:{_amount.Value}, duration:{_duration.Right}, totalDuration:{_totalDuration.Right}");
#endif
                                        Current = Current.Next();
                                        break;
                                    }

                                    throw new NotImplementedException();
                                }


                                if (firstInterval.Data.MinDurationAmount > Current.IntervalData.MinDurationAmount)
                                {
                                    var firstDuration = ToDuration(firstInterval);
                                    var obstractDuration = new DataAdapter<TDuration>(Current.Duration).Min(firstDuration);
                                    var obstractAmount = _requestAmount(obstractDuration.Value, firstInterval.Data.OriginalData, RequiredAmount);

#if CONS
                                    Console.WriteLine($"    высота первого куска >= высоты текущего");
                                    Console.WriteLine($"        режем по минимальной длительности ({obstractDuration.Value})");
                                    var checkAmount = _amount - obstractAmount;
#endif

                                    ObstructStartByInterval(obstractDuration);

#if CONS
                                    Console.WriteLine($"     amount:{_amount.Value} ({checkAmount.Value}), duration:{_duration.Right}");
#endif
                                    break;
                                }


                                throw new NotImplementedException();

                                var minDuratonAmount = Current.IntervalData.MinDurationAmount;



                                // учесть минимальный кусок
                                if (!accepted)
                                {

                                    _totalDuration += ScheduleMinDuration;
                                    _duration += ScheduleMinDuration;

                                    _amount += minDuratonAmount;

                                }

                                accepted = true;
                                skipped = false;

                                // оценить и, если надо, подтянуть хвост

                                _duration += Current.Duration;
                                _amount += Current.Amount;

                                _totalDuration += Current.Duration;

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        Current = Current.Next();
                    }
                }


                private void Obstruct(Interval<TTime> interval)
                {
                    _container.Exclude(new IteratorInterval(interval));
                }

                private void ObstructByRejected()
                {
                    var startFinishInterval = IteratorInterval.StartFinishInterval(Root.Interval, Current.Interval, _kind);
                    Obstruct(startFinishInterval);

                    Current.Reset();
                    _amount = default(TAmount);
                    _duration = new DurationInterval();
                    _totalDuration = new DurationInterval();
                }

                private void ObstructStartByDurations()
                {
                    var zero = default(TDuration);

                    // превышения длительности
                    var delta = _duration.Duration - (_restrictions.MaxDuration.PointValue ?? _duration.Duration);
                    var totalDelta = _totalDuration.Duration - (_restrictions.MaxTotalDuration.PointValue ?? _totalDuration.Duration);

                    IteratorInstruction instruction = IteratorInstruction.Accept;

                    while (delta > zero || totalDelta > zero || instruction != IteratorInstruction.Accept)
                    {
                        //var currentNode = _getNextNode(_rootNode);
                        var current = First;
                        DataAdapter<TDuration> currentNodeDuration = current.Duration;

                        // если длительность не меньше, чем какая-нить из дельт...
                        var obstructDuration = currentNodeDuration <= delta || currentNodeDuration <= totalDelta
                            ? currentNodeDuration // ... режем целиком
                            : delta.Max(totalDelta).Value; // ... иначе будем резать по максимальной дельте

                        obstructDuration = ObstructNode(obstructDuration);

                        totalDelta -= obstructDuration;

                        if (instruction == IteratorInstruction.Accept)
                            delta -= obstructDuration;

                        instruction = First.Instruction;
                    }

                }


                private void ObstructStartByInterval(DataAdapter<TDuration> intervalDuration)
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

                private DataAdapter<TDuration> ObstructNode(DataAdapter<TDuration> obstructDuration)
                {
                    var currentNode = First;

                    DataAdapter<TDuration> currentNodeDuration = currentNode.Duration;
                    var currentInterval = currentNode.Interval;
                    var currentIntravelData = currentInterval.Data;
                    var originalData = currentIntravelData.OriginalData;
                    DataAdapter<TAmount> currentNodeAmount = currentNode.Amount;

                    switch (currentNode.Instruction)
                    {
                        case IteratorInstruction.Skip:
                            {
                                _totalDuration = new DurationInterval(_totalDuration.Duration - currentNodeDuration);

                                var left = Root.Interval.Right.AsLeft();
                                var right = currentInterval.Right;
                                var obstructionInterval = new Interval<TTime>(left, right);
                                Obstruct(obstructionInterval);
                                break;
                            }
                        case IteratorInstruction.Reject:
                            throw new NotSupportedException("не должно такого быть, от слова 'никогда'");
                        case IteratorInstruction.Accept:
                            {
                                // если остаток длительносит меньше минимальной длительности
                                if (currentNodeDuration - obstructDuration < ScheduleMinDuration)
                                {
                                    // то режем целиком
                                    obstructDuration = currentNodeDuration;
                                }

                                var obstrucAmount = obstructDuration == currentNodeDuration
                                    ? currentNodeAmount // режем целиком
                                    : _requestAmount(obstructDuration.Value, originalData, RequiredAmount) // иначе будем резать по максимальной дельте
                                    ;

                                _amount -= obstrucAmount;
                                _duration -= obstructDuration;
                                _totalDuration -= obstructDuration;


                                var left = Root.Interval.Right.AsLeft();

                                var right = Root.Interval.Right;
                                right = Calendars<TTime, TDuration>.OffsetPointToRight(right, obstructDuration.Value);
                                var obstructionInterval = new Interval<TTime>(left, right);
                                Obstruct(obstructionInterval);

                                break;
                            }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return obstructDuration;
                }



                private IteratorInstruction RequestInstructions(ICalendarItem item)
                {
                    //var left = item.Left;
                    //var right = Point<TTime>.Min(item.Right, _restrictions.Start.Right);
                    //var _duration = ToDuration(left, right);
                    var data = item.Data;

                    return RequestInstructions(data);
                }


                private IteratorInstruction RequestInstructions(TData data)
                {
                    return _requestInstructions(data, _restrictions.RequiredAmount);
                }


                public void Process()
                {
                }
            }



            public delegate IteratorInstruction RequestInstructionsDelegate(TData data, TAmount requiredAmount);

            public delegate TAmount RequestAmountDelegate(TDuration duration, TData data, TAmount requiredAmount);

            public delegate TDuration RequestDurationByAmountDelegate(TDuration fullDuration, TAmount fullAmount, TAmount amount);

        }
    }
}