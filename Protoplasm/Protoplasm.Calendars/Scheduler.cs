using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
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
                if (ArithmeticAdapter<TAmount>.Add != null)
                    AddAmount = ArithmeticAdapter<TAmount>.Add;
                else
                    ArithmeticAdapter<TAmount>.Add = (a, b) => AddAmount(a, b);

                if (ArithmeticAdapter<TAmount>.Subst != null)
                    SubstAmount = ArithmeticAdapter<TAmount>.Subst;
                else
                    ArithmeticAdapter<TAmount>.Subst = (a, b) => SubstAmount(a, b);
            }

            public static Func<TAmount, TAmount, TAmount> AddAmount;
            public static Func<TAmount, TAmount, TAmount> SubstAmount;

            private readonly ISchedule _schedule;

            private readonly RequestInstructionsDelegate _requestInstructions;
            private readonly RequestAmountDelegate _requestAmount;
            private readonly RequestDurationByAmountDelegate _requestDurationByAmount;
            private readonly RequestDataForAllocateDelegate _requestDataForAllocateDelegate;

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

            public Scheduler(ISchedule schedule, RequestInstructionsDelegate requestInstructions, RequestAmountDelegate requestAmount, RequestDurationByAmountDelegate requestDurationByAmount, RequestDataForAllocateDelegate requestDataForAllocateDelegate)
            {
                _schedule = schedule;
                _requestInstructions = requestInstructions;
                _requestAmount = requestAmount;
                _requestDurationByAmount = requestDurationByAmount;
                _requestDataForAllocateDelegate = requestDataForAllocateDelegate;
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
                    s = StartByFinish(start, finish, d);
                    f = FinishByStart(start, finish, d);
                    d = DurationByHorizonts(s, f, d);
                }
                else
                {
                    d = DurationByHorizonts(start, finish, d);

                    if (start.IsDefined)
                    {
                        f = FinishByStart(start, finish, d);
                        s = StartByFinish(start, finish, d);
                    }
                    else
                    {
                        s = StartByFinish(start, finish, d);
                        f = FinishByStart(start, finish, d);
                    }

                    d = DurationByHorizonts(s, f, d);
                }

                var restrictions = new Restrictions(s, f, totalDuration, d, amount);

                var iterator = new Iterator
                    (
                    _schedule,
                    restrictions,
                    kind,
                    _requestInstructions,
                    _requestAmount,
                    _requestDurationByAmount,
                    _requestDataForAllocateDelegate
                    );

                iterator.Init();

                var items = iterator.Process();

                ConsoleWriteLine($"<><><><>");

                foreach (var interval in items)
                {
                    var data = _requestDataForAllocateDelegate(interval, interval.Data.OriginalData, amount);
                    ConsoleWriteLine($"allocate: {interval}, data={data}");
                    _schedule.Include(interval.Left, interval.Right, data);
                }
                ConsoleWriteLine($"<><><><>");
            }

            private Interval<TDuration> PreProcessDuration(Interval<TDuration> duration, Interval<TDuration> totalDuration)
            {
                var left = Point<TDuration>.Left(_schedule.MinDuration).Max(duration.Left);
                var right = duration.Right.Min(totalDuration.Right);
                duration = new Interval<TDuration>(left, right);

                return duration;
            }

            private Interval<TTime> StartByFinish(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> duration)
            {
                var left = finish.Left.OffsetToLeft(duration.Max).Max(start.Left);
                var right = finish.Right.OffsetToLeft(duration.Min).Min(start.Right);

                return new Interval<TTime>(left, right);
            }

            private Interval<TTime> FinishByStart(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> duration)
            {
                var left = start.Left.OffsetToRight(duration.Min).Max(finish.Left);
                var right = start.Right.OffsetToRight(duration.Max).Min(finish.Right);
                return new Interval<TTime>(left, right);
            }

            private Interval<TDuration> DurationByHorizonts(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> duration)
            {
                var left = Point<TDuration>.Left(_schedule.MinDuration).Max(duration.Left);
                var right = duration.Right;

                if (!start.Left.IsUndefined && !finish.Right.IsUndefined)
                {
                    right = Point<TDuration>.Right(ToDuration(start.Left, finish.Right), start.Left.Included && finish.Right.Included);
                    right = Point<TDuration>.Min(right, duration.Right);
                }

                duration = new Interval<TDuration>(left, right);
                return duration;
            }
        }
    }
}