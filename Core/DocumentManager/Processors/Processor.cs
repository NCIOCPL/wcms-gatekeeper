using System;
using System.Xml.Serialization;

using GateKeeper.DocumentObjects;

namespace GKManagers.Processors
{
    public class Processor
    {
        public DocumentType Type { get; set; }
        public ProcessingTarget[] ProcessingTargets { get; set; }
    }
}