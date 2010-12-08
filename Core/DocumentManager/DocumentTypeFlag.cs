using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKManagers
{
    [Flags]
    internal enum DocumentTypeFlag
    {
        Empty = 0x00,
        GlossaryTerm = 0x01,
        CTGovProtocol = 0x02,
        Media = 0x04,
        Term = 0x08,
        DrugInformationSummary = 0x10,
        InScopeProtocol = 0x20,
        Summary = 0x40,
        Organization = 0x80,
        MiscellaneousDocument = 0x100,
        OutOfScopeProtocol = 0x200,
        PoliticalSubUnit = 0x400,
        GeneticsProfessional = 0x800,
        Protocol = 0x1000
    }
}
