using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.Calendars;

namespace ConsoleApplication1.TestData
{
    public interface IAgentsManager
    {
        IHoldersCompatibilityInfo Compatible(IRequirementsHolder requirements, IAbilitiesHolder abilities);
        IHoldersCompatibilityInfo Find(IRequirementsHolder requirements, IAbilitiesHolder abilities);
        IHoldersCompatibilityInfo Add(IRequirementsHolder requirements, IAbilitiesHolder abilities, IHoldersCompatibilityInfo info);
    }

    public interface IManagedAgent : IAgent
    {
        IAgentsManager Manager { get; }
    }

    public abstract class AgentsManager : IAgentsManager
    {
        private readonly List<IHoldersCompatibilityInfo> _compatibilities = new List<IHoldersCompatibilityInfo>();

        public IHoldersCompatibilityInfo Compatible(IRequirementsHolder requirements, IAbilitiesHolder abilities)
        {
            return Find(requirements, abilities)
                ?? Generate(requirements, abilities);
        }

        public IHoldersCompatibilityInfo Find(IRequirementsHolder requirements, IAbilitiesHolder abilities)
        {
            return _compatibilities.FirstOrDefault(x => x.RequiremenetsHolder == requirements && x.AbilitiesHolder == abilities);
        }

        private IHoldersCompatibilityInfo Generate(IRequirementsHolder requirements, IAbilitiesHolder abilities)
        {
            lock (_compatibilities)
            {
                var info = Find(requirements, abilities);

                if (info != null)
                    return info;

                var requirementsAgentsManager = GetAgentsManager(requirements);
                var abilitiesAgentsManager = GetAgentsManager(abilities);

                // ���� ���-�� �� ������� ������ � IAgemtsManager, �� ������ ��� info-��� (��� ��� ��� ����, ��� ��������)
                // ���� ��� - �������  ����
                info = requirementsAgentsManager?.Add(requirements, abilities, null)
                        ?? abilitiesAgentsManager?.Add(requirements, abilities, null) 
                        ?? requirements.Compatibility(abilities);
                
                // ������ �� �����
                _compatibilities.Add(info);

                // ��������, ���� ����
                requirementsAgentsManager?.Add(requirements, abilities, info);
                abilitiesAgentsManager?.Add(requirements, abilities, info);

                return info;
            }
        }

        IAgentsManager GetAgentsManager(object candidate)
        {
            var manager = candidate as IAgentsManager ?? (candidate as IManagedAgent)?.Manager;
            if (manager == this)
                manager = null;

            return manager;
        }

        IHoldersCompatibilityInfo IAgentsManager.Add(IRequirementsHolder requirements, IAbilitiesHolder abilities, IHoldersCompatibilityInfo info)
        {
            lock (_compatibilities)
            {
                // ���� � ����
                var local = Find(requirements, abilities);

                // ���� � ��� � ���
                if (info != null && local != null)
                {

                    if(ReferenceEquals(info, local))
                        return info;
                    
                    // �������� ���
                    _compatibilities.Remove(local);
                    _compatibilities.Add(info);
                    return info;
                }

                // ��� ����, ��� ����
                if (local == null && info != null)
                {
                    // ��������� ����
                    _compatibilities.Add(info);
                    return info;
                }

                // ���� ��� (� ��� ����)
                if (local != null)
                    return local;

                // ��� �� ���, � ���
                local = requirements.Compatibility(abilities);
                _compatibilities.Add(local);
                return local;
            }
        }

    }

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