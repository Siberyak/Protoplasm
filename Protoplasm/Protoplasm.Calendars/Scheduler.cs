using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Resources;
using Microsoft.SqlServer.Server;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils.Graph;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {

        public class Scheduler<TAmount>
            where TAmount : IComparable<TAmount>
        {
            public static Func<TAmount, TAmount, TAmount> AddAmount;
            public static Func<TAmount, TAmount, TAmount> SubstAmount;
            public static Func<TDuration, TData, TAmount, TAmount> CalculateAmount;

            private RequestInstructionsDelegate _requestInstructions;

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

            public static TAmount Add(TAmount a, TAmount b)
            {
                return AddAmount(a, b);
            }
            public static TAmount Subst(TAmount a, TAmount b)
            {
                return AddAmount(a, b);
            }

            ISchedule _schedule;


            public Scheduler(ISchedule schedule, RequestInstructionsDelegate requestInstructions)
            {
                _schedule = schedule;
                _requestInstructions = requestInstructions;
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



                var point = kind == SchedulerKind.LeftToRight
                    ? s.Left
                    : f.Right;


                var restrictions = new Restrictions(s, f, totalDuration, d, amount);

                var iterator = new Iterator(_schedule, restrictions, kind, _requestInstructions, point);
                iterator.Init();
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
                    right = Point< TDuration>.Min(right, duration.Right);
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
                private ISchedule _schedule;
                private readonly Restrictions _restrictions;
                private readonly SchedulerKind _kind;

                public INode<ICalendarItem> Current => _nodes.LastOrDefault();

                public Point<TTime> Start => _getStartPoint();
                public Point<TTime> Finish => _getFinishPoint();

                public TDuration? TotalDuration => Calendars<TTime, TDuration>.ToDuration(Finish, Start);
                public TDuration? Duration => Calendars<TTime, TDuration>.ToDuration(Finish, Start) ?? default(TDuration);

                public TAmount Amount;

                public readonly List<INode<ICalendarItem>> _nodes = new List<INode<ICalendarItem>>();
                private Func<INode<ICalendarItem>, INode<ICalendarItem>> _getNextNode;
                private Func<INode<ICalendarItem>, INode<ICalendarItem>> _getPrevNode;

                private Point<TTime> _point;

                private Func<Point<TTime>> _getStartPoint;
                private Func<Point<TTime>> _getFinishPoint;

                private Interval<TTime> _max;
                private Interval<TTime> _startArea;
                private Interval<TTime> _finishArea;

                class AmountedInterval : PointedInterval<TTime, TAmount>
                {
                    public AmountedInterval(Point<TTime> left = null, Point<TTime> right = null, TAmount data = default(TAmount)) : base(left, right, data)
                    {
                    }
                }

                private PointedIntervalsContainer<AmountedInterval, TTime, TAmount> _items;
                private RequestInstructionsDelegate _requestInstructions;
                public Iterator(ISchedule schedule, Restrictions restrictions, SchedulerKind kind, RequestInstructionsDelegate requestInstructions, Point<TTime> point = null)
                {


                    point = point
                            ?? (
                                kind == SchedulerKind.LeftToRight
                                    ? restrictions.Start.Left
                                    : restrictions.Finish.Right
                                );

                    if (!restrictions.Start.Contains(point))
                        throw new ArgumentException("ограничения на начало не содержат начальную точку. <- (!restrictions.Start.Contains(point))");


                    _items = new PointedIntervalsContainer<AmountedInterval, TTime, TAmount>((left, right, data) => new AmountedInterval(left, right, data), Add, Subst);
                    _max = new Interval<TTime>(restrictions.Start.Left, restrictions.Finish.Right);


                    _schedule = schedule;
                    _restrictions = restrictions;
                    _kind = kind;
                    _requestInstructions = requestInstructions;
                    _point = point;


                    Amount = default(TAmount);

                    if (_kind == SchedulerKind.LeftToRight)
                    {
                        _getNextNode = node => node?.Next;
                        _getPrevNode = node => node?.Previous;
                        _startArea = _restrictions.Start;
                        _finishArea = _restrictions.Finish;
                    }
                    else
                    {
                        _getNextNode = node => node?.Previous;
                        _getPrevNode = node => node?.Next;
                        _startArea = _restrictions.Finish;
                        _finishArea = _restrictions.Start;
                    }
                }

                public void Init()
                {
                    _schedule.Available.Get(_restrictions.Start.Left, _restrictions.Finish.Right);
                    var node = _schedule.Available.Find(_point);

                    ICalendarItem intersection;
                    TAmount amount;

                    IteratorInstructions instruction;
                    do
                    {
                        intersection = node.Value.Intersect(_startArea);
                        var duration = ToDuration(intersection.Left, intersection.Right);
                        instruction = RequestInstructions(duration, node.Value.Data, out amount);

                    } while (instruction != IteratorInstructions.Accept);
                }


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

                private IteratorInstructions RequestInstructions(ICalendarItem item, out TAmount amount)
                {
                    var left = item.Left;
                    var right = Point<TTime>.Min(item.Right, _restrictions.Start.Right);
                    var duration = ToDuration(left, right);
                    var data = item.Data;

                    return RequestInstructions(duration, data, out amount);
                }


                private IteratorInstructions RequestInstructions(TDuration duration, TData data, out TAmount amount)
                {
                    return _requestInstructions(duration, data, _restrictions.RequiredAmount, out amount);
                }

                private bool? Check(ICalendarItem calendarItem, TAmount requiredAmount)
                {
                    return null;
                }

                void Initialize()
                {
                }

                bool MeetTheConstraints()
                {
                    return _restrictions.Start.Contains(Start) && _restrictions.Finish.Contains(Finish) && _restrictions.TotalDuration.Contains(Point<TDuration>.Right(TotalDuration)) && _restrictions.Duration.Contains(Point<TDuration>.Right(Duration));
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

            public delegate IteratorInstructions RequestInstructionsDelegate(TDuration duration, TData data, TAmount requiredAmount, out TAmount amount);
        }
    }

    public enum SchedulerKind
    {
        LeftToRight, RightToLeft
    }
}