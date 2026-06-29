using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parrot.Application.DTOs.Agents;

public class AgentInfo
{
    required public string Name { get; set; } 

    required public string Description { get; set; }

    required public string MascotUrl { get; set; }

    required public bool SupportsHumanTakeover { get; set; } = true;
}