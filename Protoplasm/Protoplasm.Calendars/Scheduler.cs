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
                class IteratorInterval : PointedInterval<TTime, Tuple<TData, IteratorInstructions>>
                {
                    protected internal IteratorInterval(TTime? left, bool leftIncluded, TTime? right, bool rightIncluded, Tuple<TData, IteratorInstructions> data) : base(left, leftIncluded, right, rightIncluded, data)
                    {
                    }

                    protected internal IteratorInterval(Point<TTime> left = null, Point<TTime> right = null, Tuple<TData, IteratorInstructions> data = null) : base(left, right, data)
                    {
                    }

                    public new IteratorInterval Intersect(Interval<TTime> interval)
                    {
                        var intersect = base.Intersect(interval);
                        return intersect == null ? null : new IteratorInterval(intersect.Left, intersect.Right, Data);
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

                public void Init()
                {
                    //Func<ICalendarItem, Point<TTime>, TAmount, AmountedInterval> f;
                    //if (_kind == SchedulerKind.LeftToRight)
                    //    f = (ci, p, a) => new AmountedInterval(p, ci.Right, a);
                    //else
                    //    f = (ci, p, a) => new AmountedInterval(ci.Left, p, a);


                    var tuples = new PointedIntervalsContainer<IteratorInterval, TTime, Tuple<TData, IteratorInstructions>>
                        (
                        (left, right, dt) => new IteratorInterval(left, right, dt), 
                        (tuple, tuple1) => tuple1,
                        (tuple, tuple1) => null
                        );

                    var items = _schedule.Available.Get(_restrictions.Start.Left, _restrictions.Finish.Right);
                    foreach (var item in items)
                    {
                        var instructions = RequestInstructions(item);
                        if(instructions == IteratorInstructions.Skip)
                            continue;

                        tuples.Include(item.Left, item.Right, new Tuple<TData, IteratorInstructions>(item.Data, instructions));
                    }


                    var getPoint = _kind == SchedulerKind.LeftToRight
                            ? (Func<Interval<TTime>, Point<TTime>>) (i => i.Left)
                            : (i => i.Right);


                    var point = getPoint(_startArea);
                    

                    var i1 = tuples.Get(_startArea);
                    var i2 = tuples.Get(_finishArea);

                    var nextpoint = _offsetNext(point, _restrictions.Duration.Left.PointValue.Value);


                    var node = tuples.FindNode(x => x.Contains(point));
                    var startNode = _getPrevNode(node);

                    DataAdapter<TAmount> amount = default(TAmount);
                    DataAdapter<TAmount> rest = _restrictions.RequiredAmount;
                    DataAdapter<TDuration> duration = default(TDuration);
                    DataAdapter<TDuration> totalDuration = default(TDuration);

                    IteratorInterval finishIntersection = null;

                    while ((finishIntersection = node.Value.Intersect(_finishArea)) == null)
                    {

                        DataAdapter<TDuration> nodeDuration = ToDuration(node.Value);

                        if (Equals(node.Value.Data, default(TData)))
                        {
                            totalDuration += nodeDuration;
                            continue;
                        }

                        var data = node.Value.Data.Item1;
                        var instructions = node.Value.Data.Item2;

                        switch (instructions)
                        {
                            case IteratorInstructions.Reject:
                                startNode = _getPrevNode(node);
                                amount = default(TAmount);
                                rest = _restrictions.RequiredAmount;
                                totalDuration = default(TDuration);
                                break;
                            case IteratorInstructions.Accept:
                                DataAdapter<TAmount> nodeAmount = _requestAmount(nodeDuration.Value, data, _restrictions.RequiredAmount);
                                
                                // учесть минимальный кусок
                                var minDuratonAmount = _requestAmount(MinDuration, data, _restrictions.RequiredAmount);

                                totalDuration += MinDuration;
                                duration += MinDuration;

                                amount += minDuratonAmount;
                                rest -= minDuratonAmount;

                                nodeAmount -= minDuratonAmount; 
                                nodeDuration -= MinDuration;
                                
                                // оценить и, если надо, подтянуть хвост

                                duration += nodeDuration;
                                amount += nodeAmount;
                                rest -= nodeAmount;

                                var restDuration = _requestDurationByAmount(duration.Value, amount.Value, rest.Value);

                                totalDuration += nodeDuration;


                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        node = _getNextNode(node);
                    }

                    var finishNode = node;

                    var startIntersection = _startArea.Intersect(startNode.Value);

                    var interval = new Interval<TTime>(Point<TTime>.Min(startIntersection.Left, finishIntersection.Left), Point<TTime>.Min(startIntersection.Right, finishIntersection.Right));
                    var dur = ToDuration(interval.Left, interval.Right);

                    //TAmount amount;
                    //INode<ICalendarItem> node = MoveNode(null, _startArea);
                    //if(node == null)
                    //    return;
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
                //    var instruction = RequestInstructions(duration, node.Value.Data);

                //    if (instruction == IteratorInstructions.Accept)
                //        return node;

                //    if (instruction == IteratorInstructions.Reject)
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
                //            case IteratorInstructions.Skip:
                //                break;
                //            case IteratorInstructions.Reject:
                //                Reset();
                //                break;
                //            case IteratorInstructions.Accept:
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

                private IteratorInstructions RequestInstructions(ICalendarItem item)
                {
                    var left = item.Left;
                    var right = Point<TTime>.Min(item.Right, _restrictions.Start.Right);
                    var duration = ToDuration(left, right);
                    var data = item.Data;

                    return RequestInstructions(data);
                }


                private IteratorInstructions RequestInstructions(TData data)
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

                //            var nodeData = node.Value.Data;
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

            public delegate IteratorInstructions RequestInstructionsDelegate(TData data, TAmount requiredAmount);

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