using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAS.Core
{
    public abstract class BaseAgent
    {
        private static readonly ComaptibilitiesAgent ComaptibilitiesAgent = new ComaptibilitiesAgent();


        public void Initialize()
        {
            RegisterBehaviors();
        }

        protected abstract void RegisterBehaviors();

        /// <summary>
        /// Потребности
        /// </summary>
        public virtual IReadOnlyCollection<BaseRequirement> Requirements => BaseRequirement.Empty;

        protected virtual ComaptibilitiesAgent GetComaptibilitiesAgent()
        {
            return ComaptibilitiesAgent;
        }

        void RequestAbilitiesCompatibilities(BaseAgent requirements)
        {
            GetComaptibilitiesAgent().RequestData<BaseRequirement[]>();
        }

        /// <summary>
        /// Проверка потенциальной возможности удовлетворить потребности другого агента
        /// </summary>
        /// <param name="requirementsAgent">агент-потребностей</param>
        /// <returns></returns>
        public virtual IAbilitiesCompatibilityInfo Compatible(BaseAgent requirementsAgent)
        {
            return null;

            //var comaptibilitiesAgent = GetComaptibilitiesAgent();
            //IAbilitiesCompatibilityInfo result;

            //comaptibilitiesAgent.RequestAbilitiesCompatibilities(this, )
            //comaptibilitiesAgent.TryGetAbilitiesCompatibilities(this, requirementsAgent, out result);

            ////IAbilitiesCompatibilityInfo result;
            ////return requirementsAgent.AbilitiesCompatibilities.TryGetValue(requirementsAgent, out result)
            ////    ? result
            ////    : new AgentsCompatibilityInfo(requirementsAgent, this, CompatibilityInfo.Empty);
        }


        //protected readonly ConcurrentDictionary<BaseAgent, IRequiremetsCompatibilityInfo> RequiremetsCompatibilities
        //    = new ConcurrentDictionary<BaseAgent, IRequiremetsCompatibilityInfo>();

        //protected readonly ConcurrentDictionary<BaseAgent, IAbilitiesCompatibilityInfo> AbilitiesCompatibilities
        //    = new ConcurrentDictionary<BaseAgent, IAbilitiesCompatibilityInfo>();

        protected internal void AddCompatibilityInfo(IAbilitiesCompatibilityInfo info)
        {
            //AbilitiesCompatibilities.TryAdd(info.Agent, info);
        }
        protected internal void AddCompatibilityInfo(IRequiremetsCompatibilityInfo info)
        {
            //RequiremetsCompatibilities.TryAdd(info.Agent, info);
        }
    }

    public class Request<T>
    {
        public Responce<T> Response(T data)
        {
            return new Responce<T> { Reqest = this, Data = data };
        }
    }

    public class Responce<T>
    {
        public Request<T> Reqest { get; set; }
        public T Data { get; set; }
    }


    public static class AgentExtender
    {
        private static T ResponceData<T>(Responce<T> response)
        {
            return response != null
                ? response.Data
                : default(T);
        }

        // === Sync ============================================

        public static T RequestData<T>(this BaseAgent agent)
        {
            return ResponceData(agent.SendAndWaitResponse<Request<T>, Responce<T>>(new Request<T>()));
        }

        public static TResponse SendAndWaitResponse<TRequest, TResponse>(this BaseAgent agent, TRequest request)
        {
            return default(TResponse);
        }

        // === Async ============================================

        public static async Task<T> RequestDataAsync<T>(this BaseAgent agent)
        {
            var response = await agent.SendAndWaitResponseAsync<Request<T>, Responce<T>>(new Request<T>());
            return ResponceData(response);
        }

        public static Task<TResponse> SendAndWaitResponseAsync<TRequest, TResponse>(this BaseAgent agent, TRequest request)
        {
            return Task.FromResult(default(TResponse));
        }
    }
}