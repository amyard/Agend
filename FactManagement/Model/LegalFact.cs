using System.ComponentModel;

namespace FactManagement.Models;

public class LegalFactGeneral: LegalFact
{
    public string FactId { get; set; } = Guid.NewGuid().ToString();
    public string DocumentName { get; set; } = string.Empty;
    public float[] FactStatementEmbedding { get; set; }

    // if user change something, change the type
    public BehaviourType BehaviourType { get; set; } = BehaviourType.SystemType;
}

public enum BehaviourType
{
    SystemType = 0,
    UserType = 1
}

// because of using json schema we need to add wrapper for response. the reason is because in schema we are calling object, not array. 
public class LegalFactWrapper
{
    public List<LegalFact> Facts { get; set; } = [];
}

public class LegalFact
{
    public string FactStatement { get; set; } = string.Empty;
    public string FactText { get; set; } = string.Empty;
    public string Dates { get; set; } = string.Empty;
    public string Page { get; set; }
    public List<string> InvolvedParties { get; set; } = [];
    public string LegalIssue { get; set; } = string.Empty;
    public int ImportanceLevel { get; set; }
}


