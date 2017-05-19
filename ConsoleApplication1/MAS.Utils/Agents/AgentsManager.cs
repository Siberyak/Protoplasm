using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Utils
{
    public abstract class AgentsManager : IAgentsManager
    {
        private readonly List<IHoldersCompatibilityInfo> _compatibilities = new List<IHoldersCompatibilityInfo>();

        public IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities, IRequirementsHolder requirements)
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

        public IEnumerable<IHoldersCompatibilityInfo> this[IRequirementsHolder requirements]
        {
            get { return _compatibilities.Where(x => x.RequiremenetsHolder == requirements).ToArray(); }
        }
    }
}