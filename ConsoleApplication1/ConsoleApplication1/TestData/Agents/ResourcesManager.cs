using System;
using System.CodeDom.Compiler;
using MAS.Core;
using Protoplasm.Calendars;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class ResourcesManager : AgentsManager
        {
            private readonly PlanningEnvironment<TTime, TDuration> _environment;

            public ResourcesManager(PlanningEnvironment<TTime, TDuration> environment)
            {
                _environment = environment;
            }
            


            TAgent Initialize<TAgent>(TAgent agent)
                where TAgent : IManagedAgent
            {
                agent.Initialize();
                return agent;
            } 

            public DepartmentAgent CreateDepartment(string caption, params Department[] memberOf)
            {
                var department = new Department(caption, memberOf);
                var agent = new DepartmentAgent(this, department);
                return Initialize(agent);
            }

            public EmployeeAgent CreateEmployeeAgent(string caption, Competences competences, IAvailabilityCalendar calendar, params MembershipItemsContainer[] memberOf)
            {
                var employee = new Employee(caption, competences, calendar, memberOf);
                var agent = new EmployeeAgent(this, employee);
                return Initialize(agent);
            }
        } 
    }
}