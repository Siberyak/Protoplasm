using System.Collections.Generic;
using MAS.Core;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class ResourcesManager : BaseAgent
        {
            private readonly PlanningEnvironment<TTime, TDuration> _environment;

            public ResourcesManager(PlanningEnvironment<TTime, TDuration> environment)
            {
                _environment = environment;
            }

            protected override void RegisterBehaviors()
            {
            
            }

            public Department CreateDepartment(string caption, params Department[] memberOf)
            {
                return new Department(caption, memberOf);
            }

            public EmployeeAgent CreateEmployeeAgent(string caption, Competences competences, Calendar<CalendarItemType> calendar, params MembershipItemsContainer[] memberOf)
            {
                var employee = new Employee(caption, competences, calendar, memberOf);
                var agent = new EmployeeAgent(employee);
                agent.Initialize();
                return agent;
            }
        } 
    }
}